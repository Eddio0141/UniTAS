use std::{
    borrow::Cow,
    ffi::{CStr, OsStr},
    fs, mem,
    os::unix::ffi::OsStrExt,
};

use goblin::elf::Elf;
use libc::*;

use crate::hook::hooks::ReverseInvoke;

unsafe extern "C" fn dl_iter(info: *mut dl_phdr_info, _: size_t, find: *mut c_void) -> c_int {
    let find: *mut Find = unsafe { mem::transmute(find) };
    let find = unsafe { &mut *find };
    let info = unsafe { &*info };

    if unsafe { fnmatch(find.name.as_ptr(), info.dlpi_name, 0) } != 0 {
        return 0;
    }

    let name = unsafe { CStr::from_ptr(info.dlpi_name).to_owned() };
    find.name = Cow::Owned(name);
    find.addr = info.dlpi_addr;

    1
}

struct Find<'a> {
    name: Cow<'a, CStr>,
    addr: u64,
}

/// Searches for the address of the symbol by parsing the library and searching in there.
pub fn orig_sym_addr_manual(lib: &CStr, symbol: &str) -> Option<usize> {
    // inspired by libTAS
    let mut find = Find {
        name: Cow::Borrowed(lib),
        addr: 0,
    };

    unsafe {
        if dl_iterate_phdr(Some(dl_iter), &mut find as *mut Find as *mut c_void) != 1 {
            return None;
        }
    }

    let file = find.name;
    // unix only way of converting from CString to OsStr: https://stackoverflow.com/questions/46342644/how-can-i-get-a-path-from-a-raw-c-string-cstr-or-const-u8
    let file = OsStr::from_bytes(file.to_bytes());
    let file = fs::read(file).expect("failed to read library");
    let file = Elf::parse(&file).expect("failed to parse library");

    for sym in file.dynsyms.iter() {
        let name = &file.dynstrtab[sym.st_name];
        if name == symbol {
            return Some((find.addr + sym.st_value) as usize);
        }
    }

    None
}

/// Searches for the address of the symbol
pub fn orig_sym_addr(symbol: &CStr) -> Option<*const ()> {
    let ri = ReverseInvoke::new();

    let addr = unsafe { dlsym(RTLD_NEXT, symbol.as_ptr()) };

    drop(ri);

    if addr.is_null() {
        None
    } else {
        Some(addr as *const ())
    }
}
