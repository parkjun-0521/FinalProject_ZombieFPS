using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedOutline : MonoBehaviour
{
    Material outline;

    Renderer renderers;
    List<Material> materialList = new List<Material>();

    public void ActivateOutline()
    {
        materialList.Clear();
        materialList.AddRange(renderers.sharedMaterials);
        materialList.Add(outline);
        renderers.materials = materialList.ToArray();
    }

    public void DeactivateOutline()
    {
        materialList.Clear();
        materialList.AddRange(renderers.sharedMaterials);
        materialList.Remove(outline);
        renderers.materials = materialList.ToArray();
    }

    void Start()
    {
        outline = new Material(Shader.Find("Draw/OutlineShader"));
        renderers = GetComponent<Renderer>();
    }
}
