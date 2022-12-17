#!/bin/bash

# Get args for building debug or release
if [ -z "$1" ]; then
    echo "No build type specified, defaulting to debug"
    BUILD_TYPE="Debug"
else
    BUILD_TYPE=$1
fi

# Dotnet build projects paths
PROJECTS=(
    "Plugin"
    "Patcher"
)

# Build projects
for PROJECT in "${PROJECTS[@]}"; do
    echo "Building $PROJECT"
    dotnet build "$PROJECT" -c $BUILD_TYPE
done

echo "Copying dlls to output folders"

OUTPUT_DIR="build/$BUILD_TYPE"
OUTPUT_PLUGIN_DIR="$OUTPUT_DIR/Plugin"
OUTPUT_PATCHER_DIR="$OUTPUT_DIR/Patcher"

SOURCE_PLUGIN_DIR="Plugin/bin/$BUILD_TYPE/net35"
SOURCE_PATCHER_DIR="Patcher/bin/$BUILD_TYPE/net35"

SOURCE_PLUGIN_EXTERNS_DIR="Plugin/Extern-Assemblies"

# Create output directories
mkdir -p "$OUTPUT_PLUGIN_DIR"
mkdir -p "$OUTPUT_PATCHER_DIR"

# Only copy dlls
cp "$SOURCE_PLUGIN_DIR"/*.dll "$OUTPUT_PLUGIN_DIR"
cp "$SOURCE_PATCHER_DIR"/*.dll "$OUTPUT_PATCHER_DIR"
cp "$SOURCE_PLUGIN_EXTERNS_DIR"/*.dll "$OUTPUT_PLUGIN_DIR"
