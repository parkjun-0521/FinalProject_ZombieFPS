using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour {
    [SerializeField]
    public float rotCamXAxisSpeed = 1; // X축 카메라 회전속도 
    [SerializeField]
    public float rotCamYAxisSpeed = 1; // Y축 카메라 회전속도

    [SerializeField]
    private float rotationSmoothness = 2f; // 회전 부드러움 정도, 낮게 설정

    private float limitMinX = -80; // 카메라 x축 회전 범위 ( 최소 ) 
    private float limitMaxX = 50; // 카메라 x축 회전 범위 ( 최대 ) 
    private float eulerAngleX;
    private float eulerAngleY;

    public Transform cameraTransform;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private void Start()
    {
        // 초기 회전 값 설정
        eulerAngleX = cameraTransform.eulerAngles.x;
        eulerAngleY = transform.eulerAngles.y;
    }

    public void UpdateRotate(float mouseX, float mouseY)
    {
        // 마우스 입력 필터링
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, 0.05f);

        // 마우스 이동에 따른 목표 회전 값 계산
        eulerAngleY += currentMouseDelta.x * rotCamYAxisSpeed; // 마우스 좌우 이동 카메라 y축 회전
        eulerAngleX -= currentMouseDelta.y * rotCamXAxisSpeed; // 마우스 상하 이동 카메라 x축 회전

        // 카메라 x축 회전 범위 지정 
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        // 목표 회전 값 설정
        Quaternion targetCameraRotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
        Quaternion targetPlayerRotation = Quaternion.Euler(0, eulerAngleY, 0);

        // 회전을 부드럽게 보간
        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, targetCameraRotation, rotationSmoothness );
        transform.rotation = Quaternion.Lerp(transform.rotation, targetPlayerRotation, rotationSmoothness );
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    public float GetEulerAngleY()
    {
        return eulerAngleY;
    }
}
