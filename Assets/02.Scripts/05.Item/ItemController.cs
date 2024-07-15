using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class ItemController : ScriptableObject {
    public enum ItemType {
        Gun,                            // ÃÑ 1
        ShotGun,                        // ÃÑ 2
        Sword1,                         // Ä® 1
        Sword2,                         // Ä® 2
        Healpack,                       // ÈúÆÑ 
        Grenade,                        // ¼ö·ùÅº
        FireGrenade,                    // È­¿°º´ 
        SupportFireGrenade,             // Áö¿ø»ç°İ ¼ö·ùÅº
        Magazine                        // ÅºÃ¢ 
    }
    public ItemType type;

    public int itemPrimaryID;
    public int itemID;
    public Sprite itemImage;            // ¾ÆÀÌÅÛ ÀÌ¹ÌÁö
    public string itemName;             // ¾ÆÀÌÅÛ ÀÌ¸§

    public float damage;                // ¾ÆÀÌÅÛ µ¥¹ÌÁö ( ¼ö·ùÅº : µ¥¹ÌÁö, ÈúÆÑ : Èú·® ) 
    public int itemCount;
    public bool isPickUp;
    public int totalCount;
}
