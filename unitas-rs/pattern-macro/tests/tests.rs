use pattern::Pattern;
use pattern_macro::pattern;

#[test]
fn valid_pattern() {
    let actual = pattern!(4, "00 ?? 1A b0 Cc 9D 5e ?? fF");
    let expected = Pattern::new(
        4,
        &[
            Some(0x00),
            None,
            Some(0x1A),
            Some(0xB0),
            Some(0xCC),
            Some(0x9D),
            Some(0x5E),
            None,
            Some(0xFF),
        ],
    );

    assert_eq!(actual, expected);
}

#[test]
fn valid_pattern_negative_offset() {
    let actual = pattern!(-4, "00 ?? 1A b0 Cc 9D 5e ?? fF");
    let expected = Pattern::new(
        -4,
        &[
            Some(0x00),
            None,
            Some(0x1A),
            Some(0xB0),
            Some(0xCC),
            Some(0x9D),
            Some(0x5E),
            None,
            Some(0xFF),
        ],
    );

    assert_eq!(actual, expected);
}

#[test]
fn test_compile_fails() {
    let tests = trybuild::TestCases::new();
    tests.compile_fail("tests/compile_fails/*.rs");
}
