using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            ItemController.ItemType.Sword2 != _item.type) {
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
        if (itemCount <= 0)
            ClearSlot();
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
                        ClearSlot();
                    }
                    else {
                        // 기존 아이템과 교환 전에 타겟 슬롯의 아이템 개수를 조정
                        int existingItemCount = targetSlot.itemCount;
                        ItemController existingItem = targetSlot.item;

                        // 현재 슬롯의 아이템을 타겟 슬롯에 추가
                        targetSlot.ClearSlot(); // 타겟 슬롯을 초기화하고 아이템 추가
                        targetSlot.AddItem(item, itemCount, true);

                        // 이전에 타겟 슬롯에 있던 아이템을 현재 슬롯으로 이동
                        ClearSlot(); // 현재 슬롯 초기화
                        AddItem(existingItem, existingItemCount, true); // 아이템 교환
                    }
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
            if (DragSlot.instance.transform.localPosition.x < baseRect.xMin
               || DragSlot.instance.transform.localPosition.x > baseRect.xMax
               || DragSlot.instance.transform.localPosition.y < baseRect.yMin
               || DragSlot.instance.transform.localPosition.y > baseRect.yMax) {

                // 아이템 프리팹 생성해줘야함 
                if (DragSlot.instance.dragSlot != null && DragSlot.instance.dragSlot.item != null) {
                    string itemName = item.type.ToString();
                    // 아이템 프리팹 생성
                    GameObject itemObj = Pooling.instance.GetObject(itemName);
                    photonView.RPC("SetItemProperties", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID, itemCount);

                    itemObj.transform.position = gameObject.GetComponentInParent<Player>().bulletPos.position; // bullet 위치 초기화
                    itemObj.transform.rotation = Quaternion.identity; // bullet 회전값 초기화
                }

                DragSlot.instance.dragSlot.ClearSlot();
            }

            DragSlot.instance.SetColor(0);
            DragSlot.instance.dragSlot = null;
        }
    }

    [PunRPC]
    public void SetItemProperties( int itemViewID, int itemCount ) {
        GameObject itemObj = PhotonView.Find(itemViewID).gameObject;
        itemObj.GetComponent<ItemPickUp>().item.totalCount = itemCount;
        itemObj.GetComponent<ItemPickUp>().item.isPickUp = true;
    }

    // 해당 슬롯에 무언가가 마우스 드롭 됐을 때 발생하는 이벤트
    public void OnDrop( PointerEventData eventData ) {
        if (DragSlot.instance.dragSlot != null)
            ChangeSlot();
    }

    private void ChangeSlot() {
        if (slotID == 0 || DragSlot.instance.dragSlot.item.itemID == slotID) {
            ItemController _tempItem = item;
            int _tempItemCount = itemCount;

            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount, true);

            if (_tempItem != null)
                DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount, true);
            else
                DragSlot.instance.dragSlot.ClearSlot();
        }
    }

    public bool HasItem() {
        return item != null; // item이 null이 아니면 슬롯에 아이템이 있음
    }
}