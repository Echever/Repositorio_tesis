using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateTerrain : MonoBehaviour
{
    public GameObject tree;
    public GameObject bench;
    public GameObject pavement;
    public GameObject l;

    public int columnLength = 7;
    public int rowLength = 9;
    public float xSpace = 2;
    public float zSpace = 3.6f;
    public string code = "aiaiaiaaiaiaiaaiaiaiaaiciciaIIIIIIIaiCiCiaaiaiaiaaiaiaiaaiaiaia"; 
    // Start is called before the first frame update
    void Start()
    {
        spawn(code);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void spawn(string code)
    {
        for(int i= 0; i< columnLength*rowLength; i++)
        {
            switch (code[i])
            {
                case 'a':
                    Instantiate(tree, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.identity);
                    break;
                case 'b':
                    Instantiate(bench, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.identity);
                    break;
                case 'B':
                    Instantiate(bench, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0,180,0)));
                    break;
                case 'c':
                    Instantiate(bench, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, 90, 0)));
                    break;
                case 'C':
                    Instantiate(bench, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, -90, 0)));
                    break;
                case 'i':
                    Instantiate(pavement, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.identity);
                    break;
                case 'I':
                    Instantiate(pavement, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, 90, 0)));
                    break;
                case 'l':
                    Instantiate(l, new Vector3(xSpace * (i % columnLength), 0, zSpace * (i / columnLength)), Quaternion.identity);
                    break;
            }
            
        }
    }

}
