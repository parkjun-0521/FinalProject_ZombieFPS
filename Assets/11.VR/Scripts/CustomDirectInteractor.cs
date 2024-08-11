using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// XRDirectInteractor ���콺 �������� alt + Enter�� �߰�.
using UnityEngine.XR.Interaction.Toolkit;

public class CustomDirectInteractor : XRDirectInteractor
{
    void Start()
    {
        
    }

    // ��� ���� ã�� ���� XRBaseInteractor �̰� �ȿ� �Ʒ� �Լ� �����Ѵ� �̰� �������̵� ����
    //protected virtual void OnSelectEntered(SelectEnterEventArgs args)
    //{
    //    m_SelectEntered?.Invoke(args);
    //    OnSelectEntered(args.interactable);
    //}

    // ���� �Լ� �� ����(��� �������̵�) (���� : OnSelectEntering�� ������ ��������)
    // �Ű� ������ interactable�� �Ѿ��(������� ���)(��ȣ�ۿ��� ���� ��)
    // ������ ��� ������
    // ����� �ڵ� ����ȭ�� ���ϰ� ���������� �ϵ� �ڵ��ߴ�.
    protected override void OnSelectEntered(XRBaseInteractable interactable)
    {
        Debug.Log("Select Entered");
        Debug.Log("Current Object : " + interactable.name);
        if(interactable.transform.GetChild(0) != null)
        {
            interactable.transform.GetChild(0).gameObject.SetActive(true);
        }
        //base.OnSelectEntered(interactable);
    }

    // �������� ��� ������(�������� ���)
    protected override void OnSelectExited(XRBaseInteractable interactable)
    {
        Debug.Log("Select Exited");
        if (interactable.transform.GetChild(0) != null)
        {
            interactable.transform.GetChild(0).gameObject.SetActive(false);
        }
        //base.OnSelectExited(interactable);
    }
}
