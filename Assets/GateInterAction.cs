using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateInterAction : MonoBehaviour
{
    public GameObject targetObject;
    // Start is called before the first frame update


    void DeactivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }






}
