# Problem
In unity, I need to have a unscaled delta time to calculate "real time" where as before this fix, I just tried to calculate unscaled delta time by doing `Time.deltaTime / Time.timeScale` but this is not accurate as well as not working when the time scale is 0

# Solution
I can simply use `Time.unscaledDeltaTime` to get the unscaled delta time but this doesn't exist in all unity versions so I need to use `Time.unscaledDeltaTime` if it exists and if not, I need to force the game FPS to the virtual environment in order to always know the delta time