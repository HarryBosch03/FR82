using System;
using Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    public Transform head;
    public float mouseSensitivity = 0.3f;

    [Space]
    public float moveSpeed = 7f;
    public float accelerationTime = 0.1f;

    [Space]
    public float collisionHeight = 1.8f;
    public float collisionRadius = 0.3f;
    public float stepHeight = 0.4f;
    public LayerMask collisionMask = ~0;

    [Space]
    public float interactDistance = 2f;

    private Camera mainCamera;
    private Vector2 rotation;
    private Vector3 velocity;
    private Vector3 force;
    private new CapsuleCollider collider;

    private Vector3 lerpPos0;
    private Vector3 lerpPos1;
    private IInteractable currentInteractable;

    public IInteractable LookingAt { get; private set; }

    private void Awake()
    {
        collider = gameObject.AddComponent<CapsuleCollider>();
        collider.hideFlags = HideFlags.HideAndDontSave;

        mainCamera = Camera.main;
    }

    private void OnEnable() { Cursor.lockState = CursorLockMode.Locked; }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        if (currentInteractable != null)
        {
            currentInteractable.Release(this);
            currentInteractable = null;
        }
    }

    private void OnDestroy() { Destroy(collider); }

    private void LateUpdate()
    {
        transform.position = Vector3.LerpUnclamped(lerpPos1, lerpPos0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);

        mainCamera.transform.position = head.position;
        mainCamera.transform.rotation = head.rotation;
    }

    private void FixedUpdate()
    {
        transform.position = lerpPos0;

        Move();
        Iterate();
        Collide();

        lerpPos1 = lerpPos0;
        lerpPos0 = transform.position;
    }

    private void Move()
    {
        transform.eulerAngles = new Vector3(0f, rotation.x, 0f);

        var kb = Keyboard.current;
        var input = new Vector2
        {
            x = kb.dKey.ReadValue() - kb.aKey.ReadValue(),
            y = kb.wKey.ReadValue() - kb.sKey.ReadValue(),
        };

        var target = transform.TransformVector(input.x, 0f, input.y) * moveSpeed;
        force += (target - velocity) / Mathf.Max(Time.fixedDeltaTime, accelerationTime);
    }

    private void Update()
    {
        var lookDelta = Vector2.zero;
        lookDelta += Mouse.current.delta.ReadValue() * mouseSensitivity;
        
        rotation += lookDelta;
        rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
        head.position = transform.position + Vector3.up * (collisionHeight - 0.1f);
        head.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);

        if (currentInteractable != null)
        {
            LookingAt = currentInteractable;
            var ray = new Ray(head.position, head.forward);
            currentInteractable.Drag(this, ray);

            if (!Mouse.current.leftButton.isPressed)
            {
                currentInteractable.Release(this);
                currentInteractable = null;
            }
        }
        else
        {
            var ray = new Ray(head.position, head.forward);
            if (Physics.Raycast(ray, out var hit, interactDistance))
            {
                LookingAt = hit.collider.GetComponentInParent<IInteractable>();
                if (LookingAt != null)
                {
                    var m = Mouse.current;
                    if (m.leftButton.isPressed)
                    {
                        currentInteractable = LookingAt;
                        currentInteractable.Press(this, new Ray(head.position, head.forward));
                    }

                    var scroll = m.scroll.ReadValue();
                    if (Mathf.Abs(scroll.y) > float.Epsilon)
                    {
                        LookingAt.Increment(this, scroll.y);
                    }
                }
            }
            else
            {
                LookingAt = null;
            }
        }
    }

    private void Iterate()
    {
        transform.position += velocity * Time.deltaTime;
        velocity += force * Time.deltaTime;
        force = new Vector3(0f, -9.81f, 0f);
    }

    private void Collide()
    {
        groundCast(0f, 0f);
        groundCast(1f, 0f);
        groundCast(-1f, 0f);
        groundCast(0f, 1f);
        groundCast(0f, -1f);

        collider.isTrigger = true;
        collider.height = collisionHeight;
        collider.radius = collisionRadius;
        collider.center = new Vector3(0f, (collisionHeight + stepHeight) / 2f, 0f);

        var bounds = collider.bounds;
        var broad = Physics.OverlapBox(bounds.center, bounds.extents * 1.1f, Quaternion.identity, collisionMask);
        foreach (var other in broad)
        {
            if (other.transform.IsChildOf(transform) || other.transform == transform) continue;

            if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, other, other.transform.position, other.transform.rotation, out var normal, out var distance))
            {
                transform.position += normal * distance;
            }
        }

        void groundCast(float xOffset, float zOffset)
        {
            var ray = new Ray(transform.position + Vector3.up + new Vector3(xOffset, 0f, zOffset) * collisionRadius, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 1f, collisionMask))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                if (velocity.y < 0f) velocity.y = 0f;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var center = new Vector3(0f, collisionHeight / 2f, 0f);
        var a = collisionRadius;
        var b = (collisionHeight - collisionRadius) / 2f;
        var c = stepHeight - center.y;

        Handles.matrix = transform.localToWorldMatrix;

        Handles.color = new Color(0.5f, 1f, 0.5f, 1f);
        Handles.DrawLine(center + new Vector3(a, c, 0f), center + new Vector3(a, b, 0f));
        Handles.DrawLine(center + new Vector3(-a, c, 0f), center + new Vector3(-a, b, 0f));
        Handles.DrawLine(center + new Vector3(0f, c, a), center + new Vector3(0f, b, a));
        Handles.DrawLine(center + new Vector3(0f, c, -a), center + new Vector3(0f, b, -a));

        Handles.DrawWireDisc(center + new Vector3(0f, b, 0f), Vector3.up, collisionRadius);
        Handles.DrawWireDisc(center + new Vector3(0f, c, 0f), Vector3.up, collisionRadius);

        Handles.DrawWireArc(center + new Vector3(0f, b, 0f), Vector3.forward, Vector3.right, 180f, collisionRadius);
        Handles.DrawWireArc(center + new Vector3(0f, b, 0f), Vector3.right, -Vector3.forward, 180f, collisionRadius);

        Handles.DrawWireDisc(new Vector3(0f, stepHeight, 0f), Vector3.up, collisionRadius * 1.5f);
    }
#endif
}