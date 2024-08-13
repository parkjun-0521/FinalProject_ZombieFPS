/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRUIRay : MonoBehaviour
{
    // 1. �켱 �ش� ��ũ��Ʈ�� �̱������� �������.
    public static VRUIRay instance;


    // 2. ���콺 ������ ������ ���ӿ�����Ʈ�� �غ��Ѵ�.
    // VR ������ ��Ʈ�ѷ�
    public Transform rightHand;
    // ���콺 �����͸� ��ü�� �̹���
    public Transform dot;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // 3. Ray�� ����ؼ� dot(���콺������)�� Ȱ��ȭ�Ѵ�.
        Ray ray = new Ray(rightHand.position, rightHand.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                dot.gameObject.SetActive(true);
                dot.position = hit.point;
            }
            else
            {
                dot.gameObject.SetActive(false);
            }


            // 4. dot�� �浹 �� �� �� Ŭ���� �� �ֵ��� �Ѵ�.
            // ���� ���� Ȱ��ȭ ���¸�
            if (dot.gameObject.activeSelf)
            {
                if (OVRInput.GetDown(OVRInput.Button.Any))
                {
                    // ��ư ��ũ��Ʈ�� �����´�
                    Button btn = hit.transform.GetComponent<Button>();
                    // ���� btn�� null�� �ƴ϶��
                    if (btn != null)
                    {
                        btn.onClick.Invoke();
                    }
                }
            }
        }
        else
        {
            dot.gameObject.SetActive(false);
        }
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class VRUIRay : MonoBehaviour
{
    // �̱������� �����
    public static VRUIRay instance;

    // VR ��Ʈ�ѷ� �� ����������
    public XRRayInteractor rayInteractor;
    public Transform dot;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // dot�� ��Ȱ��ȭ
        dot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                dot.gameObject.SetActive(true);
                dot.position = hit.point;

                // ��ư Ŭ�� ó��
                if (Input.GetButtonDown("Fire1")) // �Ǵ� OVRInput.GetDown(OVRInput.Button.Any)���� Fire1���� ����
                {
                    Button btn = hit.transform.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.Invoke();
                    }
                }
            }
            else
            {
                dot.gameObject.SetActive(false);
            }
        }
        else
        {
            dot.gameObject.SetActive(false);
        }
    }
}
