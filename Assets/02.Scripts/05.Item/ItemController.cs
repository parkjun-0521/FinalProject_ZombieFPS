using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class ItemController : ScriptableObject {
    public enum ItemType {
        Gun,                            // 총 1
        ShotGun,                        // 총 2
        Sword1,                         // 칼 1
        Sword2,                         // 칼 2
        Healpack,                       // 힐팩 
        Grenade,                        // 수류탄
        FireGrenade,                    // 화염병 
        SupportFireGrenade,             // 지원사격 수류탄
        Magazine                        // 탄창 
    }
    public ItemType type;

    public int itemPrimaryID;           // 아이템 고유 ID 
    public int itemID;                  // 아이템 슬롯 ID와 일치 ( 자동 장착 ) 
    public Sprite itemImage;            // 아이템 이미지
    public string itemName;             // 아이템 이름

    public float damage;                // 아이템 데미지 ( 수류탄 : 데미지, 힐팩 : 힐량 ) 
    public int itemCount;               // 아이템 개수 
    public bool isPickUp;               // 아이템이 누군가 주운 아이템이지 상태 확인 
    public int totalCount;              // 아이템 전체 개수 
}
