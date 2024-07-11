using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // ���� �̵� 
    void EnemyRun();        // ���� �޸��� 
    void EnemyAttack();     // ���� ����
    void EnemyDaed();       // ���� ��� 
    void EnemyTracking();   // ���� ����
 

}


public abstract class EnemyController : MonoBehaviourPun, IEnemy {

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



    // ���� ü�� 
    private float hp;
    public float EnemyHp { get { return hp; } set { hp = value; } }

    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isAttack;            // ���� �ϴ� ���� 
    protected bool isTracking;          // ���� ���� 

    public abstract void EnemyDaed();
    public abstract void EnemyMove();
    public abstract void EnemyAttack();
    public abstract void EnemyRun();
    public abstract void EnemyTracking();
    

}
