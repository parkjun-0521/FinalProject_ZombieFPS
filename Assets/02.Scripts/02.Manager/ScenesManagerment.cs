using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagerment : MonoBehaviourPun
{
    public static ScenesManagerment Instance;
    public int playerCount;
    public int stageCount;
    void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start() {
        SceneManager.LoadScene("01.LoginScene");
    }
}
