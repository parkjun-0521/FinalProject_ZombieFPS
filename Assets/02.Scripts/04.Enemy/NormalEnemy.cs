using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
   

    enum State
    {
        idle,
        randomMove,
        attack,
        chase,
        dead
    }
    [Header("ver.osj")]
    [SerializeField] State state = State.idle;
    [SerializeField] GameObject[] players;
    [SerializeField] float traceRange = 10f;  //�÷��̾� �����Ÿ�
    [SerializeField] float attackDistance = 2f;



    // ����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private Vector3 moveDirection;
    private bool isMoving = false;

    public Collider EnemyLookRange;
    
    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (state == State.dead) return;
            switch (state)
            {
                case State.idle:
                    Idle();
                    break;
                case State.randomMove:
                    RandomMove();
                    break;
                case State.chase:
                    Chase();
                    break;
                case State.attack:
                    Attack();
                    break;
            }
        }

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon"))
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
    }


    void Idle()
    {
        RandomMovePoscalculate();
    }
    bool isRandomMove;
    void RandomMovePoscalculate()
    {
        if (PV.IsMine && !isRandomMove)
        {
            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);

            targetRotation = Quaternion.LookRotation(dest);                 
            Vector3 targetPosition = transform.position + dest;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                nav.SetDestination(hit.position);
            }

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
            }
            state = State.randomMove;
            isRandomMove = true;
        }
    }

    Collider[] detectedPlayer;
    void RandomMove()
    {
        
        if((transform.position - nav.destination).magnitude < 1.3)  
        {
            state = State.idle;
            isRandomMove = false;
        }

        detectedPlayer = Physics.OverlapSphere(transform.position, traceRange, LayerMask.GetMask("LocalPlayer"));
        if(detectedPlayer.Length > 0 || hp != maxHp)  //��ó�� ���ų� �ǰ� 1�̶� �޸�
        {
            state = State.chase;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, traceRange);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    void Chase()
    {
        Transform closestPlayer = players[0].transform;
        foreach (GameObject player in players)
        {
            Vector3 playerTr = player.transform.position;
            float playerDistance = ((playerTr - transform.position).sqrMagnitude);
            if ((closestPlayer.position - transform.position).sqrMagnitude >= playerDistance)
                closestPlayer.position = playerTr;
        }
        nav.SetDestination(closestPlayer.position);
        Quaternion targetRotation = Quaternion.LookRotation(closestPlayer.position - transform.position);
        if (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if ((closestPlayer.position - transform.position).magnitude < attackDistance)
        {
            state = State.attack;
            nav.velocity = Vector3.zero;
        }
    }

    Coroutine AttackCoroutine;
    void Attack()
    {
        Transform closestPlayer = players[0].transform;
        foreach (GameObject player in players)
        {
            Vector3 playerTr = player.transform.position;
            float playerDistance = ((playerTr - transform.position).sqrMagnitude);
            if ((closestPlayer.position - transform.position).sqrMagnitude >= playerDistance)
                closestPlayer.position = playerTr;
        }
        nav.SetDestination(closestPlayer.position);
        Vector3 directionToTarget = closestPlayer.position - transform.position; // ��ǥ ���� ���
        if (directionToTarget != Vector3.zero) // ������ 0�� �ƴ� ��쿡�� ȸ�� ���
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 27f) // 1�� �̻� ���� ���� ��쿡�� ȸ��
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        
        if (AttackCoroutine == null)
        {
            AttackCoroutine = StartCoroutine(AttackCor());
        }
    }
    IEnumerator AttackCor()
    {
        ani.SetBool("isAttack", true);
        StartCoroutine(AnimationFalse("isAttack"));
        yield return new WaitForSeconds(3f); //���ݾִϸ��̼� �ð� 2.633
        state = State.chase;
        AttackCoroutine = null;
    }
    //IEnumerator ReturnToOrigin(Vector3 direction)
    //{
    //    nav.enabled = false; // NavMeshAgent ��Ȱ��ȭ

    //    while (Vector3.Distance(transform.position, enemySpawn.position) > 0.1f)
    //    {
    //        Vector3 newPosition = transform.position + direction * resetSpeed * Time.deltaTime;
    //        rigid.MovePosition(newPosition);
    //        yield return null;
    //    }

    //    // NavMeshAgent Ȱ��ȭ �� ��� ����
    //    NavMeshHit hit;
    //    if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
    //    {
    //        nav.enabled = true;
    //        nav.Warp(hit.position); // ������Ʈ�� NavMesh�� ��Ȯ�� ��ġ

    //        // NavMeshAgent�� Ȱ��ȭ�� ���¿����� Resume ȣ��
    //        if (nav.isOnNavMesh)
    //        {
    //            nav.isStopped = false;
    //        }
    //        nav.SetDestination(enemySpawn.position);
    //    }
    //    else
    //    {
    //        Debug.LogError("Failed to place agent on NavMesh after returning to origin");
    //    }

    //    isRangeOut = false;
    //}

    IEnumerator RotateTowards(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }




    //public override void EnemyRun()
    //{
    //    if (PV.IsMine)
    //    {
    //        isRun = true;
    //        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_run))
    //        {
    //            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_run);
    //        }
    //        ani.SetBool("isAttack", false);

    //        // NavMeshAgent ����
    //        nav.speed = runSpeed;
    //        nav.destination = playerTr.position;

    //        // Rigidbody�� NavMeshAgent�� �ӵ��� ����ȭ
    //        Vector3 desiredVelocity = nav.desiredVelocity;

    //        // �̵� ����� �ӵ��� ����
    //        rigid.velocity = Vector3.Lerp(rigid.velocity, desiredVelocity, Time.deltaTime * runSpeed);

    //        // �ӵ� ����
    //        if (rigid.velocity.magnitude > maxTracingSpeed)
    //        {
    //            rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
    //        }

    //        // ���� ���� ������ ���߱�
    //        float versusDist = Vector3.Distance(transform.position, playerTr.position);

    //        if (versusDist < attackRange)
    //        {
    //            rigid.velocity = Vector3.zero;
    //            nav.isStopped = true;
    //        }
    //        else
    //        {
    //            nav.isStopped = false;
    //        }
    //    }

    //}


    //public override void EnemyMeleeAttack()
    //{
    //    if (PV.IsMine)
    //    {
    //        ani.SetBool("isAttack", true);
    //        nextAttack += Time.deltaTime;
    //        if (nextAttack > meleeDelay)
    //        {
    //            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack))
    //            {
    //                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack);
    //            }
    //            StartCoroutine(AttackExit());
    //            nextAttack = 0;
    //        }
    //    }
    //}
    //IEnumerator AttackExit()
    //{
    //    yield return new WaitForSeconds(2f);
    //    ani.SetBool("isAttack", false);

    //}

    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            state = State.dead;
            ani.SetBool("isDead", true);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
            }
        }
    }


    [PunRPC]
    void NormalEnemyChangeHpRPC(float value)
    {
        hp += value;
        EnemyDead();
    }
    public override void ChangeHp(float value)
    {
        photonView.RPC("NormalEnemyChangeHpRPC", RpcTarget.AllBuffered, value);
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);

        state = State.chase;
    }
}
