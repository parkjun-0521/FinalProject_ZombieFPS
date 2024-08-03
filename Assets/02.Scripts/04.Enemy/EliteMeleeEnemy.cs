using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EliteMeleeEnemy : EnemyController {


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
    [SerializeField] float traceRange = 10f;  //�÷��̾� �����Ÿ�
    [SerializeField] float attackDistance = 2f;
    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation; // ��ǥ ȸ��
    [SerializeField] float AtkCoolTime = 3.0f;
    [SerializeField] Collider zombieCollider;

    public GameObject[] splitZombies;
    public ParticleSystem bloodParticle;
    // HP ���� 
    public override float Hp {
        get {
            return hp;                          //�׳� ��ȯ
        }
        set {
            if (hp > 0) {
                ChangeHp(value);                   //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����             
                Debug.Log(hp);
            }
            else if(hp <= 0) {
                EnemyDead();
            }
        }
    }

    public bool isDead;         // RPC ����ȭ�� Bool ���� 


    void Awake() {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        zombieCollider = GetComponent<CapsuleCollider>();
    }

    private void OnEnable() {
        if (PV.IsMine) {


            hp = maxHp;
            ani.SetBool("isDead", false);
            bloodParticle.Stop();
            // �ʱ⿡ ������ ���� 
            // damage = 20f;
        }
    }

    void OnDisable() {
        if (PV.IsMine) {

        }
    }


    void Start() {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    void Update()
    {
        if (PV.IsMine)
        {
            if (state == State.dead) return;
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
            }
        
        }
    }
    void OnTriggerEnter( Collider other )                       //�Ѿ�, ��������...triggerEnter
    {
        if (other.CompareTag("Bullet")){
            if (state == State.dead) return;
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            if (state == State.dead) return;
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade")) {
            if (state == State.dead) return;
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        return;
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

        if ((transform.position - nav.destination).magnitude < 1.3)
        {
            state = State.idle;
            isRandomMove = false;
        }

        detectedPlayer = Physics.OverlapSphere(transform.position, traceRange, LayerMask.GetMask("LocalPlayer"));
        if (detectedPlayer.Length > 0 || hp != maxHp)  //��ó�� ���ų� �ǰ� 1�̶� �޸�
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
        Vector3 directionToTarget = closestPlayer.position - transform.position; // ��ǥ ���� ���
        if (directionToTarget != Vector3.zero) // ������ 0�� �ƴ� ��쿡�� ȸ�� ���
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 27f) // 1�� �̻� ���� ���� ��쿡�� ȸ��
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
        yield return new WaitForSeconds(AtkCoolTime); //���ݾִϸ��̼� �ð� 2.633
        AttackCoroutine = null;
        if (state != State.dead)
        {
            state = State.chase;
            AttackCoroutine = null;
        }
    }




    public override void EnemyDead() {
        if (hp <= 0 && !isDead) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            ani.SetBool("isDead", true);
            state = State.dead;
            nav.isStopped = true;
            rigid.velocity = Vector3.zero;
            zombieCollider.enabled = false;

            for (int i = 0; i < 4; i++) {
                GameObject splitEnemy = Pooling.instance.GetObject("Zombie1", transform.position);
                splitEnemy.transform.position = transform.position + new Vector3(Random.Range(0, 2), 0, Random.Range(0, 2));
                splitEnemy.GetComponent<NormalEnemy>().maxHp = this.maxHp * 0.8f;           // �п����� �ɷ�ġ ( ���� ���̵� ���� )
                splitEnemy.GetComponent<NormalEnemy>().hp = this.maxHp * 0.8f;              // �п����� �ɷ�ġ
                splitEnemy.GetComponent<NormalEnemy>().damage = this.damage * 0.2f;         // �п����� �ɷ�ġ
                                                                                            // ������ ����Ʈ �߰�
                if (splitEnemy.GetComponent<NormalEnemy>().enemySpawn == null) {
                    splitEnemy.GetComponent<NormalEnemy>().enemySpawn = this.transform;
                }
                bloodParticle.Play();
                damage = 50f;
                if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_explosion)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_explosion);
                }

            }
        }
    }
    [PunRPC]
    public void HandleEnemyDeath() {
        hp = 0;
        isWalk = false;
        isDead = true;
        rigid.isKinematic = true;
        state = State.dead;
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
        }
        ani.SetBool("isDead", true);
    }

    [PunRPC]
    void EliteMeleeChangeHpRPC(float value)
    {
        hp += value;
        if (state == State.randomMove)
        {
            state = State.chase;
        }
        EnemyDead();
    }
    public override void ChangeHp(float value)
    {
        if(photonView.IsMine)
        photonView.RPC("EliteMeleeChangeHpRPC", RpcTarget.AllBuffered, value);
    }
    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }
}
