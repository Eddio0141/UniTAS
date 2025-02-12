type Callback = extern "C" fn(i32) -> i32;

static mut CALLBACK: Option<Callback> = None;

#[no_mangle]
pub extern "C" fn set_callback(cb: Callback) {
    unsafe {
        CALLBACK = Some(cb);
    }
}

#[no_mangle]
pub extern "C" fn hello_world() {
    let value = unsafe { CALLBACK.unwrap()(123) };
    println!("hello world, {value}");
}

