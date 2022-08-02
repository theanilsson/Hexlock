using UnityEngine;
using System.Collections;

public class EnergyBall : BaseSpell 
{
    void Start()
    {
        // Calls for the superclass start function
        base.Start();

        // Sets the damage and force for this spell, originating from the superclass
        damage = 10;
        force = 15f;
    }

    // Calls for the superclass' collision controller when the energyball collides with a gameobject
    void OnTriggerEnter(Collider collider)
    {
        base.CollisionController(collider,damage,force);
    }
}
