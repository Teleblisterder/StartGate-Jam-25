using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static PlayerInput PlayerInput;
    public static Vector2 Movement;
    public static bool portalPressed;


    private InputAction _moveAction;
    private InputAction _portalAction;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _portalAction = PlayerInput.actions["Portal"];
    }
    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>().normalized;
        portalPressed = _portalAction.WasPressedThisFrame();

    }



}
