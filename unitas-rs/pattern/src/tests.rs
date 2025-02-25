use crate::Pattern;

#[test]
fn match_exact() {
    let bytes = &[0x00, 0x01, 0x02, 0x03, 0x04];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), Some(1));
}

#[test]
fn match_exact_offset() {
    let bytes = &[0x00, 0x01, 0x02, 0x03, 0x04];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: 1,
    };

    assert_eq!(pattern.matches(bytes), Some(2));
}

#[test]
fn match_exact_offset2() {
    let bytes = &[0x00, 0x01, 0x02, 0x03, 0x04];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: 3,
    };

    assert_eq!(pattern.matches(bytes), Some(4));
}

#[test]
fn match_exact_offset3() {
    let bytes = &[0x00, 0x01, 0x02, 0x03, 0x04];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: isize::MAX,
    };

    assert_eq!(pattern.matches(bytes), Some(isize::MAX as usize + 1));
}

#[test]
fn match_exact_neg_offset() {
    let bytes = &[0x00, 0x01, 0x02, 0x03, 0x04];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: -1,
    };

    assert_eq!(pattern.matches(bytes), Some(0));
}

#[test]
fn match_start() {
    let bytes = &[0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(0), Some(1), Some(2)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), Some(0));
}

#[test]
fn match_end() {
    let bytes = &[0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(1), Some(2), Some(3)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), Some(1));
}

#[test]
fn match_retry() {
    let bytes = &[0x00, 0x02, 0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(0), Some(1), Some(2)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), Some(2));
}

#[test]
fn match_wildcard() {
    let bytes = &[0x00, 0x02, 0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(0), None, Some(2)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), Some(2));
}

#[test]
fn not_found_head() {
    let bytes = &[0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(4)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), None);
}

#[test]
fn not_match() {
    let bytes = &[0x00, 0x01, 0x00, 0x02, 0x00, 0x03];
    let pattern = Pattern {
        pattern: &[Some(0), Some(4)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), None);
}

#[test]
fn not_enough_data() {
    let bytes = &[0x00];
    let pattern = Pattern {
        pattern: &[Some(0), Some(1)],
        offset: 0,
    };

    assert_eq!(pattern.matches(bytes), None);
}

#[test]
fn offset_underflow() {
    let bytes = &[0x00, 0x01, 0x02, 0x03];
    let pattern = Pattern {
        pattern: &[Some(0), Some(1), Some(2)],
        offset: -1,
    };

    assert_eq!(pattern.matches(bytes), None);
}
