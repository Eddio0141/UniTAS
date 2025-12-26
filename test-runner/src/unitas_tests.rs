use crate::{cli::Args, symbols};
use anyhow::{Context, Result, bail};
use colored::Colorize;
use log::{debug, trace};
#[cfg(target_family = "unix")]
use std::os::unix::process::ExitStatusExt;
use std::{
    collections::VecDeque,
    fmt::Debug,
    fs::{self, File},
    io::{self, Read, Write},
    net::{IpAddr, Ipv4Addr, SocketAddr, TcpStream},
    path::Path,
    process::{Command, Stdio},
    thread,
    time::Duration,
};
use thiserror::Error;

mod unity_2022_3_41f1_base;
mod unity_latest;

pub const LINUX_TESTS: &[Test] = &[unity_latest::TEST];
pub const WIN_TESTS: &[Test] = &[unity_2022_3_41f1_base::TEST, unity_latest::TEST];

pub struct Test {
    pub name: &'static str,
    test: fn(ctx: &mut TestCtx, args: TestArgs) -> Result<()>,
}

struct TestCtx {
    results: Vec<TestResult>,
}

impl TestCtx {
    fn assert(&mut self, condition: bool, name: &str, message: &str) {
        if condition {
            println!("{} {name}", symbols::SUCCESS.green());
        } else {
            println!("{} {name}", symbols::FAIL.red());
        }
        let result = if condition {
            TestResult::Success
        } else {
            TestResult::Fail(TestFailInfo {
                name: name.to_string(),
                message: format!("assertion failed: {message}"),
            })
        };
        self.results.push(result);
    }

    fn assert_eq<T: PartialEq + Debug>(&mut self, left: T, right: T, name: &str, message: &str) {
        let result = left == right;
        self.assert(
            result,
            name,
            &format!(
                "`left == right`: {message}\n  left: `{:?}`\n right: `{:?}`",
                left, right
            ),
        );
    }

    fn assert_eq_precision(&mut self, left: f64, right: f64, name: &str, message: &str) {
        let result = f64::abs(right - left) < 0.00001;
        self.assert(
            result,
            name,
            &format!(
                "`left == right`: {message}\n  left: `{:?}`\n right: `{:?}`",
                left, right
            ),
        );
    }

    fn run_init_and_general_tests(&mut self, stream: &mut UniTasStream) -> Result<()> {
        self.print_test_results(stream, TestType::Init)?;
        self.run_general_tests_iter(stream)?;

        stream.send(
            "service('IGameRestart').SoftRestart(traverse('DateTime').property('Now').GetValue())",
        )?;

        let mut setup_fail = true;
        for _ in 0..30 {
            stream.send("print(service('IGameRestart').Restarting)")?;
            if stream.receive()? == "false" {
                setup_fail = false;
                break;
            }
            thread::sleep(Duration::from_secs(1));
        }

        if setup_fail {
            panic!("failed to soft restart");
        }

        self.print_test_results(stream, TestType::Init)?;
        self.run_general_tests_iter(stream)?;

        thread::sleep(Duration::from_secs(1));

        Ok(())
    }

    // single iteration version
    fn run_general_tests_iter(&mut self, stream: &mut UniTasStream) -> Result<()> {
        stream.send("traverse('TestFrameworkRuntime').method('RunGeneralTests').GetValue()")?;

        let mut timeout = true;
        for _ in 0..60 {
            stream.send(
                "print(traverse('TestFrameworkRuntime').field('_generalTestsDone').GetValue())",
            )?;
            if stream.receive()? == "true" {
                timeout = false;
                break;
            }
            thread::sleep(Duration::from_secs(1));
        }

        if timeout {
            panic!("failed to finish running general tests");
        }

        self.print_test_results(stream, TestType::General)?;
        self.reset_general_tests(stream)?;

        Ok(())
    }

    fn reset_general_tests(&self, stream: &mut UniTasStream) -> Result<()> {
        stream.send("traverse('TestFrameworkRuntime').method('ResetGeneralTests').GetValue()")
    }

