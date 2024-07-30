using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InputKeyManager;
using static ItemController;

public class Slot : MonoBehaviourPun, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public int slotID;          // 장착 가능한 슬롯 ID
    public ItemController item; // 획득한 아이템
    public int itemCount;       // 획득한 아이템의 개수
    public Image itemImage;     // 아이템의 이미지

    [SerializeField]
    private Text text_Count;            // 아이템 개수 
    [SerializeField]
    private GameObject go_CountImage;   // 개수 Image 
    private Inventory inventory;        // 인벤토리 스크립트 

    private Rect baseRect;              // 아이템 버리기 범위 

    void Awake() {
        inventory = GetComponentInParent<Inventory>();
        baseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
    }

    // 아이템 이미지의 투명도 조절
    private void SetColor( float _alpha ) {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // 인벤토리에 새로운 아이템 슬롯 추가
    public void AddItem( ItemController _item, int _count, bool isEquip ) {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;
        // RPC로 isPickUp 속성을 false로 설정
        if (!isEquip)
            photonView.RPC("SetItemPickupStatus", RpcTarget.AllBuffered, _item.itemPrimaryID);

        // 무기 종류가 아닐 때 
        if (ItemController.ItemType.Gun != _item.type &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type &&
            ItemController.ItemType.Sword2 != _item.type ) {                 // 총과 칼은 합쳐지지 않는 무기기 때문에 if문으로 조건 처리
            go_CountImage.SetActive(true);                      // 개수 UI 활성화 
            text_Count.text = itemCount.ToString();             // 개수 UI 변경 
        }
        else {
            text_Count.text = "0";                              // 무기종류 일때는 0 고정 
            go_CountImage.SetActive(false);                     // 개수 UI 비활성화 
        }
        SetColor(1);
    }

    // 아이템을 버리고 주웠을 때 개수를 동기화 하기위한 RPC 
    [PunRPC]
    public void SetItemPickupStatus( int itemID ) {
        // 모든 ItemController를 찾아서 itemID가 일치하는 아이템의 isPickUp 속성을 false로 설정
        ItemController[] allItems = Resources.FindObjectsOfTypeAll<ItemController>();
        foreach (ItemController item in allItems) {
            if (item.itemPrimaryID == itemID) {
                item.isPickUp = false;
                break;
            }
        }
    }

    // 해당 슬롯의 아이템 갯수 업데이트
    public void SetSlotCount( int _count ) {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0) {
            ClearSlot();
        }
    }

    // 해당 슬롯 하나 삭제
    private void ClearSlot() {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(1);

        text_Count.text = "0";
        go_CountImage.SetActive(false);
    }

    public void OnPointerClick( PointerEventData eventData ) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            if (item == null) return;

            if (item != null && item.itemID != 0) {
                Slot targetSlot = inventory.FindSlotByID(item.itemID);

                if (targetSlot != null && targetSlot != this) {
                    if (targetSlot.item == null) {
                        targetSlot.AddItem(item, itemCount, true);
                        UIManager.Instance.weaponItem[targetSlot.item.itemID - 1].color = new Color(1, 1, 1, 1);
                        if(targetSlot.item.itemID == 1)
                            WeaponSwap();
                        ClearSlot();
                    }
                    else {
                        // 기존 아이템과 교환 전에 타겟 슬롯의 아이템 개수를 조정
                        int existingItemCount = targetSlot.itemCount;
                        ItemController existingItem = targetSlot.item;

                        // 현재 슬롯의 아이템을 타겟 슬롯에 추가
                        targetSlot.ClearSlot(); // 타겟 슬롯을 초기화하고 아이템 추가
                        targetSlot.AddItem(item, itemCount, true);

                        if (targetSlot.item.itemID == 1)
                            WeaponSwap();

                        // 이전에 타겟 슬롯에 있던 아이템을 현재 슬롯으로 이동
                        ClearSlot(); // 현재 슬롯 초기화
                        AddItem(existingItem, existingItemCount, true); // 아이템 교환
                    }

                    inventory.UpdateTotalGrenadeCountFromUI(2);
                    inventory.UpdateTotalGrenadeCountFromUI(3);
                }
                else {
                    Debug.Log("해당 슬롯이 없습니다.");
                }
            }

        }
    }

    public void OnBeginDrag( PointerEventData eventData ) {
        if (item != null) {
            DragSlot.instance.dragSlot = this;
            DragSlot.instance.DragSetImage(itemImage);
            DragSlot.instance.transform.position = eventData.position;
        }
    }

    // 마우스 드래그 중일 때 계속 발생하는 이벤트
    public void OnDrag( PointerEventData eventData ) {
        if (item != null)
            DragSlot.instance.transform.position = eventData.position;
    }

    // 마우스 드래그가 끝났을 때 발생하는 이벤트
    public void OnEndDrag( PointerEventData eventData ) {
        if (photonView.IsMine) {
            // 아이템 버리기 
            bool isHalf = false;

            if (DragSlot.instance.transform.localPosition.x < baseRect.xMin
               || DragSlot.instance.transform.localPosition.x > baseRect.xMax
               || DragSlot.instance.transform.localPosition.y < baseRect.yMin
               || DragSlot.instance.transform.localPosition.y > baseRect.yMax) {

                // 아이템 프리팹 생성해줘야함 
                if (DragSlot.instance.dragSlot != null && DragSlot.instance.dragSlot.item != null) {
                    ItemController draggedItem = DragSlot.instance.dragSlot.item;
                    string itemName = item.type.ToString();
                    int originCount = itemCount;
                    if (itemCount > 1) {
                        if (Input.GetKey(InputKeyManager.instance.GetKeyCode(KeyCodeTypes.Run))) {
                            isHalf = true;
                            itemCount /= 2;
                        }
                    }
                    // 아이템 프리팹 생성
                    GameObject itemObj = Pooling.instance.GetObject(itemName, Vector3.zero);
                    photonView.RPC("SetItemProperties", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID, itemCount);

                    if (inventory != null && inventory.go_MauntingSlotsParent != null) {
                        Transform slotsParent = inventory.go_MauntingSlotsParent.transform;
                        if (slotsParent.childCount > 1) {
                            Transform firstChild = slotsParent.GetChild(0);
                            Transform grandChild = firstChild.GetChild(0);
                            Image grandChildImage = grandChild.GetComponent<Image>();

                            if (grandChildImage != null && grandChildImage.sprite != null) {
                                string imageComponent = grandChildImage.sprite.name;
                                if(imageComponent == null) {
                                    UIManager.Instance.CurBulletCount.text = "0";
                                }
                            }
                        }
                    }


                    itemObj.transform.position = gameObject.GetComponentInParent<Player>().bulletPos.position;
                    itemObj.transform.rotation = Quaternion.identity;

                    ItemController.ItemType droppedItemType = DragSlot.instance.dragSlot.item.type;
                    DragSlot.instance.dragSlot.ClearSlot();

                    if (isHalf) {
                        if (originCount % 2 == 0) {
                            AddItem(draggedItem, originCount / 2, true);
                        }
                        else {
                            AddItem(draggedItem, originCount / 2 + 1, true);
                        }
                    }

                    if (droppedItemType == ItemController.ItemType.Magazine) {
                        UIManager.Instance.UpdateTotalBulletCount(inventory.CalculateTotalItems(ItemController.ItemType.Magazine));
                    }
                    else if (droppedItemType == ItemController.ItemType.ShotMagazine) {
                        UIManager.Instance.UpdateTotalBulletCount(inventory.CalculateTotalItems(ItemController.ItemType.ShotMagazine));
                    }

                    inventory.UpdateTotalGrenadeCountFromUI(2);
                    inventory.UpdateTotalGrenadeCountFromUI(3);
                }


                Slot[] slots = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>();
                foreach (Slot slot in slots) {
                    if (slot.item == null) {
                        if (slot.slotID - 1 < UIManager.Instance.weaponItem.Length) {
                            UIManager.Instance.weaponItem[slot.slotID - 1].color = new Color(1, 1, 1, 0.2f);
                            Player PlayerHand = GetComponentInParent<Player>();
                            if (PlayerHand.equipWeapon != null) {
                                PlayerHand.equipWeapon.SetActive(false);
                                PlayerHand.weaponSelected = false;
                            }
                        }
                    }
                    WeaponSwap();
                } 
            }

            DragSlot.instance.SetColor(0);
            DragSlot.instance.dragSlot = null;
        }
    }

    [PunRPC]
    public void SetItemProperties( int itemViewID, int itemCount ) {
        GameObject itemObj = PhotonView.Find(itemViewID).gameObject;
        itemObj.GetComponent<ItemPickUp>().totalCount = itemCount;
        itemObj.GetComponent<ItemPickUp>().isPickUp = true;
    }

    // 해당 슬롯에 무언가가 마우스 드롭 됐을 때 발생하는 이벤트
    public void OnDrop( PointerEventData eventData ) {
        if (DragSlot.instance.dragSlot != null)
            ChangeSlot();

        Slot[] slots = inventory.go_MauntingSlotsParent.GetComponentsInChildren<Slot>();
        foreach (Slot slot in slots) {
            if (slot.item == null) {
                if (slot.slotID - 1 < UIManager.Instance.weaponItem.Length) {
                    UIManager.Instance.weaponItem[slot.slotID - 1].color = new Color(1, 1, 1, 0.2f);
                    WeaponSwap();
                    inventory.UpdateTotalGrenadeCountFromUI(2);
                    inventory.UpdateTotalGrenadeCountFromUI(3);
                    Player PlayerHand = GetComponentInParent<Player>();
                    if (PlayerHand.equipWeapon != null) {
                        PlayerHand.equipWeapon.SetActive(false);
                        PlayerHand.weaponSelected = false;
                    }
                }
            }
        }
    }

    private void ChangeSlot() {
        if (slotID == 0 || DragSlot.instance.dragSlot.item.itemID == slotID) {
            ItemController _tempItem = item;
            int _tempItemCount = itemCount;

            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount, true);

            if (DragSlot.instance.dragSlot.item.itemID == slotID && slotID != 0) {
                UIManager.Instance.weaponItem[slotID - 1].color = new Color(1, 1, 1, 1);
                WeaponSwap();
                inventory.UpdateTotalGrenadeCountFromUI(2);
                inventory.UpdateTotalGrenadeCountFromUI(3);
            }

            if (_tempItem != null) {
                DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount, true);
                inventory.UpdateTotalGrenadeCountFromUI(2);
                inventory.UpdateTotalGrenadeCountFromUI(3);
            }
            else
                DragSlot.instance.dragSlot.ClearSlot();
        }
    }

    public bool HasItem() {
        return item != null; // item이 null이 아니면 슬롯에 아이템이 있음
    }

    public void WeaponSwap()
    {
        Player player = transform.root.GetComponent<Player>();
        if (inventory != null && inventory.go_MauntingSlotsParent != null) {
            if (player.WeaponSwapStatus(0, false, false, true, "isDrawRifle", 0, player.beforeWeapon)) {
                player.countZero = false;
                player.beforeWeapon = 1;
            }
            Transform slotsParent = inventory.go_MauntingSlotsParent.transform;
            if (slotsParent.childCount > 1) {
                Transform firstChild = slotsParent.GetChild(0);
                Transform grandChild = firstChild.GetChild(0);
                Image grandChildImage = grandChild.GetComponent<Image>();

                if (grandChildImage != null && grandChildImage.sprite != null) {
                    string imageComponent = grandChildImage.sprite.name;

                    if (imageComponent.Equals("Gun")) {
                        UIManager.Instance.CurBulletCount.text = "0";
                        UIManager.Instance.UpdateTotalBulletCount(inventory.CalculateTotalItems(ItemController.ItemType.Magazine));
                        player.bulletCount[0] = 0;
                        if (imageComponent != null)
                            player.weaponIndex = 0;
                    }
                    else if (imageComponent.Equals("ShotGun")) {
                        UIManager.Instance.CurBulletCount.text = "0";
                        UIManager.Instance.UpdateTotalBulletCount(inventory.CalculateTotalItems(ItemController.ItemType.ShotMagazine));
                        player.bulletCount[1] = 0;
                        if (imageComponent != null)
                            player.weaponIndex = 4;
                    }
                }            
            }  
            player.isGun = true;
        }   
        
    }
}