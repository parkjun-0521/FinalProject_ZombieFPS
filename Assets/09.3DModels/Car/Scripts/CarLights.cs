using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyStang
{

public class CarLights : MonoBehaviour
{
    public bool isDay; // change this bool from the inspector to have the front spot lights on when it's night.
    
    [Header("Rear Lights")]
    public GameObject RearWhitePointLights;
    public GameObject RearRedPointLights;

    [Header("Front Lights")]
    public GameObject FrontSpotLights;

    void Start() // called the first frame, when the game starts.
    {
        RearWhitePointLights.SetActive(false);
        RearRedPointLights.SetActive(false);
        
        if(isDay == false)
        {
            FrontSpotLights.SetActive(true);
        }
        else
        {
            FrontSpotLights.SetActive(false);
        }
    }

    // rear white lights.
    public void RearWhiteLightsOn()
    {
        RearWhitePointLights.SetActive(true);
    }
    public void RearWhiteLightsOff()
    {
        RearWhitePointLights.SetActive(false);
    }

    // rear red lights.
    public void RearRedLightsOn()
    {
        RearRedPointLights.SetActive(true);
    }
    public void RearRedLightsOff()
    {
        RearRedPointLights.SetActive(false);
    }
}

}