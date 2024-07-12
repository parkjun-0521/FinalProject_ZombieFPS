using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour {

    [SerializeField]
    private GameObject go_InventoryBase; // Inventory_Base 이미지
    [SerializeField]
    private GameObject go_SlotsParent;  // Slot들의 부모인 Grid Setting 
    [SerializeField]
    private GameObject go_MauntingSlotsParent;  // Slot들의 부모인 Grid Setting 

    public List<Slot> slots;            // 슬롯들 배열

    void Start() {
        slots = new List<Slot>();       // slots를 List로 초기화
        slots.AddRange(go_SlotsParent.GetComponentsInChildren<Slot>());
        slots.AddRange(go_MauntingSlotsParent.GetComponentsInChildren<Slot>());
    }

    public void AcquireItem( ItemController _item, int _count = 1 ) {
        if (ItemController.ItemType.Gun != _item.type &&
            ItemController.ItemType.ShotGun != _item.type &&
            ItemController.ItemType.Sword1 != _item.type &&
            ItemController.ItemType.Sword2 != _item.type) {
            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].item != null && slots[i].slotID == 0)  // null 이라면 slots[i].item.itemName 할 때 런타임 에러 나서
                {
                    if (slots[i].item.itemName == _item.itemName) {
                        slots[i].SetSlotCount(_count);
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].item == null && slots[i].slotID == 0) {
                slots[i].AddItem(_item, _count);
                return;
            }
        }
    }

    public bool IsFull() {
        for (int i = 0; i < slots.Count - 4; i++) {
            Slot slot = slots[i];
            if (!slot.HasItem()) // slot이 비어있으면 인벤토리가 가득 차지 않음
                return false;
        }
        return true;
    }

    public Slot FindSlotByID( int itemID ) {
        foreach (Slot slot in slots) {
            if (slot.slotID == itemID) {
                return slot;
            }
        }
        return null;
    }
}
