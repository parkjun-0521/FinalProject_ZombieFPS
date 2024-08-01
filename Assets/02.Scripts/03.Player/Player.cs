using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    public bool cursorLocked = true;

    // ��ȣ�ۿ� Ray  

    Ray ray;
    bool isRayPlayer = false;

    //dot damage �ڷ�ƾ
    Coroutine dotCoroutine;

    void Awake()
    {
        // ���۷��� �ʱ�ȭ 
        PV = GetComponent<PhotonView>();
        if (PV.IsMine) {
            rigid = GetComponent<Rigidbody>();
            Animator[] animators = GetComponentsInChildren<Animator>();
            animator = animators[1];                        //���߿� ���ο� �ִϸ����� �߰��� ������ �� �Ҿ������� 
            handAnimator = animators[2];
            capsuleCollider = GetComponent<CapsuleCollider>();

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
            ItemController[] items = FindObjectsOfType<ItemController>(true); // true�� ����Ͽ� ��Ȱ��ȭ�� ������Ʈ�� ����
            photonView.RPC("UpdateHealthBar", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, photonView.ViewID, (hp / maxHp) * 100);
            aimingObj.SetActive(true);
            weaponSelected = false;
        }
        else {
            playerCamera.gameObject.SetActive(false);
            aimingObj.SetActive(false);
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
                attackMaxDelay = stanceWeaponType ? 1.0f : weaponIndex == 4 ? 1f : weaponIndex == 1 ? 1f : 0.1f;

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

            // ����
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.BulletLoad)) && isGun && !isLoad) {

                if (inventory != null && theInventory.go_MauntingSlotsParent != null) {
                    Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
                    if (slotsParent.childCount > 1) {
                        Transform firstChild = slotsParent.GetChild(0);
                        Transform grandChild = firstChild.GetChild(0);
                        string imageComponent = grandChild.GetComponent<Image>().sprite.name;
                        if (imageComponent.Equals("Gun")) {
                            if (UIManager.Instance.CurBulletCount.text.Equals("30")) return;
                            string totalBullet = UIManager.Instance.totalBulletCount.text;
                            if (int.Parse(totalBullet) < 30) return;
                        }
                        else if (imageComponent.Equals("ShotGun")) {
                            if (UIManager.Instance.CurBulletCount.text.Equals("15")) return;
                            string totalBullet = UIManager.Instance.totalBulletCount.text;
                            if (int.Parse(totalBullet) < 15) return;
                        }
                    }
                }
                isLoad = true;
                isBulletZero = true;
                handAnimator.SetTrigger("isReload");
                StartCoroutine(BulletLoad());
            }

            // �κ��丮
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Inventory))) {
                OnPlayerInventory?.Invoke();
            }

            // ����
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Setting)) && inventory.activeSelf) {
                InventoryClose();
                cursorLocked = false;
                ToggleCursor();
            }

            // �÷��̾� ��ȣ�ۿ�
            OnPlayerInteraction?.Invoke();
            // �÷��̾� ȸ��
            OnPlayerRotation?.Invoke();

            // ���콺 Ŀ�� ���� 
            if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                ToggleCursor();
            }

            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            //��� ������ ������ ���̽��� ui true
            
        }
    }

    // ���콺 Ŀ�� ���� 
    public void ToggleCursor() {
        cursorLocked = !cursorLocked;           // cursorLocked : false => ���콺�� Ȱ��ȭ None, true => ���콺 ��Ȱ��ȭ Lock
        Cursor.visible = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        if(cursorLocked) {
            if (isFaint || isDead) return;
            OnPlayerMove += PlayerMove;                 // �÷��̾� �̵� 
            OnPlayerRotation += PlayerRotation;         // �÷��̾� ȸ��
            OnPlayerJump += PlayerJump;                 // �÷��̾� ���� 
            OnPlayerAttack += PlayerAttack;             // �÷��̾� ����
            OnPlayerSwap += WeaponSwap;                 // ���� ��ü
            OnPlayerInteraction += PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
            OnPlayerInventory += PlayerInventory;
        }
        else {
            OnPlayerMove -= PlayerMove;
            OnPlayerRotation -= PlayerRotation;
            OnPlayerJump -= PlayerJump;
            OnPlayerAttack -= PlayerAttack;
            OnPlayerSwap -= WeaponSwap;                 
            OnPlayerInteraction -= PlayerInteraction;   
            OnPlayerInventory -= PlayerInventory;
        }
    }

    void FixedUpdate() {
        // delegate ���
        if (PV.IsMine) {
            // �̵�
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(PV.IsMine)
        {
            if(other.CompareTag("DotArea"))
            {
                if(dotCoroutine == null)
                {
                    EliteRangeEnemyDotArea EREP = other.GetComponent<EliteRangeEnemyDotArea>();
                    dotCoroutine = StartCoroutine(DotDamage(EREP));
                }
            }
        }
    }

    void OnTriggerEnter( Collider other )                       //���� Ʈ�����ݶ��̴��� enter������
    {
        if (PV.IsMine) {
            // ���� �浹 
            if (other.CompareTag("Enemy"))
            {
                Hp = -(other.GetComponentInParent<EnemyController>().damage);  //-�� ������ �����ʿ��� ���ݷ��� -5 �̷����ϸ� ����-������
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_hurt);
            }
            else if(other.CompareTag("EnemyProjectile"))
            {
                Hp = -(other.GetComponent<EliteRangeEnemyProjectile>().damage);
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_hurt);
                other.gameObject.SetActive(false);
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
            isMove = false;
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

            if (isMove) {
                animator.SetFloat("speedBlend", type ? 1.0f : 0.5f);
                if (type && !AudioManager.Instance.IsPlaying(AudioManager.Sfx.Player_run2)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_run2);
                }
                else if (!type && !AudioManager.Instance.IsPlaying(AudioManager.Sfx.Player_walk3)) {
                    AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_walk3);
                }
            }
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
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_jump);
            isJump = false; 
        }
    }

    // �κ��丮 Ȱ��ȭ
    public void PlayerInventory() {
        if (PV.IsMine) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.UI_Button);
            cursorLocked = true;
            ToggleCursor();
            inventory.SetActive(true);
        }
    }
    // �κ��丮 ��Ȱ��ȭ
    public void InventoryClose() {
        if (PV.IsMine) {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.UI_Button);
            cursorLocked = false;
            ToggleCursor();
            inventory.SetActive(false);
        }
    }

    // �÷��̾� ��ȣ�ۿ� 
    public override void PlayerInteraction() {
        if (PV.IsMine) {
            RaycastHit hit;

            int layerMask = LayerMask.GetMask("LocalPlayer", "Item", "Door", "Npc");
            foreach (SelectedOutline outlineComponent in FindObjectsOfType<SelectedOutline>()) {
                outlineComponent.DeactivateOutline();
            }      
            if (Physics.Raycast(bulletPos.position, ray.direction, out hit, interactionRange, layerMask))   
            {
                SelectedOutline outlineComponent = hit.collider.GetComponent<SelectedOutline>();
                if (outlineComponent != null) {
                    outlineComponent.ActivateOutline();
                }

                if (hit.collider.CompareTag("Item")) {
                    playerReviveUI.SetActive(true);
                    playerReviveUI.GetComponentInChildren<Text>().text = string.Format("'E' {0} ������ �ݱ�", hit.collider.GetComponent<ItemPickUp>().itemName);
                    if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                        ItemPickUp(hit.collider.gameObject);
                        AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_item);
                        AcquireItems();
                    }
                }
                else if (hit.collider.CompareTag("Player")) {
                    isRayPlayer = true;
                    Player otherPlayer = hit.collider.GetComponent<Player>();
                    if (otherPlayer.isFaint) {
                        playerReviveUI.SetActive(true);
                        playerReviveUI.GetComponentInChildren<Text>().text = "'E' �÷��̾� ��Ȱ";
                        if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                            StartCoroutine(CorPlayerReviveUI(8.0f, otherPlayer));
                        }
                    }
                    else {
                        playerReviveUI.SetActive(false);
                        isRayPlayer = false;
                    }
                }
                else if (hit.collider.CompareTag("Door")) {
                    playerReviveUI.SetActive(true);
                    Door door = hit.transform.GetComponentInChildren<Door>();
                    if(door.isOpen)
                    {
                        playerReviveUI.GetComponentInChildren<Text>().text = string.Format("'E' �� �ݱ�");
                    }
                    else
                    {
                        playerReviveUI.GetComponentInChildren<Text>().text = string.Format("'E' �� ����");
                    }

                    if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                        if (door.isOpen) {
                            door.CloseDoor();
                        }
                        else if (!door.isOpen) {
                            door.OpenDoor();
                        }
                        playerReviveUI.SetActive(false);
                    }
                }
                else if (hit.collider.CompareTag("Npc")) {

                    Debug.Log(2);
                    if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                        Debug.Log("��ȭ");
                        hit.collider.GetComponent<NPC>().QusetTalkRPC();
                        photonView.RPC("ShaderVisibleRPC", RpcTarget.AllBuffered);
                       
                        if (inventory != null && theInventory.go_SlotsParent != null) {
                            Transform slotsParent = theInventory.go_SlotsParent.transform;
                            for (int i = 0; i < slotsParent.childCount; i++) {
                                Transform firstChild = slotsParent.GetChild(i);
                                Transform grandChild = firstChild.GetChild(0);
                                Image imageComponent = grandChild.GetComponent<Image>();
                                if (imageComponent != null && imageComponent.sprite != null) {
                                    string spriteName = imageComponent.sprite.name;
                                    if (spriteName.Equals("QuestItem")) {
                                        Debug.Log("����Ʈ �Ϸ�");
                                        photonView.RPC("QuestCompleteRPC", RpcTarget.AllBuffered);
                                        hit.collider.GetComponent<NPC>().QusetClearTalkRPC();
                                        theInventory.DecreaseMagazineCount(ItemController.ItemType.QuestItem);
                                    }
                                    else {
                                        Debug.Log("������ ���� ��ȭ");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                playerReviveUI.SetActive(false);
                isRayPlayer = false;
            }
        }
    }

    [PunRPC]
    public void ShaderVisibleRPC() {
        GameObject questItemObject = GameObject.Find("QuestItem");
        if(questItemObject == null) {
            Debug.Log("������ ã��???");
            questItemObject = GameObject.Find("QuestItem(Clone)");
            isShaderApplied = false;
        }

        if (questItemObject != null && !isShaderApplied) {
            QuestItemShader questItemShader = questItemObject.GetComponent<QuestItemShader>();
            questItemShader.VisibleShader();
            questItemShader.visible.SetTexture("_MainTex", Resources.Load<Texture2D>("PlayerDiffuse"));
            isShaderApplied = true;
        }
    }

    [PunRPC]
    void QuestCompleteRPC() {
        if (ScenesManagerment.Instance.stageCount == 0) {
            NextSceneManager.Instance.isQuest1 = true;
        }
        else if (ScenesManagerment.Instance.stageCount == 1) {
            NextSceneManager.Instance.isQuest2 = true;
        }
        else if (ScenesManagerment.Instance.stageCount == 2) {
            NextSceneManager.Instance.isQuest3 = true;
        }
    }

    void AcquireItems()
    {
        if (theInventory != null && theInventory.go_MauntingSlotsParent != null) {
            Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
            if (slotsParent.childCount > 1) {
                Transform firstChild = slotsParent.GetChild(0);
                Transform grandChild = firstChild.GetChild(0);
                Image imageComponent = grandChild.GetComponent<Image>();
                if (imageComponent != null && imageComponent.sprite != null) {
                    string spriteName = imageComponent.sprite.name;
                    if (spriteName.Equals("Gun")) UIManager.Instance.UpdateTotalBulletCount(theInventory.CalculateTotalItems(ItemController.ItemType.Magazine));
                    else if (spriteName.Equals("ShotGun")) UIManager.Instance.UpdateTotalBulletCount(theInventory.CalculateTotalItems(ItemController.ItemType.ShotMagazine));
                }
            }
        }
    }

    // ������ �ݱ� 
    private void ItemPickUp(GameObject itemObj) {
        if (PV.IsMine) { 
            if (theInventory.IsFull()) {
                Debug.Log("�κ��丮�� ���� á���ϴ�. �� �̻� �������� ���� ���մϴ�.");
            }
            else {
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                    // �κ��丮�� ������ �ֱ� 
                    theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>());
                    // ������ ���� ����ȭ
                    PV.RPC("ItemPickUpRPC", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID);
                }
                else {
                    // ���� �ƴ� �� �׽�Ʈ �� ( ���� ���� ���� ) ============================================================
                    theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>());
                    itemObj.SetActive(false);
                    // ���� �ƴ� �� �׽�Ʈ �� ( ���� ���� ���� ) ============================================================
                }
            }
        }
    }

    // ������ �ֿ��� �� ����ȭ 
    [PunRPC]
    public void ItemPickUpRPC(int viewID)
    {
        GameObject itemObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        if (itemObj != null)
            itemObj.SetActive(false);
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

    // ������ ������ ��� �Ұ�
    public bool ItemNotEquipped(int slotID)
    {
        // theInventory���� Inventory ������Ʈ�� ������
        Inventory inventory = theInventory.GetComponent<Inventory>();
        // MountingSlotsParent���� Slot ������Ʈ���� ��������, ID�� 3�� ������ ã��
        Slot slotWithID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == slotID);

        // ���Ⱑ �������� �ʾ��� ��
        if (slotWithID == null || slotWithID.item == null) {
            Debug.Log("��� �������� ����");
            return false;
        }

        switch (slotID) {
            case 3:
                // ID�� 3�� ������ null�̰ų� ��� �ִ� ����� ���ǹ�   
                if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.Grenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.Grenade);
                }
                else if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.FireGrenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.FireGrenade);
                }
                else if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.SupportFireGrenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.SupportFireGrenade);
                }
                // �������� �� ���� �� �� �տ� �ִ� ���� ��Ȱ��ȭ
                if (slotWithID.itemCount == 0) {
                    countZero = true;
                }
                break;
            case 4:
                // ID�� 4�� ������ null�̰ų� ��� �ִ� ����� ���ǹ�   
                StartCoroutine(HealItemUse(6.0f, 40.0f, slotWithID)); ; //�� �ð�, ����
                // �������� �� ���� �� �� �տ� �ִ� ���� ��Ȱ��ȭ
                break;
        }
        return true;
    }

    // ���� Į
    void SwordAttack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Sword1) && !theInventory.HasItemUse(ItemController.ItemType.Sword2)) {
                Debug.Log("Į�� ����");
                return;
            }
            // ������ ������
            if (!ItemNotEquipped(2))
                return;

            Debug.Log("Į ����");
            animator.SetBool("isMeleeWeaponSwing", true);          //�ܺο��� �������� �ִϸ��̼�
            handAnimator.SetBool("isMeleeWeaponSwing", true);   //�÷��̾� 1��Ī �ִϸ��̼�
            StartCoroutine(AnimReset("isMeleeWeaponSwing", handAnimator));
            // �������� weapon���� �ٲ��� �׸��� ü���� ���񿡼� ���ҽ�ų����
        }
    }

    // ���Ÿ� ��
    void GunAttack()
    {
        if (PV.IsMine)
        {
            // ������ �� ���� 
            if (!ItemNotEquipped(1))
            {

                if (inventory != null && theInventory.go_MauntingSlotsParent != null)
                {
                    Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
                    if (slotsParent.childCount > 1)
                    {
                        Transform firstChild = slotsParent.GetChild(0);
                        Transform grandChild = firstChild.GetChild(0);
                        Image imageComponent = grandChild.GetComponent<Image>();
                        if (imageComponent != null && imageComponent.sprite != null) {
                            string spriteName = imageComponent.sprite.name;
                            if (spriteName.Equals("Gun")) {
                                bulletCount[0] = 0;
                                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                            }
                            else if (spriteName.Equals("ShotGun")) {
                                bulletCount[1] = 0;
                                UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
                            }
                        }

                    }
                }

                return;
            }

            // źâ�� ���� �� ��� X
            if (!theInventory.HasItemUse(ItemController.ItemType.Magazine))
            {
                if (inventory != null && theInventory.go_MauntingSlotsParent != null)
                {
                    Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
                    if (slotsParent.childCount > 1)
                    {
                        Transform firstChild = slotsParent.GetChild(0);
                        Transform grandChild = firstChild.GetChild(0);
                        Image imageComponent = grandChild.GetComponent<Image>();
                        if (imageComponent != null && imageComponent.sprite != null) {
                            string spriteName = imageComponent.sprite.name;
                            if (spriteName.Equals("Gun")) {
                                bulletCount[0] = 0;
                                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                                Debug.Log("źâ ����");
                                return;
                            }
                        }

                    }
                }
            }

            if (!theInventory.HasItemUse(ItemController.ItemType.ShotMagazine))
            {
                if (inventory != null && theInventory.go_MauntingSlotsParent != null)
                {
                    Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
                    if (slotsParent.childCount > 1)
                    {
                        Transform firstChild = slotsParent.GetChild(0);
                        Transform grandChild = firstChild.GetChild(0);
                        Image imageComponent = grandChild.GetComponent<Image>();
                        if (imageComponent != null && imageComponent.sprite != null) {

                            string spriteName = imageComponent.sprite.name;
                            if (spriteName.Equals("ShotGun")) {
                                bulletCount[1] = 0;
                                UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
                                Debug.Log("źâ ����");
                                return;
                            }
                        }
                    }
                }
            }

            if (inventory != null && theInventory.go_MauntingSlotsParent != null) {
                Transform slotsParent = theInventory.go_MauntingSlotsParent.transform;
                if (slotsParent.childCount > 1) {
                    Transform firstChild = slotsParent.GetChild(0);
                    Transform grandChild = firstChild.GetChild(0);
                    string imageComponent = grandChild.GetComponent<Image>().sprite.name;
                    if (imageComponent.Equals("Gun")) {
                        if (bulletCount[0] == 0 && isGun && !isLoad) {
                            isLoad = true;
                            StartCoroutine(BulletLoad());
                            isBulletZero = true;
                        }
                    }
                    else if (imageComponent.Equals("ShotGun")) {
                        if (bulletCount[1] == 0 && isGun && !isLoad) {
                            isLoad = true;
                            StartCoroutine(BulletLoad());
                            isBulletZero = true;
                        }
                    }
                }
            }

            // ���� �ʿ� 
            if (isBulletZero) return;
            // �� ���� ���ϸ� �� ��� ���� ó�� 
            if (!isGun) return;

            // ī�޶� �߾ӿ��� Ray ���� 
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 targetPoint;
            // ����Ʈ ���� 
            muzzleFlashEffect.Play();
            // �浹 Ȯ��
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                targetPoint = hit.point;
                hit.transform.GetComponent<EnemyController>().BloodEffect(hit.point);       // ���� �� ����Ʈ ����
            }
            else {
                targetPoint = ray.origin + ray.direction * 1000f;                           // ���̰� ���� �ʾ��� ���� �� ������ ��ǥ�� ����
            }

            // �Ѿ� ���� (������Ʈ Ǯ�� ���)
            if (weaponIndex == 0) {
                GameObject bullet = Pooling.instance.GetObject("Bullet", Vector3.zero);   // �Ѿ� ���� 
                bullet.transform.position = bulletPos.position;                           // bullet ��ġ �ʱ�ȭ
                bullet.transform.rotation = Quaternion.identity;                          // bullet ȸ���� �ʱ�ȭ
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_gun2);

                TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
                if (trail != null) {
                    trail.Clear();
                }

                // źâ ���� ����
                theInventory.DecreaseMagazineCount(ItemController.ItemType.Magazine);
                bulletCount[0] -= 1;
                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();

                // �Ѿ��� ���� ����
                Vector3 direction = (targetPoint - bulletPos.position).normalized;

                // �Ѿ��� �ʱ� �ӵ��� �÷��̾��� �̵� �ӵ��� �����ϰ� �߻� ���� ����
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                // ������ٵ��� �ӵ��� ���ӵ� �ʱ�ȭ
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // �߻� ����� �ӵ��� �Բ� ����
                rb.AddForce(direction * 50f, ForceMode.VelocityChange);
            }
            else if(weaponIndex == 4) {
                float spreadRadius = 0.1f; // �߻� ������ �л� ���� ������ ���� (���� ����)

                for (int i = 0; i < 5; i++) {
                    GameObject bullet = Pooling.instance.GetObject("Bullet", Vector3.zero);   // �Ѿ� ���� 
                    bullet.transform.position = bulletPos.position;                           // bullet ��ġ �ʱ�ȭ
                    bullet.transform.rotation = Quaternion.identity;                          // bullet ȸ���� �ʱ�ȭ
                    TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
                    if (trail != null) {
                        trail.Clear();
                    }

                    // �Ѿ��� �ʱ� �ӵ��� �÷��̾��� �̵� �ӵ��� �����ϰ� �߻� ���� ����
                    Rigidbody rb = bullet.GetComponent<Rigidbody>();

                    // ī�޶� �߾� ������ �⺻ �������� ����
                    Vector3 direction = Camera.main.transform.forward;

                    // ���⿡ �л��� �߰��ϱ� ���� ���� ���� ���� ������ ���� �߰�
                    Vector3 randomSpread = UnityEngine.Random.insideUnitCircle * spreadRadius;

                    // ī�޶��� ������ �������� �л�� ������ ����
                    Vector3 spreadDirection = direction + Camera.main.transform.right * randomSpread.x + Camera.main.transform.up * randomSpread.y;
                    spreadDirection.Normalize();

                    // ������ٵ��� �ӵ��� ���ӵ� �ʱ�ȭ
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    // �߻� ����� �ӵ��� �Բ� ����
                    rb.AddForce(spreadDirection * 50f, ForceMode.VelocityChange);
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.ShotMagazine);
                }
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_gun2);

                // źâ ���� ����
                bulletCount[1] -= 5;
                UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
            }
        }
    }
    
    IEnumerator BulletLoadImage()
    {
        UIManager.Instance.reloadImage.fillAmount = 0;
        while (UIManager.Instance.reloadImage.fillAmount <= 0.99f)
        {
            yield return null;
            UIManager.Instance.reloadImage.fillAmount += Time.deltaTime / 2;
        }
        UIManager.Instance.reloadImage.fillAmount = 0;
    }
    IEnumerator BulletLoad()
    {
        AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_reload3);
        StartCoroutine(BulletLoadImage());      //�����̹���

        yield return new WaitForSeconds(2f);
        Debug.Log("2�ʰ� ������");

        if (weaponIndex == 0) {
            int availableBullets = theInventory.CalculateTotalItems(ItemController.ItemType.Magazine); // �� �޼ҵ�� �κ��丮���� ��� ������ ��� �Ѿ��� ���� ��ȯ�ؾ� ��
            bulletCount[0] = (availableBullets > 0) ? Mathf.Min(availableBullets, 30) : 0;
            UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
            isBulletZero = false;

            if (availableBullets > 0) {
                bulletCount[0] = Mathf.Min(availableBullets, 30); // ���� �Ѿ��� 30�� �̻��̸� 30����, �׷��� ������ ���� �Ѿ˸�ŭ ����
                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                isBulletZero = false; // �Ѿ��� ��������
            }
            else {
                bulletCount[0] = 0;
                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                isBulletZero = false; // �Ѿ��� �� ������
            }
        }
        else if(weaponIndex == 4) {
            int availableBullets = theInventory.CalculateTotalItems(ItemController.ItemType.ShotMagazine); // �� �޼ҵ�� �κ��丮���� ��� ������ ��� �Ѿ��� ���� ��ȯ�ؾ� ��
            bulletCount[1] = (availableBullets > 0) ? Mathf.Min(availableBullets, 15) : 0;
            UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
            isBulletZero = false;

            if (availableBullets > 0) {
                bulletCount[1] = Mathf.Min(availableBullets, 15); // ���� �Ѿ��� 30�� �̻��̸� 30����, �׷��� ������ ���� �Ѿ˸�ŭ ����
                UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
                isBulletZero = false; // �Ѿ��� ��������
            }
            else {
                bulletCount[1] = 0;
                UIManager.Instance.CurBulletCount.text = bulletCount[1].ToString();
                isBulletZero = false; // �Ѿ��� �� ������
            }
        }


        isLoad = false;
    }

    // �ٰŸ� ������ ���� 
    void ItemHealpack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Healpack)) {
                Debug.Log("���� ����");
                return; 
            }

            // ������ ������ 
            if (!ItemNotEquipped(4))
                return;
   
            Debug.Log("����");
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Medikit2);
            //�� �ϴ½ð� ������ ���� ���� �߾ӿ� ui���� �� �ϴ½ð� ������ Hp = (+30) �ڷ�ƾ����� ������ �߰��� Ű�Է½� return �ִϸ��̼��߰�;

        }
    }

    // ���Ÿ� ������ ����ź  
    void ItemGrenade()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Grenade) && 
                !theInventory.HasItemUse(ItemController.ItemType.FireGrenade) && 
                !theInventory.HasItemUse(ItemController.ItemType.SupportFireGrenade)) {
                Debug.Log("��ô���� ����");
                return; 
            }
            // ������ ������ 
            if (!ItemNotEquipped(3))
                return;

            Debug.Log("��ô ����");
            animator.SetBool("isGrenadeThrow", true);
            StartCoroutine(AnimReset("isGrenadeThrow"));
            float throwForce = 15f;    // ������ ��
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_granede);

            //if( �κ��丮 ��� ĭ���� �̸��� ������ == ����ź)
            GameObject grenade = Pooling.instance.GetObject("GrenadeObject", Vector3.zero);   // ����ź ���� 
            //else if( �κ��丮 ��� ĭ���� �̸��� ������ == ȭ���� ) 
            Rigidbody grenadeRigid = grenade.GetComponent<Rigidbody>();
            // �ʱ� ��ġ ����
            grenadeRigid.velocity = Vector3.zero;               // ���� �� ���ӵ� �ʱ�ȭ
            grenade.transform.position = grenadePos.position;   // bullet ��ġ �ʱ�ȭ                   
            grenade.transform.rotation = Quaternion.identity;   // bullet ȸ���� �ʱ�ȭ 

            // ī�޶��� �߾ӿ��� ������ ���� ���ϱ�
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            Vector3 targetPoint;

            // �¾��� �� , �ȸ¾��� �� �浹 ���� ���� 
            targetPoint = (Physics.Raycast(ray, out hit)) ? hit.point : ray.GetPoint(1000);

            // ���� ���� ���
            Vector3 throwDirection = (targetPoint - grenade.transform.position).normalized;

            // Rigidbody�� ���� ����
            grenadeRigid.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
    }

    // ���� ��ü
    public override void WeaponSwap() {
        if (PV.IsMine) {
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon1))) {        // ���Ÿ� ���� 
                if (WeaponSwapStatus(0, false, false, true, "isDrawRifle", 0, beforeWeapon)) {
                    countZero = false;
                    beforeWeapon = 1;
                }
                Inventory inventory = theInventory.GetComponent<Inventory>();
                if (inventory != null && inventory.go_MauntingSlotsParent != null) {
                    Transform slotsParent = inventory.go_MauntingSlotsParent.transform;
                    if (slotsParent.childCount > 1) {
                        Transform firstChild = slotsParent.GetChild(0);
                        Transform grandChild = firstChild.GetChild(0);
                        string imageComponent = grandChild.GetComponent<Image>().sprite.name;
                        if (imageComponent != null) {
                            if (imageComponent.Equals("Gun"))
                                weaponIndex = 0;
                            else if (imageComponent.Equals("ShotGun"))
                                weaponIndex = 4;
                        }
                    }
                }
                isGun = true;
                handAnimator.SetTrigger("isTakeOut");
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // ���� ����
                if (WeaponSwapStatus(1, true, false, true, "isDrawMelee", 1, beforeWeapon)) {
                    countZero = false;
                    handAnimator.SetTrigger("isDrawMelee");
                    beforeWeapon = 2;
                }
                isGun = false;
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // ��ô ����
                if(WeaponSwapStatus(2, false, true, true, "isDrawGrenade", 2, beforeWeapon))
                    beforeWeapon = 3;
                isGun = false;
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // ����
                if(WeaponSwapStatus(3, true, true, true, "isDrawHeal", 3, beforeWeapon))
                    beforeWeapon = 4;
                isGun = false;
            }

            // ���⸦ �������� �ʾ��� �� 
            if (!weaponSelected) return;

            // ����� ��� ����ó�� 
            if (equipWeapon != null)
                equipWeapon.SetActive(false);
            // ���� ���� 
            
            equipWeapon = weapons[weaponIndex];

            // ���õ� ���� Ȱ��ȭ ( ���⸦ �� ������� �� ��Ȱ��ȭ )
            if (!countZero) {
                equipWeapon.SetActive(true);
            }
            else {
                if (beforeWeapon != 0) 
                    UIManager.Instance.weaponItem[beforeWeapon - 1].color = new Color(1, 1, 1, 0.2f);
                beforeWeapon = 0;
                equipWeapon.SetActive(false);
            }
        }
    }
    // ���� ��ü ���� ���� : ���ڰ� (0:����ID, 1:���� ��Ÿ�, 2:����Ÿ��, 3:���⸦ ������ ���� ) 
    public bool WeaponSwapStatus(int weaponIndex, bool isAtkDistance, bool stanceWeaponType, bool weaponSelected, string Animation, int uiPos, int beforeWeapon)
    {
        // ������ ������ ���� Ȯ�� �� ���� ó�� 
        Inventory inventory = theInventory.GetComponent<Inventory>();
        Slot slotWithID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == weaponIndex + 1);
        Slot slotBeforeWeaponID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == beforeWeapon);

        if (slotWithID == null || slotWithID.item == null) {
            UIManager.Instance.weaponItem[uiPos].color = new Color(1, 1, 1, 0.2f);
            Debug.Log("��� �������� ����");
            return false;
        }
        // �������� 0���� ������ Ȱ��ȭ �� �� �ֵ��� 
        if (slotWithID.itemCount > 0) {
            countZero = false;
        }

        // �� ���� ���� �ʱ�ȭ
        this.weaponIndex = weaponIndex;
        this.isAtkDistance = isAtkDistance;
        this.stanceWeaponType = stanceWeaponType;
        this.weaponSelected = weaponSelected;
        if (beforeWeapon != 0) {
            if (slotBeforeWeaponID.item != null) {
                UIManager.Instance.weaponItem[beforeWeapon - 1].color = Color.white;
            }
            else if(slotBeforeWeaponID == null || slotBeforeWeaponID.item == null) {
                UIManager.Instance.weaponItem[beforeWeapon - 1].color = new Color(1, 1, 1, 0.2f);
            }
        }
        UIManager.Instance.weaponItem[uiPos].color = Color.green;
        animator.SetBool(Animation, true);
        StartCoroutine(AnimReset(Animation));
        return true;
    }

    // ü�� ��ȭ 
    [PunRPC]
    public override void ChangeHp( float value )
    {
        if (hp == 0) return;
        if(isDead) return;
        if (PV.IsMine)
        {
            hp += value;
            if (hp > 100)
                hp = 100;
            if (value > 0) {
                StartCoroutine(ShowHealScreen());   //�� ȭ�� ���
            }
            else if (value < 0) {
                StartCoroutine(ShowBloodScreen(value));  //�ǰ�ȭ�� ��� 
                hp = Mathf.Clamp(hp, 0, maxHp);
                UIManager.Instance.hpBar[0].value = (hp / maxHp) * 100;
            }
            photonView.RPC("UpdateHealthBar", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, photonView.ViewID, (hp / maxHp) * 100);
        }
    }

    // �÷��̾� ���� 
  
    public override void PlayerFaint()
    {
        if (hp <= 0 && PV.IsMine)                            //���� �÷��̾� ü���� 0���� �۾�����
        {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_help);
            hp = 0;                             //���⼭ hp�� 0 ���� ������ ����� ��Ȱ, ui���� ���ҰŰ���
            photonView.RPC("IsFaintRPC", RpcTarget.AllBuffered, true);                    //�������� true
            OnPlayerMove -= PlayerMove;
            OnPlayerRotation -= PlayerRotation;
            OnPlayerJump -= PlayerJump;
            OnPlayerAttack -= PlayerAttack;
            OnPlayerSwap -= WeaponSwap;
            OnPlayerInteraction -= PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
            OnPlayerInventory -= PlayerInventory;
            animator.SetBool("isFaint", true);          //���� �ִϸ��̼� ��� 
            inventory.SetActive(false);                 //�̰� ���ҽ� �κ��丮 Ű�� ����� �κ��丮 ���� ���׳� 
            StartCoroutine(AnimReset("isFaint"));
            StartCoroutine(PlayerFaintUI(faintTime));
            //capsuleCollider.direction = 2;
        }
  
            
    }
    [PunRPC]
    public void IsFaintRPC(bool isFaint)
    {
        this.isFaint = isFaint;
        if(isFaint)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.direction = 2;
        }
        else
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.direction = 1;
        }
        
    }
    [PunRPC]
    public void IsDeadRPC(bool isDead)
    {
        this.isDead = isDead;
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.direction = 2;
    }

    // �÷��̾� ���
    public override void PlayerDead()
    {
        if (hp <= 0 && isFaint)                            //���� �÷��̾� ü���� 0���� �۰� ��������
        {
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_Death);
            hp = 0;                                 //���⼭ hp�� 0   //anim.setbool("isFaint", true);    //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
            animator.SetBool("isDead", true);          //�׾����� �ִϸ��̼� ���
            StartCoroutine(AnimReset("isDead"));
            AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_death_BGM);
            photonView.RPC("IsFaintRPC", RpcTarget.AllBuffered, false);
            photonView.RPC("IsDeadRPC", RpcTarget.AllBuffered, true);
        }
    }

    //�÷��̾� �������½� ü���پ��� UI, ui�ٴ޸� ���� ���� �����ڷ�ƾ
    IEnumerator PlayerFaintUI(float faintTime)
    {
        Slider faintSlider = playerFaintUI.GetComponentInChildren<Slider>();
        Image[] images = playerFaintUI.GetComponentsInChildren<Image>();
        Color defaultColor = new Color(1, 1, 1, 0);
        faintSlider.value = 1;
        
        playerFaintUI.SetActive(true);
        while(faintSlider.value != 0)
        {
            yield return null;
            faintSlider.value -= Time.deltaTime / faintTime;
            defaultColor.a += Time.deltaTime / faintTime;
            images[3].color = defaultColor;
            images[4].color = defaultColor;
        }
        PlayerDead();
    }
    //�÷��̾� ��Ȱ �ڷ�ƾ + ui
    IEnumerator CorPlayerReviveUI(float time, Player otherPlayer)
    {
        Image fillImage = playerReviveUI.GetComponent<Image>();
        float _time = 0;
        while (time >= _time)
        {
            yield return null;
            _time += Time.deltaTime;
            fillImage.fillAmount = _time / time;
            if(!isRayPlayer || isFaint)
            {
                fillImage.fillAmount = 0;
                playerReviveUI.SetActive(false);
                yield break;
            }
        }
        otherPlayer.PlayerReviveRPC();
        fillImage.fillAmount = 0;
        playerReviveUI.SetActive(false);
        playerReviveUI.GetComponentInChildren<Text>().text = "";
    }
    void PlayerReviveRPC()
    {
        photonView.RPC("PlayerRevive", RpcTarget.AllBuffered);
    }
    // �÷��̾� ��Ȱ
    [PunRPC]
    public override void PlayerRevive()             //�÷��̾� ��Ȱ - �ٸ��÷��̾ ��Ȱ�Ҷ� ���� player�� �����ؼ� ȣ�� ���ο��� �Ⱦ����� �������⸦�������ʴ��̻�...
    {                                               //PlayerFaint �Լ��� �ݴ�� �ϸ� ��
        if(PV.IsMine)
        {
            OnPlayerMove += PlayerMove;                 // �÷��̾� �̵� 
            OnPlayerRotation += PlayerRotation;         // �÷��̾� ȸ��
            OnPlayerJump += PlayerJump;                 // �÷��̾� ���� 
            OnPlayerAttack += PlayerAttack;             // �÷��̾� ����
            OnPlayerSwap += WeaponSwap;                 // ���� ��ü
            OnPlayerInteraction += PlayerInteraction;   // �÷��̾� ��ȣ�ۿ�
            OnPlayerInventory += PlayerInventory;
            animator.SetBool("isRevive", true);     //���� �ִϸ��̼� ��� ���߿� �÷��̾� �ϼ��Ǹ� �߰�
            StartCoroutine(AnimReset("isRevive"));
            Hp = 50;                             //��Ȱ�� ���Ƿ� ���� maxHp = 100; �� ���� �����ؼ� maxHp / 2�ص� �Ǵµ� Ǯ�Ǵ� 100�ϰ��� ��
            photonView.RPC("IsFaintRPC", RpcTarget.AllBuffered, false);
            photonView.RPC("UpdateHealthBar", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, photonView.ViewID, (hp / maxHp) * 100);
            playerFaintUI.SetActive(false);      //����ȭ�� ����
        }
        
    }
    // �ǰݽ� ���̴� ���� 
    IEnumerator ShowBloodScreen(float value)                  //ȭ�� �Ӱ�
    {
        if(value > -5)
        {
            bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.1f, 0.15f));  //�û��Ӱ� ����
        }
        else if(value > -20)
        {
            bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.3f, 0.4f));  //�û��Ӱ� ����
        }
        else
        {
            bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.5f, 0.6f));  //�û��Ӱ� ����
        }

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

    //���� �ڷ�ƾ
    IEnumerator HealItemUse(float time, float healAmount, Slot slot)                                                     
    {
        if (Hp == 100) yield break;
        OnPlayerInteraction -= PlayerInteraction;

        Image fillImage = playerHealPackUI.GetComponent<Image>();
        playerHealPackUI.SetActive(true);
        float _time = 0;
        while (time >= _time)
        {
            yield return null;
            _time += Time.deltaTime;
            fillImage.fillAmount = _time / time;
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Interaction)) || isFaint)
            {
                OnPlayerInteraction += PlayerInteraction;

                playerHealPackUI.SetActive(false);
                fillImage.fillAmount = 0;
                yield break;
            }
        }
        OnPlayerInteraction += PlayerInteraction;

        theInventory.DecreaseMagazineCount(ItemController.ItemType.Healpack);
        if (slot.itemCount == 0)
            countZero = true;

        Hp = healAmount;
        fillImage.fillAmount = 0;
        playerHealPackUI.SetActive(false);
    }

    IEnumerator AnimReset(string animString = null, Animator handAnim = null)
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(animString, false);
        if(handAnim != null)
        handAnim.SetBool(animString, false);
    }

    IEnumerator DotDamage(EliteRangeEnemyDotArea _EREP)
    {
        Hp = -_EREP.dotDamage;
        AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_hurt);
        yield return new WaitForSeconds(_EREP.dotDelay);
        dotCoroutine = null;
    }

    // �÷��̾� ����ȭ
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        if (stream.IsWriting) {
            stream.SendNext(hp);
        }
        else {
            hp = (float)stream.ReceiveNext();
        }
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

    [PunRPC]
    public void UpdateHealthBar( string nickName, int viewID, float healthPercent ) {
        if (photonView.ViewID == viewID) {
            UIManager.Instance.UpdatePlayerHealthBar(nickName, viewID, healthPercent);
        }
    }


    public override void FaintChangeHp(float value)
    {
        if (PV.IsMine)
        {
            if(isDead)
            {
                return;
            }
            hp = 0;
            hp += value;
            if (hp > 100)
                hp = 100;
            if (value > 0)
            {
                StartCoroutine(ShowHealScreen());   //�� ȭ�� ���
            }
            photonView.RPC("UpdateHealthBar", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, photonView.ViewID, (hp / maxHp) * 100);
        }
    }
}
