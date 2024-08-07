using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyStang
{

public class CameraSwitcher : MonoBehaviour
{
    [Header("Virtual cameras")]
    public GameObject RearVirtualCamera;
    public GameObject FrontVirtualCamera;

    public KeyCode switchVirtualCameraKey = KeyCode.R;

    void Start() // called the first frame, when the game starts.
    {
        RearVirtualCamera.SetActive(true);
        FrontVirtualCamera.SetActive(false);
    }
    
    void Update() // called every frame
    {
        if(Input.GetKeyDown(switchVirtualCameraKey))
        {
            SwitchCamera();
        }
    }

    public void SwitchCamera()
    {
        if(RearVirtualCamera.activeInHierarchy) // if the rear virtual camera is active, switch to the front virtual camera
        {
            RearVirtualCamera.SetActive(false);
            FrontVirtualCamera.SetActive(true);
        }
        else // vice versa, switch to the rear virtual camera
        {
            RearVirtualCamera.SetActive(true);
            FrontVirtualCamera.SetActive(false);
        }
    }
}

}