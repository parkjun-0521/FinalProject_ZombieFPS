using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public enum ItemType {
        Gun,
        Sword,
        Healpack,
        Grenade
    }

    public ItemType type;
    public int itemID;                  // ������ ID
    public float damege;                // ������ ������ ( ����ź : ������, ���� : ���� ) 

    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public new Collider collider;       // �� �������� �ݶ��̴� 
}
