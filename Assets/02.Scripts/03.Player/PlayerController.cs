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

    // Player ü�� 
    [SerializeField]
    protected float interactionRange = 2.0f;               //��ȣ�ۿ��ִ�Ÿ�
    protected float hp = 100.0f;                              //�÷��̾�hp 
    public abstract float Hp { get; set; }                 //hp ������Ƽ 
    protected bool isFaint = false;                        //�����ߴ��� true false
    public Image bloodScreen;                             //�ǰݽ� ȭ�� ������ �� �̹���
    public Image healScreen;                      //ġ���� ȭ�� ���λ� �� �̹���
 
    
    protected bool isWalk;              // �Ȱ��ִ� ���� 
    protected bool isRun;               // �޸��� ���� 
    protected bool isJump;              // ���� ����
    protected bool isInteraction;       // ��ȣ�ۿ� ���� 
    protected bool isAttack;            // ���� ���� 
    protected bool isThrow;             // ������ ������ ���� 
    protected bool isDead;              // ��� ���� 

    
    public abstract void PlayerMove(bool type);
    public abstract void PlayerJump();
    public abstract void PlayerInteraction();
    public abstract void PlayerAttack( bool type );
    public abstract void ItemThrowAway( int id );
    public abstract void ItemThrow();
    public abstract void PlayerDead();
    public abstract void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info );





    protected abstract void ChangeHp(float value);
    protected abstract void PlayerFaint();
    protected abstract IEnumerator ShowBloodScreen();
    protected abstract IEnumerator ShowHealScreen();
    public abstract void PlayerRevive();                //�ܺο��� ���ٿ��� �ҰŶ� public
}
