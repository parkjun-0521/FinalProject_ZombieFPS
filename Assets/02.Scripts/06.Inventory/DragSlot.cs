using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    // 드래그 할 수 있는 슬롯 
    static public DragSlot instance;        // 싱글톤처럼 구현 
    public Slot dragSlot;                   // 드래그 슬롯 

    [SerializeField]
    private Image imageItem;                // 드래그 슬롯 이미지

    void Start() {
        instance = this;
    }

    // 드래그 되는 아이템의 이미지 및 색 조정 
    public void DragSetImage( Image _itemImage ) {
        imageItem.sprite = _itemImage.sprite;
        SetColor(1);
    }
    // 색 변경 ( 알파값 조절 ) 
    public void SetColor( float _alpha ) {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;
    }
}
