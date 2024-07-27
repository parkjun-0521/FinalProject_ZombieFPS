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
    public int spawnCount;          // �����Ǵ� ���� ���� 
    GameObject enemyObj;            // ���� ������Ʈ 

    // ���� �̸� 
    public string[] enemyName = { "Zombie1", "EliteMeleeZombie", "EliteRangeZombie", "BossZombie", "Boss_Phobos" };
    public int[] persent;           // �� ������ ���� Ȯ�� 
        
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isSpawn)
        {
            isSpawn = true;
            photonView.RPC("SpawnEnemies", RpcTarget.AllBuffered, isSpawn);

            // ���� ��ŭ ���� ���� 
            for (int i = 0; i < spawnCount; i++) {
                int randomIndex = Random.Range(0, 100);
                int cumulativePercentage = 0;

                // ���� ����Ȯ�� 
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

 


