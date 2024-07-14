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
    // delegate 선언
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
        // 레퍼런스 초기화 
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

            // 인벤토리
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Inventory))) {
                OnPlayerInventory?.Invoke();
                ToggleCursor();
            }
          
            // 플레이어 상호작용
            if (Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) {
                OnPlayerInteraction?.Invoke();
            }

            // 플레이어 회전
            OnPlayerRotation?.Invoke();

            // 마우스 커서 생성 
            if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                ToggleCursor();
            }
        }
    }

    // 마우스 커서 생성 
    private void ToggleCursor() {
        cursorLocked = !cursorLocked;
        Cursor.visible = !cursorLocked;
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    void FixedUpdate() {
        // delegate 등록
        if (PV.IsMine) {
            // 이동
            if (cursorLocked) {
                bool isRun = Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Run));
                OnPlayerMove?.Invoke(isRun);
            }
        }
    }

    void OnTriggerEnter( Collider other )                       //좀비 트리거콜라이더에 enter했을때
    {
        if (PV.IsMine) {
            // 적과 충돌 
            if (other.CompareTag("Enemy")){
                //hp = -(other.GetComponent<Enemy>().attackdamage)  //-로 했지만 좀비쪽에서 공격력을 -5 이렇게하면 여기-떼도됨
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

            if(isMove) 
                animator.SetFloat("speedBlend", type ? 1.0f : 0.5f);
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
            OnPlayerMove -= PlayerMove;                 // 플레이어 이동 해제
            OnPlayerRotation -= PlayerRotation;         // 플레이어 회전 해제
            OnPlayerJump -= PlayerJump;                 // 플레이어 점프 해제
            OnPlayerAttack -= PlayerAttack;             // 플레이어 공격 해제
            OnPlayerSwap -= WeaponSwap;                 // 무기 교체 해제
            OnPlayerInteraction -= PlayerInteraction;   // 플레이어 상호작용 해제
            inventory.SetActive(true);
        }
    }
    // 인벤토리 비활성화
    public void InventoryClose() {
        if (PV.IsMine) {
            // 마우스 비활성화
            Cursor.visible = false;                         // 마우스 커서 비활성화
            Cursor.lockState = CursorLockMode.Locked;       // 마우스 커서 현재 위치 고정 
            ToggleCursor();

            OnPlayerMove += PlayerMove;                 // 플레이어 이동 
            OnPlayerRotation += PlayerRotation;         // 플레이어 회전
            OnPlayerJump += PlayerJump;                 // 플레이어 점프 
            OnPlayerAttack += PlayerAttack;             // 플레이어 공격
            OnPlayerSwap += WeaponSwap;                 // 무기 교체
            OnPlayerInteraction += PlayerInteraction;   // 플레이어 상호작용
            inventory.SetActive(false);
        }
    }

    // 플레이어 상호작용 
    public override void PlayerInteraction() {
        if (PV.IsMine) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            int layerMask = LayerMask.GetMask("Player", "Item");
            float radius = 1.5f;
            if (Physics.Raycast(ray, out hit, interactionRange, layerMask))   //레이어 이름, 거리에대해 상의
            {
                if (hit.collider.CompareTag("Item")) {           // ex)text : 'E' 아이템줍기 ui띄워주기
                    ItemPickUp(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Player")) {    //만약 플레이어면
                                                                 //ex)text : 'E' 플레이어 살리기 ui띄워주기
                    if (hit.collider.GetComponent<Player>().isFaint == true) //만약 태그가 player고 기절이 true면
                    {
                        //slider or shader로 (slider가 편할듯) 살려주기 바가 차오름
                        //슬라이더 밸류가 1이 되는순간 순간 그녀석의 player에 접근해서 PlayerRevive()함수호출
                    }
                }
            }
        }
    }

    private void ItemPickUp(GameObject itemObj) {
        if (theInventory.IsFull()) {
            Debug.Log("인벤토리가 가득 찼습니다. 더 이상 아이템을 줍지 못합니다.");
        }
        else {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                Debug.Log(itemObj.transform.GetComponent<ItemPickUp>().item.itemName + " 획득 했습니다.");  // 인벤토리 넣기
                theInventory.AcquireItem(itemObj.transform.GetComponent<ItemPickUp>().item);
                // 아이템 제거
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

    // 근접 칼
    void SwordAttack()
    {
        if (PV.IsMine) {
            Debug.Log("칼 공격");
            // 근거리 공격 애니메이션 
            // 데미지는 weapon에서 줄꺼임 그리고 체력은 좀비에서 감소시킬예정
        }
    }

    // 원거리 총
    void GunAttack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Magazine)) {
                Debug.Log("탄창 없음");
                return; // 탄창이 없으면 메소드를 종료하여 총을 쏘지 않음
            }

            // 카메라 중앙에서 Ray 생성 
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Ray 테스트 
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.red); // 나중에 지우기

            Vector3 targetPoint;

            muzzleFlashEffect.Play();
            // 충돌 확인
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer)) {
                Debug.Log("총 공격");
                targetPoint = hit.point;
            }
            else {
                // 레이가 맞지 않았을 때는 먼 지점을 목표로 설정
                targetPoint = ray.origin + ray.direction * 1000f;
            }

            theInventory.DecreaseMagazineCount(ItemController.ItemType.Magazine);

            // 총알 생성 (오브젝트 풀링 사용)
            GameObject bullet = Pooling.instance.GetObject("Bullet"); // 총알이 들어가 있는 index로 변경 (0은 임시)
            bullet.transform.position = bulletPos.position; // bullet 위치 초기화
            bullet.transform.rotation = Quaternion.identity; // bullet 회전값 초기화


            // 총알의 방향 설정
            Vector3 direction = (targetPoint - bulletPos.position).normalized;


            // 총알의 초기 속도를 플레이어의 이동 속도로 설정하고 발사 방향 설정
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(direction * 300f, ForceMode.VelocityChange); // 발사 방향과 속도를 함께 적용
        }
    }

    // 근거리 아이템 힐팩 
    void ItemHealpack()
    {
        if (PV.IsMine) {
            if (!theInventory.HasItemUse(ItemController.ItemType.Healpack)) {
                Debug.Log("힐팩 없음");
                return; // 탄창이 없으면 메소드를 종료하여 총을 쏘지 않음
            }
            theInventory.DecreaseMagazineCount(ItemController.ItemType.Healpack);
            Debug.Log("힐팩");

            //힐 하는시간 변수로 빼고 대충 중앙에 ui띄우고 힐 하는시간 지나면 Hp = (+30) 코루틴사용이 좋겠지 중간에 키입력시 return 애니메이션추가;
            
        }
    }

    // 원거리 아이템 수류탄  
    void ItemGrenade()
    {
        if (PV.IsMine) {
            Debug.Log("투척 공격");
            animator.SetTrigger("isGranadeThrow");
            float throwForce = 15f;    // 던지는 힘


            GameObject grenade = Pooling.instance.GetObject("GrenadeObject"); 
            Rigidbody grenadeRigid = grenade.GetComponent<Rigidbody>();
            grenadeRigid.velocity = Vector3.zero;
            grenade.transform.position = grenadePos.position; // bullet 위치 초기화                   
            grenade.transform.rotation = Quaternion.identity; // bullet 회전값 초기화 

            // 카메라의 중앙에서 나가는 레이 구하기
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit)) {
                targetPoint = hit.point;  // 목표 지점이 충돌 지점
            }
            else {
                targetPoint = ray.GetPoint(1000);  // 충돌이 없으면 먼 지점으로 설정
            }
            // 던질 방향 계산
            Vector3 throwDirection = (targetPoint - grenade.transform.position).normalized;

            // Rigidbody에 힘을 가함
            grenadeRigid.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
    }

    // 무기 교체
    public override void WeaponSwap() {
        if (PV.IsMine) {
            int weaponIndex = -1;           // 초기 무기 인덱스 ( 빈손 ) 
            bool weaponSelected = false;    // 무기가 선택되었는지 확인 

            if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon1))) {        // 원거리 무기 
                weaponIndex = 0;
                isAtkDistance = stanceWeaponType = false;
                Debug.Log("원거리");
                weaponSelected = true;
                animator.SetTrigger("isDrawRifle");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon2))) {   // 근접 무기
                weaponIndex = 1;
                isAtkDistance = true;
                stanceWeaponType = false;
                Debug.Log("근거리");
                weaponSelected = true;
                animator.SetTrigger("isDrawMelee");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon3))) {   // 투척 무기
                weaponIndex = 2;
                isAtkDistance = false;
                stanceWeaponType = true;
                Debug.Log("투척");
                weaponSelected = true;
                animator.SetTrigger("isDrawGranade");
                StartCoroutine(AnimReset());
            }
            else if (Input.GetKey(keyManager.GetKeyCode(KeyCodeTypes.Weapon4))) {   // 힐팩
                weaponIndex = 3;
                isAtkDistance = stanceWeaponType = true;
                Debug.Log("힐");
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

    // 아이템 버리기 ( 버리는 item id가져오기 )
    public override void ItemThrowAway( int id ) {
        throw new System.NotImplementedException();
    }

    // 체력 변화 
    public override void ChangeHp( float value ) {
        hp += value;
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
            OnPlayerMove -= PlayerMove;         //움직이는거막고
            OnPlayerAttack -= PlayerAttack;     //공격도 막겠다
            animator.SetTrigger("isFaint");     //기절 애니메이션 출력 
        }
    }

    // 플레이어 부활
    public override void PlayerRevive()            //플레이어 부활 - 다른플레이어가 부활할때 얘의 player에 접근해서 호출 내부에선 안쓸꺼임 제세동기를만들지않는이상...
    {                                        //PlayerFaint 함수와 반대로 하면 됨
        isFaint = false;                     //기절상태 false
        OnPlayerMove += PlayerMove;          //움직이는거 더하고
        OnPlayerAttack += PlayerAttack;      //공격도 더함 
        animator.SetTrigger("isRevive");    //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
        Hp = 50;                             //부활시 반피로 변경! maxHp = 100; 을 따로 선언해서 maxHp / 2해도 되는데 풀피는 100하겠지 뭐
    }

    // 플레이어 사망
    public override void PlayerDead()
    {
        if (hp <= 0 && isFaint)                            //만약 플레이어 체력이 0보다 작고 기절상태
        {
            hp = 0;                                 //여기서 hp를 0   //anim.setbool("isFaint", true);    //기절 애니메이션 출력 나중에 플레이어 완성되면 추가
            OnPlayerMove -= PlayerMove;             // 플레이어 이동 
            OnPlayerRotation -= PlayerRotation;     // 플레이어  회전
            OnPlayerJump -= PlayerJump;             // 플레이어 점프 
            OnPlayerAttack -= PlayerAttack;         // 플레이어 공격
            OnPlayerSwap -= WeaponSwap;             // 무기 교체
            animator.SetTrigger("isDead");          //죽었을때 애니메이션 출력

        }
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

    IEnumerator HealItemUse()                                                     //체력회복아이템사용 임시
    {
        int hpTime = 0;
        animator.SetTrigger("isHealUse");
        while(!Input.GetKeyDown(keyManager.GetKeyCode(KeyCodeTypes.Interaction))) //상호작용키(e)를 누르면 취소
        {
            yield return new WaitForSeconds(0.1f);
            hpTime++;
            //ui출력추가
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
