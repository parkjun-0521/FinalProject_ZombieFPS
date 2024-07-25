using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviourPun
{
    public Transform spawnPoint;
    public bool isSpawn;
    public int spawnCount;

    string[] enemyName = { "Zombie1", "EliteMeleeZombie", "EliteRangeZombie", "BossZombie", "Boss_Phobos" };

    private void OnTriggerEnter( Collider other ) {
        if (other.gameObject.CompareTag("Player") && !isSpawn) {
            isSpawn = true;
            photonView.RPC("SpawnEnemies", RpcTarget.AllBuffered, isSpawn);
            for (int i = 0; i < spawnCount; i++) {
                int randomIndex = Random.Range(0, enemyName.Length - 2);
                GameObject enemyObj = Pooling.instance.GetObject(enemyName[randomIndex], spawnPoint.position);
                enemyObj.transform.position = new Vector3(Random.Range(-1f, 1f), transform.position.y, Random.Range(-1f, 1f));
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
