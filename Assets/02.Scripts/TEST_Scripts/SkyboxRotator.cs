using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    // Skybox ȸ�� �ӵ� (degrees per second)
    public float rotationSpeed = 1.0f;

    void Update()
    {
        // ���� Skybox�� rotation ���� ������
        float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");

        // ���� rotation ���� deltaTime�� ���� rotationSpeed�� ����
        currentRotation += rotationSpeed * Time.deltaTime;

        // Skybox�� rotation ���� ������Ʈ
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}
