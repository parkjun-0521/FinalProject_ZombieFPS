using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController
{
    enum State
    {
        idle,
        randomMove,
        attack,
        chase,
        dead
    }
    [Header("ver.osj")]
    [SerializeField] State state = State.idle;
    [SerializeField] GameObject[] players;
    [SerializeField] float traceRange = 10f;  //플레이어 감지거리
    [SerializeField] float attackDistance = 2f;
    [SerializeField] float rotationSpeed = 2.0f;
    [SerializeField] float AtkCoolTime = 3.0f;
    private Quaternion targetRotation; // 목표 회전
    [SerializeField] Collider zombieCollider;

    void Awake()
    {
        // 레퍼런스 초기화 
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        zombieCollider = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        if(PV.IsMine)
        {
            state = State.idle;
        }
    }

    private void OnDisable()
    {

    }

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

    }

    void Update()
    {
        if (PV.IsMine)
        {
            switch (state)
            {
                case State.idle:
                    Idle();
                    break;
                case State.randomMove:
                    RandomMove();
                    break;
                case State.chase:
                    Chase();
                    break;
                case State.attack:
                    Attack();
                    break;
                case State.dead:
                    break;
            }
        }

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (state == State.dead) return;
            Hp = -(other.GetComponent<Bullet>().itemData.damage);
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon"))
        {
            if (state == State.dead) return;
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade"))
        {
            if (state == State.dead) return;
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
    }


    void Idle()
    {
        RandomMovePoscalculate();
    }
    bool isRandomMove;
    void RandomMovePoscalculate()
    {
        if (PV.IsMine && !isRandomMove)
        {
            float dirX = Random.Range(-40, 40);
            float dirZ = Random.Range(-40, 40);
            Vector3 dest = new Vector3(dirX, 0, dirZ);

            targetRotation = Quaternion.LookRotation(dest);                 
            Vector3 targetPosition = transform.position + dest;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                nav.SetDestination(hit.position);
            }

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
            }
            state = State.randomMove;
            isRandomMove = true;
        }
    }

    Collider[] detectedPlayer;
    void RandomMove()
    {
        
        if((transform.position - nav.destination).magnitude < 1.3)  
        {
            state = State.idle;
            isRandomMove = false;
        }

        detectedPlayer = Physics.OverlapSphere(transform.position, traceRange, LayerMask.GetMask("LocalPlayer"));
        if(detectedPlayer.Length > 0 || hp != maxHp)  //근처에 오거나 피가 1이라도 달면
        {
            state = State.chase;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, traceRange);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    void Chase()
    {
        Transform closestPlayer = players[0].transform;
        foreach (GameObject player in players)
        {
            Vector3 playerTr = player.transform.position;
            float playerDistance = ((playerTr - transform.position).sqrMagnitude);
            if ((closestPlayer.position - transform.position).sqrMagnitude >= playerDistance)
                closestPlayer.position = playerTr;
        }
        nav.SetDestination(closestPlayer.position);
        Quaternion targetRotation = Quaternion.LookRotation(closestPlayer.position - transform.position);
        if (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if ((closestPlayer.position - transform.position).magnitude < attackDistance)
        {
            state = State.attack;
            nav.velocity = Vector3.zero;
        }
    }

    Coroutine AttackCoroutine;
    void Attack()
    {
        Transform closestPlayer = players[0].transform;
        foreach (GameObject player in players)
        {
            Vector3 playerTr = player.transform.position;
            float playerDistance = ((playerTr - transform.position).sqrMagnitude);
            if ((closestPlayer.position - transform.position).sqrMagnitude >= playerDistance)
                closestPlayer.position = playerTr;
        }
        nav.SetDestination(closestPlayer.position);
        Vector3 directionToTarget = closestPlayer.position - transform.position; // 목표 방향 계산
        if (directionToTarget != Vector3.zero) // 방향이 0이 아닌 경우에만 회전 계산
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 27f) // 1도 이상 차이 나는 경우에만 회전
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        
        if (AttackCoroutine == null)
        {
            AttackCoroutine = StartCoroutine(AttackCor());
        }
    }
    IEnumerator AttackCor()
    {
        ani.SetBool("isAttack", true);
        StartCoroutine(AnimationFalse("isAttack"));
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack2))
        {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack2);
        }
        yield return new WaitForSeconds(AtkCoolTime); //공격애니메이션 시간 2.633
        if(state != State.dead)
        {
            state = State.chase;
            AttackCoroutine = null;
        }
    }


    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            state = State.dead;
            nav.isStopped = true;
            rigid.velocity = Vector3.zero;
            zombieCollider.enabled = false;
            ani.SetBool("isDead", true);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
            }
        }
    }


    [PunRPC]
    void NormalEnemyChangeHpRPC(float value)
    {
        hp += value;
        if(state == State.randomMove)
        {
            state = State.chase;
        }
        EnemyDead();
    }
    public override void ChangeHp(float value)
    {
        if (photonView.IsMine)
            photonView.RPC("NormalEnemyChangeHpRPC", RpcTarget.AllBuffered, value);
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }
}
