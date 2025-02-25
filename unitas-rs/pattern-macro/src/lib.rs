use proc_macro::*;

#[proc_macro]
/// Creates a new pattern.
///
/// # Example
/// ```
/// # use pattern::Pattern;
/// # use pattern_macro::pattern;
/// let pattern = pattern!(4, "00 ?? 1D FF");
/// let actual = Pattern::new(4, &[Some(0x00), None, Some(0x1D), Some(0xFF)]);
///
/// assert_eq!(pattern, actual);
/// ```
///
/// # Arguments
/// - The first argument is the offset as an isize.
/// - The second argument is the pattern as a string literal.
///
/// ## Pattern
/// - `??` is a wildcard, which matches any byte.
/// - Otherwise it must be a two character hex byte.
pub fn pattern(input: TokenStream) -> TokenStream {
    let mut input = input.into_iter();

    // offset
    // take until comma
    let mut builder = "pattern::Pattern::new(".to_string();

    let mut found_comma = false;
    for input in input.by_ref() {
        if let TokenTree::Punct(token) = &input {
            if token.as_char() == ',' {
                found_comma = true;
                break;
            }
        }

        builder.push_str(&input.to_string());
    }

    if !found_comma {
        return r#"compile_error!("expected a comma")"#.to_string().parse().unwrap();
    }

    builder.push_str(",&[");

    // pattern
    const EXPECTED_STRING_ERROR: &str = "expected a pattern as a string literal";

    let Some(TokenTree::Literal(token)) = input.next() else {
        return format!(r#"compile_error!("{EXPECTED_STRING_ERROR}")"#)
            .parse()
            .unwrap();
    };

    let token_string = token.to_string();
    if token_string.len() < 2 {
        return error(token.span(), EXPECTED_STRING_ERROR);
    }

    if !token_string.starts_with('"') || !token_string.ends_with('"') {
        return error(token.span(), EXPECTED_STRING_ERROR);
    }

    let token_string = &token_string[1..token_string.len() - 1];
    let pattern = token_string.split_whitespace().collect::<Vec<_>>();

    if pattern.is_empty() {
        return error(token.span(), "what's the point of an empty pattern?");
    }

    const EXPECTED_PATTERN_ERROR: &str = "expected `??` or a two character hex byte";
    let mut first = true;
    let mut was_wildcard = false;

    for pattern_str in pattern {
        if pattern_str == "??" {
            if first {
                // starting with wildcard is not allowed
                return error(
                    token.span(),
                    "there's no point in starting with wildcards, remove them and fix the offset",
                );
            }
            first = false;
            was_wildcard = true;

            builder.push_str("None,");
            continue;
        }

        first = false;
        was_wildcard = false;

        if pattern_str.len() != 2 {
            return error(token.span(), EXPECTED_PATTERN_ERROR);
        }

        // don't bother with parsing to usize
        const HEX_CHARS: &[char] = &[
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'A',
            'B', 'C', 'D', 'E', 'F',
        ];

        for c in pattern_str.chars() {
            if !HEX_CHARS.contains(&c) {
                return error(token.span(), EXPECTED_PATTERN_ERROR);
            }
        }

        builder.push_str(&format!("Some(0x{pattern_str}),"));
    }

    if was_wildcard {
        return error(
            token.span(),
            "there's no point in ending with wildcards, remove them",
        );
    }

    builder.push_str("])");

    builder.parse().unwrap()
}

fn error(span: Span, msg: &str) -> TokenStream {
    format!(r#"compile_error!("{msg}")"#)
        .parse::<TokenStream>()
        .unwrap()
        .into_iter()
        .map(|mut token| {
            token.set_span(span);
            token
        })
        .collect()
}
