using UnityEngine;
using System.Collections;

public class Action09_WallRun : MonoBehaviour {

    /*
    public ActionManager Action;
    public Animator CharacterAnimator;
    PlayerBhysics Player;

    public bool isAdditive;
    public float RunTimerLimit;
    public float FacingAmount;
	public GameObject JumpDashParticle;
    float Timer;
    float Speed;
    float Aspeed;
    Vector3 direction;


    public Transform Target { get; set; }
    public float skinRotationSpeed;
    public bool WallJumpable { get; set; }
    public bool IsAirDash { get; set; }
    */

    [Header("Wallruning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float hInput;
    private float vInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerBhysics Player;
    //private PlayerPhysics PlayerPhys;
    private Rigidbody rb;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        rb = Player.p_rigidbody.GetComponent<Rigidbody>();
        Player = GetComponent<PlayerBhysics>();
        //PlayerPhys = GetComponent<PlayerPhysics>();
    }

    private void Update()
    {
        checkForWall();
        stateMachine();
    }

    private void FixedUpdate()
    {
        if (Player.WallRunning) { wallRuningMovement(); }
    }

    private void checkForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void stateMachine()
    {
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        // State 1 - Wall Running
        if ((wallLeft || wallRight) && vInput > 0 && AboveGround())
        {
            if (!Player.WallRunning) { startWallRun(); }
        }

        // State 3 - None
        else
        {
            if (Player.WallRunning) { stopWallRun(); }
        }
    }

    private void startWallRun()
    {
        Player.WallRunning = true;
    }

    private void wallRuningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // Allow back and forth wall running.
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //Pushes you to the wall if you're not trying to get away.
        if (!(wallLeft && hInput > 0) && !(wallRight && hInput < 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
    }

    private void stopWallRun()
    {
        Player.WallRunning = false;
    }

    /*
    void Awake()
    {
        WallJumpable = true;
        Player = GetComponent<PlayerBhysics>();
    }

    public void InitialEvents()
    {

		Action.Action01.JumpBall.SetActive(false);

		Player.speed

        if (Action.Action09Control.HasTarget)
        {
            Target = HomingAttackControl.TargetObject.transform;
        }

        Timer = 0;
        WallJumpable = false;

        if (isAdditive)
        {
			
            // Apply Max Speed Limit
            float XZmag = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z).magnitude;

            if (XZmag < HomingAttackSpeed)
            {
                Speed = HomingAttackSpeed;
            }
            else
            {
                Speed = XZmag;
            }

            if(XZmag < AirDashSpeed)
            {
                Aspeed = AirDashSpeed;
            }
            else
            {
                Aspeed = XZmag;
            }
        }
        else
        {
            Aspeed = AirDashSpeed;
            Speed = HomingAttackSpeed;
        }

        //Check if not facing Object
        if (!IsAirDash)
        {
            Vector3 TgyXY = HomingAttackControl.TargetObject.transform.position.normalized;
            TgyXY.y = 0;
            float facingAmmount = Vector3.Dot(Player.PreviousRawInput.normalized, TgyXY);
           // //Debug.Log(facingAmmount);
           // if (facingAmmount < FacingAmount) { IsAirDash = true; }
        }

    }

    void Update()
    {

        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 1);
        CharacterAnimator.SetFloat("YSpeed", Player.p_rigidbody.velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.p_rigidbody.velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.p_rigidbody.velocity.x, 0, Player.p_rigidbody.velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);
    }

    private void OnTriggerEnter()
    {

    }

    void FixedUpdate()
    {
        Timer += 1;

        CharacterAnimator.SetInteger("Action", 1);

        if (IsAirDash)
        {
            if (Player.RawInput != Vector3.zero)
            {
                Player.p_rigidbody.velocity = transform.TransformDirection(Player.RawInput) * Aspeed;
            }
            else
            {
               // //Debug.Log("prev");
                Player.p_rigidbody.velocity = transform.TransformDirection(Player.PreviousRawInput) * Aspeed;
            }
            Timer = RunTimerLimit + 10;
        }
        else
        {
            direction = Target.position - transform.position;
            Player.p_rigidbody.velocity = direction.normalized * Speed;
        }

		//Set Player location when close enough, for precision.
		if (Target != null && Vector3.Distance (Target.position, transform.position) < 5) 
		{
			transform.position = Target.position;
			//Debug.Log ("CloseEnough");
		}

        //End homing attck if on air for too long
        if (Timer > RunTimerLimit)
        {
            Action.ChangeAction(0);
        }
    }
    
    public void ResetHomingVariables()
    {
        Timer = 0;
		HomingTrailContainer.transform.DetachChildren ();
        IsAirDash = false;
    }

*/

}
