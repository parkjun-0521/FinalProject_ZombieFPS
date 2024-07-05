using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5;     // X축 카메라 회전속도 
    [SerializeField]
    private float rotCamYAxisSpeed = 3;     // Y축 카메라 회전속도

    private float limitMinX = -80;          // 카메라 x축 회전 범위 ( 최소 ) 
    private float limitMaxX = 50;           // 카메라 x축 회전 범위 ( 최대 ) 
    private float eulerAngleX; 
    private float eulerAngleY;

    public void UpdateRotate(float mouseX, float mouseY ) {
        eulerAngleY += mouseX * rotCamYAxisSpeed;       // 마우스 좌우 이동 카메라 y축 회전
        eulerAngleX -= mouseY * rotCamXAxisSpeed;       // 마우스 좌우 이동 카메라 x축 회전

        //eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);
    }

/*    private float ClampAngle(float angle, float min, float max ) {

    }*/

}
