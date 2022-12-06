# Get args for building debug or release
$buildType = $args[0]

# Dotnet build projects
$dotnetProjects = @(
    "Plugin",
    "Patcher"
)

# Build each project
foreach ($project in $dotnetProjects) {
    dotnet msbuild $project -p:Configuration=$buildType
}

# Create output folder
$buildOutput = "build/$buildType"

if (!(Test-Path $buildOutput)) {
    New-Item -ItemType Directory -Path $buildOutput
}

# Copy all dll files in build folder
$copyDirs = @(
    "Plugin/bin/$buildType/net35",
    "Patcher/bin/$buildType"
)

foreach ($dir in $copyDirs) {
    Copy-Item "$dir/*.dll" $buildOutput
}