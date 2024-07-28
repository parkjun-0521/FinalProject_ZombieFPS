using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour {
    [SerializeField]
    public float rotCamXAxisSpeed = 1; // X�� ī�޶� ȸ���ӵ� 
    [SerializeField]
    public float rotCamYAxisSpeed = 1; // Y�� ī�޶� ȸ���ӵ�

    [SerializeField]
    private float rotationSmoothness = 2f; // ȸ�� �ε巯�� ����, ���� ����

    private float limitMinX = -80; // ī�޶� x�� ȸ�� ���� ( �ּ� ) 
    private float limitMaxX = 50; // ī�޶� x�� ȸ�� ���� ( �ִ� ) 
    private float eulerAngleX;
    private float eulerAngleY;

    public Transform cameraTransform;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private void Start()
    {
        // �ʱ� ȸ�� �� ����
        eulerAngleX = cameraTransform.eulerAngles.x;
        eulerAngleY = transform.eulerAngles.y;
    }

    public void UpdateRotate(float mouseX, float mouseY)
    {
        // ���콺 �Է� ���͸�
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, 0.05f);

        // ���콺 �̵��� ���� ��ǥ ȸ�� �� ���
        eulerAngleY += currentMouseDelta.x * rotCamYAxisSpeed; // ���콺 �¿� �̵� ī�޶� y�� ȸ��
        eulerAngleX -= currentMouseDelta.y * rotCamXAxisSpeed; // ���콺 ���� �̵� ī�޶� x�� ȸ��

        // ī�޶� x�� ȸ�� ���� ���� 
        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        // ��ǥ ȸ�� �� ����
        Quaternion targetCameraRotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
        Quaternion targetPlayerRotation = Quaternion.Euler(0, eulerAngleY, 0);

        // ȸ���� �ε巴�� ����
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
