using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOrActiveFalse : MonoBehaviour
{
    public bool isDestroy;
    public bool isActiveFalse;
    public float time;


    private void OnEnable()
    {
        Invoke("DesOrActive", time);
    }

    void DesOrActive()
    {
        if(isActiveFalse)
        {
            gameObject.SetActive(false);
        }
        else if (isDestroy)
        {
            Destroy(gameObject);
        }
    }
}
