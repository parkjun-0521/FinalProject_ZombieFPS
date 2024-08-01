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

    // 랜덤 타겟팅 
    private List<Transform> players = new List<Transform>();    // 모든 플레이어의 위치를 저장할 리스트
    public float targetChangeInterval = 10f;                    // 타겟 변경 간격(초)
    private float targetChangeTimer;

    // 랜덤 패턴 
    int randPattern;
    float AttackCooltime;

    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // 목표 회전
    private bool isMoving = false;

    public GameObject projectilePrefab;
    public Transform bulletPos;

    void Awake()
    {
        // 레퍼런스 초기화 
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
            OnChangeTarget -= ChangeTarget;
        }
    }

    void Start()
    {
        if (PV.IsMine) {
            FindAllPlayers();
            ChangeTarget();                             // 시작 시 바로 대상 변경
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            targetChangeTimer = targetChangeInterval;
            capsuleCollider.enabled = true;
            rigid.isKinematic = false;
            bloodParticle.Stop();
            hp = maxHp;
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
                OnChangeTarget?.Invoke();
                targetChangeTimer = targetChangeInterval;
            }

            if (playerTr != null) {
                if (isRangeOut == true) OnEnemyReset?.Invoke();
                if (isTracking) OnEnemyRun?.Invoke();
                if (nav.isStopped == true) OnEnemyAttack?.Invoke();
            }
            // 회전과 이동 처리
            if (isMoving)
            {
                // 이동 중 회전 업데이트
                Vector3 moveDirection = (nav.destination - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(moveDirection);

                // 회전
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                // 이동
                Vector3 moveDelta = moveDirection * speed * Time.deltaTime;
                rigid.MovePosition(transform.position + moveDelta);

                // 이동 중 속도를 초기화
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
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
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack6)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack6);
            }
        }
    }

  
    void OnTriggerEnter(Collider other)                       //총알, 근접무기...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // 총알과 trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossHit)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossHit);
            }
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
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
            if (nav.isOnNavMesh)
            {
                nav.isStopped = true; // 먼저 멈춤
                nav.ResetPath(); // 경로 초기화
                nav.isStopped = false; // 다시 시작
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

            // 목표 회전 설정
            targetRotation = Quaternion.LookRotation(dest);

            Vector3 toOrigin = enemySpawn.position - transform.position;

            // 일정 범위를 나가면
            if (toOrigin.magnitude > rangeOut / 2)
            {
                CancelInvoke();
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;

                // 다시 돌아오는 방향 설정 및 이동
                Vector3 direction = (enemySpawn.position - transform.position).normalized;
                StartCoroutine(ReturnToOrigin(direction));

                isRangeOut = true;
                isNow = false;
            }
            else {
                isNow = true;
                // NavMeshAgent를 사용하여 이동
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
        nav.enabled = false; // NavMeshAgent 비활성화

        while (Vector3.Distance(transform.position, enemySpawn.position) > 0.1f)
        {
            Vector3 newPosition = transform.position + direction * resetSpeed * Time.deltaTime;
            rigid.MovePosition(newPosition);
            yield return null;
        }

        // NavMeshAgent 활성화 및 경로 설정
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            nav.enabled = true;
            nav.Warp(hit.position); // 에이전트를 NavMesh에 정확히 배치

            // NavMeshAgent가 활성화된 상태에서만 Resume 호출
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

            // NavMeshAgent 설정
            nav.speed = runSpeed;
            nav.destination = playerTr.position;

            // Rigidbody와 NavMeshAgent의 속도를 동기화
            Vector3 desiredVelocity = nav.desiredVelocity;

            // 이동 방향과 속도를 조절
            rigid.velocity = Vector3.Lerp(rigid.velocity, desiredVelocity, Time.deltaTime * runSpeed);

            // 속도 제한
            if (rigid.velocity.magnitude > maxTracingSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
            }

            // 공격 범위 내에서 멈추기
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
                    randPattern = Random.Range(0, 4);   // 기본 공격 패턴 선택
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

    // 기본 패턴 1,2,3
    IEnumerator BossPattern1()          // 물기 
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
    IEnumerator BossPattern2()                  // 마구찍기
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
    IEnumerator BossPattern3()                      // 꼬리치기
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
        damage = 5f;                               // 증가 데미지

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
        damage = baseDamage;                        // 원래 데미지
    }


    IEnumerator BossPattern4()                  // 토사물 뱉기 
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
            LaunchProjectile(playerTr.position); // 플레이어의 현재 위치를 향해 발사
            yield return new WaitForSeconds(0.1f); // 각 투사체 발사 간의 간격   
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
        rb.velocity = firingDirection * 30f; // 속도 설정
    }


    // 일정 거리 이상 멀어지면 특수 패턴 4,5
    IEnumerator BossPattern5()                  // 날아오기 
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
            // 특정 위치로 이동 
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPosition;   // 최종 위치에 정확히 배치
        rigid.velocity = Vector3.zero;      // 속도 초기화
        meleeDelay = 4f;
        if (ani.GetBool("isDeath"))
            yield break;
        yield return new WaitForSeconds(0.5f);
        AttackCooltime = 0;
        OnEnemyAttack += EnemyMeleeAttack; // 공격 가능 상태로 복귀
        OnEnemyRun += EnemyRun;
        ani.SetBool("isAttack5", false);  // 애니메이션 종료
        nav.isStopped = false;            // 네비게이션 이동 재개
        randPattern = 0;                  // 패턴 초기화
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