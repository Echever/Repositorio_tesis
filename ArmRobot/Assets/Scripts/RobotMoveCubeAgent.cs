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

    //Elementos del entorno
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;
    public GameObject table;
    public GameObject goal;
    public GameObject goalBox;

    RobotController robotController;
    TargetTouchDetector touchDetector;
    TableTouchDetector tableTouchDetector;
    PincherController pincherController;
   
    //Variables del estado de la pieza
    bool cubeIsGripped = false;
    bool cubeIsReleased = false;

    //Variables sobre el entrenamiento del brazo robot, todas estas se configuran desde el 
    //La aparicion del cubo es aleatoria
    [SerializeField]
    bool cubeIsRandom = false;

    //Si estamos entrenando al brazo unicamente a agarrar el cubo
    [SerializeField]
    bool grabBoxCurriculum = true;

    //Cual es la distancia maxima de aparicion del cubo
    [SerializeField]
    float distanciaMaximaActual;

    //Al inicio de cada entorno se obtienen las recompensas del archivo yaml y se guardan en el objeto SaveRewards que está definido más abajo
    private SaveRewards rewards = new SaveRewards();
    
    //Posicion inicial del cubo
    private Vector3 cubeStartingpos;

    //Variables maximas y minimas para la normalizacion del cubo y el robot.
    private float maxHandX = 0.6f;
    private float minHandX = -0.6f;
    private float maxHandZ = 0.6f;
    private float minHandZ = -0.6f;
    private float maxHandY = 0.7f;
    private float minHandY = 0f;

    private float maxCubeX = 0.9f;
    private float minCubeX = -0.9f;
    private float maxCubeZ = 0.6f;
    private float minCubeZ = -0.6f;
    private float maxCubeY = 1.5f;
    private float minCubeY = 0.7f;



    //Se llama la primera vez que se inicializa el entorno, se usa para cargar los valores de entorno del yaml
    public void Awake()
    {

        
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

        if (Academy.Instance.EnvironmentParameters.GetWithDefault("grabBoxCurriculum", 0) == 0)
        {
            grabBoxCurriculum = false;
        }
        else
        {
            grabBoxCurriculum = true;
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
        
    }


    // AGENT

    //Se ejecuta al inicio de cada episodio
    public override void OnEpisodeBegin()
    {

        //Si queremos la caja de destino es aleatoria se determina su posicion
        if (rewards.randomBox)
        {
            goalBox.transform.localPosition = new Vector3(Random.Range(-0.70f, -0.33f), goalBox.transform.localPosition.y, Random.Range(-0.25f, 0.26f));

        }
        
        //Se resetean los parametros del robot
        //IMPORTANTE: La rotacion 0.0f es la ultima rotacion que ha sido guardada en el editor de unity
        //TODO: Buscar los valores adecuados para el entenamiento por curriculum y que se modifiquen en ejecucion
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.StopAllJointRotations();
        robotController.ForceJointsToRotations(defaultRotations);
        pincherController.ResetGripToOpen();

        cubeIsGripped = false;
        cubeIsReleased = false;

        touchDetector.hasTouchedTarget = false;
        tableTouchDetector.hasTouchedTable = false;



        Vector3 posicionIncialCubo = new Vector3(0.472f, 0.78f, -0.033f);
        distanciaMaximaActual = Academy.Instance.EnvironmentParameters.GetWithDefault("curriculumGrip", 0.11f);


        //Aleatorizacion de la pieza
        if (cubeIsRandom)
        {
            //Cambiar la generacion de la pieza 
            if (true)
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
            cubeStartingpos = posicionIncialCubo;
        }

        cube.transform.localPosition = cubeStartingpos;

        //Rotacion de la pieza
        float rotacionActual = Academy.Instance.EnvironmentParameters.GetWithDefault("rotation", 90f);
        cube.transform.localRotation = Quaternion.Euler(0, Random.Range(-rotacionActual, rotacionActual), 0);
        
        //Eliminamos la velocidad de la pieza
        cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        //Observacion si la pieza está agarrada
        sensor.AddObservation(cubeIsGripped);

        //Observacion del agarre de la pinza
        sensor.AddObservation(pincherController.grip);

        if (!cubeIsGripped)
        {
            //sensor.AddObservation(cube.transform.rotation);
            
            //Observaciones de la rotacion de la pieza y la pinza
            sensor.AddObservation(normalizarValor(cube.transform.rotation.eulerAngles.y, 360, 0));
            sensor.AddObservation(normalizarValor(endEffector.transform.localRotation.eulerAngles.x, 360, 0));

            // Posicion relativa a la pieza
            Vector3 cubeNormalized;
            cubeNormalized.x = normalizarValor(cube.transform.localPosition.x, maxCubeX, minCubeX);
            cubeNormalized.y = normalizarValor(cube.transform.localPosition.y, maxCubeY, minCubeY);
            cubeNormalized.z = normalizarValor(cube.transform.localPosition.z, maxCubeZ, minCubeZ);

            Vector3 robotNormalized;
            robotNormalized.x = normalizarValor(robot.transform.localPosition.x, maxCubeX, minCubeX);
            robotNormalized.y = normalizarValor(robot.transform.localPosition.y, maxCubeY, minCubeY);
            robotNormalized.z = normalizarValor(robot.transform.localPosition.z, maxCubeZ, minCubeZ);

            Vector3 cubePosition = cubeNormalized - robotNormalized;
            sensor.AddObservation(cubePosition);

            // Posicion relativa de la pinza
            Vector3 handNormalized;

            Vector3 handpos = endEffector.transform.position - robot.transform.position;

            handNormalized.x = normalizarValor(handpos.x, maxHandX, minHandX);
            handNormalized.y = normalizarValor(handpos.y, maxHandY, minHandY);
            handNormalized.z = normalizarValor(handpos.z, maxHandZ, minHandZ);


            Vector3 endPosition = handNormalized - robotNormalized;
            sensor.AddObservation(endPosition);
            sensor.AddObservation(cubePosition - endPosition);

            
            //Posiciones de la caja
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

           
        }
        else
        {

            

            sensor.AddObservation(cube.transform.rotation.eulerAngles.y);
            sensor.AddObservation(endEffector.transform.localRotation.eulerAngles.x);

            //Posiciones del cubo
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

            //Posicion relativa de la caja
            Vector3 robotNormalized;

            robotNormalized.x = normalizarValor(robot.transform.localPosition.x, maxCubeX, minCubeX);
            robotNormalized.y = normalizarValor(robot.transform.localPosition.y, maxCubeY, minCubeY);
            robotNormalized.z = normalizarValor(robot.transform.localPosition.z, maxCubeZ, minCubeZ);

            Vector3 goalNormalized;
            goalNormalized.x = normalizarValor(goal.transform.localPosition.x, maxCubeX, minCubeX);
            goalNormalized.y = normalizarValor(goal.transform.localPosition.y, maxCubeY, minCubeY);
            goalNormalized.z = normalizarValor(goal.transform.localPosition.z, maxCubeZ, minCubeZ);

            Vector3 goalPosition = goalNormalized - robotNormalized;
            sensor.AddObservation(goalPosition);


            Vector3 handNormalized;

            Vector3 handpos = endEffector.transform.position - robot.transform.position;

            handNormalized.x = normalizarValor(handpos.x, maxHandX, minHandX);
            handNormalized.y = normalizarValor(handpos.y, maxHandY, minHandY);
            handNormalized.z = normalizarValor(handpos.z, maxHandZ, minHandZ);

            // Posicion relativa de la pinza con la caja
            Vector3 endPosition = handNormalized - robotNormalized;
            sensor.AddObservation(endPosition);
            sensor.AddObservation(goalPosition - endPosition);

        }


        //Observaciones del robot
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[0]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[1]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[2]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[3]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[4]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[5]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[6]));




    }

    public override void OnActionReceived(float[] vectorAction)
    {
        
        // Movimiento del robot RESTAMOS 1 POR QUE NO SON JOINTS
        for (int jointIndex = 0; jointIndex < vectorAction.Length - 1; jointIndex++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection((int)vectorAction[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);
        }

        //Movimiento de la pinza
        int pincherInput = (int) vectorAction[7];
        pincherController.gripState = GripStateForInput(pincherInput - 1);

        //Si detectamos el agarre se da la recompensa y se activa el flag cubeIsGripped para que solo se de la recompensa una vez
        if (estaAgarrado())
        {
            cubeIsGripped = true;
            Debug.Log("Cube Gripped");

            SetReward(rewards.rewardGrip);

            //Una pequeña comprobacion para ver los agarres en zonas nuevas
            if (cube.transform.localPosition.x > (cubeStartingpos.x + distanciaMaximaActual - 0.01) || cube.transform.localPosition.x < (cubeStartingpos.x - distanciaMaximaActual + 0.01) && cube.transform.localPosition.z > (cubeStartingpos.z + distanciaMaximaActual - 0.01) || cube.transform.localPosition.z < (cubeStartingpos.z - distanciaMaximaActual + 0.01))
            {
                Debug.Log("A");
            }

            //Si tenemos el curriculum activado esto va a hacer que al principio solo aprenda a agarrar el cubo, una vez pase de un cierto punto avanzará en el entrenamiento.
            if (grabBoxCurriculum)
            {
                Debug.Log("Estamos en curriculum, asi que reseteamos!");
                EndEpisode();
            }
        }

        
        

        // Comprobaciones si el cubo se cae de la mesa
        if (cube.transform.position.y < 0.6f || cube.transform.localPosition.z < -0.9f || cube.transform.localPosition.z > 0.955f || cube.transform.localPosition.x < -0.92f || cube.transform.localPosition.x > 0.92f)
        {
            SetReward(rewards.rewardCubeKnockedOff);
            EndEpisode();
        }

        
        //Comprobacion para ver si echa el cubo fuera de la zona inicial
        /*
        if ((cube.transform.localPosition.z < (cubeStartingpos.z - 0.1f) || cube.transform.localPosition.z > (cubeStartingpos.z + 0.1f) || cube.transform.localPosition.x < (cubeStartingpos.x  - 0.1) || cube.transform.localPosition.x > (cubeStartingpos.x + 0.1)) && !cubeIsGripped)
        {
            SetReward(rewards.rewardCubeKnockedOff);
            EndEpisode();
        }
        */
        
        
        //Comprobar que la pieza se suelta dentro del area del cubo

        //Se calcula lis limites, y si se suelta dentro de este area se da una recompensa
        float lowX = goalBox.transform.localPosition.x - (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float upX = goalBox.transform.localPosition.x + (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float lowZ = goalBox.transform.localPosition.z - (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);
        float upZ = goalBox.transform.localPosition.z + (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);

        
        if (!cubeIsReleased && cubeIsGripped && pincherController.grip < 0.4f && cube.transform.localPosition.x > lowX && cube.transform.localPosition.x < upX && cube.transform.localPosition.z > lowZ && cube.transform.localPosition.z < upZ)
        {
            Debug.Log("Cube released in the area! NUEVO");
            cubeIsReleased = true;
            SetReward(rewards.rewardReleaseInArea);
        }

        //Comprobamos que el cubo ha tocado el area verde de la caja
        if (goal.GetComponent<Goal>().goal || cube.GetComponent<TargetTouchDetector>().hasTouchedTarget)
        {
            Debug.Log("Finished!");
            SetReward(rewards.rewardFinish);
            EndEpisode();
        }
        
      
        //Si se suelta el cubo y cae en la mesa se penaliza
        if (tableTouchDetector.hasTouchedTable)
        {
            cubeIsGripped = false;
            SetReward(rewards.rewardCubeReleased);
            EndEpisode();
        }

        AddReward(rewards.rewardTime / MaxStep);
        
    }

    // Controles manuales
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

    public bool estaAgarrado()
    {

        bool cuboEnPinza = cube.GetComponent<Collider>().bounds.Contains(endEffector.GetComponent<PincherController>().CurrentGraspCenter());
        bool pinzasCerrandas = pincherController.grip > 0.5f;
        bool collidersTocanPieza = pincherController.fingerA.GetComponent<FingerTouch>().isTouchingCube && pincherController.fingerB.GetComponent<FingerTouch>().isTouchingCube;
        bool piezaEnElAire = cube.transform.localPosition.y > 0.8f;

        return cuboEnPinza && pinzasCerrandas && collidersTocanPieza && piezaEnElAire && !cubeIsGripped;
    }

    // HELPERS
    //Objeto para guardar recomensas
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

    //Direcciones
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

    //Funciones para normalizar
    public float normalizarValor(float valorRecibido, float valorMaximo, float valorMinimo)
    {
        float valorNormalizado;
        
        valorNormalizado = (valorRecibido - valorMinimo) / (valorMaximo - valorMinimo);
        
        return valorNormalizado;
    }

    public float normalizarRotacion(float valorRecibido)
    {
        float eliminarVueltas = valorRecibido % 360;
        

        if(eliminarVueltas < 0)
        {
            eliminarVueltas = Math.Abs(eliminarVueltas);
            eliminarVueltas = 360 - eliminarVueltas;
        }

        float valorNormalizado = (eliminarVueltas - 0) / (360 - 0);
    
        return valorNormalizado;
    }




}


