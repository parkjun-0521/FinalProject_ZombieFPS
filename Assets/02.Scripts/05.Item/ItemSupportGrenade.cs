using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ItemSupportGrenade : MonoBehaviour
{
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] float minWidth;
    [SerializeField] float maxWidth;
    [SerializeField] int count;
    PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
            AirStrike();
        }
    }

    void AirStrike()
    {
        if (!pv.IsMine) return;
        for(int i = 0; i < count; i++)
        {
            Vector3 airPos = new Vector3(Random.Range(minWidth, maxWidth), Random.Range(minHeight, maxHeight), Random.Range(minWidth, maxWidth));
            //PhotonNetwork.Instantiate("Missile", transform.position + airPos, Quaternion.Euler(90,0,0));
            GameObject missile = Pooling.instance.GetObject("Missile", Vector3.zero); 
            missile.transform.position = transform.position + airPos;                          
            missile.transform.rotation = Quaternion.Euler(90, 0, 0);
            missile.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
