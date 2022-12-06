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

echo "Copying dlls to output folder"

OUTPUT_DIR="build/$BUILD_TYPE"

if [ ! -d "$OUTPUT_DIR" ]; then
    mkdir -p $OUTPUT_DIR
fi

COPY_DIRS=(
    "Plugin/bin/$BUILD_TYPE/net35",
    "Patcher/bin/$BUILD_TYPE"
)

# Only copy dlls
for DIR in "${COPY_DIRS[@]}"
do
    cp $DIR/*.dll $OUTPUT_DIR
done