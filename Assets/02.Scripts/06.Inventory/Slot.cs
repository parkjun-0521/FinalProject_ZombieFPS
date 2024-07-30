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
            ItemController.ItemType.Sword2 != _item.type ) {                 // �Ѱ� Į�� �������� �ʴ� ����� ������ if������ ���� ó��
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

        if (itemCount <= 0) {
            ClearSlot();
        }
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
                        UIManager.Instance.weaponItem[targetSlot.item.itemID - 1].color = new Color(1, 1, 1, 1);
                        if(targetSlot.item.itemID == 1)
                            WeaponSwap();
                        ClearSlot();
                    }
                    else {
                        // ���� �����۰� ��ȯ ���� Ÿ�� ������ ������ ������ ����
                        int existingItemCount = targetSlot.itemCount;
                        ItemController existingItem = targetSlot.item;

                        // ���� ������ �������� Ÿ�� ���Կ� �߰�
                        targetSlot.ClearSlot(); // Ÿ�� ������ �ʱ�ȭ�ϰ� ������ �߰�
                        targetSlot.AddItem(item, itemCount, true);

                        if (targetSlot.item.itemID == 1)
                            WeaponSwap();

                        // ������ Ÿ�� ���Կ� �ִ� �������� ���� �������� �̵�
                        ClearSlot(); // ���� ���� �ʱ�ȭ
                        AddItem(existingItem, existingItemCount, true); // ������ ��ȯ
                    }

                    inventory.UpdateTotalGrenadeCountFromUI(2);
                    inventory.UpdateTotalGrenadeCountFromUI(3);
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
            bool isHalf = false;

            if (DragSlot.instance.transform.localPosition.x < baseRect.xMin
               || DragSlot.instance.transform.localPosition.x > baseRect.xMax
               || DragSlot.instance.transform.localPosition.y < baseRect.yMin
               || DragSlot.instance.transform.localPosition.y > baseRect.yMax) {

                // ������ ������ ����������� 
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
                    // ������ ������ ����
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

    // �ش� ���Կ� ���𰡰� ���콺 ��� ���� �� �߻��ϴ� �̺�Ʈ
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
        return item != null; // item�� null�� �ƴϸ� ���Կ� �������� ����
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