using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooling : MonoBehaviourPun {
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
        if (index < 0 || index >= pools.Length) {
            Debug.LogError("Invalid index for pool");
            return null;
        }
        GameObject select = null;

        try {
            foreach (GameObject obj in pools[index]) {
                // 오브젝트가 비활성화 되었을 시 활성화
                if (obj != null && !obj.activeSelf) {
                    select = obj;
                    obj.SetActive(true);
                    break;
                }
            }

            // 오브젝트가 없을 때 오브젝트 생성 후 pools에 추가
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
