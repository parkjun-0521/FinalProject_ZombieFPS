using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{

    private PhotonView PV;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private Vector3 origin = new Vector3(0, 0, 0);
    private Animator ani;




    //일정범위 지정 반지름단위
    public float rangeOut=10f;
    //리셋 속도 10고정
    public float resetSpeed=10f;
    InputKeyManager keyManager;
    //추적거리
    public float rad = 3f;
    public float distance = 5f;
 
    bool isRangeOut = false;
    bool shouldEvaluate = true;


 
    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
    }

  


    void Start()
    {
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
    }
    void Update()
    {
        Vector3 enemyPos = transform.position;
        Vector3 enemyDir = transform.forward;
        RaycastHit hit;
        bool isHit = Physics.SphereCast(enemyPos, rad, Vector3.up, out hit, distance);
        Debug.DrawRay(transform.position, enemyDir * distance, Color.red);
        Debug.DrawRay(transform.position, transform.up * distance, Color.red);

        if (isRangeOut == true)
        {
            Vector3 dest = new Vector3();
            transform.LookAt(dest);
            if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                isRangeOut = false;
            }
            if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f && shouldEvaluate)
            {
                InvokeRepeating("EnemyMove", 0.5f, 3.0f);
                shouldEvaluate = false;
            }
            shouldEvaluate = true;
        }
        if (isHit)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            Debug.Log("CircleCast hit: " + hit.collider.name);

        }

    }
   

    //보통 적 NPC의 이동
    public override void EnemyMove()
    {
        isWalk = true;
        float dirX = Random.Range(-50, 50);
        float dirZ = Random.Range(-50, 50);
        Vector3 dest = new Vector3(dirX, 0, dirZ);
        transform.LookAt(dest);
        Vector3 toOrigin = origin - transform.position;
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            Debug.Log("reset:ING");
            Vector3 direction = (Vector3.zero - transform.position).normalized;
            rigid.AddForce(direction * resetSpeed , ForceMode.VelocityChange);
            if (ani != null)
            {
                ani.SetTrigger("Walk");
            }
            isRangeOut = true;
            rigid.velocity = Vector3.zero;
        }
        else
        {
            Debug.Log("Move");
            if (ani != null)
            {
                ani.SetTrigger("Walk");
            }
            rigid.AddForce(dest * speed * Time.deltaTime,ForceMode.VelocityChange);
            rigid.velocity = Vector3.zero;
        }
        rigid.velocity = Vector3.zero;
    }

    public override void EnemyRun()
    {
        isRun = true;
        nav.speed = runSpeed;
        if (ani != null)
        {
            ani.SetTrigger("Run");
        }

    }
    public override void EnemyTracking(Vector3 _targetPos)
    {
        isTracking = true;
        


    }

    public override void EnemyAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void EnemyDaed()
    {
        throw new System.NotImplementedException();
    }


    

    

    // Start is called before the first frame update


    // Update is called once per frame

}
