using UnityEngine;

public class FollowPositionOnly : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}
