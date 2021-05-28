using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanciatePrefab : MonoBehaviour
{
    public GameObject prefab;
    public GameObject copyContainer;
    public int num = 3;
    // Start is called before the first frame update
    void Start()
    {
        createPrefabs(num);
    }
    
    void createPrefabs(int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject humanCopy = Instantiate(prefab, new Vector3(i, prefab.transform.position.y, i), Quaternion.identity);
            humanCopy.transform.parent = copyContainer.transform;
            humanCopy.name = "Human" + (i+1);
        
        }
    }
}
