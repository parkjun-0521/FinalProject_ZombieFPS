using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public ItemController item; // ȹ���� ������
    public int itemCount;       // ȹ���� �������� ����
    public Image itemImage;     // �������� �̹���

    [SerializeField]
    private Text text_Count;
    [SerializeField]
    private GameObject go_CountImage;

    // ������ �̹����� ���� ����
    private void SetColor( float _alpha ) {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // �κ��丮�� ���ο� ������ ���� �߰�
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

    // �ش� ������ ������ ���� ������Ʈ
    public void SetSlotCount( int _count ) {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)
            ClearSlot();
    }

    // �ش� ���� �ϳ� ����
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
                    // ����
                    Debug.Log("��");
                }
                else if (item.type == ItemController.ItemType.ShotGun){
                    Debug.Log("����");
                }
                else if (item.type == ItemController.ItemType.Sword1) {
                    Debug.Log("Į1");
                }
                else if (item.type == ItemController.ItemType.Sword2) {
                    Debug.Log("Į2");
                }
                else if(item.type == ItemController.ItemType.Magazine) {
                    Debug.Log("ź��");
                }
                else if(item.type == ItemController.ItemType.Grenade) {
                    Debug.Log("����ź");
                }
                else if(item.type == ItemController.ItemType.FireGrenade) {
                    Debug.Log("ȭ����");
                }
                else if(item.type == ItemController.ItemType.SupportFireGrenade) {
                    Debug.Log("���� ����ź");
                }
                else if(item.type == ItemController.ItemType.Healpack) {
                    Debug.Log("����");
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

    // ���콺 �巡�� ���� �� ��� �߻��ϴ� �̺�Ʈ
    public void OnDrag( PointerEventData eventData ) {
        if (item != null)
            DragSlot.instance.transform.position = eventData.position;
    }

    // ���콺 �巡�װ� ������ �� �߻��ϴ� �̺�Ʈ
    public void OnEndDrag( PointerEventData eventData ) {
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    // �ش� ���Կ� ���𰡰� ���콺 ��� ���� �� �߻��ϴ� �̺�Ʈ
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