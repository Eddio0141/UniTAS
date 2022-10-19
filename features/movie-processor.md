# Movie processor functionality

## Overview
When the opcodes are ran for the current frame
- It will process the "main" sections of the opcodes such as the main scope, or subroutines
- It will process the concurrently running methods, which the script can subscribe with through built in method

There are internal methods that can be called which isn't user defined
- TODO how to make a convinient way of defining and adding method
  - List of methods in a class??
  - If method needs to be called from outside, either have a method that takes those methods, or have an event for subscribing, which is then wrapped in the "list of methods in a class"

## AddMethod
Takes opcode list and name, stores for use from main method

## CheckValid
Checks currently stored opcodes, throws exception if something is invalid within
- Checks if methods exist

## CurrentInputs
Retrieves input data such as what is being pressed in the current frame
Returns null if no more data is found

## AdvanceFrame
Engine advances state to next frame

## IsMovieEnd
Returns true if movie has no more inputs to process
