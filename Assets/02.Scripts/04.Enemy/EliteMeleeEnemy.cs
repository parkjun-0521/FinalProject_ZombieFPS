using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMeleeEnemy : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead;

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

    void Awake() {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
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
        if (PV.IsMine) {
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            bloodParticle.Stop();
            // 초기에 데미지 지정 
            // damage = 20f;
        }
    }

    void Update() {
        if (PV.IsMine) {
            if (isRangeOut == true)     OnEnemyReset?.Invoke();         // 범위 나갔을 때 초기화 
            if (isWalk)                 OnEnemyTracking?.Invoke();      // 플레이어 추격 
            if (isTracking)             OnEnemyRun?.Invoke();           // 추격 시 달리기 
            if (nav.isStopped == true)  OnEnemyAttack?.Invoke();        // 몬스터 공격 
        }
    }

    void OnTriggerEnter( Collider other )                       //총알, 근접무기...triggerEnter
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
    public override void EnemyMove() {
        if (PV.IsMine) {
            OnEnemyMove?.Invoke();
        }
    }

    void ResetEnemy() {
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

    void RandomMove() {
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

    public override void EnemyRun() {
        if (PV.IsMine) {
            OnEnemyMove -= RandomMove;
            isRun = true;
            ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
            nav.speed = runSpeed;
            nav.destination = playerTr.position;
            if (rigid.velocity.magnitude > maxTracingSpeed)
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

            float versusDist = Vector3.Distance(transform.position, playerTr.position);

            nav.isStopped = (versusDist < 3f) ? true : false;
        }
    }

    public override void EnemyMeleeAttack() {
        if (PV.IsMine) {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay) {
                ani.SetBool("isAttack", true);
                Debug.Log("ATtak");
                nextAttack = 0;
            }
        }
    }

    public override void EnemyDead() {
        if (hp <= 0 && PV.IsMine) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void HandleEnemyDeath() {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = false;

        for (int i = 0; i < 4; i++) {
            // 임시로 좀비 생성 ( 풀링에 넣고 생성할 예정 )
            GameObject splitEnemy = Instantiate(splitZombies[i], transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2)), Quaternion.identity);
            splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // 분열좀비 능력치 ( 추후 난이도 조절 )
            splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // 분열좀비 능력치
            splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // 분열좀비 능력치
                                                                                        // 터지는 이펙트 추가
            bloodParticle.Play();
            damage = 50f;

            // Vector3 direction = (Vector3.zero - transform.position).normalized;
            // 노말 좀비의 이 부분을 zero가 아니라 transform.position으로 바꿔줘야함 
        }

        ani.SetBool("isDead", true);
        OnEnemyDead -= EnemyDead;
    }


    public override void ChangeHp( float value ) {
        hp += value;
        if (value > 0) {
            //좀비가 체력회복할일은 없겠지만 나중에 보스 or 엘리트 좀비가 주변몹 회복할수도있으니 확장성때매 놔둠
            //힐 좀비 주변에 +모양 파티클생성
        }
        else if (value < 0) {
            //공격맞은거
            //ani.setTrigger("피격모션");
        }
    }
}