    fn print_test_results(&mut self, stream: &mut UniTasStream, test_type: TestType) -> Result<()> {
        println!("---");
        let res_field_name = test_type.results_field_name();

        stream
            .send(&format!("print(traverse('TestFrameworkRuntime').field('_instance').field('{res_field_name}').property('Count').GetValue())"))?;
        let count = stream
            .receive()?
            .parse::<usize>()
            .expect("count of test results should be a number");

        stream.send(&format!(
            "local results = traverse('TestFrameworkRuntime').field('_instance').field('{res_field_name}').GetValue() \
            for _, res in ipairs(results) do print(res.Name) print(res.Success) print(res.Message) end",
        )
        )?;

        for _ in 0..count {
            let name = stream.receive()?;
            let success = stream.receive()? == "true";
            let message = stream.receive()?;

            if success {
                println!("{} {name}", symbols::SUCCESS.green());
            } else {
                println!("{} {name}", symbols::FAIL.red());
            }
            let result = if success {
                TestResult::Success
            } else {
                TestResult::Fail(TestFailInfo { name, message })
            };
            self.results.push(result);
        }

        Ok(())
    }

    // TODO: movie test always run no matter what, which shouldn't happen!
    fn run_movie_test(
        &mut self,
        stream: &mut UniTasStream,
        movie: &str,
        name: &str,
        game_dir: &Path,
    ) -> Result<()> {
        let dest = game_dir.join(format!("{name}.lua"));
        fs::write(&dest, movie)
            .with_context(|| format!("failed to write movie file to `{}`", dest.display()))?;

        // OnPreGameRestart event resets static fields, so an event after that is registered
        stream.send(&format!(
            r#"
            local function on_restart(_, pre_scene_load)
                if pre_scene_load then
                    return
                end
                hook_on_game_restart(on_restart, false)

                traverse("TestFrameworkRuntime").field("_movieTestClassToRun").SetValue("{name}")
            end

            hook_on_game_restart(on_restart, true)
            play("{}")
            "#,
            dest.to_string_lossy()
        ))?;

        // wait till movie ends
        let mut fail = true;
        for _ in 0..60 {
            thread::sleep(Duration::from_secs(1));
            stream.send("print(movie_status().basically_running)")?;
            if stream.receive()? == "false" {
                fail = false;
                break;
            }
        }

        if fail {
            panic!("movie failed to stop running");
        }

        self.print_test_results(stream, TestType::Movie)?;

        Ok(())
    }
}

enum TestType {
    General,
    Movie,
    Init,
}

impl TestType {
    fn results_field_name(&self) -> &str {
        match self {
            TestType::General => "_generalTestResults",
            TestType::Movie => "_movieTestResults",
            TestType::Init => "_initTestResults",
        }
    }
}

enum TestResult {
    Success,
    Fail(TestFailInfo),
}

struct TestFailInfo {
    name: String,
    message: String,
}

struct TestArgs<'a> {
    game_dir: &'a Path,
    stream: UniTasStream,
}

struct UniTasStream {
    stream: TcpStream,
    buf: Vec<u8>,
    buf_msg_len: [u8; 8], // u32 (int) length
    received_queue: VecDeque<String>,
    ready_to_send: bool,
}

#[repr(u8)]
enum ReceivePrefix {
    Prefix = 0,
    Stdout = 1,
}

impl From<u8> for ReceivePrefix {
    fn from(value: u8) -> Self {
        match value {
            0 => ReceivePrefix::Prefix,
            1 => ReceivePrefix::Stdout,
            _ => unimplemented!(
                "invalid ReceivePrefix value `{value}`, forgot to implement new prefix on rust side?"
            ),
        }
    }
}

const HUMAN_PREFIX: &str = ">> ";

const ERR_PREFIX_READ_FAIL: &str = "failed to receive UniTAS remote prefix";

impl UniTasStream {
    fn new(mut stream: TcpStream) -> Result<Self, io::Error> {
        // add timeout to the stream
        let timeout = Some(Duration::from_secs(1));
        stream.set_read_timeout(timeout).unwrap();
        stream.set_write_timeout(timeout).unwrap();

        let mut buf = vec![0; HUMAN_PREFIX.len()];

        // initialise connection
        stream.read_exact(&mut buf)?;
        assert_eq!(
            String::from_utf8_lossy(&buf),
            HUMAN_PREFIX,
            "somehow there's a mismatch in expected initial message"
        );
        // verify we are a script
        buf.resize(1, 0);
        buf[0] = 0;
        stream.write_all(&buf)?;

        Ok(Self {
            stream,
            buf,
            buf_msg_len: [0; 8],
            ready_to_send: true,
            received_queue: VecDeque::new(),
        })
    }

