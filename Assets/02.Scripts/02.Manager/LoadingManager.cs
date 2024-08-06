using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public void SetActiveTrue() {
        gameObject.SetActive(true);
    }

    public void SetActiveFalse() {
        gameObject.SetActive(false);
    }
}
