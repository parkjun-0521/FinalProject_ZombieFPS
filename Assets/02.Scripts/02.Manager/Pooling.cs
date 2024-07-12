using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviourPun {
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
        if (index < 0 || index >= pools.Length) {
            Debug.LogError("Invalid index for pool");
            return null;
        }
        GameObject select = null;

        try {
            foreach (GameObject obj in pools[index]) {
                // ������Ʈ�� ��Ȱ��ȭ �Ǿ��� �� Ȱ��ȭ
                if (obj != null && !obj.activeSelf) {
                    select = obj;
                    obj.SetActive(true);
                    break;
                }
            }

            // ������Ʈ�� ���� �� ������Ʈ ���� �� pools�� �߰�
            if (select == null) {
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                    select = PhotonNetwork.Instantiate(prefabs[index].name, Vector3.zero, Quaternion.identity);
                }
                else {
                    select = Instantiate(prefabs[index], Vector3.zero, Quaternion.identity);
                }
                pools[index].Add(select);
            }
        }
        catch (System.Exception ex) {
            Debug.LogError("Error in GetObject: " + ex.Message);
        }

        return select;
    }
}
