using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// XRDirectInteractor 마우스 가져가서 alt + Enter로 추가.
using UnityEngine.XR.Interaction.Toolkit;

public class CustomDirectInteractor : XRDirectInteractor
{
    void Start()
    {
        
    }

    // 상속 구조 찾아 보면 XRBaseInteractor 이거 안에 아래 함수 존재한다 이걸 오버라이드 하자
    //protected virtual void OnSelectEntered(SelectEnterEventArgs args)
    //{
    //    m_SelectEntered?.Invoke(args);
    //    OnSelectEntered(args.interactable);
    //}

    // 기존 함수 재 정의(기능 오버라이드) (조심 : OnSelectEntering은 눌르고 있을때다)
    // 매개 변수로 interactable이 넘어옴(잡았을때 대상)(상호작용을 당한 넘)
    // 잡을때 기능 가져옴
    // 참고로 코드 최적화는 안하고 직관적으로 하드 코딩했다.
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

    // 놓았을때 기능 가져옴(놓았을때 대상)
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
