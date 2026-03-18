use std::{ffi::CStr, mem, sync::LazyLock};

use libc::*;
use log::trace;

use crate::{hook::hooks, utils};

struct OrigFuncs {
    mono_add_internal_call: extern "C" fn(*const c_char, *const c_void),
}

static ORIG_FUNCS: LazyLock<OrigFuncs> = LazyLock::new(|| OrigFuncs {
    mono_add_internal_call: unsafe {
        mem::transmute::<*const (), extern "C" fn(*const i8, *const libc::c_void)>(
            utils::unix::orig_sym_addr(c"mono_add_internal_call")
                .expect("failed to find mono_add_internal_call"),
        )
    },
});

pub extern "C" fn mono_add_internal_call(name: *const c_char, method: *const c_void) {
    if name.is_null() {
        return (ORIG_FUNCS.mono_add_internal_call)(name, method);
    }

    let name_str = unsafe { CStr::from_ptr(name) };
    trace!(
        "mono_add_internal_call, name: {}, method: {:x}",
        name_str.to_string_lossy(),
        method.addr()
    );

    if name_str == c"UnityEngine.Time::get_realtimeSinceStartup" {
        hooks::unity::hook_get_real_time_since_startup(method as usize);
    }

    (ORIG_FUNCS.mono_add_internal_call)(name, method)
}
