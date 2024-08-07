using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyStang
{

public class CarSounds : MonoBehaviour
{
    private float currentSpeed;

    private Rigidbody carRb;
    private AudioSource carAudio;

    [Header("Engine Pitch settings")]
    public float minPitch;
    public float maxPitch;
    private float pitchFromCar;

    public float[] smoothVelocities; // IMPORTANT: add enough floats of this array from the inspector (as many as the wheels).

    void Start() // called the first frame, when the game starts.
    {
        carAudio = GetComponent<AudioSource>();
        carRb = GetComponent<Rigidbody>();
    }

    void Update() // called every frame
    {
        EngineSound();
    }

    void EngineSound() // changing the pitch according to the speed
    {
        currentSpeed = carRb.velocity.magnitude;
        pitchFromCar = currentSpeed / 60f;

        carAudio.pitch = Mathf.Clamp(minPitch + pitchFromCar, minPitch, maxPitch); // keeping sure to stay into the pitch bounds.
    }

    // called from Car Controller
    public void PlaySkidSound(GameObject skidSound) // setting the volume of the skid sound to 1.
    {
        skidSound.GetComponent<AudioSource>().volume = 1;
    }

    // called from Car Controller
    public void StopSkidSound(GameObject skidSound, int index) // changing the volume from the skid sound from the value it currently has to 0.
    {
        skidSound.GetComponent<AudioSource>().volume = Mathf.SmoothDamp(skidSound.GetComponent<AudioSource>().volume, 0, ref smoothVelocities[index], 0.1f);
    }
}

}