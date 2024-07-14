using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{

    



    //일정범위 지정 반지름단위
    public float rangeOut=10f;
    //리셋 속도 5고정
    public float resetSpeed=5f;
    InputKeyManager keyManager;
    //추적거리
    public float rad = 3f;
    public float distance = 5f;
    public LayerMask layermask;

    private Transform playerTr;
    

    bool isRangeOut = false;
<<<<<<< Updated upstream
    bool shouldEvaluate = true;
    bool isNow = true;
    bool isTracing = false;

=======
    bool shouldEvaluate = false;
    bool isNow = false;

    //공격
    public GameObject attackColliderPrefab;
    public Transform attackPoint;
    public float attackDuration = 0.5f;
>>>>>>> Stashed changes

    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
 
    }




    void Start()
    {
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
       
        if (isWalk&&isNow)
        {
            EnemyTracking();
        }
        if(isTracing)
        {
            
            EnemyRun();
        }
<<<<<<< Updated upstream
      
=======
        if(nav.isStopped== true)
        {
            EnemyMeleeAttack();
        }
>>>>>>> Stashed changes

    }
   

    //보통 적 NPC의 이동
    public override void EnemyMove()
    {
        isWalk = true;
        float dirX = Random.Range(-40, 40);
        float dirZ = Random.Range(-40, 40);
        Vector3 dest = new Vector3(dirX, 0, dirZ);
        transform.LookAt(dest);
        Vector3 toOrigin = origin - transform.position;
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove"); 
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            //Debug.Log("reset:ING");
            Vector3 direction = (Vector3.zero - transform.position).normalized;
            rigid.AddForce(direction * resetSpeed , ForceMode.VelocityChange);
           
            isRangeOut = true;
            isNow = false;
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        else
        {
            //Debug.Log("Move");
            rigid.AddForce(dest * speed * Time.deltaTime,ForceMode.VelocityChange);
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
        rigid.velocity = Vector3.zero;
    }

    public override void EnemyRun()
    {
        isRun = true;
        nav.speed = runSpeed;
        nav.destination = playerTr.position;
        float versusDist = Vector3.Distance(transform.position, playerTr.position);
        if(versusDist<3.3f)
        {
            nav.isStopped = true;
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
            isTracing = true;
        }
     }

    public override void EnemyMeleeAttack()
    {
<<<<<<< Updated upstream


=======
        GameObject attackCollider = Instantiate(attackColliderPrefab, attackPoint.position, attackPoint.rotation);
        Destroy(attackCollider, attackDuration); // 일정 시간이 지나면 collider 제거
>>>>>>> Stashed changes
    }

    public override void EnemyDead()
    {
        throw new System.NotImplementedException();
    }


    

    

    // Start is called before the first frame update


    // Update is called once per frame

}
