using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggingManager : MonoBehaviour
{
    //IK target(�����̴� ���)
    public Transform leftHandIK;
    public Transform rightHandIK;
    public Transform headIK;
    //controller(vr���)
    public Transform leftHandController;
    public Transform rightHandController;
    //����ī�޶�
    public Transform hmd;

    //������ �迭�� �� ����: ������+�����̼�
    public Vector3[] leftOffset;
    public Vector3[] rightOffset;
    public Vector3[] headOffset;

    private float smoothValue = 0.1f;
    public float modelHeight;




    private void LateUpdate()
    {
        MappingHandTransform(leftHandIK, leftHandController, true);
        MappingHandTransform(rightHandIK, rightHandController, false);
        MappingBodyTransform(headIK, hmd);
        MappingHeadTransform(headIK, hmd);

    }
    void MappingHandTransform(Transform ik, Transform controller, bool isLeft)
    {
        var offset = isLeft ? leftOffset : rightOffset;
        ik.position = controller.TransformPoint(offset[0]);
        ik.rotation = controller.rotation * Quaternion.Euler(offset[1]);
    }
    void MappingBodyTransform(Transform ik, Transform hmd)
    {
        this.transform.position = new Vector3(hmd.position.x, hmd.position.y - modelHeight, hmd.position.z);
        float yaw = hmd.eulerAngles.y;
        var targetRotation = new Vector3(this.transform.eulerAngles.x, yaw, this.transform.eulerAngles.z);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(targetRotation), smoothValue);
    }
    void MappingHeadTransform(Transform ik, Transform hmd)
    {
        ik.position = hmd.TransformPoint(headOffset[0]);
        ik.rotation = hmd.rotation * Quaternion.Euler(headOffset[1]);
    }





}
