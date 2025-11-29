## How to use

This package come with no dependency by default but we recommand you to add sketch.common with the following code to get started easily
```cs
private bool _isMousePressed;

private void Update()
{
    // Get an instance of your PlayerInput
    PlayerInput pInput = /* */;

    // Get mouse position (with sketch.common)
    var mousePos = CursorUtils.GetPosition(pInput).Value;

    // Update position of drawing
    DrawingManager.Instance.UpdatePosition(mousePos, _isMousePressed);
}

// Callback for Unity input system
public void OnClick(InputAction.CallbackContext value)
{
    if (value.phase == InputActionPhase.Started)
    {
        _isMousePressed = true;
    }
    else if (value.phase == InputActionPhase.Canceled)
    {
        _isMousePressed = false;
    }
}
```

You can then define your object that will be circled
```cs
public class YourObject : MonoBehaviour, ITargetShape
{
    public PolygonCollider2D Collider => _coll;

    public Vector2 Position => transform.position;

    public float Scale => transform.localScale.x;

    public void GetCircled()
    {
        // Do something once this object is circled
    }

    private PolygonCollider2D _coll;
    
    private void Awake()
    {
        _coll = GetComponent<PolygonCollider2D>();
        DrawingManager.Instance.Register(this);
    }
}
```
