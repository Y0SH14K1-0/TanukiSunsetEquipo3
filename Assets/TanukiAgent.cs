using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;
using Unity.MLAgents.Sensors.Reflection;
using System.Collections.Generic;



public class TanukiAgent : Agent
{
    [Observable(numStackedObservations: 8)]
    public float speedZ = 0.0f;

    [Observable(numStackedObservations: 8)]
    public float speedX = 0.0f;

    [Header("Configuracion de Pista y Sensores")]
    public LayerMask capaPista;
    public LayerMask capaObstaculos;
    public float distanciaSensorLateral = 2.0f;

    [Header("Parametros de Recompensa")]
    public float recompensaWaypoint = 0.5f;
    public float recompensaDrift = 0.004f;
    public float penalizacionObstaculo = -2.0f;
    public bool terminarEpisodioAlChocar = true;

    [Header("Debug")]
    public bool driftActivo = false;


    LongBoardControls lbc;
    Vector3 initialPosition;
    Quaternion initialRotation;
    public bool visualizar = false;
    public Slider girar;
    public Image frenar;
    public Image acelerar;
    private Rigidbody rb;
    private float timer = 0.0f;
    private float tiempoEnAire = 0.0f;
    private float tiempoSinAvanzar = 0.0f;
    private float steerAnterior = 0.0f;
    private HashSet<Collider> waypointsVisitados = new HashSet<Collider>();
    private float Norm(float current, float min, float max)
    {
        float normalizedValue = (current - min) / (max - min);
        if (normalizedValue > 1.0f) normalizedValue = 1.0f;
        if (normalizedValue < -1.0f) normalizedValue = -1.0f;
        return normalizedValue;
    }

