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
    public float maxTracingSpeed;       // 최대 추적 속도 

    // 좀비 공격
    public float meleeDelay = 4.0f;     // 공격 max 주기
    public float nextAttack = 4;        // 공격 쿨타임
    public float attackRange;           // 공격 거리 

    // 일정범위 지정 반지름단위
    public float rangeOut = 10f;        // walk 범위 
    //리셋 속도 5고정
    public float resetSpeed = 5f;       // 원점 돌아오는 속도 
    //스폰 지점 지정
    public Transform enemySpawn;        // 스폰 지점 

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

    public float baseDamage = 10;       // 기본 데미지 
    public float damage;                // 데미지 
        
    // 상태 변수 
    public bool isRangeOut = false;     // 범위 판정 
    public bool shouldEvaluate = false; // 추적 가능 상태 
    public bool isNow = false;          // 걷기 가능 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isAttack;            // 공격 하는 상태 
    protected bool isTracking;          // 추적 상태


    // 컴포넌트
    protected PhotonView PV;            // 포톤 뷰 
    public Transform playerTr;          // 플레이어 Transform ( 추적 ) 
    protected Rigidbody rigid;
    protected NavMeshAgent nav;         // NavMesh 
    protected Animator ani;             // 애니메이션    
    public CapsuleCollider capsuleCollider;     // 몬스터 콜라이더 
    public SphereCollider sphereCollider;       // 엘리트 몹  
    public SphereCollider EnemyLookRange;       // 인식 범위 콜라이더 

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking(Collider other)
    {
        // 객체가 파괴되었으면 함수 종료
        if (this == null || transform == null) return;

        CancelInvoke("EnemyMove");
        // 속도 초기화 
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        // 플레이어 바라보기 
        transform.LookAt(other.transform);
        // 플레이어 방향으로 전환 
        playerTr = other.transform;
        // 트레킹 가능 상태 
        isTracking = true;

        if (hp <= 0) {
            transform.LookAt(null);
            isWalk = false;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float closestDistance = Mathf.Infinity;
            Collider closestPlayer = null;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Mathf.Infinity);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = hitCollider;
                    }
                }
            }

            if (closestPlayer != null)
            {
                EnemyTracking(closestPlayer);
            }
        }
        else if(other.CompareTag("FireDotArea"))
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
