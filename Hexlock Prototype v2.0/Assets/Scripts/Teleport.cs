using UnityEngine;
using System.Collections;

public class Teleport : BaseSpell 
{

	// Use this for initialization
	public void Start () 
    {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () 
    {
        base.Update();
	}

    void OnTriggerEnter(Collider collider)
    {
        CollisionController(collider, 0, 0f);
    }

    protected override void CollisionController(Collider collider, int damage, float force)
    {
        if (collider.gameObject.tag == "Wall")
        {
            Network.Destroy(gameObject);
        }
        else
        {
            TeleportPlayer();
            Network.Destroy(gameObject);
        }
    }

    public void TeleportPlayer()
    {
        if (network.isMine)
        {
            player.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
    }
}
