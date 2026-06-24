using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerInput input { get; private set; }

    public static Vector2 movement;

    void Awake()
    {
        input = new PlayerInput();
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        movement = input.Player.Move.ReadValue<Vector2>();
    }
}
