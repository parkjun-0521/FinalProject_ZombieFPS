using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour {

    [SerializeField]
    private GameObject go_InventoryBase;        // Inventory_Base 이미지
    [SerializeField]
    private GameObject go_SlotsParent;          // Slot들의 부모인 Grid Setting 
    [SerializeField]
    public GameObject go_MauntingSlotsParent;   // Slot들의 부모인 Grid Setting 

    public List<Slot> slots;                    // 슬롯들 배열
    public List<Slot> allSlots;                 // 모든 슬롯 

    void Start() {
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
            UIManager.Instance.UpdateTotalBulletCount(CalculateTotalBullets());
        }
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

    public void AcquireItem( ItemController _item, int _count = 1 ) {       // 초기값이 없으면 1로  
        if (ItemController.ItemType.Gun != _item.type     &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type  &&
            ItemController.ItemType.Sword2 != _item.type) {                 // 총과 칼은 합쳐지지 않는 무기기 때문에 if문으로 조건 처리

            if (_item.type == ItemController.ItemType.Magazine) {
                UIManager.Instance.UpdateTotalBulletCount(CalculateTotalBullets());
            }

            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].item != null && slots[i].slotID == 0)          // null 이라면 slots[i].item.itemName 할 때 런타임 에러 나서
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
    public bool IsFull() {
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
    public Slot FindSlotByID( int itemID ) {
        foreach (Slot slot in slots) {
            if (slot.slotID == itemID) {
                return slot;
            }
        }
        return null;
    }

    public int CalculateTotalBullets()
    {
        int totalBullets = 0;
        foreach (Slot slot in slots) {
            if (slot.item != null && slot.item.type == ItemController.ItemType.Magazine) {
                totalBullets += slot.itemCount;  // 각 슬롯의 아이템 개수를 합산
            }
        }
        return totalBullets;
    }
}
