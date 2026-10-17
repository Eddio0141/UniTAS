alias b := build
alias t := test
alias c := clean

unitas_rs_file := if os_family() == "windows" { "unitas_rs.dll" } else { "libunitas_rs.so" }

default:
  just --list

[group("build")]
[arg("target", long="target")]
build target="release":
    cd unitas-rs && cargo build {{ if target == "debug" { "" } else { "--release" } }}
    dotnet build UniTAS -c {{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}
    if [ -z ${CARGO_BUILD_TARGET+x} ]; then source=""; else source="$CARGO_BUILD_TARGET/"; fi && cp unitas-rs/target/"$source"{{ if target == "debug" { "debug" } else { "release" } }}/{{ unitas_rs_file }} UniTAS/Patcher/bin/{{ if target == "release" { "Release" } else if target == "debug" { "Debug" } else { target } }}/BepInEx/patchers/UniTAS

[group("test")]
[arg("target", long="target")]
test-unit target="release": (build target)
    cd unitas-rs && cargo test {{ if target == "debug" { "" } else { "--release" } }}
    dotnet test UniTAS -c {{ if target == "release" { "ReleaseTest" } else if target == "debug" { "DebugTest" } else { target } }}

[group("test")]
[arg("target", long="target")]
test-games target="release" *TESTS: (build target)
    cd test-runner && cargo build --release
    ./test-games.nu {{TESTS}} --target {{ if target == "release" { "Release" } else { target } }}

[group("test")]
[arg("target", long="target")]
test target="release": (test-unit target) (test-games target)

clean:
    dotnet clean UniTAS
    cd unitas-rs && cargo clean
    cd test-runner && cargo clean

[group("packaging")]
[script]
package:
    cd packaging/thunderstore
    if [ ! -d "BepInEx" ]; then
        echo "Place the BepInEx directory in $(pwd)"
        exit 1
    fi

    zip -r ../unitas-thunderstore.zip *

[group("utils")]
detect-game-unity-version path:
    @cat "{{path}}/ProjectSettings/ProjectVersion.txt" | cut -d " " -f 2

[group("build")]
[script]
build-test-game name editors buildTarget:
    set -x
    game=$(readlink -f "TestGames/{{name}}")
    version=$(just detect-game-unity-version "$game")
    echo "Detected version $version"
    editor="{{editors}}/${version}/Editor/Unity"
    if [ ! -f "$editor" ]; then
        echo "Couldn't find an editor at '${editor}'"
        exit 1
    fi

    mkdir -p logs

    "$editor" -batchmode -nographics -quit -logFile logs/build-test-game.log -projectPath /home/yuu/src/UniTAS/TestGames/2017.4.6f1 -executeMethod Editor.UniTASTest.BuildScript.Build -buildTarget {{buildTarget}}
