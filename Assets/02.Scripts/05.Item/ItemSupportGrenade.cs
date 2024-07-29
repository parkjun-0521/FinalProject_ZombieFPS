using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSupportGrenade : MonoBehaviour
{
    public ItemController itemData;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            //PhotonNetwork.Instantiate("ЦјАн", transform.position, Quaternion.identity);
        }
    }
}
