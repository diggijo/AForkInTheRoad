using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance {  get; private set; }
    private InputActions inputActions;
    private void Awake()
    {
        Instance = this;
        inputActions = new InputActions();
        inputActions.Enable();
    }

    public bool IsLeftClickPressed()
    {
        return inputActions.Game.DrawRoad.WasPressedThisFrame();
    }

    public Vector2 GetMousePosition()
    {
        return inputActions.Game.MousePosition.ReadValue<Vector2>();
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }
}
