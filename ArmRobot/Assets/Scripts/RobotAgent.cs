using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class RobotAgent : Agent
{
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;
    public GameObject table;
    public GameObject wrist;

    RobotController robotController;
    TargetTouchDetector touchDetector;
    TableTouchDetector tableTouchDetector;
    //TablePositionRandomizer tablePositionRandomizer;


    void Start()
    {
        robotController = robot.GetComponent<RobotController>();
        touchDetector = cube.GetComponent<TargetTouchDetector>();
        tableTouchDetector = table.GetComponent<TableTouchDetector>();
        //tablePositionRandomizer = cube.GetComponent<TablePositionRandomizer>();
    }


    // AGENT

    public override void OnEpisodeBegin()
    {
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.ForceJointsToRotations(defaultRotations);
        touchDetector.hasTouchedTarget = false;
        tableTouchDetector.hasTouchedTable = false;
        cube.transform.localPosition = new Vector3(0.472f, 0.778f, -0.0396f);
        //tablePositionRandomizer.Move();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        
        // relative cube position
        Vector3 cubePosition = cube.transform.position - robot.transform.position;
        sensor.AddObservation(cubePosition);

        // relative end position
        Vector3 endPosition = endEffector.transform.position - robot.transform.position;
        sensor.AddObservation(endPosition);
        sensor.AddObservation(cubePosition - endPosition);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // move
        for (int jointIndex = 0; jointIndex < vectorAction.Length; jointIndex ++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int) vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }

        // Knocked the cube off the table
        if (cube.transform.position.y < -1.0)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // end episode if we touched the cube
        //QUE EL ANGULO DE LA PINZA SEA EL Z
        //Debug.Log("Rotacion Local: " + endEffector.transform.localRotation.eulerAngles);
        //Debug.Log("Rotacion Global: " + endEffector.transform.rotation.eulerAngles);
        //Debug.Log(endEffector.transform.rotation.eulerAngles);
        var handRotation = endEffector.transform.rotation.eulerAngles;
        if (touchDetector.hasTouchedTarget)
        {
            if ((handRotation.z <= 190 || handRotation.z >= 170) && (handRotation.x >= 350 || handRotation.x <= 10))
            {
                Debug.Log("Ha tocado el objeto bien!");
                SetReward(1f);
            }
            else
            {
                Debug.Log("Ha tocado el objeto mal!");
                SetReward(-1f);
            }
            
            //EndEpisode();
        }

        if(tableTouchDetector.hasTouchedTable)
        {
            SetReward(-1f);
            EndEpisode();
        }


        //reward
        float distanceToCube = Vector3.Distance(endEffector.transform.position, cube.transform.position); // roughly 0.7f


        var jointHeight = 0f; // This is to reward the agent for keeping high up // max is roughly 3.0f
        for (int jointIndex = 0; jointIndex < robotController.joints.Length; jointIndex ++)
        {
            jointHeight += robotController.joints[jointIndex].robotPart.transform.position.y - cube.transform.position.y;
        }
        var reward = -distanceToCube + jointHeight / 100f;

        //Lo ideal es que el valor de la rotacion esté en 360º, pero vamos a aceptar 350º y 10º
        
        if((handRotation.z <= 190 || handRotation.z >= 170) && (handRotation.x >= 350 || handRotation.x <= 10))
        {
            reward += 2 / 100f;
        }
        else
        {
            reward += -10 / 100f;
        }
        
        AddReward(reward * 0.1f);

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
        robotController.StopAllJointRotations();
        
        float input = Input.GetAxis("Fingers");
        PincherController pincherController = endEffector.GetComponent<PincherController>();
        pincherController.gripState = GripStateForInput(input);


    }

    // HELPERS

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




}


