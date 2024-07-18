using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ItemController itemData;

    void OnEnable() {
        // 2초 뒤에 DeactivateBullet 메서드 호출
        Invoke("DeactivateBullet", 2f);
    }

    void DeactivateBullet() {
        gameObject.SetActive(false);
    }
}
