using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    private PhotonView PV;
    private Rigidbody rigid;
    private CharacterController characterController;

    InputKeyManager keyManager;

    
    void Awake() {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        keyManager = InputKeyManager.instance.GetComponent<InputKeyManager>();
    }

    void OnEnable() {
        OnPlayerMove += PlayerMove;
        OnPlayerAttack += PlayerAttack;
    }
    void OnDisable() {
        OnPlayerMove -= PlayerMove;
        OnPlayerAttack -= PlayerAttack;
    }

    void Start() {
        // 변수 초기화 
    }


    void Update() {
        // 단발적인 행동 
       /* bool isAttack = Input.GetMouseButton(0);
        OnPlayerAttack?.Invoke(isAttack);*/

    }

    void FixedUpdate() {
        // delegate 등록
        if (PV.IsMine)
        {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    void OnTriggerEnter( Collider other )                       //좀비 트리거콜라이더에 enter했을때
    {
        if(other.tag == "Enemy")                                //좀비로할지 enemy로할지 쨌든 태그 상의
        {
            //hp = -(other.GetComponent<Enemy>().attackdamage)  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
        }
    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("플레이어 이동");
        if (PV.IsMine) {
            float z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;               
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;            

            transform.Translate(x, 0, z);
        }
    }

    // 플레이어 점프 
    public override void PlayerJump() {
        throw new System.NotImplementedException();
    }


    // 플레이어 상호작용 
    public override void PlayerInteraction() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit, interactionRange, LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("Item")))   //레이어 이름, 거리에대해 상의
        {
            if(hit.collider.tag == "Item")                                  //만약 아이템이면
            {
                //ex)text : 'E' 아이템줍기 ui띄워주기
                //hit.collider.GetComponent<Item>.itemCode
            }
            else if(hit.collider.tag == "Player")
            {
                if(hit.collider.GetComponent<Player>().isFaint == true)     //만약 태그가 player고 기절이 true면
                {
                    //ex)text : 'E' 플레이어 살리기 ui띄워주기
                    //slider or shader로 (slider가 편할듯) 살려주기
                }
            }
        }
    }

    // 플레이어 공격 ( 근접인지 원거리인지 판단 bool ) 
    public override void PlayerAttack( bool type ) {
        Debug.Log("공격");
    }

    // 아이템 버리기 ( 버리는 item id가져오기 )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // 아이템 던지기 ( 수류탄 ) 
    public override void ItemThrow() {
        throw new System.NotImplementedException();
    }

    // 플레이어 사망
    public override void PlayerDead() {
        throw new System.NotImplementedException();
    }
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            // 데이터 전송
            stream.SendNext(rigid.position);
            stream.SendNext(rigid.rotation);
            stream.SendNext(rigid.velocity);
        }
        else {
            // 데이터 수신
            rigid.position = (Vector3)stream.ReceiveNext();
            rigid.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }



    protected override void ChangeHp(float value)
    {
        hp += value;
        if(value > 0)
        {
            StartCoroutine(ShowHealScreen());   //힐 화면 출력
        }
        else if(value < 0)
        {
            StartCoroutine(ShowBloodScreen());  //피격화면 출력 
        }
    }

    protected override void PlayerFaint()       
    {
        if (hp <= 0)                            //만약 플레이어 체력이 0보다 작아지면
        {
            hp = 0;                             //여기서 hp를 0 으로 강제로 해줘야 부활, ui에서 편할거같음
            isFaint = true;                     //기절상태 true
            OnPlayerMove -= PlayerMove;         //움직이는거막고
            OnPlayerAttack -= PlayerAttack;     //공격도 막겠다
            //anim.setbool("isFaint", true);    //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
        }
    }

    public override void PlayerRevive()      //플레이어 부활 - 다른플레이어가 부활할때 얘의 player에 접근해서 호출 내부에선 안쓸꺼임 제세동기를만들지않는이상...
    {                                        //PlayerFaint 함수와 반대로 하면 됨
        isFaint = false;                     //기절상태 false
        OnPlayerMove += PlayerMove;          //움직이는거 더하고
        OnPlayerAttack += PlayerAttack;      //공격도 더함 
        //anim.setbool("isFaint", false);    //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
        Hp = 50;                             //부활시 반피로 변경! maxHp = 100; 을 따로 선언해서 maxHp / 2해도 되는데 풀피는 100하겠지 뭐
    }

    protected override IEnumerator ShowBloodScreen()                        //화면 붉게
    {
        bloodScreen.color = new Color(1, 0, 0, Random.Range(0.1f, 0.15f));  //시뻘겋게 변경
        yield return new WaitForSeconds(0.5f);                              //0.5f초 후에   - 이거 변수로 뺄까?
        bloodScreen.color = Color.clear;                                    //화면 정상적으로 변경!
    }
    protected override IEnumerator ShowHealScreen()                         //화면 연두색 
    {
        float curTime = Time.time;                                          //현재의 시간을 변수로 저장을하고
        healScreen.color = new Color(1, 1, 1, 1);                           //힐 하는 이미지로 변경
        while(true)                                                         //반복문 1초후에 해제
        {
            yield return new WaitForSeconds(0.1f);                          //0.1초마다           
            float lerpColor = 1 - (Time.time - curTime);                    
            healScreen.color = new Color(1, 1, 1, lerpColor);               //대충 조금씩 없어진다고 생각하면됨(러프)
            if (lerpColor <= 0)                                             //1초후에
            {
                healScreen.color = Color.clear;                             //컬러를 0000으로 해서 정상적으로 변경하고
                break;                                                      //break로 반복문 탈출
            }
        }
    }

    public override float Hp                         //hp 프로퍼티
    {
        get
        {
            return hp;                               //그냥 반환
        }
        set
        {
            ChangeHp(value);                         //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함
            PlayerFaint();                           //만약 hp를 수정했을때 체력이 0보다 작으면 기절
            Debug.Log("플레이어 hp 변경" + hp);
        }
    }

    
    [ContextMenu("프로퍼티--")]                      //TEST용 추후삭제
    void test()
    {
        Hp = -1;
    }
    [ContextMenu("프로퍼티++")]                      //TEST용 추후삭제
    void test2()
    {
        Hp = +1;
    }
}
