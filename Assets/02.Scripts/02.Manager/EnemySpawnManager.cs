using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviourPun
{
    public Transform spawnPoint;
    public bool isSpawn;
    public int spawnCount;          // 생성되는 좀비 숫자 
    GameObject enemyObj;            // 좀비 오브젝트 
    public bool isInfinite;         // 무한 생성 트리거 
    public float spawnInterval = 1f; // 좀비 생성 간격 시간 (초 단위)

    // 좀비 이름 
    public string[] enemyName = { "Zombie1", "EliteMeleeZombie", "EliteRangeZombie", "BossZombie", "Boss_Phobos" };
    public int[] persent;           // 각 좀비의 등장 확률 

    private void OnTriggerEnter( Collider other ) {
        if (other.gameObject.CompareTag("Player") && !isSpawn) {
            if (!isInfinite) {
                EnemySpawn();
            }
            else {
                StartCoroutine(InfiniteEnemySpawn());
            }
        }
    }

    IEnumerator InfiniteEnemySpawn() {
        while (true) {
            EnemySpawn();
            yield return new WaitForSeconds(spawnInterval); // 지정된 시간 간격으로 대기
        }
    }



    public void EnemySpawn() {
        isSpawn = true;
        photonView.RPC("SpawnEnemies", RpcTarget.AllBuffered, isSpawn);

        // 개수 만큼 좀비 생성 
        for (int i = 0; i < spawnCount; i++) {
            int randomIndex = Random.Range(0, 100);
            int cumulativePercentage = 0;

            // 좀비 등장확률 
            for (int j = 0; j < enemyName.Length; j++) {
                cumulativePercentage += persent[j];
                if (randomIndex < cumulativePercentage) {
                    enemyObj = Pooling.instance.GetObject(enemyName[j], spawnPoint.position);
                    break;
                }
            }
            if (enemyObj.transform != null)
                enemyObj.transform.rotation = transform.rotation;


            EnemyController enemyLogin = enemyObj.GetComponent<EnemyController>();
            enemyLogin.enemySpawn = spawnPoint;
            if (isInfinite) {
                enemyLogin.EnemyLookRange.radius = 100;
            }
        }
    }

    [PunRPC]
    void SpawnEnemies(bool isSpawn) {
        this.isSpawn = isSpawn;     
    }
}

 


