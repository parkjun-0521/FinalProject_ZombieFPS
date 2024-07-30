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
    private Quaternion targetRotation; // 목표 회전
    private Vector3 moveDirection;
    private bool isMoving = false;

    //공격
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP 구현 
    public override float Hp {
        get {
            return hp;                          //그냥 반환
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp를 value만큼 더함 즉 피해량을 양수로하면 힐이됨 음수로 해야함 여기서 화면 시뻘겋게 and 연두색도함             
                Debug.Log(hp);
            }
            else if(hp <= 0) {
                OnEnemyDead?.Invoke();
            }
        }
    }

    public bool isDead;         // RPC 동기화용 Bool 변수 

    void Awake() {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
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
            // 초기에 데미지 지정 
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
        rigid.isKinematic = false;
        // 초기에 데미지 지정 
        // damage = 20f;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isRangeOut == true) OnEnemyReset?.Invoke();         // 범위 나갔을 때 초기화 
            if (isTracking) OnEnemyRun?.Invoke();           // 추격 시 달리기 
            if (nav.isStopped == true) OnEnemyAttack?.Invoke();        // 몬스터 공격
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
    void OnTriggerEnter( Collider other )                       //총알, 근접무기...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // 총알과 trigger
            {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
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

    //보통 적 NPC의 이동
    public override void EnemyMove() {
        if (PV.IsMine) {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy() {
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

    void RandomMove() {
        if (PV.IsMine) {
            isWalk = true;
            ani.SetBool("isAttack", false);

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
            else
            {
                isNow = true;

                // NavMeshAgent를 사용하여 이동
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

    public override void EnemyRun() {
        if (PV.IsMine) {
            isRun = true;
            ani.SetBool("isAttack", false);

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
        }
    }

    public override void EnemyMeleeAttack() {
        if (PV.IsMine) {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay) {
                ani.SetBool("isAttack", true);
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack2)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack2);
                }
                Debug.Log("ATtak");
                nextAttack = 0;
            }
        }
    }

    public override void EnemyDead() {
        if (hp <= 0 && !isDead) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            ani.SetBool("isDead", true);
            
            for (int i = 0; i < 4; i++) {
                GameObject splitEnemy = Pooling.instance.GetObject("Zombie1", transform.position);
                splitEnemy.transform.position = transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2));
                splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // 분열좀비 능력치 ( 추후 난이도 조절 )
                splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // 분열좀비 능력치
                splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // 분열좀비 능력치
                                                                                            // 터지는 이펙트 추가
                if (splitEnemy.GetComponent<NormalEnemy>().enemySpawn == null) {
                    splitEnemy.GetComponent<NormalEnemy>().enemySpawn = this.transform;
                }
                bloodParticle.Play();
                damage = 50f;
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_explosion);

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
        AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
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
        photonView.RPC("EliteMeleeChangeHpRPC", RpcTarget.AllBuffered, value);
    }
    //public override void ChangeHp( float value ) {
    //    hp += value;
    //    if (value > 0) {
    //        //좀비가 체력회복할일은 없겠지만 나중에 보스 or 엘리트 좀비가 주변몹 회복할수도있으니 확장성때매 놔둠
    //        //힐 좀비 주변에 +모양 파티클생성
    //    }
    //    else if (value < 0) {
    //        //공격맞은거
    //        //ani.setTrigger("피격모션");
    //    }
    //}
}
