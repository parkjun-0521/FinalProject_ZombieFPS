using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
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
