using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static InputKeyManager;

public class Player : PlayerController 
{
    // delegate ����
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    public delegate void PlayerJumpedHandler();
    public static event PlayerJumpedHandler OnPlayerRotation, OnPlayerJump, OnPlayerSwap, OnPlayerInteraction, OnPlayerInventory;

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
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Animator myComponent = child.GetComponent<Animator>();
                if (myComponent != null)
                {
                    animator = myComponent;
                    break;
                }
            }

            Cursor.visible = false;                         // ���콺 Ŀ�� ��Ȱ��ȭ
            Cursor.lockState = CursorLockMode.Locked;       // ���콺 Ŀ�� ���� ��ġ ���� 
            rotateToMouse = GetComponentInChildren<RotateToMouse>();
        }
    }

    void OnEnable() {
        // �̺�Ʈ ���
        OnPlayerMove += PlayerMove;                 // �÷��̾� �̵� 
        OnPlayerRotation += PlayerRotation;         // �÷��̾� ȸ��
        OnPlayerJump += PlayerJump;                 // �÷��̾� ���� 
        OnPlayerAttack += PlayerAttack;             // �÷��̾� ����
        OnPlayerSwap += WeaponSwap;                 // ���� ��ü
        OnPlayerInteraction += PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
        OnPlayerInventory += PlayerInventory;
    }

    void OnDisable() {
        // �̺�Ʈ ���� 
        OnPlayerMove -= PlayerMove;
        OnPlayerRotation -= PlayerRotation;
        OnPlayerJump -= PlayerJump;
        OnPlayerAttack -= PlayerAttack;
        OnPlayerSwap -= WeaponSwap;
        OnPlayerInteraction -= PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
        OnPlayerInventory -= PlayerInventory;
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
            // ���� ���� 
            OnPlayerSwap?.Invoke();

            // ����
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack)) && !EventSystem.current.IsPointerOverGameObject()) {
                // ��,Į 0.1��, ����ź,���� 1�� ������
                attackMaxDelay = stanceWeaponType ? 1.0f : 0.1f;

                animator.SetBool("isRifleMoveShot", true);  //�ѽ�� �ִϸ��̼�

                // ���� �����̰� �� �� ���� �Ѿ��� �߻�
                if (Time.time - lastAttackTime >= attackMaxDelay) {
                    OnPlayerAttack?.Invoke(isAtkDistance);
                    lastAttackTime = Time.time;                 // ������ �ʱ�ȭ
                }
            }
            
            // ���� 
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Jump)) && isJump) {
                OnPlayerJump?.Invoke();
            }

            // ���� ���� �ִϸ��̼� 
            if (Input.GetKeyUp(keyManager.GetKeyCode(KeyCodeTypes.Attack)))
            {
                animator.SetBool("isRifleMoveShot", false);
            }

            // �κ��丮
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Inventory))) {
                OnPlayerInventory?.Invoke();
                ToggleCursor();
            }
          
            // �÷��̾� ��ȣ�ۿ�
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                OnPlayerInteraction?.Invoke();
            }

            // �÷��̾� ȸ��
            OnPlayerRotation?.Invoke();

            // ���콺 Ŀ�� ���� 
            if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                ToggleCursor();
            }
        }
    }

    // ���콺 Ŀ�� ���� 
    private void ToggleCursor() {
        cursorLocked = !cursorLocked;
        Cursor.visible = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine) {
            // �̵�
            if (cursorLocked) {
                bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
                OnPlayerMove?.Invoke(isRun);
            }
        }
    }

    void OnTriggerEnter( Collider other )                       //���� Ʈ�����ݶ��̴��� enter������
    {
        if (PV.IsMine) {
            // ���� �浹 
            if (other.CompareTag("Enemy")){
                //hp = -(other.GetComponent<Enemy>().attackdamage)  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (PV.IsMine) {
            // ���� �±� �ʿ� 
            if (collision.gameObject.CompareTag("Ground")) {
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
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.LeftMove))) {
                isMove = true;
                x = -1f;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.RightMove))) {
                isMove = true;
                x = 1f;
            }
            // ���� �̵�         
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.DownMove))) {
                isMove = true;
                z = -1f;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.UpMove))) {
                isMove = true;
                z = 1f;
            }

            if(isMove) 
                animator.SetFloat("speedBlend", type ? 1.0f : 0.5f);
            else 
                animator.SetFloat("speedBlend", 0);

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

    // �κ��丮 Ȱ��ȭ
    public void PlayerInventory() {
        if (PV.IsMine) {
            OnPlayerMove -= PlayerMove;                 // �÷��̾� �̵� ����
            OnPlayerRotation -= PlayerRotation;         // �÷��̾� ȸ�� ����
            OnPlayerJump -= PlayerJump;                 // �÷��̾� ���� ����
            OnPlayerAttack -= PlayerAttack;             // �÷��̾� ���� ����
            OnPlayerSwap -= WeaponSwap;                 // ���� ��ü ����
            OnPlayerInteraction -= PlayerInteraction;   // �÷��̾� ��ȣ�ۿ� ����
            inventory.SetActive(true);
        }
    }
    // �κ��丮 ��Ȱ��ȭ
    public void InventoryClose() {
        if (PV.IsMine) {
            // ���콺 ��Ȱ��ȭ
            Cursor.visible = false;                         // ���콺 Ŀ�� ��Ȱ��ȭ
            Cursor.lockState = CursorLockMode.Locked;       // ���콺 Ŀ�� ���� ��ġ ���� 
            ToggleCursor();

            OnPlayerMove += PlayerMove;                 // �÷��̾� �̵� 
            OnPlayerRotation += PlayerRotation;         // �÷��̾� ȸ��
            OnPlayerJump += PlayerJump;                 // �÷��̾� ���� 
            OnPlayerAttack += PlayerAttack;             // �÷��̾� ����
            OnPlayerSwap += WeaponSwap;                 // ���� ��ü
            OnPlayerInteraction += PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
            inventory.SetActive(false);
        }
    }

    // �÷��̾� ��ȣ�ۿ� 
    public override void PlayerInteraction() {
        if (PV.IsMine) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            int layerMask = LayerMask.GetMask("Player", "Item");
            float radius = 1.5f;
            if (Physics.Raycast(ray, out hit, interactionRange, layerMask))   //���̾� �̸�, �Ÿ������� ����
            {
                if (hit.collider.CompareTag("Item")) {           // ex)text : 'E' �������ݱ� ui����ֱ�
                    ItemPickUp(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Player")) {    //���� �÷��̾��
                                                                 //ex)text : 'E' �÷��̾� �츮�� ui����ֱ�
                    if (hit.collider.GetComponent<Player>().isFaint == true) //���� �±װ� player�� ������ true��
                    {
                        //slider or shader�� (slider�� ���ҵ�) ����ֱ� �ٰ� ������
                        //�����̴� ����� 1�� �Ǵ¼��� ���� �׳༮�� player�� �����ؼ� PlayerRevive()�Լ�ȣ��
                    }
                }
            }
        }
    }

    private void ItemPickUp(GameObject itemObj) {
        if (theInventory.IsFull()) {
            Debug.Log("�κ��丮�� ���� á���ϴ�. �� �̻� �������� ���� ���մϴ�.");
        }
        else {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                Debug.Log(itemObj.transform.GetComponent<ItemPickUp>().item.itemName + " ȹ�� �߽��ϴ�.");  // �κ��丮 �ֱ�
                theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>().item);
                // ������ ����
                PV.RPC("ItemPickUpRPC", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID);
            }
            else {
                theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>().item);
                itemObj.SetActive(false);
            }
        }
    }

    [PunRPC]
    private void ItemPickUpRPC(int viewID)
    {
        GameObject itemObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        if (itemObj != null) {
            itemObj.SetActive(false);
        }
        else {
            Debug.LogError("viewID: " + viewID);
        }
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
            if (!theInventory.HasItemUse(ItemController.ItemType.Magazine)) {
                Debug.Log("źâ ����");
                return; // źâ�� ������ �޼ҵ带 �����Ͽ� ���� ���� ����
            }

            // ī�޶� �߾ӿ��� Ray ���� 
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Ray �׽�Ʈ 
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red); // ���߿� �����

            Vector3 targetPoint;

            muzzleFlashEffect.Play();
            // �浹 Ȯ��
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                Debug.Log("�� ����");
                targetPoint = hit.point;
            }
            else {
                // ���̰� ���� �ʾ��� ���� �� ������ ��ǥ�� ����
                targetPoint = ray.origin + ray.direction * 1000f;
            }

            theInventory.DecreaseMagazineCount(ItemController.ItemType.Magazine);

            // �Ѿ� ���� (������Ʈ Ǯ�� ���)
            GameObject bullet = Pooling.instance.GetObject("Bullet"); // �Ѿ��� �� �ִ� index�� ���� (0�� �ӽ�)
            bullet.transform.position = bulletPos.position; // bullet ��ġ �ʱ�ȭ
            bullet.transform.rotation = Quaternion.identity; // bullet ȸ���� �ʱ�ȭ


            // �Ѿ��� ���� ����
            Vector3 direction = (targetPoint - bulletPos.position).normalized;


            // �Ѿ��� �ʱ� �ӵ��� �÷��̾��� �̵� �ӵ��� �����ϰ� �߻� ���� ����
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(direction * 300f, ForceMode.VelocityChange); // �߻� ����� �ӵ��� �Բ� ����
        }
    }

    // �ٰŸ� ������ ���� 
    void ItemHealpack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Healpack)) {
                Debug.Log("���� ����");
                return; // źâ�� ������ �޼ҵ带 �����Ͽ� ���� ���� ����
            }
            theInventory.DecreaseMagazineCount(ItemController.ItemType.Healpack);
            Debug.Log("����");

            //�� �ϴ½ð� ������ ���� ���� �߾ӿ� ui���� �� �ϴ½ð� ������ Hp = (+30) �ڷ�ƾ����� ������ �߰��� Ű�Է½� return �ִϸ��̼��߰�;
            
        }
    }

    // ���Ÿ� ������ ����ź  
    void ItemGrenade()
    {
        if (PV.IsMine) {
            Debug.Log("��ô ����");
            animator.SetTrigger("isGranadeThrow");
            float throwForce = 15f;    // ������ ��


            GameObject grenade = Pooling.instance.GetObject("GrenadeObject"); 
            Rigidbody grenadeRigid = grenade.GetComponent<Rigidbody>();
            grenadeRigid.velocity = Vector3.zero;
            grenade.transform.position = grenadePos.position; // bullet ��ġ �ʱ�ȭ                   
            grenade.transform.rotation = Quaternion.identity; // bullet ȸ���� �ʱ�ȭ 

            // ī�޶��� �߾ӿ��� ������ ���� ���ϱ�
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit)) {
                targetPoint = hit.point;  // ��ǥ ������ �浹 ����
            }
            else {
                targetPoint = ray.GetPoint(1000);  // �浹�� ������ �� �������� ����
            }
            // ���� ���� ���
            Vector3 throwDirection = (targetPoint - grenade.transform.position).normalized;

            // Rigidbody�� ���� ����
            grenadeRigid.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
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
                animator.SetTrigger("isDrawRifle");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // ���� ����
                weaponIndex = 1;
                isAtkDistance = true;
                stanceWeaponType = false;
                Debug.Log("�ٰŸ�");
                weaponSelected = true;
                animator.SetTrigger("isDrawMelee");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // ��ô ����
                weaponIndex = 2;
                isAtkDistance = false;
                stanceWeaponType = true;
                Debug.Log("��ô");
                weaponSelected = true;
                animator.SetTrigger("isDrawGranade");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // ����
                weaponIndex = 3;
                isAtkDistance = stanceWeaponType = true;
                Debug.Log("��");
                weaponSelected = true;
                animator.SetTrigger("isDrawHeal");
                StartCoroutine(AnimReset());
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

    // ü�� ��ȭ 
    public override void ChangeHp( float value ) {
        hp += value;
        if (value > 0) {
            StartCoroutine(ShowHealScreen());   //�� ȭ�� ���
        }
        else if (value < 0) {
            StartCoroutine(ShowBloodScreen());  //�ǰ�ȭ�� ��� 
        }
    }

    // �÷��̾� ���� 
    public override void PlayerFaint() {
        if (hp <= 0)                            //���� �÷��̾� ü���� 0���� �۾�����
       {
            hp = 0;                             //���⼭ hp�� 0 ���� ������ ����� ��Ȱ, ui���� ���ҰŰ���
            isFaint = true;                     //�������� true
            OnPlayerMove -= PlayerMove;         //�����̴°Ÿ���
            OnPlayerAttack -= PlayerAttack;     //���ݵ� ���ڴ�
            animator.SetTrigger("isFaint");     //���� �ִϸ��̼� ��� 
        }
    }

    // �÷��̾� ��Ȱ
    public override void PlayerRevive()            //�÷��̾� ��Ȱ - �ٸ��÷��̾ ��Ȱ�Ҷ� ���� player�� �����ؼ� ȣ�� ���ο��� �Ⱦ����� �������⸦�������ʴ��̻�...
    {                                        //PlayerFaint �Լ��� �ݴ�� �ϸ� ��
        isFaint = false;                     //�������� false
        OnPlayerMove += PlayerMove;          //�����̴°� ���ϰ�
        OnPlayerAttack += PlayerAttack;      //���ݵ� ���� 
        animator.SetTrigger("isRevive");    //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
        Hp = 50;                             //��Ȱ�� ���Ƿ� ����! maxHp = 100; �� ���� �����ؼ� maxHp / 2�ص� �Ǵµ� Ǯ�Ǵ� 100�ϰ��� ��
    }

    // �÷��̾� ���
    public override void PlayerDead()
    {
        if (hp <= 0 && isFaint)                            //���� �÷��̾� ü���� 0���� �۰� ��������
        {
            hp = 0;                                 //���⼭ hp�� 0   //anim.setbool("isFaint", true);    //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
            OnPlayerMove -= PlayerMove;             // �÷��̾� �̵� 
            OnPlayerRotation -= PlayerRotation;     // �÷��̾�  ȸ��
            OnPlayerJump -= PlayerJump;             // �÷��̾� ���� 
            OnPlayerAttack -= PlayerAttack;         // �÷��̾� ����
            OnPlayerSwap -= WeaponSwap;             // ���� ��ü
            animator.SetTrigger("isDead");          //�׾����� �ִϸ��̼� ���

        }
    }
   
    // �ǰݽ� ���̴� ���� 
    IEnumerator ShowBloodScreen()                  //ȭ�� �Ӱ�
    {
        bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.1f, 0.15f));  //�û��Ӱ� ����
        yield return new WaitForSeconds(0.5f);                                          //0.5f�� �Ŀ�   - �̰� ������ ����?
        bloodScreen.color = Color.clear;                                                //ȭ�� ���������� ����!
    }
    // ���� ������ ���� ���̴� ���� 
    IEnumerator ShowHealScreen()                   //ȭ�� ���λ� 
    {
        float curTime = Time.time;                                          //������ �ð��� ������ �������ϰ�
        healScreen.color = new Color(1, 1, 1, 1);                           //�� �ϴ� �̹����� ����
        while (true)                                                        //�ݺ��� 1���Ŀ� ����
        {
            yield return new WaitForSeconds(0.1f);                          //0.1�ʸ���           
            float lerpColor = 1 - (Time.time - curTime);
            healScreen.color = new Color(1, 1, 1, lerpColor);               //���� ���ݾ� �������ٰ� �����ϸ��(����)
            if (lerpColor <= 0)                                             //1���Ŀ�
            {
                healScreen.color = Color.clear;                             //�÷��� 0000���� �ؼ� ���������� �����ϰ�
                break;                                                      //break�� �ݺ��� Ż��
            }
        }
    }

    IEnumerator HealItemUse()                                                     //ü��ȸ�������ۻ�� �ӽ�
    {
        int hpTime = 0;
        animator.SetTrigger("isHealUse");
        while(!Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) //��ȣ�ۿ�Ű(e)�� ������ ���
        {
            yield return new WaitForSeconds(0.1f);
            hpTime++;
            //ui����߰�
            if(hpTime > 80)
            {
                Hp = 40;
                break;
            }
        }
    }

    IEnumerator AnimReset()
    {
        yield return new WaitForSeconds(0.3f);
        animator.ResetTrigger("isDrawGranade");
        animator.ResetTrigger("isDrawHeal");
        animator.ResetTrigger("isDrawRifle");
        animator.ResetTrigger("isDrawMelee");
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

    [ContextMenu("������Ƽ--")]                      //TEST�� ���Ļ���
    void test()
    {
        Hp = -1;
    }
    [ContextMenu("������Ƽ++")]                     //TEST�� ���Ļ���
    void test2()
    {
        Hp = +1;
    }
}
