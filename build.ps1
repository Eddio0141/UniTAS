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
    dotnet build "$project" -c "$buildType"
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
Copy-Item "Patcher/bin/$buildType/net35/*.dll" "$buildOutputPatcher" -Force