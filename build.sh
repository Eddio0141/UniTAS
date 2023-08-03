#!/bin/bash

# Get args for building debug or release
if [ -z "$1" ]; then
    echo "No build type specified, defaulting to debug"
    BUILD_TYPE="Debug"
else
    BUILD_TYPE=$1
fi

OUTPUT_DIR="build/$BUILD_TYPE"
OUTPUT_PATCH_DIR="$OUTPUT_DIR/patchers/UniTAS"

# Clean output directory
rm -rf "$OUTPUT_DIR"

DOTNET_SOURCE="UniTAS"

SOURCE_PATCH_DIR="$DOTNET_SOURCE/Patcher"
RUNTIME_RESOURCES="$SOURCE_PATCH_DIR/RuntimeResources"

# Dotnet build
dotnet build "$SOURCE_PATCH_DIR" -c "$BUILD_TYPE"

echo "Copying dlls to output folders"

# Create output dirs
mkdir -p "$OUTPUT_PATCH_DIR"

cp "$RUNTIME_RESOURCES/Extern-Assemblies"/* "$OUTPUT_PATCH_DIR"
cp -r "$RUNTIME_RESOURCES/Resources" "$OUTPUT_PATCH_DIR"
cp "$SOURCE_PATCH_DIR/bin/$BUILD_TYPE/net35"/*.dll "$OUTPUT_PATCH_DIR"
