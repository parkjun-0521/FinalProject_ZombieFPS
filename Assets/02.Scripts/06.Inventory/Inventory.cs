using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static ItemController;
public class Inventory : MonoBehaviour {

    [SerializeField]
    private GameObject go_InventoryBase;        // Inventory_Base �̹���
    [SerializeField]
    public GameObject go_SlotsParent;          // Slot���� �θ��� Grid Setting 
    [SerializeField]
    public GameObject go_MauntingSlotsParent;   // Slot���� �θ��� Grid Setting 

    public List<Slot> slots;                    // ���Ե� �迭
    public List<Slot> allSlots;                 // ��� ���� 

    void Start()
    {
        // slots�� List�� �ʱ�ȭ
        slots = new List<Slot>();
        slots.AddRange(go_SlotsParent.GetComponentsInChildren<Slot>());             // �κ��丮 ���� �߰� 
        slots.AddRange(go_MauntingSlotsParent.GetComponentsInChildren<Slot>());     // ��� ����ĭ ������ �κ��丮 ĭ�� �߰� ( ������ �ڵ� ������ ���� ) 
        allSlots = new List<Slot>(GetComponentsInChildren<Slot>(true));             // ��� ���� 
    }

    // ������ ���� ���� ���� 
    public void DecreaseMagazineCount(ItemController.ItemType itemType)
    {
        // ��� ������ ��ȸ 
        foreach (Slot slot in allSlots) {
            // ����� �������� Type�� ã�� ���� ���� 
            if (slot.item != null && slot.item.type == itemType) {
                slot.SetSlotCount(-1);
            }
        }

        if (itemType == ItemController.ItemType.Magazine) {
            UIManager.Instance.UpdateTotalBulletCount(CalculateTotalItems(ItemController.ItemType.Magazine));
        }
        else if (itemType == ItemController.ItemType.ShotMagazine) {
            UIManager.Instance.UpdateTotalBulletCount(CalculateTotalItems(ItemController.ItemType.ShotMagazine));
        }

        UpdateTotalGrenadeCountFromUI(2);
        UpdateTotalGrenadeCountFromUI(3);
    }

    // �������� �ִ��� 
    public bool HasItemUse(ItemController.ItemType itemType)
    {
        // ��� ������ ��ȸ
        foreach (Slot slot in allSlots) {
            // �������� �ϳ��� ������ true ��ȯ
            if (slot.item != null && slot.item.type == itemType && slot.itemCount > 0) {
                return true;
            }
        }
        // ��� ������ �˻������� �������� ���� ���
        return false;
    }

    public void AcquireItem(ItemController _item, int _count = 1)
    {       // �ʱⰪ�� ������ 1��  
        if (ItemController.ItemType.Gun != _item.type &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type &&
            ItemController.ItemType.Sword2 != _item.type) {                 // �Ѱ� Į�� �������� �ʴ� ����� ������ if������ ���� ó��

            /*if (_item.type == ItemController.ItemType.Magazine) {
                UIManager.Instance.UpdateTotalBulletCount(CalculateTotalItems(ItemController.ItemType.Magazine));
            }*/

            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].item != null)          // null �̶�� slots[i].item.itemName �� �� ��Ÿ�� ���� ����
                {
                    if (slots[i].item.itemName == _item.itemName) {
                        if (!_item.isPickUp) {
                            if (ItemController.ItemType.Magazine == _item.type)
                                _count = _item.itemCount;                   // �Ѿ� 30�� 
                            else
                                _count = _item.itemCount;                   // ����ź 1�� 
                        }
                        else {
                            if (ItemController.ItemType.Magazine == _item.type)
                                _count = _item.totalCount;                  // �Ѿ� 30�� 
                            else
                                _count = _item.totalCount;                  // ����ź 1��
                        }
                        slots[i].SetSlotCount(_count);                      // ������ ���� ���� ������Ʈ

                        UpdateTotalGrenadeCountFromUI(2);
                        UpdateTotalGrenadeCountFromUI(3);
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].item == null && slots[i].slotID == 0) {
                if (!_item.isPickUp) {
                    if (ItemController.ItemType.Magazine == _item.type)
                        _count = _item.itemCount;    // �Ѿ��� 30�� 
                    else
                        _count = _item.itemCount;
                }
                else {
                    if (ItemController.ItemType.Magazine == _item.type)
                        _count = _item.totalCount;    // �Ѿ��� 30�� 
                    else
                        _count = _item.totalCount;
                }
                slots[i].AddItem(_item, _count, false);
                return;
            }
        }
    }

    // �������� ���� á���� Ȯ�� 
    public bool IsFull()
    {
        // ��� ����ĭ 4ĭ�� �����ϰ� ��� ������ ��ȸ
        for (int i = 0; i < slots.Count - 4; i++) {
            Slot slot = slots[i];
            // slot�� ��������� �κ��丮�� ���� ���� ����
            if (!slot.HasItem())
                return false;
        }
        return true;
    }

    // �������� ID�� ã�ƿ��� 
    public Slot FindSlotByID(int itemID)
    {
        foreach (Slot slot in slots) {
            if (slot.slotID == itemID) {
                return slot;
            }
        }
        return null;
    }

    public int CalculateTotalItems(ItemController.ItemType ItemType)
    {
        int totalItemCount = 0;
        foreach (Slot slot in slots) {
            if (slot.item != null && slot.item.type == ItemType) {
                totalItemCount += slot.itemCount;  // �� ������ ������ ������ �ջ�
            }
        }
        return totalItemCount;
    }

    public void UpdateTotalGrenadeCountFromUI(int index)
    {
        Transform slotTransform = go_MauntingSlotsParent.transform.GetChild(index);
        Text slotText = slotTransform.GetComponentInChildren<Text>();

        if (slotText != null) {
            int grenadeCount = int.Parse(slotText.text);
            if (index == 2) UIManager.Instance.UpdateTotalGrenadeCount(grenadeCount);
            else if (index == 3) UIManager.Instance.UpdateTotalHealCount(grenadeCount);
        }
        else {
            if (index == 2) UIManager.Instance.UpdateTotalGrenadeCount(0);
            else if (index == 3) UIManager.Instance.UpdateTotalHealCount(0);
        }
    }

    public void AllItemInfo()
    {
        foreach (Slot slot in allSlots) {
            // ������ �θ� ������Ʈ�� �ӽ÷� Ȱ��ȭ
            GameObject parent = slot.gameObject;
            bool wasActive = parent.activeSelf;
            parent.SetActive(true);

            // �����͸� ������
            if (slot.item != null) {
                Debug.Log(slot.item.itemName + " " + slot.item.itemCount);
                string userID = PhotonNetwork.NickName;
                StartCoroutine(SendItemData(userID, slot.item.itemName, slot.item.itemCount));
            }
            else {
                Debug.Log("Item is null");
            }

            // �ٽ� ���� ���·� ����
            parent.SetActive(wasActive);
        }
    }
    // ������ �����͸� ������ ����
    IEnumerator SendItemData(string userID, string itemName, int itemCount)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID); // UserID �߰�
        form.AddField("ItemName", itemName);
        form.AddField("ItemCount", itemCount);

        using (UnityWebRequest www = UnityWebRequest.Post(URLs.ItemSaveURL, form)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Error while sending item data: " + www.error);
            }
            else {
                Debug.Log("Item data sent successfully!");
            }
        }
    }
}
