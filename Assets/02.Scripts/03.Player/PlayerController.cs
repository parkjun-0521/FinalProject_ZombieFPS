using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IPlayer {
    void PlayerMove(bool type);         // �÷��̾� �̵� 
    void PlayerJump();                  // �÷��̾� ���� 
    void PlayerInteraction();           // �÷��̾� ��ȣ�ۿ�
    void PlayerAttack(bool type);       // �÷��̾� ���� 
    void WeaponSwap();                  // ���� ��ü
    void ItemThrowAway(int id);         // ������ ������
    void ItemThrow();                   // ������ ������ 
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
    private float hp;
    public float PlayerHp {  get { return hp; } set { hp = value; } }

    [Header("����=======")]
    public float attackMaxDelay = 0.1f; // ���Ÿ� ���� ������ ( ������ ������ ���� weapon�� ����� �ű⼭ �ҷ��ͼ� ���� ) 
    [HideInInspector]
    public float lastAttackTime = 0.0f; // ������ ���� �ð� 

    [Header("���º���=======")]
    public bool isAtkDistance;          // ���� �Ÿ� ( false ���Ÿ�, true �ٰŸ� ) 
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
    public CharacterController characterController;

    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void WeaponSwap();
    public abstract void ItemThrowAway( int id );
    public abstract void ItemThrow();
    public abstract void PlayerDead();
    // ���� ����ȭ �޼ҵ�
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );
}
