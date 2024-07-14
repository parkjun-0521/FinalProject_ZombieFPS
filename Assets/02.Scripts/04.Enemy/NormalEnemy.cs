using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{





    //�������� ���� ����������
    public float rangeOut=10f;
    //���� �ӵ� 5����
    public float resetSpeed=5f;
    InputKeyManager keyManager;
    //�����Ÿ�
    public float rad = 3f;
    public float distance = 5f;
    public LayerMask layermask;

    private Transform playerTr;
    

    bool isRangeOut = false;
    bool shouldEvaluate = true;
    bool isNow = true;
    bool isTracing = false;



    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        
    }

    private void OnEnable()
    {
        hp = maxHp;
        //��������Ʈ ������� �Q���� �� �ֱ�
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
      

    }
    
    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().scriptableObject.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            //Hp = -(other.GetComponent<Weapon>().attackdamage)
            BloodEffect(transform.position);
        }
        else if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().Hp = -damage;
        }
        else
            return;
    }

    //���� �� NPC�� �̵�
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

    public override void EnemyAttack()
    {


    }

    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            ani.SetTrigger("isDead");
            //��������Ʈ �ٸ��� �� ����
            //?���Ŀ�
            //gameObject.SetActive(false); 
        }
    }

    public override void ChangeHp(float value)
    {
        hp += value;
        if (value > 0)
        {
            //���� ü��ȸ�������� �������� ���߿� ���� or ����Ʈ ���� �ֺ��� ȸ���Ҽ��������� Ȯ�强���� ����
            //�� ���� �ֺ��� +��� ��ƼŬ����
        }
        else if (value < 0)
        {
            //���ݸ�����
            //ani.setTrigger("�ǰݸ��");
        }
    }
}
