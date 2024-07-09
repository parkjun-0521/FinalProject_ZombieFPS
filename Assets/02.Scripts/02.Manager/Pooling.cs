using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviour {
    public static Pooling instance;

    public GameObject[] prefabs;            // ������ ������Ʈ 
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
        // pools �ʱ�ȭ 
        pools = new List<GameObject>[prefabs.Length];
        for (int index = 0; index < prefabs.Length; index++) {
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject GetObject(int index)
    {
        GameObject select = null;

        foreach (GameObject objects in pools[index]) {
            // ������Ʈ�� ��Ȱ��ȭ �Ǿ��� �� Ȱ��ȭ
            if (!objects.activeSelf) {
                select = objects;
                objects.SetActive(true);
                break;
            }
        }

        // ������Ʈ�� ���� �� ������Ʈ ���� �� pools�� �߰� 
        if (!select) {
            select = Instantiate(prefabs[index], transform);

            pools[index].Add(select);
        }

        return select;
    }
}
