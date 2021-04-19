using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

using Random = UnityEngine.Random;
using System.IO;

public class RobotMoveCubeAgent : Agent
{
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;
    public GameObject table;
    public GameObject wrist;
    public GameObject goal;
    public GameObject goalBox;

    RobotController robotController;
    TargetTouchDetector touchDetector;
    TableTouchDetector tableTouchDetector;
    PincherController pincherController;
   

    bool cubeIsGripped = false;
    bool cubeIsReleased = false;
    //TablePositionRandomizer tablePositionRandomizer;

    [SerializeField]
    bool cubeIsRandom = false;

    [SerializeField]
    float distanciaMaximaActual;

    [SerializeField]
    float distanciaMaximaAnterior = 0;

    private SaveRewards rewards = new SaveRewards();

    private Vector3 cubeStartingpos;

    private String path;

    public void Awake()
    {

        /*
        path = Application.dataPath + "/StreamingAssets";

        if (File.Exists(path + "/rewards.json"))
        {
            Debug.Log("JSON EXISTS!");
            string jsonRewardString = File.ReadAllText(path + "/rewards.json");

            rewards = JsonUtility.FromJson<SaveRewards>(jsonRewardString);

            Debug.Log("Box Random: " + rewards.randomBox);
            Debug.Log("Cube Random: " + rewards.randomCube);
            Debug.Log("rewardGrip " + rewards.rewardGrip);
            Debug.Log("rewardReleaseInArea: " + rewards.rewardReleaseInArea);
            Debug.Log("rewardFinish: " + rewards.rewardFinish);
            Debug.Log("rewardCubeKnockedOff:  " + rewards.rewardCubeKnockedOff);
            Debug.Log("rewardCubeReleased: " + rewards.rewardCubeReleased);
            Debug.Log("rewardTime: " + rewards.rewardTime);

            Debug.Log(jsonRewardString);
        }
        */

        
        if(Academy.Instance.EnvironmentParameters.GetWithDefault("randomBox", 0) == 0)
        {
            rewards.randomBox = false;
        }
        else
        {
            rewards.randomBox = true;
        }

        if (Academy.Instance.EnvironmentParameters.GetWithDefault("randomCube", 0) == 0)
        {
            cubeIsRandom = false;
        }
        else
        {
            cubeIsRandom = true;
        }
        
        

        rewards.rewardGrip = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardGrip", 0);
        rewards.rewardReleaseInArea = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardReleaseInArea", 0);
        rewards.rewardFinish = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardFinish", 0);
        rewards.rewardCubeKnockedOff = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardCubeKnockedOff", 0);
        rewards.rewardCubeReleased = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardCubeReleased", 0);
        rewards.rewardTime = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardTime", 0);
        

        robotController = robot.GetComponent<RobotController>();
        touchDetector = cube.GetComponent<TargetTouchDetector>();
        tableTouchDetector = table.GetComponent<TableTouchDetector>();
        pincherController = endEffector.GetComponent<PincherController>();
        //tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }


    // AGENT

    public override void OnEpisodeBegin()
    {
        if (rewards.randomBox)
        {
            goalBox.transform.localPosition = new Vector3(Random.Range(-0.70f, -0.33f), goalBox.transform.localPosition.y, Random.Range(-0.25f, 0.26f));

        }
        //goalBox.transform.localPosition = new Vector3(Random.Range(-0.70f, -0.33f), goalBox.transform.localPosition.y, Random.Range(-0.25f, 0.26f));

        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.StopAllJointRotations();
        robotController.ForceJointsToRotations(defaultRotations);

        cubeIsGripped = false;
        cubeIsReleased = false;

        touchDetector.hasTouchedTarget = false;
        tableTouchDetector.hasTouchedTable = false;

        Vector3 posicionIncialCubo = new Vector3(0.472f, 0.778f, -0.0396f);
        distanciaMaximaActual = Academy.Instance.EnvironmentParameters.GetWithDefault("curriculumGrip", 0.01f);

       


        //Aleatorizacion de la pieza
        if (cubeIsRandom)
        {
            //Cambiar la generacion de la pieza 
            if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculumGrip", 0) != 0)
            {
                //Si la distancia en X es mas que 0.18 el brazo no llega
                if (distanciaMaximaActual > 0.18)
                {
                    cubeStartingpos = new Vector3(Random.Range(posicionIncialCubo.x - distanciaMaximaActual, posicionIncialCubo.x + 0.18f), posicionIncialCubo.y, Random.Range(posicionIncialCubo.z - distanciaMaximaActual, posicionIncialCubo.z + distanciaMaximaActual));
                }
                else
                {
                    cubeStartingpos = new Vector3(Random.Range((posicionIncialCubo.x - distanciaMaximaActual), posicionIncialCubo.x + distanciaMaximaActual), posicionIncialCubo.y, Random.Range(posicionIncialCubo.z - distanciaMaximaActual, posicionIncialCubo.z + distanciaMaximaActual));
                }
            }
        }
        else
        {
            cubeStartingpos = new Vector3(0.472f, 0.778f, -0.0396f);
        }

        cube.transform.localPosition = cubeStartingpos;

        

        cube.transform.localRotation = Quaternion.Euler(0, 0, 0);
        cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
        
        pincherController.ResetGripToOpen();

       

        //tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }


        sensor.AddObservation(cubeIsGripped);



        if (!cubeIsGripped)
        {
            // relative cube position
            Vector3 cubePosition = cube.transform.position - robot.transform.position;
            sensor.AddObservation(cubePosition);

            // relative end position
            Vector3 endPosition = endEffector.transform.position - robot.transform.position;
            sensor.AddObservation(endPosition);
            sensor.AddObservation(cubePosition - endPosition);

            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

            //Debug.Log("Cube pos: "+ cubePosition);
            //Debug.Log("End pos: " + endPosition);
            //Debug.Log("Relative end pos: " + (cubePosition - endPosition));
        }
        else
        {

            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

            // relative goal position
            Vector3 goalPosition = goalBox.transform.position - robot.transform.position;
            sensor.AddObservation(goalPosition);

            // relative end position
            Vector3 endPosition = endEffector.transform.position - robot.transform.position;
            sensor.AddObservation(endPosition);
            sensor.AddObservation(goalPosition - endPosition);

            //Debug.Log("Goal pos: "+ goalPosition);
            //Debug.Log("End pos: " + endPosition);
            //Debug.Log("Relative end pos" + (goalPosition - endPosition));
        }

        
        
        

        
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // move RESTAMOS 2 POR QUE NO SON JOINTS
        for (int jointIndex = 0; jointIndex < vectorAction.Length - 1; jointIndex++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int)vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }

        //pincher
        float pincherInput = (int) vectorAction[7];
        pincherController.gripState = GripStateForInput(pincherInput - 1);

        /*
        int handInput = (int) vectorAction[8];
        robotController.RotateJoint(6, ActionIndexToRotationDirection(handInput), false);
        */

        //Debug.Log(endEffector.GetComponent<PincherController>().CurrentGraspCenter().ToString("F4"));

        //if (cube.GetComponent<Collider>().bounds.Contains(endEffector.GetComponent<PincherController>().CurrentGraspCenter()) && pincherController.grip > 0.5f && pincherController.fingerA.GetComponent<FingerTouch>().isTouchingCube && pincherController.fingerB.GetComponent<FingerTouch>().isTouchingCube && cube.transform.localPosition.y > 0.8f && !cubeIsGripped)

        if (cube.GetComponent<Collider>().bounds.Contains(endEffector.GetComponent<PincherController>().CurrentGraspCenter()) && pincherController.grip > 0.5f && pincherController.fingerA.GetComponent<FingerTouch>().isTouchingCube && pincherController.fingerB.GetComponent<FingerTouch>().isTouchingCube && cube.transform.localPosition.y > 0.8f && !cubeIsGripped)
        {
            cubeIsGripped = true;
            Debug.Log("Cube Gripped");
            SetReward(rewards.rewardGrip);

            if (cube.transform.localPosition.x > (cubeStartingpos.x + distanciaMaximaActual - 0.01) || cube.transform.localPosition.x < (cubeStartingpos.x - distanciaMaximaActual + 0.01) && cube.transform.localPosition.z > (cubeStartingpos.z + distanciaMaximaActual - 0.01) || cube.transform.localPosition.z < (cubeStartingpos.z - distanciaMaximaActual + 0.01))
            {
                Debug.Log("A");
            }

            //Si tenemos el curriculum activado esto va a hacer que al principio solo aprenda a agarrar el cubo, una vez pase de un cierto punto avanzará en el entrenamiento.
            if(Academy.Instance.EnvironmentParameters.GetWithDefault("curriculumGrip", 0) != 0)
            {
                Debug.Log("Estamos en curriculum, asi que reseteamos!");
                EndEpisode();
            }
        }

        
        

        // Knocked the cube off the table
        if (cube.transform.position.y < 0.6f || cube.transform.localPosition.z < -0.9f || cube.transform.localPosition.z > 0.955f || cube.transform.localPosition.x < -0.92f || cube.transform.localPosition.x > 0.92f)
        {
            SetReward(rewards.rewardCubeKnockedOff);
            EndEpisode();
        }

        //knocked the cube from starting position (Si el cubo es aleatorio esto no hace falta)
        if ((cube.transform.localPosition.z < (cubeStartingpos.z - 0.1f) || cube.transform.localPosition.z > (cubeStartingpos.z + 0.1f) || cube.transform.localPosition.x < (cubeStartingpos.x  - 0.1) || cube.transform.localPosition.x > (cubeStartingpos.x + 0.1)) && !cubeIsGripped)
        {
            SetReward(rewards.rewardCubeKnockedOff);
            EndEpisode();
        }
        
        

        float lowX = goalBox.transform.localPosition.x - (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float upX = goalBox.transform.localPosition.x + (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float lowZ = goalBox.transform.localPosition.z - (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);
        float upZ = goalBox.transform.localPosition.z + (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);

        //Debug.Log("goalX" + goal.transform.localPosition.x);
        //Debug.Log("goalZ" + goal.transform.localPosition.z);
        //Debug.Log("lowX " + lowX);
        //Debug.Log("upX " + upX);
        //Debug.Log("lowZ " + lowZ);
        //Debug.Log("upZ " + upZ);


        if (!cubeIsReleased && cubeIsGripped && pincherController.grip < 0.4f && cube.transform.localPosition.x > lowX && cube.transform.localPosition.x < upX && cube.transform.localPosition.z > lowZ && cube.transform.localPosition.z < upZ)
        {
            Debug.Log("Cube released in the area! NUEVO");
            cubeIsReleased = true;
            SetReward(rewards.rewardReleaseInArea);
        }


        if (goal.GetComponent<Goal>().goal || cube.GetComponent<TargetTouchDetector>().hasTouchedTarget)
        {
            Debug.Log("Finished!");
            SetReward(rewards.rewardFinish);
            EndEpisode();
        }
      

        if (tableTouchDetector.hasTouchedTable)
        {
            SetReward(rewards.rewardCubeReleased);
            EndEpisode();
        }

        AddReward(rewards.rewardTime / MaxStep);
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        for (int i = 0; i < robotController.joints.Length; i++)
        {
            float inputVal = Input.GetAxis(robotController.joints[i].inputAxis);
            switch (Mathf.RoundToInt(inputVal))
            {
                case -1:

                    discreteActions[i] = 2;
                    break;
                case 0:
                    discreteActions[i] = 1;
                    break;

                case +1:
                    discreteActions[i] = 0;
                    break;
            }
        }
       

        float pincherInput = Input.GetAxis("Fingers");
        

        switch (Mathf.RoundToInt(pincherInput))
        {
            case -1:

                discreteActions[7] = 2;
                break;
            case 0:
                discreteActions[7] = 1;
                break;

            case +1:
                discreteActions[7] = 0;
                break;
        }



    }

    // HELPERS

    public class SaveRewards
    {
        public bool randomBox = false;
        public bool randomCube = false;
        public float rewardGrip = 0.5f;
        public float rewardReleaseInArea = 0.5f;
        public float rewardFinish = 0.5f;
        public float rewardCubeKnockedOff = -1f;
        public float rewardCubeReleased = -1f;
        public float rewardTime = -1f;


    }

    public SaveRewards loadFromJson()
    {
        SaveRewards ret = new SaveRewards();

        return ret;
    }

    static public RotationDirection ActionIndexToRotationDirection(int actionIndex)
    {
        return (RotationDirection)(actionIndex - 1);
    }

    static GripState GripStateForInput(float input)
    {
        if (input > 0)
        {
            return GripState.Closing;
        }
        else if (input < 0)
        {
            return GripState.Opening;
        }
        else
        {
            return GripState.Fixed;
        }
    }

    static RotationDirection GetRotationDirection(float inputVal)
    {
        if (inputVal > 0)
        {
            return RotationDirection.Positive;
        }
        else if (inputVal < 0)
        {
            return RotationDirection.Negative;
        }
        else
        {
            return RotationDirection.None;
        }
    }




}


