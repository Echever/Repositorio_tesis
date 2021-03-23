using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableTouchDetector : MonoBehaviour
{
    public string touchTableTag;
    public bool hasTouchedTable = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == touchTableTag)
        {
            hasTouchedTable = true;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.gameObject.tag == touchTableTag)
        {
            hasTouchedTable = false;
        }

    }
}
