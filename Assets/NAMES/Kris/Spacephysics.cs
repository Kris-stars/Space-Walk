
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class SpaceRigidbodyMovement : MonoBehaviour
{
    [Header("VR References")]
    public Transform vrCamera;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Grip Inputs")]
    public InputActionReference leftGrip;
    public InputActionReference rightGrip;

    [Header("Wall Grabbing")]
    public LayerMask wallLayer;
    public float grabRadius = 0.08f;
    public float gripStrength = 50f;
    public float gripDamping = 10f;
    public float pushMultiplier = 1.5f;

    [Header("Force Field")]
    public Transform forceFieldCenter;
    public float forceFieldRadius = 20f;
    public float pullStrength = 3f;

    [Header("Speed")]
    public float maxSpeed = 8f;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Transform activeHand;
    private Vector3 handAnchor;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        UpdateBodyCollider();
        HandleWallGrip();

        if (activeHand == null)
        {
            ApplyForceField();
        }

        // Only limit speed when not attached.
        if (activeHand == null &&
            rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void UpdateBodyCollider()
    {
        if (vrCamera == null)
            return;

        Vector3 head =
            transform.InverseTransformPoint(vrCamera.position);

        float height = Mathf.Clamp(
            head.y, 0.5f, 2.2f
        );

        capsule.height = height;

        capsule.center = new Vector3(
            head.x,
            height / 2f,
            head.z
        );
    }

    void HandleWallGrip()
    {
        bool leftPressed =
            leftGrip != null &&
            leftGrip.action.IsPressed();

        bool rightPressed =
            rightGrip != null &&
            rightGrip.action.IsPressed();

        // Find a wall when the player presses Grip.
        if (activeHand == null)
        {
            if (leftPressed && TouchingWall(leftHand))
            {
                StartGrip(leftHand);
            }
            else if (rightPressed &&
                     TouchingWall(rightHand))
            {
                StartGrip(rightHand);
            }
        }

        if (activeHand == null)
            return;

        bool stillPressed =
            activeHand == leftHand
            ? leftPressed
            : rightPressed;

        if (!stillPressed)
        {
            // Keep the movement gained while pulling.
            rb.linearVelocity *= pushMultiplier;

            activeHand = null;
            return;
        }

        // Pull the player's body to keep
        // the grabbing hand near its wall anchor.
        Vector3 difference =
            handAnchor - activeHand.position;

        Vector3 acceleration =
            difference * gripStrength
            - rb.linearVelocity * gripDamping;

        rb.AddForce(
            acceleration,
            ForceMode.Acceleration
        );
    }

    bool TouchingWall(Transform hand)
    {
        if (hand == null)
            return false;

        return Physics.CheckSphere(
            hand.position,
            grabRadius,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    void StartGrip(Transform hand)
    {
        activeHand = hand;

        handAnchor = hand.position;

        rb.linearVelocity = Vector3.zero;
    }

    void ApplyForceField()
    {
        if (forceFieldCenter == null)
            return;

        Vector3 playerPosition =
            vrCamera != null
            ? vrCamera.position
            : transform.position;

        Vector3 direction =
            forceFieldCenter.position - playerPosition;

        if (direction.magnitude <= forceFieldRadius)
            return;

        rb.AddForce(
            direction.normalized * pullStrength,
            ForceMode.Acceleration
        );
    }

    void OnDrawGizmosSelected()
    {
        if (forceFieldCenter == null)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            forceFieldCenter.position,
            forceFieldRadius
        );
    }
}
