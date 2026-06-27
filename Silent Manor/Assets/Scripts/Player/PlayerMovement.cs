using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator anim;

    [SerializeField] private float footstepSpeed = .5f;
    private bool playingFootsteps = false;
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
        if(GameManager.Instance.IsGamePaused)
            return;

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

            // 正在走路 + 脚步声没播放 → 开启脚步声循环
            if (!playingFootsteps)
            {
                StartFootsteps();
            }
        }    
        else
        {
            anim.SetBool("isWalking", false);

            // 停下 + 脚步声正在播放 → 停止
            if (playingFootsteps)
            {
                StopFootsteps();
            }
        }

        anim.SetFloat("InputX", moveInput.x);
        anim.SetFloat("InputY", moveInput.y);
    }

    private void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootstep), 0f, footstepSpeed);
    }

    private void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    private void PlayFootstep()
    {
        SoundManager.Instance.Play("Footstep", true);
    }
}
