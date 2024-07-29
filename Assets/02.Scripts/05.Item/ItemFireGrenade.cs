using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemFireGrenade : MonoBehaviour
{
    public ItemController itemData;

    void Awake()
    {
    }

    void OnEnable()
    {

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            //PhotonNetwork.Instantiate("È­¿°Áö¿ª", transform.position, Quaternion.identity);
        }
    }
}
