using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviour {
    public static Pooling instance;

    public GameObject[] prefabs;            // 생성될 오브젝트 
    List<GameObject>[] pools;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        // pools 초기화 
        pools = new List<GameObject>[prefabs.Length];
        for (int index = 0; index < prefabs.Length; index++) {
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject GetObject(int index)
    {
        GameObject select = null;

        foreach (GameObject objects in pools[index]) {
            // 오브젝트가 비활성화 되었을 시 활성화
            if (!objects.activeSelf) {
                select = objects;
                objects.SetActive(true);
                break;
            }
        }

        // 오브젝트가 없을 떄 오브젝트 생성 후 pools에 추가 
        if (!select) {
            select = Instantiate(prefabs[index], transform);

            pools[index].Add(select);
        }

        return select;
    }
}
