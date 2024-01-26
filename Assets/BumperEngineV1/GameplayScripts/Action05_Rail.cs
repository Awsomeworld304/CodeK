using SplineMesh;
using UnityEngine;

public class Action05_Rail : MonoBehaviour
{

    PlayerBhysics Player;
    PlayerBinput BInput;
    public Animator CharacterAnimator;
    public SonicSoundsControl Sound;
    Quaternion CharRot;
    Rail_Interaction Rail_int;
    ActionManager Actions;
    CameraControl Cam;

    [Header("Skin Params")]

    public GameObject Skin;
    public float skinRotationSpeed = 50f;
    public Vector3 SkinOffsetPos = new Vector3(0, -0.4f, 0);
    Vector3 OGSkinLocPos;
    public float Offset = 2.05f;

    public float CameraSpeed = 2f;




    [Header("Action Values")]

    public float MinStartSpeed = 60f;
    public float PushFowardmaxSpeed = 80f;
    public float PushFowardIncrements = 15f;
    public float PushFowardDelay = 0.5f;
    public float SlopePower = 2.5f;
    public float UpHillMultiplier = 0.25f;
    public float DownHillMultiplier = 0.35f;
    public float UpHillMultiplierCrouching = 0.4f;
    public float DownHillMultiplierCrouching = 0.6f;
    public float DragVal = 0.0001f;
    public float PlayerBrakePower = 0.95f;

    float curvePosSlope { get; set; }

    // Setting up Values
    float timer = 0f;
    private float range = 0f;
    Transform RailTransform;
    public bool OnRail = false;
    float PlayerSpeed;
    bool backwards;
    int RailSound = 1;
    bool RailContactSound;
    bool isBraking;
    CurveSample sample;
    bool Crouching;
    [SerializeField]
    //float rotYFix;
    //Quaternion rot;
    Quaternion InitialRot;

    private void Awake()
    {
        Actions = GetComponent<ActionManager>();
        //Input = GetComponent<PlayerBinput>();
        Rail_int = GetComponent<Rail_Interaction>();
        Player = GetComponent<PlayerBhysics>();
        RailContactSound = false;
        OGSkinLocPos = Skin.transform.localPosition;
        Cam = GetComponent<CameraControl>();
    }

    public void InitialEvents(float Range, Transform RailPos)
    {
        // Kill Jump Ball
        Actions.Action01.JumpBall.SetActive(false);

        //Animations and Skin Changes
        Skin.transform.localPosition = Skin.transform.localPosition + SkinOffsetPos;

        //Setting up Rails
        range = Range;
        RailTransform = RailPos;
        OnRail = true;
        PlayerSpeed = Player.GetComponent<Rigidbody>().velocity.magnitude;
        CurveSample sample = Rail_int.RailSpline.GetSampleAtDistance(range);
        float dotdir = Vector3.Dot(Player.GetComponent<Rigidbody>().velocity.normalized, sample.tangent);
        Crouching = false;

        InitialRot = (Quaternion.FromToRotation(transform.up, sample.up) * transform.rotation);


        // make sure that Player wont have a shitty Speed...
        if (dotdir > 0.85f || dotdir < -.85f)
        {
            PlayerSpeed = Mathf.Abs(PlayerSpeed * 1);
        }
        else if (dotdir < 0.5 && dotdir > -.5f)
        {
            PlayerSpeed = Mathf.Abs(PlayerSpeed * 0.5f);
        }
        PlayerSpeed = Mathf.Max(PlayerSpeed, MinStartSpeed);

        // Get Direction for the Rail
        if (dotdir > 0)
        {
            backwards = false;
        }
        else
        {
            backwards = true;
        }
    }

    public void RailGrind()
    {

        //Player Rail Sound
        //Base Sound value
        RailSound = 1;
        //Second Stage
        if (PlayerSpeed > 100)
        {
            RailSound = 2;
        }
        //ThirdStage
        if (PlayerSpeed > 150)
        {
            RailSound = 3;
        }
        //If Entring Rail
        if (!RailContactSound)
        {
            RailSound = 0;
            RailContactSound = true;
        }



        //Increase the Amount of distance trought the Spline by DeltaTime
        float ammount = (Time.deltaTime * PlayerSpeed);

        //Check for Low Speed to change direction so player dont get stuck
        if (PlayerSpeed < 10)
        {
            if (!backwards)
            {
                backwards = true;
                PlayerSpeed = 12;
                ammount = (Time.deltaTime * PlayerSpeed);
            }
            else
            {
                backwards = false;
                PlayerSpeed = 12;
                ammount = (Time.deltaTime * PlayerSpeed);

            }

        }

        // Increase/Decrease Range depending on direction

        if (!backwards)
        {
            //range += ammount / dist;
            range += ammount;
        }
        else
        {
            //range -= ammount / dist;
            range -= ammount;
        }

        //Check so for the size of the Spline
        if (range < Rail_int.RailSpline.Length && range > 0)
        {
            //Get Sample of the Rail to put player
            sample = Rail_int.RailSpline.GetSampleAtDistance(range);

            //Set player Position and rotation on Rail
            transform.rotation = (Quaternion.FromToRotation(transform.up, sample.up) * transform.rotation);
            transform.position = sample.location + RailTransform.position + (transform.up * Offset);

            //Add Physics
            SlopePhys();

            if (isBraking && PlayerSpeed > MinStartSpeed) PlayerSpeed *= PlayerBrakePower;

            //Debug.DrawRay(transform.position, sample.tangent * 10f,Color.black);

            //Set Player Speed correctly so that it becomes smooth grinding
            if (!backwards)
            {
                Player.GetComponent<Rigidbody>().velocity = sample.tangent * (PlayerSpeed);

                //remove camera tracking at the end of the rail to be safe from strange turns
                //if (range > Rail_int.RailSpline.Length * 0.9f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f;}
            }
            else
            {
                Player.GetComponent<Rigidbody>().velocity = -sample.tangent * (PlayerSpeed);

                //remove camera tracking at the end of the rail to be safe from strange turns
                //if (range < 0.1f) { Player.MainCamera.GetComponent<HedgeCamera>().Timer = 0f; }
            }

        }
        else
        {

            //Check if the Spline is loop and resets position
            if (Rail_int.RailSpline.IsLoop)
            {
                if (!backwards)
                {
                    range = range - Rail_int.RailSpline.Length;
                    RailGrind();
                }
                else
                {
                    range = range + Rail_int.RailSpline.Length;
                    RailGrind();
                }
            }
            else
            {

                OnRail = false;

            }
        }

    }

