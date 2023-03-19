# Problem
- Because the audio thread runs separately from the main thread, if the audio is generated too quickly compared to the main update thread, the audio will be choppy when recorded as the audio thread is going to be ahead of the main thread

# Idea
- The audio thread should be able to be paused and resumed, so that the main thread can catch up with the audio thread
- To also further reduce waiting time for the main thread, I can increase the audio buffer size, so that the audio thread can generate more audio before the main thread has to catch up
- I can either delay the audio thread via a sleep function, or I can use AudioListener.pause to pause the audio thread, just make sure to set AudioSource.ignoreListenerPause to false so audio is actually paused
  - For AudioListner.pause, in case something tries to set it, queue the data and set it when the audio thread is resumed

# Implementation
- On recording start, increase the audio buffer size to max it can be, store the old buffer size and disable any attempt of setting the buffer size via patching
- On record end, set the buffer size back to the old buffer size and enable setting the buffer size again

## AudioListener.pause
- Do the same as sleep except use AudioListener.pause and pause from the main thread when required to

## Sleep (NOT WORKING)
- I use a script and an external helper class that attaches itself to the AudioListener object, which invokes the OnAudioFilterRead delegate so I can use it
- When audio is generated, check if the audio thread is ahead of the main thread, if it is, pause the audio thread until the main thread goes ahead of the audio thread
- If main thread is ahead of audio thread more than a whole buffer size, pause the main thread until the audio thread goes ahead of the main thread

### Sleep issue
- Doing `Thread.Sleep` in the audio thread causes the audio thread to stop generating audio but also freezes the main thread