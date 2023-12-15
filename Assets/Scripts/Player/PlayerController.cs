using BeauRoutine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [Header("External References")]
    [SerializeField] private GameObject afterimagePrefab;
    [SerializeField] private Transform modeWheelTransform;
    [SerializeField] private SpriteRenderer modeWheelSprite;
    [SerializeField] private float[] scootSpeeds;
    [SerializeField] private GameObject[] attackPrefabs;

    private Routine spinWheelRoutine = Routine.Null;

    [Header("Self-References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] private Animator boom_anim;
    [SerializeField] private Transform attackRotate;
    [SerializeField] private Transform attackParent;
    [SerializeField] private List<Transform> raycastPoints;
    [SerializeField] private float raycastHeight;

    private bool canBufferAttack = false;
    private bool attackBuffered = false;

    private bool isAttacking;
    public event Action<int> OnModeChange;

    private PlayerAnimStateEnum currentAnimation;
    enum PlayerAnimStateEnum
    {
        Player_Idle,
        Player_Jump_Up,
        Player_Jump_Down,
        Player_Run,
        Player_Drill_Down,
        Tome_light = 5,
        Shield_light = 6,
        Gauntlet_light = 7,
        Scythe_light = 8,
        Hammer_light = 9,
        Katana_light = 10,
        Bow_light = 11,
    }

    private ModeEnum currentMode = ModeEnum.Tome;
    enum ModeEnum
    {
        Tome = 5,
        Shield = 6,
        Gauntlet = 7,
        Scythe = 8,
        Hammer = 9,
        Katana = 10,
        Bow = 11,
    }
    float numModes = 7;

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

            //XZ Friction + acceleration
            Vector3 currInput = new Vector3(InputHandler.Instance.Direction.x, 0, InputHandler.Instance.Direction.y);

            if (currInput.magnitude > 0.05f)
                currInput.Normalize();

            if (isAttacking)
                currInput = Vector3.zero;
            else
            {
                if (currInput.x > epsilon)
                    spriteRenderer.flipX = false;
                else if (currInput.x < -epsilon)
                    spriteRenderer.flipX = true;
            }

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

        if (isAttacking)
            return;

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
            if (Mathf.Abs(rb.velocity.x) > epsilon)
                ChangeAnimationState(PlayerAnimStateEnum.Player_Run);
            else
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


        if (!isAttacking)
        {
            if (spinWheelRoutine == Routine.Null)
            {
                if (InputHandler.Instance.PrevMode.Pressed)
                {
                    int currIndex = (int)currentMode;
                    currIndex--;

                    if (currIndex == 4)
                        currIndex = 11;

                    currentMode = (ModeEnum)currIndex;
                    spinWheelRoutine = Routine.Start(this, SpinWheel(-360f / numModes, currIndex));
                }
                else
                {
                    if (InputHandler.Instance.NextMode.Pressed)
                    {
                        int currIndex = (int)currentMode;
                        currIndex++;

                        if (currIndex == 12)
                            currIndex = 5;

                        currentMode = (ModeEnum)currIndex;
                        spinWheelRoutine = Routine.Start(this, SpinWheel(360f / numModes, currIndex));
                    }
                }
            }

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
            if (attackBuffered || InputHandler.Instance.LightAttack.Pressed)
            {
                ChangeAnimationState((PlayerAnimStateEnum)currentMode);

                isAttacking = true;
                attackBuffered = false;
            }
        }
        else
        {
            if (canBufferAttack && InputHandler.Instance.LightAttack.Pressed)
                attackBuffered = true;
        }

        //Space release gravity
        if (InputHandler.Instance.Jump.Released && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * spaceReleaseGravMult);
        }
    }

    private IEnumerator SpinWheel(float _degToTurn, int _modeIndex)
    {
        Vector3 startAngle = modeWheelTransform.localEulerAngles;

        yield return Tween.Float(0, 1, (f) => { modeWheelSprite.SetAlpha(f); }, 0.1f);

        yield return Tween.Float(0, _degToTurn, (f) => { modeWheelTransform.localEulerAngles = startAngle + new Vector3(0, 0, f); }, 0.1f);

        OnModeChange?.Invoke(_modeIndex);

        yield return Tween.Float(1, 0, (f) => { modeWheelSprite.SetAlpha(f); }, 0.1f);

        spinWheelRoutine = Routine.Null;
    }

    private void Jump()
    {
        return;

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

    public void SpawnHitbox()
    {
        canBufferAttack = true;

        if (spriteRenderer.flipX)
            attackRotate.localScale = new Vector3(-1, 1, 1);
        else
            attackRotate.localScale = Vector3.one;

        GameObject newHitbox = Instantiate(attackPrefabs[(int)currentMode - 5], attackParent);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;

        canBufferAttack = false;
    }

    public void Scoot()
    {
        float speedToUse = scootSpeeds[(int)currentMode - 5];

        if (spriteRenderer.flipX)
            speedToUse *= -1;

        rb.velocity = new Vector3(speedToUse, rb.velocity.y, rb.velocity.z);
    }
}