    fn send(&mut self, content: &str) -> Result<()> {
        trace!("send to remote call with content `{content}`");
        if !self.ready_to_send {
            debug!("can't send message to remote yet, `ready_to_send` is false");

            // wait for prefix
            let mut no_response = true;
            for _ in 0..30 {
                self.buf.resize(size_of::<ReceivePrefix>(), 0);
                self.stream
                    .set_read_timeout(Some(Duration::from_secs(30)))
                    .expect("failed to set timeout, somehow");
                self.stream.read_exact(&mut self.buf).with_context(|| {
                    format!("{ERR_PREFIX_READ_FAIL}, can't send data to remote")
                })?;

                match ReceivePrefix::from(self.buf[0]) {
                    ReceivePrefix::Prefix => {
                        debug!("ready to send to remote");
                        no_response = false;
                        break;
                    }
                    ReceivePrefix::Stdout => {
                        let Some(msg) = self.read_stdout() else {
                            thread::sleep(Duration::from_secs(1));
                            continue;
                        };
                        let msg = msg.context("Failed to read message from UniTAS")?;

                        debug!("got stdout message: `{msg}`, adding to queue");
                        self.received_queue.push_back(msg);
                        no_response = false;
                        break;
                    }
                }
            }

            if no_response {
                bail!("UniTAS is not responding");
            }
        } else {
            self.ready_to_send = false;
        }

        let content_len_raw = content.len().to_le_bytes();
        let content = content.as_bytes();

        let content = [&content_len_raw, content].concat();

        self.stream
            .write_all(&content)
            .context("failed to send message to UniTAS remote")?;

        trace!("sent msg to remote, msg len: {}", content.len());

        Ok(())
    }

    fn receive(&mut self) -> Result<String> {
        trace!("receive call");

        if let Some(msg) = self.received_queue.pop_front() {
            trace!("found message in queue already, `{msg}`");
            return Ok(msg);
        }

        for _ in 0..30 {
            self.buf.resize(size_of::<ReceivePrefix>(), 0);
            self.stream
                .set_read_timeout(Some(Duration::from_secs(30)))
                .expect("failed to set timeout, somehow");
            self.stream.read_exact(&mut self.buf).with_context(|| {
                format!("{ERR_PREFIX_READ_FAIL}, failed to receive data from remote")
            })?;

            match ReceivePrefix::from(self.buf[0]) {
                ReceivePrefix::Prefix => {
                    self.ready_to_send = true;
                    debug!("received prefix data from remote, ready to send");
                    continue;
                }
                ReceivePrefix::Stdout => {
                    let Some(msg) = self.read_stdout() else {
                        thread::sleep(Duration::from_secs(1));
                        continue;
                    };
                    return msg.context("failed to read message from UniTAS");
                }
            }
        }

        bail!("UniTAS is not responding");
    }

    fn read_stdout(&mut self) -> Option<io::Result<String>> {
        // use after reading message type prefix
        self.stream
            .set_read_timeout(Some(Duration::from_secs(1)))
            .expect("failed to set timeout somehow");
        if let Err(err) = self.stream.read_exact(&mut self.buf_msg_len) {
            if matches!(err.kind(), io::ErrorKind::TimedOut) {
                return None;
            }
            return Some(Err(err));
        }

        let msg_len = u64::from_le_bytes(self.buf_msg_len) as usize;

        self.buf.resize(msg_len, 0);
        if let Err(err) = self.stream.read_exact(&mut self.buf) {
            if matches!(err.kind(), io::ErrorKind::TimedOut) {
                return None;
            }
            return Some(Err(err));
        }

        let msg = String::from_utf8_lossy(&self.buf).trim_end().to_owned();

        debug!("received stdout msg: `{msg}`, len: `{msg_len}`");

        Some(Ok(msg))
    }
}

