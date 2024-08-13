using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class VRModeSwitcher : MonoBehaviour
{
    public GameObject vrRig; // VR Rig ��ü�� �����մϴ�.

    private void Start()
    {
        // VR ��带 �ڵ����� Ȱ��ȭ���� �ʵ��� �����մϴ�.
        // StartXR()�� ����ڰ� ��ư�� Ŭ���� �� ȣ��˴ϴ�.
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

        // VR Rig ��Ȱ��ȭ
        if (vrRig != null)
        {
            vrRig.SetActive(false);
        }
    }
}
