using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks {
    public Text[] chatText;
    public InputField chatInput;
    public Button chatButton;

    void Start() {
        NetworkManager.Instance.chatText = chatText;
        NetworkManager.Instance.chatInput = chatInput;
        chatButton.onClick.AddListener(() => NetworkManager.Instance.Send());


        
        //Pooling.instance.GetObject("EliteMeleeZombie");
        Pooling.instance.GetObject("EliteRangeZombie");
        Pooling.instance.GetObject("Boss_Phobos");

    }
}
