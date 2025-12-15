alias b := build

default:
  just --list

build target="release": (_dotnet-profile "build" target) (_cargo-profile "build" target)

test target="release": (_dotnet-profile "test" target) (_cargo-profile "test" target)

clean:
    dotnet clean UniTAS
    cd unitas-rs && cargo clean

_dotnet-profile args profile:
    dotnet {{args}} UniTAS -c {{ if profile == "release" { "Release" } else if profile == "debug" { "Debug" } else { profile } }}

_cargo-profile args profile:
    cd unitas-rs && cargo {{args}} {{ if profile == "debug" { "" } else { "--release" } }}
