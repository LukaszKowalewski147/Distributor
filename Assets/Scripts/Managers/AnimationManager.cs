using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        Idle();
    }

    public void SetBaseMovement(float magnitude, bool isSprinting, bool isJumping)
    {
        animator.SetFloat("Speed", magnitude);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsJumping", isJumping);
    }

    public void Idle()
    {
        SetBaseMovement(0.0f, false, false);
    }

    public void GetHit()
    {
        animator.SetTrigger("Hit");
    }

    public void Collect()
    {
        animator.SetTrigger("Collecting");
    }
}
