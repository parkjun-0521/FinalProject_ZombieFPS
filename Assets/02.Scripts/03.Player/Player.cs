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
        if (PV.IsMine) {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    void OnTriggerEnter( Collider other ) {

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
        throw new System.NotImplementedException();
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
}
