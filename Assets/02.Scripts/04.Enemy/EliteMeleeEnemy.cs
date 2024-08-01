using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMeleeEnemy : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;


    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private Vector3 moveDirection;
    private bool isMoving = false;

    //����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP ���� 
    public override float Hp {
        get {
            return hp;                          //�׳� ��ȯ
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����             
                Debug.Log(hp);
            }
            else if(hp <= 0) {
                OnEnemyDead?.Invoke();
            }
        }
    }

    public bool isDead;         // RPC ����ȭ�� Bool ���� 

    void Awake() {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnEnable() {
        if (PV.IsMine) {
            OnEnemyReset += ResetEnemy;
            OnEnemyMove += RandomMove;
            OnEnemyTracking += EnemyTracking;
            OnEnemyRun += EnemyRun;
            OnEnemyAttack += EnemyMeleeAttack;
            OnEnemyDead += EnemyDead;

            hp = maxHp;
            ani.SetBool("isDead", false);
            bloodParticle.Stop();
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void OnDisable() {
        if (PV.IsMine) {
            OnEnemyReset -= ResetEnemy;
            OnEnemyMove -= RandomMove;
            OnEnemyTracking -= EnemyTracking;
            OnEnemyRun -= EnemyRun;
            OnEnemyAttack -= EnemyMeleeAttack;
            OnEnemyDead -= EnemyDead;
        }
    }


    void Start() {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        bloodParticle.Stop();
        capsuleCollider.enabled = true;
        sphereCollider.enabled = true;
        rigid.isKinematic = false;
        // �ʱ⿡ ������ ���� 
        damage = 20f;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isRangeOut == true) OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
            if (isTracking) OnEnemyRun?.Invoke();           // �߰� �� �޸��� 
            if (nav.isStopped == true) OnEnemyAttack?.Invoke();        // ���� ����
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
    void OnTriggerEnter( Collider other )                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet")){
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        return;
    }

    //���� �� NPC�� �̵�
    public override void EnemyMove() {
        if (PV.IsMine) {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy() {
        if (PV.IsMine) {
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

    void RandomMove() {
        if (PV.IsMine) {
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
            }

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk)) {
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

    public override void EnemyRun() {
        if (PV.IsMine) {
            isRun = true;
            ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
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

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_run)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_run);
            }
        }
    }

    public override void EnemyMeleeAttack() {
        if (PV.IsMine) {
            ani.SetBool("isAttack", true);
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay) {
                /*if(playerTr.GetComponent<Player>().Hp <= 0) {
                    playerTr = null;
                }*/
                StartCoroutine(AttackExit());
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack2)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack2);
                }
                nextAttack = 0;
            }

        }
    }
    IEnumerator AttackExit()
    {
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        yield return new WaitForSeconds(2f);
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;
        OnEnemyAttack += EnemyMeleeAttack;
        ani.SetBool("isAttack", false);

    }

    public override void EnemyDead() {
        if (hp <= 0 && !isDead) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            ani.SetBool("isDead", true);
            
            for (int i = 0; i < 4; i++) {
                GameObject splitEnemy = Pooling.instance.GetObject("Zombie1", transform.position);
                splitEnemy.transform.position = transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2));
                splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // �п����� �ɷ�ġ ( ���� ���̵� ���� )
                splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // �п����� �ɷ�ġ
                splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // �п����� �ɷ�ġ
                                                                                            // ������ ����Ʈ �߰�
                if (splitEnemy.GetComponent<NormalEnemy>().enemySpawn == null) {
                    splitEnemy.GetComponent<NormalEnemy>().enemySpawn = this.transform;
                }
                bloodParticle.Play();
                damage = 50f;
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_explosion)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_explosion);
                }

            }
        }
    }
    [PunRPC]
    public void HandleEnemyDeath() {
        hp = 0;
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = false;
        isDead = true;
        rigid.isKinematic = true;
        capsuleCollider.enabled = false;
        sphereCollider.enabled = false;
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
        }
        OnEnemyDead -= EnemyDead;
    }

    [PunRPC]
    void EliteMeleeChangeHpRPC(float value)
    {
        hp += value;
        EnemyDead();
    }
    public override void ChangeHp(float value)
    {
        if(photonView.IsMine)
        photonView.RPC("EliteMeleeChangeHpRPC", RpcTarget.AllBuffered, value);
    }
}
