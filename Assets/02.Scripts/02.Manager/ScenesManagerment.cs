using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagerment : MonoBehaviourPun
{
    public static ScenesManagerment Instance;
    public int playerCount;     // 다음 스테이지 이동에 들어가있는 플레이어 
    public int stageCount;      // 현재 스테이지 카운트 

    public int readyUserCount;  // 준비된 플레이어 확인 
    public bool isResign;       // 강퇴 확인 

    void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start() {
        SceneManager.LoadScene("01.LoginScene");
    }
}
