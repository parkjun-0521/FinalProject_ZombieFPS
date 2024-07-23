using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossZombie : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead, OnChangeTarget;

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
    bool isAppear = false;

    public GameObject projectilePrefab;
    public Transform bulletPos;
    public CapsuleCollider capsuleCollider;

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
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
            ani.SetBool("isAppear", true);
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
                if (isWalk) OnEnemyTracking?.Invoke();
                if (isTracking) OnEnemyRun?.Invoke();
                if (nav.isStopped == true) OnEnemyAttack?.Invoke();
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
            Debug.Log("����3");
            Debug.Log("Ÿ�� ��ȯ: " + playerTr.name);
        }
    }


    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (PV.IsMine) {
            if (!isAppear) return;

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
            else if (other.CompareTag("Grenade")) {
                Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            }
            return;
        }
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
    }

    void RandomMove()
    {
        if (PV.IsMine) {
            if (ani.GetBool("isAppear")) {
                OnEnemyReset -= ResetEnemy;
                OnEnemyMove -= RandomMove;
                OnEnemyTracking -= EnemyTracking;
                OnEnemyRun -= EnemyRun;
                OnEnemyAttack -= EnemyMeleeAttack;
                OnEnemyDead -= EnemyDead;
                OnChangeTarget -= ChangeTarget;
                StartCoroutine(AppearEnemy());
                return;
            }

            isWalk = true;
            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);
            transform.LookAt(dest);
            Vector3 toOrigin = enemySpawn.position - transform.position;
            ani.SetBool("isWalk", true);

            //���� ������ ������
            if (toOrigin.magnitude > rangeOut) {
                CancelInvoke("EnemyMove");
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //�ٽõ��ƿ���
                Vector3 direction = (enemySpawn.position - transform.position).normalized;
                rigid.AddForce(direction * resetSpeed, ForceMode.VelocityChange);
                isRangeOut = true;
                OnEnemyTracking -= EnemyTracking;
            }
            //�ƴϸ� ����
            else {
                OnEnemyTracking += EnemyTracking;
                rigid.AddForce(dest * speed * Time.deltaTime, ForceMode.VelocityChange);
            }

            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    IEnumerator AppearEnemy() {
        yield return new WaitForSeconds(12f);
        ani.SetBool("isAppear", false);
        OnEnemyReset += ResetEnemy;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;
        OnChangeTarget += ChangeTarget;
        isAppear = true;
    }

    public override void EnemyRun()
    {
        if (PV.IsMine) {
            OnEnemyMove -= RandomMove;
            transform.LookAt(playerTr);
            Debug.Log("����2");
            isRun = true;
            nav.speed = runSpeed;
            ani.SetBool("isRun", true);
            nav.destination = playerTr.position;
            if (rigid.velocity.magnitude > maxTracingSpeed)
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

            float versusDist = Vector3.Distance(transform.position, playerTr.position);

            nav.isStopped = (versusDist < 17f) ? true : false;

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
                Debug.Log("����");
                if (randPattern < 4) {
                    randPattern = Random.Range(0, 4);   // �⺻ ���� ���� ����
                }
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
    IEnumerator BossPattern1()
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.SetBool("isAttack1", true);

        if(ani.GetBool("isDeath"))
            yield break;
        isWalk = false;

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
    IEnumerator BossPattern2()
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;

        ani.SetBool("isAttack2", true);

        if (ani.GetBool("isDeath"))
            yield break;
        isWalk = false;

        yield return new WaitForSeconds(15f);
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;

        meleeDelay = 4;
        isWalk = true;

        ani.SetBool("isAttack2", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern3()
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
        GameObject projectile = Pooling.instance.GetObject("EliteRangeZombieProjectile");
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

    public override void EnemyDead()
    {
        if (hp <= 0 && PV.IsMine) {
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
        }
    }

    public override void ChangeHp(float value)
    {
        hp += value;
        if (value > 0) {
            //���� ü��ȸ�������� �������� ���߿� ���� or ����Ʈ ���� �ֺ��� ȸ���Ҽ��������� Ȯ�强���� ����
            //�� ���� �ֺ��� +��� ��ƼŬ����
        }
        else if (value < 0) {
            //���ݸ�����
        }
    }
}