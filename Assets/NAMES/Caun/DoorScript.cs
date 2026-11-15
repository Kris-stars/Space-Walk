using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenDoor()
    {
        animator.SetTrigger("Open Door");
    }
    public void CloseDoor()
    {
        animator.SetTrigger("Close Door");
    }
}

