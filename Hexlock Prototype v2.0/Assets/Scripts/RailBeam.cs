using UnityEngine;
using System.Collections;

public class RailBeam : BaseSpell 
{
    private RaycastHit ray;
    public ParticleSystem rayParticle;
    private ParticleSystem instantHolder;
    private int particleSpace;
    private NetworkView network;
    private PlayerController playerScript;

	// Use this for initialization
	void Start () 
    {
        force = 30;
        particleSpace = 3;
        damage = 20;
        network = GetComponent<NetworkView>();
        camera = GameObject.FindGameObjectWithTag("MainCamera");

        // The direction is set to youre cameras dir
        if (network.isMine)
        {
            Vector3 desiredMove = camera.transform.forward;
            desiredMove.y += 0.15f;
            network.RPC("setDir", RPCMode.AllBuffered, desiredMove);
        }

        Physics.Raycast(transform.position, direction, out ray, 200f);

        float distance = Vector3.Distance(position, ray.point);

        // Creates a particleSystem along the raycast with the space of particleSpace. also makes them a child of the spell preefab
        for (int i = 0; i * particleSpace < distance; i++)
        {
            
            instantHolder = ParticleSystem.Instantiate(rayParticle, transform.position + (direction * i * particleSpace), Quaternion.LookRotation(direction)) as ParticleSystem;
            instantHolder.transform.SetParent(transform);
            instantHolder.Play();
        }

        if (ray.collider.tag == "Player")
        {
            player = ray.collider.gameObject;
            playerScript = player.GetComponent<PlayerController>();
            player.GetComponent<Animator>().SetTrigger("GotHit");
            playerScript.lastSpellForce = force;
            playerScript.lastHitDir = transform.forward;
            playerScript.Health -= damage;
            playerScript.InvokeRepeating("GotHit", 0f, 0.01f);
        }
        Destroy(this.gameObject, 1f);
	}
	
	// Update is called once per frame
	void Update () 
    {
        
	}
}
