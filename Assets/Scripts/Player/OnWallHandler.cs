using UnityEngine;

public class OnWallHandler : MonoBehaviour
{
    [SerializeField] private Transform ground;

    float minX, maxX, minZ, maxZ;

    private void Start()
    {
        Vector3 groundSize = ground.localScale * 10;
        minX = ground.position.x - groundSize.x / 2;
        maxX = ground.position.x + groundSize.x / 2;
        minZ = ground.position.z - groundSize.z / 2;
        maxZ = ground.position.z + groundSize.z / 2;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            transform.position = pos;
        }
    }
}
