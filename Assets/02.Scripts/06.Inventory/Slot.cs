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
    private Text text_Count;
    [SerializeField]
    private GameObject go_CountImage;

    private Inventory inventory;

    private Rect baseRect;

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
    public void AddItem( ItemController _item, int _count = 1 ) {
        item = _item;
        if (ItemController.ItemType.Magazine == _item.type)
            _count = 30;    // 총알은 30발 
        else
            _count = 1;     // 나머진 1개씩 
        itemCount = _count;
        itemImage.sprite = item.itemImage;

        if (ItemController.ItemType.Gun != _item.type       &&
            ItemController.ItemType.ShotGun != _item.type   &&
            ItemController.ItemType.Sword1 != _item.type    &&
            ItemController.ItemType.Sword2 != _item.type ) {
            go_CountImage.SetActive(true);
            text_Count.text = itemCount.ToString();
        }
        else {
            text_Count.text = "0";
            go_CountImage.SetActive(false);
        }

        SetColor(1);
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
            if(item.type == ItemController.ItemType.Magazine) {
                SetSlotCount(-1);
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                if (item != null && item.itemID != 0) {
                    Slot targetSlot = inventory.FindSlotByID(item.itemID);

                    if (targetSlot != null && targetSlot != this) {
                        if (targetSlot.item == null) {
                            targetSlot.AddItem(item, itemCount);
                            ClearSlot();
                        }
                        else {
                            int tempItemCount = targetSlot.itemCount;
                            ItemController tempItem = targetSlot.item;

                            targetSlot.AddItem(item, itemCount);
                            AddItem(tempItem, tempItemCount);
                        }
                    }
                    else {
                        Debug.Log("해당 슬롯이 없습니다.");
                    }
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
                string itemName = item.type.ToString();
                // 아이템 프리팹 생성
                GameObject itemObj = Pooling.instance.GetObject(itemName);
                itemObj.transform.position = gameObject.GetComponentInParent<Player>().bulletPos.position; // bullet 위치 초기화
                itemObj.transform.rotation = Quaternion.identity; // bullet 회전값 초기화

                DragSlot.instance.dragSlot.ClearSlot();
            }

            DragSlot.instance.SetColor(0);
            DragSlot.instance.dragSlot = null;
        }
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

            AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

            if (_tempItem != null)
                DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
            else
                DragSlot.instance.dragSlot.ClearSlot();
        }
    }

    public bool HasItem() {
        return item != null; // item이 null이 아니면 슬롯에 아이템이 있음
    }
}