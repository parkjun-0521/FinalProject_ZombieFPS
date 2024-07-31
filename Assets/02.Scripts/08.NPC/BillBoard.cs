using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BillBoard : MonoBehaviour
{

    //자기 자신의 Transform을 참조 할 수있는 레퍼런스
    [SerializeField] Transform textBubbleTrans;
    //메인 카메라의 Transform을 참조 할 수있는 레퍼런스
    [SerializeField]GameObject[] players;
    [SerializeField]GameObject isMinePlayer;
    void Start()
    {
        StartCoroutine(Find());
    }

    void Update()
    {
        if(isMinePlayer != null)
            textBubbleTrans.LookAt(isMinePlayer.transform);
    }

    IEnumerator Find()
    {
        yield return new WaitForSeconds(1.0f);
        players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                isMinePlayer = player;
            }
        }

    }
}