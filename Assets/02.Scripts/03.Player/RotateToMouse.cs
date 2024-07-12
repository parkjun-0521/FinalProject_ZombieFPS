using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour {
    [SerializeField]
    private float rotCamXAxisSpeed = 10; // X축 카메라 회전속도 
    [SerializeField]
    private float rotCamYAxisSpeed = 7; // Y축 카메라 회전속도

    [SerializeField]
    private float rotationSmoothness = 10f; // 회전 부드러움 정도, 높게 설정

    private float limitMinX = -80; // 카메라 x축 회전 범위 ( 최소 ) 
    private float limitMaxX = 50; // 카메라 x축 회전 범위 ( 최대 ) 
    private float eulerAngleX;
    private float eulerAngleY;

    public Transform cameraTransform;

    private void Start() {
        // 초기 회전 값 설정
        eulerAngleX = cameraTransform.eulerAngles.x;
        eulerAngleY = transform.eulerAngles.y;
    }

    public void UpdateRotate( float mouseX, float mouseY ) {
        eulerAngleY += mouseX * rotCamYAxisSpeed; // 마우스 좌우 이동 카메라 y축 회전
        eulerAngleX -= mouseY * rotCamXAxisSpeed; // 마우스 좌우 이동 카메라 x축 회전

        // 카메라 x축 회전 범위 지정 
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        // 목표 회전 값 설정
        Quaternion targetCameraRotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
        Quaternion targetPlayerRotation = Quaternion.Euler(0, eulerAngleY, 0);

        // 회전을 부드럽게 보간, 보간 비율을 높게 설정하여 즉각적인 회전
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetCameraRotation, rotationSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetPlayerRotation, rotationSmoothness * Time.deltaTime);
    }

    private float ClampAngle( float angle, float min, float max ) {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    public float GetEulerAngleY() {
        return eulerAngleY;
    }
}
