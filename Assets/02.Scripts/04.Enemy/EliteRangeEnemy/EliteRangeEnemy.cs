using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteRangeEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnRandomMove, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;

    //공격
    public Transform attackPos;
    public float attackPrefabSpeed = 30.0f;

    public GameObject rangeProjectile;

    public GameObject[] player;
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
        OnEnemyAttack += EnemyRangeAttack;
        OnEnemyDead += EnemyDead;
    }

    private void OnDisable()
    {
        OnRandomMove -= RandomMove;             // 랜덤 방향전환 이동 
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyRangeAttack;
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
                EnemyRangeAttack();
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

        StartCoroutine(ResteWalk());

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
        }

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

    public void EnemyRangeAttack()
    {
        nextAttack += Time.deltaTime;
        if (nextAttack > meleeDelay) {
            ani.SetBool("isAttack", true);
            photonView.RPC("RPCEnemyRangeAttack", RpcTarget.AllBuffered);
            nextAttack = 0;
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack5)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack5);
            }
            StartCoroutine(AnimReset("isAttack"));
        }
    }
    [PunRPC]
    void RPCEnemyRangeAttack()
    {
        Vector3 attackDir = (playerTr.position - transform.position).normalized;
        GameObject zombieRangeAtkPrefab = Instantiate(rangeProjectile, attackPos.position, Quaternion.identity);
        zombieRangeAtkPrefab.GetComponent<Rigidbody>().AddForce(attackDir * attackPrefabSpeed, ForceMode.Impulse);

    }


    IEnumerator AnimReset(string animString = null)
    {
        yield return new WaitForSeconds(0.5f);
        ani.SetBool(animString, false);
    }

    public override void EnemyDead()
    {
        if (hp <= 0) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
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
        OnEnemyAttack -= EnemyRangeAttack;
        isWalk = true;
        isTracking = true;
        EnemyLookRange.enabled = false;
        nav.isStopped = true;
        rigid.isKinematic = true;
        capsuleCollider.enabled = false;
        ani.SetBool("isDead", true);
        StartCoroutine(AnimationFalse("isDead"));
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
