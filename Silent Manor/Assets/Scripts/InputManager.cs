using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public static PlayerInput input { get; private set; }
    public static Vector2 movement;

    public static bool WasTabClicked;
    public static bool canPlayerMove = true;

    protected override void Awake()
    {
        base.Awake();
        input = new PlayerInput();
    }

    void OnEnable()
    {
        input.Enable();
        input.UI.Enable();
    }

    void OnDisable()
    {
        input.UI.Disable();
        input.Disable();
    }

    void Update()
    {
        if(canPlayerMove)
            movement = input.Player.Move.ReadValue<Vector2>();
        else
            movement = Vector2.zero;

        WasTabClicked = input.UI.Info.WasPressedThisFrame();
    }

    void OnDestroy()
    {
        input?.Dispose();
    }
}
