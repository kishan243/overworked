using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float dashSpeed = 20f;

    private Rigidbody rb;
    private Vector3 moveDir;

    // Dash variables
    private bool isDashing = false;
    private float dashTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. Get Input (WASD)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        moveDir = new Vector3(x, 0, z).normalized;

        // 2. Dash Input (L)
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            isDashing = true;
            dashTimer = 0.2f; // Dash lasts 0.2 seconds
        }

        // 3. Action Inputs (Just prints to console for now)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pick Up / Drop");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Chop!");
        }
    }

    void FixedUpdate()
    {
        // 4. Move
        if (isDashing)
        {
            // Move super fast while dashing
            rb.velocity = moveDir * dashSpeed;

            // Count down dash timer
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
        else
        {
            // Normal Movement
            rb.velocity = moveDir * moveSpeed;
        }

        // 5. Face the direction we are walking (Instant Snap)
        if (moveDir != Vector3.zero)
        {
            transform.forward = moveDir;
        }
    }
}