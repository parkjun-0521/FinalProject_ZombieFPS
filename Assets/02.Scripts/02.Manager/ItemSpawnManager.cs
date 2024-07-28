using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnManager : MonoBehaviourPun
{
    public GameObject[] itemSpawns;         
    public GameObject[] items;          // ¼ÒÃÑ, ¼¦°Ç, Ä®1, Ä®2, ¼ö·ùÅº, È­¿°º´, Áö¿ø¼ö·ùÅº, ÈúÆÑ, ÅºÃ¢, ¼¦°ÇÅºÃ¢
    public bool isSpawn;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isSpawn) {
            isSpawn = true;
            photonView.RPC("SpawnEnemies", RpcTarget.AllBuffered, isSpawn);
            ItemSpawnCount[] itemCount = GetComponentsInChildren<ItemSpawnCount>();
            for (int i = 0; i < itemSpawns.Length; i++) {
                for (int j = 0; j < itemCount[i].spawnCount; j++) {
                    int itemIndex = GetRandomItemIndex(itemCount[i].persent);
                    if (itemIndex != -1) {
                        Pooling.instance.GetObject(items[itemIndex].name, itemSpawns[i].transform.position);
                    }
                }
            }
        }
    }

    private int GetRandomItemIndex(int[] persent)
    {
        int total = 0;
        foreach (int perc in persent) {
            total += perc;
        }

        int randomPoint = Random.Range(0, total);

        for (int i = 0; i < persent.Length; i++) {
            if (randomPoint < persent[i]) {
                return i;
            }
            else {
                randomPoint -= persent[i];
            }
        }

        return -1; // ¿À·ù ¹ß»ý ½Ã
    }

    [PunRPC]
    void SpawnEnemies(bool isSpawn)
    {
        this.isSpawn = isSpawn;
    }
}
