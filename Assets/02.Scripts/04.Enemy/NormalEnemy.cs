using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnRandomMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

    //����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public float rotationSpeed = 5.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private bool isRotating = false;
    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        //PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        capsuleCollider = GetComponent<CapsuleCollider>();
       

        // ��: ���� �並 ����Ͽ� Ư�� �ʱ�ȭ�� ����
        //if (PV.IsMine)
    }

    private void OnEnable()
    {
        OnEnemyReset += ResetEnemy;
        OnEnemyMove += RandomMove;
        OnRandomMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;

        hp = maxHp;
        rigid.velocity = Vector3.zero;
        //��������Ʈ ������� �Q���� �� �ֱ�
    }

    private void OnDisable()
    {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyDead -= EnemyDead;
    }
    
    void Start()
    {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        capsuleCollider.enabled = true;
        rigid.isKinematic = false;
        nav.enabled = true;
    }
    void Update()
    {
        if (isRangeOut == true) OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
        if (isTracking)OnEnemyRun?.Invoke();
        if (nav.enabled == true)
        if (nav.isStopped == true) OnEnemyAttack?.Invoke(); // ���� ���� 
      

    }

    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    { 
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
           {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        return;
    }

    void ResetEnemy()
    {
        
            transform.LookAt(enemySpawn.position);
            if (Vector3.Distance(transform.position, enemySpawn.position) < 0.1f && shouldEvaluate)
            {
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
               OnRandomMove?.Invoke();
    }
    void RandomMove()
    {
        isWalk = true;
        ani.SetBool("isAttack", false);
        float dirX = Random.Range(-40, 40);
        float dirZ = Random.Range(-40, 40);
        Vector3 dest = new Vector3(dirX, 0, dirZ);
        targetRotation = Quaternion.LookRotation(dest - transform.position);
        StartCoroutine(RotateTowards(targetRotation));
        Vector3 toOrigin = enemySpawn.position - transform.position;
        //���� ������ ������
        if (toOrigin.magnitude > rangeOut)
        {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            //�ٽõ��ƿ���
            Vector3 direction = (enemySpawn.position - transform.position).normalized;
            rigid.AddForce(direction * resetSpeed, ForceMode.VelocityChange);
            isRangeOut = true;
            isNow = false;
        }
        //�ƴϸ� ����
        else
        {
            isNow = true;
            rigid.AddForce(dest * speed * Time.deltaTime, ForceMode.VelocityChange);
        }
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }
    IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    public override void EnemyRun()
    {
            isRun = true;
            ani.SetBool("isAttack", false);
            nav.speed = runSpeed;
            nav.destination = playerTr.position;
            if (rigid.velocity.magnitude > maxTracingSpeed)
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
            float versusDist = Vector3.Distance(transform.position, playerTr.position);
            nav.isStopped = (versusDist < attackRange) ? true : false;
    }

    public override void EnemyMeleeAttack()
    {
        nextAttack += Time.deltaTime;
        ani.SetBool("isAttack", true);
        if (nextAttack > meleeDelay)
        { 
            nextAttack = 0;
        }
        
    }

    public override void EnemyDead()
    {
        
    }
    [PunRPC]
    public void HandleEnemyDeath()
    {
        ani.SetBool("isDead", true);
        StartCoroutine(AnimationFalse("isDead"));
        capsuleCollider.enabled = false;
        rigid.isKinematic = true;
        nav.enabled = false;
        OnEnemyReset -= ResetEnemy;
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = false;
        isTracking = false;
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

    IEnumerator AnimationFalse( string str ) {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }
  
}

