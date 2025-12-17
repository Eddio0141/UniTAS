#!/usr/bin/env nu

use std

const repo_dir = path self | path dirname
let cache_dir = $env.HOME | path join ".cache" "unitas-test-runner"
let bepinex_dir = $cache_dir | path join "BepInEx"

mkdir $cache_dir
 
if ($bepinex_dir | path type) != "dir" {
    print "downloading bepinex"
    # TODO: not crossplatform friendly
    let url = curl -s "https://api.github.com/repos/BepInEx/BepInEx/releases/latest"
    let url = try {
        $url | from json | get assets | where ($it.name | str contains "BepInEx_linux_x64") | get 0.browser_download_url
    } catch { |e| 
        print $"failed to download bepinex\n($e.rendered)\n\nAPI return:\n($url)"
        exit 1
    }
    print $"url: `($url)`"
    let bepinex_zip = mktemp
    curl -Lso $bepinex_zip $url
    print "setting up bepinex"
    unzip $bepinex_zip -d $bepinex_dir o+e> (std null-device)
    rm -f $bepinex_zip
    # TODO: not crossplatform friendly
    let run_bepinex = $bepinex_dir | path join "run_bepinex.sh"
    let run_script_contents = open $run_bepinex | lines | each {|v| if ($v | str contains "executable_name=\"\"") { "executable_name=\"build.x86_64\"" } else { $v }} | to text
    rm $run_bepinex
    $run_script_contents | save $run_bepinex
    # TODO: not crossplatform friendly
    chmod u+x $run_bepinex
    let config_dir = $bepinex_dir | path join "BepInEx" "config"
    mkdir $config_dir
    "[Remote]\nEnable = true" | save ($config_dir | path join "UniTAS.cfg")
    print "done"
}

let games_dir = mktemp -d
let unitas_build_dir = $repo_dir | path join "UniTAS" "Patcher" "bin" "Release"

for dir in (ls -la $unitas_build_dir) {
    cp -r ($dir | get name) $bepinex_dir
}

let logs_dir = $cache_dir | path join "logs"
rm -rf $logs_dir
print $"logs directory is `($logs_dir)`"
mkdir $logs_dir

print ""

mut exit_code = 0
for game_dir in (ls ($repo_dir | path join "TestGames")) {
    if ($game_dir | get type) != "dir" {
        continue
    }

    let game_name = $game_dir | get name | path basename
    let build_dir = $game_dir | get name | path join "build"

    if ($build_dir | path type) != "dir" {
        print $"game `($game_name)` doesn't have a build directory, skipping"
        continue
    }

    let game_dir = $games_dir | path join $game_name
    # TODO: not crossplatform friendly
    let executable = $game_dir | path join "run_bepinex.sh"

    # setup cached game file
    mkdir $game_dir
    for dir in ((ls -la $build_dir) | append (ls -la $bepinex_dir)) {
        cp -r ($dir | get name) $game_dir
    } 
    # TODO: not crossplatform friendly
    chmod u+x ($game_dir | path join "build.x86_64")

    print $"\n[($game_name)]"
    # TODO: not crossplatform friendly
    $exit_code = try {
        ^$"($repo_dir)/test-runner/target/release/test-runner" $executable
        0
    } catch { |e| 
        let exit_code = $e | get -o exit_code
        if $exit_code == null {
            print $e.rendered
            exit 1
        }
        $exit_code
    }

    let logs_dir = $logs_dir | path join $game_name
    mkdir $logs_dir
    try { mv ($game_dir | path join "stdout.log") $logs_dir }
    try { mv ($game_dir | path join "stderr.log") $logs_dir }
    try { mv ($game_dir | path join "game-stdout.log") $logs_dir }
    try { mv ($game_dir | path join "BepInEx" "unitas-rs.log") $logs_dir }
    try { mv ($game_dir | path join "BepInEx" "UniTAS.log") $logs_dir }

    if $exit_code != 0 {
        print "\nNot running next test game(s) due to failure"
        break
    }
}

# cleanup
rm -rf $games_dir

exit $exit_code
