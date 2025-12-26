alias b := build
alias t := test
alias c := clean

unitas_rs_file := if os_family() == "windows" { "unitas_rs.dll" } else { "libunitas_rs.so" }

default:
  just --list

build target="release":
    cd unitas-rs && cargo build {{ if target == "debug" { "" } else { "--release" } }}
    dotnet build UniTAS -c {{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}
    if [ -z ${CARGO_BUILD_TARGET+x} ]; then source=""; else source="$CARGO_BUILD_TARGET/"; fi && cp unitas-rs/target/"$source"{{ if target == "debug" { "debug" } else { "release" } }}/{{ unitas_rs_file }} UniTAS/Patcher/bin/{{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}

test target="release":
    cd unitas-rs && cargo test {{ if target == "debug" { "" } else { "--release" } }}
    dotnet test UniTAS -c {{ if target == "release" { "ReleaseTest" } else if target == "debug" { "DebugTest" } else { target } }}
    cd test-runner && cargo build --release
    ./test-games.nu

clean:
    dotnet clean UniTAS
    cd unitas-rs && cargo clean
    cd test-runner && cargo clean
