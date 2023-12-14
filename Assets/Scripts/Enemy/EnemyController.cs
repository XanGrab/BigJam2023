using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private GameObject afterimagePrefab;

    [Header("Self-References")]
    [SerializeField] private Rigidbody rb;
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

    /// <returns>/// Returns if the player is currently able to move (not attacking, dashing, stunned, etc.)</returns>
    private bool CanMove => (true);

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

            Vector3 dist = (PlayerController.Instance.transform.position - transform.position);
            dist.y = 0;
            Vector3 currInput = new Vector3();
            //XZ Friction + acceleration
            if (dist.magnitude > 2)
                currInput = dist.normalized;

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
        if (rb.velocity.x < -epsilon)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            if (rb.velocity.x > epsilon)
            {
                spriteRenderer.flipX = false;
            }
        }

        //Gravity
        if (useGravity)
        {
            if ((PlayerController.Instance.transform.position.y - 1 > transform.position.y) && rb.velocity.y > 0)
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
        if (PlayerController.Instance.transform.position.y - 2 > transform.position.y)
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

        //TODO: Enemy atttacks

        //Boom attack
        //if (InputHandler.Instance.LightAttack.Pressed)
        //{
        //    float angle;

        //    if (InputHandler.Instance.Direction.magnitude > epsilon)
        //    {
        //        //Attack in the held direction
        //        angle = Mathf.Atan2(InputHandler.Instance.Direction.y, InputHandler.Instance.Direction.x);
        //        angle = Mathf.Rad2Deg * angle;

        //        angle = Mathf.RoundToInt(angle / 90.0f) * 90.0f;
        //    }
        //    else
        //    {
        //        //Attack in the faced direction (horizontally)
        //        if (!spriteRenderer.flipX)
        //            angle = 0;
        //        else
        //            angle = 180;
        //    }

        //    boom_anim.transform.eulerAngles = new Vector3(0, 0, angle);

        //    boom_anim.SetTrigger("Boom");
        //}

        ////Space release gravity
        //if (InputHandler.Instance.Jump.Released && rb.velocity.y > 0)
        //{
        //    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * spaceReleaseGravMult);
        //}
    }

    private void Jump()
    {
        useGravity = true;

        rb.velocity = new Vector2(rb.velocity.x, jumpPower);

        grounded = false;
    }

    private void ChangeAnimationState(PlayerAnimStateEnum _newState) {
        //Stop same animation from interrupting itself
        if (currentAnimation == _newState || !anim)
            return;

        //Play new animation
        anim.Play(_newState.ToString());

        //Update current anim state var
        currentAnimation = _newState;
    }
}
