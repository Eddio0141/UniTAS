# Get args for building debug or release
param(
    [string]$buildType = "Debug"
)

# Dotnet build projects
$dotnetProjects = @(
    "Plugin",
    "Patcher"
)

# Build each project
foreach ($project in $dotnetProjects) {
    # If patcher and $buildType is ReleaseTrace, build with Release
    if ($project -eq "Patcher" -and $buildType -eq "ReleaseTrace") {
        dotnet build "$project" -c "Release"
    }
    else {
        dotnet build "$project" -c "$buildType"
    }
}

# Create output folders
$buildOutput = "build/$buildType"
$buildOutputPlugin = "$buildOutput/Plugin"
$buildOutputPatcher = "$buildOutput/Patcher"

if (!(Test-Path $buildOutput)) {
    New-Item -ItemType Directory -Path $buildOutput > $null
    # Folder for plugin
    New-Item -ItemType Directory -Path "$buildOutputPlugin" > $null
    # Folder for patcher
    New-Item -ItemType Directory -Path "$buildOutputPatcher" > $null
}

# Copy plugin dlls
Copy-Item "Plugin/bin/$buildType/net35/*.dll" "$buildOutputPlugin" -Force

# Copy patcher dlls
if ($buildType -eq "ReleaseTrace") {
    Copy-Item "Patcher/bin/Release/net35/*.dll" "$buildOutputPatcher" -Force
} else {
    Copy-Item "Patcher/bin/$buildType/net35/*.dll" "$buildOutputPatcher" -Force
}

# Copy external plugin dlls
Copy-Item "Plugin/Extern-Assemblies/*.dll" "$buildOutputPlugin" -Force