impl Test {
    pub fn run(&self, args: &Args) -> Result<(), BatchTestError> {
        println!("initialising test");

        let game_dir = args
            .path
            .parent()
            .expect("failed to get parent of executable, which is unexpected");
        let stdout =
            File::create(game_dir.join("stdout.log")).expect("failed to create `stdout.log`");
        let stderr =
            File::create(game_dir.join("stderr.log")).expect("failed to create `stderr.log`");

        // execute game
        println!("executing unity game");
        let mut process = Command::new(&args.path)
            .current_dir(game_dir)
            .arg("-batchmode")
            .arg("-nographics")
            .args(["-logFile", "game-stdout.log"])
            .stdout(stdout)
            .stderr(stderr)
            .stdin(Stdio::null())
            .spawn()
            .with_context(|| {
                format!(
                    "failed to run unity game, attempted to run `{}`",
                    args.path.display()
                )
            })?;

        let addr = SocketAddr::new(IpAddr::V4(Ipv4Addr::LOCALHOST), args.port);

        // now connect
        let mut stream = None;
        let fail_secs = 30usize;
        println!("connecting to UniTAS remote...");
        for i in 0..fail_secs {
            match TcpStream::connect_timeout(&addr, Duration::from_secs(30)) {
                Ok(s) => {
                    stream = Some(s);
                    break;
                }
                Err(err) => {
                    // last error?
                    if i == fail_secs - 1 {
                        process.kill().context("failed to stop running game")?;

                        return Err(anyhow::Error::new(err)
                            .context(format!(
                                "failed to connect to UniTAS after {fail_secs} seconds"
                            ))
                            .into());
                    }

                    // wait and try again
                    thread::sleep(Duration::from_secs(1));
                }
            }
        }

        let mut stream = UniTasStream::new(stream.unwrap()).context("failed to initialise connection to UniTAS, verifying connection as a script has failed")?;

        println!("connected\n");

        // get full access of lua api, before moving into test_args
        stream.send("full_access(true)")?;
        stream.receive()?;

        let test_args = TestArgs { game_dir, stream };
        let mut test_ctx = TestCtx {
            results: Vec::new(),
        };

        // run tests
        let result = (self.test)(&mut test_ctx, test_args);

        println!();
        process.kill().context("failed to stop running game")?;

        let status = process.wait().unwrap();

        result?;
        println!("test completed\n\n");

        let success_count = test_ctx
            .results
            .iter()
            .filter(|r| matches!(r, TestResult::Success))
            .count();
        let fails = test_ctx.results.iter().filter_map(|r| match r {
            TestResult::Success => None,
            TestResult::Fail(info) => Some(info),
        });

        let mut fail_count = 0usize;
        for fail in fails.clone() {
            println!("failed test `{}`", fail.name);
            println!("{}\n", fail.message);
            fail_count += 1;
        }

        if fail_count > 0 {
            println!("\nfailures:");

            for fail in fails {
                println!("    {}", fail.name);
            }
        }

        let success = if fail_count == 0 {
            "SUCCESS".green()
        } else {
            "FAILED".red()
        };
        println!("\ntest result: {success}. {success_count} passed; {fail_count} failed\n\n");

        if fail_count > 0 {
            let signal: Option<i32>;

            #[cfg(target_family = "unix")]
            {
                signal = status.signal();
            };
            #[cfg(not(target_family = "unix"))]
            {
                signal = None;
            };

            // check if not sigkill (process.kill() would terminate it with sigkill)
            let err = if status.success() || signal == Some(9) {
                BatchTestError::TestFail
            } else {
                BatchTestError::GameCrash {
                    code: status.code(),
                    signal,
                }
            };

            Err(err)
        } else {
            Ok(())
        }
    }
}

#[derive(Error, Debug)]
pub enum BatchTestError {
    #[error("all test didn't complete successfully")]
    TestFail,
    #[error("game has crashed, exit code: {}", code.map(|c| c.to_string()).unwrap_or_else(|| format!("None, signal: {}", signal.map(|s| s.to_string()).unwrap_or_else(|| "None".to_string()))))]
    GameCrash {
        code: Option<i32>,
        signal: Option<i32>,
    },
    #[error(transparent)]
    Other(#[from] anyhow::Error),
}
