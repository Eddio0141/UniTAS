#[cfg(test)]
mod tests;

#[derive(Debug, PartialEq, Eq)]
pub struct Pattern {
    pattern: &'static [Option<u8>],
    offset: isize,
}

impl Pattern {
    pub const fn new(offset: isize, pattern: &'static [Option<u8>]) -> Self {
        if pattern.is_empty() {
            panic!("pattern must not be empty");
        }

        if pattern[0].is_none() {
            panic!("pattern must not start with None");
        }

        if pattern[pattern.len() - 1].is_none() {
            panic!("pattern must not end with None");
        }

        Self { pattern, offset }
    }

    /// Tries to match the pattern against the data.
    /// - If the pattern matches, returns `Some(offset)`.
    /// - If the pattern doesn't match, returns `None`.
    /// - The offset would be the offset of the first byte of the pattern.
    pub fn matches(&self, mut data: &[u8]) -> Option<usize> {
        // we don't let pattern to be constructed by the user, so head is always Some
        let head = self.pattern[0].unwrap();
        let mut data_offset = 0;

        'data_loop: loop {
            // advance data slice to head
            data = match data.iter().position(|&f| f == head) {
                Some(index) => {
                    data_offset += index;
                    &data[index..]
                }
                None => return None,
            };

            // not enough bytes?
            if data.len() < self.pattern.len() {
                return None;
            }

            // check if pattern matches
            for (i, pattern) in self.pattern[1..].iter().enumerate() {
                if let Some(pattern) = pattern {
                    if data[i + 1] != *pattern {
                        data_offset += 1;
                        data = &data[1..];
                        continue 'data_loop;
                    }
                }
            }

            let offset = data_offset.checked_add_signed(self.offset)?;

            return Some(offset);
        }
    }
}
