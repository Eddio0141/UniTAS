#!/bin/bash

# Get args for building debug or release
if [ -z "$1" ]; then
    echo "No build type specified, defaulting to debug"
    BUILD_TYPE="Debug"
else
    BUILD_TYPE=$1
fi

OUTPUT_DIR="build/$BUILD_TYPE"
OUTPUT_PLUGIN_DIR="$OUTPUT_DIR/plugins/UniTAS"
OUTPUT_PATCH_DIR="$OUTPUT_DIR/patchers/UniTAS"

# Clean output directory
rm -rf "$OUTPUT_DIR"

DOTNET_SOURCE="UniTAS"

SOURCE_PLUGIN_DIR="$DOTNET_SOURCE/Plugin"
SOURCE_PLUGIN_EXTERNS_DIR="$SOURCE_PLUGIN_DIR/Extern-Assemblies"
SOURCE_PATCH_DIR="$DOTNET_SOURCE/Patcher"

# Dotnet builds
dotnet build "$SOURCE_PLUGIN_DIR" -c $BUILD_TYPE
dotnet build "$SOURCE_PATCH_DIR" -c $BUILD_TYPE

echo "Copying dlls to output folders"

# Create output directories
mkdir -p "$OUTPUT_PLUGIN_DIR"
mkdir -p "$OUTPUT_PATCH_DIR"

# Only copy dlls
cp "$SOURCE_PLUGIN_DIR/bin/$BUILD_TYPE/net35"/*.dll "$OUTPUT_PLUGIN_DIR"
cp "$SOURCE_PLUGIN_EXTERNS_DIR"/*.dll "$OUTPUT_PLUGIN_DIR"
cp "$SOURCE_PATCH_DIR/bin/$BUILD_TYPE/net35"/*.dll "$OUTPUT_PATCH_DIR"

# Build and copy set up tool
cd unitas_setup_tool
if [ "$BUILD_TYPE" = "Debug" ]; then
    cargo build
    cp target/debug/unitas_setup_tool ../$OUTPUT_DIR
else
    cargo build --release
    cp target/release/unitas_setup_tool ../$OUTPUT_DIR
fi

cd ..