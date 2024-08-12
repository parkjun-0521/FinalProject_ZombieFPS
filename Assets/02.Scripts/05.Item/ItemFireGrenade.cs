using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemFireGrenade : MonoBehaviour
{
   


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
            PhotonNetwork.Instantiate("FireGrenadeObjectDotArea", transform.position, Quaternion.identity);
        }
    }
}
