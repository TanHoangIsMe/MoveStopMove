using UnityEngine;

public class BillBoard : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.rotation = mainCam.transform.rotation;
    }
}
