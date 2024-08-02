using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteRangeEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyRun, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle(Collider other);
    public static event EnemyTraceHandle OnEnemyTracking;


    private Vector3 moveDirection;
    private bool isMoving = false;
    //공격
    public Transform attackPos;
    public float attackPrefabSpeed = 30.0f;

    public GameObject rangeProjectile;

    public GameObject[] player;
    // HP 구현 
    public override float Hp
    {
        get
        {
            return hp;                       
        }
        set
        {
            if (hp > 0)
            {
                ChangeHp(value);                        
                Debug.Log(hp);
            }
            else if (hp <= 0)
            {
                OnEnemyDead?.Invoke();
            }
        }
    }

    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        sphereCollider = (SphereCollider)EnemyLookRange;
    }

    private void OnEnable()
    {
        if (PV.IsMine)
        {
            OnEnemyReset += ResetEnemy;
            OnEnemyMove += RandomMove;
            OnEnemyTracking += EnemyTracking;
            OnEnemyRun += EnemyRun;
            OnEnemyAttack += EnemyRangeAttack;
            OnEnemyDead += EnemyDead;

            hp = maxHp;
            ani.SetBool("isDead", false);
            // 초기에 데미지 지정 
            // damage = 20f;
        }
    }

    void OnDisable()
    {
        if (PV.IsMine)
        {
            OnEnemyReset -= ResetEnemy;
            OnEnemyMove -= RandomMove;
            OnEnemyTracking -= EnemyTracking;
            OnEnemyRun -= EnemyRun;
            OnEnemyAttack -= EnemyRangeAttack;
            OnEnemyDead -= EnemyDead;
        }
    }


    void Start()
    {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        // 초기에 데미지 지정 
        // damage = 20f;
        rigid.isKinematic = false;
        //nav.enabled = true;
        sphereCollider.enabled = true;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isRangeOut == true) OnEnemyReset?.Invoke();         // 범위 나갔을 때 초기화 
            if (isTracking) OnEnemyRun?.Invoke();                   // 추격 시 달리기 
            if (nav.isStopped == true) OnEnemyAttack?.Invoke();     // 몬스터 공격 

        }
        
    }

    void OnTriggerEnter(Collider other)                       //총알, 근접무기...triggerEnter
    {
        if (other.CompareTag("Bullet")) {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);

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

    //보통 적 NPC의 이동
    public override void EnemyMove()
    {
        if (PV.IsMine)
        {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy()
    {
        if (PV.IsMine)
        {
            Vector3 dest = new Vector3();
            transform.LookAt(dest);
            if (Vector3.Distance(transform.position, enemySpawn.position) < 0.1f && shouldEvaluate)
            {
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
        if (PV.IsMine)
        {
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
            }
            else
            {

                // NavMeshAgent를 사용하여 이동
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
        if (PV.IsMine)
        {
            isRun = true;
            ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
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

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_run)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_run);
            }
        }
    }
    public void EnemyRangeAttack()
    {
        if(PV.IsMine)
        {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay)
            {
                ani.SetBool("isAttack", true);
                photonView.RPC("RPCEnemyRangeAttack", RpcTarget.AllBuffered);
                nextAttack = 0;
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack5)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack5);
                }
                StartCoroutine(AnimReset("isAttack"));
            }
        }
    }
    [PunRPC]
    void RPCEnemyRangeAttack()
    {
        Vector3 attackDir = (playerTr.position - transform.position).normalized;
        GameObject zombieRangeAtkPrefab = Instantiate(rangeProjectile, attackPos.position, Quaternion.identity);
        zombieRangeAtkPrefab.GetComponent<Rigidbody>().AddForce(attackDir * attackPrefabSpeed, ForceMode.Impulse);

    }


    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void HandleEnemyDeath()
    {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyRangeAttack;
        isWalk = false;
        sphereCollider.enabled = false;
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
        }
        ani.SetBool("isDead", true);
        OnEnemyDead -= EnemyDead;
    }

    [PunRPC]
    void EliteRangeChangeHpRPC(float value)
    {
        hp += value;
        EnemyDead();
    }
    public override void ChangeHp(float value)
    {
        photonView.RPC("EliteRangeChangeHpRPC", RpcTarget.AllBuffered, value);
    }

    IEnumerator AnimReset(string animString = null)
    {
        yield return new WaitForSeconds(0.5f);
        ani.SetBool(animString, false);
    }
}
