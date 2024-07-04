using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    private PhotonView PV;
    private Rigidbody rigid;
    private CharacterController characterController;

    InputKeyManager keyManager;

    void Awake() {
        // 레퍼런스 초기화 
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
        // 변수 초기화 
    }


    void Update() {
        // 단발적인 행동 
       /* bool isAttack = Input.GetMouseButton(0);
        OnPlayerAttack?.Invoke(isAttack);*/

    }

    void FixedUpdate() {
        // delegate 등록
        if (PV.IsMine) {
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    void OnTriggerEnter( Collider other ) {

    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("플레이어 이동");
        if (PV.IsMine) {
            float z = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;               
            float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5.0f;            

            transform.Translate(x, 0, z);
        }
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
        Debug.Log("공격");
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
