using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 관련 네임스페이스 추가
using UnityEngine.XR.Interaction.Toolkit;

public class CustomGrabInteractable : XRGrabInteractable
{
    public Transform left_grab_transform;
    public Transform right_grab_transform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // 여기서 interactor는 잡은 손이다.
    // 왼손, 오른손을 다르게 작동하고 싶다면...
    // 즉, CustomGrabInteractable 는 물건/사물(Object)에 들어갈 것이다. 그런데 그 사물에 있는 잡는 위치가 다를 경우가 있다.
    // 그럴 경우에는 왼손, 오른손에 따라서 위치 포지션을 다르게 잡아줘야 한다.
    protected override void OnSelectEntered(XRBaseInteractor interactor)
    {
        // 아래 처럼 어떤 손인지 확인 가능하지만 Tag로 확인 하는것이 더 좋다.
        //if(interactor.name == "LeftHand Controller")
        if (interactor.CompareTag("Left Hand"))
        {
            // interactor 한테 어테치포인트가 있다. 이 값을 뭔가 다른걸 잡아줘도 되고,
            //interactor.attachTransform =
            // 그러나, interactor로 접근해서 손에 잡는 물건 위치를 변경하는게 아니라,
            //현재 스크립트 처럼, 이 사물한테 어테치 위치를 두개 만들어 두는게 좋다.
            this.attachTransform = left_grab_transform;
        }
        else if(interactor.CompareTag("Right Hand"))
        {
            // 이 사물에 어테치 위치를 오른쪽 그랩 위치로...
            // 이런식으로 왼손, 오른손에 다르게 접근 시킬수 있다.
            // 이런식은 VR에서 많이 쓰이는 방법으로, 어떤 물건을 왼손 오른손 양손으로 잡을때가 있다. 그 상황에 유용.
            this.attachTransform = right_grab_transform;
        }
        //base.OnSelectEntered(interactor);
    }

    protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        //base.OnSelectExited(interactor);
    }

    // 왼손으로 잡아서 트리거(검지) 버튼 눌르면 텍스트 메쉬가 나올거다. override : 재정의, 기능변경
    // 참고로, OnActivate, OnDeactivate 이벤트가 눌르고 있다고 Update 처럼 도는게 아니라...누르는 순간에 켜지고,( OnActivate 실행 )
    // 손을 놓을때 클릭 처럼 딸깍하면 자연스럽게 꺼진다.( OnDeactivate 실행 )
    // 총 게임 같은경우 글씨 대신에 총알이 나가면 됨.
    protected override void OnActivate(XRBaseInteractor interactor)
    {
        if(this.transform.GetChild(0) != null)
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
        }
        //base.OnActivate(interactor);
    }

    protected override void OnDeactivate(XRBaseInteractor interactor)
    {
        if (this.transform.GetChild(0) != null)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        //base.OnDeactivate(interactor);
    }
}
