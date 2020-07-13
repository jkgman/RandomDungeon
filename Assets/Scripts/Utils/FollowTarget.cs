using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField]
    Transform target;
    [SerializeField]
    bool lateUpdate = false;

    Vector3 lastOffset;

    void Start() {
        lastOffset = transform.position - target.position;
    }

    void Update()
    {
        if(!lateUpdate)
            UpdatePosition(lastOffset);
    }
    private void LateUpdate()
    {
        if(lateUpdate)
            UpdatePosition(lastOffset);
    }

    public void UpdatePosition(Vector3 offset)
    {
        transform.position = target.position + offset;
        lastOffset = offset;
    }
}
