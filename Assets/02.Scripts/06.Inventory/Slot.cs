using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public ItemController item; // 획득한 아이템
    public int itemCount;       // 획득한 아이템의 개수
    public Image itemImage;     // 아이템의 이미지

    [SerializeField]
    private Text text_Count;
    [SerializeField]
    private GameObject go_CountImage;

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
            _count = 30;
        else
            _count = 1;
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
            if (item != null) {
                if (item.type == ItemController.ItemType.Gun) {
                    // 장착
                    Debug.Log("총");
                }
                else if (item.type == ItemController.ItemType.ShotGun){
                    Debug.Log("샷건");
                }
                else if (item.type == ItemController.ItemType.Sword1) {
                    Debug.Log("칼1");
                }
                else if (item.type == ItemController.ItemType.Sword2) {
                    Debug.Log("칼2");
                }
                else if(item.type == ItemController.ItemType.Magazine) {
                    Debug.Log("탄약");
                }
                else if(item.type == ItemController.ItemType.Grenade) {
                    Debug.Log("수류탄");
                }
                else if(item.type == ItemController.ItemType.FireGrenade) {
                    Debug.Log("화염병");
                }
                else if(item.type == ItemController.ItemType.SupportFireGrenade) {
                    Debug.Log("지원 수류탄");
                }
                else if(item.type == ItemController.ItemType.Healpack) {
                    Debug.Log("힐팩");
                    SetSlotCount(-1);
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
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    // 해당 슬롯에 무언가가 마우스 드롭 됐을 때 발생하는 이벤트
    public void OnDrop( PointerEventData eventData ) {
        if (DragSlot.instance.dragSlot != null)
            ChangeSlot();
    }

    private void ChangeSlot() {
        ItemController _tempItem = item;
        int _tempItemCount = itemCount;

        AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

        if (_tempItem != null)
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
        else
            DragSlot.instance.dragSlot.ClearSlot();
    }
}