using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMeleeEnemy : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnRandomMove, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

    // 공격
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;

    bool isDead;

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
        OnRandomMove += RandomMove;             // 랜덤 방향전환 이동 
        OnEnemyTracking += EnemyTracking;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;
    }

    private void OnDisable()
    {
        OnRandomMove -= RandomMove;             // 랜덤 방향전환 이동 
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyDead -= EnemyDead;
    }

    void Start()
    {
        RandomMove();

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
        damage = 20f;
    }

    void Update()
    {
        if (hp <= 0) return;

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
                EnemyMeleeAttack();
            }
        }
        else if (!isTracking && !isAttack) {
            if (!isWalk) {
                RandomMove();
            }
        }
    }

    void OnTriggerEnter(Collider other)
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
        if (!isTracking) return;
        ani.SetBool("isRun", true);
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
    }

    public override void EnemyMeleeAttack()
    {
        if (hp <= 0 || playerTr == null || isAttack) return;

        isAttack = true;
        nav.isStopped = true;
        ani.SetBool("isAttack", true);

        // 공격 애니메이션을 수행
        // 애니메이션 이벤트 또는 코루틴을 통해 실제 데미지를 적용

        // 일정 시간이 지난 후 공격 상태 해제
        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(meleeDelay);
        isAttack = false;
        ani.SetBool("isAttack", false);
        nav.isStopped = false;

        if (playerTr != null) {
            nav.SetDestination(playerTr.position);
        }
    }

    public override void EnemyDead()
    {
        if (hp <= 0) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            if (!isDead) {
                isDead = true;
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
                    damage = 30f;
                    if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_explosion)) {
                        AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_explosion);
                    }

                }
            }
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
            }
        }
    }

    [PunRPC]
    public void HandleEnemyDeath()
    {
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = true;
        isTracking = true;
        EnemyLookRange.enabled = false;
        nav.isStopped = true;
        rigid.isKinematic = true;
        capsuleCollider.enabled = false;
        ani.SetBool("isDead", true);
        StartCoroutine(AnimationFalse("isDead"));
        OnEnemyDead -= EnemyDead;
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }

    public override void ChangeHp(float value)
    {
        photonView.RPC("NormalEnemyChangeHpRPC", RpcTarget.AllBuffered, value);
    }

    [PunRPC]
    public void NormalEnemyChangeHpRPC(float value)
    {
        hp += value;
        EnemyDead();
    }



}
