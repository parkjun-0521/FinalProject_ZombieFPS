using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy
{
    void EnemyMove();       // 좀비 이동 
    void EnemyRun();        // 좀비 달리기 

    void EnemyMeleeAttack();// 좀비 공격
    void EnemyDead();       // 좀비 사망 
    void EnemyTracking(Collider other);   // 좀비 추적
}


public class EnemyController : MonoBehaviourPun, IEnemy
{


    // 좀비 걷기 속도 
    [SerializeField]
    protected float speed;

    // 좀비 달리기 속도
    [SerializeField]
    protected float runSpeed;

    // 좀비 공격
    public float meleeDelay = 4.0f;     // 공격 max 주기
    public float nextAttack = 4;        // 공격 쿨타임
    public float attackRange;


    //일정범위 지정 반지름단위
    public float rangeOut = 10f;
    //리셋 속도 5고정
    public float resetSpeed = 5f;
    //스폰 지점 지정
    public Transform enemySpawn;

    //추적거리
    public float maxTracingSpeed;


    // 컴포넌트
    protected PhotonView PV;
    public Transform playerTr;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator ani;

    public CapsuleCollider capsuleCollider;
    public SphereCollider sphereCollider;

    //좀비 최대 체력
    [SerializeField]
    public float maxHp;
    // 좀비 현재 체력 
    public float hp;
    public virtual float Hp
    {
        get
        {
            return hp;                          //그냥 반환
        }
        set
        {
            if (hp > 0)
            {
                ChangeHp(value);                   //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함             
                Debug.Log(hp);
            }
            else if (hp <= 0)
            {
                EnemyDead();
            }
        }
    }

    public float baseDamage = 10;
    public float damage;

    public bool isRangeOut = false;
    public bool shouldEvaluate = false;
    public bool isNow = false;

    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isAttack;            // 공격 하는 상태 
    protected bool isTracking;          // 추적 상태
  

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking(Collider other)
    {
        if (this == null || transform == null)
        {
            return; // 객체가 파괴되었으면 함수 종료
        }

        CancelInvoke("EnemyMove");
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.LookAt(other.transform);
        playerTr = other.transform;
        isTracking = true;

        if (hp <= 0)
        {
            transform.LookAt(Vector3.zero);
            isWalk = false;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("FireDotArea"))
        {
            Hp = -(other.GetComponent<ItemFireGrenadeDotArea>().dotDamage); //안되면 Hp=>hp 해보기 stay라 여러번 호출되니 뎀지 0.1정도 
        }
    }
    public virtual void ChangeHp(float value) { }
    public virtual void BloodEffect(Vector3 pos, Collider other = null)
    {
        Pooling.instance.GetObject("BloodSprayEffect", Vector3.zero).transform.position = pos;
    }
    public virtual void EnemyTakeDamage(float damage)
    {
        Hp = -damage;
    }
    
}
