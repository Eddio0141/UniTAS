use std::{env, ffi::CStr};

pub mod player_loop;

const PLAYER_MAIN_SYMBOL: &CStr = c"_Z10PlayerMainiPPc";

const UNITY_PLAYER_MODULE: &CStr =
    const_str::cstr!(const_str::format!("UnityPlayer{}", env::consts::DLL_SUFFIX));
