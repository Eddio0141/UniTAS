# Design on calling original methods on patched method

## Reverse patches

This would work mostly except recursively patched methods.
The reverse patch will reverse the original method, but if the original method calls a patched method, this won't work

## Wrapper