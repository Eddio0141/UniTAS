How to sync update and fixed update invokes

# Idea

- patch all `Update` event invokes and grab `Time.deltaTime` on the first invoke. this tracks the deltaTime but
  subtracts by physics update time if the tracking time is bigger than that
- when syncing, use last update event to calculate the fps to set to change the offset to match fixed update perfectly,
  then this makes it so the next update syncs
    - if the version of unity supports setting fps by float, you can just directly
      pass `fixed_update_rate - tracking_time`
    - if the version of unity doesn't support fps by float, might have to set fps for few frames until its at a
      tolerable sync
        - calculate fps with just `1 / (fixed_update_rate - tracking_time)` and repeat that
    - make sure to set `Time.maximumDeltaTime` to something that can cover the fps im doing and restore it later

# Extra notes

## save state syncing

when I eventually work on save states, i need to make the update invoke offset match with the save state one. to do
this, I should make the new syncing accept an argument to specify how many seconds of offset you want

# Implementation

1. Receive callback to process
2. Queue up the callback to be processed
3. If not processing, grab item from queue and start process
4. If first processing, store original frame time
5. On processing, set frame time to target sync time, if target time is lower than target, wait until update invoke and
   set it again
6. If target time matches, invoke callback