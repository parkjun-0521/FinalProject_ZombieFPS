using Photon.Pun;
using System;
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
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    public delegate void PlayerJumpedHandler();
    public static event PlayerJumpedHandler OnPlayerRotation, OnPlayerJump, OnPlayerSwap;


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
        // 이벤트 등록
        OnPlayerMove += PlayerMove;             // 플레이어 이동 
        OnPlayerRotation += PlayerRotation;     // 플레이어  회전
        OnPlayerJump += PlayerJump;             // 플레이어 점프 
        OnPlayerAttack += PlayerAttack;         // 플레이어 공격
        OnPlayerSwap += WeaponSwap;             // 무기 교체
    }

    void OnDisable() {
        // 이벤트 해제 
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
        // 단발적인 행동 
        if (PV.IsMine) {
            OnPlayerSwap?.Invoke();
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack))){
                // 총,칼 0.1초, 수류탄,힐팩 1초 딜레이
                attackMaxDelay = stanceWeaponType ? 1.0f : 0.1f;
                // 일정 딜레이가 될 때 마다 총알을 발사
                if (Time.time - lastAttackTime >= attackMaxDelay) {             
                    OnPlayerAttack?.Invoke(isAtkDistance);
                    lastAttackTime = Time.time;         // 딜레이 초기화
                }
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Jump))) {
                OnPlayerJump?.Invoke();
            }
        }
    }

    void FixedUpdate() {
        // delegate 등록
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

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type){
        if (!PV.IsMine) return;

        float x = 0f;
        float z = 0f;

        // 걷기, 달리기 속도 조절
        float playerSpeed = type ? runSpeed : speed;

        // 좌우 이동
        if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.LeftMove)))
            x = -1f;
        else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.RightMove)))
            x = 1f;

        // 상하 이동         
        if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.DownMove)))
            z = -1f;
        else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.UpMove)))
            z = 1f;

        // 공중에 떠있는지 확인
        if (!characterController.isGrounded) {
            moveForce.y += gravity * Time.deltaTime;
        }

        MoveTo(new Vector3(x, 0, z), playerSpeed);
        characterController.Move(moveForce * Time.deltaTime);
    }

    // 이동 방향 및 힘 결정 
    public void MoveTo(Vector3 direction, float speed) {
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);
        moveForce = new Vector3(direction.x * speed, moveForce.y, direction.z * speed);
    }
    // 플레이어 회전
    public void PlayerRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }


    // 플레이어 점프 
    public override void PlayerJump(){
        if (PV.IsMine) {         
            // 땅에 붙어있을 때 점프
            if(characterController.isGrounded)  {
                moveForce.y = jumpForce;
            }
        }
    }


    // 플레이어 상호작용 
    public override void PlayerInteraction() {
        throw new System.NotImplementedException();
    }

    // 플레이어 공격 ( 근접인지 원거리인지 판단 bool ) 
    public override void PlayerAttack( bool type ) {
        if (!PV.IsMine) return;

        // 무기 거리에 따른 공격 
        if (type)
            // 근거리 공격
            (stanceWeaponType ? (Action)ItemHealpack : SwordAttack)();      // 힐팩 : 근접공격
        else
            // 원거리 공격
            (stanceWeaponType ? (Action)ItemGrenade : GunAttack)();         // 수류탄 : 원거리 공격 
    }

    // 근접 칼
    void SwordAttack()
    {
        Debug.Log("칼 공격");
        // 근거리 공격 애니메이션 
        // 데미지는 weapon에서 줄꺼임 그리고 체력은 좀비에서 감소시킬예정
    }

    // 원거리 총
    void GunAttack()
    {
        // 카메라 중앙에서 Ray 생성 
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        // Ray 테스트 
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red);                       // 나중에 지우기

        // 충돌 확인 및 총알 생성 
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
            Debug.Log("총 공격");
            // 총알 생성 
            //Instantiate(bullet, bulletPos.position, Quaternion.identity); // 이건 나중에 오브젝트 풀링으로 바꿀 예정 
            GameObject bullet = Pooling.instance.GetObject(0);              // (index) : 총알이 들어가 있는 index로 변경 0은 임시로 지정 ( 오브젝트 풀링 )
            bullet.transform.position = bulletPos.position;                 // bullet 위치 초기화                   
            bullet.transform.rotation = Quaternion.identity;                // bullet 회전값 초기화 
            // 애니메이션 생성 
            // 파티클 또는 스프라이트 이미지로 이펙트 표현 
            // 데미지는 Bullet에서 처리 체력은 좀비에서 감소 시킬 예정
        }
    }


    // 근거리 아이템 힐팩 
    void ItemHealpack()
    {
        Debug.Log("힐팩");
    }

    // 원거리 아이템 수류탄  
    void ItemGrenade()
    {
        Debug.Log("투척 공격");
    }

    // 무기 교체
    public override void WeaponSwap() {
        if (!PV.IsMine) return;

        int weaponIndex = -1;           // 초기 무기 인덱스 ( 빈손 ) 
        bool weaponSelected = false;    // 무기가 선택되었는지 확인 
            
        if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon1))) {        // 원거리 무기 
            weaponIndex = 0;                        
            isAtkDistance = stanceWeaponType = false;
            Debug.Log("원거리");
            weaponSelected = true;
        }
        else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // 근접 무기
            weaponIndex = 1; 
            isAtkDistance = true;
            stanceWeaponType = false;
            Debug.Log("근거리");
            weaponSelected = true;
        }
        else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // 투척 무기
            weaponIndex = 2; 
            isAtkDistance = false;
            stanceWeaponType = true;
            Debug.Log("투척");
            weaponSelected = true;
        }
        else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // 힐팩
            weaponIndex = 3; 
            isAtkDistance = stanceWeaponType = true;
            Debug.Log("힐");
            weaponSelected = true;
        }

        if (!weaponSelected) return;

        if (equipWeapon != null)
            equipWeapon.SetActive(false);
        equipWeapon = weapons[weaponIndex];
        equipWeapon.SetActive(true);
 
    }

    // 아이템 버리기 ( 버리는 item id가져오기 )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // 플레이어 사망
    public override void PlayerDead() {
        throw new System.NotImplementedException();
    }

    // 플레이어 동기화
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            // 데이터 전송 ( 동기화 ) 
            stream.SendNext(rigid.position);    // 위치 
            stream.SendNext(rigid.rotation);    // 회전
            stream.SendNext(rigid.velocity);    // 속도 
        }
        else {
            // 데이터 수신
            rigid.position = (Vector3)stream.ReceiveNext();
            rigid.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }
}
