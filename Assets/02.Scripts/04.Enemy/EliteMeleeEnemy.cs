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
        //PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
    }

    private void OnEnable() {
        OnEnemyReset += ResetEnemy;
        OnEnemyMove += RandomMove;
        OnEnemyTracking += EnemyTracking;
        OnEnemyRun += EnemyRun;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;

        hp = maxHp;
        ani.applyRootMotion = false;
        //델리게이트 사망에서 뻈으니 다 넣기
    }

    void OnDisable() {
        OnEnemyReset -= ResetEnemy;
        OnEnemyMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyRun -= EnemyRun;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyDead -= EnemyDead;
    }


    void Start() {      
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        InvokeRepeating("EnemyMove", 0.5f, 3.0f);
        ani.applyRootMotion = false;
    }

    void Update() {
        if (isRangeOut == true) {
            OnEnemyReset?.Invoke();
        }

        if (isWalk) {
            OnEnemyTracking?.Invoke();
        }

        if (isTracking) {
            OnEnemyRun?.Invoke();
        }

        if (nav.isStopped == true) {
            OnEnemyAttack?.Invoke();
        }

    }


    void OnTriggerEnter( Collider other )                       //총알, 근접무기...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // 총알과 trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
        {
            //Hp = -(other.GetComponent<Weapon>().attackdamage)
            BloodEffect(transform.position);
        }
        else if (other.CompareTag("Player")) {
            other.GetComponent<Player>().Hp = -damage;
        }
        else
            return;
    }

    //보통 적 NPC의 이동
    public override void EnemyMove() {
        OnEnemyMove?.Invoke();
    }

    void ResetEnemy() {
        Vector3 dest = new Vector3();
        transform.LookAt(dest);
        if (Vector3.Distance(transform.position, Vector3.zero) < 0.1f && shouldEvaluate) {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            InvokeRepeating("EnemyMove", 0.5f, 3.0f);
            OnEnemyTracking += EnemyTracking;
            shouldEvaluate = false;
            isRangeOut = false;
        }
        shouldEvaluate = true;
    }

    void RandomMove() {
        isWalk = true;
        float dirX = Random.Range(-40, 40);
        float dirZ = Random.Range(-40, 40);
        Vector3 dest = new Vector3(dirX, 0, dirZ);
        transform.LookAt(dest);
        Vector3 toOrigin = origin - transform.position;

        //일정 범위를 나가면
        if (toOrigin.magnitude > rangeOut) {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            //다시돌아오는
            Vector3 direction = (Vector3.zero - transform.position).normalized;
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

    public override void EnemyRun() {
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

    public override void EnemyMeleeAttack() {
        nextAttack += Time.deltaTime;
        if (nextAttack > meleeDelay) {
            ani.SetBool("isAttack", true);
            Debug.Log("ATtak");
            nextAttack = 0;
        }
    }

    public override void EnemyDead() {
        if (hp <= 0) {
            OnEnemyReset -= ResetEnemy;
            OnEnemyMove -= RandomMove;
            OnEnemyTracking -= EnemyTracking;
            OnEnemyRun -= EnemyRun;
            OnEnemyAttack -= EnemyMeleeAttack;
            isWalk = false;

            for (int i = 0; i < 2; i++) {
                Instantiate(splitZombies[i], transform.position, Quaternion.identity);
            }
            ani.applyRootMotion = true;
            ani.SetTrigger("isDead");
            OnEnemyDead -= EnemyDead;
        }
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
