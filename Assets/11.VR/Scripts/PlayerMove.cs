using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Transform xrOriginTransform; // XR Origin�� Transform�� �Ҵ�

    void Update()
    {
        float moveSpeed = 5.0f;
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        xrOriginTransform.position += new Vector3(moveX, 0, moveZ);
    }
}
