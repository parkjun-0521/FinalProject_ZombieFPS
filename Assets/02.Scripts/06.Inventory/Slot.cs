using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviourPun, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public int slotID;          // ���� ������ ���� ID
    public ItemController item; // ȹ���� ������
    public int itemCount;       // ȹ���� �������� ����
    public Image itemImage;     // �������� �̹���

    [SerializeField]
    private Text text_Count;            // ������ ���� 
    [SerializeField]
    private GameObject go_CountImage;   // ���� Image 
    private Inventory inventory;        // �κ��丮 ��ũ��Ʈ 

    private Rect baseRect;              // ������ ������ ���� 

    void Awake() {
        inventory = GetComponentInParent<Inventory>();
        baseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
    }

    // ������ �̹����� ���� ����
    private void SetColor( float _alpha ) {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // �κ��丮�� ���ο� ������ ���� �߰�
    public void AddItem( ItemController _item, int _count, bool isEquip ) {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;
        // RPC�� isPickUp �Ӽ��� false�� ����
        if (!isEquip)
            photonView.RPC("SetItemPickupStatus", RpcTarget.AllBuffered, _item.itemPrimaryID);

        // ���� ������ �ƴ� �� 
        if (ItemController.ItemType.Gun != _item.type &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type &&
            ItemController.ItemType.Sword2 != _item.type) {
            go_CountImage.SetActive(true);                      // ���� UI Ȱ��ȭ 
            text_Count.text = itemCount.ToString();             // ���� UI ���� 
        }
        else {
            text_Count.text = "0";                              // �������� �϶��� 0 ���� 
            go_CountImage.SetActive(false);                     // ���� UI ��Ȱ��ȭ 
        }
        SetColor(1);
    }

    // �������� ������ �ֿ��� �� ������ ����ȭ �ϱ����� RPC 
    [PunRPC]
    public void SetItemPickupStatus( int itemID ) {
        // ��� ItemController�� ã�Ƽ� itemID�� ��ġ�ϴ� �������� isPickUp �Ӽ��� false�� ����
        ItemController[] allItems = Resources.FindObjectsOfTypeAll<ItemController>();
        foreach (ItemController item in allItems) {
            if (item.itemPrimaryID == itemID) {
                item.isPickUp = false;
                break;
            }
        }
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
            if (item == null) return;

            if (item != null && item.itemID != 0) {
                Slot targetSlot = inventory.FindSlotByID(item.itemID);

                if (targetSlot != null && targetSlot != this) {
                    if (targetSlot.item == null) {
                        targetSlot.AddItem(item, itemCount, true);
                        ClearSlot();
                    }
                    else {
                        // ���� �����۰� ��ȯ ���� Ÿ�� ������ ������ ������ ����
                        int existingItemCount = targetSlot.itemCount;
                        ItemController existingItem = targetSlot.item;

                        // ���� ������ �������� Ÿ�� ���Կ� �߰�
                        targetSlot.ClearSlot(); // Ÿ�� ������ �ʱ�ȭ�ϰ� ������ �߰�
                        targetSlot.AddItem(item, itemCount, true);

                        // ������ Ÿ�� ���Կ� �ִ� �������� ���� �������� �̵�
                        ClearSlot(); // ���� ���� �ʱ�ȭ
                        AddItem(existingItem, existingItemCount, true); // ������ ��ȯ
                    }
                }
                else {
                    Debug.Log("�ش� ������ �����ϴ�.");
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
        if (photonView.IsMine) {
            // ������ ������ 
            if (DragSlot.instance.transform.localPosition.x < baseRect.xMin
               || DragSlot.instance.transform.localPosition.x > baseRect.xMax
               || DragSlot.instance.transform.localPosition.y < baseRect.yMin
               || DragSlot.instance.transform.localPosition.y > baseRect.yMax) {

                // ������ ������ ����������� 
                if (DragSlot.instance.dragSlot != null && DragSlot.instance.dragSlot.item != null) {
                    string itemName = item.type.ToString();
                    // ������ ������ ����
                    GameObject itemObj = Pooling.instance.GetObject(itemName);
                    photonView.RPC("SetItemProperties", RpcTarget.AllBuffered, itemObj.GetComponent<PhotonView>().ViewID, itemCount);

                    itemObj.transform.position = gameObject.GetComponentInParent<Player>().bulletPos.position; // bullet ��ġ �ʱ�ȭ
                    itemObj.transform.rotation = Quaternion.identity; // bullet ȸ���� �ʱ�ȭ
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

    // �ش� ���Կ� ���𰡰� ���콺 ��� ���� �� �߻��ϴ� �̺�Ʈ
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
        return item != null; // item�� null�� �ƴϸ� ���Կ� �������� ����
    }
}