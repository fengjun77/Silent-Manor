using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Move(InputManager.movement);
        
    }

    private void Move(Vector2 moveInput)
    {
        if(moveInput.magnitude > 1f)
            moveInput.Normalize();

        rb.linearVelocity = moveInput * moveSpeed;

        if(moveInput != Vector2.zero)
        {
            anim.SetBool("isWalking", true);
            anim.SetFloat("LastInputX", moveInput.x);
            anim.SetFloat("LastInputY", moveInput.y);
        }    
        else
        {
            anim.SetBool("isWalking", false);
            
        }

        anim.SetFloat("InputX", moveInput.x);
        anim.SetFloat("InputY", moveInput.y);
    }
}
