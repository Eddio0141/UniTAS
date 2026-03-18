use std::mem;

use iced_x86::{Code, Decoder, DecoderOptions, Instruction, Mnemonic};
use retour::static_detour;

use crate::{detour_setup_log_fail, memory::SEARCH, state::os::TIME};

pub mod player_loop;

static_detour! {
    static realtimeSinceStartup_detour: unsafe extern "C" fn() -> f64;
}

type RealtimeSinceStartup = unsafe extern "C" fn() -> f64;

/// Hooks `UnityEngine.Time::get_realtimeSinceStartup`
/// This hook will try hook in the base function that is called as part of this function
pub fn hook_get_real_time_since_startup(method: usize) {
    let start = SEARCH.search().set_start(method);

    let ret = start
        .find_instruction(|inst| inst.mnemonic() == Mnemonic::Ret)
        .expect("couldn't find ret instruction");

    let mut call = start.set_end(ret.start());
    // always skip an instruction when searching
    let mut skip = true;
    while let Some(res) = call.find_instruction(|inst| {
        if skip {
            skip = false;
            return false;
        }
        inst.mnemonic() == Mnemonic::Call
    }) {
        call = res;
        skip = true;
    }
    let mut decoder = Decoder::with_ip(
        usize::BITS as u32,
        call.memory(),
        call.start() as u64,
        DecoderOptions::NONE,
    );
    assert!(
        decoder.can_decode(),
        "expected call instruction but cannot decode"
    );
    let mut call = Instruction::default();
    decoder.decode_out(&mut call);
    assert_eq!(call.mnemonic(), Mnemonic::Call);
    assert!(call.is_call_near(), "didn't expect a far branch...");

    let addr: RealtimeSinceStartup = unsafe { mem::transmute(call.near_branch_target()) };

    detour_setup_log_fail!(
        realtimeSinceStartup_detour,
        addr,
        RealtimeSinceStartup,
        || { TIME.lock().unwrap().time_since_startup().as_secs_f64() }
    );
}
