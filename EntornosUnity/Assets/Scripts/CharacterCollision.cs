using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollision : MonoBehaviour
{
    
    private void OnCollisionEnter(Collision other) {
        Debug.Log(other.gameObject.name);
        if(other.gameObject.tag == "Waste")
        {
            
            int spawnPositionX = Random.Range(-6,2);
            int spawnPositionZ = Random.Range(9,15);
            Vector3 spawnPosition = new Vector3(spawnPositionX, 0.15f, spawnPositionZ);
            other.gameObject.transform.position = spawnPosition;
            
        }
    }
}
