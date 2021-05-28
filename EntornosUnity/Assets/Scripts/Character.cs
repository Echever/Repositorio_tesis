using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    
    public float distance;
    public float speed;
    public float angle;
    private float step;
    public GameObject prefab;
    private GameObject copy;
    Animator animator;

    private Vector3 startPos;
    private Vector3 endPos;
/*
    public Character()
    {
        distance = 10.0f;
        angle = -1.4f;
    }
    
    public Character(float sp)
    {
        speed = sp;
        
    }
    */
    void Awake()
    {
        //Character c = new Character();
        //InstanciatePrefab(prefab);
        
    }

    // Start is called before the first frame update
    void Start()
    {
    
        animator = GetComponent<Animator>();
        startPos = transform.position;
        endPos = transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle))* distance;
        Rotate(angle);
    }

    // Update is called once per frame
    void Update()
    { 
        Move(speed);
    }

    public void Move(float speed) 
    {
        bool isWalking = animator.GetBool("isWalking");
        if(!isWalking && (transform.position.x < endPos.x) || (transform.position.z < endPos.z))
        {
            animator.SetBool("isWalking", true);
        } 
        if(isWalking && (transform.position.x >= endPos.x)  && (transform.position.z >= endPos.z))
        {
            animator.SetBool("isWalking", false);
        }
        step += speed;
        transform.position = Vector3.MoveTowards(startPos, endPos, step);
    }
    
    public void Rotate(float angle)
    {
        transform.Rotate(0, Mathf.Rad2Deg * angle, 0); 
    }

    public void MoveHand(bool right){
        
    }

    public void InstanciatePrefab(GameObject prefab)
    {
        copy = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
    }
}
