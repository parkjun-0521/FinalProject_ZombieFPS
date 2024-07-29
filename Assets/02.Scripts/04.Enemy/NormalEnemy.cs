using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnRandomMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

    // ����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private Vector3 moveDirection;
    private bool isMoving = false;

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        capsuleCollider = GetComponent<CapsuleCollider>();
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
        if (PV.IsMine)
        {
            if (isRangeOut) OnEnemyReset?.Invoke();
            if (isTracking) OnEnemyRun?.Invoke();
            if (nav.enabled && nav.isStopped) OnEnemyAttack?.Invoke();
            // ȸ���� �̵� ó��
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


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);
            other.gameObject.SetActive(false);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        else if (other.CompareTag("Weapon"))
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        else if (other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
    }

    void ResetEnemy()
    {
        if (PV.IsMine)
        {
            if (nav.isOnNavMesh)
            {
                nav.isStopped = true; // ���� ����
                nav.ResetPath(); // ��� �ʱ�ȭ
                nav.isStopped = false; // �ٽ� ����
            }

            transform.LookAt(enemySpawn.position);
            if (Vector3.Distance(transform.position, enemySpawn.position) < 0.1f && shouldEvaluate)
            {
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                InvokeRepeating("EnemyMove", 0.5f, 3.0f);
                shouldEvaluate = false;
                isRangeOut = false;
            }
            shouldEvaluate = true;
        }
            
    }

    // ���� �� NPC�� �̵�
    public override void EnemyMove()
    {
        if (PV.IsMine)
        {
            OnRandomMove?.Invoke();
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
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
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
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_run))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_run);
            }
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


    public override void EnemyMeleeAttack()
    {
        if (PV.IsMine)
        {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay)
            {
                ani.SetBool("isAttack", true);
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack))
                {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack);
                }
                StartCoroutine(AttackExit());
                nextAttack = 0;
            }
        }

    }
    IEnumerator AttackExit()
    {
        yield return new WaitForSeconds(2f);
        ani.SetBool("isAttack", false);

    }
    public override void EnemyDead()
    {
        if (hp <= 0 && photonView.IsMine)
        {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
        }
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
        }
        else if (value < 0)
        {
        }
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }
}
