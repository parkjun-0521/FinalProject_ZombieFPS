using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy {
    void EnemyMove();       // 좀비 이동 
    void EnemyRun();        // 좀비 달리기 
    void EnemyAttack();     // 좀비 공격
    void EnemyDaed();       // 좀비 사망 
    void EnemyTracking();   // 좀비 추적
 

}


public abstract class EnemyController : MonoBehaviourPun, IEnemy {

    // 좀비 걷기 속도 
    [SerializeField]
    protected float speed;

    // 좀비 달리기 속도
    [SerializeField]
    protected float runSpeed;


    protected PhotonView PV;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Vector3 origin = new Vector3(0, 0, 0);
    protected Animator ani;



    // 좀비 체력 
    private float hp;
    public float EnemyHp { get { return hp; } set { hp = value; } }

    protected bool isWalk;              // 걷고있는 상태 
    protected bool isRun;               // 달리는 상태 
    protected bool isAttack;            // 공격 하는 상태 
    protected bool isTracking;          // 추적 상태 

    public abstract void EnemyDaed();
    public abstract void EnemyMove();
    public abstract void EnemyAttack();
    public abstract void EnemyRun();
    public abstract void EnemyTracking();
    

}
