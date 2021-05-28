using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
    public const float constSpeed = 10f;
    public const float constAngle = 0;
    private Vector3 position = new Vector3(0,0,0);
    public GameObject prefab;
    private GameObject model;
    private float step;
    private Vector3 direction;
    private Human h;
    private Human c;

    public float angle;
    public float distance;
    public float speed;


    public Human(Vector3 position, GameObject prefab)
    {
        this.position = position;
        this.model = Instantiate(prefab, this.position, Quaternion.identity);
    }

    
		
    void Awake()
    {
        h = new Human(new Vector3(-6.5f,-4,19), prefab);
        h.Rotate(90);
       // c = new Human(new Vector3(1,0,1), prefab);
        direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle))* distance;
    }
    void Start()
    {
    	   	
      	h.HumanInfo();
        h.Move(new Vector3(8, 0, 0));  
       
    }

    void Update()
    {    	
        if(Input.GetKey("w")){
            h.Move();
        }

    	if(Input.GetKey("q")){
            h.rightHandUp();
        }

        

        if(Input.GetKey("e")){
            h.leftHandUp();
        }

         if(Input.GetKey("space")){
            h.Rotate(angle);
            h.HumanInfo();
        }
        //c.Move();
        //new Vector3(Mathf.Sin(2.4f), 0, Mathf.Cos(2.4f))* distance,0.1f
    }

    void HumanInfo()
    {
        Debug.Log(this.model.transform.rotation);
    }

	public void Move(Vector3 endPos, float speed = constSpeed) 
    {    
        this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, endPos, speed * Time.deltaTime);
    }

    public void Move(){
        Vector3 endPos = this.model.transform.position + new Vector3(this.model.transform.rotation.y, 0, this.model.transform.rotation.y);
        speed = 10f;
              
        this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, endPos, speed * Time.deltaTime);
    }

   public void Rotate(float angle = constAngle)
    {
        var startAngle = this.model.transform.rotation;
        var toAngle = Quaternion.Euler(this.model.transform.eulerAngles + new Vector3(0,Mathf.Rad2Deg * angle,0));
        this.model.transform.rotation = Quaternion.SlerpUnclamped(startAngle, toAngle, Time.deltaTime);       
    }

    public void leftHandUp(float angle = -90)
    {
        GameObject leftShoulder = this.model.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject;
        var startAngle = leftShoulder.transform.rotation;
        
        var toAngle = Quaternion.Euler(leftShoulder.transform.eulerAngles + new Vector3(angle,0,0));
        
        leftShoulder.transform.rotation = Quaternion.SlerpUnclamped(startAngle, toAngle, Time.deltaTime);
        
    }

    public void rightHandUp(float angle = -90)
    {
        GameObject rightShoulder = this.model.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetChild(2).gameObject;
        var startAngle = rightShoulder.transform.rotation;
        
        var toAngle = Quaternion.Euler(rightShoulder.transform.eulerAngles + new Vector3(angle,0,0));
        
        rightShoulder.transform.rotation = Quaternion.Slerp(startAngle, toAngle, Time.deltaTime);
        
        
    }
    

   
}
