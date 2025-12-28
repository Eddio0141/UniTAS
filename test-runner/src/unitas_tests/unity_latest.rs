use super::*;
use crate::movies;

pub const TEST: Test = Test {
    name: "unity_latest",
    test,
};

fn test(ctx: &mut TestCtx, mut args: TestArgs) -> Result<()> {
    let stream = &mut args.stream;

    ctx.run_init_and_general_tests(stream)?;
    ctx.run_movie_test(
        stream,
        movies::OLD_INPUT_SYSTEM__2022_3__6000_0_44F1,
        "old_input_system__2022_3__6000_0_44f1",
        args.game_dir,
    )?;

    Ok(())
}
