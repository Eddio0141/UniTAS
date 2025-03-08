use std::{env, ffi::CStr};

pub mod player_loop;

const UNITY_PLAYER_MODULE: &CStr =
    const_str::cstr!(const_str::format!("UnityPlayer{}", env::consts::DLL_SUFFIX));
