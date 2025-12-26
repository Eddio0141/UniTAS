use super::{Test, TestArgs, TestCtx, TestType};
use anyhow::{Context, Result};
use std::fs;

const MOVIE: &str = include_str!("unity_2022_3_41f1_base_movie.lua");

pub const TEST: Test = Test {
    name: "2022.3.41f1-base",
    test,
};

fn test(ctx: &mut TestCtx, mut args: TestArgs) -> Result<()> {
    let movie_path = args.game_dir.join("movie.lua");
    fs::write(&movie_path, MOVIE).with_context(|| {
        format!(
            "failed to write movie file to path `{}`",
            movie_path.display()
        )
    })?;

    let stream = &mut args.stream;

    ctx.run_init_and_general_tests(stream)?;

    // frame advancing test

    // sanity check
    stream.send("service('ITimeWrapper').capture_frame_time = 0.01 service('ISceneManagerWrapper').load_scene('FrameAdvancing')")?;
    ctx.print_test_results(stream, TestType::General)?;
    ctx.reset_general_tests(stream)?;

    // actual test
    /*
    stream.send(
        r#"event_coroutine(function()
        local y = coroutine.yield

        local fa = service("IFrameAdvancing")
        fa.FrameAdvance(1)

        local frameAdvancing_YieldNull = traverse("FrameAdvancing").field("_yieldNull")

        service("ISceneManagerWrapper").load_scene("FrameAdvancing")
        y("UpdateUnconditional")

        for _ = 1, 250 do
            y("UpdateUnconditional")
        end

        print(frameAdvancing_YieldNull.GetValue())

        for _ = 1, 5 do
            fa.FrameAdvance(1)
            y("UpdateActual")
            print(frameAdvancing_YieldNull.GetValue())
            y("UpdateUnconditional")
            y("UpdateUnconditional")
            y("UpdateUnconditional")
            y("UpdateUnconditional")
            y("UpdateUnconditional")
        end

        fa.TogglePause() -- resume

        for _ = 1, 150 do
            y("UpdateActual")
        end
    end)"#,
    )?;

    // frame advancing checks
    ctx.assert_eq(
        &0.to_string(),
        &stream.receive()?,
        "Frame advancing: yield null check",
        "Mismatch in reach stage",
    );
    for i in 0..5u8 {
        ctx.assert_eq(
            &i.to_string(),
            &stream.receive()?,
            &format!("Frame advancing: yield null check {i}"),
            "Mismatch in reach stage",
        );
    }

    // final check
    ctx.get_assert_results(stream)?;
    ctx.reset_assert_results(stream)?;
    */

    let frame_count = 100u8;
    let time_offset_check_count = 10u8;

    // unitas updates

    // fixed update: 0.02
    // ---
    // update: 0.02
    // late update: 0.02 - not included here
    // end of frame: 0.02
    // last update: 0.02
    // ---
    // update: 0.03
    // late update: 0.03 - not included here
    // end of frame: 0.03
    // last update: 0.03

    stream.send(&format!(
        r#"time = traverse('UnityEngine.Time')
service('ITimeWrapper').capture_frame_time = 0.01

wait_for_fixed_update = true
wait_for_last = true
fixed_update_count = 0
update_count = 0
end_of_frame_count = 0
last_update_count = 0
update_offsets_start_time = -1
update_offsets = {{}}

printed_results = 0

local reverse_invoker = service("IPatchReverseInvoker")
local fixed_time = traverse("UnityEngine.Time").property("fixedTime")

patch("UniTAS.Patcher.Implementations.UnityEvents.UnityEvents.InvokeFixedUpdate", function(this)

    reverse_invoker.invoking = true
    local fixed_time = fixed_time.GetValue()
    reverse_invoker.invoking = false
    if traverse(this).field("_prevFixedTime").GetValue() == fixed_time then
        return
    end

    wait_for_fixed_update = false

    if wait_for_last then
        return
    end

    if update_count >= {frame_count} then
        if printed_results < 2 then
            print(fixed_update_count)
            print(update_count)
            print(end_of_frame_count)
            print(last_update_count)
            print(update_offsets_start_time)
            for _, v in pairs(update_offsets) do
                print(v)
            end
            printed_results = printed_results + 1

            -- test #2 init
            if printed_results == 1 then
                time.property('maximumDeltaTime').set_value(0.3333333)
                time.property('fixedDeltaTime').set_value(0.02)
                time.property('timeScale').set_value(1)
                service('ITimeWrapper').capture_frame_time = 0.04
                wait_for_fixed_update = true
                wait_for_last = true

                fixed_update_count = 0
                update_count = 0
                end_of_frame_count = 0
                last_update_count = 0
            end
        else
            return
        end
    end

    fixed_update_count = fixed_update_count + 1
end, "method")

local time = traverse("UnityEngine.Time").property("time")
local offset = service("IUpdateInvokeOffset")

patch("UniTAS.Patcher.Implementations.UnityEvents.UnityEvents.InvokeUpdate", function(this)
    if wait_for_last or wait_for_fixed_update or update_count >= {frame_count} or traverse(this).field("_updated").get_value() then
        return
    end

    update_count = update_count + 1
    
    if update_offsets_start_time == -1 then
        reverse_invoker.invoking = true
        update_offsets_start_time = time.GetValue()
        reverse_invoker.invoking = false
    end
    if #update_offsets < {time_offset_check_count} then
        table.insert(update_offsets, offset.Offset)
    end
end, "method")
patch("UniTAS.Patcher.Implementations.UnityEvents.UnityEvents.InvokeEndOfFrame", function(this)
    if wait_for_last or wait_for_fixed_update or update_count >= {frame_count} or traverse(this).field("_endOfFrameUpdated").get_value() then
        return
    end

    end_of_frame_count = end_of_frame_count + 1
end, "method")
patch("UniTAS.Patcher.Implementations.UnityEvents.UnityEvents.InvokeLastUpdate", function(this)
    if wait_for_fixed_update or update_count >= {frame_count} then
        return
    end

    if traverse(this).field("_calledLastUpdate").GetValue() then
        print("last update was called twice in a single update, this is absolutely invalid")
    end

    last_update_count = last_update_count + 1
    wait_for_last = false
end, "method")
"#
    ))?;

    // ignore messages
    for _ in 0..8 {
        stream.receive()?;
    }

    // fixed_update_count
    // update_count
    // end_of_frame_count
    // last_update_count
    ctx.assert_eq(
        &(frame_count / 2).to_string(),
        &stream.receive()?,
        "unitas updates: fixed update count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &frame_count.to_string(),
        &stream.receive()?,
        "unitas updates: update count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &(frame_count - 1).to_string(),
        &stream.receive()?,
        "unitas updates: end of frame count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &frame_count.to_string(),
        &stream.receive()?,
        "unitas updates: last update count",
        "mismatch in update count",
    );
    let time_offset = stream.receive()?;
    let mut time_offset = time_offset
        .parse::<f64>()
        .with_context(|| format!("time offset is an invalid f64 value, got: {time_offset}"))?;
    // since we're testing by hooking onto the update methods themselves, the offset is literally off by 1 frame in this case
    time_offset += 0.01;
    time_offset %= 0.02;
    for _ in 0..time_offset_check_count {
        let offset = stream.receive()?;
        let offset = offset
            .parse::<f64>()
            .with_context(|| format!("update offset is an invalid f64 value, got: {offset}"))?;

        ctx.assert_eq_precision(
            time_offset,
            offset,
            "offset check",
            "failed to validate tracked update offset",
        );

        time_offset += 0.01;
        if time_offset >= 0.02 {
            time_offset -= 0.02;
        }
    }

    // multiple fixed updates in a row
    // in this case, this pattern is made

    // ...
    // f: 0.02
    // f: 0.04
    // u: 0.04
    // f: 0.06
    // f: 0.08
    // u: 0.08
    // f: 0.10
    // f: 0.12
    // u: 0.12

    // fixed_update_count
    // update_count
    // end_of_frame_count
    // last_update_count
    ctx.assert_eq(
        &(frame_count * 2 + 1).to_string(),
        &stream.receive()?,
        "unitas updates: fixed update count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &frame_count.to_string(),
        &stream.receive()?,
        "unitas updates: update count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &(frame_count - 1).to_string(),
        &stream.receive()?,
        "unitas updates: end of frame count",
        "mismatch in update count",
    );
    ctx.assert_eq(
        &frame_count.to_string(),
        &stream.receive()?,
        "unitas updates: last update count",
        "mismatch in update count",
    );
    for _ in 0..2 {
        stream.receive()?;
    }

    Ok(())
}
