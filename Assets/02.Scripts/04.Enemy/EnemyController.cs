using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IEnemy {
    void EnemyMove();       // ���� �̵� 
    void EnemyRun();        // ���� �޸��� 
    void EnemyAttack();     // ���� ����
    void EnemyDaed();       // ���� ��� 
    void EnemyTracking();   // ���� ����

}


public abstract class EnemyController : MonoBehaviour, IEnemy {

    // ���� �ȱ� �ӵ� 
    [SerializeField]
    protected float speed;

    // ���� �޸��� �ӵ�
    [SerializeField]
    protected float runSpeed;

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
