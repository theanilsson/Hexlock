using UnityEngine;
using System.Collections;

public class BaseSpell : MonoBehaviour 
{
    // Variables
    public Vector3 direction;
    public Vector3 position;
    protected float speed = 25f;
    public int damage;
    public float force;
    PlayerController controller;
    protected NetworkView network;
    private Rigidbody rBody;
    protected GameObject camera;
    public GameObject player;

	protected void Start () 
    {
        // Gets all the nessecary game objects and components and puts them into the proper variables
        network = GetComponent<NetworkView>();
        rBody = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        player = GameObject.FindGameObjectWithTag("Player");

        // The direction is set to youre cameras dir
        if (network.isMine)
        {
            Vector3 desiredMove = camera.transform.forward;
            desiredMove.y += 0.25f;
            network.RPC("setDir", RPCMode.AllBuffered, desiredMove);
        }

        // Shoots the spell in the direction the player declared when creating it
        rBody.AddForce(direction * speed, ForceMode.Impulse);
        
	}
	
	protected void Update () 
    {
        // This is what all users will change after
        if (network.isMine)
        {
            position = transform.position;
        }
	}

    // Handles all collision related functionality and can be overwritten by child classes using different CollisionControllers
    protected virtual void CollisionController(Collider collider, int damage, float force)
    {
        // Damages a player if the spell hits a player
        if (collider.gameObject.tag == "Player")
        {
            collider.GetComponent<PlayerController>().Health -= damage;
        }
        // Destroys the spell
        else if (network.isMine)
        {
            Network.Destroy(gameObject);
        }
    }

    // The current player sets the direction of a spell he casts and sends that information to all other players
    [RPC]
    protected void setDir(Vector3 dir)
    {
        direction = dir;
    }
}
