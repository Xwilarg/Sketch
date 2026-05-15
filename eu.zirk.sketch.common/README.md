Mainly for internal use for others sketch modules

- `CameraUtils.CalculateBounds`: Calculate the bounds of what the camera can see in the world (might cause issues on perspective camera)
- `CursorUtils.GetPosition`: Get mouse position, also handle touch screen, returns null if no press detected
- `Timer`: Timer using Update loop to keep track of progress
    - Use OnDone to define a callback once done
    - Call Start with the time in seconds to start the timer
    - Call Update every Update with `Time.DeltaTime` to update the timer