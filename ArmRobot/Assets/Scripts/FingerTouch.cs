using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTouch : MonoBehaviour
{

    public bool isTouchingCube = false;
    public string cube = "Cube";
    public string finger;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == cube)
        {
            //Debug.Log("Finger " + finger + " is touching the cube");
            isTouchingCube = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.gameObject.tag == cube)
        {
            //Debug.Log("Finger " + finger + " is not touching the cube");
            isTouchingCube = false;
        }

    }

    public bool isTouching()
    {
        return isTouchingCube;
    }

    public void resetIstouching()
    {
        isTouchingCube = false;
    }
}
