using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]WheelCollider[] wheelCols;
    [SerializeField] Transform[] wheels;
    [SerializeField] float torque = 100;
    [SerializeField] float angle = 45;
    [SerializeField] bool isDrive;
    private void Update()
    {
        if(isDrive)
        {
            for (int i = 0; i < wheelCols.Length; i++)
            {
                wheelCols[i].brakeTorque = 0;
                wheelCols[i].motorTorque = InputAxisVer() * torque;
                if (i == 0 || i == 2)
                {
                    wheelCols[i].steerAngle = InputAxisHor() * angle;
                }
                Vector3 pos = transform.position;
                Quaternion rot = transform.rotation;
                wheelCols[i].GetWorldPose(out pos, out rot);
                wheels[i].position = pos;
                wheels[i].rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, 90));
            }

            if (Input.anyKey)
            {
                if (Input.GetKey(InputKeyManager.instance.GetKeyCode(InputKeyManager.KeyCodeTypes.Jump)))
                {
                    foreach (WheelCollider wheelCol in wheelCols)
                    {
                        wheelCol.brakeTorque = 2000;
                    }
                }
                else
                {
                    foreach (WheelCollider wheelCol in wheelCols)
                    {
                        wheelCol.brakeTorque = 0;
                    }
                }
            }
        }
    }

    float InputAxisVer()
    {
        if (Input.GetKey(InputKeyManager.instance.GetKeyCode(InputKeyManager.KeyCodeTypes.UpMove)))
            return -1;
        else if (Input.GetKey(InputKeyManager.instance.GetKeyCode(InputKeyManager.KeyCodeTypes.DownMove)))
            return 1;
        return 0;
    }
    float InputAxisHor()
    {
        if (Input.GetKey(InputKeyManager.instance.GetKeyCode(InputKeyManager.KeyCodeTypes.LeftMove)))
            return -1;
        else if (Input.GetKey(InputKeyManager.instance.GetKeyCode(InputKeyManager.KeyCodeTypes.RightMove)))
            return 1;
        return 0;
    }
}
