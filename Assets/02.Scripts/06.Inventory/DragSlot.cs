using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    // �巡�� �� �� �ִ� ���� 
    static public DragSlot instance;        // �̱���ó�� ���� 
    public Slot dragSlot;                   // �巡�� ���� 

    [SerializeField]
    private Image imageItem;                // �巡�� ���� �̹���

    void Start() {
        instance = this;
    }

    // �巡�� �Ǵ� �������� �̹��� �� �� ���� 
    public void DragSetImage( Image _itemImage ) {
        imageItem.sprite = _itemImage.sprite;
        SetColor(1);
    }
    // �� ���� ( ���İ� ���� ) 
    public void SetColor( float _alpha ) {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;
    }
}
