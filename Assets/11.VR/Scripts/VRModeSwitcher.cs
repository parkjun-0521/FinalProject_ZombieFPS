using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class VRModeSwitcher : MonoBehaviour
{
    public GameObject vrRig; // VR Rig 객체를 참조합니다.

    private void Start()
    {
        // VR 모드를 자동으로 활성화하지 않도록 설정합니다.
        // StartXR()는 사용자가 버튼을 클릭할 때 호출됩니다.
        StopXR();
    }

    public void EnterVRMode()
    {
        StartCoroutine(StartXR());
    }

    public void ExitVRMode()
    {
        StopXR();
    }

    private IEnumerator StartXR()
    {
        Debug.Log("Starting XR...");

        var xrManager = XRGeneralSettings.Instance.Manager;
        xrManager.InitializeLoaderSync();

        if (xrManager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed.");
        }
        else
        {
            xrManager.StartSubsystems();
            Debug.Log("XR started successfully.");

            
        }

        yield return null;
    }

    private void StopXR()
    {
        Debug.Log("Stopping XR...");
        var xrManager = XRGeneralSettings.Instance.Manager;
        xrManager.StopSubsystems();
        xrManager.DeinitializeLoader();

        // VR Rig 비활성화
        if (vrRig != null)
        {
            vrRig.SetActive(false);
        }
    }
}
