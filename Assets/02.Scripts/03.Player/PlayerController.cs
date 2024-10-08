using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

interface IPlayer {
    void PlayerMove(bool type);         // 플레이어 이동 
    void PlayerJump();                  // 플레이어 점프 
    void PlayerInteraction();           // 플레이어 상호작용
    void PlayerAttack(bool type);       // 플레이어 공격 
    void WeaponSwap();                  // 무기 교체
    void PlayerFaint();                 // 플레이어 기절
    void PlayerRevive();                // 플레이어 부활
    void PlayerDead();                  // 플레이어 사망 
}

public abstract class PlayerController : MonoBehaviourPun, IPlayer, IPunObservable
{
    [Header("이동=======")]
    // 플레이어 이동속도 
    [SerializeField]
    protected float speed;

    // 플레이어 달리기 속도
    [SerializeField]
    protected float runSpeed;

    [Header("점프=======")]
    // 플레이어 점프 힘 
    [SerializeField]
    protected float jumpForce;


    [Header("체력=======")]
    // Player 체력 
    [SerializeField]
    protected float interactionRange = 2.0f;    //상호작용최대거리

    public float maxHp = 100.0f;
    protected float hp = 100.0f;                //플레이어hp
    protected float faintTime = 30.0f;         //플레이어 기절 시간 기절시간 지날시 사망

    [Header("UI======")] [SerializeField]
    protected GameObject playerReviveUI;
    [SerializeField]
    protected GameObject playerHealPackUI;
    [SerializeField]
    protected GameObject playerFaintUI;
    public float Hp                             //hp 프로퍼티
    {
        get {
            return hp;                          //그냥 반환
        }
        set {
            if(isFaint || isDead)
            {
                FaintChangeHp(value);
                return;
            }
            ChangeHp(value);                    //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함
            PlayerFaint();                      //만약 hp를 수정했을때 체력이 0보다 작으면 기절
            Debug.Log("플레이어 hp 변경" + hp);
        }
    }
    [SerializeField]
    public Image bloodScreen;                   //피격시 화면 빨갛게 할 이미지
    public Image healScreen;                    //치유시 화면 연두색 할 이미지

    [Header("공격=======")]
    public GameObject[] weapons;        // 무기 종류 ( 순서대로 넣어줘야 함, 0 : 원거리, 1 : 근접, 2 : 투척, 3 : 힐팩 )
    [HideInInspector]
    public GameObject equipWeapon;      // 들고 있는 무기 ( 스왑 시 비활성화 해주기 위함 ) 
    public int beforeWeapon = 0;
    public int[] bulletCount;
    public bool isBulletZero;
    public bool isGun = false;

    public Transform bulletPos;         // 총알 나가는 위치 
    public Transform grenadePos;

    public float attackMaxDelay = 0.1f; // 원거리 무기 딜레이 ( 무기의 딜레는 추후 weapon을 만들면 거기서 불러와서 조절 ) 
    [HideInInspector]
    public float lastAttackTime = 0.0f; // 마지막 공격 시간 

    [Header("상태변수=======")]
    public int weaponIndex = -1;        // 초기 무기 인덱스 ( 빈손 ) 
    public bool weaponSelected = false; // 무기가 선택되었는지 확인 
    public bool isAtkDistance;          // 공격 거리 ( false 원거리, true 근거리 ) 
    public bool stanceWeaponType;       // 원거리 ( false : 원거리, true : 투척 ), 근거리 ( false : 근접, true :  힐팩 ) 각각 구분 
    public bool countZero;
    public bool isFaint = false;        // 기절했는지 true false
    public bool isJump;              // 점프 상태
    protected bool isInteraction;       // 상호작용 상태 
    protected bool isAttack;            // 공격 상태 
    protected bool isThrow;             // 아이템 버리는 상태 
    public bool isShaderApplied;        // 쉐이더 활성화 
    public bool isDead; // 사망 상태 
    public bool isStart;
    public bool isLoad;
    public bool isNextStageZone;        // 다음 맵 이동 존 확인 

    // 사용 안할 꺼 같은데 예외처리에 필요할 Bool 변수 모음 
    protected bool isMove;              // 걷고있는 상태 

    public Vector3 moveForce;           // Player 이동 방향 및 힘 
    public LayerMask enemyLayer;        // Enemy Layer 변수 

    [Header("파티클=======")]
    public ParticleSystem muzzleFlashEffect;

    [Header("컴포넌트=======")]
    public PhotonView PV;               // 포톤 ( 동기화 관련 및 서버 관련 ) 
    public Rigidbody rigid;
    public Animator animator;
    public Animator handAnimator;
    public CapsuleCollider capsuleCollider;

    [Header("인벤토리=======")]
    public GameObject inventory;        // 인벤토리 UI
    public Inventory theInventory;

    [Header("에임=======")]
    public Image aiming;
    public GameObject aimingObj;

    [Header("닉네임=======")]
    public TMP_Text nickNameText;

    [HideInInspector]
    public GameObject ReadyObj;
    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void WeaponSwap();
    public abstract void PlayerFaint();
    public abstract void PlayerRevive();   

    public abstract void PlayerDead();
    // 플레이어 체력 변화 이벤트 
    public abstract void ChangeHp( float value );
    // 포톤 동기화 메소드
    public abstract void FaintChangeHp(float value);
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );
}
