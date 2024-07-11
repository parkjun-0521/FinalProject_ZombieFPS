using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    [SerializeField]
    private GameObject go_InventoryBase; // Inventory_Base �̹���
    [SerializeField]
    private GameObject go_SlotsParent;  // Slot���� �θ��� Grid Setting 

    private Slot[] slots;  // ���Ե� �迭

    void Start() {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>();
    }

    public void AcquireItem( ItemController _item, int _count = 1 ) {
        if (ItemController.ItemType.Gun != _item.type       &&
            ItemController.ItemType.ShotGun != _item.type   &&
            ItemController.ItemType.Sword1 != _item.type    &&
            ItemController.ItemType.Sword2 != _item.type ) {
            for (int i = 0; i < slots.Length; i++) {
                if (slots[i].item != null)  // null �̶�� slots[i].item.itemName �� �� ��Ÿ�� ���� ����
                {
                    if (slots[i].item.itemName == _item.itemName ) {
                        slots[i].SetSlotCount(_count);
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].item == null) {
                slots[i].AddItem(_item, _count);
                return;
            }
        }
    }
}
