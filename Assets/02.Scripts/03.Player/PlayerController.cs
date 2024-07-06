using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IPlayer {
    void PlayerMove(bool type);         // �÷��̾� �̵� 
    void PlayerJump();                  // �÷��̾� ���� 
    void PlayerInteraction();           // �÷��̾� ��ȣ�ۿ�
    void PlayerAttack(bool type);       // �÷��̾� ���� 
    void ItemThrowAway(int id);         // ������ ������
    void ItemThrow();                   // ������ ������ 
    void PlayerDead();                  // �÷��̾� ��� 
}

public abstract class PlayerController : MonoBehaviourPun, IPlayer, IPunObservable
{
    // �÷��̾� �̵��ӵ� 
    [SerializeField]
    protected float speed;

    // �÷��̾� �޸��� �ӵ�
    [SerializeField]
    protected float runSpeed;

    // �÷��̾� ���� �� 
    [SerializeField]
    protected float jumpForce;

    // �߷� ��� ( ���� �� ���� )
    [SerializeField]
    protected float gravity;

    // Player ü�� 
    private float hp;
    public float PlayerHp {  get { return hp; } set { hp = value; } }

    protected bool isJump;              // ���� ����
    protected bool isInteraction;       // ��ȣ�ۿ� ���� 
    protected bool isAttack;            // ���� ���� 
    public bool isAtkDistance;          // ���� �Ÿ� ( false ���Ÿ�, true �ٰŸ� ) 
    protected bool isThrow;             // ������ ������ ���� 
    protected bool isDead;              // ��� ���� 

    // ��� ���� �� ������ ����ó���� �ʿ��� Bool ���� ���� 
    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ����   

    public Vector3 moveForce;           // Player �̵� ���� �� �� 
    public LayerMask enemyLayer;        // Enemy Layer ���� 

    public PhotonView PV;               // ���� ( ����ȭ ���� �� ���� ���� ) 
    public Rigidbody rigid;             
    public CharacterController characterController;

    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void ItemThrowAway( int id );
    public abstract void ItemThrow();
    public abstract void PlayerDead();
    // ���� ����ȭ �޼ҵ�
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );

 
}
