using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGizmo : MonoBehaviour
{
    public Color Mycolor = Color.red;

    public float Myraduis = 0.05f;

    private void OnDrawGizmos() {
        Gizmos.color = Mycolor;
        Gizmos.DrawWireSphere(transform.position, Myraduis);
        
    }
}
