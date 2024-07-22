using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMeleeEnemy : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead;

    //����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP ���� 
    public override float Hp {
        get {
            return hp;                          //�׳� ��ȯ
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����             
                Debug.Log(hp);
            }
            else if(hp <= 0) {
                OnEnemyDead?.Invoke();
            }
        }
    }

    void Awake() {
        // ���۷��� �ʱ�ȭ 
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
            // �ʱ⿡ ������ ���� 
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
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void Update() {
        if (PV.IsMine) {
            if (isRangeOut == true)     OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
            if (isWalk)                 OnEnemyTracking?.Invoke();      // �÷��̾� �߰� 
            if (isTracking)             OnEnemyRun?.Invoke();           // �߰� �� �޸��� 
            if (nav.isStopped == true)  OnEnemyAttack?.Invoke();        // ���� ���� 
        }
    }

    void OnTriggerEnter( Collider other )                       //�Ѿ�, ��������...triggerEnter
    {
        if (PV.IsMine) {
            if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
            {
                Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Weapon"))        // ��������� trigger
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

    //���� �� NPC�� �̵�
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

            //���� ������ ������
            if (toOrigin.magnitude > rangeOut) {
                CancelInvoke("EnemyMove");
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //�ٽõ��ƿ���
                Vector3 direction = (enemySpawn.position - transform.position).normalized;
                rigid.AddForce(direction * resetSpeed, ForceMode.VelocityChange);
                isRangeOut = true;
                OnEnemyTracking -= EnemyTracking;
            }
            //�ƴϸ� ����
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
            // �ӽ÷� ���� ���� ( Ǯ���� �ְ� ������ ���� )
            GameObject splitEnemy = Instantiate(splitZombies[i], transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2)), Quaternion.identity);
            splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // �п����� �ɷ�ġ ( ���� ���̵� ���� )
            splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // �п����� �ɷ�ġ
            splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // �п����� �ɷ�ġ
                                                                                        // ������ ����Ʈ �߰�
            bloodParticle.Play();
            damage = 50f;

            // Vector3 direction = (Vector3.zero - transform.position).normalized;
            // �븻 ������ �� �κ��� zero�� �ƴ϶� transform.position���� �ٲ������ 
        }

        ani.SetBool("isDead", true);
        OnEnemyDead -= EnemyDead;
    }


    public override void ChangeHp( float value ) {
        hp += value;
        if (value > 0) {
            //���� ü��ȸ�������� �������� ���߿� ���� or ����Ʈ ���� �ֺ��� ȸ���Ҽ��������� Ȯ�强���� ����
            //�� ���� �ֺ��� +��� ��ƼŬ����
        }
        else if (value < 0) {
            //���ݸ�����
            //ani.setTrigger("�ǰݸ��");
        }
    }
}
