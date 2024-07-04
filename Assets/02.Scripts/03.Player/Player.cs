using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : PlayerController 
{
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    void Awake() {
        // 레퍼런스 초기화 
    }

    public override void OnEnable() {
        OnPlayerMove += PlayerMove;
        OnPlayerAttack += PlayerAttack;
    }
    public override void OnDisable() {
        OnPlayerMove -= PlayerMove;
        OnPlayerAttack -= PlayerAttack;
    }

    void Start() {
        // 변수 초기화 
    }

    void Update() {
        // 단발적인 행동 
        bool isAttack = Input.GetMouseButton(0);
        OnPlayerAttack?.Invoke(isAttack);

    }

    void FixedUpdate() {
        // delegate 등록
        bool isRun = Input.GetKey(KeyCode.LeftShift);
        OnPlayerMove?.Invoke(isRun);
    }

    void OnTriggerEnter( Collider other ) {

    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("플레이어 이동");
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
        if (isJump) return;



        throw new System.NotImplementedException();
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
