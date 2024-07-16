using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    Collider coll;
    public float activeTime;
    private void Awake()
    {
        coll = GetComponent<Collider>();
    }

    public void StartCorAtkColl(float time) 
    {
        StartCoroutine(AttackColliderActiveTrue(time));
    }
    IEnumerator AttackColliderActiveTrue(float _actimeTime)
    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(_actimeTime);
        gameObject.SetActive(false);
    }
}
