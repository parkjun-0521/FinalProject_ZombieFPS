using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IPlayer {
    void PlayerMove(bool type);         // 플레이어 이동 
    void PlayerJump();                  // 플레이어 점프 
    void PlayerInteraction();           // 플레이어 상호작용
    void PlayerAttack(bool type);       // 플레이어 공격 
    void ItemThrowAway(int id);         // 아이템 버리기
    void ItemThrow();                   // 아이템 던지기 
    void PlayerDead();                  // 플레이어 사망 
}

public abstract class PlayerController : MonoBehaviour , IPlayer
{
    // 플레이어 이동속도 
    [SerializeField]
    protected float speed;

    // 플레이어 달리기 속도
    [SerializeField]
    protected float runSpeed;

    // Player 체력 
    private float hp;
    public float PlayerHp {  get { return hp; } set { hp = value; } }

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
}
