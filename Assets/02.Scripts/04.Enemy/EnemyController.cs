using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy
{
    void EnemyMove();       // ���� �̵� 
    void EnemyRun();        // ���� �޸��� 

    void EnemyMeleeAttack();// ���� ����
    void EnemyDead();       // ���� ��� 
    void EnemyTracking(Collider other);   // ���� ����
}


public class EnemyController : MonoBehaviourPun, IEnemy
{


    // ���� �ȱ� �ӵ� 
    [SerializeField]
    protected float speed;

    // ���� �޸��� �ӵ�
    [SerializeField]
    protected float runSpeed;

    // ���� ����
    public float meleeDelay = 4.0f;     // ���� max �ֱ�
    public float nextAttack = 4;        // ���� ��Ÿ��
    public float attackRange;


    //�������� ���� ����������
    public float rangeOut = 10f;
    //���� �ӵ� 5����
    public float resetSpeed = 5f;
    //���� ���� ����
    public Transform enemySpawn;

    //�����Ÿ�
    public float maxTracingSpeed;


    // ������Ʈ
    protected PhotonView PV;
    public Transform playerTr;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator ani;

    public CapsuleCollider capsuleCollider;


    //���� �ִ� ü��
    [SerializeField]
    public float maxHp;
    // ���� ���� ü�� 
    public float hp;
    public virtual float Hp
    {
        get
        {
            return hp;                          //�׳� ��ȯ
        }
        set
        {
            if (hp > 0)
            {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����             
                Debug.Log(hp);
            }
            else if (hp <= 0)
            {
                EnemyDead();
            }
        }
    }

    public float baseDamage = 10;
    public float damage;

    public bool isRangeOut = false;
    public bool shouldEvaluate = false;
    public bool isNow = false;

    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isAttack;            // ���� �ϴ� ���� 
    protected bool isTracking;          // ���� ����
  

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking(Collider other)
    {
        if (this == null || transform == null)
        {
            return; // ��ü�� �ı��Ǿ����� �Լ� ����
        }

        CancelInvoke("EnemyMove");
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        transform.LookAt(other.transform);
        playerTr = other.transform;
        isTracking = true;

        if (hp <= 0)
        {
            isWalk = false;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float closestDistance = Mathf.Infinity;
            Collider closestPlayer = null;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Mathf.Infinity);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = hitCollider;
                    }
                }
            }

            if (closestPlayer != null)
            {
                EnemyTracking(closestPlayer);
            }
        }
    }
    public virtual void ChangeHp(float value) { }
    public virtual void BloodEffect(Vector3 pos, Collider other = null)
    {
        Pooling.instance.GetObject("BloodSprayEffect", Vector3.zero).transform.position = pos;
    }
    public virtual void EnemyTakeDamage(float damage)
    {
        Hp = -damage;
    }
    
}
