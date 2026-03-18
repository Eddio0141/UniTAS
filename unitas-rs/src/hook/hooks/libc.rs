use std::{ffi::CStr, mem, sync::LazyLock};

use libc::*;
use log::trace;

use crate::{
    hook::hooks::{self, REVERSE_INVOKE, ReverseInvoke},
    reverse_invoke, utils,
};

pub mod time;

struct OrigFuncs {
    dlopen: extern "C" fn(*const c_char, c_int) -> *mut c_void,
    dlsym: extern "C" fn(*mut c_void, *const c_char) -> *mut c_void,
}

static ORIG_FUNCS: LazyLock<OrigFuncs> = LazyLock::new(|| {
    let libs = [c"*libdl.so*", c"*libc.so*", c"*libc.*.so*"];

    let mut dlopen = 0;
    let mut dlsym = 0;

    for lib in libs {
        if dlopen == 0
            && let Some(addr) = utils::unix::orig_sym_addr_manual(lib, "dlopen")
        {
            dlopen = addr;
        }
        if dlsym == 0
            && let Some(addr) = utils::unix::orig_sym_addr_manual(lib, "dlsym")
        {
            dlsym = addr;
        }
        if dlopen != 0 && dlsym != 0 {
            break;
        }
    }

    if dlopen == 0 {
        panic!("dlopen couldn't be found");
    }
    if dlsym == 0 {
        panic!("dlsym couldn't be found");
    }

    OrigFuncs {
        dlopen: unsafe {
            mem::transmute::<usize, extern "C" fn(*const i8, i32) -> *mut libc::c_void>(dlopen)
        },
        dlsym: unsafe {
            mem::transmute::<usize, extern "C" fn(*mut libc::c_void, *const i8) -> *mut libc::c_void>(
                dlsym,
            )
        },
    }
});

#[unsafe(no_mangle)]
pub extern "C" fn dlopen(file: *const c_char, method: c_int) -> *mut c_void {
    // `file` can be a null pointer!
    if file.is_null() || REVERSE_INVOKE.get() {
        return (ORIG_FUNCS.dlopen)(file, method);
    }

    // use reverse invoker to prevent dlopen-ception, any extra dlopen will force original to be called
    let ri = ReverseInvoke::new();

    let file_cstr = unsafe { CStr::from_ptr(file) };
    trace!(
        "dlopen, file: {}, method: {method}",
        file_cstr.to_string_lossy()
    );

    let res = (ORIG_FUNCS.dlopen)(file, method);
    trace!("dlopen, result: {:X}", res.addr());

    drop(ri);

    res
}

#[unsafe(no_mangle)]
pub extern "C" fn dlsym(handle: *mut c_void, symbol: *const c_char) -> *mut c_void {
    // `symbol` can be a null pointer!
    if symbol.is_null() || REVERSE_INVOKE.get() {
        return (ORIG_FUNCS.dlsym)(handle, symbol);
    }

    // use reverse invoker to prevent dlopen-ception, any extra dlopen will force original to be called
    reverse_invoke!({
        let symbol_cstr = unsafe { CStr::from_ptr(symbol) };
        trace!("dlsym, symbol: {}", symbol_cstr.to_string_lossy());

        // TODO: consider handle flags
        match symbol_cstr.to_bytes() {
            b"mono_add_internal_call" => return hooks::mono::mono_add_internal_call as *mut c_void,
            b"dlopen" => return dlopen as *mut c_void,
            b"dlsym" => return dlsym as *mut c_void,
            _ => (),
        }

        let res = (ORIG_FUNCS.dlsym)(handle, symbol);
        trace!("dlsym, result: {:X}", res.addr());

        res
    })
}
