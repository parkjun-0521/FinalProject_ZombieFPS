using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // ���� �̵� 
    void EnemyRun();        // ���� �޸��� 
    void EnemyAttack();     // ���� ����
    void EnemyMeleeAttack();     // ���� ����
    void EnemyDead();       // ���� ��� 
    void EnemyTracking();   // ���� ����
    
}


public class EnemyController : MonoBehaviourPun, IEnemy {

    
    // ���� �ȱ� �ӵ� 
    [SerializeField]
    protected float speed;

    // ���� �޸��� �ӵ�
    [SerializeField]
    protected float runSpeed;


    protected PhotonView PV;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Vector3 origin = new Vector3(0, 0, 0);
    protected Animator ani;

    //���� �ִ� ü��
    [SerializeField]
    protected float maxHp;
    // ���� ���� ü�� 
    
    protected float hp;
    public float Hp
    {
        get
        {
            return hp;                          //�׳� ��ȯ
        }
        set
        {
            if(hp > 0)
            {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����
                EnemyDead();                      //���� hp�� ���������� ü���� 0���� ������ ����
                Debug.Log(hp);
            }
        }
    }

    public float damage;

    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isAttack;            // ���� �ϴ� ���� 
    protected bool isTracking;          // ���� ���� 


    public virtual void EnemyDead() { }
    public virtual void EnemyAttack() { }
    public virtual void ChangeHp(float value) { }
    public virtual void BloodEffect(Vector3 pos, Collider other = null)
    {
        Pooling.instance.GetObject("BloodSprayEffect").transform.position = pos;
    }
    public virtual void EnemyMove() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyTracking() { }
    public virtual void EnemyTakeDamage(float damage)
    {
        Hp = -damage;
    }
}
