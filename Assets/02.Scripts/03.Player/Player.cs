using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    public delegate void PlayerJumpedHandler();
    public static event PlayerJumpedHandler OnPlayerRotation, OnPlayerJump, OnPlayerSwap;

    private RotateToMouse rotateToMouse;
    private InputKeyManager keyManager;

    public Camera playerCamera;

    private bool cursorLocked = true;

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        if (PV.IsMine) {
            rigid = GetComponent<Rigidbody>();

            Cursor.visible = false;                         // ���콺 Ŀ�� ��Ȱ��ȭ
            Cursor.lockState = CursorLockMode.Locked;       // ���콺 Ŀ�� ���� ��ġ ���� 
            rotateToMouse = GetComponentInChildren<RotateToMouse>();
        }
    }

    void OnEnable() {
        // �̺�Ʈ ���
        OnPlayerMove += PlayerMove;             // �÷��̾� �̵� 
        OnPlayerRotation += PlayerRotation;     // �÷��̾�  ȸ��
        OnPlayerJump += PlayerJump;             // �÷��̾� ���� 
        OnPlayerAttack += PlayerAttack;         // �÷��̾� ����
        OnPlayerSwap += WeaponSwap;             // ���� ��ü
    }

    void OnDisable() {
        // �̺�Ʈ ���� 
        OnPlayerMove -= PlayerMove;
        OnPlayerRotation -= PlayerRotation;
        OnPlayerJump -= PlayerJump;
        OnPlayerAttack -= PlayerAttack;
        OnPlayerSwap -= WeaponSwap;
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
            OnPlayerSwap?.Invoke();
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack))) {
                // ��,Į 0.1��, ����ź,���� 1�� ������
                attackMaxDelay = stanceWeaponType ? 1.0f : 0.1f;
                // ���� �����̰� �� �� ���� �Ѿ��� �߻�
                if (Time.time - lastAttackTime >= attackMaxDelay) {
                    OnPlayerAttack?.Invoke(isAtkDistance);
                    lastAttackTime = Time.time;         // ������ �ʱ�ȭ
                }
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Jump)) && isJump) {
                OnPlayerJump?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
                ToggleCursor();
            }

            if (cursorLocked) {
                bool isRun = Input.GetKey(KeyCode.LeftShift);
                OnPlayerMove?.Invoke(isRun);

                float z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;
                float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;
                transform.Translate(x, 0, z);
            }
        }
    }
    private void ToggleCursor() {
        cursorLocked = !cursorLocked;
        Cursor.visible = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine) {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
            OnPlayerRotation?.Invoke();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (PV.IsMine) {
            if (collision.gameObject.CompareTag("Ground")) // ���� �±� ���� �ʿ�
            {
                isJump = true;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (PV.IsMine) {
            if (collision.gameObject.CompareTag("Ground")) {
                isJump = false;
            }
        }
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
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.UpMove)))
                z = 1f;

            Vector3 moveDirection = (transform.forward * z + transform.right * x).normalized;
            rigid.MovePosition(transform.position + moveDirection * playerSpeed * Time.deltaTime);
        }
    }

    // �÷��̾� ȸ��
    public void PlayerRotation() {
        if (PV.IsMine) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            rotateToMouse.UpdateRotate(mouseX, mouseY);
        }
    }


    // �÷��̾� ���� 
    public override void PlayerJump() {
        // ���� �پ����� �� ����
        if (PV.IsMine) {
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJump = false;
        }
    }


    // �÷��̾� ��ȣ�ۿ� 
    public override void PlayerInteraction() {
        throw new System.NotImplementedException();
    }

    // �÷��̾� ���� ( �������� ���Ÿ����� �Ǵ� bool ) 
    public override void PlayerAttack(bool type) {
        // ���� �Ÿ��� ���� ���� 
        if (PV.IsMine) {
            if (type)
                // �ٰŸ� ����
                (stanceWeaponType ? (Action)ItemHealpack : SwordAttack)();      // ���� : ��������
            else
                // ���Ÿ� ����
                (stanceWeaponType ? (Action)ItemGrenade : GunAttack)();         // ����ź : ���Ÿ� ���� 
        }
    }

    // ���� Į
    void SwordAttack()
    {
        if (PV.IsMine) {
            Debug.Log("Į ����");
            // �ٰŸ� ���� �ִϸ��̼� 
            // �������� weapon���� �ٲ��� �׸��� ü���� ���񿡼� ���ҽ�ų����
        }
    }

    // ���Ÿ� ��
    void GunAttack()
    {
        if (PV.IsMine) {
            // ī�޶� �߾ӿ��� Ray ���� 
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Ray �׽�Ʈ 
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red);                       // ���߿� �����

            Vector3 targetPoint;

            // �浹 Ȯ��
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                Debug.Log("�� ����");
                targetPoint = hit.point;
            }
            else {
                // ���̰� ���� �ʾ��� ���� �� ������ ��ǥ�� ����
                targetPoint = ray.origin + ray.direction * 1000f;
            }

            // �Ѿ� ���� (������Ʈ Ǯ�� ���)
            GameObject bullet = Pooling.instance.GetObject(0); // �Ѿ��� �� �ִ� index�� ���� (0�� �ӽ�)
            bullet.transform.position = bulletPos.position; // bullet ��ġ �ʱ�ȭ                   
            bullet.transform.rotation = Quaternion.identity; // bullet ȸ���� �ʱ�ȭ 

            // �Ѿ��� ���� ����
            Vector3 direction = (targetPoint - bulletPos.position).normalized;

            // �Ѿ˿� ���� ���Ͽ� �߻�
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(direction * 300f, ForceMode.Impulse);
        }
    }


    // �ٰŸ� ������ ���� 
    void ItemHealpack()
    {
        if (PV.IsMine) {
            Debug.Log("����");
        }
    }

    // ���Ÿ� ������ ����ź  
    void ItemGrenade()
    {
        if (PV.IsMine) {
            Debug.Log("��ô ����");
        }
    }

    // ���� ��ü
    public override void WeaponSwap() {
        if (PV.IsMine) {
            int weaponIndex = -1;           // �ʱ� ���� �ε��� ( ��� ) 
            bool weaponSelected = false;    // ���Ⱑ ���õǾ����� Ȯ�� 

            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon1))) {        // ���Ÿ� ���� 
                weaponIndex = 0;
                isAtkDistance = stanceWeaponType = false;
                Debug.Log("���Ÿ�");
                weaponSelected = true;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // ���� ����
                weaponIndex = 1;
                isAtkDistance = true;
                stanceWeaponType = false;
                Debug.Log("�ٰŸ�");
                weaponSelected = true;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // ��ô ����
                weaponIndex = 2;
                isAtkDistance = false;
                stanceWeaponType = true;
                Debug.Log("��ô");
                weaponSelected = true;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // ����
                weaponIndex = 3;
                isAtkDistance = stanceWeaponType = true;
                Debug.Log("��");
                weaponSelected = true;
            }

            if (!weaponSelected) return;

            if (equipWeapon != null)
                equipWeapon.SetActive(false);
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);
        }
    }

    // ������ ������ ( ������ item id�������� )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // �÷��̾� ���
    public override void PlayerDead() {
        throw new System.NotImplementedException();
    }

    // �÷��̾� ����ȭ
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        /*if (stream.IsWriting) {
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
        }*/
    }
}
