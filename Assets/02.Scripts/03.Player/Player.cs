using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static InputKeyManager;

[RequireComponent(typeof(CharacterController))]
public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    public delegate void PlayerJumpedHandler();
    public static event PlayerJumpedHandler OnPlayerRotation, OnPlayerJump;


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
        // �̺�Ʈ ���
        OnPlayerMove += PlayerMove;             // �÷��̾� �̵� 
        OnPlayerRotation += PlayerRotation;     // �÷��̾�  ȸ��
        OnPlayerJump += PlayerJump;             // �÷��̾� ���� 
        OnPlayerAttack += PlayerAttack;         // �÷��̾� ����
    }

    void OnDisable() {
        // �̺�Ʈ ���� 
        OnPlayerMove -= PlayerMove;
        OnPlayerRotation -= PlayerRotation;
        OnPlayerJump -= PlayerJump;
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
                // ���� �����̰� �� �� ���� �Ѿ��� �߻�
                if (Time.time - lastAttackTime >= attackMaxDelay) {             
                    bool isAttack = isAtkDistance;
                    OnPlayerAttack?.Invoke(isAttack);
                    lastAttackTime = Time.time;         // ������ �ʱ�ȭ
                }
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Jump))) {
                OnPlayerJump?.Invoke();
            }
        }
    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine) {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
            OnPlayerRotation?.Invoke();
        }
    }

    void OnTriggerEnter( Collider other ) {
        
    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // �÷��̾� �̵� ( �޸��� ���ΰ� check bool ) 
    public override void PlayerMove(bool type) {
        if (PV.IsMine) {
            float x = 0f;   
            float z = 0f;

            // �ȱ�, �޸��� �ӵ� ����
            float playerSpeed = type ? runSpeed : speed;

            // �¿� �̵�
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.LeftMove))) 
                x = -1f;
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.RightMove)))
                x = 1f;

            // ���� �̵�         
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.DownMove)))
                z = -1f;
            else if(Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.UpMove)))
                z = 1f;

            // ���߿� ���ִ��� Ȯ��
            if (!characterController.isGrounded) {
                moveForce.y += gravity * Time.deltaTime;
            }

            MoveTo(new Vector3(x,0,z), playerSpeed);
            characterController.Move(moveForce * Time.deltaTime);
        }
    }

    // �̵� ���� �� �� ���� 
    public void MoveTo(Vector3 direction, float speed) {
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);
        moveForce = new Vector3(direction.x * speed, moveForce.y, direction.z * speed);
    }
    // �÷��̾� ȸ��
    public void PlayerRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }


    // �÷��̾� ���� 
    public override void PlayerJump(){
        if (PV.IsMine) {         
            // ���� �پ����� �� ����
            if(characterController.isGrounded)  {
                moveForce.y = jumpForce;
            }
        }
    }


    // �÷��̾� ��ȣ�ۿ� 
    public override void PlayerInteraction() {
        throw new System.NotImplementedException();
    }

    // �÷��̾� ���� ( �������� ���Ÿ����� �Ǵ� bool ) 
    public override void PlayerAttack( bool type ) {
        if (PV.IsMine) {
            if (!type) {        // ���Ÿ� ���� 
                // ī�޶� �߾ӿ��� Ray ���� 
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                // Ray �׽�Ʈ 
                Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red);                       // ���߿� �����
                // �浹 Ȯ�� �� �Ѿ� ���� 
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                    Debug.Log("�� ����");
                    // �Ѿ� ���� 
                    // �ִϸ��̼� ���� 
                    // ��ƼŬ �Ǵ� ��������Ʈ �̹����� ����Ʈ ǥ�� 
                    // �������� Bullet���� ó�� ü���� ���񿡼� ���� ��ų ����
                }
            }
            else {              // �ٰŸ� ���� 
                Debug.Log("�ٰŸ� ����");
                // �ٰŸ� ���� �ִϸ��̼� 
                // �������� weapon���� �ٲ��� �׸��� ü���� ���񿡼� ���ҽ�ų����
            }
        }
    }

    // ���� ��ü
    public override void WeaponSwap() {
        int itemID = 0;
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

    // �÷��̾� ����ȭ
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            // ������ ���� ( ����ȭ ) 
            stream.SendNext(rigid.position);    // ��ġ 
            stream.SendNext(rigid.rotation);    // ȸ��
            stream.SendNext(rigid.velocity);    // �ӵ� 
        }
        else {
            // ������ ����
            rigid.position = (Vector3)stream.ReceiveNext();
            rigid.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }
}
