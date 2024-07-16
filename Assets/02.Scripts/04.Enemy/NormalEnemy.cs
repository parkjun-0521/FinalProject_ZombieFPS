using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    public delegate void MoveDelegate();
    public MoveDelegate moveDelegate;

    //일정범위 지정 반지름단위
    public float rangeOut = 10f;
    //리셋 속도 5고정
    public float resetSpeed = 5f;
    InputKeyManager keyManager;
    //추적거리
    public float rad = 3f;
    public float distance = 5f;
    public LayerMask layermask;
    public float maxTracingSpeed;

    private Transform playerTr;

    public float meleeDelay = 4.0f;
    float nextAttack = 4;


    bool isRangeOut = false;
    bool shouldEvaluate = false;
    bool isNow = false;


    //공격
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    void Awake()
    {
        // 레퍼런스 초기화 
        //PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();

        //if(PV == null)
        //{
        //    Debug.LogError("PhotonView component is missing on " + gameObject.name);
        //}

        // 예: 포톤 뷰를 사용하여 특정 초기화를 수행
        //if (PV.IsMine)
    }

    private void OnEnable()
    {
        hp = maxHp;
        //델리게이트 사망에서 뻈으니 다 넣기
    }


    void Start()
    {
        moveDelegate = RandomMove;
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }
    void Update()
    {

        Vector3 enemyPos = transform.position;
        Vector3 enemyDir = transform.forward;

        if (isRangeOut == true)
        {
            Vector3 dest = new Vector3();
            transform.LookAt(dest);
            if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f && shouldEvaluate)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                InvokeRepeating("EnemyMove", 0.5f, 3.0f);
                isNow = true;
                shouldEvaluate = false;
                isRangeOut = false;
            }
            shouldEvaluate = true;

        }
        if (isWalk && isNow)
        {
            EnemyTracking();

        }
        if (isTracking)
        {
            EnemyRun();
        }


        if(nav.isStopped== true)
        {
            EnemyMeleeAttack();
        }



    }

    
    void OnTriggerEnter(Collider other)                       //총알, 근접무기...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // 총알과 trigger
        {
            Hp = -(other.GetComponent<Bullet>().scriptableObject.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
        {
            Hp = -10;
            BloodEffect(transform.position);
        }
        else if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Hp = -damage;
        }
        else
            return;
    }




    //보통 적 NPC의 이동
    public override void EnemyMove()
    {
        moveDelegate?.Invoke();
    }
    void RandomMove()
    {
        isWalk = true;

        float dirX = Random.Range(-40, 40);
        float dirZ = Random.Range(-40, 40);
        Vector3 dest = new Vector3(dirX, 0, dirZ);
        transform.LookAt(dest);
        Vector3 toOrigin = origin - transform.position;
        //일정 범위를 나가면
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            Debug.Log("reset:ING");
            //다시돌아오는
            Vector3 direction = (Vector3.zero - transform.position).normalized;
            rigid.AddForce(direction * resetSpeed , ForceMode.VelocityChange);
            isRangeOut = true;
            isNow = false;
        }
        //아니면 속행
        else
        {
            Debug.Log("Move");

            isNow = true;
            rigid.AddForce(dest * speed * Time.deltaTime, ForceMode.VelocityChange);
        }

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }
    public override void EnemyRun()
    {
        isRun = true;
        
        
        nav.speed = runSpeed;
        nav.destination = playerTr.position;
        if(rigid.velocity.magnitude > maxTracingSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

        }
        
        float versusDist = Vector3.Distance(transform.position, playerTr.position);
        if (versusDist < 3.3f)
        {
            nav.isStopped = true;
        }
        else
        {
            nav.isStopped = false;
        }


    }
    public override void EnemyTracking()
    {

        Vector3 skyLay = new Vector3(transform.position.x, 10, transform.position.z);
        RaycastHit hit;
        bool isHit = Physics.SphereCast(skyLay, rad, Vector3.down, out hit, distance, layermask);


        if (isHit)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            transform.LookAt(hit.transform);
            isTracking = true;
        }


    }

    public override void EnemyMeleeAttack()
    {
        nextAttack += Time.deltaTime;
        if (nextAttack > meleeDelay)
        {
            GameObject attackCollider = Instantiate(attackColliderPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(attackCollider, 0.1f);
            Debug.Log("ATtak");
            nextAttack = 0;
        }
      

    }

    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            ani.SetTrigger("isDead");
            //델리게이트 다른거 다 빼기
            //?초후에
            //gameObject.SetActive(false); 
        }
    }


    public override void ChangeHp(float value)
    {
        hp += value;
        if (value > 0)
        {
            //좀비가 체력회복할일은 없겠지만 나중에 보스 or 엘리트 좀비가 주변몹 회복할수도있으니 확장성때매 놔둠
            //힐 좀비 주변에 +모양 파티클생성
        }
        else if (value < 0)
        {
            //공격맞은거
            //ani.setTrigger("피격모션");
        }
    }


}
