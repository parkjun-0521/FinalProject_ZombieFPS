using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InputKeyManager;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class Player : PlayerController 
{
    // delegate 선언
    public delegate void PlayerMoveHandler( bool value );
    public static event PlayerMoveHandler OnPlayerMove, OnPlayerAttack;

    public delegate void PlayerJumpedHandler();
    public static event PlayerJumpedHandler OnPlayerRotation, OnPlayerJump, OnPlayerSwap, OnPlayerInteraction, OnPlayerInventory;

    private RotateToMouse rotateToMouse;
    private InputKeyManager keyManager;
    public Camera playerCamera;

    public bool cursorLocked = true;

    // 상호작용 Ray  
    RaycastHit hit;
    Ray ray;
    bool isRayPlayer = false;

    void Awake()
    {
        // 레퍼런스 초기화 
        PV = GetComponent<PhotonView>();
        if (PV.IsMine) {
            rigid = GetComponent<Rigidbody>();
            Animator[] animators = GetComponentsInChildren<Animator>();
            animator = animators[1];                        //나중에 새로운 애니메이터 중간에 들어오면 좀 불안정해짐 
            handAnimator = animators[2];
            capsuleCollider = GetComponent<CapsuleCollider>();

            Cursor.visible = false;                         // 마우스 커서 비활성화
            Cursor.lockState = CursorLockMode.Locked;       // 마우스 커서 현재 위치 고정 
            rotateToMouse = GetComponentInChildren<RotateToMouse>();
           
        }
    }

    void OnEnable() {
        // 이벤트 등록
        OnPlayerMove += PlayerMove;                 // 플레이어 이동 
        OnPlayerRotation += PlayerRotation;         // 플레이어 회전
        OnPlayerJump += PlayerJump;                 // 플레이어 점프 
        OnPlayerAttack += PlayerAttack;             // 플레이어 공격
        OnPlayerSwap += WeaponSwap;                 // 무기 교체
        OnPlayerInteraction += PlayerInteraction;   // 플레이어 상호작용
        OnPlayerInventory += PlayerInventory;
    }

    void OnDisable() {
        // 이벤트 해제 
        OnPlayerMove -= PlayerMove;
        OnPlayerRotation -= PlayerRotation;
        OnPlayerJump -= PlayerJump;
        OnPlayerAttack -= PlayerAttack;
        OnPlayerSwap -= WeaponSwap;
        OnPlayerInteraction -= PlayerInteraction;   // 플레이어 상호작용
        OnPlayerInventory -= PlayerInventory;
    }

    void Start() {
        if (PV.IsMine) {
            keyManager = InputKeyManager.instance.GetComponent<InputKeyManager>();
            playerCamera.gameObject.SetActive(true);
            ItemController[] items = FindObjectsOfType<ItemController>(true); // true를 사용하여 비활성화된 오브젝트도 포함
        }
        else {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update() {
        // 단발적인 행동 
        if (PV.IsMine) {
            // 무기 스왑 
            OnPlayerSwap?.Invoke();

            // 공격
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Attack)) && !EventSystem.current.IsPointerOverGameObject()) {
                // 총,칼 0.1초, 수류탄,힐팩 1초 딜레이
                attackMaxDelay = stanceWeaponType ? 1.0f : 0.1f;

                animator.SetBool("isRifleMoveShot", true);  //총쏘는 애니메이션

                // 일정 딜레이가 될 때 마다 총알을 발사
                if (Time.time - lastAttackTime >= attackMaxDelay) {
                    OnPlayerAttack?.Invoke(isAtkDistance);
                    lastAttackTime = Time.time;                 // 딜레이 초기화
                }
            }
            
            // 점프 
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Jump)) && isJump) {
                OnPlayerJump?.Invoke();
            }

            // 공격 이후 애니메이션 
            if (Input.GetKeyUp(keyManager.GetKeyCode(KeyCodeTypes.Attack)))
            {
                animator.SetBool("isRifleMoveShot", false);
            }

            // 장전
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.BulletLoad)) && isGun) {
                StartCoroutine(BulletLoad());
            }

            // 인벤토리
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Inventory))) {
                OnPlayerInventory?.Invoke();
            }

            // 설정
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Setting)) && inventory.activeSelf) {
                InventoryClose();
                cursorLocked = false;
                ToggleCursor();
            }

            // 플레이어 상호작용
            OnPlayerInteraction?.Invoke();
            // 플레이어 회전
            OnPlayerRotation?.Invoke();

            // 마우스 커서 생성 
            if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                ToggleCursor();
            }

            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            //사람 죽은놈 쪽으로 레이쏴서 ui true
            
        }
    }

    // 마우스 커서 생성 
    public void ToggleCursor() {
        cursorLocked = !cursorLocked;           // cursorLocked : false => 마우스가 활성화 None, true => 마우스 비활성화 Lock
        Cursor.visible = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        if(cursorLocked) {
            OnPlayerMove += PlayerMove;                 // 플레이어 이동 
            OnPlayerRotation += PlayerRotation;         // 플레이어 회전
            OnPlayerJump += PlayerJump;                 // 플레이어 점프 
            OnPlayerAttack += PlayerAttack;             // 플레이어 공격
            OnPlayerSwap += WeaponSwap;                 // 무기 교체
            OnPlayerInteraction += PlayerInteraction;   // 플레이어 상호작용
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
        // delegate 등록
        if (PV.IsMine) {
            // 이동
            bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
            OnPlayerMove?.Invoke(isRun);
        }
    }

    void OnTriggerEnter( Collider other )                       //좀비 트리거콜라이더에 enter했을때
    {
        if (PV.IsMine) {
            // 적과 충돌 
            if (other.CompareTag("Enemy"))
            {
                Hp = -(other.GetComponentInParent<EnemyController>().damage);  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (PV.IsMine) {
            // 지면 태그 필요 
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

    // 플레이어 이동 ( 달리는 중인가 check bool ) 
    public override void PlayerMove(bool type) {
        if (PV.IsMine) {
            float x = 0f;
            float z = 0f;

            // 걷기, 달리기 속도 조절
            float playerSpeed = type ? runSpeed : speed;
            isMove = false;
            // 좌우 이동
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.LeftMove))) {
                isMove = true;
                x = -1f;
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.RightMove))) {
                isMove = true;
                x = 1f;
            }
            // 상하 이동         
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
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Walk);
            }
            else
                animator.SetFloat("speedBlend", 0);

            Vector3 moveDirection = (transform.forward * z + transform.right * x).normalized;
            rigid.MovePosition(transform.position + moveDirection * playerSpeed * Time.deltaTime);
        }
    }

    // 플레이어 회전
    public void PlayerRotation() {
        if (PV.IsMine) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            rotateToMouse.UpdateRotate(mouseX, mouseY);
        }
    }

    // 플레이어 점프 
    public override void PlayerJump() {
        // 땅에 붙어있을 때 점프
        if (PV.IsMine) {
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJump = false; 
        }
    }

    // 인벤토리 활성화
    public void PlayerInventory() {
        if (PV.IsMine) {
            cursorLocked = true;
            ToggleCursor();
            inventory.SetActive(true);
        }
    }
    // 인벤토리 비활성화
    public void InventoryClose() {
        if (PV.IsMine) {
            cursorLocked = false;
            ToggleCursor();
            inventory.SetActive(false);
        }
    }

    // 플레이어 상호작용 
    public override void PlayerInteraction() {
        if (PV.IsMine) {
            int layerMask = LayerMask.GetMask("LocalPlayer", "Item");
            if (Physics.Raycast(bulletPos.position, ray.direction, out hit, interactionRange, layerMask))   
            {
                if (hit.collider.CompareTag("Item"))
                {
                    playerReviveUI.SetActive(true);
                    playerReviveUI.GetComponentInChildren<Text>().text = string.Format("'E' {0} 아이템 줍기", hit.collider.GetComponent<ItemPickUp>().item.itemName);
                    if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction)))
                    {
                        ItemPickUp(hit.collider.gameObject);
                        if(hit.collider.GetComponent<ItemPickUp>().item.type == ItemController.ItemType.Magazine) {
                            UIManager.Instance.UpdateTotalBulletCount(theInventory.CalculateTotalItems(ItemController.ItemType.Magazine));
                        }
                    }
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    isRayPlayer = true;
                    Player otherPlayer = hit.collider.GetComponent<Player>();
                    if (otherPlayer.isFaint)
                    {
                        playerReviveUI.SetActive(true);
                        playerReviveUI.GetComponentInChildren<Text>().text = "'E' 플레이어 부활";
                        if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction)))
                        {
                            StartCoroutine(CorPlayerReviveUI(8.0f, otherPlayer));
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

    // 아이템 줍기 
    private void ItemPickUp(GameObject itemObj) {
        if (PV.IsMine) { 
            if (theInventory.IsFull()) {
                Debug.Log("인벤토리가 가득 찼습니다. 더 이상 아이템을 줍지 못합니다.");
            }
            else {
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                    // 인벤토리에 아이템 넣기 
                    theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>().item);
                    // 아이템 제거 동기화
                    PV.RPC("ItemPickUpRPC", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID);
                }
                else {
                    // 룸이 아닐 때 테스트 용 ( 추후 지울 예정 ) ============================================================
                    theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>().item);
                    itemObj.SetActive(false);
                    // 룸이 아닐 때 테스트 용 ( 추후 지울 예정 ) ============================================================
                }
            }
        }
    }

    // 아이템 주웠을 때 동기화 
    [PunRPC]
    public void ItemPickUpRPC(int viewID)
    {
        GameObject itemObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        if (itemObj != null)
            itemObj.SetActive(false);
    }

    // 플레이어 공격 ( 근접인지 원거리인지 판단 bool ) 
    public override void PlayerAttack(bool type) {
        // 무기 거리에 따른 공격 
        if (PV.IsMine) {
            if (type)
                // 근거리 공격
                (stanceWeaponType ? (Action)ItemHealpack : SwordAttack)();      // 힐팩 : 근접공격
            else
                // 원거리 공격
                (stanceWeaponType ? (Action)ItemGrenade : GunAttack)();         // 수류탄 : 원거리 공격 
        }
    }

    // 미장착 아이템 사용 불가
    public bool ItemNotEquipped(int slotID)
    {
        // theInventory에서 Inventory 컴포넌트를 가져옴
        Inventory inventory = theInventory.GetComponent<Inventory>();
        // MountingSlotsParent에서 Slot 컴포넌트들을 가져오고, ID가 3인 슬롯을 찾음
        Slot slotWithID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == slotID);

        // 무기가 장착되지 않았을 때
        if (slotWithID == null || slotWithID.item == null) {
            Debug.Log("장비가 장착되지 않음");
            return false;
        }

        switch (slotID) {
            case 3:
                // ID가 3인 슬롯이 null이거나 비어 있는 경우의 조건문   
                if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.Grenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.Grenade);
                }
                else if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.FireGrenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.FireGrenade);
                }
                else if (slotWithID != null && slotWithID.item.type == ItemController.ItemType.SupportFireGrenade) {
                    theInventory.DecreaseMagazineCount(ItemController.ItemType.SupportFireGrenade);
                }
                // 아이템을 다 쓰고 난 후 손에 있는 무기 비활성화
                if (slotWithID.itemCount == 0) {
                    countZero = true;
                }
                break;
            case 4:
                // ID가 4인 슬롯이 null이거나 비어 있는 경우의 조건문   
                StartCoroutine(HealItemUse(6.0f, 40.0f, slotWithID)); ; //힐 시간, 힐량
                // 아이템을 다 쓰고 난 후 손에 있는 무기 비활성화
                break;
        }
        return true;
    }

    // 근접 칼
    void SwordAttack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Sword1) && !theInventory.HasItemUse(ItemController.ItemType.Sword2)) {
                Debug.Log("칼이 없음");
                return;
            }
            // 아이템 미장착
            if (!ItemNotEquipped(2))
                return;

            Debug.Log("칼 공격");
            animator.SetBool("isMeleeWeaponSwing", true);          //외부에서 보여질때 애니메이션
            handAnimator.SetBool("isMeleeWeaponSwing", true);   //플레이어 1인칭 애니메이션
            StartCoroutine(AnimReset("isMeleeWeaponSwing", handAnimator));
            // 데미지는 weapon에서 줄꺼임 그리고 체력은 좀비에서 감소시킬예정
        }
    }

    // 원거리 총
    void GunAttack()
    {
        if (PV.IsMine) {
            // 아이템 미 장착 
            if (!ItemNotEquipped(1)) {
                bulletCount[0] = 0;
                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                return;
            }

            // 탄창이 없을 때 사격 X
            if (!theInventory.HasItemUse(ItemController.ItemType.Magazine)) {
                bulletCount[0] = 0;
                UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
                Debug.Log("탄창 없음");
                return; 
            }

            if (bulletCount[0] == 0 && isGun) {
                StartCoroutine(BulletLoad());
                isBulletZero = true;
            }

            // 장전 필요 
            if (isBulletZero) return;
            // 총 장착 안하면 못 쏘게 예외 처리 
            if (!isGun) return;

            // 카메라 중앙에서 Ray 생성 
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 targetPoint;
            // 이펙트 생성 
            muzzleFlashEffect.Play();
            // 충돌 확인
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                targetPoint = hit.point;
                hit.transform.GetComponent<EnemyController>().BloodEffect(hit.point);       // 좀비 피 이펙트 동작
            }
            else {
                targetPoint = ray.origin + ray.direction * 1000f;                           // 레이가 맞지 않았을 때는 먼 지점을 목표로 설정
            }         

            // 총알 생성 (오브젝트 풀링 사용)
            GameObject bullet = Pooling.instance.GetObject("Bullet");   // 총알 생성 
            bullet.transform.position = bulletPos.position;             // bullet 위치 초기화
            bullet.transform.rotation = Quaternion.identity;            // bullet 회전값 초기화

            TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
            if (trail != null) {
                trail.Clear();
            }

            // 탄창 개수 감소
            theInventory.DecreaseMagazineCount(ItemController.ItemType.Magazine);
            bulletCount[0] -= 1;
            UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();

            // 총알의 방향 설정
            Vector3 direction = (targetPoint - bulletPos.position).normalized;

            // 총알의 초기 속도를 플레이어의 이동 속도로 설정하고 발사 방향 설정
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            // 리지드바디의 속도와 각속도 초기화
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // 발사 방향과 속도를 함께 적용
            rb.AddForce(direction * 50f, ForceMode.VelocityChange);     
            
        }
    }

    IEnumerator BulletLoad()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("2초간 장전중");
        // 여기에 뭔가 애니메이션이나 사운드 넣어줘야 할듯 

        int availableBullets = theInventory.CalculateTotalItems(ItemController.ItemType.Magazine); // 이 메소드는 인벤토리에서 사용 가능한 모든 총알의 수를 반환해야 함

        if (availableBullets > 0) {
            bulletCount[0] = Mathf.Min(availableBullets, 30); // 남은 총알이 30개 이상이면 30개를, 그렇지 않으면 남은 총알만큼 장전
            UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
            isBulletZero = false; // 총알이 남아있음
        }
        else {
            bulletCount[0] = 0;
            UIManager.Instance.CurBulletCount.text = bulletCount[0].ToString();
            isBulletZero = false; // 총알이 다 떨어짐
        }
    }

    // 근거리 아이템 힐팩 
    void ItemHealpack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Healpack)) {
                Debug.Log("힐팩 없음");
                return; 
            }

            // 아이템 미장착 
            if (!ItemNotEquipped(4))
                return;
   
            Debug.Log("힐팩");

            //힐 하는시간 변수로 빼고 대충 중앙에 ui띄우고 힐 하는시간 지나면 Hp = (+30) 코루틴사용이 좋겠지 중간에 키입력시 return 애니메이션추가;
            
        }
    }

    // 원거리 아이템 수류탄  
    void ItemGrenade()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Grenade) && 
                !theInventory.HasItemUse(ItemController.ItemType.FireGrenade) && 
                !theInventory.HasItemUse(ItemController.ItemType.SupportFireGrenade)) {
                Debug.Log("투척무기 없음");
                return; 
            }
            // 아이템 미장착 
            if (!ItemNotEquipped(3))
                return;

            Debug.Log("투척 공격");
            animator.SetBool("isGrenadeThrow", true);
            StartCoroutine(AnimReset("isGrenadeThrow"));
            float throwForce = 15f;    // 던지는 힘

            GameObject grenade = Pooling.instance.GetObject("GrenadeObject");   // 수류탄 생성 
            Rigidbody grenadeRigid = grenade.GetComponent<Rigidbody>();

            // 초기 위치 설정
            grenadeRigid.velocity = Vector3.zero;               // 생성 시 가속도 초기화
            grenade.transform.position = grenadePos.position;   // bullet 위치 초기화                   
            grenade.transform.rotation = Quaternion.identity;   // bullet 회전값 초기화 

            // 카메라의 중앙에서 나가는 레이 구하기
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;
            Vector3 targetPoint;

            // 맞았을 때 , 안맞았을 때 충돌 지점 지정 
            targetPoint = (Physics.Raycast(ray, out hit)) ? hit.point : ray.GetPoint(1000);

            // 던질 방향 계산
            Vector3 throwDirection = (targetPoint - grenade.transform.position).normalized;

            // Rigidbody에 힘을 가함
            grenadeRigid.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
    }

    // 무기 교체
    public override void WeaponSwap() {
        if (PV.IsMine) {
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon1))) {        // 원거리 무기 
                if (WeaponSwapStatus(0, false, false, true, "isDrawRifle", 0, beforeWeapon)) {
                    countZero = false;
                    beforeWeapon = 1;
                }
                isGun = true;
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // 근접 무기
                if (WeaponSwapStatus(1, true, false, true, "isDrawMelee", 1, beforeWeapon)) {
                    countZero = false;
                    beforeWeapon = 2;
                }
                isGun = false;
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // 투척 무기
                if(WeaponSwapStatus(2, false, true, true, "isDrawGrenade", 2, beforeWeapon))
                    beforeWeapon = 3;
                isGun = false;
            }
            else if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // 힐팩
                if(WeaponSwapStatus(3, true, true, true, "isDrawHeal", 3, beforeWeapon))
                    beforeWeapon = 4;
                isGun = false;
            }

            // 무기를 선택하지 않았을 때 
            if (!weaponSelected) return;

            // 빈손일 경우 예외처리 
            if (equipWeapon != null)
                equipWeapon.SetActive(false);
            // 무기 선택 
            equipWeapon = weapons[weaponIndex];

            // 선택된 무기 활성화 ( 무기를 다 사용했을 시 비활성화 )
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
    // 무기 교체 상태 변경 : 인자값 (0:무기ID, 1:무기 사거리, 2:무기타입, 3:무기를 선택한 상태 ) 
    public bool WeaponSwapStatus(int weaponIndex, bool isAtkDistance, bool stanceWeaponType, bool weaponSelected, string Animation, int uiPos, int beforeWeapon)
    {
        // 아이템 장착된 것을 확인 후 예외 처리 
        Inventory inventory = theInventory.GetComponent<Inventory>();
        Slot slotWithID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == weaponIndex + 1);
        Slot slotBeforeWeaponID = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>().FirstOrDefault(slot => slot.slotID == beforeWeapon);

        if (slotWithID == null || slotWithID.item == null) {
            UIManager.Instance.weaponItem[uiPos].color = new Color(1, 1, 1, 0.2f);
            Debug.Log("장비가 장착되지 않음");
            return false;
        }
        // 아이템이 0보다 많으면 활성화 될 수 있도록 
        if (slotWithID.itemCount > 0) {
            countZero = false;
        }

        // 각 변수 상태 초기화
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

    // 체력 변화 
    public override void ChangeHp( float value ) {
        hp += value;
        if (hp > 100)
            hp = 100;
        if (value > 0) {
            StartCoroutine(ShowHealScreen());   //힐 화면 출력
        }
        else if (value < 0) {
            StartCoroutine(ShowBloodScreen());  //피격화면 출력 
        }
    }

    // 플레이어 기절 
    public override void PlayerFaint() {
        if (hp <= 0)                            //만약 플레이어 체력이 0보다 작아지면
        {
            hp = 0;                             //여기서 hp를 0 으로 강제로 해줘야 부활, ui에서 편할거같음
            isFaint = true;                     //기절상태 true
            OnPlayerMove -= PlayerMove;
            OnPlayerRotation -= PlayerRotation;
            OnPlayerJump -= PlayerJump;
            OnPlayerAttack -= PlayerAttack;
            OnPlayerSwap -= WeaponSwap;
            OnPlayerInteraction -= PlayerInteraction;   // 플레이어 상호작용
            OnPlayerInventory -= PlayerInventory;
            animator.SetBool("isFaint", true);     //기절 애니메이션 출력 
            StartCoroutine(AnimReset("isFaint"));
            StartCoroutine(PlayerFaintUI(faintTime));
            capsuleCollider.direction = 2;
        }
    }

    // 플레이어 부활
    public override void PlayerRevive()             //플레이어 부활 - 다른플레이어가 부활할때 얘의 player에 접근해서 호출 내부에선 안쓸꺼임 제세동기를만들지않는이상...
    {                                               //PlayerFaint 함수와 반대로 하면 됨
        isFaint = false;                            //기절상태 false
        OnPlayerMove += PlayerMove;                 // 플레이어 이동 
        OnPlayerRotation += PlayerRotation;         // 플레이어 회전
        OnPlayerJump += PlayerJump;                 // 플레이어 점프 
        OnPlayerAttack += PlayerAttack;             // 플레이어 공격
        OnPlayerSwap += WeaponSwap;                 // 무기 교체
        OnPlayerInteraction += PlayerInteraction;   // 플레이어 상호작용
        OnPlayerInventory += PlayerInventory;
        animator.SetBool("isRevive", true);     //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
        StartCoroutine(AnimReset("isRevive"));
        Hp = 50;                             //부활시 반피로 변경! maxHp = 100; 을 따로 선언해서 maxHp / 2해도 되는데 풀피는 100하겠지 뭐
    }

    // 플레이어 사망
    public override void PlayerDead()
    {
        if (hp <= 0 && isFaint)                            //만약 플레이어 체력이 0보다 작고 기절상태
        {
            hp = 0;                                 //여기서 hp를 0   //anim.setbool("isFaint", true);    //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
            animator.SetBool("isDead", true);          //죽었을때 애니메이션 출력
            StartCoroutine(AnimReset("isDead"));
        }
    }
   
    //void PlayerReviveUI()  //필요없는거같은데 혹시몰라서 7/22일정도까지 문제없을시 삭제
    //{
    //    if (Physics.Raycast(ray, out hit, interactionRange, LayerMask.NameToLayer("LocalPlayer") | LayerMask.NameToLayer("Item")))
    //    {
    //        if (hit.collider.CompareTag("Player"))
    //        {
    //            isRayPlayer = true;
    //            Player otherPlayer = hit.collider.GetComponent<Player>();
    //            if (otherPlayer.isFaint && hit.collider.CompareTag("Player"))
    //            {
    //                playerReviveUI.SetActive(true);
    //                if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction)))
    //                {
    //                    StartCoroutine(CorPlayerReviveUI(8.0f, otherPlayer));
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        playerReviveUI.SetActive(false);
    //        isRayPlayer = false;
    //    }
    //}

    //플레이어 기절상태시 체력줄어드는 UI
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
    }
    //플레이어 부활 코루틴 + ui
    IEnumerator CorPlayerReviveUI(float time, Player otherPlayer)
    {
        Image fillImage = playerReviveUI.GetComponent<Image>();
        float _time = 0;
        while (time >= _time)
        {
            yield return null;
            _time += Time.deltaTime;
            fillImage.fillAmount = _time / time;
            if(!isRayPlayer)
            {
                fillImage.fillAmount = 0;
                yield break;
            }
        }
        otherPlayer.PlayerRevive();
        fillImage.fillAmount = 0;
        playerReviveUI.SetActive(false);
    }
    // 피격시 셰이더 변경 
    IEnumerator ShowBloodScreen()                  //화면 붉게
    {
        bloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.Range(0.1f, 0.15f));  //시뻘겋게 변경
        yield return new WaitForSeconds(0.5f);                                          //0.5f초 후에   - 이거 변수로 뺄까?
        bloodScreen.color = Color.clear;                                                //화면 정상적으로 변경!
    }
    // 힐팩 아이템 사용시 셰이더 변경 
    IEnumerator ShowHealScreen()                   //화면 연두색 
    {
        float curTime = Time.time;                                          //현재의 시간을 변수로 저장을하고
        healScreen.color = new Color(1, 1, 1, 1);                           //힐 하는 이미지로 변경
        while (true)                                                        //반복문 1초후에 해제
        {
            yield return new WaitForSeconds(0.1f);                          //0.1초마다           
            float lerpColor = 1 - (Time.time - curTime);
            healScreen.color = new Color(1, 1, 1, lerpColor);               //대충 조금씩 없어진다고 생각하면됨(러프)
            if (lerpColor <= 0)                                             //1초후에
            {
                healScreen.color = Color.clear;                             //컬러를 0000으로 해서 정상적으로 변경하고
                break;                                                      //break로 반복문 탈출
            }
        }
    }

    //힐팩 코루틴
    IEnumerator HealItemUse(float time, float healAmount, Slot slot)                                                     
    {
        OnPlayerInteraction -= PlayerInteraction;

        Image fillImage = playerHealPackUI.GetComponent<Image>();
        playerHealPackUI.SetActive(true);
        float _time = 0;
        while (time >= _time)
        {
            yield return null;
            _time += Time.deltaTime;
            fillImage.fillAmount = _time / time;
            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Interaction)))
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
        //animator.ResetTrigger("isDrawGrenade");
        //animator.ResetTrigger("isDrawHeal");
        //animator.ResetTrigger("isDrawRifle");
        //animator.ResetTrigger("isDrawMelee");
        //animator.ResetTrigger(animString);
        animator.SetBool(animString, false);
        if(handAnim != null)
        handAnim.SetBool(animString, false);
    }

    // 플레이어 동기화
    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
        /*if (stream.IsWriting) {
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
        }*/
    }

    [ContextMenu("프로퍼티--")]                      //TEST용 추후삭제
    void test()
    {
        Hp = -1;
    }
    [ContextMenu("프로퍼티++")]                     //TEST용 추후삭제
    void test2()
    {
        Hp = +1;
    }
}
