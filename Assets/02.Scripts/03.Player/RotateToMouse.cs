using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 10;     // X�� ī�޶� ȸ���ӵ� 
    [SerializeField]
    private float rotCamYAxisSpeed = 7;     // Y�� ī�޶� ȸ���ӵ�

    private float limitMinX = -80;          // ī�޶� x�� ȸ�� ���� ( �ּ� ) 
    private float limitMaxX = 50;           // ī�޶� x�� ȸ�� ���� ( �ִ� ) 
    private float eulerAngleX; 
    private float eulerAngleY;

    public Transform cameraTransform;

    public void UpdateRotate(float mouseX, float mouseY ) {
        eulerAngleY += mouseX * rotCamYAxisSpeed;       // ���콺 �¿� �̵� ī�޶� y�� ȸ��
        eulerAngleX -= mouseY * rotCamXAxisSpeed;       // ���콺 �¿� �̵� ī�޶� x�� ȸ��

        // ī�޶� x�� ȸ�� ���� ���� 
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
