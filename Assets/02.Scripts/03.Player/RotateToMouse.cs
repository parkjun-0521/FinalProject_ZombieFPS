using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 10;     // X축 카메라 회전속도 
    [SerializeField]
    private float rotCamYAxisSpeed = 7;     // Y축 카메라 회전속도

    private float limitMinX = -80;          // 카메라 x축 회전 범위 ( 최소 ) 
    private float limitMaxX = 50;           // 카메라 x축 회전 범위 ( 최대 ) 
    private float eulerAngleX; 
    private float eulerAngleY;

    public Transform cameraTransform;

    public void UpdateRotate(float mouseX, float mouseY ) {
        eulerAngleY += mouseX * rotCamYAxisSpeed;       // 마우스 좌우 이동 카메라 y축 회전
        eulerAngleX -= mouseY * rotCamXAxisSpeed;       // 마우스 좌우 이동 카메라 x축 회전

        // 카메라 x축 회전 범위 지정 
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        cameraTransform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);

        transform.rotation = Quaternion.Euler(0, eulerAngleY, 0);
    }

    private float ClampAngle( float angle, float min, float max) {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    public float GetEulerAngleY() {
        return eulerAngleY;
    }
}
