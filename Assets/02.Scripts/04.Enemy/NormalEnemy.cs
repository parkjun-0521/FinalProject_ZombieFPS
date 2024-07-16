using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    public delegate void MoveDelegate();
    public MoveDelegate moveDelegate;

    //�������� ���� ����������
    public float rangeOut = 10f;
    //���� �ӵ� 5����
    public float resetSpeed = 5f;
    InputKeyManager keyManager;
    //�����Ÿ�
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


    //����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        //PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();

        //if(PV == null)
        //{
        //    Debug.LogError("PhotonView component is missing on " + gameObject.name);
        //}

        // ��: ���� �並 ����Ͽ� Ư�� �ʱ�ȭ�� ����
        //if (PV.IsMine)
    }

    private void OnEnable()
    {
        hp = maxHp;
        //��������Ʈ ������� �Q���� �� �ֱ�
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

    
    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().scriptableObject.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
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




    //���� �� NPC�� �̵�
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
        //���� ������ ������
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            Debug.Log("reset:ING");
            //�ٽõ��ƿ���
            Vector3 direction = (Vector3.zero - transform.position).normalized;
            rigid.AddForce(direction * resetSpeed , ForceMode.VelocityChange);
            isRangeOut = true;
            isNow = false;
        }
        //�ƴϸ� ����
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
