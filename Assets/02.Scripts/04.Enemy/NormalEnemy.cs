using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : EnemyController {
    public delegate void EnemymoveHandle();
    public static event EnemymoveHandle OnRandomMove, OnEnemyAttack, OnEnemyDead;
    public delegate void EnemyTraceHandle( Collider other );
    public static event EnemyTraceHandle OnEnemyTracking;

    // ����
    public GameObject attackColliderPrefab;
    public Transform attackPoint;

    RaycastHit hit;

    void Awake() {
        // ���۷��� �ʱ�ȭ 
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnEnable() {
        OnRandomMove += RandomMove;             // ���� ������ȯ �̵� 
        OnEnemyTracking += EnemyTracking;
        OnEnemyAttack += EnemyMeleeAttack;
        OnEnemyDead += EnemyDead;
    }

    private void OnDisable() {
        OnRandomMove -= RandomMove;             // ���� ������ȯ �̵� 
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyMeleeAttack;
        OnEnemyDead -= EnemyDead;
    }

    void Start() {
        nav.enabled = true;
        OnRandomMove?.Invoke();

        SphereCollider lookRangeCollider = EnemyLookRange; // EnemyLookRange �ݶ��̴� ����
        int weaponLayer = LayerMask.NameToLayer("Weapon"); // 'Weapon' ���̾� �̸��� �ش��ϴ� ���̾� �ε��� ��������

        // ��� ���� �ݶ��̴��� ã�� EnemyLookRange���� �浹�� ����
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");
        foreach (var weapon in weapons) {
            Collider[] weaponColliders = weapon.GetComponentsInChildren<Collider>();
            foreach (var collider in weaponColliders) {
                Physics.IgnoreCollision(lookRangeCollider, collider, true);
            }
        }
    }

    void Update() {
        if (hp <= 0) return;

        if (isTracking && playerTr != null) {
            Vector3 directionToPlayer = (playerTr.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (nav.remainingDistance <= nav.stoppingDistance) {
                nav.isStopped = false;
                nav.SetDestination(playerTr.position);
            }

            float versusDist = Vector3.Distance(transform.position, playerTr.position);
            if (versusDist < attackRange && !isAttack) {
                EnemyMeleeAttack();
            }
        }
        else if (!isTracking && !isAttack) {
            if (!isWalk) {
                OnRandomMove?.Invoke();
            }
        }
    }

    bool isSwordHeat;
    IEnumerator DelaySecond(float second)
    {
        yield return new WaitForSeconds(second);
        isSwordHeat = false;
    }
    void OnTriggerEnter( Collider other ) {
        if (other.CompareTag("Bullet")) {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);
            other.gameObject.SetActive(false);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Weapon")) {
            if (isSwordHeat) return;
            isSwordHeat = true;
            StartCoroutine(DelaySecond(0.8f));
            Hp = -(other.transform.GetComponent<ItemSword>().itemData.damage);
            BloodEffectSword(transform.position + Vector3.up);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade")) {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
    }

    void RandomMove() {
        if (isTracking) return;

        StartCoroutine(ResteWalk());

        float dirX = Random.Range(-50, 50);
        float dirZ = Random.Range(-50, 50);
        Vector3 dest = new Vector3(dirX, 0, dirZ);

        targetRotation = Quaternion.LookRotation(dest);

        Vector3 targetPosition = transform.position + dest;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas)) {
            nav.SetDestination(hit.position);
        }

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_walk)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_walk);
        }

        // �̷л� �ѿ� ������ ����� �÷��̾ ���� 
        //==================================================//
        if (hp != maxHp) {
            float closestDistance = Mathf.Infinity;
            Collider closestPlayer = null;
            GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
            foreach (var hitCollider in player) {
                if (hitCollider.CompareTag("Player")) {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestPlayer = hitCollider.GetComponent<Collider>();
                    }
                }
            }
            EnemyTracking(closestPlayer);
        }
        //==================================================//
    }

    IEnumerator ResteWalk() {
        isWalk = true;
        curMoveTime = 0;
        yield return new WaitForSeconds(1f);
        isWalk = false;
        /*RandomMove(); // �ٽ� ���� �̵� ȣ��*/
        yield return new WaitForSeconds(2f);
    }

    public override void EnemyRun() {
        if (!isTracking) return;

        nav.speed = runSpeed;
        nav.SetDestination(playerTr.position);

        Vector3 desiredVelocity = nav.desiredVelocity;

        rigid.velocity = Vector3.Lerp(rigid.velocity, desiredVelocity, Time.deltaTime * runSpeed);

        if (rigid.velocity.magnitude > maxTracingSpeed) {
            rigid.velocity = rigid.velocity.normalized * maxTracingSpeed;
        }

        float versusDist = Vector3.Distance(transform.position, playerTr.position);

        if (versusDist < attackRange) {
            rigid.velocity = Vector3.zero;
            nav.isStopped = true;
        }
        else {
            nav.isStopped = false;
        }
    }

    public override void EnemyMeleeAttack() {
        if (hp <= 0 || playerTr == null || isAttack) return;

        isAttack = true;
        nav.isStopped = true;
        ani.SetBool("isAttack", true);

        // ���� �ִϸ��̼��� ����
        // �ִϸ��̼� �̺�Ʈ �Ǵ� �ڷ�ƾ�� ���� ���� �������� ����
        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack2)) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack2);
        }
        // ���� �ð��� ���� �� ���� ���� ����
        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState() {
        yield return new WaitForSeconds(meleeDelay);

        isAttack = false;
        ani.SetBool("isAttack", false);
        if (hp <= 0 || playerTr == null || isAttack) yield break;
        nav.isStopped = false;
        if (playerTr != null) {
            nav.SetDestination(playerTr.position);
        }
    }

    public override void EnemyDead() {
        if (hp <= 0) {
            photonView.RPC("HandleEnemyDeath", RpcTarget.AllBuffered);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
            }
        }
    }

    [PunRPC]
    public void HandleEnemyDeath() {
        OnRandomMove -= RandomMove;
        OnEnemyTracking -= EnemyTracking;
        OnEnemyAttack -= EnemyMeleeAttack;
        isWalk = true;
        isTracking = true;
        EnemyLookRange.enabled = false;
        rigid.isKinematic = true;
        capsuleCollider.enabled = false;
        nav.enabled = false;
       // ani.SetBool("isDead", true);
        ani.SetInteger("isDeadInt", Random.Range(0, 3));
        //StartCoroutine(AnimationFalse("isDead"));
    }

    IEnumerator AnimationFalse( string str ) {
        yield return new WaitForSeconds(0.1f);
        ani.SetBool(str, false);
    }

    public override void ChangeHp( float value ) {
        if (UnityEngine.XR.XRSettings.isDeviceActive) {
            hp += value;
        }
        else {
            photonView.RPC("NormalEnemyChangeHpRPC", RpcTarget.AllBuffered, value);
        }
    }

    [PunRPC]
    public void NormalEnemyChangeHpRPC( float value ) {
        hp += value;
        EnemyDead();
    }
}
