
using UnityEngine;
using System.Collections;

public class CameraTurn : MonoBehaviour
{
   [HideInInspector]
   public Transform target; //The target transformation to rotate around.

   public float rotSpeed = 5; //The speed to rotate around the target transform

    void Update()
    {
        Transform targetPos = target.transform;

        //set targetPos y equal to mine, so I only look at my own plane
        targetPos.transform.position = new Vector3(targetPos.position.x, transform.position.y, targetPos.position.z);

        //Alignes the targets direction.
        Quaternion targetDir = Quaternion.LookRotation(-(targetPos.position - transform.position));
        //Interpolates for smoother rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetDir, rotSpeed * Time.deltaTime);
    }
}