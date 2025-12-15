alias b := build
alias t := test
alias c := clean

unitas_rs_file := if os_family() == "windows" { "unitas_rs.dll" } else { "libunitas_rs.so" }

default:
  just --list

build target="release": (_dotnet-target "build" target) (_cargo-target "build" target)
    cp unitas-rs/target/{{ if target == "debug" { "debug" } else { "release" } }}/{{ unitas_rs_file }} UniTAS/Patcher/bin/{{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}

test target="release": (_dotnet-target "test" target) (_cargo-target "test" target)

clean:
    dotnet clean UniTAS
    cd unitas-rs && cargo clean

_dotnet-target args target:
    dotnet {{args}} UniTAS -c {{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}

_cargo-target args target:
    cd unitas-rs && cargo {{args}} {{ if target == "debug" { "" } else { "--release" } }}
