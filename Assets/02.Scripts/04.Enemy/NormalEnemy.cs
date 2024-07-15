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
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
        //if(PV == null)
        //{
        //    Debug.LogError("PhotonView component is missing on " + gameObject.name);
        //}

        // 예: 포톤 뷰를 사용하여 특정 초기화를 수행
        //if (PV.IsMine)
        {
            
        }
    }




    void Start()
    {
        moveDelegate = RandomMove;
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
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
            rigid.AddForce(direction * resetSpeed, ForceMode.VelocityChange);


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
        throw new System.NotImplementedException();
    }






    // Start is called before the first frame update


    // Update is called once per frame

}
