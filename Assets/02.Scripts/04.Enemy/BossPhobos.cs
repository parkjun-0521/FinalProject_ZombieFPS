using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class BossPhobos : EnemyController
{

    public override float Hp
    {
        get
        {
            return hp;                          //�׳� ��ȯ
        }
        set
        {
            if (state == State.dead) return;
            if (hp > 0)
            {

                PV.RPC("ChangeHp", RpcTarget.AllBuffered, value);
                //ChangeHp(value);                   
                Debug.Log(hp);
    
            }
        }
    }
    enum State
    {
        move,
        dead

    }
    State state = State.move;
    [Header("������ �ν�����")]
    [SerializeField] private GameObject[] players;
    [SerializeField] private float swingAtkDistance = 200;
    [SerializeField] private float dashAtkDistance = 900;
    [SerializeField] private float dashPower = 30;
    [SerializeField] private float knockBackPower = 3.0f;
    [Header("�������� ������")]
    [SerializeField] private float swingDamage = 10;
    [SerializeField] private float shockwaveDamage = 20;
    [SerializeField] private float dashDamage = 30;
    
    private Collider[] playerCollider;
    private float defalutKnockBackPower = 3.0f;
    private bool isLook;
    [Space(20)]
    [SerializeField] private float traceTime = 0;
    [SerializeField] private float traceChacgeTime = 15.0f;
    [SerializeField] [Header("�νĹ���")] private float lookRadius = 10;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponentInChildren<Animator>();
        //if(!PhotonNetwork.IsMasterClient)
        //{
        //    nav.enabled = false;
        //}
    }
    
    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        //if(PhotonNetwork.IsMasterClient)
        //{
        //    StartCoroutine(Trace());
        //}
        //StartCoroutine(Trace());
        //���� �߰��� �ؿ� �ο�����ŭforeach
        //foreach(var player in players)
        //{
        //    Physics.IgnoreCollision(player.GetComponent<Collider>(), transform.GetComponent<Collider>(), true);
        //}
        //Physics.IgnoreCollision(players[0].GetComponent<Collider>(), transform.GetComponent<Collider>(), true);
    }


    private void Update()
    {
        if (!PV.IsMine) return;

        traceTime += Time.deltaTime;
        playerCollider = Physics.OverlapSphere(transform.position, lookRadius, LayerMask.GetMask("LocalPlayer"));
        if(playerCollider.Length > 0 && !isLook)
        {
            StartCoroutine(Trace());
            ani.SetBool("isDash", true);
            isLook = true;
        }
    }


    IEnumerator Trace()
    {
        while (state != State.dead)             //���׾����� 0.5�ʸ��� �������ִ� �÷��̾� �߰�
        {
            Vector3 closestPlayer = players[0].transform.position;
            foreach (GameObject player in players)
            {
                Vector3 playerTr = player.transform.position;
                float playerDistance = ((playerTr - transform.position).sqrMagnitude);
                if ((closestPlayer - transform.position).sqrMagnitude >= playerDistance)
                    closestPlayer = playerTr;
            }
            nav.SetDestination(closestPlayer);
            transform.LookAt(closestPlayer);
            if((closestPlayer - transform.position).sqrMagnitude < swingAtkDistance)
            {
                StartCoroutine(AtkPattern());
                yield break;
            }
            else if((closestPlayer - transform.position).sqrMagnitude > dashAtkDistance || traceTime > traceChacgeTime)
            {
                StartCoroutine(DashPattern());
                traceTime = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);

        }
    }

    IEnumerator AtkPattern()
    {
        StopCoroutine(Trace());
        int randomNum = Random.Range(0, 100);
        if(randomNum < 70)
        {
            damage = swingDamage;
            knockBackPower = 5.0f;
            ani.SetBool("isSwing", true);
            StartCoroutine(AnimationFalse("isSwing"));

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack4))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack4);
            }

            yield return new WaitForSeconds(2.5f);
            knockBackPower = defalutKnockBackPower;

        }
        else if(randomNum < 90)
        {
            damage = shockwaveDamage;
            knockBackPower = 5.0f;
            ani.SetBool("isShockWave", true);
            StartCoroutine(AnimationFalse("isShockWave"));

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack5))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack5);
            }

            yield return new WaitForSeconds(3.75f);
            knockBackPower = defalutKnockBackPower;
        }
        else if (randomNum < 100)
        {
            StartCoroutine(DashPattern());
            yield break;
        }
        //else if(randomNum < 100)
        //{
        //    ani.SetBool("isThunder", true);
        //    StartCoroutine(AnimationFalse("isThunder"));
        //    yield return new WaitForSeconds(2.0f);
        //    foreach (GameObject player in players)
        //    {
        //         player.transform.position + Random.insideUnitSphere......
        //    }
        //}



        StartCoroutine(Trace());
    }

    IEnumerator DashPattern()
    {
        damage = dashDamage;
        knockBackPower = 10.0f;
        StopCoroutine(Trace());
        ani.SetBool("isDash", true);
        StartCoroutine(AnimationFalse("isDash"));

        yield return new WaitForSeconds(1.0f);
        int randomPlayer = Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount);
        nav.velocity = (players[randomPlayer].transform.position - transform.position).normalized * dashPower;
        nav.destination = players[randomPlayer].transform.position;

        if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_attack4))
        {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_attack4);
        }

        yield return new WaitForSeconds(1.5f);
        nav.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.625f);
        knockBackPower = defalutKnockBackPower;
        StartCoroutine(Trace());
    }








    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon")) {
            if (gameObject.CompareTag("EnemyRange")) return;

            Hp = -(other.transform.parent.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_hurt)) {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_hurt);
            }
        }
        else if (other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }
        else if(other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().rigid == null) return; //
            if (other.GetComponent<Rigidbody>() == null) return;
            other.GetComponent<PlayerController>().rigid.AddForce((other.transform.position - transform.position).normalized * knockBackPower, ForceMode.Impulse);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(swingAtkDistance));
        Gizmos.DrawWireSphere(transform.position, Mathf.Sqrt(dashAtkDistance));
    }

    [PunRPC]
    public override void ChangeHp(float value)
    {
        hp += value;
        //if (value > 0)
        //{
        //    //���� ü��ȸ�������� �������� ���߿� ���� or ����Ʈ ���� �ֺ��� ȸ���Ҽ��������� Ȯ�强���� ����
        //    //�� ���� �ֺ��� +��� ��ƼŬ����
        //}
        //else if (value < 0)
        //{
        //    //���ݸ�����
        //    //ani.setTrigger("�ǰݸ��");
        //}
        if (hp <= 0)
        {
            EnemyDead();
        }
    }

    public override void EnemyDead()
    {
        if (hp <= 0)
        {
            ani.SetBool("isDead", true);
            nav.enabled = false;
            StopAllCoroutines();
            StartCoroutine(AnimationFalse("isDead"));
            state = State.dead;

            if (!AudioManager.Instance.IsPlaying(AudioManager.Sfx.Zombie_dead1))
            {
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Zombie_dead1);
            }

            if (photonView != null) {
                photonView.RPC("QuestCompleteRPC", RpcTarget.AllBuffered, true);
            }

            //��������Ʈ �ٸ��� �� ����
            //?���Ŀ�
            //gameObject.SetActive(false); 
        }
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.2f);
        ani.SetBool(str, false);
    }

    [PunRPC]
    public void QuestCompleteRPC(bool isTrue)
    {
        Debug.Log("RPC ���� ");
        if (ScenesManagerment.Instance.stageCount == 0)
        {
            Debug.Log("RPC ����1 ");
            NextSceneManager.Instance.isQuest1 = isTrue;
        }
        else if (ScenesManagerment.Instance.stageCount == 1)
        {
            Debug.Log("RPC ����2 ");
            NextSceneManager.Instance.isQuest2 = isTrue;
        }
        else if (ScenesManagerment.Instance.stageCount == 2)
        {
            Debug.Log("RPC ����3 ");
            GameObject nextStageZone = GameObject.FindGameObjectWithTag("NextStageZone");
            nextStageZone.GetComponent<BoxCollider>().enabled = true;
            NextSceneManager.Instance.isQuest3 = isTrue;

        }
    }

}
