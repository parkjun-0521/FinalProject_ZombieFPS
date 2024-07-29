using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteRangeEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;


    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private Vector3 moveDirection;
    private bool isMoving = false;
    //����
    public Transform attackPos;
    public float attackPrefabSpeed = 30.0f;

    public GameObject rangeProjectile;

    public GameObject[] player;
    // HP ���� 
    public override float Hp
    {
        get
        {
            return hp;                       
        }
        set
        {
            if (hp > 0)
            {
                ChangeHp(value);                        
                Debug.Log(hp);
            }
            else if (hp <= 0)
            {
                OnEnemyDead?.Invoke();
            }
        }
    }

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (PV.IsMine)
        {
            OnEnemyReset += ResetEnemy;
            OnEnemyMove += RandomMove;
            OnEnemyTracking += EnemyTracking;
            OnEnemyRun += EnemyRun;
            OnEnemyAttack += EnemyRangeAttack;
            OnEnemyDead += EnemyDead;

            hp = maxHp;
            ani.SetBool("isDead", false);
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void OnDisable()
    {
        if (PV.IsMine)
        {
            OnEnemyReset -= ResetEnemy;
            OnEnemyMove -= RandomMove;
            OnEnemyTracking -= EnemyTracking;
            OnEnemyRun -= EnemyRun;
            OnEnemyAttack -= EnemyRangeAttack;
            OnEnemyDead -= EnemyDead;
        }
    }


    void Start()
    {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        // �ʱ⿡ ������ ���� 
        // damage = 20f;
        rigid.isKinematic = false;
        nav.enabled = true;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isRangeOut == true) OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
            if (isTracking) OnEnemyRun?.Invoke();                   // �߰� �� �޸��� 
            if (nav.isStopped == true) OnEnemyAttack?.Invoke();     // ���� ���� 
            if (isMoving)
            {
                // �̵� �� ȸ�� ������Ʈ
                Vector3 moveDirection = (nav.destination - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(moveDirection);

                // ȸ��
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                // �̵�
                Vector3 moveDelta = moveDirection * speed * Time.deltaTime;
                rigid.MovePosition(transform.position + moveDelta);

                // �̵� �� �ӵ��� �ʱ�ȭ
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
            }

        }
        
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
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }
        return;
    }

    //���� �� NPC�� �̵�
    public override void EnemyMove()
    {
        if (PV.IsMine)
        {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy()
    {
        if (PV.IsMine)
        {
            Vector3 dest = new Vector3();
            transform.LookAt(dest);
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
    }

    void RandomMove()
    {
        if (PV.IsMine)
        {
            isWalk = true;
            ani.SetBool("isAttack", false);

            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);

            // ��ǥ ȸ�� ����
            targetRotation = Quaternion.LookRotation(dest);

            Vector3 toOrigin = enemySpawn.position - transform.position;

            // ���� ������ ������
            if (toOrigin.magnitude > rangeOut / 2)
            {
                CancelInvoke();
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;

                // �ٽ� ���ƿ��� ���� ���� �� �̵�
                Vector3 direction = (enemySpawn.position - transform.position).normalized;
                StartCoroutine(ReturnToOrigin(direction));

                isRangeOut = true;
                isNow = false;
            }
            else
            {
                isNow = true;

                // NavMeshAgent�� ����Ͽ� �̵�
                Vector3 targetPosition = transform.position + dest;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
                {
                    nav.SetDestination(hit.position);
                }
                else
                {
                    Debug.LogWarning("Failed to find valid random destination on NavMesh");
                }
            }
        }
    }
    IEnumerator ReturnToOrigin(Vector3 direction)
    {
        nav.enabled = false; // NavMeshAgent ��Ȱ��ȭ

        while (Vector3.Distance(transform.position, enemySpawn.position) > 0.1f)
        {
            Vector3 newPosition = transform.position + direction * resetSpeed * Time.deltaTime;
            rigid.MovePosition(newPosition);
            yield return null;
        }

        // NavMeshAgent Ȱ��ȭ �� ��� ����
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            nav.enabled = true;
            nav.Warp(hit.position); // ������Ʈ�� NavMesh�� ��Ȯ�� ��ġ

            // NavMeshAgent�� Ȱ��ȭ�� ���¿����� Resume ȣ��
            if (nav.isOnNavMesh)
            {
                nav.isStopped = false;
            }
            nav.SetDestination(enemySpawn.position);
        }
        else
        {
            Debug.LogError("Failed to place agent on NavMesh after returning to origin");
        }

        isRangeOut = false;
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
        if (PV.IsMine)
        {
            isRun = true;
            ani.SetBool("isAttack", false);

            // NavMeshAgent ����
            nav.speed = runSpeed;
            nav.destination = playerTr.position;

            // Rigidbody�� NavMeshAgent�� �ӵ��� ����ȭ
            Vector3 desiredVelocity = nav.desiredVelocity;

            // �̵� ����� �ӵ��� ����
            rigid.velocity = Vector3.Lerp(rigid.velocity, desiredVelocity, Time.deltaTime * runSpeed);

            // �ӵ� ����
            if (rigid.velocity.magnitude > maxTracingSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
            }

            // ���� ���� ������ ���߱�
            float versusDist = Vector3.Distance(transform.position, playerTr.position);
            if (versusDist < attackRange)
            {
                rigid.velocity = Vector3.zero;
                nav.isStopped = true;
            }
            else
            {
                nav.isStopped = false;
            }
        }
    }
    public void EnemyRangeAttack()
    {
        if(PV.IsMine)
        {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay)
            {
                //ani.SetBool("isIdle", false);
                ani.SetBool("isAttack", true);
                photonView.RPC("RPCEnemyRangeAttack", RpcTarget.AllBuffered);
                nextAttack = 0;
                StartCoroutine(AnimReset("isAttack"));
            }
        }
    }
    [PunRPC]
    void RPCEnemyRangeAttack()
    {
        Vector3 attackDir = (playerTr.position - transform.position).normalized;
        //GameObject zombieRangeAtkPrefab = Pooling.instance.GetObject("EliteRangeZombieProjectile");
        //zombieRangeAtkPrefab.transform.position = attackPos.position;
        //zombieRangeAtkPrefab.transform.rotation = Quaternion.identity;
        GameObject zombieRangeAtkPrefab = Instantiate(rangeProjectile, attackPos.position, Quaternion.identity);
        zombieRangeAtkPrefab.GetComponent<Rigidbody>().AddForce(attackDir * attackPrefabSpeed, ForceMode.Impulse);

    }


    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void HandleEnemyDeath()
    {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyRangeAttack;
        isWalk = false;

        ani.SetBool("isDead", true);
        OnEnemyDead -= EnemyDead;
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

    IEnumerator AnimReset(string animString = null)
    {
        yield return new WaitForSeconds(0.5f);
        ani.SetBool(animString, false);
    }
}
