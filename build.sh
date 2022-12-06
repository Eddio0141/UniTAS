#!/bin/bash

# Get args for building debug or release
if [ -z "$1" ]; then
    echo "No build type specified, defaulting to debug"
    BUILD_TYPE="debug"
else
    BUILD_TYPE=$1
fi

# Dotnet build projects paths
PROJECTS=(
    "Plugin",
    "Patcher"
)

# Build projects
for PROJECT in "${PROJECTS[@]}"
do
    echo "Building $PROJECT"
    dotnet msbuild $PROJECT -p:Configuration=$BUILD_TYPE
done

echo "Copying dlls to output folders"

OUTPUT_DIR="build/$BUILD_TYPE"
OUTPUT_PLUGIN_DIR="$OUTPUT_DIR/Plugin"
OUTPUT_PATCHER_DIR="$OUTPUT_DIR/Patcher"

# Create output directories
mkdir -p $OUTPUT_PLUGIN_DIR
mkdir -p $OUTPUT_PATCHER_DIR

# Only copy dlls
cp "Plugin/bin/$BUILD_TYPE/net35/*.dll" $OUTPUT_DIR_PLUGIN
cp "Patcher/bin/$BUILD_TYPE/*.dll" $OUTPUT_DIR_PATCHER