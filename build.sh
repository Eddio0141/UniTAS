#!/bin/bash

# Get args for building debug or release
if [ -z "$1" ]; then
    echo "No build type specified, defaulting to debug"
    BUILD_TYPE="Debug"
else
    BUILD_TYPE=$1
fi

OUTPUT_DIR="build/$BUILD_TYPE"
OUTPUT_PLUGIN_DIR="$OUTPUT_DIR/plugins"
SOURCE_PLUGIN_EXTERNS_DIR="Plugin/Extern-Assemblies"

# Dotnet builds
dotnet build "Plugin" -c $BUILD_TYPE

echo "Copying dlls to output folders"

# Create output directories
mkdir -p "$OUTPUT_PLUGIN_DIR"

# Only copy dlls
cp "Plugin/bin/$BUILD_TYPE/net35"/*.dll "$OUTPUT_PLUGIN_DIR"
cp "$SOURCE_PLUGIN_EXTERNS_DIR"/*.dll "$OUTPUT_PLUGIN_DIR"

# Build set up tool
cd unitas_setup_tool
if [ "$BUILD_TYPE" = "Debug" ]; then
    cargo build
else
    cargo build --release
fi

# Copy setup tool to output directory
cp target/release/unitas_setup_tool ../$OUTPUT_DIR
cd ..