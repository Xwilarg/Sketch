## How to use

This package come with no dependency by default but we recommand you to add sketch.common with the following code to get started easily
```
private void Update()
{
    // Get an instance of your PlayerInput
    PlayerInput pInput = /* */;

    // Get mouse position (with sketch.common)
    var mousePos = CursorUtils.GetPosition(pInput).Value;

    // Update position of drawing
    DrawingManager.Instance.UpdatePosition(mousePos);
}

// Callback for Unity input system
public void OnClick(InputAction.CallbackContext value)
{
    if (value.phase == InputActionPhase.Started)
    {
        DrawingManager.Instance.UpdateMousePress(true);
    }
    else if (value.phase == InputActionPhase.Canceled)
    {
        DrawingManager.Instance.UpdateMousePress(false);
    }
}
```
