using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // ���� �̵� 
    void EnemyRun();        // ���� �޸��� 
    void EnemyAttack();     // ���� ����
    void EnemyMeleeAttack();// ���� ����
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

    // ���� ����
    public float meleeDelay = 4.0f;     // ���� max �ֱ�
    public float nextAttack = 4;        // ���� ��Ÿ��

    //�������� ���� ����������
    public float rangeOut = 10f;
    //���� �ӵ� 5����
    public float resetSpeed = 5f;

    //�����Ÿ�
    public float rad = 3f;
    public float distance = 5f;
    public LayerMask layermask;
    public float maxTracingSpeed;

    // ������Ʈ
    public Transform playerTr;
    protected PhotonView PV;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator ani;

    protected Vector3 origin = new Vector3(0, 0, 0);

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

    public bool isRangeOut = false;
    public bool shouldEvaluate = false;
    public bool isNow = false;

    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isAttack;            // ���� �ϴ� ���� 
    protected bool isTracking;          // ���� ���� 

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyAttack() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking() {
        Vector3 skyLay = new Vector3(transform.position.x, 10, transform.position.z);
        RaycastHit hit;
        bool isHit = Physics.SphereCast(skyLay, rad, Vector3.down, out hit, distance, layermask);

        if (isHit) {
            CancelInvoke("EnemyMove");
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            transform.LookAt(hit.transform);
            isTracking = true;
        }
    }
    public virtual void ChangeHp( float value ) { }
    public virtual void BloodEffect( Vector3 pos, Collider other = null ) {
        Pooling.instance.GetObject("BloodSprayEffect").transform.position = pos;
    }
}
