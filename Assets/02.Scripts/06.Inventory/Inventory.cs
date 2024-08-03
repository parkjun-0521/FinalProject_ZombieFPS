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
    private GameObject go_InventoryBase;        // Inventory_Base 이미지
    [SerializeField]
    public GameObject go_SlotsParent;          // Slot들의 부모인 Grid Setting 
    [SerializeField]
    public GameObject go_MauntingSlotsParent;   // Slot들의 부모인 Grid Setting 

    public List<Slot> slots;                    // 슬롯들 배열
    public List<Slot> allSlots;                 // 모든 슬롯 

    void Start()
    {
        // slots를 List로 초기화
        slots = new List<Slot>();
        slots.AddRange(go_SlotsParent.GetComponentsInChildren<Slot>());             // 인벤토리 슬롯 추가 
        slots.AddRange(go_MauntingSlotsParent.GetComponentsInChildren<Slot>());     // 장비 장착칸 슬롯을 인벤토리 칸에 추가 ( 아이템 자동 장착을 위해 ) 
        allSlots = new List<Slot>(GetComponentsInChildren<Slot>(true));             // 모든 슬롯 
    }

    // 아이템 사용시 개수 감소 
    public void DecreaseMagazineCount(ItemController.ItemType itemType)
    {
        // 모든 슬롯을 순회 
        foreach (Slot slot in allSlots) {
            // 사용한 아이템의 Type을 찾아 개수 감소 
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

    // 아이템이 있는지 
    public bool HasItemUse(ItemController.ItemType itemType)
    {
        // 모든 슬롯을 순회
        foreach (Slot slot in allSlots) {
            // 아이템이 하나라도 있으면 true 반환
            if (slot.item != null && slot.item.type == itemType && slot.itemCount > 0) {
                return true;
            }
        }
        // 모든 슬롯을 검사했지만 아이템이 없는 경우
        return false;
    }

    public void AcquireItem(ItemController _item, int _count = 1)
    {       // 초기값이 없으면 1로  
        if (ItemController.ItemType.Gun != _item.type &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type &&
            ItemController.ItemType.Sword2 != _item.type) {                 // 총과 칼은 합쳐지지 않는 무기기 때문에 if문으로 조건 처리

            /*if (_item.type == ItemController.ItemType.Magazine) {
                UIManager.Instance.UpdateTotalBulletCount(CalculateTotalItems(ItemController.ItemType.Magazine));
            }*/

            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].item != null)          // null 이라면 slots[i].item.itemName 할 때 런타임 에러 나서
                {
                    if (slots[i].item.itemName == _item.itemName) {
                        if (!_item.isPickUp) {
                            if (ItemController.ItemType.Magazine == _item.type)
                                _count = _item.itemCount;                   // 총알 30발 
                            else
                                _count = _item.itemCount;                   // 수류탄 1개 
                        }
                        else {
                            if (ItemController.ItemType.Magazine == _item.type)
                                _count = _item.totalCount;                  // 총알 30발 
                            else
                                _count = _item.totalCount;                  // 수류탄 1개
                        }
                        slots[i].SetSlotCount(_count);                      // 아이템 개수 슬롯 업데이트

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
                        _count = _item.itemCount;    // 총알은 30발 
                    else
                        _count = _item.itemCount;
                }
                else {
                    if (ItemController.ItemType.Magazine == _item.type)
                        _count = _item.totalCount;    // 총알은 30발 
                    else
                        _count = _item.totalCount;
                }
                slots[i].AddItem(_item, _count, false);
                return;
            }
        }
    }

    // 아이템이 가득 찼는지 확인 
    public bool IsFull()
    {
        // 장비 장착칸 4칸을 제외하고 모든 슬롯을 순회
        for (int i = 0; i < slots.Count - 4; i++) {
            Slot slot = slots[i];
            // slot이 비어있으면 인벤토리가 가득 차지 않음
            if (!slot.HasItem())
                return false;
        }
        return true;
    }

    // 아이템의 ID값 찾아오기 
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
                totalItemCount += slot.itemCount;  // 각 슬롯의 아이템 개수를 합산
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
            // 슬롯의 부모 오브젝트를 임시로 활성화
            GameObject parent = slot.gameObject;
            bool wasActive = parent.activeSelf;
            parent.SetActive(true);

            // 데이터를 가져옴
            if (slot.item != null) {
                Debug.Log(slot.item.itemName + " " + slot.item.itemCount);
                string userID = PhotonNetwork.NickName;
                StartCoroutine(SendItemData(userID, slot.item.itemName, slot.item.itemCount));
            }
            else {
                Debug.Log("Item is null");
            }

            // 다시 원래 상태로 설정
            parent.SetActive(wasActive);
        }
    }
    // 아이템 데이터를 서버로 전송
    IEnumerator SendItemData(string userID, string itemName, int itemCount)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserID", userID); // UserID 추가
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
