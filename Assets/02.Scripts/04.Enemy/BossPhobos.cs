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
            return hp;                          //그냥 반환
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
    [Header("보스용 인스펙터")]
    [SerializeField] private GameObject[] players;
    [SerializeField] private float swingAtkDistance = 200;
    [SerializeField] private float dashAtkDistance = 600;
    [SerializeField] private float dashPower = 30;
    [SerializeField] private float knockBackPower = 3.0f;

    private float defalutKnockBackPower = 3.0f;
    [SerializeField]private float traceTime = 0;
    [SerializeField] private float traceChacgeTime = 15.0f;

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
        //포톤 추가시 밑에 인원수만큼foreach
        Physics.IgnoreCollision(players[0].GetComponent<Collider>(), transform.GetComponent<Collider>(), true);
    }

   
    
    IEnumerator Trace()
    {
        while(state != State.dead)             //안죽었으면 0.5초마다 가까이있는 플레이어 추격
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
            else if((closestPlayer.position - transform.position).sqrMagnitude > dashAtkDistance || traceTime > traceChacgeTime)
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
            knockBackPower = 5.0f;
            ani.SetBool("isSwing", true);
            StartCoroutine(AnimationFalse("isSwing"));
            yield return new WaitForSeconds(2.5f);
            knockBackPower = defalutKnockBackPower;
        }
        else if(randomNum < 90)
        {
            knockBackPower = 5.0f;
            ani.SetBool("isShockWave", true);
            StartCoroutine(AnimationFalse("isShockWave"));
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
        knockBackPower = 10.0f;
        StopCoroutine(Trace());
        ani.SetBool("isDash", true);
        StartCoroutine(AnimationFalse("isDash"));

        yield return new WaitForSeconds(1.0f);
        //int randomPlayer = random.range(0, 인원수);
        nav.velocity = (players[0].transform.position - transform.position).normalized * dashPower;
        
        yield return new WaitForSeconds(1.5f);
        nav.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.625f);
        knockBackPower = defalutKnockBackPower;
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
        if (other.CompareTag("Bullet"))             // 총알과 trigger
        {
            Hp = -(other.GetComponent<Bullet>().itemData.damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Weapon"))        // 근접무기와 trigger
        {
            Hp = -(other.GetComponent<ItemSword>().itemData.damage);
            BloodEffect(transform.position);
        }
        else if (other.CompareTag("Grenade"))
        {
            Hp = -(other.GetComponentInParent<ItemGrenade>().itemData.damage);
        }else if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().rigid.AddForce((other.transform.position - transform.position).normalized * knockBackPower, ForceMode.Impulse);
        }
    }

    private void Update()
    {
        traceTime += Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 14.1f);
        Gizmos.DrawWireSphere(transform.position, 24.5f);
    }

    [PunRPC]
    public override void ChangeHp(float value)
    {
        hp += value;
        //if (value > 0)
        //{
        //    //좀비가 체력회복할일은 없겠지만 나중에 보스 or 엘리트 좀비가 주변몹 회복할수도있으니 확장성때매 놔둠
        //    //힐 좀비 주변에 +모양 파티클생성
        //}
        //else if (value < 0)
        //{
        //    //공격맞은거
        //    //ani.setTrigger("피격모션");
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
            //델리게이트 다른거 다 빼기
            //?초후에
            //gameObject.SetActive(false); 
        }
    }

    IEnumerator AnimationFalse(string str)
    {
        yield return new WaitForSeconds(0.2f);
        ani.SetBool(str, false);
    }
}
