using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgenteCarro : Agent
{

    public GameObject carroRobot;
    public GameObject guia;
    public GameObject cube;
    public GameObject goalBox;

    Rigidbody carroRobotRigibody;
    bool cubeIsGripped;

    private float maxCubeX = 2.05f;
    private float minCubeX = 1.55f;
    private float maxCubeZ = 2f;
    private float minCubeZ = -2f;
    private float maxCubeY = 1.5f;
    private float minCubeY = 0.77f;

    private float maxGuiaX = 1.56f;
    private float minGuiaX = -1.77f;
    private float maxGuiaZ = 2f;
    private float minGuiaZ = -2f;
    private float maxGuiaY = 0.71f;
    private float minGuiaY = 0.72f;

    private float maxCarroX = 1.27f;
    private float minCarroX = -1.425f;
    private float maxCarroZ = 1.5f;
    private float minCarroZ = -2f;
    private float maxCarroY = 0.87f;
    private float minCarroY = 0.87f;

    public override void Initialize()
    {
        carroRobotRigibody = carroRobot.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {

        

    }


    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(cubeIsGripped);

        if (!cubeIsGripped)
        {

            sensor.AddObservation(cube.transform.localPosition - carroRobot.transform.localPosition);
            sensor.AddObservation(guia.transform.localPosition - carroRobot.transform.localPosition);
            sensor.AddObservation((cube.transform.localPosition - carroRobot.transform.localPosition) - (guia.transform.localPosition - carroRobot.transform.localPosition));


            //Posiciones de la caja
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
        }
        else{

            //Posiciones del cubo
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

            sensor.AddObservation(goalBox.transform.localPosition - carroRobot.transform.localPosition);
            sensor.AddObservation(guia.transform.localPosition - carroRobot.transform.localPosition);
            sensor.AddObservation((goalBox.transform.localPosition - carroRobot.transform.localPosition) - (guia.transform.localPosition - carroRobot.transform.localPosition));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("b");
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("b");
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 3;
        }

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                Debug.Log("A");
                break;
            case 2:
                Debug.Log("b");
                rotateDir = transform.up * 1f;
                break;
            case 3:
                rotateDir = transform.up * -1f;
                break;
        }
        carroRobot.transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        carroRobotRigibody.AddForce(dirToGo * 0.5f, ForceMode.Force);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        MoveAgent(actionBuffers.DiscreteActions);

    }

    //HELPER
    public float normalizarValor(float valorRecibido, float valorMaximo, float valorMinimo)
    {
        float valorNormalizado;

        valorNormalizado = (valorRecibido - valorMinimo) / (valorMaximo - valorMinimo);

        return valorNormalizado;
    }

}
