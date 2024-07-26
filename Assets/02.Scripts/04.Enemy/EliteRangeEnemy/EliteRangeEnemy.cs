using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteRangeEnemy : EnemyController
{
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnEnemyReset, OnEnemyMove, OnEnemyTracking, OnEnemyRun, OnEnemyAttack, OnEnemyDead;

    //����
    public Transform attackPos;
    public float attackPrefabSpeed = 30.0f;

    public GameObject rangeProjectile;
    public float attackRangeDistance = 30.0f;

    public GameObject[] player;
    // HP ���� 
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
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
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
            // �ʱ⿡ ������ ���� 
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
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (isRangeOut == true) OnEnemyReset?.Invoke();         // ���� ������ �� �ʱ�ȭ 
            if (isWalk) OnEnemyTracking?.Invoke();      // �÷��̾� �߰� 
            if (isTracking) OnEnemyRun?.Invoke();           // �߰� �� �޸��� 
            if (nav.isStopped == true) OnEnemyAttack?.Invoke();        // ���� ���� 
        }
    }

    void OnTriggerEnter(Collider other)                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
            {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }
        return;
    }

    //���� �� NPC�� �̵�
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
            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);
            transform.LookAt(dest);
            Vector3 toOrigin = enemySpawn.position - transform.position;

            //���� ������ ������
            if (toOrigin.magnitude > rangeOut)
            {
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
            else
            {
                OnEnemyTracking += EnemyTracking;
                rigid.AddForce(dest * speed * Time.deltaTime, ForceMode.VelocityChange);
            }

            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    public override void EnemyRun()
    {
        if (PV.IsMine)
        {
            OnEnemyMove -= RandomMove;
            isRun = true;
            //ani.SetBool("isAttack", false);
            ani.SetBool("isRun", true);
            if((playerTr.position - transform.position).magnitude < attackRangeDistance)
            {
                ani.SetBool("isRun", false);
                ani.SetBool("isIdle", true);
            }
            else
            {
                ani.SetBool("isIdle", false);
            }
            nav.speed = runSpeed;
            nav.destination = playerTr.position;
            if (rigid.velocity.magnitude > maxTracingSpeed)
                rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;

            float versusDist = Vector3.Distance(transform.position, playerTr.position);

            nav.isStopped = (versusDist < attackRangeDistance) ? true : false;
        }
    }
    public void EnemyRangeAttack()
    {
        if(PV.IsMine)
        {
            nextAttack += Time.deltaTime;
            if (nextAttack > meleeDelay)
            {
                //ani.SetBool("isIdle", false);
                ani.SetBool("isAttack", true);
                photonView.RPC("RPCEnemyRangeAttack", RpcTarget.AllBuffered);
                nextAttack = 0;
                StartCoroutine(AnimReset("isAttack"));
            }
        }
    }
    [PunRPC]
    void RPCEnemyRangeAttack()
    {
        Vector3 attackDir = (playerTr.position - transform.position).normalized;
        //GameObject zombieRangeAtkPrefab = Pooling.instance.GetObject("EliteRangeZombieProjectile");
        //zombieRangeAtkPrefab.transform.position = attackPos.position;
        //zombieRangeAtkPrefab.transform.rotation = Quaternion.identity;
        GameObject zombieRangeAtkPrefab = Instantiate(rangeProjectile, attackPos.position, Quaternion.identity);
        zombieRangeAtkPrefab.GetComponent<Rigidbody>().AddForce(attackDir * attackPrefabSpeed, ForceMode.Impulse);

    }


    public override void EnemyDead()
    {
        if (hp <= 0 && photonView.IsMine)
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

        ani.SetBool("isDead", true);
        OnEnemyDead -= EnemyDead;
    }


    public override void ChangeHp(float value)
    {
        hp += value;
        if (value > 0)
        {
            //���� ü��ȸ�������� �������� ���߿� ���� or ����Ʈ ���� �ֺ��� ȸ���Ҽ��������� Ȯ�强���� ����
            //�� ���� �ֺ��� +��� ��ƼŬ����
        }
        else if (value < 0)
        {
            //���ݸ�����
            //ani.setTrigger("�ǰݸ��");
        }
    }

    IEnumerator AnimReset(string animString = null)
    {
        yield return new WaitForSeconds(0.5f);
        ani.SetBool(animString, false);
    }
}
