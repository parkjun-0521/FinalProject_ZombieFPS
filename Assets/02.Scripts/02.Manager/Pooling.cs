using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviourPun {
    public static Pooling instance;

    public GameObject[] prefabs;            // 생성될 오브젝트 
    private Dictionary<string, List<GameObject>> pools;

    PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }

        // pools 초기화 
        pools = new Dictionary<string, List<GameObject>>();
        foreach (GameObject prefab in prefabs) {
            pools[prefab.name] = new List<GameObject>();
        }
    }

    public GameObject GetObject(string key)
    {
        if (!pools.ContainsKey(key)) {
            return null;
        }

        GameObject select = null;

        foreach (GameObject obj in pools[key]) {
            if (obj != null && !obj.activeSelf) {
                PhotonView objPhotonView = obj.GetComponent<PhotonView>();
                if (objPhotonView == null) {
                    objPhotonView = obj.AddComponent<PhotonView>();
                }
                select = obj;
                photonView.RPC("ActivateObject", RpcTarget.All, objPhotonView.ViewID);
                break;
            }
        }

        if (select == null) {
            GameObject prefab = null;
            foreach (GameObject p in prefabs) {
                if (p.name == key) {
                    prefab = p;
                    break;
                }
            }

            if (prefab == null) {
                return null;
            }

            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                select = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            }
            else {
                select = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            pools[key].Add(select);
        }

        return select;
    }


    [PunRPC]
    public void ActivateObject(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null) {
            targetView.gameObject.SetActive(true);
        }
    }
}