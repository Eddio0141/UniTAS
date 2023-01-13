# Get args for building debug or release
param(
    [string]$buildType = "Debug"
)

$buildOutput = "build/$buildType"

# Dotnet builds
$buildOutputPlugin = "$buildOutput/plugins"

dotnet build "Plugin" -c "$buildType"

if (!(Test-Path "$buildOutput")) {
    New-Item -ItemType Directory -Path "$buildOutput" > $null
    New-Item -ItemType Directory -Path "$buildOutputPlugin" > $null
}

# Copy plugin dlls
Copy-Item "Plugin/bin/$buildType/net35/*.dll" "$buildOutputPlugin" -Force

# Copy external plugin dlls
Copy-Item "Plugin/Extern-Assemblies/*.dll" "$buildOutputPlugin" -Force