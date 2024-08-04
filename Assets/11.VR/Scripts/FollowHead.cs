using UnityEngine;

public class FollowHead : MonoBehaviour
{
    public Transform playerHead;  // 플레이어 캐릭터의 머리 Transform
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
