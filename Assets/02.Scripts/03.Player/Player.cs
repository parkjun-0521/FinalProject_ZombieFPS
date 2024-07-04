using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    void Awake() {
        // ���۷��� �ʱ�ȭ 
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
        bool isAttack = Input.GetMouseButton(0);
        OnPlayerAttack?.Invoke(isAttack);

    }

    void FixedUpdate() {
        // delegate ���
        bool isRun = Input.GetKey(KeyCode.LeftShift);
        OnPlayerMove?.Invoke(isRun);
    }

    void OnTriggerEnter( Collider other ) {

    }

    void OnCollisionEnter( Collision collision ) {
        
    }

    // �÷��̾� �̵� ( �޸��� ���ΰ� check bool ) 
    public override void PlayerMove(bool type) {
        Debug.Log("�÷��̾� �̵�");
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
        if (isJump) return;



        throw new System.NotImplementedException();
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
}
