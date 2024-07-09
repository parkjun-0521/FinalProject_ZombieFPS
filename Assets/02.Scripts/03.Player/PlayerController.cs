using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

interface IPlayer {
    void PlayerMove(bool type);         // �÷��̾� �̵� 
    void PlayerJump();                  // �÷��̾� ���� 
    void PlayerInteraction();           // �÷��̾� ��ȣ�ۿ�
    void PlayerAttack(bool type);       // �÷��̾� ���� 
    void WeaponSwap();                  // ���� ��ü
    void ItemThrowAway(int id);         // ������ ������
    void PlayerDead();                  // �÷��̾� ��� 
}

public abstract class PlayerController : MonoBehaviourPun, IPlayer, IPunObservable
{
    [Header("�̵�=======")]
    // �÷��̾� �̵��ӵ� 
    [SerializeField]
    protected float speed;

    // �÷��̾� �޸��� �ӵ�
    [SerializeField]
    protected float runSpeed;

    [Header("����=======")]
    // �÷��̾� ���� �� 
    [SerializeField]
    protected float jumpForce;

    // �߷� ��� ( ���� �� ���� )
    [SerializeField]
    protected float gravity;

    [Header("ü��=======")]
    // Player ü�� 
    [SerializeField]
    protected float interactionRange = 2.0f;               //��ȣ�ۿ��ִ�Ÿ�
    protected float hp = 100.0f;                              //�÷��̾�hp 
    public abstract float Hp { get; set; }                 //hp ������Ƽ 
    protected bool isFaint = false;                        //�����ߴ��� true false
    public Image bloodScreen;                             //�ǰݽ� ȭ�� ������ �� �̹���
    public Image healScreen;                      //ġ���� ȭ�� ���λ� �� �̹���

    [Header("����=======")]
    public GameObject[] weapons;        // ���� ���� ( ������� �־���� ��, 0 : ���Ÿ�, 1 : ����, 2 : ��ô, 3 : ���� )
    [HideInInspector]
    public GameObject equipWeapon;      // ��� �ִ� ���� ( ���� �� ��Ȱ��ȭ ���ֱ� ���� ) 
    public Transform bulletPos;         // �Ѿ� ������ ��ġ 
    public float attackMaxDelay = 0.1f; // ���Ÿ� ���� ������ ( ������ ������ ���� weapon�� ����� �ű⼭ �ҷ��ͼ� ���� ) 
    [HideInInspector]
    public float lastAttackTime = 0.0f; // ������ ���� �ð� 

    [Header("���º���=======")]
    public bool isAtkDistance;          // ���� �Ÿ� ( false ���Ÿ�, true �ٰŸ� ) 
    public bool stanceWeaponType;       // ���Ÿ� ( false : ���Ÿ�, true : ��ô ), �ٰŸ� ( false : ����, true :  ���� ) ���� ���� 
    protected bool isJump;              // ���� ����
    protected bool isInteraction;       // ��ȣ�ۿ� ���� 
    protected bool isAttack;            // ���� ���� 
    protected bool isThrow;             // ������ ������ ���� 
    protected bool isDead;              // ��� ���� 

    // ��� ���� �� ������ ����ó���� �ʿ��� Bool ���� ���� 
    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ����   

    public Vector3 moveForce;           // Player �̵� ���� �� �� 
    public LayerMask enemyLayer;        // Enemy Layer ���� 

    [Header("������Ʈ=======")]
    public PhotonView PV;               // ���� ( ����ȭ ���� �� ���� ���� ) 
    public Rigidbody rigid;
    public Animator animator;

    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void WeaponSwap();
    public abstract void ItemThrowAway( int id );
    public abstract void PlayerDead();
    // ���� ����ȭ �޼ҵ�
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );

    protected abstract void ChangeHp(float value);
    protected abstract void PlayerFaint();
    protected abstract IEnumerator ShowBloodScreen();
    protected abstract IEnumerator ShowHealScreen();
    public abstract void PlayerRevive();                //�ܺο��� ���ٿ��� �ҰŶ� public
}
