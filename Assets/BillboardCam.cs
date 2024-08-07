using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BillboardCam : MonoBehaviour
{ 

    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
 
}