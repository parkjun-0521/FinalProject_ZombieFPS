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
    public float maxTracingSpeed;       // �ִ� ���� �ӵ� 

    // ���� ����
    public float meleeDelay = 4.0f;     // ���� max �ֱ�
    public float nextAttack = 4;        // ���� ��Ÿ��
    public float attackRange;           // ���� �Ÿ� 

    // �������� ���� ����������
    public float rangeOut = 10f;        // walk ���� 
    //���� �ӵ� 5����
    public float resetSpeed = 5f;       // ���� ���ƿ��� �ӵ� 
    //���� ���� ����
    public Transform enemySpawn;        // ���� ���� 

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

    public float baseDamage = 10;       // �⺻ ������ 
    public float damage;                // ������ 
        
    // ���� ���� 
    public bool isRangeOut = false;     // ���� ���� 
    public bool shouldEvaluate = false; // ���� ���� ���� 
    public bool isNow = false;          // �ȱ� ���� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isAttack;            // ���� �ϴ� ���� 
    protected bool isTracking;          // ���� ����


    // ������Ʈ
    protected PhotonView PV;            // ���� �� 
    public Transform playerTr;          // �÷��̾� Transform ( ���� ) 
    protected Rigidbody rigid;
    protected NavMeshAgent nav;         // NavMesh 
    protected Animator ani;             // �ִϸ��̼�    
    public CapsuleCollider capsuleCollider;     // ���� �ݶ��̴� 
    public SphereCollider sphereCollider;       // ����Ʈ ��  
    public SphereCollider EnemyLookRange;       // �ν� ���� �ݶ��̴� 

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking(Collider other)
    {
        // ��ü�� �ı��Ǿ����� �Լ� ����
        if (this == null || transform == null) return;

        CancelInvoke("EnemyMove");
        // �ӵ� �ʱ�ȭ 
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        // �÷��̾� �ٶ󺸱� 
        transform.LookAt(other.transform);
        // �÷��̾� �������� ��ȯ 
        playerTr = other.transform;
        // Ʈ��ŷ ���� ���� 
        isTracking = true;

        if (hp <= 0) {
            transform.LookAt(null);
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
        else if(other.CompareTag("FireDotArea"))
        {
            Hp = -(other.GetComponent<ItemFireGrenadeDotArea>().dotDamage); //�ȵǸ� Hp=>hp �غ��� stay�� ������ ȣ��Ǵ� ���� 0.1���� 
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
