using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // 좀비 이동 
    void EnemyRun();        // 좀비 달리기 
    void EnemyAttack();     // 좀비 공격
    void EnemyMeleeAttack();// 좀비 공격
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

    // 좀비 공격
    public float meleeDelay = 4.0f;     // 공격 max 주기
    public float nextAttack = 4;        // 공격 쿨타임

    //일정범위 지정 반지름단위
    public float rangeOut = 10f;
    //리셋 속도 5고정
    public float resetSpeed = 5f;

    //추적거리
    public float rad = 3f;
    public float distance = 5f;
    public LayerMask layermask;
    public float maxTracingSpeed;

    // 컴포넌트
    public Transform playerTr;
    protected PhotonView PV;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator ani;

    protected Vector3 origin = new Vector3(0, 0, 0);

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

    public bool isRangeOut = false;
    public bool shouldEvaluate = false;
    public bool isNow = false;

    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isAttack;            // 공격 하는 상태 
    protected bool isTracking;          // 추적 상태 

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyAttack() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking() {
        Vector3 skyLay = new Vector3(transform.position.x, 10, transform.position.z);
        RaycastHit hit;
        bool isHit = Physics.SphereCast(skyLay, rad, Vector3.down, out hit, distance, layermask);

        if (isHit) {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            transform.LookAt(hit.transform);
            isTracking = true;
        }
    }
    public virtual void ChangeHp( float value ) { }
    public virtual void BloodEffect( Vector3 pos, Collider other = null ) {
        Pooling.instance.GetObject("BloodSprayEffect").transform.position = pos;
    }
}
