using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    private RotateToMouse rotateToMouse;
    private InputKeyManager keyManager;

    public Camera playerCamera;

    void Awake() {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        Cursor.visible = false;                         // ���콺 Ŀ�� ��Ȱ��ȭ
        Cursor.lockState = CursorLockMode.Locked;       // ���콺 Ŀ�� ���� ��ġ ���� 
        rotateToMouse = GetComponentInChildren<RotateToMouse>();
       
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
        if (PV.IsMine) {
            keyManager = InputKeyManager.instance.GetComponent<InputKeyManager>();
            playerCamera.gameObject.SetActive(true);
        }
        else {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update() {
        // �ܹ����� �ൿ 
        if (PV.IsMine) {
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack))){
                bool isAttack = isAtkDistance;
                OnPlayerAttack?.Invoke(isAttack);
            }
        }
    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            UpdateRotate(mouseX, mouseY);
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    public void UpdateRotate(float mouseX, float mouseY) {
        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    void OnTriggerEnter( Collider other ) {
        
    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // �÷��̾� �̵� ( �޸��� ���ΰ� check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("�÷��̾� �̵�");
        if (PV.IsMine) {
            characterController.Move(moveForce * Time.deltaTime);
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            MoveTo(new Vector3(x,0,z));
        }
    }

    public void MoveTo(Vector3 direction) {
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);
        moveForce = new Vector3(direction.x * speed, moveForce.y, direction.z * speed);
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
        if (!type) {        // ���Ÿ� ���� 
            Debug.Log("���Ÿ� ����");
        }
        else {              // �ٰŸ� ���� 
            Debug.Log("�ٰŸ� ����");
        }
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
