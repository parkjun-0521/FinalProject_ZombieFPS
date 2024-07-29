using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedOutline : MonoBehaviour
{
    Material outline;

    Renderer renderers;
    List<Material> materialList = new List<Material>();

    void Start() {
        Shader outlineShader = Shader.Find("Draw/OutlineShader");

        outline = new Material(outlineShader);
        renderers = GetComponent<Renderer>();
    }

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


}
