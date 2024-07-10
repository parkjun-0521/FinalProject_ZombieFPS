using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class ItemController : ScriptableObject {
    public enum ItemType {
        Gun,                            // ��
        Sword,                          // Į
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
