using UnityEngine;
using KinematicCharacterController;

public enum  CrouchInput
{
    None, Toggle
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}


public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 15f;
    [SerializeField] private float crouchResponse = 15f;
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0f,1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] public float airJumpsRemaining = 1f;
    [Range(1f,5f)]
    [SerializeField] public float airJumpsMax = 1f;
    [SerializeField] private float gravity = -90f;
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float baseSlideFriction = 0.8f;
    private float alteredSlideFriction;
    [SerializeField] private float currentSlideFriction;
    public float alteredSlideFrictionTimer;
    [SerializeField] private float splatterCheckRadius = 1f;
    [SerializeField] private LayerMask splatterLayer;
    private Collider[] _splatterResults = new Collider[4];
    [SerializeField]  private float slideSteerAcceleration = 5f;
    [SerializeField]  private float slideGravity = -90f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    [SerializeField] private float checkpointCheckRadius = 1f;
    [SerializeField] private LayerMask checkpointLayer;
    private Collider[] _checkpointResults = new Collider[4];

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _preventAirClimbing = true;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;

    private Collider[] _uncrouchOverlapResults;
    public static PlayerCharacter Instance { get; private set; }
    
    void Awake()
    {
        Instance = this;
        currentSlideFriction = baseSlideFriction;
    }

    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;

        _uncrouchOverlapResults = new Collider[8];

        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        // Take the 2D input vector and create a 3D movement vector on the Xz plane
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp the length to 1 to prevent moving faster diagonally with WASD input
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        //Orient the input so its relative to the direction the player is facing
        _requestedMovement = input.Rotation * _requestedMovement;

        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequestingJump)
            _timeSinceJumpRequest = 0f;

        _requestedSustainedJump = input.JumpSustain;

        var wasRequestingCrouch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            //CrouchInput.Crouch => true,
            //CrouchInput.Uncrouch => false
            _ => _requestedCrouch
        };
        if (_requestedCrouch && !wasRequestingCrouch)
            _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasRequestingCrouch)
            _requestedCrouchInAir = false;
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        var cameraTargetHeight = currentHeight *
        (
            _state.Stance is Stance.Stand
                ? standCameraTargetHeight
                : crouchCameraTargetHeight
        );
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp
        (
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );
        root.localScale = Vector3.Lerp
        (
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );
    }
    
    public void AddVelocity(Vector3 velocity)
    {
        motor.BaseVelocity += velocity;
    }
    
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;

        //If on the ground...
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0f;
            _ungroundedDueToJump = false;
            
            //Snap the requested movement direction to the angle of the surface the character is on
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            //Start sliding
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand;
                var wasInAir = !_lastState.Grounded;
                if (moving && crouching && (wasStanding || wasInAir))
                {
                    Debug.DrawRay(transform.position, currentVelocity, Color.red, 5f);
                    Debug.DrawRay(transform.position, _lastState.Velocity, Color.green, 5f);

                    _state.Stance = Stance.Slide;

                    // When landing on stable ground the character motor projects the velocity onto a flat gound plane
                    // See: KinematicCharacterMotor.HandleVelocityProjection()
                    // This is normally good because under normal cicumstances the player shouldn't slide when landing
                    // Here, however, we *want* the player to slide
                    // Reproject the last frames (falling) velocity onto the ground normal to slide
                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: _lastState.Velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    };

                    var effectiveSlideStartSpeed = slideStartSpeed;
                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        _requestedCrouchInAir = false;
                    }
                    var slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);
                    currentVelocity = motor.GetDirectionTangentToSurface
                    (
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;
                }
            }

            //Move
            if (_state.Stance is Stance.Stand or Stance.Crouch)
            {
                //Calculate the speed and responsiveness of movement based on the character's stance
                var speed = _state.Stance is Stance.Stand
                    ? walkSpeed
                    : crouchSpeed;
                
                var response = _state.Stance is Stance.Stand
                    ? walkResponse
                    : crouchResponse;
                
                //Smoothly move along the ground in that direction
                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
                
                // Update acceleration
                _state.Acceleration = moveVelocity - currentVelocity;

                // Update current velocity to new move velocity
                currentVelocity = moveVelocity;
            }

            // Continue sliding
            else
            {
                // Friction
                currentVelocity -= currentVelocity * (currentSlideFriction * deltaTime);

                // On a slope
                {
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;

                    currentVelocity -= force * deltaTime;
                }
                
                // Steer
                {
                    // Target velocity is the player's movement direction at the current speed
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentSpeed;
                    var steerVelocity = currentVelocity;
                    var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;
                    // Add steer force but clamp velocity so the slide speed doesn't increase due to direct movement input
                    steerVelocity += steerForce;
                    steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                    _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                    
                    currentVelocity = steerVelocity;
                }

                //Stop
                if (currentVelocity.magnitude < slideEndSpeed)
                    _state.Stance = Stance.Crouch;
            }

            
            CheckSplatterOverlap();

            //Change friction during slide (to the splatter friction) if splatter timer is above 0
            if (alteredSlideFrictionTimer > 0f)
            {
                currentSlideFriction = alteredSlideFriction;
                alteredSlideFrictionTimer -= Time.deltaTime;
            }
            else
            {
                currentSlideFriction = baseSlideFriction;
            }


            //Set jumps remaining to the maximum
            airJumpsRemaining = airJumpsMax;
        }
        //Else, in the air...
        else
        {
            _timeSinceUngrounded  += deltaTime;
            //Move midair
            if (_requestedMovement.sqrMagnitude >0f)
            {
                //Requested movement projected onto movement plane (magnitude preserved)
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.magnitude;

                // Current velocity on movement plane.
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                //Calculate movement force
                var movementForce = planarMovement * airAcceleration * deltaTime;

                // If moving slower than the max air speed, treat movementForce as a simple steering force
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    //Add it to the current planar velocity for a target velocity.
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    //Limit target velocity to air speed
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    //Steer towards current velocity
                    movementForce += targetPlanarVelocity - currentPlanarVelocity;
                }
                // Otherwise, nerf the movement force when it is in the direction of the current planar velocity to prevent acceleration further beyond the max air speed
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    //Project movement force onto the plane whose normal is the current planar velocity
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                //Prevent air-climbing steep slopes
                if (motor.GroundingStatus.FoundAnyGround && _preventAirClimbing)
                {
                    // If moving in the same direction as the resultant velocity...
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce ) > 0f)
                    {
                        //Calculate obstruction normal
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )
                        ).normalized;

                        // Project movement force onto obstruction plane
                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }

                currentVelocity += movementForce;
            }
            
            //Gravity
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);

            if (_requestedSustainedJump && verticalSpeed > 0f)
                effectiveGravity *= jumpSustainGravity;

            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }
        
        //If jump is requested...
        if (_requestedJump)
        {
            var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;


            //Then check if they are grounded or if they have jumps remaining
            if (motor.GroundingStatus.IsStableOnGround || canCoyoteJump || airJumpsRemaining > 0)
            {
                _requestedJump = false; // Unset jump request
                _requestedCrouch = false; // and request the character uncrouches
                _requestedCrouchInAir = false;

                //If midair remove a jump
                if (!motor.GroundingStatus.IsStableOnGround && !canCoyoteJump)
                {
                    airJumpsRemaining = airJumpsRemaining - 1f;
                }

                //Unstick player from ground
                UnstickFromGround();
                _ungroundedDueToJump = true;

                //Set minimum vertical velocity
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                //Add the difference in current and target vertical speed to the character's velocity
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
                Debug.Log("Jump successful! Grounded = " + motor.GroundingStatus.IsStableOnGround + ", " + airJumpsRemaining + " airjump(s) remaining");
            }
            else
            {
                _timeSinceJumpRequest += deltaTime;

                //Defer the jump request until coyote time has passed
                var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                _requestedJump = canJumpLater;

                Debug.Log("Jump failed/delayed! Grounded = " + motor.GroundingStatus.IsStableOnGround + ", " + airJumpsRemaining + " airjump(s) remaining");
            }
        }
    }

    public void UnstickFromGround()
    {
        motor.ForceUnground(time: 0.1f);
    }

    private void CheckSplatterOverlap()
    {
        var count = Physics.OverlapSphereNonAlloc(
            motor.TransientPosition,
            splatterCheckRadius,
            _splatterResults,
            splatterLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            var splatter = _splatterResults[i].GetComponent<SplatterEffects>();
            if (splatter != null)
            {
                alteredSlideFriction = splatter.frictionOnSplatter;
                alteredSlideFrictionTimer = 0.5f;
            }
        }
    }

    private void CheckCheckpointOverlap()
    {
        var count = Physics.OverlapSphereNonAlloc(
            motor.TransientPosition,
            checkpointCheckRadius,
            _checkpointResults,
            checkpointLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            var spawnPoint = _checkpointResults[i].GetComponent<SpawnPoint>();
            if (spawnPoint != null)
            {
                SpawnManager.Instance.SetActiveSpawnPoint(spawnPoint);
            }
        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }
    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        //Crouch
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }
    public void PostGroundingUpdate(float deltaTime)
    {
        // Stop sliding if midair
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
        {
            _state.Stance = Stance.Crouch;
        }
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        //Uncrouch
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            //Tentatively "stanup" the character capsule
            _state.Stance = Stance.Stand;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight * 0.5f
            );

            //Then see if the capsulke overlaps any colliders before actually standing up
            var pos = motor.TransientPosition; 
            var rot = motor.TransientRotation; 
            var mask = motor.CollidableLayers; 
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                //Re-crouch
                _requestedCrouch = true;
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: crouchHeight * 0.5f
                );
            }
            else
            {
                _state.Stance = Stance.Stand;
            }
        }

        // Update state to reflect relevant motor properties
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        // And update the _lastState to store the character state snapshot taken at the start of the character update
        _lastState = _tempState;

        CheckCheckpointOverlap();
    }
    public bool IsColliderValidForCollisions(Collider coll) => true;
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport){}
    public void OnDiscreteCollisionDetected(Collider hitCollider){}
    public Transform GetCameraTarget() => cameraTarget;
    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _state;
    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
            motor.BaseVelocity = Vector3.zero;
    }
}