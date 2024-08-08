using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : ItemSword
{
    [SerializeField] float liveTime = 5.0f;
    [SerializeField] GameObject missile;
    [SerializeField] GameObject[] tails;
    [SerializeField] GameObject explosionParticle;
    [SerializeField] CapsuleCollider rangeColl;
     
    private void OnEnable()
    {
        tails[1].GetComponent<ParticleSystem>().Clear();
    }

    private void OnDisable()
    {
        missile.SetActive(true);
        foreach (GameObject tail in tails)
        {
            tail.SetActive(true);
        }

        explosionParticle.SetActive(false);
        rangeColl.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            missile.SetActive(false);
            foreach(GameObject tail in tails)
            {
                tail.SetActive(false);
            }
            explosionParticle.SetActive(true);
            StartCoroutine(LiveTime(liveTime));
            rangeColl.enabled = true;
        }
    }

    IEnumerator LiveTime(float _liveTime)
    {

        float _time = 0;
        yield return new WaitForSeconds(0.2f);
        if (rangeColl.enabled)
        {
            rangeColl.enabled = false;
        }

        while (1 >= _time)
        {
            
            _time += Time.deltaTime / _liveTime;
        }
        gameObject.SetActive(false);
   
    }

}
