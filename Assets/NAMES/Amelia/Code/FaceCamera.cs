using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - transform.position;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}