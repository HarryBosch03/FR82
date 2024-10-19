using System;
using Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerEVAController : MonoBehaviour
{
    public float mouseSensitivity = 0.3f;
    public float rollSensitivity = 0.3f;

    [Space]
    public float acceleration;
    public float maxSpeed;

    [Space]
    public float interactRange = 3f;

    [Space]
    public float rotationSpring;
    public float rotationDamping;

    [Space]
    public Canvas followCanvas;
    public Canvas staticCanvas;

    [Space]
    public float currentSpeed;

    private float smoothRollInput;

    private Rigidbody body;
    private Quaternion rotation = Quaternion.identity;
    private Camera mainCamera;

    private bool startInteractInput;
    private bool stopInteractInput;
    private IInteractable currentInteractable;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    private void OnEnable() { Cursor.lockState = CursorLockMode.Locked; }

    private void OnDisable() { Cursor.lockState = CursorLockMode.None; }

    private void FixedUpdate()
    {
        DoInteractables();
        Move();
        Rotate();

        currentSpeed = body.linearVelocity.magnitude;
    }

    private void DoInteractables()
    {
        var lookingAt = GetLookingAt();

        if (startInteractInput && lookingAt != null)
        {
            currentInteractable = lookingAt;
            lookingAt.StartInteract(gameObject);
        }
        else if (currentInteractable != null && (stopInteractInput || currentInteractable != lookingAt))
        {
            currentInteractable.StopInteract(gameObject);
            currentInteractable = null;
        }

        startInteractInput = false;
        stopInteractInput = false;
    }

    private IInteractable GetLookingAt()
    {
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out var hit, interactRange))
        {
            return hit.collider.GetComponentInParent<IInteractable>();
        }
        else return null;
    }

    private void Rotate()
    {
        var up = rotation * Vector3.up;
        var forward = rotation * Vector3.forward;

        var difference = Vector3.Cross(transform.up, up) + Vector3.Cross(transform.forward, forward);
        var torque = difference * rotationSpring - body.angularVelocity * rotationDamping;
        body.AddTorque(torque, ForceMode.Acceleration);
    }

    private void Move()
    {
        if (currentInteractable != null)
        {
            var force = Vector3.ClampMagnitude(-body.linearVelocity / Time.deltaTime, acceleration);
            body.AddForce(force, ForceMode.Acceleration);
        }
        else
        {
            var map = InputSystem.actions;
            var input = new Vector3
            {
                x = map.FindAction("MoveX").ReadValue<float>(),
                y = map.FindAction("MoveY").ReadValue<float>(),
                z = map.FindAction("MoveZ").ReadValue<float>(),
            };
            var force = rotation * input * acceleration;
            var newVelocity = body.linearVelocity + force * Time.fixedDeltaTime;
            var newSpeed = newVelocity.magnitude;
            var speed = body.linearVelocity.magnitude;

            if (newSpeed > maxSpeed && newSpeed > speed)
            {
                force = (newVelocity.normalized * speed - body.linearVelocity) / Time.fixedDeltaTime;
            }

            body.AddForce(force, ForceMode.Acceleration);
        }
    }

    private void Update()
    {
        var m = Mouse.current;

        smoothRollInput = Mathf.Lerp(smoothRollInput, InputSystem.actions.FindAction("Roll").ReadValue<float>(), Time.deltaTime * 12f);

        var delta = Vector2.zero;
        if (m != null) delta += m.delta.ReadValue() * mouseSensitivity;
        rotation *= Quaternion.Euler(-delta.y, delta.x, -smoothRollInput * rollSensitivity * Time.deltaTime);

        mainCamera.transform.position = transform.position;
        mainCamera.transform.rotation = rotation;

        if (m.leftButton.wasPressedThisFrame) startInteractInput = true;
        if (m.leftButton.wasReleasedThisFrame) stopInteractInput = true;
    }

    private void OnValidate()
    {
        if (followCanvas != null && staticCanvas != null)
        {
            followCanvas.transform.position = staticCanvas.transform.position;
            followCanvas.transform.rotation = staticCanvas.transform.rotation;
            followCanvas.transform.localScale = staticCanvas.transform.localScale;
        }
    }
}