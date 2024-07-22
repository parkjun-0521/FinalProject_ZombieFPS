using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossZombie : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead;

    //공격
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP 구현 
    public override float Hp
    {
        get {
            return hp;                          //그냥 반환
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함             
                Debug.Log(hp);
            }
            else if (hp <= 0) {
                OnEnemyDead?.Invoke();
            }
        }
    }

    private List<Transform> players = new List<Transform>();    // 모든 플레이어의 위치를 저장할 리스트
    public float targetChangeInterval = 10f;                    // 타겟 변경 간격(초)
    private float targetChangeTimer;
    int randPattern;
    float AttackCooltime;
    public GameObject projectilePrefab;
    public Transform bulletPos;

    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
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

            hp = maxHp;
            bloodParticle.Stop();
            // 초기에 데미지 지정 
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
        }
    }

    void Start()
    {
        if (PV.IsMine) {
            FindAllPlayers();
            ChangeTarget();                             // 시작 시 바로 대상 변경
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            targetChangeTimer = targetChangeInterval;
            bloodParticle.Stop();
            // 초기에 데미지 지정 
            // damage = 20f;
        }
    }

    void Update()
    {
        if (PV.IsMine) {
            // 어그로 전환 
            targetChangeTimer -= Time.deltaTime;
            if (targetChangeTimer <= 0) {
                ChangeTarget();
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

    // Player 탐색 
    void FindAllPlayers()
    {
        players.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects) {
            players.Add(playerObject.transform);
        }
    }

    // 어그로 전환 
    void ChangeTarget()
    {
        if (players.Count > 0) {
            int index = Random.Range(0, players.Count);
            playerTr = players[index]; // 랜덤 플레이어를 새로운 타겟으로 설정
            transform.LookAt(playerTr);
            Debug.Log("타겟 전환: " + playerTr.name);
        }
    }


    void OnTriggerEnter(Collider other)                       //총알, 근접무기...triggerEnter
    {
        if (PV.IsMine) {
            if (other.CompareTag("Bullet"))             // 총알과 trigger
            {
                Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
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

    //보통 적 NPC의 이동
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
            isWalk = true;
            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);
            transform.LookAt(dest);
            Vector3 toOrigin = enemySpawn.position - transform.position;

            //일정 범위를 나가면
            if (toOrigin.magnitude > rangeOut) {
                CancelInvoke("EnemyMove");
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //다시돌아오는
                Vector3 direction = (enemySpawn.position - transform.position).normalized;
                rigid.AddForce(direction * resetSpeed, ForceMode.VelocityChange);
                isRangeOut = true;
                OnEnemyTracking -= EnemyTracking;
            }
            //아니면 속행
            else {
                OnEnemyTracking += EnemyTracking;
                rigid.AddForce(dest * speed * Time.deltaTime, ForceMode.VelocityChange);
            }

            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    public override void EnemyRun()
    {
        if (PV.IsMine) {
            OnEnemyMove -= RandomMove;
            isRun = true;
            nav.speed = runSpeed;
            ani.SetBool("isRun", true);
            nav.destination = playerTr.position;
            if (rigid.velocity.magnitude > maxTracingSpeed)
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

            float versusDist = Vector3.Distance(transform.position, playerTr.position);

            nav.isStopped = (versusDist < 18f) ? true : false;

            AttackCooltime += Time.deltaTime;

            if (AttackCooltime > 3f) {
                if ((playerTr.position - transform.position).magnitude > 25f) {
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
                Debug.Log("공격");
                if (randPattern < 3) {
                    //randPattern = Random.Range(0, 3);   // 기본 공격 패턴 선택
                    randPattern = 3;
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

    // 기본 패턴 1,2,3
    IEnumerator BossPattern1()
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack1", true);
        yield return new WaitForSeconds(0.5f);
        OnEnemyAttack += EnemyMeleeAttack;
        ani.SetBool("isAttack1", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern2()
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack2", true);
        yield return new WaitForSeconds(0.5f);
        OnEnemyAttack += EnemyMeleeAttack;
        ani.SetBool("isAttack2", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern3()
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack3", true);
        damage = 30f;                               // 증가 데미지
        yield return new WaitForSeconds(0.5f);
        OnEnemyAttack += EnemyMeleeAttack;
        damage = baseDamage;                        // 원래 데미지
        ani.SetBool("isAttack3", false);
        nav.isStopped = false;
    }


    IEnumerator BossPattern4()                  // 토사물 뱉기 
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack5", true);

        for (int i = 0; i < 30; i++) {
            LaunchProjectile(playerTr.position); // 플레이어의 현재 위치를 향해 발사
            yield return new WaitForSeconds(0.1f); // 각 투사체 발사 간의 간격
        }

        yield return new WaitForSeconds(0.4f);
        AttackCooltime = 0;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyRun += EnemyRun;
        ani.SetBool("isAttack5", false);
        nav.isStopped = false;
        randPattern = 0;
    }
    void LaunchProjectile( Vector3 target ) {
        GameObject projectile = Instantiate(projectilePrefab, bulletPos.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        Vector3 firingDirection = (target - bulletPos.position).normalized;
        rb.velocity = firingDirection * 30f; // 속도 설정
    }


    // 일정 거리 이상 멀어지면 특수 패턴 4,5
    IEnumerator BossPattern5()                  // 날아오기 
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        ani.SetBool("isAttack4", true);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = playerTr.position;
        float duration = 0.5f; 
        float elapsedTime = 0;
        while (elapsedTime < duration) {
            // 특정 위치로 이동 
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition;   // 최종 위치에 정확히 배치
        rigid.velocity = Vector3.zero;      // 속도 초기화

        yield return new WaitForSeconds(0.5f);
        AttackCooltime = 0;
        OnEnemyAttack += EnemyMeleeAttack; // 공격 가능 상태로 복귀
        OnEnemyRun += EnemyRun;
        ani.SetBool("isAttack4", false);  // 애니메이션 종료
        nav.isStopped = false;            // 네비게이션 이동 재개
        randPattern = 0;                  // 패턴 초기화
    }

    public override void EnemyDead()
    {
        if (hp <= 0 && PV.IsMine) {
            ani.SetBool("isDeath", true);
        }
    }

    public override void ChangeHp(float value)
    {
        hp += value;
        if (value > 0) {
            //좀비가 체력회복할일은 없겠지만 나중에 보스 or 엘리트 좀비가 주변몹 회복할수도있으니 확장성때매 놔둠
            //힐 좀비 주변에 +모양 파티클생성
        }
        else if (value < 0) {
            //공격맞은거
        }
    }
}