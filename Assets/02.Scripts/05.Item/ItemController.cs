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
    public int itemID;                  // 아이템 ID
    public float damege;                // 아이템 데미지 ( 수류탄 : 데미지, 힐팩 : 힐량 ) 

    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public new Collider collider;       // 각 아이템의 콜라이더 
}
