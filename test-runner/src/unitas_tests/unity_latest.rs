use super::*;
use crate::movies;

pub const TEST: Test = Test {
    name: "unity_latest",
    test,
};

fn test(ctx: &mut TestCtx, args: &TestArgs) -> Result<()> {
    ctx.run_init_and_general_tests(args)?;
    ctx.run_movie_test(
        movies::OLD_INPUT_SYSTEM__2022_3__6000_0_44F1,
        "old_input_system__2022_3__6000_0_44f1",
        args,
    )?;
    ctx.run_movie_test(
        movies::NEW_INPUT_SYSTEM__2022_3__6000_0_44F1,
        "new_input_system__2022_3__6000_0_44f1",
        args,
    )?;

    Ok(())
}
