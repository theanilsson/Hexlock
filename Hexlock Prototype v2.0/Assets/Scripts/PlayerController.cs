using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Network related variables
    private float xAxis, yAxis, lastSynchronizationTime, syncDelay, syncTime;
    private Vector3 syncStartPosition, syncEndPosition;
    private NetworkView network;

    // Player GUI related variables
    private int health = 100;
    private float speed = 3f;
    private int lavaDamage = 5;
    [HideInInspector]
    public int lives = 5;
    private Text healthText;
    private Text livesText;
    private Text respawnTimerText;
    private Image killscreen;
    private float respawnTimer = 3f;
    private bool startRespawnTimer = false;
    private GameObject continueMenu;

    // Timer related variables
    private float lavaTimer = 1f;
    private float energyBallCooldown = 0f;
    private float railgunCooldown = 0f;
    private float teleportCooldown = 0f;
    private float gotHitCounter;
    private bool startEnergyballCooldown = false;
    private bool startRailgunCooldown = false;
    private bool startTeleportCooldown = false;

    [HideInInspector]
    public Vector3 lastHitDir;
    [HideInInspector]
    public float lastSpellForce;

    // Movement related variables
    private Vector3 respawnPoint = new Vector3(0f, 5f, 0f);
    private Rigidbody rBody;
    private Animator anim;
    private bool canMove = true;

    // Jump-raycast related variables
    private RaycastHit hit;
    private float groundDist = 0.1f;

    // Spell GUI related variables
    public GameObject[] spells;
    public ParticleSystem bloodSplatter;
    public ParticleSystem energyBallEffect;
    public Image[] cooldownBoxes;
    public float jumpForce = 0.05f;
    private int currentSpell = 0;
    public Image[] bars;
    private Image currentBar;

    // Camera-control related variables
    public GameObject camera;
    public GameObject camerapoint;
    MouseOrbit orbit;
    SmoothFollow sFollow;
    CameraTurn cTurn;

    // Sound related variables
    public AudioClip[] audioClips;
    private AudioSource sorce;

    private GameManager gameManager;

    Renderer[] renderers;

    public ParticleSystem lavaSplash;
    private ParticleSystem particleHolder;

    private void Awake()
    {
        
    }

    void Start()
    {
        // Gets all the nessecary game objects and components and puts them into the proper variables
        camera = GameObject.Find("Main Camera");
        healthText = GameObject.Find("Health").GetComponent<Text>();
        livesText = GameObject.Find("Lives").GetComponent<Text>();
        killscreen = GameObject.Find("Killscreen").GetComponent<Image>();
        continueMenu = GameObject.Find("Continue Menu");
        respawnTimerText = GameObject.Find("Respawn Timer").GetComponent<Text>();
        cTurn = GetComponent<CameraTurn>();
        rBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        network = GetComponent<NetworkView>();
        sFollow = camera.GetComponent<SmoothFollow>();
        orbit = camera.GetComponent<MouseOrbit>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        renderers = GetComponentsInChildren<Renderer>();
        sorce = GetComponent<AudioSource>();

        bars[0] = GameObject.Find("EnergyBallBar").GetComponent<Image>();
        bars[1] = GameObject.Find("RailBeamBar").GetComponent<Image>();
        bars[2] = GameObject.Find("TeleportBar").GetComponent<Image>();

        bars[1].enabled = false;
        bars[2].enabled = false;
        currentBar = bars[0];

        // Sets the cursor to not be visible
        Cursor.visible = false;
        killscreen.enabled = false;
        respawnTimerText.enabled = false;
        continueMenu.SetActive(false);
        cooldownBoxes[0] = GameObject.Find("EnergyBallCD").GetComponent<Image>();
        cooldownBoxes[1] = GameObject.Find("RailBeamCD").GetComponent<Image>();
        cooldownBoxes[2] = GameObject.Find("TeleportCD").GetComponent<Image>();

        // Sets the camera variables targets
        if (network.isMine)
        {
            cTurn.target = camera.transform;
            orbit.target = camerapoint.transform;
            sFollow.target = camerapoint.transform;
        }
    }

    void Update()
    {
        //Always keeps the player informed what spell is selected in the GUI and changes between what spell is going to be instantiated from an array.
        //The string references are pre-set in the InputManager.
        if (network.isMine)
        {
            if (Input.GetButtonDown("EnergyballButton"))
            {
                bars[currentSpell].enabled = false;
                currentSpell = 0;
                bars[currentSpell].enabled = true;
            }
            else if (Input.GetButtonDown("RailgunButton"))
            {
                bars[currentSpell].enabled = false;
                currentSpell = 1;
                bars[currentSpell].enabled = true;
            }
            else if (Input.GetButtonDown("TeleportButton"))
            {
                bars[currentSpell].enabled = false;
                currentSpell = 2;
                bars[currentSpell].enabled = true;
            }

            // Gets the x and y axises into variables for moving the player relative to the camera
            xAxis = Input.GetAxis("Horizontal");
            yAxis = Input.GetAxis("Vertical");

            //Store down the axis value for movement animations and call them with an RPC-function to animate properly over the network.
            float animMoveX = xAxis;
            float animMoveY = yAxis;
            network.RPC("SetAnimFloat", RPCMode.All, "animMoveX", animMoveX);
            network.RPC("SetAnimFloat", RPCMode.All, "animMoveY", animMoveY);

            // Allows the player to respawn if his health reaches zero or less
            if (startRespawnTimer)
            {
                respawnTimer -= Time.deltaTime;
            }
            // Respawns the player if he has any lives left. Also sets the playerobject mesh inactive for everyone on the network.
            //Also lets the player know that he has died with GUI feedback.
            if (Health <= 0 && lives >= 1)
            {
                Health = 0;
                startRespawnTimer = true;

                network.RPC("SetMeshActive", RPCMode.All, false);

                spells[currentSpell].SetActive(false);
                killscreen.enabled = true;
                respawnTimerText.enabled = true;
            }
            //Spawns the player and sets variables to their original values. 
            if (respawnTimer <= 0)
            {
                transform.position = gameManager.spawnPoints[Random.Range(0, 5)];
                Health = 100;
                lives--;
                gameObject.SetActive(true);
                startRespawnTimer = false;
                respawnTimer = 5f;
                network.RPC("SetMeshActive", RPCMode.All, true);
                spells[currentSpell].SetActive(true);
                respawnTimerText.enabled = false;
                killscreen.enabled = false;
            }
            //If you run out of lives, the playerobject mesh is inactive and you update the GameManager with the amount of killed players.
            if (lives == 0)
            {

                network.RPC("SetMeshActive", RPCMode.All, false);
                network.RPC("PlayerKilled", RPCMode.All);
                Debug.Log(GameManager.killedPlayers);
                gameObject.SetActive(false);
                Cursor.visible = true;

            }
            // Gives the host a replay option if all but one player is dead
            if (GameManager.killedPlayers == Network.connections.Length && Time.timeSinceLevelLoad >= 10f && Network.isServer)
            {
                continueMenu.SetActive(true);
                Cursor.visible = true;
            }

            // Calls for the function handling all the movement
            if (!startRespawnTimer)
            {
                UpdateMovement();
            }

            // Calls for the function handling the spell cooldowns
                UpdateSpellCooldowns();

            int drawEnergyBall = (int)energyBallCooldown;
            int drawRailBeam = (int)railgunCooldown;
            int drawTeleport = (int)teleportCooldown;
            int drawRespawnTimer = (int)respawnTimer;

            // Sets the health guitext to the current health of the player
            healthText.text = Health.ToString();
            livesText.text = lives.ToString();
            respawnTimerText.text = "Respawn in: " + (drawRespawnTimer.ToString());
        }

        // Counts down the timer for taking damage in lava
        lavaTimer -= Time.deltaTime;
    }

    // Will make movment more fluid across the network
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;

        if (stream.isWriting)
        {
            syncPosition = GetComponent<Rigidbody>().position;
            stream.Serialize(ref syncPosition);

            syncVelocity = GetComponent<Rigidbody>().velocity;
            stream.Serialize(ref syncVelocity);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = GetComponent<Rigidbody>().position;
        }
    }

    // Allows the current value of the speed variable to be gotten or set to another value
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    // Allows the current value of the speed variable to be gotten or set to another value
    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    // Manages all the cooldowns for spells so that you can't spam spells
    private void UpdateSpellCooldowns()
    {
        // Displays the cooldown of the spells
        cooldownBoxes[0].fillAmount = energyBallCooldown / 2f;
        cooldownBoxes[1].fillAmount = railgunCooldown / 5f;
        cooldownBoxes[2].fillAmount = teleportCooldown / 8f;        // The variables 2 5 and 8 are the is equal to the spells max cooldown

        // Checks so that a new spell can't be fired until the cooldown has passed by.
        if (startEnergyballCooldown)
        {
            energyBallCooldown -= Time.deltaTime;
        }
        if (startRailgunCooldown)
        {
            railgunCooldown -= Time.deltaTime;
        }
        if (startTeleportCooldown)
        {
            teleportCooldown -= Time.deltaTime;
        }
        if (energyBallCooldown <= 0)
        {
            startEnergyballCooldown = false;
        }
        if (railgunCooldown <= 0)
        {
            startRailgunCooldown = false;
        }
        if (teleportCooldown <= 0)
        {
            startTeleportCooldown = false;
        }

        //Plays the "Casting Spell" animation, instantiates the EnergyBall object over the network, starts the cooldown and plays the appropriate sound effect.
        if (energyBallCooldown <= 0 && currentSpell == 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                network.RPC("SetAnimTrigger", RPCMode.All, "Casting Spell");
                Vector3 spellSpawnPos = transform.position + transform.forward * 2f;
                spellSpawnPos.y += 1.2f;
                Network.Instantiate(spells[currentSpell], spellSpawnPos, transform.rotation, 1);
                network.RPC("PlayEnergyBall", RPCMode.All);
                energyBallCooldown = 2f;
                startEnergyballCooldown = true;
                network.RPC("PlaySound", RPCMode.All, 0);
            }
        }
        //Plays the "Casting Spell" animation, instantiates the Railgun object over the network, starts the cooldown and plays the appropriate sound effect.
        if (railgunCooldown <= 0 && currentSpell == 1)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                network.RPC("SetAnimTrigger", RPCMode.All, "Casting Spell");
                Vector3 spellSpawnPos = transform.position + transform.forward * 2f;
                spellSpawnPos.y += 1.2f;
                Network.Instantiate(spells[currentSpell], spellSpawnPos, transform.rotation, 1);
                railgunCooldown = 5f;
                startRailgunCooldown = true;
                network.RPC("PlaySound", RPCMode.All, 3);
            }
        }
        //Plays the "Casting Spell" animation, instantiates the Teleport object over the network, starts the cooldown and plays the appropriate sound effect.
        if (teleportCooldown <= 0 && currentSpell == 2)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                network.RPC("SetAnimTrigger", RPCMode.All, "Casting Spell");
                Vector3 spellSpawnPos = transform.position + transform.forward * 2f;
                spellSpawnPos.y += 1.2f;
                Network.Instantiate(spells[currentSpell], spellSpawnPos, transform.rotation, 1);
                teleportCooldown = 8f;
                startTeleportCooldown = true;
                network.RPC("PlaySound", RPCMode.All, 0);
            }
        }
    }
    // Manages all the movement of the player character
    private void UpdateMovement()
    {
        // Allows the character to jump if the jump inputbutton is pressed and the character isn't airborne
        if (Input.GetButtonDown("Jump"))
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hit, groundDist))
            {
                network.RPC("SetAnimTrigger", RPCMode.All, "Jump");
                rBody.velocity += new Vector3(0f, jumpForce, 0f);
            }
        }

        // Moves the player relative to the camera
        if (canMove)
        {
            Vector3 t = (camera.transform.forward * yAxis + camera.transform.right * xAxis) /** Time.deltaTime*/;
            Vector3 desiredMove = Vector3.ProjectOnPlane(t, Vector3.down);
            desiredMove.Normalize();
            rBody.velocity += (new Vector3(desiredMove.x * speed, 0f, desiredMove.z * speed) * Time.deltaTime * 60f);
        }

        // Moves the player down while he is airborne, like gravity
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, groundDist))
            rBody.velocity -= new Vector3(0f, 2f, 0f);
        // Allows the player to respawn if his health reaches zero or less
        if (network.isMine)
        {
            if (Health <= 0 && lives >=1)
            {
                Health = 0;
                startRespawnTimer = true;
                if (respawnTimer <= 0)
                {
                    transform.position = gameManager.spawnPoints[Random.Range(0, 5)];
                    Health = 100;
                    lives--;
                    gameObject.SetActive(true);
                    startRespawnTimer = false;
                    respawnTimer = 5f;
                }
            }
        }
    }

    // Pushes the player when hit
    private void GotHit()
    {
        // Pushes the player in the direction of the spell, makes him/her slow down after a while and does this relative to the current health
        // Less health = more push
        network.RPC("SetAnimTrigger", RPCMode.All, "GotHit");
        gotHitCounter++;
        rBody.velocity +=(lastHitDir * Mathf.Clamp((30f/gotHitCounter)/(Health/60f),0f,20f));
        Debug.Log(rBody.velocity);
        if (gotHitCounter > 100)
        {
            CancelInvoke("GotHit");
        }
    }

    // Plays the blood splatter on-hit animation
    [RPC]
    private void PlayBlood()
    {
        bloodSplatter.Play();
    }
    //Plays the Energyball animation.
    [RPC]
    private void PlayEnergyBall()
    {
        energyBallEffect.Play();
    }
    //Increases the amount of killed players for all players on the network.
    [RPC]
    private void PlayerKilled()
    {
        GameManager.killedPlayers++;
    }
    //Sets a bool value to the current AnimationState paramater.
    [RPC]
    private void SetAnimBool(string currentState, bool value)
    {
        anim.SetBool(currentState, value);
    }
    //Sets a float value to the current AnimationState parameter.
    [RPC]
    private void SetAnimFloat(string currentState, float value)
    {
        anim.SetFloat(currentState, value);
    }
    //Activates a trigger value to the current AnimationState parameter.
    [RPC]
    private void SetAnimTrigger(string currentState)
    {
        anim.SetTrigger(currentState);
    }
    //Plays a sound over the Network.
    [RPC]
    private void PlaySound(int soundNr)
    {
        sorce.PlayOneShot(audioClips[soundNr], Random.Range(1.5f,2f));
    }
    [RPC]
    //Sets a bool value to enable or disable the playerobject mesh.
    private void SetMeshActive(bool value)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = value;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Makes the character unable to stick to the walls
        if (collision.gameObject.tag == "Wall")
        {
            Health -= 100;
        }
    }

    // Manages the collision of any colliders entering the players collider
    void OnTriggerStay(Collider collider)
    {
        if (network.isMine)
        {
            // Damages the player if it stands in lava, plays the appropriate particle effect and sound.
            if (collider.gameObject.tag == "Lava")
            {
                if (lavaTimer <= 0)
                {
                    Health -= lavaDamage;
                    lavaTimer = 1;

                    particleHolder = Network.Instantiate(lavaSplash, transform.position, new Quaternion(-90f,0f,0f,0f), 1) as ParticleSystem;
                    particleHolder.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
                    Destroy(particleHolder.gameObject, 1f);
                    network.RPC("PlaySound", RPCMode.All, 2);
                }
            }
            // Makes the character unable to stick to the walls
            else if (collider.gameObject.tag == "Wall")
            {
                canMove = false;
            }

            // Damages and pushes the player back if he is hit by an energyball
            if (collider.gameObject.tag == "Energyball")
            {
                // Pushes you in the direction of the spell that hits you, it also stacks
                lastHitDir = collider.transform.forward;
                gotHitCounter = 0;
                InvokeRepeating("GotHit", 0f, 0.01f);

                Health -= collider.gameObject.GetComponent<EnergyBall>().damage;

                network.RPC("PlaySound", RPCMode.All, 1);

                network.RPC("PlayBlood", RPCMode.AllBuffered);

                if (network.isMine)
                {
                    Network.Destroy(collider.gameObject);
                }
            }
        }
    }
}
