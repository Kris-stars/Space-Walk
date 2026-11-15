using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

public class MovementGravity : MonoBehaviour
{
    [Header("References")]
    public ClimbProvider climbProvider;
    public CharacterController controller;
    public Transform vrCamera;

    [Header("Space Movement")]
    public float pushMultiplier = 1.5f;
    public float maxSpeed = 5f;
    public float smoothing = 15f;

    private Vector3 velocity;
    private Vector3 lastPosition;
    private bool wasClimbing;

    [Range(0f, 1f)]
    public float bounceAmount = 0f;

    [Header("Force Field")]
    public Transform forceFieldCenter;

    public float forceFieldRadius = 20f;
    public float pullStrength = 3f;
    public float maxPullSpeed = 8f;

    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (climbProvider == null || controller == null)
            return;

        float dt = Time.deltaTime;

        if (dt <= 0f)
            return;

        // Keep the player's collider aligned with the headset.
        if (vrCamera != null)
        {
            Vector3 head =
                transform.InverseTransformPoint(vrCamera.position);

            float height = Mathf.Clamp(head.y, 0.5f, 2.2f);

            controller.height = height;

            controller.center = new Vector3(
                head.x,
                height / 2f,
                head.z
            );
        }

        // Check whether the player is holding a climbable wall.
        bool isClimbing =
            climbProvider.climbAnchorInteractor != null;

        if (isClimbing)
        {
            if (!wasClimbing)
            {
                // Stop previous momentum when grabbing.
                velocity = Vector3.zero;
            }
            else
            {
                // Measure movement made by the Climb Provider.
                Vector3 movement =
                    transform.position - lastPosition;

                Vector3 targetVelocity = movement / dt;

                // Smooth the velocity to reduce jitter.
                float blend = 1f - Mathf.Exp(-smoothing * dt);

                velocity = Vector3.Lerp(
                    velocity,
                    targetVelocity,
                    blend
                );
            }
        }
        else
        {
            if (wasClimbing)
            {
                // Preserve velocity when Grip is released.
                velocity *= pushMultiplier;

                velocity = Vector3.ClampMagnitude(
                    velocity,
                    maxSpeed
                );
            }

            ApplyForceField(dt);

            // Float in the current direction.
            Vector3 movement = velocity * dt;

            if (movement.sqrMagnitude > 0f)
            {
                controller.Move(movement);
            }
        }

        wasClimbing = isClimbing;
        lastPosition = transform.position;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (climbProvider.climbAnchorInteractor != null)
            return;

        float dot = Vector3.Dot(velocity, hit.normal);

        if (dot < 0f)
        {
            Vector3 slide = Vector3.ProjectOnPlane(
                velocity,
                hit.normal
            );

            Vector3 bounce = Vector3.Reflect(
                velocity,
                hit.normal
            );

            velocity = Vector3.Lerp(
                slide,
                bounce,
                bounceAmount
            );
        }
    }

    void ApplyForceField(float dt)
    {
        if (forceFieldCenter == null)
            return;

        Vector3 direction =
            forceFieldCenter.position - vrCamera.position;

        float distance = direction.magnitude;

        // Inside the field: no extra force.
        if (distance <= forceFieldRadius)
            return;

        // Outside the field: accelerate inward.
        Vector3 pullDirection = direction.normalized;

        velocity += pullDirection * pullStrength * dt;

        // Limit the player's floating speed.
        velocity = Vector3.ClampMagnitude(
            velocity,
            maxPullSpeed
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
