use std::{
    env,
    ffi::{CStr, CString},
    ops::Range,
    slice,
};

use log::{debug, info, warn};
use pattern::Pattern;

use crate::{
    hook::HOOKS,
    memory::{MemoryMap, SymbolLookup},
};

pub struct Search {
    pattern: Option<Pattern>,
    start_symbol: Option<&'static CStr>,
    /// Module to search for
    /// - `None` will make it search in the executable itself
    module: Option<&'static CStr>,
    /// Function callback for when this search matches
    installer: &'static dyn Fn(usize),
}

impl Search {
    pub const fn new(installer: &'static dyn Fn(usize)) -> Search {
        Search {
            pattern: None,
            start_symbol: None,
            module: None,
            installer,
        }
    }

    pub const fn with_pattern(mut self, pattern: Pattern) -> Search {
        self.pattern = Some(pattern);
        self
    }

    pub const fn with_module(mut self, module: &'static CStr) -> Search {
        self.module = Some(module);
        self
    }

    pub const fn with_symbol(mut self, symbol: &'static CStr) -> Search {
        self.start_symbol = Some(symbol);
        self
    }
}

pub struct Hook<'a>(pub &'a [Search]);

pub fn install() {
    let modules = MemoryMap::read_proc_maps().expect("couldn't read memory map");
    let mut symbol_lookup = SymbolLookup::new();
    info!("installing {} hooks", HOOKS.len());
    'hooks: for hook in HOOKS {
        debug!("hook: attempting {} hook searches", hook.0.len());
        for search in hook.0 {
            debug!(
                "hook: targeting module {}",
                search
                    .module
                    .map(|m| m.to_string_lossy())
                    .unwrap_or("<executable>".into())
            );
            if search.start_symbol.is_none() && search.pattern.is_none() {
                warn!("hook: start symbol and search pattern is `None`, sure this is intended?");
            }
            let mut get_mem_range = |base: &Range<usize>, file| match search.start_symbol {
                Some(start_symbol) => {
                    debug!("hook: targeting symbol {}", start_symbol.to_string_lossy());
                    match symbol_lookup.get_symbol_in_file(file, start_symbol) {
                        Some(off) => Some((off, base.end)),
                        None => {
                            debug!("failed to find symbol");
                            None
                        }
                    }
                }
                None => Some((base.start, base.end)),
            };
            let (base, mem_start, mem_end) = match search.module {
                Some(m) => {
                    let Some(base) = modules.find_by_filename(m) else {
                        continue;
                    };
                    let Some((mem_start, mem_end)) = get_mem_range(&base, m) else {
                        continue;
                    };
                    (base, mem_start, mem_end)
                }
                None => {
                    let exe_path =
                        env::current_exe().expect("failed to get current executable path");
                    let base = modules.find_exe();
                    let exe_path_cstr = CString::new(exe_path.to_string_lossy().as_ref()).unwrap();
                    let Some((mem_start, mem_end)) = get_mem_range(&base, &exe_path_cstr) else {
                        continue;
                    };
                    (base, mem_start, mem_end)
                }
            };
            let offset = match search.pattern.as_ref() {
                Some(pattern) => {
                    let memory = unsafe {
                        slice::from_raw_parts(mem_start as *const u8, mem_end - mem_start)
                    };

                    debug!("hook: search addr range: 0x{mem_start:x}..0x{mem_end:x}");

                    let Some(offset) = pattern.matches(memory) else {
                        debug!("search: failed to match pattern!");
                        continue;
                    };
                    let offset = offset + mem_start;

                    debug!(
                        "search: found pattern in 0x{offset:x} (0x{:x})",
                        offset - base.start
                    );

                    offset
                }
                None => mem_start,
            };

            (search.installer)(offset);

            continue 'hooks;
        }
        panic!("failed to install hook, no patterns matches");
    }
}
