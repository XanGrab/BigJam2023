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

    private bool isDead = false;

    private AnimState currentAnimation;
    enum AnimState
    {
        Idle,
        Walking,
        Attack,
        Death
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

    [SerializeField] private float distToPlayerForAttack = 1.5f;
    [SerializeField] private float scootSpeed;
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackParent;
    [SerializeField] private Transform attackRotate;

    [SerializeField] private bool isSpin = false;

    private bool isAttacking;

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
            // ChangeAnimationState( AnimState.Walking );
            #region Acceleration
            //Get gravityless velocity
            Vector3 noGravVelocity = rb.velocity;
            noGravVelocity.y = 0;

            //Convert global velocity to local velocity
            Vector3 velocity_local = noGravVelocity;

            Vector3 dist = (PlayerController.Instance.transform.position - transform.position);
            dist.y = 0;
            Vector3 currInput = new Vector3();

            if (!isDead)
            {
                //XZ Friction + acceleration
                if (dist.magnitude > distToPlayerForAttack)
                    currInput = dist.normalized;
                else
                {
                    if (!isAttacking)
                    {
                        ChangeAnimationState(AnimState.Attack);

                        isAttacking = true;
                    }
                }
            }

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

        if (isAttacking || isDead)
            return;

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
            if (Mathf.Abs(rb.velocity.x) > epsilon)
                ChangeAnimationState(AnimState.Walking);
            else
                ChangeAnimationState(AnimState.Idle);
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
    }

    private void ChangeAnimationState(AnimState _newState, bool doAnyways = false)
    {
        //Stop same animation from interrupting itself
        if (currentAnimation == _newState)
            return;

        if (!isSpin || doAnyways)
        {
            //Play new animation
            anim.Play(_newState.ToString());
        }

        //Update current anim state var
        currentAnimation = _newState;
    }

    private void Jump()
    {
        useGravity = true;

        rb.velocity = new Vector2(rb.velocity.x, jumpPower);

        grounded = false;
    }

    public void SpawnHitbox()
    {
        if (spriteRenderer.flipX)
            attackRotate.localScale = new Vector3(-1, 1, 1);
        else
            attackRotate.localScale = Vector3.one;

        GameObject newHitbox = Instantiate(attackPrefab, attackParent);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    public void Scoot()
    {
        float speedToUse = scootSpeed;

        if (spriteRenderer.flipX)
            speedToUse *= -1;

        if (isSpin)
        {
            Vector3 dist = (PlayerController.Instance.transform.position - transform.position).normalized;
            dist.y = 0;
            rb.velocity = dist * scootSpeed;
        }
        else
            rb.velocity = new Vector3(speedToUse, rb.velocity.y, rb.velocity.z);
    }

    public void OnDeath()
    {
        if (isDead)
            return;

        ChangeAnimationState(AnimState.Death, true);

        rb.velocity = Vector3.zero;
        isDead = true;

        PlayerHUD.Instance.OnEnemyKilled();
        Debug.Log("enemy killed!");
    }
}
