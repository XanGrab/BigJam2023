using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [Header("External References")]
    [SerializeField] private GameObject afterimagePrefab;

    [Header("Self-References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider col;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator boom_anim;
    [SerializeField] private List<Transform> raycastPoints;
    [SerializeField] private float raycastHeight;

    private PlayerAnimStateEnum currentAnimation;

    //Animation states
    enum PlayerAnimStateEnum
    {
        Player_Idle,
        Player_Jump_Up,
        Player_Jump_Down,
        Player_Drill_Right,
        Player_Drill_Down
    }

    [Header("Parameters")]
    [SerializeField] private float accelSpeed_ground;
    [SerializeField] private float accelSpeed_air;
    [SerializeField] private float frictionSpeed_ground;
    [SerializeField] private float frictionSpeed_air;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpPower;
    [Space(5)]
    [SerializeField] private float gravUp;
    [SerializeField] private float gravDown;
    [SerializeField] private float spaceReleaseGravMult;
    [Space(5)]
    [SerializeField] private LayerMask terrainLayer;

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


    /// <summary>
    /// Returns if the player is currently able to move (not attacking, dashing, stunned, etc.)
    /// </summary>
    /// <returns></returns>
    private bool CanMove =>
    (
        true
    );

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        if (CanMove)
        {
            #region Acceleration
            //Get gravityless velocity
            Vector3 noGravVelocity = rb.velocity;
            noGravVelocity.y = 0;

            //Convert global velocity to local velocity
            Vector3 velocity_local = noGravVelocity;

            //XZ Friction + acceleration
            Vector3 currInput = new Vector3(InputHandler.Instance.Direction.x, 0, InputHandler.Instance.Direction.y);

            if (currInput.magnitude > 0.05f)
                currInput.Normalize();
            if (grounded)
            {
                //Apply ground fricion
                Vector3 velocity_local_friction = velocity_local.normalized * Mathf.Max(0, velocity_local.magnitude - frictionSpeed_ground * Time.deltaTime);

                Vector3 updatedVelocity = velocity_local_friction;

                if (currInput.magnitude > 0.05f) //Pressing something, try to accelerate
                {
                    Vector3 velocity_local_input = velocity_local_friction + accelSpeed_ground * Time.deltaTime * currInput;

                    if (velocity_local_friction.magnitude <= maxSpeed)
                    {
                        //under max speed, accelerate towards max speed
                        updatedVelocity = velocity_local_input.normalized * Mathf.Min(maxSpeed, velocity_local_input.magnitude);
                    }
                    else
                    {
                        //over max speed
                        if (velocity_local_input.magnitude <= maxSpeed) //Use new direction, would go less than max speed
                        {
                            updatedVelocity = velocity_local_input;
                        }
                        else //Would stay over max speed, use vector with smaller magnitude
                        {
                            //Would accelerate more, so don't user player input
                            if (velocity_local_input.magnitude > velocity_local_friction.magnitude)
                                updatedVelocity = velocity_local_friction;
                            else
                                //Would accelerate less, user player input (input moves velocity more to 0,0 than just friciton)
                                updatedVelocity = velocity_local_input;
                        }
                    }
                }

                //Convert local velocity to global velocity
                rb.velocity = new Vector3(0, rb.velocity.y, 0) + transform.TransformDirection(updatedVelocity);
            }
            else
            {
                //Apply air fricion
                Vector3 velocity_local_friction = velocity_local.normalized * Mathf.Max(0, velocity_local.magnitude - frictionSpeed_air);

                Vector3 updatedVelocity = velocity_local_friction;

                if (currInput.magnitude > 0.05f) //Pressing something, try to accelerate
                {
                    Vector3 velocity_local_with_input = velocity_local_friction + currInput * accelSpeed_air;

                    if (velocity_local_friction.magnitude <= maxSpeed)
                    {
                        //under max speed, accelerate towards max speed
                        updatedVelocity = velocity_local_with_input.normalized * Mathf.Min(maxSpeed, velocity_local_with_input.magnitude);
                    }
                    else
                    {
                        //over max speed
                        if (velocity_local_with_input.magnitude <= maxSpeed) //Use new direction, would go less than max speed
                        {
                            updatedVelocity = velocity_local_with_input;
                        }
                        else //Would stay over max speed, use vector with smaller magnitude
                        {
                            // Debug.Log("withotInput: " + velocity_local.magnitude);
                            // Debug.Log(velocity_local);
                            // Debug.Log("input: " + velocity_local_with_input.magnitude);
                            // Debug.Log(velocity_local_with_input);
                            // Debug.Log("friction: " + velocity_local_friction.magnitude);
                            // Debug.Log(velocity_local_friction);

                            //Would accelerate more, so don't user player input
                            if (velocity_local_with_input.magnitude > velocity_local_friction.magnitude)
                                updatedVelocity = velocity_local_friction;
                            else
                                //Would accelerate less, user player input (input moves velocity more to 0,0 than just friciton)
                                updatedVelocity = velocity_local_with_input;
                        }
                    }
                }

                //Convert local velocity to global velocity
                rb.velocity = new Vector3(0, rb.velocity.y, 0) + updatedVelocity;
            }
            #endregion
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
                rb.velocity -= new Vector3(0, gravUp * Time.deltaTime, 0);
            else
                rb.velocity -= new Vector3(0, gravDown * Time.deltaTime, 0);
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

        #region determine if player is grounded or not
        grounded = false; //number of raycasts that hit the ground 

        foreach (Transform point in raycastPoints)
        {
            if (Physics.Raycast(point.position, -transform.up, out RaycastHit hit, raycastHeight, terrainLayer))
            {
                grounded = true;
                break;
            }
        }
        #endregion

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
