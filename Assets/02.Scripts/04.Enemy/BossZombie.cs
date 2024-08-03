using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossZombie : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnRandomMove, OnEnemyAttack, OnEnemyDead, OnChangeTarget;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

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


    private bool isMoving = false;

    public GameObject projectilePrefab;
    public Transform bulletPos;

    void Awake()
    {
        // 레퍼런스 초기화 
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        if (PV.IsMine) {
            OnRandomMove += RandomMove;             // 랜덤 방향전환 이동 
            OnEnemyTracking += EnemyTracking;
            OnEnemyAttack += EnemyMeleeAttack;
            OnEnemyDead += EnemyDead;
            bloodParticle.Stop();
            capsuleCollider.enabled = true;
        }
    }

    void OnDisable()
    {
        if (PV.IsMine) {
            OnRandomMove -= RandomMove;             // 랜덤 방향전환 이동 
            OnEnemyTracking -= EnemyTracking;
            OnEnemyAttack -= EnemyMeleeAttack;
            OnEnemyDead -= EnemyDead;
        }
    }

    void Start()
    {
        if (PV.IsMine) {
            FindAllPlayers();
            ChangeTarget();                             // 시작 시 바로 대상 변경
            RandomMove();

            targetChangeTimer = targetChangeInterval;
            capsuleCollider.enabled = true;
            rigid.isKinematic = false;
            bloodParticle.Stop();
            hp = maxHp;

            SphereCollider lookRangeCollider = EnemyLookRange; // EnemyLookRange 콜라이더 참조
            int weaponLayer = LayerMask.NameToLayer("Weapon"); // 'Weapon' 레이어 이름에 해당하는 레이어 인덱스 가져오기

            // 모든 무기 콜라이더를 찾아 EnemyLookRange와의 충돌을 무시
            GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");
            foreach (var weapon in weapons) {
                Collider[] weaponColliders = weapon.GetComponentsInChildren<Collider>();
                foreach (var collider in weaponColliders) {
                    Physics.IgnoreCollision(lookRangeCollider, collider, true);
                }
            }
        }
    }

    void Update()
    {
        if (PV.IsMine) {
            // 어그로 전환 
            if (hp <= 0) return;

            targetChangeTimer -= Time.deltaTime;
            if (targetChangeTimer <= 0) {
                OnChangeTarget?.Invoke();
                targetChangeTimer = targetChangeInterval;
            }

            if (isTracking && playerTr != null) {
                Vector3 directionToPlayer = (playerTr.position - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                if (nav.remainingDistance <= nav.stoppingDistance) {
                    nav.isStopped = false;
                    nav.SetDestination(playerTr.position);
                }

                float versusDist = Vector3.Distance(transform.position, playerTr.position);
                if (versusDist < attackRange && !isAttack) {
                    Debug.Log("Player in attack range");
                    EnemyMeleeAttack();
                }
            }
            else if (!isTracking && !isAttack) {
                if (!isWalk) {
                    RandomMove();
                }
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
        if (other.CompareTag("Bullet")) {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon")) {
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
    void RandomMove()
    {
        if (isTracking) return;
        ani.SetBool("isWalk", true);
        Debug.Log("RandomMove called");

        StartCoroutine(ResteWalk());

        float dirX = Random.Range(-50, 50);
        float dirZ = Random.Range(-50, 50);
        Vector3 dest = new Vector3(dirX, 0, dirZ);

        targetRotation = Quaternion.LookRotation(dest);

        Vector3 targetPosition = transform.position + dest;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas)) {
            nav.SetDestination(hit.position);
        }
    }
    IEnumerator ResteWalk()
    {
        isWalk = true;
        curMoveTime = 0;
        yield return new WaitForSeconds(3f);
        isWalk = false;
        RandomMove(); // 다시 랜덤 이동 호출
    }

    public override void EnemyRun()
    {
        if (PV.IsMine) {
            if (!isTracking) return;

            ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.BossRun1)){
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.BossRun1);
            }

            nav.speed = runSpeed;
            nav.SetDestination(playerTr.position);

            Vector3 desiredVelocity = nav.desiredVelocity;

            rigid.velocity = Vector3.Lerp(rigid.velocity, desiredVelocity, Time.deltaTime * runSpeed);

            if (rigid.velocity.magnitude > maxTracingSpeed) {
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
            }

            float versusDist = Vector3.Distance(transform.position, playerTr.position);

            if (versusDist < attackRange) {
                rigid.velocity = Vector3.zero;
                nav.isStopped = true;
            }
            else {
                nav.isStopped = false;
            }

            AttackCooltime += Time.deltaTime;

            if (AttackCooltime > 3f) {
                if ((playerTr.position - transform.position).magnitude > 25f) {
                    meleeDelay = 1;
                    randPattern = 4;
                    nav.isStopped = true;
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
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;

        ani.SetBool("isAttack1", true);

        if(ani.GetBool("isDeath"))
            yield break;
        isWalk = false;

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack3)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack3);
        }

        yield return new WaitForSeconds(3f);

        OnEnemyAttack += EnemyMeleeAttack;
        OnRandomMove += RandomMove;
        OnEnemyTracking += EnemyTracking;

        meleeDelay = 2;
        isWalk = true;

        ani.SetBool("isAttack1", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern2()                  // 마구찍기
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;

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
        OnRandomMove += RandomMove;
        OnEnemyTracking += EnemyTracking;

        meleeDelay = 4;
        isWalk = true;

        ani.SetBool("isAttack2", false);
        nav.isStopped = false;
    }
    IEnumerator BossPattern3()                      // 꼬리치기
    {
        OnEnemyAttack -= EnemyMeleeAttack;
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;

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
        OnRandomMove += RandomMove;
        OnEnemyTracking += EnemyTracking;

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
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;

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
        OnRandomMove += RandomMove;
        OnEnemyTracking += EnemyTracking;

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
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = true;
        isTracking = true;
        EnemyLookRange.enabled = false;
        nav.isStopped = true;
        rigid.isKinematic = true;
        capsuleCollider.enabled = false;
        capsuleCollider.enabled = false;
        rigid.isKinematic = true;
    }
    public override void ChangeHp( float value ) {
        photonView.RPC("EliteRangeChangeHpRPC", RpcTarget.AllBuffered, value);
    }

    [PunRPC]
    void EliteRangeChangeHpRPC( float value ) {
        hp += value;
        EnemyDead();
    }

}