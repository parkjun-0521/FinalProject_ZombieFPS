using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItemShader : MonoBehaviour
{
    [HideInInspector]
    public Material visible;

    Renderer renderers;
    List<Material> materialList = new List<Material>();

    void Start() {
        Shader visibleShader = Shader.Find("lwsoft/phantom");
        visible = new Material(visibleShader);
        renderers = GetComponent<Renderer>();
    }

    public void VisibleShader() {
        materialList.Clear();
        materialList.Add(visible);
        materialList.AddRange(renderers.sharedMaterials);
        renderers.materials = materialList.ToArray();
    }

}
