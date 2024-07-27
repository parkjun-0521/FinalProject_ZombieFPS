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

    // 좀비 이름 
    public string[] enemyName = { "Zombie1", "EliteMeleeZombie", "EliteRangeZombie", "BossZombie", "Boss_Phobos" };
    public int[] persent;           // 각 좀비의 등장 확률 
        
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isSpawn)
        {
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

                enemyObj.transform.rotation = transform.rotation;
                EnemyController enemyLogin = enemyObj.GetComponent<EnemyController>();
                enemyLogin.enemySpawn = spawnPoint;
            }
        }
    }


    [PunRPC]
    void SpawnEnemies(bool isSpawn) {
        this.isSpawn = isSpawn;     
    }
}

 


