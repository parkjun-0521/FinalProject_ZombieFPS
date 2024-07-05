using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5;     // X�� ī�޶� ȸ���ӵ� 
    [SerializeField]
    private float rotCamYAxisSpeed = 3;     // Y�� ī�޶� ȸ���ӵ�

    private float limitMinX = -80;          // ī�޶� x�� ȸ�� ���� ( �ּ� ) 
    private float limitMaxX = 50;           // ī�޶� x�� ȸ�� ���� ( �ִ� ) 
    private float eulerAngleX; 
    private float eulerAngleY;

    public void UpdateRotate(float mouseX, float mouseY ) {
        eulerAngleY += mouseX * rotCamYAxisSpeed;       // ���콺 �¿� �̵� ī�޶� y�� ȸ��
        eulerAngleX -= mouseY * rotCamXAxisSpeed;       // ���콺 �¿� �̵� ī�޶� x�� ȸ��

        //eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);
    }

/*    private float ClampAngle(float angle, float min, float max ) {

    }*/

}
