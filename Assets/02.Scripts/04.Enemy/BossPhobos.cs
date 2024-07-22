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
                ChangeHp(value);                   
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
    public GameObject[] players;
    public float swingAtkDistance = 200;
    public float dashAtkDistance = 600;
    public float dashPower = 30;

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
        StartCoroutine(Trace());
        //���� �߰��� �ؿ� �ο�����ŭforeach
        Physics.IgnoreCollision(players[0].GetComponent<Collider>(), transform.GetComponent<Collider>(), true);
    }

   
    
    IEnumerator Trace()
    {
        while(state != State.dead)             //���׾����� 0.5�ʸ��� �������ִ� �÷��̾� �߰�
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
            transform.LookAt(closestPlayer);
            if((closestPlayer.position - transform.position).sqrMagnitude < swingAtkDistance)
            {
                StartCoroutine(AtkPattern());
                yield break;
            }
            else if((closestPlayer.position - transform.position).sqrMagnitude > dashAtkDistance)
            {
                StartCoroutine(DashPattern());
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
            ani.SetBool("isSwing", true);
            StartCoroutine(AnimationFalse("isSwing"));
            yield return new WaitForSeconds(2.5f);
        }
        else if(randomNum < 90)
        {
            ani.SetBool("isShockWave", true);
            StartCoroutine(AnimationFalse("isShockWave"));
            yield return new WaitForSeconds(3.75f);
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
        StopCoroutine(Trace());
        ani.SetBool("isDash", true);
        StartCoroutine(AnimationFalse("isDash"));

        yield return new WaitForSeconds(1.0f);
        //int randomPlayer = random.range(0, �ο���);
        nav.velocity = (players[0].transform.position - transform.position).normalized * dashPower;
        
        yield return new WaitForSeconds(1.5f);
        nav.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.625f);

        StartCoroutine(Trace());
    }

    IEnumerator ThunderPattern()
    {
        StopCoroutine(Trace());
        ani.SetBool("isThunder", true);
        StartCoroutine(AnimationFalse("isThunder"));
        yield return new WaitForSeconds(7.0f);
    }







    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))             // �Ѿ˰� trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // ��������� trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
        }
        else if (other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }else if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().rigid.AddForce((other.transform.position - transform.position).normalized * 3.0f, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        
    }


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
}
