using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class VRModeSwitcher : MonoBehaviour
{
    private bool isVRMode = false;

    public void ToggleVRMode()
    {
        isVRMode = !isVRMode;
        if (isVRMode)
        {
            EnterVRMode();
        }
        else
        {
            ExitVRMode();
        }
    }

    private void EnterVRMode()
    {
        StartCoroutine(StartXR());
    }

    private void ExitVRMode()
    {
        StartCoroutine(StopXR());
    }

    private IEnumerator StartXR()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    private IEnumerator StopXR()
    {
        Debug.Log("Stopping XR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped.");
        yield return null;
    }
}
