using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BillBoard : MonoBehaviour
{

    //�ڱ� �ڽ��� Transform�� ���� �� ���ִ� ���۷���
    [SerializeField] Transform textBubbleTrans;
    //���� ī�޶��� Transform�� ���� �� ���ִ� ���۷���
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