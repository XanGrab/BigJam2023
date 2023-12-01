using UnityEngine;

public class PlayerController : Singleton<PlayerController> {
    [Header("External References")]
    [SerializeField] private GameObject afterimagePrefab;

    [Header("Self-References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator boom_anim;

    private PlayerAnimStateEnum currentAnimation;

    //Animation states
    enum PlayerAnimStateEnum {
        Player_Idle,
        Player_Jump_Up,
        Player_Jump_Down,
        Player_Drill_Right,
        Player_Drill_Down
    }

    [Header("Parameters")]
    [SerializeField] private float accelSpeed;
    [SerializeField] private float frictionSpeed_Ground;
    [SerializeField] private float frictionSpeed_Air;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpPower;
    [Space(5)]
    [SerializeField] private float gravUp;
    [SerializeField] private float gravDown;
    [SerializeField] private float spaceReleaseGravMult;
    [Space(5)]
    [SerializeField] private LayerMask terrainLayer;

    private float xSpeed = 0;
    private bool grounded = false;
    private bool useGravity = true;
    private float lastTimePressedJump = -100.0f;
    private float lastTimeGrounded = -100.0f;
    private const float epsilon = 0.05f;
    private const float jumpBuffer = 0.1f;
    private const float coyoteTime = 0.1f;

    #region Afterimage
    private const float secsPerAfterimage = 0.01f;
    private float lastTimeSpawnedAfterimage = -100.0f;
    #endregion

    /// <returns>/// Returns if the player is currently able to move (not attacking, dashing, stunned, etc.)</returns>
    private bool CanMove => ( true );

    private void Update() {

        if (CanMove) {
            //Accelerate + Friction
            float inputX = InputHandler.Instance.Direction.x;

            if (inputX < -epsilon) //Pressing left
            {
                if (xSpeed > -maxSpeed)
                {
                    //Can still accelerate to the left (but do not exceed max)
                    xSpeed = Mathf.Max(-maxSpeed, xSpeed + accelSpeed * inputX * Time.deltaTime);
                }
                else
                {
                    //Apply friction for when moving over max speed
                    if (grounded)
                        xSpeed = Mathf.Min(xSpeed + frictionSpeed_Ground * Time.deltaTime, -maxSpeed);
                    else
                        xSpeed = Mathf.Min(xSpeed + frictionSpeed_Air * Time.deltaTime, -maxSpeed);
                }
            }
            else
            {
                if (inputX > epsilon) //Pressing right
                {
                    if (xSpeed < maxSpeed)
                    {
                        //Can still accelerate to the right (but do not exceed max)
                        xSpeed = Mathf.Min(maxSpeed, xSpeed + accelSpeed * inputX * Time.deltaTime);
                    }
                    else
                    {
                        //Apply friction for when moving over max speed
                        if (grounded)
                            xSpeed = Mathf.Max(xSpeed - frictionSpeed_Ground * Time.deltaTime, maxSpeed);
                        else
                            xSpeed = Mathf.Max(xSpeed - frictionSpeed_Air * Time.deltaTime, maxSpeed);
                    }
                }
                else //pressing nothing
                {
                    //Get sign of current speed
                    float sign = 1;
                    if (xSpeed < 0)
                        sign = -1;

                    //Not pressing anything, subtract friction and update speed
                    float newSpeedMagnitude;
                    if (grounded)
                        newSpeedMagnitude = Mathf.Max(0, Mathf.Abs(xSpeed) - frictionSpeed_Ground * Time.deltaTime);
                    else
                        newSpeedMagnitude = Mathf.Max(0, Mathf.Abs(xSpeed) - frictionSpeed_Air * Time.deltaTime);
                    xSpeed = newSpeedMagnitude * sign;
                }
            }

            //Set velocity
            rb.velocity = new Vector2(xSpeed, rb.velocity.y);
        }


        //Sprite flipping + spark vfx
        bool leftVFXActive = false;
        bool rightVFXActive = false;
        if (rb.velocity.x < -epsilon)
        {
            spriteRenderer.flipX = true;
            leftVFXActive = true;
        }
        else
        {
            if (rb.velocity.x > epsilon)
            {
                spriteRenderer.flipX = false;
                rightVFXActive = true;
            }
        }

        //Gravity
        if (useGravity)
        {
            if (InputHandler.Instance.Jump.Holding && rb.velocity.y > 0)
                rb.velocity -= new Vector2(0, gravUp * Time.deltaTime);
            else
                rb.velocity -= new Vector2(0, gravDown * Time.deltaTime);
        }

        //Set animation states
        if (grounded)
        {
            ChangeAnimationState(PlayerAnimStateEnum.Player_Idle);
        }
        else
        {
            if (rb.velocity.y >= 0)
                ChangeAnimationState(PlayerAnimStateEnum.Player_Jump_Up);
            else
                ChangeAnimationState(PlayerAnimStateEnum.Player_Jump_Down);
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Ground checking
        bool lastGrounded = grounded;
        grounded = (rb.velocity.y < 1.0f) && Physics2D.BoxCast(col.bounds.center, col.bounds.size * 0.99f, 0f, Vector2.down, 0.1f, terrainLayer);


        //Get last time grounded
        if ((lastGrounded == true) && (grounded == false))
            lastTimeGrounded = Time.time;

        //Jump - grounded
        if (InputHandler.Instance.Jump.Pressed)
        {
            if ((grounded || (Time.time - lastTimeGrounded <= coyoteTime)))
                Jump();
            else
                lastTimePressedJump = Time.time;
        }
        //Jump - buffered
        if ((grounded == true))
        {
            if (Time.time - lastTimePressedJump <= jumpBuffer)
                Jump();
        }

        //Boom attack
        if (InputHandler.Instance.LightAttack.Pressed)
        {
            float angle;

            if (InputHandler.Instance.Direction.magnitude > epsilon)
            {
                //Attack in the held direction
                angle = Mathf.Atan2(InputHandler.Instance.Direction.y, InputHandler.Instance.Direction.x);
                angle = Mathf.Rad2Deg * angle;

                angle = Mathf.RoundToInt(angle / 90.0f) * 90.0f;
            }
            else
            {
                //Attack in the faced direction (horizontally)
                if (!spriteRenderer.flipX)
                    angle = 0;
                else
                    angle = 180;
            }

            boom_anim.transform.eulerAngles = new Vector3(0, 0, angle);

            boom_anim.SetTrigger("Boom");
        }

        //Space release gravity
        if (InputHandler.Instance.Jump.Released && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * spaceReleaseGravMult);
        }
    }

    private void Jump()
    {
        useGravity = true;

        rb.velocity = new Vector2(rb.velocity.x, jumpPower);

        grounded = false;
    }

    private void SpawnAfterimage()
    {
        lastTimeSpawnedAfterimage = Time.time;
    }

    private void ChangeAnimationState(PlayerAnimStateEnum _newState)
    {
        //Stop same animation from interrupting itself
        if (currentAnimation == _newState)
            return;

        //Play new animation
        anim.Play(_newState.ToString());

        //Update current anim state var
        currentAnimation = _newState;
    }
}
