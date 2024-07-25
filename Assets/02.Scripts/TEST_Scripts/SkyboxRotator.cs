using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    // Skybox 회전 속도 (degrees per second)
    public float rotationSpeed = 1.0f;

    void Update()
    {
        // 현재 Skybox의 rotation 값을 가져옴
        float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");

        // 현재 rotation 값에 deltaTime을 곱한 rotationSpeed를 더함
        currentRotation += rotationSpeed * Time.deltaTime;

        // Skybox의 rotation 값을 업데이트
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}
