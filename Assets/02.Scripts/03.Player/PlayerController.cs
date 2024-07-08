using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

interface IPlayer {
    void PlayerMove(bool type);         // 플레이어 이동 
    void PlayerJump();                  // 플레이어 점프 
    void PlayerInteraction();           // 플레이어 상호작용
    void PlayerAttack(bool type);       // 플레이어 공격 
    void ItemThrowAway(int id);         // 아이템 버리기
    void ItemThrow();                   // 아이템 던지기 
    void PlayerDead();                  // 플레이어 사망 
}

public abstract class PlayerController : MonoBehaviourPun, IPlayer, IPunObservable
{
    // 플레이어 이동속도 
    [SerializeField]
    protected float speed;

    // 플레이어 달리기 속도
    [SerializeField]
    protected float runSpeed;

    // Player 체력 
    [SerializeField]
    protected float interactionRange = 2.0f;               //상호작용최대거리
    protected float hp = 100.0f;                              //플레이어hp 
    public abstract float Hp { get; set; }                 //hp 프로퍼티 
    protected bool isFaint = false;                        //기절했는지 true false
    public Image bloodScreen;                             //피격시 화면 빨갛게 할 이미지
    public Image healScreen;                      //치유시 화면 연두색 할 이미지
 
    
    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isJump;              // 점프 상태
    protected bool isInteraction;       // 상호작용 상태 
    protected bool isAttack;            // 공격 상태 
    protected bool isThrow;             // 아이템 버리는 상태 
    protected bool isDead;              // 사망 상태 

    
    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void ItemThrowAway( int id );
    public abstract void ItemThrow();
    public abstract void PlayerDead();
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );





    protected abstract void ChangeHp(float value);
    protected abstract void PlayerFaint();
    protected abstract IEnumerator ShowBloodScreen();
    protected abstract IEnumerator ShowHealScreen();
    public abstract void PlayerRevive();                //외부에서 접근에서 할거라 public
}
