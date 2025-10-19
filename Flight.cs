using UnityEngine;
using System.Collections.Generic;

public class Flight_Script : MonoBehaviour
{
    [Header("Flight Settings")]
    public float maxThrust = 1000f;
    public float throttleIncrement = 50f;
    public float rollSpeed = 5f;
    public float pitchSpeed = 5f;
    public float yawSpeed = 2f;
    public float mouseControlSensitivity = 0.5f;
    public float yawToRollFactor = 0.5f;
    public float maxSpeed = 500f;

    [Header("Start Options")]
    public bool startFlyingInAir = false;

    private Rigidbody rb;
    private float currentThrust = 0f;
    private float currentFlapSetting = 0f;
    private bool gearDown = true;

    [Header("Lights (Bacon, Display, Taxi, Landing)")]
    public List<GameObject> lights = new List<GameObject>();
    private int lightStateIndex = 0;
    private const int MAX_LIGHT_STATE = 4;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is required!");
        }

        SetLights(0);

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (startFlyingInAir)
            {
                rb.useGravity = false;
                currentThrust = throttleIncrement * 2f;
            }
            else
            {
                rb.useGravity = true;
            }

            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.5f;
            rb.sleepThreshold = 0f;
        }
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    void FixedUpdate()
    {
        HandleFlightPhysics();
    }

    void HandleFlightPhysics()
    {
        if (rb == null) return;

        Vector3 thrustDirection = transform.right;
        rb.AddForce(thrustDirection * currentThrust, ForceMode.Acceleration);

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        float pitchInput = -Input.GetAxis("Mouse Y") * mouseControlSensitivity;
        float yawInput = Input.GetAxis("Mouse X") * mouseControlSensitivity;
        float adRollInput = Input.GetKey(KeyCode.A) ? -1f : (Input.GetKey(KeyCode.D) ? 1f : 0f);

        float mouseRollFromYaw = yawInput * yawToRollFactor;
        float totalRollInput = adRollInput + mouseRollFromYaw;

        Vector3 torque = new Vector3(
            totalRollInput * rollSpeed,
            yawInput * yawSpeed,
            -pitchInput * pitchSpeed
        );

        rb.AddRelativeTorque(torque, ForceMode.Acceleration);

        ApplyFlapAndGearEffects();
    }

    void ApplyFlapAndGearEffects()
    {
        if (rb == null) return;

        float flapDrag = currentFlapSetting * 0.1f;
        float gearDrag = gearDown ? 0.4f : 0f;

        rb.linearDamping = 0.1f + flapDrag + gearDrag;

        if (currentFlapSetting > 0 && rb.useGravity)
        {
            rb.AddForce(transform.up * currentFlapSetting * rb.linearVelocity.sqrMagnitude * 0.01f);
        }
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            currentThrust = Mathf.Min(currentThrust + throttleIncrement * Time.deltaTime, maxThrust);
            Debug.Log($"Throttle: {currentThrust:F0} / {maxThrust:F0}");
        }

        if (Input.GetKey(KeyCode.S))
        {
            currentThrust = Mathf.Max(currentThrust - throttleIncrement * Time.deltaTime, 0f);
            Debug.Log($"Throttle: {currentThrust:F0} / {maxThrust:F0}");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            currentFlapSetting = 0f;
            Debug.Log("Flaps: Up (0)");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentFlapSetting = 1f;
            Debug.Log("Flaps: Down (1)");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gearDown = !gearDown;
            Debug.Log($"Gear: {(gearDown ? "Down" : "Up")}");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLights();
        }
    }

    private void ToggleLights()
    {
        lightStateIndex = (lightStateIndex + 1);

        if (lightStateIndex > MAX_LIGHT_STATE)
        {
            lightStateIndex = 0;
        }

        SetLights(lightStateIndex);
    }

    private void SetLights(int countToEnable)
    {
        for (int i = 0; i < lights.Count; i++)
        {
            if (lights[i] != null)
            {
                lights[i].SetActive(i < countToEnable);
            }
        }

        if (countToEnable == 0)
        {
            Debug.Log("All lights OFF");
        }
        else
        {
            string[] lightNames = { "Bacon", "Display", "Taxi", "Landing" };
            string lightStatus = "";
            for (int i = 0; i < countToEnable && i < lightNames.Length; i++)
            {
                lightStatus += lightNames[i] + (i < countToEnable - 1 ? ", " : "");
            }
            Debug.Log($"Lights ON (State {countToEnable}): {lightStatus}");
        }
    }
}
