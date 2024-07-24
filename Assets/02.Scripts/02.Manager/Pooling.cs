using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviourPun {
    public static Pooling instance;                     // 싱글톤 생성

    public GameObject[] prefabs;                        // 생성될 오브젝트 
    private Dictionary<string, List<GameObject>> pools; // 오브젝트를 찾을 딕셔너리 생성

    void Awake()
    {
        // 싱글톤 생성 
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
        // 딕셔너리에 key가 있는지 확인 없으면 return;
        if (!pools.ContainsKey(key)) {
            return null;
        }

        GameObject select = null;

        // 아이템 생성 
        foreach (GameObject obj in pools[key]) {
            if (obj != null && !obj.activeSelf) {
                // 오브젝트에 포톤 추가 
                PhotonView objPhotonView = obj.GetComponent<PhotonView>();
                if (objPhotonView == null) {
                    objPhotonView = obj.AddComponent<PhotonView>();
                }
                // 생성된 오브젝트 반환
                select = obj;
                // 오브젝트 생성 동기화 
                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                    photonView.RPC("ActivateObject", RpcTarget.All, objPhotonView.ViewID);
                else
                    select.gameObject.SetActive(true);
                break;
            }
        }

        // 새로운 오브젝트 생성
        if (select == null) {
            GameObject prefab = null;
            foreach (GameObject p in prefabs) {
                if (p.name == key) {
                    prefab = p;
                    break;
                }
            }

            if (prefab == null) return null;

            // 새로운 아이템 프리팹 생성 
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom) {
                select = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            }
            else {  // 테스트용
                select = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            // 풀에 아이템 추가 
            pools[key].Add(select);
        }

        return select;
    }

    // 아이템 생성 동기화 
    [PunRPC]
    public void ActivateObject(int viewID) {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
            targetView.gameObject.SetActive(true);
    }
}