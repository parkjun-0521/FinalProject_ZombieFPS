using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : ItemSword
{
    [SerializeField] float liveTime = 5.0f;
    [SerializeField] GameObject missile;
    [SerializeField] GameObject[] tails;
    [SerializeField] GameObject explosionParticle;
    [SerializeField] GameObject range;
     
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        missile.SetActive(true);
        foreach (GameObject tail in tails)
        {
            tail.SetActive(true);
        }

        explosionParticle.SetActive(false);
        range.SetActive(false);
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
            range.SetActive(true);
        }
    }

    IEnumerator LiveTime(float _liveTime)
    {

        float _time = 0;
        while (1 >= _time)
        {
            yield return null;
            if(range.activeSelf)
            {
                range.SetActive(false);
            }
            _time += Time.deltaTime / _liveTime;
        }
        gameObject.SetActive(false);
        
        

        
    }


}
