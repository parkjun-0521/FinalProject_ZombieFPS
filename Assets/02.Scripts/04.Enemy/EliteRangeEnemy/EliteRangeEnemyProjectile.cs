using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EliteRangeEnemyProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [SerializeField] GameObject eliteEnemyRange;

    private void Start()
    {
        damage = eliteEnemyRange.GetComponent<EliteRangeEnemy>().damage;
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            PhotonNetwork.Instantiate("EliteRangeZombieDotArea", transform.position, Quaternion.identity);
        }
    }
}
