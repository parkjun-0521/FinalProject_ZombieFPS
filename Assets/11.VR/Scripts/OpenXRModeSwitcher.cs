using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class OpenXRModeSwitcher : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(StartXR());
    }

    IEnumerator StartXR()
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

    public void StopXR()
    {
        Debug.Log("Stopping XR...");
        var xrManager = XRGeneralSettings.Instance.Manager;
        xrManager.StopSubsystems();
        xrManager.DeinitializeLoader();
    }
}
