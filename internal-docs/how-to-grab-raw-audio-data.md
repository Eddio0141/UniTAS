# Steps
1. Find the game object that has AudioListener component
2. Attach your own script that contains the event method "OnAudioFilterRead" to the game object

# Keeping the script alive
- On the script with the event method, add "OnDestroy", which then you can callback to something to notify the audio grabber has been destroyed
- On destruction of the audio grabber, redo the steps above