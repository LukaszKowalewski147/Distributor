using UnityEngine;

public class OtherPlayerManager : MonoBehaviour
{
    private Animator animator;
    private string ID;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ID = RemovePlayerPrefix(gameObject.name);
    }

    public void MovePlayer(Vector3 position)
    {
        transform.position = position;
    }

    public void TriggerAnimation(string animation)
    {
        switch (animation)
        {
            case "idle":
                animator.SetTrigger("Idle");
                break;
            case "run":
                animator.SetTrigger("Run");
                break;
            case "sprint":
                animator.SetTrigger("Sprint");
                break;
            case "jump":
                animator.SetTrigger("Jump");
                break;
            case "collect":
                animator.SetTrigger("Collect");
                break;
            case "hit":
                animator.SetTrigger("Hit");
                break;
        }
    }
    
    private string RemovePlayerPrefix(string input)
    {
        return input.Replace("player_", "");
    }
}
