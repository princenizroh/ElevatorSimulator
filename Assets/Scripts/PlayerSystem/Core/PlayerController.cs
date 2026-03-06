using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Controller
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 5f;

        [SerializeField] private LayerMask groundLayer;

        [Header("Squash & Stretch")]
        [SerializeField] private float squashAmount = 0.8f;  // Pendek melebar
        [SerializeField] private float stretchAmount = 1.2f; // Tinggi ramping
        [SerializeField] private float squashSpeed = 10f; 
        private Vector3 targetScale = new Vector3(4f, 4f, 1f);
        Rigidbody2D rb;
        Animator animator;
        Collider2D coll;
        Vector2 movement;
        bool canJump = true;
        private bool wasInAir = false;
        public PlayerInputActions InputActions {get; private set; }
        public PlayerInputActions.PlayerActions PlayerActions { get; private set; }

        // Awake untuk inisialisasi komponen
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            coll = GetComponent<Collider2D>();

            InputActions = new PlayerInputActions();
            PlayerActions = InputActions.Player;

        }
        
        // OnEnable untuk mengaktifkan input action
        void OnEnable()
        {
            PlayerActions.Enable();
            PlayerActions.Jump.performed += OnJumpInput;
        }

        // OnDisable untuk menonaktifkan input action
        void OnDisable()
        {
            PlayerActions.Jump.performed -= OnJumpInput;
            PlayerActions.Disable();
        }

        // Update untuk menangani logika per frame
        void Update()
        {
            HandleMovemnt();
            HandleSquashStretch();
            if (IsGrounded())
                canJump = true;
        }

        // FixedUpdate untuk menangani fisika per frame
        void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);
        }

        // HandleMovemnt untuk menangani input pergerakan dan animasi
        private void HandleMovemnt()
        {
            Vector2 input = PlayerActions.Movement.ReadValue<Vector2>();

            movement = new Vector2(input.x, 0f);

            animator.SetFloat("Speed", math.abs(movement.x));

            // Flip sprite berdasarkan arah pergerakan
            if (movement.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(movement.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }  
        }

        // HandleSquashStretch untuk menangani efek squash dan stretch
        private void HandleSquashStretch()
        {
            float velocityY = rb.linearVelocity.y;
            float direction = Mathf.Sign(transform.localScale.x);
            
            if (!IsGrounded())
            {
                wasInAir = true;
                // Jumping stretch
                if (velocityY > 2f)
                {
                    targetScale = new Vector3(
                        direction * 4 * 0.8f,  // 3.2
                        4 * stretchAmount,    // 4.8                      
                        1f
                    );
                }

                // Falling 
                else if (velocityY < -2f)
                {
                    targetScale = new Vector3(
                        direction * 4 * 0.9f,   // 3.6f
                        4 * 1.1f,   // 4.4f
                        1f  
                    );
                }
            }

            else {
                // Landing squash
                if (wasInAir)
                {
                    targetScale = new Vector3(
                        direction * 4 * 1.2f,  // 4.8
                        4 * squashAmount,    // 3.2                    
                        1f
                    );
                    wasInAir = false;
                }

                // Normal
                else
                {
                    targetScale = new Vector3(
                        direction * 4f,
                        4f,
                        1f
                    );
                }

            }

        
            if (IsGrounded() || wasInAir)
            {
                transform.localScale = Vector3.Lerp(
                    transform.localScale, 
                    targetScale, 
                    squashSpeed * Time.deltaTime
                );
            }
        }


        // IsGrounded untuk memeriksa apakah pemain berada di tanah
        private bool IsGrounded()
        {
            float extraHeight = 0.2f; 
            return Physics2D.Raycast(
                coll.bounds.center, 
                Vector2.down, 
                coll.bounds.extents.y + extraHeight, 
                groundLayer
            );            
         
        }

        // OnJumpInput untuk menangani input lompat
        private void OnJumpInput(InputAction.CallbackContext context)
        {
            if (IsGrounded() && canJump)
            {
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                canJump = false;
            } 
        }
    }
}
