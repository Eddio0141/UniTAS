# Soft restart

## Things that happens in order
- TODO check normal start up (gameobj count and stuff) with soft restart start up and see if its accurate
- All game objects gets deleted
- All fields are reset to default
- Inner `updateIndex` will wait until index is TODO count is hit
- RNG will reinitialize with seed
- Load first scene