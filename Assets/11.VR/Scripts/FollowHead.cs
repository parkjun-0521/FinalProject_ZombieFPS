using UnityEngine;

public class FollowHead : MonoBehaviour
{
    public Transform playerHead;  // �÷��̾� ĳ������ �Ӹ� Transform
    private Camera vrCamera;

    void Start()
    {
        vrCamera = Camera.main;
    }

    void Update()
    {
        if (vrCamera != null && playerHead != null)
        {
            vrCamera.transform.position = playerHead.position;
            vrCamera.transform.rotation = playerHead.rotation;
        }
    }
}
