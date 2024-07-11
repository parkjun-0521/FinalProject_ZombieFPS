using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class ItemController : ScriptableObject {
    public enum ItemType {
        Gun,                            // �� 1
        ShotGun,                        // �� 2
        Sword1,                         // Į 1
        Sword2,                         // Į 2
        Healpack,                       // ���� 
        Grenade,                        // ����ź
        FireGrenade,                    // ȭ���� 
        SupportFireGrenade,             // ������� ����ź
        Magazine                        // źâ 
    }
    public ItemType type;

    public Sprite itemImage;            // ������ �̹���
    public string itemName;             // ������ �̸�
    public float damege;                // ������ ������ ( ����ź : ������, ���� : ���� ) 
}