    public override void Initialize()
    {  // Es como el método Start del agente
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        lbc = GetComponent<LongBoardControls>();

        if (capaObstaculos.value == 0)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1)
            {
                capaObstaculos = 1 << obstacleLayer;
            }
        }
    }

    public override void OnEpisodeBegin()
    {  // Acciones a realizar al inicio de un episodio de entrenamiento
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        rb.position = initialPosition;
        rb.rotation = initialRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        lbc.myHorizontal = 0.0f;
        lbc.myVertical = 0.0f;
        lbc.myDrift = false;
        driftActivo = false;
        lbc.lStickInputLerped = Vector2.zero;
        lbc.grounded = true;

        waypointsVisitados.Clear();
        timer = 0.0f;
        tiempoEnAire = 0.0f;
        tiempoSinAvanzar = 0.0f;
        steerAnterior = 0.0f;

        Physics.SyncTransforms();
        lbc.EnableGameplay();
    }

    public override void CollectObservations(VectorSensor sensor)
    {  // Recolecta observaciones y arma el vector
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        speedZ = Norm(localVel.z, 0.0f, 50.0f);
        speedX = Mathf.Clamp(localVel.x / 50.0f, -1.0f, 1.0f);

        // Sensores de vacio en los costados (ignoran triggers como Waypoints)
        bool sueloIzq = Physics.Raycast(
            transform.position - transform.right * distanciaSensorLateral + Vector3.up,
            Vector3.down,
            4f,
            capaPista,
            QueryTriggerInteraction.Ignore
        );
        bool sueloDer = Physics.Raycast(
            transform.position + transform.right * distanciaSensorLateral + Vector3.up,
            Vector3.down,
            4f,
            capaPista,
            QueryTriggerInteraction.Ignore
        );
        sensor.AddObservation(sueloIzq ? 1.0f : 0.0f);
        sensor.AddObservation(sueloDer ? 1.0f : 0.0f);

        // Velocidad de rotacion en Y
        float angularY = Mathf.Clamp(rb.angularVelocity.y / 5.0f, -1.0f, 1.0f);
        sensor.AddObservation(angularY);

        // Ayuda a suavizar la conduccion
        sensor.AddObservation(steerAnterior);

        // Estado actual de drifteo y deslizamiento lateral
        sensor.AddObservation(lbc.drifting ? 1.0f : 0.0f);
        sensor.AddObservation(Mathf.Clamp(lbc.driftVal, -1.0f, 1.0f));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   // Ejecuta las acciones recibidas
        float steer = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1.0f, 1.0f);
        float accel = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1.0f, 1.0f);
        bool drift = false;
        if (actionBuffers.DiscreteActions.Length > 0)
        {
            drift = actionBuffers.DiscreteActions[0] == 1;
        }
        else if (actionBuffers.ContinuousActions.Length > 2)
        {
            drift = actionBuffers.ContinuousActions[2] > 0.0f;
        }

        lbc.myHorizontal = steer;
        lbc.myVertical = accel;
        lbc.myDrift = drift;
        driftActivo = drift;

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float maxSpd = lbc.maxSpeed > 0.1f ? lbc.maxSpeed : 50.0f;
        float forwardSpeedNorm = Mathf.Clamp01(localVel.z / maxSpd);

        // Recompensa continua por avanzar hacia adelante
        AddReward(0.005f * forwardSpeedNorm);

        // Recompensa por drifteo controlado avanzando sobre la pista
        if (lbc.drifting && lbc.grounded && lbc.dirDot > 0.35f && lbc.dirDotABS < 0.92f && rb.linearVelocity.magnitude > 10.0f)
        {
            AddReward(recompensaDrift * Mathf.Abs(lbc.driftVal));
        }

        // Ligera penalizacion por paso para incentivar velocidad
        AddReward(-0.001f);

        // Penalizacion por cambios bruscos de direccion
        float cambioGiro = Mathf.Abs(steer - steerAnterior);
        AddReward(-0.002f * cambioGiro);
        steerAnterior = steer;
    }

    private bool EsObstaculo(GameObject obj)
    {
        if (obj == null) return false;
        return (capaObstaculos.value & (1 << obj.layer)) != 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (EsObstaculo(collision.gameObject) || EsObstaculo(collision.collider.gameObject))
        {
            AddReward(penalizacionObstaculo);
            if (terminarEpisodioAlChocar)
            {
                EndEpisode();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint") && !waypointsVisitados.Contains(other))
        {
            waypointsVisitados.Add(other);
            AddReward(recompensaWaypoint);
        }
        else if (EsObstaculo(other.gameObject))
        {
            AddReward(penalizacionObstaculo);
            if (terminarEpisodioAlChocar)
            {
                EndEpisode();
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // A/D o Flechas
        continuousActions[1] = Input.GetAxis("Vertical");   // W/S o Flechas

        bool driftInput = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift) ||
                          (RewindInput.Instance != null && RewindInput.Instance.GetButton(RewindInputActions.DRIFT));

        var discreteActions = actionsOut.DiscreteActions;
        if (discreteActions.Length > 0)
        {
            discreteActions[0] = driftInput ? 1 : 0;
        }
        else if (continuousActions.Length > 2)
        {
            continuousActions[2] = driftInput ? 1.0f : -1.0f;
        }
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (!lbc.grounded)
        {
            tiempoEnAire += Time.fixedDeltaTime;
        }
        else
        {
            tiempoEnAire = 0.0f;
        }

        // Termina el episodio si lleva mas de 0.25s fuera de la pista
        if (timer > 0.5f && tiempoEnAire > 0.25f)
        {
            AddReward(-2.0f);
            EndEpisode();
            return;
        }

        // Termina el episodio si se queda atascado o yendo hacia atras
        float forwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;
        if (timer > 1.0f && forwardSpeed < 1.0f)
        {
            tiempoSinAvanzar += Time.fixedDeltaTime;
            if (tiempoSinAvanzar > 2.0f)
            {
                AddReward(-1.0f);
                EndEpisode();
                return;
            }
        }
        else
        {
            tiempoSinAvanzar = 0.0f;
        }
    }


}

