using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ItemController itemData;

    void OnEnable() {
        // 2�� �ڿ� DeactivateBullet �޼��� ȣ��
        Invoke("DeactivateBullet", 2f);
    }

    void DeactivateBullet() {
        gameObject.SetActive(false);
    }
}
