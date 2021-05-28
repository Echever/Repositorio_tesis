using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human2 : MonoBehaviour
{
    public const float constSpeed = 0.1f;
    public const float constAngle = 0;
    Animator animator;
    private Vector3 position = new Vector3(0,0,0);
    public GameObject prefab;
    public GameObject waste;
    private GameObject model;
    private float step;
    private Vector3 direction;
    private Human2 h;
    //private Human2 c;
    private bool finish = false;

    public float angle;
    public float distance;
    public float speed;



    public Human2(Vector3 position, GameObject prefab)
    {
        this.position = position;
        this.model = Instantiate(prefab, this.position, Quaternion.identity);
        this.animator = this.model.GetComponent<Animator>();
    }

    
		
    void Awake()
    {
        h = new Human2(new Vector3(-6.5f, -4, 19), prefab);
        //c = new Human2(new Vector3(1,0,1), prefab);
        //direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle))* distance;
    }
    void Start()
    {
    	   	
      	h.HumanInfo();
      	h.Rotate(Mathf.PI/2);
        //c.Rotate(0);
        
    }

    void Update()
    {
        //h.Move(direction, speed);
        //c.Move();
        //new Vector3(Mathf.Sin(2.4f), 0, Mathf.Cos(2.4f))* distance,0.1f

        h.Move(new Vector3(3, -4, 19), speed);

       
        
    }

    void HumanInfo()
    {
        Debug.Log(this.model.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject.transform.rotation);
    }

	public void Move(Vector3 endPos, float speed = constSpeed) 
    {
        bool isWalking = this.animator.GetBool("isWalking");
        if(!isWalking && (this.model.transform.position.x < endPos.x) || (this.model.transform.position.z < endPos.z))
        {
            this.animator.SetBool("isWalking", true);
        } 
        if(isWalking && (this.model.transform.position.x >= endPos.x)  && (this.model.transform.position.z >= endPos.z))
        {
            this.animator.SetBool("isWalking", false);
        }
        
        
        this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, endPos, speed * Time.deltaTime);
    }

    public void Move(){
        Vector3 endPos = new Vector3(0,0,1);
        speed = 0.1f;
        bool isWalking = this.animator.GetBool("isWalking");
        if(!isWalking && (this.model.transform.position.x < endPos.x) || (this.model.transform.position.z < endPos.z))
        {
            this.animator.SetBool("isWalking", true);
        } 
        if(isWalking && (this.model.transform.position.x >= endPos.x)  && (this.model.transform.position.z >= endPos.z))
        {
            this.animator.SetBool("isWalking", false);
            
        }
        
        this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, endPos, speed * Time.deltaTime);
    }

   public void Rotate(float angle = constAngle)
    {
        this.model.transform.Rotate(0, Mathf.Rad2Deg * angle, 0); 
    }

    public void leftHandUp(float angle = -90)
    {
        GameObject leftShoulder = this.model.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject;
        var startAngle = leftShoulder.transform.rotation;
        
        var toAngle = Quaternion.Euler(leftShoulder.transform.eulerAngles + new Vector3(angle,0,0));
        
        leftShoulder.transform.rotation = Quaternion.SlerpUnclamped(startAngle, toAngle, Time.deltaTime);
        
    }

    public void spawnWaste()
    {
        Instantiate(waste, this.model.transform.position, Quaternion.identity);
    }
    
}
