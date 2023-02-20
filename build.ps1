# Get args for building debug or release
param(
    [string]$buildType = "Debug"
)

$buildOutput = "build/$buildType"

# Dotnet builds
$dotnetSource = "UniTAS"
$pluginSource = "$dotnetSource/Plugin"
$buildOutputPlugin = "$buildOutput/plugins/UniTAS"

dotnet build "$pluginSource" -c "$buildType"

if (!(Test-Path "$buildOutput")) {
    New-Item -ItemType Directory -Path "$buildOutput" > $null
}

if (!(Test-Path "$buildOutputPlugin")) {
    New-Item -ItemType Directory -Path "$buildOutputPlugin" > $null
}

# Get full build output path
$buildOutput = (Resolve-Path $buildOutput).Path

# Copy plugin dlls
Copy-Item "$pluginSource/bin/$buildType/net35/*.dll" "$buildOutputPlugin" -Force

# Copy external plugin dlls
Copy-Item "$pluginSource/Extern-Assemblies/*.dll" "$buildOutputPlugin" -Force

# Build and copy set up tool
Push-Location -Path "unitas_setup_tool"
if ($buildType -eq "Debug") {
    cargo build
    Copy-Item "target/debug/unitas_setup_tool.exe" "$buildOutput" -Force
} else {
    cargo build --release
    Copy-Item "target/release/unitas_setup_tool.exe" "$buildOutput" -Force
}

Pop-Location