using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviourPun {
    public static Pooling instance;                     // �̱��� ����

    public GameObject[] prefabs;                        // ������ ������Ʈ 
    private Dictionary<string, List<GameObject>> pools; // ������Ʈ�� ã�� ��ųʸ� ����

    void Awake()
    {
        // �̱��� ���� 
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }

        // pools �ʱ�ȭ 
        pools = new Dictionary<string, List<GameObject>>();
        foreach (GameObject prefab in prefabs) {
            pools[prefab.name] = new List<GameObject>();
        }
    }

    public GameObject GetObject(string key)
    {
        // ��ųʸ��� key�� �ִ��� Ȯ�� ������ return;
        if (!pools.ContainsKey(key)) {
            return null;
        }

        GameObject select = null;

        // ������ ���� 
        foreach (GameObject obj in pools[key]) {
            if (obj != null && !obj.activeSelf) {
                // ������Ʈ�� ���� �߰� 
                PhotonView objPhotonView = obj.GetComponent<PhotonView>();
                if (objPhotonView == null) {
                    objPhotonView = obj.AddComponent<PhotonView>();
                }
                // ������ ������Ʈ ��ȯ
                select = obj;
                // ������Ʈ ���� ����ȭ 
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                    photonView.RPC("ActivateObject", RpcTarget.All, objPhotonView.ViewID);
                else
                    select.gameObject.SetActive(true);
                break;
            }
        }

        // ���ο� ������Ʈ ����
        if (select == null) {
            GameObject prefab = null;
            foreach (GameObject p in prefabs) {
                if (p.name == key) {
                    prefab = p;
                    break;
                }
            }

            if (prefab == null) return null;

            // ���ο� ������ ������ ���� 
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                select = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            }
            else {  // �׽�Ʈ��
                select = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            // Ǯ�� ������ �߰� 
            pools[key].Add(select);
        }

        return select;
    }

    // ������ ���� ����ȭ 
    [PunRPC]
    public void ActivateObject(int viewID) {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
            targetView.gameObject.SetActive(true);
    }
}