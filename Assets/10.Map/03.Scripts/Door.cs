using UnityEngine;
using Photon.Pun;

public class Door : MonoBehaviourPun
{
    private Animator animator;
    public bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoorRPC()
    {
        photonView.RPC("OpenDoor", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void OpenDoor()
    {
        animator.SetBool("isOpen", true);
        isOpen = true;
    }


    public void CloseDoorRPC()
    {
        photonView.RPC("CloseDoor", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void CloseDoor()
    {
        animator.SetBool("isOpen", false);
        isOpen = false;
    }
}
