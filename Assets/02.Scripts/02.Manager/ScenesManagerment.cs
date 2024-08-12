using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScenesManagerment : MonoBehaviourPun
{
    public static ScenesManagerment Instance;
    public int playerCount;     // ���� �������� �̵��� ���ִ� �÷��̾� 
    public int stageCount;      // ���� �������� ī��Ʈ 

    public int readyUserCount;  // �غ�� �÷��̾� Ȯ�� 
    public bool isResign;       // ���� Ȯ�� 

    // �� ��ȯ �� ���� ���� 
    public float x;
    public float y;

    void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start() {
        SceneManager.LoadScene("01.LoginScene");
    }
}
