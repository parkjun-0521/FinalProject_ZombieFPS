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
        damage = baseDamage;
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

    bool isSwordHeat;
    IEnumerator DelaySecond(float second)
    {
        yield return new WaitForSeconds(second);
        isSwordHeat = false;
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
        else if (other.CompareTag("Weapon"))
        {
            if (gameObject.CompareTag("EnemyRange")) return;
            if (isSwordHeat) return;
            isSwordHeat = true;
            StartCoroutine(DelaySecond(0.8f));
            Hp = -(other.transform.GetComponent<ItemSword>().itemData.damage);
            BloodEffectSword(transform.position + Vector3.up);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
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

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
        }
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

        if (hp != maxHp) {
            float closestDistance = Mathf.Infinity;
            Collider closestPlayer = null;
            GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
            foreach (var hitCollider in player) {
                if (hitCollider.CompareTag("Player")) {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestPlayer = hitCollider.GetComponent<Collider>();
                    }
                }
            }
            EnemyTracking(closestPlayer);
        }
    }

    IEnumerator ResteWalk()
    {
        isWalk = true;
        curMoveTime = 0;
        yield return new WaitForSeconds(1f);
        isWalk = false;
        /*RandomMove(); // 다시 랜덤 이동 호출*/
        yield return new WaitForSeconds(2f);
    }

    public override void EnemyRun()
    {
        if (!isTracking) return;
        ani.SetBool("isRun", true);
        nav.speed = runSpeed;
        nav.SetDestination(playerTr.position);
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_run)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_run);
        }
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
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack2)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack2);
        }

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
        if (!photonView.IsMine) return;
        if (hp <= 0) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            photonView.RPC("DeadState", RpcTarget.OthersBuffered);
            if (!isDead) {
                isDead = true;
                for (int i = 0; i < 4; i++) {
                    GameObject splitEnemy = Pooling.instance.GetObject("Zombie1", transform.position);
                    splitEnemy.transform.position = transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2));
                    splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // 분열좀비 능력치 ( 추후 난이도 조절 )
                    splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // 분열좀비 능력치
                    splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // 분열좀비 능력치
                                                                                                
                    if (splitEnemy.GetComponent<NormalEnemy>().enemySpawn == null) {
                        splitEnemy.GetComponent<NormalEnemy>().enemySpawn = this.transform;
                    }
                   
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
        bloodParticle.Play();               // 터지는 이펙트 추가
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

    [PunRPC]
    public void DeadState() {
        isDead = true;
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
    }



}
