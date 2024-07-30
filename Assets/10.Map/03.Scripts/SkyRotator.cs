using UnityEngine;

public class SkyRotator : MonoBehaviour
{
    // Skybox ȸ�� �ӵ� (degrees per second)
    public float skyRot = 1.0f;

    void Update()
    {
        // ���� Skybox�� rotation ���� ������
        float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");

        // ���� rotation ���� deltaTime�� ���� rotationSpeed�� ����
        currentRotation += skyRot * Time.deltaTime;

        // Skybox�� rotation ���� ������Ʈ
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}
