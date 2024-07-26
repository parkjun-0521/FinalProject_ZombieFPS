using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRectTransform
{
    private RectTransform rectTransform;

    // ������: RectTransform�� �޾Ƽ� MyRectTransform�� �ʱ�ȭ�մϴ�.
    public MyRectTransform(RectTransform rt)
    {
        rectTransform = rt;
    }

    // Left: offsetMin.x�� ����
    public float Left
    {
        get { return rectTransform.offsetMin.x; }
        set
        {
            Vector2 offset = rectTransform.offsetMin;
            offset.x = value;
            rectTransform.offsetMin = offset;
        }
    }

    // Right: offsetMax.x�� ����
    public float Right
    {
        get { return -rectTransform.offsetMax.x; }
        set
        {
            Vector2 offset = rectTransform.offsetMax;
            offset.x = -value;
            rectTransform.offsetMax = offset;
        }
    }

    // Top: offsetMax.y�� ����
    public float Top
    {
        get { return -rectTransform.offsetMax.y; }
        set
        {
            Vector2 offset = rectTransform.offsetMax;
            offset.y = -value;
            rectTransform.offsetMax = offset;
        }
    }

    // Bottom: offsetMin.y�� ����
    public float Bottom
    {
        get { return rectTransform.offsetMin.y; }
        set
        {
            Vector2 offset = rectTransform.offsetMin;
            offset.y = value;
            rectTransform.offsetMin = offset;
        }
    }
}
public class PlayerMiniMap : MonoBehaviour
{
    public GameObject MiniMap;
    public RectTransform minimap;
    Player player;

    Vector3 prevPlayerPos;
    Vector3 playerPos;
    Vector3 PlayerPos
    {
        get
        {
            return playerPos;
        }
        set
        {
            prevPlayerPos = value;
            playerPos += value;
           // Debug.Log(value.magnitude);
        }
    }

    
    private void Awake()
    {
        
    }
    private void Start()
    {
        MyRectTransform myRect = new MyRectTransform(minimap);
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        
    }
    private void Update()
    {
        if (player.PV.IsMine)
        {
            PlayerPos = player.transform.position;

            Debug.Log("�̴ϸʷ���������" + minimap.localPosition);
            Debug.Log("�̴ϸ�tran������" + minimap.transform.position);
            Debug.Log("�̴ϸ�������" + minimap.position);
        }
    }
}
