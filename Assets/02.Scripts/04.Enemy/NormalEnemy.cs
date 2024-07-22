using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead;

    public delegate void MoveDelegate();
    public MoveDelegate moveDelegate;

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
        OnEnemyReset += ResetEnemy;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;

        hp = maxHp;
        //��������Ʈ ������� �Q���� �� �ֱ�
    }

    private void OnDisable() {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyDead -= EnemyDead;
    }

    void Start()
    {
        moveDelegate = RandomMove;
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
    }
    void Update()
    {
        if (isRangeOut == true) OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
        if (isWalk) OnEnemyTracking?.Invoke();      // �÷��̾� �߰� 
        if (isTracking) OnEnemyRun?.Invoke();           // �߰� �� �޸��� 
        if (nav.isStopped == true) OnEnemyAttack?.Invoke();        // ���� ���� 
    }

    
    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
        }
        else if(other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }
       
        return;
    }

    void ResetEnemy() {

         transform.LookAt(enemySpawn.position);
        if (Vector3.Distance(transform.position, enemySpawn.position) < 0.1f && shouldEvaluate) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            OnEnemyTracking += EnemyTracking;
            shouldEvaluate = false;
            isRangeOut = false;
        }
        shouldEvaluate = true;

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
        Vector3 toOrigin = enemySpawn.position - transform.position;
        //���� ������ ������
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            Debug.Log("reset:ING");
            //�ٽõ��ƿ���
            Vector3 direction = (enemySpawn.position - transform.position).normalized;
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
    public override void EnemyRun() {
        isRun = true;

        nav.speed = runSpeed;
        nav.destination = playerTr.position;
        if (rigid.velocity.magnitude > maxTracingSpeed)
            rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

        float versusDist = Vector3.Distance(transform.position, playerTr.position);

        nav.isStopped = (versusDist < 1f) ? true : false;
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
