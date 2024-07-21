using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EliteRangeEnemyProjectile : MonoBehaviour
{
    public float damage = 8.0f;
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            PhotonNetwork.Instantiate("EliteRangeZombieDotArea", transform.position, Quaternion.identity);
        }
    }
}
