using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMiniMap : MonoBehaviour
{
    //public GameObject MiniMap;
    public RectTransform minimap;
    public GameObject playerUI;
    Player player;
    [SerializeField]GameObject[] players;
    [SerializeField] float sizeX = 8;
    [SerializeField] float sizeY = 8f;
    [SerializeField] Vector3 temp = new Vector3(-35.26f, 0 ,1.05f);
    [SerializeField] GameObject[] playersIcon;
    [SerializeField] RectTransform[] playerIconRect;
    [SerializeField] Vector3 otherTemp = new Vector3(36.08f, 0, -0.31f);
    [SerializeField] float otherPlayerSize = 1.35f;
    bool isPlayerFind = true;

    
    int playerCount = 0;
    int PlayerCount
    {
        get
        {
            return playerCount;
        }
        set
        {
            if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount) return;
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            StartCoroutine(Search());
            Debug.Log("프로퍼티작동");
        }
    }
    GameObject otherPlayerUI;

    private void Awake()
    {

    }
    private void Start()
    {
        //player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }
    private void Update()
    {
        PlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if (player != null)
        {
            minimap.offsetMax = new Vector2((temp.x + player.transform.position.x) * sizeX, (temp.z + player.transform.position.z) * sizeY);
            minimap.offsetMin = new Vector2((temp.x + player.transform.position.x) * sizeX, (temp.z + player.transform.position.z) * sizeY);
        }
        else
        {
            //player = GameObject.FindWithTag("Player").GetComponent<Player>();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject temp in players)
            {
                if (temp.GetComponent<PhotonView>().IsMine == true)
                {
                    player = temp.GetComponent<Player>();
                    return;
                }
            }
            return;
        }



        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            OtherPlayerIcon(1);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            OtherPlayerIcon(2);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
        {
            OtherPlayerIcon(3);
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
        {
            OtherPlayerIcon(4);
        }
        //if (photonView.IsMine)
        //{
        //    PlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        //    if (player != null)
        //    {
        //        minimap.offsetMax = new Vector2((temp.x + player.transform.position.x) * sizeX, (temp.z + player.transform.position.z) * sizeY);
        //        minimap.offsetMin = new Vector2((temp.x + player.transform.position.x) * sizeX, (temp.z + player.transform.position.z) * sizeY);
        //    }
        //    else
        //    {
        //        //player = GameObject.FindWithTag("Player").GetComponent<Player>();
        //        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //        foreach(GameObject temp in players)
        //        {
        //            if(temp.GetComponent<PhotonView>().IsMine == true)
        //            {
        //                player = temp.GetComponent<Player>();
        //            }
        //        }
        //    }



        //    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        //    {
        //        OtherPlayerIcon(1);
        //    }
        //    else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        //    {
        //        OtherPlayerIcon(2);
        //    }
        //    else if(PhotonNetwork.CurrentRoom.PlayerCount == 3)
        //    {
        //        OtherPlayerIcon(3);
        //    }
        //    else if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
        //    {
        //        OtherPlayerIcon(4);
        //    }
        //}
    }

    void OtherPlayerIcon(float num)
    {
        for(int i = 0; i < num - 1; i++)
        {
            playersIcon[i].SetActive(true);
        }
        for(int i = 2; i > num - 2; i--)  //1     2  0    3 1    4   2
        {
            playersIcon[i].SetActive(false);
        }
        switch (num)
        {
            case 2:
                //playersIcon[0].SetActive(true);
                if (players.Length != 2) return;
                playerIconRect[0].offsetMax = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);
                playerIconRect[0].offsetMin = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);
                break;
            case 3:
                //playersIcon[0].SetActive(true);
                if (players.Length != 3) return;
                playerIconRect[0].offsetMax = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);
                playerIconRect[0].offsetMin = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);

                //playersIcon[1].SetActive(true);
                playerIconRect[1].offsetMax = new Vector2((otherTemp.x - players[2].transform.position.x) * otherPlayerSize, (otherTemp.z - players[2].transform.position.z) * otherPlayerSize);
                playerIconRect[1].offsetMin = new Vector2((otherTemp.x - players[2].transform.position.x) * otherPlayerSize, (otherTemp.z - players[2].transform.position.z) * otherPlayerSize);
                break;
            case 4:
                //playersIcon[0].SetActive(true);
                if (players.Length != 4) return;
                playerIconRect[0].offsetMax = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);
                playerIconRect[0].offsetMin = new Vector2((otherTemp.x - players[1].transform.position.x) * otherPlayerSize, (otherTemp.z - players[1].transform.position.z) * otherPlayerSize);

                //playersIcon[1].SetActive(true);
                playerIconRect[1].offsetMax = new Vector2((otherTemp.x - players[2].transform.position.x) * otherPlayerSize, (otherTemp.z - players[2].transform.position.z) * otherPlayerSize);
                playerIconRect[1].offsetMin = new Vector2((otherTemp.x - players[2].transform.position.x) * otherPlayerSize, (otherTemp.z - players[2].transform.position.z) * otherPlayerSize);

                //playersIcon[2].SetActive(true);
                playerIconRect[2].offsetMax = new Vector2((otherTemp.x - players[3].transform.position.x) * otherPlayerSize, (otherTemp.z - players[3].transform.position.z) * otherPlayerSize);
                playerIconRect[2].offsetMin = new Vector2((otherTemp.x - players[3].transform.position.x) * otherPlayerSize, (otherTemp.z - players[3].transform.position.z) * otherPlayerSize);
                break;
        }
    }

    IEnumerator Search()
    {
        yield return new WaitForSeconds(1.0f);
        players = GameObject.FindGameObjectsWithTag("Player");
    }
}
