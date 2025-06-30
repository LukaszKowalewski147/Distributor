using UnityEngine;

public class OtherPlayerManager : MonoBehaviour
{
    private Animator animator;
    private string ID;

    private void Awake()
    {
        Debug.Log("OtherPlayerManager.cs - Awake()");
        animator = GetComponent<Animator>();
    }

    public void SetID(string id)
    {
        ID = id;
        //Debug.Log("OtherPlayerManager.cs - SetID(): ID set to: " + ID);
    }

    public string GetID()
    {
        return ID;
    }

    public void MovePlayer(Vector3 position, float rotationY)
    {
        //Debug.Log("OtherPlayerManager.cs [" + ID + "] - MovePlayer()");
        transform.position = position;
        transform.eulerAngles = new Vector3(0.0f, rotationY, 0.0f);
    }

    public void TriggerAnimation(AnimationManager.Animation animation)
    {
        //Debug.Log("OtherPlayerManager.cs [" + ID + "] - TriggerAnimation(" + animation.ToString() +")");
        switch (animation)
        {
            case AnimationManager.Animation.IDLE:
                animator.SetTrigger("Idle");
                break;
            case AnimationManager.Animation.RUN:
                animator.SetTrigger("Run");
                break;
            case AnimationManager.Animation.SPRINT:
                animator.SetTrigger("Sprint");
                break;
            case AnimationManager.Animation.JUMP:
                animator.SetTrigger("Jump");
                break;
            case AnimationManager.Animation.COLLECT:
                animator.SetTrigger("Collect");
                break;
            case AnimationManager.Animation.HIT:
                animator.SetTrigger("Hit");
                break;
        }
        //Debug.Log(ID + " triggered " + animation.ToString() + " animation");
    }
}
