using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;
    public bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        animator.SetBool("isOpen", true);
        isOpen = true;
    }

    public void CloseDoor()
    {
        animator.SetBool("isOpen", false);
        isOpen = false;
    }
}
