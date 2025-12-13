using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D Rigidbody2D;
    private Vector2 MovementInput;

    [SerializeField] private Animator animator;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    public float speedMultiplier = 1f;
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
    
        MovementInput = InputManager.Movement;
        CharAnimations();

    }
    void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = MovementInput * moveSpeed;
    }
    public void CharAnimations()
    {

        if (MovementInput.sqrMagnitude > 0.01f)
        {
            animator.SetBool("isWalking", true);
            animator.SetFloat("inputX", MovementInput.x);
            animator.SetFloat("inputY", MovementInput.y);
            animator.SetFloat("LastInputX", MovementInput.x);
            animator.SetFloat("LastInputY", MovementInput.y);

            FacingDirection = MovementInput.normalized;
        }
        else
        {
            animator.SetBool("isWalking", false);

        }


    }

}
