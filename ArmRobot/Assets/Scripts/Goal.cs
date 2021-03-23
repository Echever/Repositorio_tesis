using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool goal = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "Cube")
        {
            goal = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.gameObject.tag == "Cube")
        {
            goal = false;
        }

    }
}
