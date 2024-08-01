using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossZombie : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead, OnChangeTarget;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

    //����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP ���� 
    public override float Hp
    {
        get {
            return hp;                          //�׳� ��ȯ
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����             
                Debug.Log(hp);
            }
            else if (hp <= 0) {
                OnEnemyDead?.Invoke();
            }
        }
    }

    // ���� Ÿ���� 
    private List<Transform> players = new List<Transform>();    // ��� �÷��̾��� ��ġ�� ������ ����Ʈ
    public float targetChangeInterval = 10f;                    // Ÿ�� ���� ����(��)
    private float targetChangeTimer;

    // ���� ���� 
    int randPattern;
    float AttackCooltime;

    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    private bool isMoving = false;

    public GameObject projectilePrefab;
    public Transform bulletPos;

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        if (PV.IsMine) {
            OnEnemyReset += ResetEnemy;
            OnEnemyMove += RandomMove;
            OnEnemyTracking += EnemyTracking;
            OnEnemyRun += EnemyRun;
            OnEnemyAttack += EnemyMeleeAttack;
            OnEnemyDead += EnemyDead;
            OnChangeTarget += ChangeTarget;
            hp = maxHp;
            bloodParticle.Stop();
            capsuleCollider.enabled = true;
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void OnDisable()
    {
        if (PV.IsMine) {
            OnEnemyReset -= ResetEnemy;
            OnEnemyMove -= RandomMove;
            OnEnemyTracking -= EnemyTracking;
            OnEnemyRun -= EnemyRun;
            OnEnemyAttack -= EnemyMeleeAttack;
            OnEnemyDead -= EnemyDead;
            OnChangeTarget -= ChangeTarget;
        }
    }

    void Start()
    {
        if (PV.IsMine) {
            FindAllPlayers();
            ChangeTarget();                             // ���� �� �ٷ� ��� ����
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            targetChangeTimer = targetChangeInterval;
            capsuleCollider.enabled = true;
            rigid.isKinematic = false;
            bloodParticle.Stop();
            hp = maxHp;
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void Update()
    {
        if (PV.IsMine) {
            // ��׷� ��ȯ 
            targetChangeTimer -= Time.deltaTime;
            if (targetChangeTimer <= 0) {
                OnChangeTarget?.Invoke();
                targetChangeTimer = targetChangeInterval;
            }

            if (playerTr != null) {
                if (isRangeOut == true) OnEnemyReset?.Invoke();
                if (isTracking) OnEnemyRun?.Invoke();
                if (nav.isStopped == true) OnEnemyAttack?.Invoke();
            }
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
    
    // Player Ž�� 
    void FindAllPlayers()
    {
        players.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects) {
            players.Add(playerObject.transform);
        }
    }

    // ��׷� ��ȯ 
    void ChangeTarget()
    {
        if (players.Count > 0) {
            int index = Random.Range(0, players.Count);
            playerTr = players[index]; // ���� �÷��̾ ���ο� Ÿ������ ����
            transform.LookAt(playerTr);
            Debug.Log("Ÿ�� ��ȯ: " + playerTr.name);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack6)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack6);
            }
        }
    }

  
    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossHit)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossHit);
            }
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossHit)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossHit);
            }
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossHit)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossHit);
            }
        }
        return;
    }

    //���� �� NPC�� �̵�
    public override void EnemyMove()
    {
        if (PV.IsMine) {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy()
    {
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

    void RandomMove()
    {
        if (PV.IsMine) {
            isWalk = true;
            ani.SetBool("isAttack", false);
            ani.SetBool("isWalk", true);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossWalk1)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossWalk1);
            }
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
            else {
                isNow = true;
                // NavMeshAgent�� ����Ͽ� �̵�
                Vector3 targetPosition = transform.position + dest;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, 2.0f, NavMesh.AllAreas)) {
                    nav.SetDestination(hit.position);
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
        if (PV.IsMine) {
            isRun = true;
            ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossRun1)){
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossRun1);
            }

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

            AttackCooltime += Time.deltaTime;

            if (AttackCooltime > 3f) {
                if ((playerTr.position - transform.position).magnitude > 25f) {
                    meleeDelay = 1;
                    randPattern = 4;
                    nav.isStopped = true;
                    OnEnemyRun -= EnemyRun;
                }
            }
        }
    }

    public override void EnemyMeleeAttack()
    {
        if (PV.IsMine) {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay) {
                nav.isStopped = true;
                if (randPattern < 4) {
                    randPattern = Random.Range(0, 4);   // �⺻ ���� ���� ����
                }
                Debug.Log(randPattern);
                switch (randPattern) {
                    case 0:
                        StartCoroutine(BossPattern1());
                        break;
                    case 1:
                        StartCoroutine(BossPattern2());
                        break;
                    case 2:
                        StartCoroutine(BossPattern3());
                        break;
                    case 3:
                        StartCoroutine(BossPattern4());
                        break;
                    case 4:
                        StartCoroutine(BossPattern5());
                        break;
                }
                rigid.velocity = Vector3.zero;
                nextAttack = 0;
            }
        }
    }

    // �⺻ ���� 1,2,3
    IEnumerator BossPattern1()          // ���� 
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.SetBool("isAttack1", true);

        if(ani.GetBool("isDeath"))
            yield break;
        isWalk = false;

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack3)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack3);
        }

        yield return new WaitForSeconds(3f);

        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;

        meleeDelay = 2;
        isWalk = true;

        ani.SetBool("isAttack1", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern2()                  // �������
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.SetBool("isAttack2", true);

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack4)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack4);
        }
        yield return new WaitForSeconds(4f);

        if (ani.GetBool("isDeath"))
            yield break;
        isWalk = false;

        yield return new WaitForSeconds(11f);
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;

        meleeDelay = 4;
        isWalk = true;

        ani.SetBool("isAttack2", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern3()                      // ����ġ��
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.applyRootMotion = true;
        ani.SetBool("isAttack3", true);

        if (ani.GetBool("isDeath"))
            yield break;
        isWalk = false;
        damage = 5f;                               // ���� ������

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack5)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack5);
        }

        yield return new WaitForSeconds(4f);
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;

        meleeDelay = 2;
        isWalk = true;
        ani.SetBool("isAttack3", false);
        nav.isStopped = false;

        ani.applyRootMotion = false;
        damage = baseDamage;                        // ���� ������
    }


    IEnumerator BossPattern4()                  // ��繰 ��� 
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.SetBool("isAttack4", true);

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack6)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack6);
        }

        for (int i = 0; i < 60; i++) {
            if (ani.GetBool("isDeath"))
                yield break;
            LaunchProjectile(playerTr.position); // �÷��̾��� ���� ��ġ�� ���� �߻�
            yield return new WaitForSeconds(0.1f); // �� ����ü �߻� ���� ����   
        }

        isWalk = false;
        yield return new WaitForSeconds(1f);
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;

        meleeDelay = 6;
        isWalk = true;

        ani.SetBool("isAttack4", false);
        nav.isStopped = false;
    }
    void LaunchProjectile( Vector3 target ) {
        GameObject projectile = Pooling.instance.GetObject("EliteRangeZombieProjectile", Vector3.zero);
        projectile.transform.position = bulletPos.transform.position;
        projectile.transform.rotation = Quaternion.identity;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 firingDirection = (target - bulletPos.position).normalized;
        rb.velocity = firingDirection * 30f; // �ӵ� ����
    }


    // ���� �Ÿ� �̻� �־����� Ư�� ���� 4,5
    IEnumerator BossPattern5()                  // ���ƿ��� 
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack5", true);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = playerTr.position;
        float duration = 0.5f; 
        float elapsedTime = 0;

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack4)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack4);
        }

        while (elapsedTime < duration) {
            // Ư�� ��ġ�� �̵� 
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition;   // ���� ��ġ�� ��Ȯ�� ��ġ
        rigid.velocity = Vector3.zero;      // �ӵ� �ʱ�ȭ
        meleeDelay = 4f;
        if (ani.GetBool("isDeath"))
            yield break;
        yield return new WaitForSeconds(0.5f);
        AttackCooltime = 0;
        OnEnemyAttack += EnemyMeleeAttack; // ���� ���� ���·� ����
        OnEnemyRun += EnemyRun;
        ani.SetBool("isAttack5", false);  // �ִϸ��̼� ����
        nav.isStopped = false;            // �׺���̼� �̵� �簳
        randPattern = 0;                  // ���� �ʱ�ȭ
    }

    public override void EnemyDead() {
        if (hp <= 0 && PV.IsMine) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossDie1)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossDie1);
            }
        }
    }
    [PunRPC]
    public void HandleEnemyDeath() {
        ani.SetBool("isDeath", true);
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnChangeTarget -= ChangeTarget;
        isWalk = false;
        isTracking = false;
        capsuleCollider.enabled = false;
        rigid.isKinematic = true;
    }

    [PunRPC]
    void EliteRangeChangeHpRPC( float value ) {
        hp += value;
        EnemyDead();
    }
    public override void ChangeHp( float value ) {
        photonView.RPC("EliteRangeChangeHpRPC", RpcTarget.AllBuffered, value);
    }

}