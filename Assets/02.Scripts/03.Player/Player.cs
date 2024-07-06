using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    private PhotonView PV;
    private Rigidbody rigid;
    private CharacterController characterController;

    InputKeyManager keyManager;

    
    void Awake() {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        keyManager = InputKeyManager.instance.GetComponent<InputKeyManager>();
    }

    void OnEnable() {
        OnPlayerMove += PlayerMove;
        OnPlayerAttack += PlayerAttack;
    }
    void OnDisable() {
        OnPlayerMove -= PlayerMove;
        OnPlayerAttack -= PlayerAttack;
    }

    void Start() {
        // ���� �ʱ�ȭ 
    }


    void Update() {
        // �ܹ����� �ൿ 
       /* bool isAttack = Input.GetMouseButton(0);
        OnPlayerAttack?.Invoke(isAttack);*/

    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine)
        {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    void OnTriggerEnter( Collider other )                       //���� Ʈ�����ݶ��̴��� enter������
    {
        if(other.tag == "Enemy")                                //��������� enemy������ ·�� �±� ����
        {
            //hp = -(other.GetComponent<Enemy>().attackdamage)  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
        }
    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // �÷��̾� �̵� ( �޸��� ���ΰ� check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("�÷��̾� �̵�");
        if (PV.IsMine) {
            float z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;               
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;            

            transform.Translate(x, 0, z);
        }
    }

    // �÷��̾� ���� 
    public override void PlayerJump() {
        throw new System.NotImplementedException();
    }


    // �÷��̾� ��ȣ�ۿ� 
    public override void PlayerInteraction() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit, interactionRange, LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("Item")))   //���̾� �̸�, �Ÿ������� ����
        {
            if(hit.collider.tag == "Item")                                  //���� �������̸�
            {
                //ex)text : 'E' �������ݱ� ui����ֱ�
                //hit.collider.GetComponent<Item>.itemCode
            }
            else if(hit.collider.tag == "Player")
            {
                if(hit.collider.GetComponent<Player>().isFaint == true)     //���� �±װ� player�� ������ true��
                {
                    //ex)text : 'E' �÷��̾� �츮�� ui����ֱ�
                    //slider or shader�� (slider�� ���ҵ�) ����ֱ�
                }
            }
        }
    }

    // �÷��̾� ���� ( �������� ���Ÿ����� �Ǵ� bool ) 
    public override void PlayerAttack( bool type ) {
        Debug.Log("����");
    }

    // ������ ������ ( ������ item id�������� )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // ������ ������ ( ����ź ) 
    public override void ItemThrow() {
        throw new System.NotImplementedException();
    }

    // �÷��̾� ���
    public override void PlayerDead() {
        throw new System.NotImplementedException();
    }
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            // ������ ����
            stream.SendNext(rigid.position);
            stream.SendNext(rigid.rotation);
            stream.SendNext(rigid.velocity);
        }
        else {
            // ������ ����
            rigid.position = (Vector3)stream.ReceiveNext();
            rigid.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }



    protected override void ChangeHp(float value)
    {
        hp += value;
        if(value > 0)
        {
            StartCoroutine(ShowHealScreen());   //�� ȭ�� ���
        }
        else if(value < 0)
        {
            StartCoroutine(ShowBloodScreen());  //�ǰ�ȭ�� ��� 
        }
    }

    protected override void PlayerFaint()       
    {
        if (hp <= 0)                            //���� �÷��̾� ü���� 0���� �۾�����
        {
            hp = 0;                             //���⼭ hp�� 0 ���� ������ ����� ��Ȱ, ui���� ���ҰŰ���
            isFaint = true;                     //�������� true
            OnPlayerMove -= PlayerMove;         //�����̴°Ÿ���
            OnPlayerAttack -= PlayerAttack;     //���ݵ� ���ڴ�
            //anim.setbool("isFaint", true);    //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
        }
    }

    public override void PlayerRevive()      //�÷��̾� ��Ȱ - �ٸ��÷��̾ ��Ȱ�Ҷ� ���� player�� �����ؼ� ȣ�� ���ο��� �Ⱦ����� �������⸦�������ʴ��̻�...
    {                                        //PlayerFaint �Լ��� �ݴ�� �ϸ� ��
        isFaint = false;                     //�������� false
        OnPlayerMove += PlayerMove;          //�����̴°� ���ϰ�
        OnPlayerAttack += PlayerAttack;      //���ݵ� ���� 
        //anim.setbool("isFaint", false);    //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
        Hp = 50;                             //��Ȱ�� ���Ƿ� ����! maxHp = 100; �� ���� �����ؼ� maxHp / 2�ص� �Ǵµ� Ǯ�Ǵ� 100�ϰ��� ��
    }

    protected override IEnumerator ShowBloodScreen()                        //ȭ�� �Ӱ�
    {
        bloodScreen.color = new Color(1, 0, 0, Random.Range(0.1f, 0.15f));  //�û��Ӱ� ����
        yield return new WaitForSeconds(0.5f);                              //0.5f�� �Ŀ�   - �̰� ������ ����?
        bloodScreen.color = Color.clear;                                    //ȭ�� ���������� ����!
    }
    protected override IEnumerator ShowHealScreen()                         //ȭ�� ���λ� 
    {
        float curTime = Time.time;                                          //������ �ð��� ������ �������ϰ�
        healScreen.color = new Color(1, 1, 1, 1);                           //�� �ϴ� �̹����� ����
        while(true)                                                         //�ݺ��� 1���Ŀ� ����
        {
            yield return new WaitForSeconds(0.1f);                          //0.1�ʸ���           
            float lerpColor = 1 - (Time.time - curTime);                    
            healScreen.color = new Color(1, 1, 1, lerpColor);               //���� ���ݾ� �������ٰ� �����ϸ��(����)
            if (lerpColor <= 0)                                             //1���Ŀ�
            {
                healScreen.color = Color.clear;                             //�÷��� 0000���� �ؼ� ���������� �����ϰ�
                break;                                                      //break�� �ݺ��� Ż��
            }
        }
    }

    public override float Hp                         //hp ������Ƽ
    {
        get
        {
            return hp;                               //�׳� ��ȯ
        }
        set
        {
            ChangeHp(value);                         //hp�� value��ŭ ���� �� ���ط��� ������ϸ� ���̵� ������ �ؾ��� ���⼭ ȭ�� �û��Ӱ� and ���λ�����
            PlayerFaint();                           //���� hp�� ���������� ü���� 0���� ������ ����
            Debug.Log("�÷��̾� hp ����" + hp);
        }
    }

    
    [ContextMenu("������Ƽ--")]                      //TEST�� ���Ļ���
    void test()
    {
        Hp = -1;
    }
    [ContextMenu("������Ƽ++")]                      //TEST�� ���Ļ���
    void test2()
    {
        Hp = +1;
    }
}
