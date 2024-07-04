using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    void Start() {
        SceneManager.LoadScene("01.LoginScene");
    }
}
