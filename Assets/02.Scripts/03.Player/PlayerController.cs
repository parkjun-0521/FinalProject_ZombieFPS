using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IPlayer {
    void PlayerMove(bool type);         // 플레이어 이동 
    void PlayerJump();                  // 플레이어 점프 
    void PlayerInteraction();           // 플레이어 상호작용
    void PlayerAttack(bool type);       // 플레이어 공격 
    void WeaponSwap();                  // 무기 교체
    void ItemThrowAway(int id);         // 아이템 버리기
    void ItemThrow();                   // 아이템 던지기 
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

    // 중력 계수 ( 점프 후 착지 )
    [SerializeField]
    protected float gravity;

    [Header("체력=======")]
    // Player 체력 
    private float hp;
    public float PlayerHp {  get { return hp; } set { hp = value; } }

    [Header("공격=======")]
    public float attackMaxDelay = 0.1f; // 원거리 무기 딜레이 ( 무기의 딜레는 추후 weapon을 만들면 거기서 불러와서 조절 ) 
    [HideInInspector]
    public float lastAttackTime = 0.0f; // 마지막 공격 시간 

    [Header("상태변수=======")]
    public bool isAtkDistance;          // 공격 거리 ( false 원거리, true 근거리 ) 
    protected bool isJump;              // 점프 상태
    protected bool isInteraction;       // 상호작용 상태 
    protected bool isAttack;            // 공격 상태 
    protected bool isThrow;             // 아이템 버리는 상태 
    protected bool isDead;              // 사망 상태 

    // 사용 안할 꺼 같은데 예외처리에 필요할 Bool 변수 모음 
    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태   

    public Vector3 moveForce;           // Player 이동 방향 및 힘 
    public LayerMask enemyLayer;        // Enemy Layer 변수 

    [Header("컴포넌트=======")]
    public PhotonView PV;               // 포톤 ( 동기화 관련 및 서버 관련 ) 
    public Rigidbody rigid;             
    public CharacterController characterController;

    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void WeaponSwap();
    public abstract void ItemThrowAway( int id );
    public abstract void ItemThrow();
    public abstract void PlayerDead();
    // 포톤 동기화 메소드
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );
}
