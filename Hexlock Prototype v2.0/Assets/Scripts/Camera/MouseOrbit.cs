using UnityEngine;
using System.Collections;

public class MouseOrbit : MonoBehaviour {

    public Transform target; //The target we are following

    public float distance = 10.0f; // The distance in the x-z plane to the target.
    public float xSpeed = 250.0f; //The orbit speed in the X-axis.
    public float ySpeed = 120.0f; //The orbit speed in the Y-axis.
    public float yMinLimit = -20; //The minimum rotation limit in the Y-axis.
    public float yMaxLimit = 80; //The maximum rotation limit in the Y-axis.

    private float x; //Used to get input from the mouse in the X-axis
    private float y; //Used to get input from the mouse in the Y-axis

    void Start ()
    {
        Vector3 angles = transform.eulerAngles;
    
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate ()
    {
        if (target) 
        {
            //Get the input from the mouse and clamp the Y-axis to set angles.
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            //Rotate the player according to mouse input.
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

            position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    //Used for clamping the camera rotation with the player.
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;

        if (angle > 360)
            angle -= 360;
	
        return Mathf.Clamp (angle, min, max);
    }
}