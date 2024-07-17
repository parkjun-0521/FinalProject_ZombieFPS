using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // 좀비 이동 
    void EnemyRun();        // 좀비 달리기 
    void EnemyAttack();     // 좀비 공격
    void EnemyMeleeAttack();     // 좀비 공격
    void EnemyDead();       // 좀비 사망 
    void EnemyTracking();   // 좀비 추적
    
}


public class EnemyController : MonoBehaviourPun, IEnemy {

    
    // 좀비 걷기 속도 
    [SerializeField]
    protected float speed;

    // 좀비 달리기 속도
    [SerializeField]
    protected float runSpeed;


    protected PhotonView PV;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Vector3 origin = new Vector3(0, 0, 0);
    protected Animator ani;

    //좀비 최대 체력
    [SerializeField]
    protected float maxHp;
    // 좀비 현재 체력 
    
    protected float hp;
    public float Hp
    {
        get
        {
            return hp;                          //그냥 반환
        }
        set
        {
            if(hp > 0)
            {
                ChangeHp(value);                   //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함
                EnemyDead();                      //만약 hp를 수정했을때 체력이 0보다 작으면 기절
                Debug.Log(hp);
            }
        }
    }

    public float damage;

    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isAttack;            // 공격 하는 상태 
    protected bool isTracking;          // 추적 상태 


    public virtual void EnemyDead() { }
    public virtual void EnemyAttack() { }
    public virtual void ChangeHp(float value) { }
    public virtual void BloodEffect(Vector3 pos, Collider other = null)
    {
        Pooling.instance.GetObject("BloodSprayEffect").transform.position = pos;
    }
    public virtual void EnemyMove() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyTracking() { }
    public virtual void EnemyTakeDamage(float damage)
    {
        Hp = -damage;
    }
}
