# Get args for building debug or release
param(
    [string]$buildType = "Debug"
)

$buildOutput = "build/$buildType"

# Clear build output
if (Test-Path "$buildOutput") {
    Remove-Item -Path "$buildOutput" -Recurse -Force
}

# Dotnet builds
$dotnetSource = "UniTAS"
$patcherSource = "$dotnetSource/Patcher"
$runtimeResources = "$patcherSource"

# Build output paths
$buildOutputPatcher = "$buildOutput/patchers/UniTAS"

dotnet build "$patcherSource" -c "$buildType"

if (!(Test-Path "$buildOutput")) {
    New-Item -ItemType Directory -Path "$buildOutput" > $null
}

if (!(Test-Path "$buildOutputPatcher"))
{
    New-Item -ItemType Directory -Path "$buildOutputPatcher" > $null
}

# Get full build output path
$buildOutput = (Resolve-Path $buildOutput).Path

# Copy patcher dlls
Copy-Item "$patcherSource/bin/$buildType/net35/*.dll" "$buildOutputPatcher" -Force

# Copy external dlls
Copy-Item "$runtimeResources/Extern-Assemblies/*.dll" "$buildOutputPatcher" -Force

# Copy resources folder
Copy-Item "$runtimeResources/Resources" "$buildOutputPatcher" -Recurse -Force