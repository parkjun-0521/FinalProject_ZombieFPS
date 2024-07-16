using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeZombie_EyesGlow : MonoBehaviour
{



    private int eyesTyp;
    public Material[] BodyMaterials = new Material[1];
    public SkinnedMeshRenderer meshRenderer;
    public enum EyesGlow
    {
        No,
        Yes
    }


    public EyesGlow eyesGlow;

    void Start()
    {
        if (eyesGlow == 0)
        {
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetFloat("_EmissiveExposureWeight", 1);

            //BodyMaterials[0].DisableKeyword("_EMISSION");
            //BodyMaterials[0].SetFloat("_EmissiveExposureWeight", 1);
        }
        else
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetFloat("_EmissiveExposureWeight", 0);

            //BodyMaterials[0].EnableKeyword("_EMISSION");
            //BodyMaterials[0].SetFloat("_EmissiveExposureWeight", 0);

        }

    }
}
