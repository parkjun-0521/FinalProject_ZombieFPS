using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    private RotateToMouse rotateToMouse;
    private InputKeyManager keyManager;

    public Camera playerCamera;

    void Awake() {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        Cursor.visible = false;                         // 마우스 커서 비활성화
        Cursor.lockState = CursorLockMode.Locked;       // 마우스 커서 현재 위치 고정 
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
        // 단발적인 행동 
        if (PV.IsMine) {
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack))){
                bool isAttack = isAtkDistance;
                OnPlayerAttack?.Invoke(isAttack);
            }
        }
    }

    void FixedUpdate() {
        // delegate 등록
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

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("플레이어 이동");
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

    // 플레이어 점프 
    public override void PlayerJump() {
        throw new System.NotImplementedException();
    }


    // 플레이어 상호작용 
    public override void PlayerInteraction() {
        throw new System.NotImplementedException();
    }

    // 플레이어 공격 ( 근접인지 원거리인지 판단 bool ) 
    public override void PlayerAttack( bool type ) {
        if (!type) {        // 원거리 공격 
            Debug.Log("원거리 공격");
        }
        else {              // 근거리 공격 
            Debug.Log("근거리 공격");
        }
    }

    // 아이템 버리기 ( 버리는 item id가져오기 )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // 아이템 던지기 ( 수류탄 ) 
    public override void ItemThrow() {
        throw new System.NotImplementedException();
    }

    // 플레이어 사망
    public override void PlayerDead() {
        throw new System.NotImplementedException();
    }
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            // 데이터 전송
            stream.SendNext(rigid.position);
            stream.SendNext(rigid.rotation);
            stream.SendNext(rigid.velocity);
        }
        else {
            // 데이터 수신
            rigid.position = (Vector3)stream.ReceiveNext();
            rigid.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }
}
