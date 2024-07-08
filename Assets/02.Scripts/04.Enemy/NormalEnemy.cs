using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    
    private PhotonView PV;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private Vector3 origin = new Vector3(0, 0, 0);

    public float rangeOut = 20.0f;
    public float resetSpeed = 40.0f;
    InputKeyManager keyManager;

    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
    }

    

    void Start()
    {
        InvokeRepeating("EnemyMove", 0.5f,3.0f);
       

    }
  

    //보통 적 NPC의 이동
    public override void EnemyMove()
    {


        

        float dirX = Random.Range(-100  , 100);
        float dirZ = Random.Range(-100, 100);
        Vector3 dest = new Vector3(dirX,0,dirZ);
        transform.LookAt(dest);
        Vector3 toOrigin = origin - transform.position;
        if (toOrigin.magnitude > rangeOut)
        {
            rigid.AddForce(toOrigin.normalized * resetSpeed);
        }
        else
        {
            rigid.AddForce(dest * speed * Time.fixedDeltaTime, ForceMode.Force);
        }
            rigid.velocity = Vector3.zero;
    }
    
    public override void EnemyAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void EnemyDaed()
    {
        throw new System.NotImplementedException();
    }

 
    public override void EnemyRun()
    {
        throw new System.NotImplementedException();
    }

    public override void EnemyTracking()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
   

    // Update is called once per frame
    void Update()
    {
        
    }
}
