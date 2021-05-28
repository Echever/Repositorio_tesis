using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public InventoryObject inventory;
    
    public void OnCollisionEnter(Collision other)
    {
        
        if (other.gameObject.tag == "Waste")
        {
            Debug.Log("Waste");
            var item = other.collider.GetComponent<Item>();

            int spawnPositionX = Random.Range(-5, 5);
            int spawnPositionZ = Random.Range(-5, 5);
            Vector3 spawnPosition = new Vector3(spawnPositionX, -2f, spawnPositionZ);
            other.gameObject.transform.position = spawnPosition;
            inventory.AddItem(item.item, 1);
           

        }
        
    }

    public void OnApplicationQuit()
    {
        inventory.Container.Clear();
    }
}
