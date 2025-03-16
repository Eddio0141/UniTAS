/// CPU frequency
///
/// # Note
/// 2^40 is chosen as when converted to khz for QueryPerformanceFrequency,
/// it still fits under 32 bits integer (value is under 2^31) in case of storing the result in an i32 for whatever reason
///
/// Setting it too high may break some calculations
#[cfg(windows)]
pub const CPU_CLOCK_SPEED: u64 = 2u64.pow(40);
