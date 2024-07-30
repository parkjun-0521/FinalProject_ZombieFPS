using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRangeEnemyDotArea : MonoBehaviour
{
    public float dotDelay;
    public float dotDamage;
    [SerializeField] float liveTime;

    
    private void OnEnable()
    {
        StartCoroutine(LiveTime(liveTime));
    }

    IEnumerator LiveTime(float liveTime)
    {
        
        float _time = 0;
        while (1 >= _time)
        {
            yield return null;
            _time += Time.deltaTime / liveTime;
        }
        Transform parent = transform.parent;
        parent.gameObject.SetActive(false);
    }
}
