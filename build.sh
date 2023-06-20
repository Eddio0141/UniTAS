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
SOURCE_PATCH_EXTERN_DLL_DIR="$SOURCE_PATCH_DIR/Extern-Assemblies"
SOURCE_RESOURCES_DIR="SOURCE_PATCH_DIR/Resources"

# Dotnet build
dotnet build "$SOURCE_PATCH_DIR" -c "$BUILD_TYPE"

echo "Copying dlls to output folders"

# Create output dirs
mkdir -p "$OUTPUT_PATCH_DIR"

cp "$SOURCE_PATCH_EXTERN_DLL_DIR"/* "$OUTPUT_PATCH_DIR"
cp -r "$SOURCE_RESOURCES_DIR" "$OUTPUT_PATCH_DIR"
cp "$SOURCE_PATCH_DIR/bin/$BUILD_TYPE/net35"/*.dll "$OUTPUT_PATCH_DIR"