    void SlopePhys()
    {

        //slope curve from Bhys
        curvePosSlope = Player.curvePosSlope;
        //float v = Input.InputExporter.y;
        //v = (v + 1) / 2;
        //use player vertical speed to find if player is going up or down
        //Debug.Log(Player.p_rigidbody.velocity.normalized.y);
        if (Player.GetComponent<Rigidbody>().velocity.y >= -3f)
        {
            //uphill and straight
            float lean = UpHillMultiplier;
            if (Crouching) { lean = UpHillMultiplierCrouching; }
            //Debug.Log("UpHill : *" + lean);
            float force = (SlopePower * curvePosSlope) * lean;
            //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y - 1));
            float AbsYPow = Mathf.Abs(Player.GetComponent<Rigidbody>().velocity.normalized.y * Player.GetComponent<Rigidbody>().velocity.normalized.y);
            //Debug.Log( "Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
            force = (AbsYPow * force) + (DragVal * PlayerSpeed);
            //Debug.Log(force);
            PlayerSpeed -= force;

            //Enforce max Speed
            if (PlayerSpeed > Player.MaxSpeed)
            {
                PlayerSpeed = Player.MaxSpeed;
            }
        }
        else
        {
            //Downhill
            float lean = DownHillMultiplier;
            if (Crouching) { lean = DownHillMultiplierCrouching; }
            //Debug.Log("DownHill : *" + lean);
            float force = (SlopePower * curvePosSlope) * lean;
            //Debug.Log(Mathf.Abs(Player.p_rigidbody.velocity.normalized.y));
            float AbsYPow = Mathf.Abs(Player.GetComponent<Rigidbody>().velocity.normalized.y * Player.GetComponent<Rigidbody>().velocity.normalized.y);
            //Debug.Log("Val" + Player.p_rigidbody.velocity.normalized.y + "Pow" + AbsYPow);
            force = (AbsYPow * force) - (DragVal * PlayerSpeed);
            //Debug.Log(force);
            PlayerSpeed += force;

            //Enforce max Speed
            if (PlayerSpeed > Player.MaxSpeed)
            {
                PlayerSpeed = Player.MaxSpeed;
            }
        }

    }


    void FixedUpdate()
    {
        if (OnRail)
        {

            RailGrind();

        }
        else
        {

            Player.transform.rotation = InitialRot;

            //Change Into Action 0
            CharacterAnimator.SetInteger("Action", 0);
            CharacterAnimator.SetBool("Grounded", Player.Grounded);
            Actions.ChangeAction(0);
            if (Actions.Action02 != null)
            {
                Actions.Action02.HomingAvailable = true;
            }
        }

    }


    private void Update()
    {
        //Set Animator Parameters
        CharacterAnimator.SetInteger("Action", 5);
        CharacterAnimator.SetFloat("YSpeed", Player.GetComponent<Rigidbody>().velocity.y);
        CharacterAnimator.SetFloat("GroundSpeed", Player.GetComponent<Rigidbody>().velocity.magnitude);
        CharacterAnimator.SetBool("Grounded", Player.Grounded);

        //Set Animation Angle
        Vector3 VelocityMod = new Vector3(Player.GetComponent<Rigidbody>().velocity.x, Player.GetComponent<Rigidbody>().velocity.y, Player.GetComponent<Rigidbody>().velocity.z);
        Quaternion CharRot = Quaternion.LookRotation(VelocityMod, transform.up);
        CharacterAnimator.transform.rotation = Quaternion.Lerp(CharacterAnimator.transform.rotation, CharRot, Time.deltaTime * skinRotationSpeed);

        Cam.Cam.FollowDirection(CameraSpeed, 5f, -10, 12);

        // Actions Goes Here

        if (Input.GetButtonDown("A"))
        {
            Player.GroundNormal = sample.up;
            Actions.Action01.InitialEvents();
            OnRail = false;
            Player.transform.rotation = InitialRot;
            //Player.transform.eulerAngles = new Vector3(0,1,0);
            Actions.ChangeAction(1);
            if (Actions.Action02 != null)
            {
                Actions.Action02.HomingAvailable = true;
            }

        }
        /*
        if (Input.GetButton("R1-Roll"))
        {
            //Crouch
            Crouching = true;
        }
        else
        {
            Crouching = false;
        }

        if (Input.GetButtonDown("B-Bounce"))
        {
            //ChangeSide
            if (timer > PushFowardDelay)
            {
                if (PlayerSpeed < PushFowardmaxSpeed)
                {
                    PlayerSpeed += PushFowardIncrements;
                }
                timer = 0f;
            }
        }

        if (Input.GetButtonDown("X-Stomp"))
        {
            isBraking = true;
        }
        else
        {
            isBraking = false;
        }
        */
    }
}


