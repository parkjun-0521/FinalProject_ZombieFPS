using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

interface IEnemy
{
    void EnemyMove();       // 좀비 이동 
    void EnemyRun();        // 좀비 달리기 
    void EnemyMeleeAttack();// 좀비 공격
    void EnemyDead();       // 좀비 사망 
    void EnemyTracking(Collider other);   // 좀비 추적
}


public class EnemyController : MonoBehaviourPun, IEnemy
{
    // 좀비 걷기 속도 
    [SerializeField] public float baseSpeed;
    [SerializeField] protected float runSpeed;
    public float maxTracingSpeed;
    public float maxMoveDirTime = 3;
    public float curMoveTime = 2;
    public float meleeDelay = 2.0f;
    public float nextAttack = 4;
    public float attackRange;
    public float rangeOut = 10f;
    public float resetSpeed = 5f;
    public Transform enemySpawn;
    [SerializeField] public float maxHp;
    public float hp;

    public virtual float Hp {
        get { return hp; }
        set {
            if (hp > 0) {
                ChangeHp(value);
                Debug.Log(hp);
            }
            else if (hp <= 0) {
                if (!isDie) {
                    isDie = true;
                    EnemyDead();
                }
            }
        }
    }

    public float baseDamage = 10;
    public float damage;
    public bool isRangeOut = false;
    public bool shouldEvaluate = false;
    public bool isWalk;
    public bool isRun;
    public bool isAttack;
    public bool isTracking;
    public bool isDie;
    public bool isNotWeaponAttack;

    public PhotonView PV;
    public Transform playerTr;
    public Rigidbody rigid;
    public NavMeshAgent nav;
    public Animator ani;
    public CapsuleCollider capsuleCollider;
    public SphereCollider sphereCollider;
    public SphereCollider EnemyLookRange;

    public float searchInterval = 1.0f;
    public float nextSearchTime = 0f;
    public Quaternion targetRotation;
    [SerializeField] public float rotationSpeed = 5.0f;

    public virtual void EnemyMove() { }
    public virtual void EnemyRun() { }
    public virtual void EnemyMeleeAttack() { }
    public virtual void EnemyDead() { }
    public virtual void EnemyTracking( Collider other ) {
        if (this == null || transform == null) return;

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        playerTr = other.transform;
        isTracking = true;

        if (hp <= 0) {
            targetRotation = Quaternion.identity;
            playerTr = null;
        }

        if (playerTr != null) {
            nav.SetDestination(playerTr.position);
            EnemyRun();
        }
    }
    void OnTriggerStay( Collider other ) {
        if (Time.time >= nextSearchTime) {
            nextSearchTime = Time.time + searchInterval;

            if (other.CompareTag("Player")) {

                float closestDistance = Mathf.Infinity;
                Collider closestPlayer = null;

                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 200);
                foreach (var hitCollider in hitColliders) {
                    if (hitCollider.CompareTag("Player")) {
                        if (hitCollider.GetComponent<Player>().isDead || hitCollider.GetComponent<Player>().isFaint) continue;

                        float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestPlayer = hitCollider;
                        }
                    }
                }
                if (closestPlayer != null) {
                    EnemyTracking(closestPlayer);
                }
            }
        }
        if (other.CompareTag("FireDotArea"))
        {
            Hp = -other.GetComponent<ItemFireGrenadeDotArea>().dotDamage;
        }
    }

    public virtual void ChangeHp(float value) { }
    public virtual void BloodEffect(Vector3 pos, Collider other = null)
    {
        Pooling.instance.GetObject("BloodSprayEffect", Vector3.zero).transform.position = pos;
    }
    public virtual void BloodEffectSword(Vector3 pos, Collider other = null)
    {
        GameObject blood = Pooling.instance.GetObject("BloodSprayEffect", Vector3.zero);
        blood.transform.position = pos;
        blood.transform.localScale = new Vector3(30, 30, 30);
    }
    public virtual void EnemyTakeDamage(float damage)
    {
        Hp = -damage;
    }
    
}
