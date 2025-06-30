using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public enum Animation
    {
        IDLE,
        RUN,
        SPRINT,
        JUMP,
        COLLECT,
        HIT
    }

    private Animator animator;
    private Animation lastAnimation;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastAnimation = Animation.IDLE;
        Idle();
    }

    public void SetBaseMovement(float magnitude, bool isSprinting, bool isJumping)
    {
        animator.SetFloat("Speed", magnitude);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsJumping", isJumping);

        ManageBaseMovementServer(magnitude, isSprinting, isJumping);
    }

    public void Idle()
    {
        SetBaseMovement(0.0f, false, false);
        UpdateAnimationServer(Animation.IDLE);
    }

    public void GetHit()
    {
        animator.SetTrigger("Hit");
        UpdateAnimationServer(Animation.HIT);
    }

    public void Collect()
    {
        animator.SetTrigger("Collecting");
        UpdateAnimationServer(Animation.COLLECT);
    }

    private void ManageBaseMovementServer(float magnitude, bool isSprinting, bool isJumping)
    {
        if (isJumping)
        {
            UpdateAnimationServer(Animation.JUMP);
            return;
        }
        if (magnitude < 0.1f)
        {
            UpdateAnimationServer(Animation.IDLE);
            return;
        }
        if (isSprinting)
        {
            UpdateAnimationServer(Animation.SPRINT);
            return;
        }
        UpdateAnimationServer(Animation.RUN);
    }

    private void UpdateAnimationServer(Animation animation)
    {
        if (!PlayerData.multiplayer)
            return;

        if (lastAnimation != animation)
        {
            switch (animation)
            {
                case Animation.IDLE:
                    ServerManager.Instance.SendAnimation("idle");
                    break;
                case Animation.RUN:
                    ServerManager.Instance.SendAnimation("run");
                    break;
                case Animation.SPRINT:
                    ServerManager.Instance.SendAnimation("sprint");
                    break;
                case Animation.JUMP:
                    ServerManager.Instance.SendAnimation("jump");
                    break;
                case Animation.COLLECT:
                    ServerManager.Instance.SendAnimation("collect");
                    break;
                case Animation.HIT:
                    ServerManager.Instance.SendAnimation("hit");
                    break;
            }
            lastAnimation = animation;
        }
    }

    public static Animation ConvertAnimationStringToEnum(string animation)
    {
        switch (animation)
        {
            case "idle":
                return Animation.IDLE;
            case "run":
                return Animation.RUN;
            case "sprint":
                return Animation.SPRINT;
            case "jump":
                return Animation.JUMP;
            case "collect":
                return Animation.COLLECT;
            case "hit":
                return Animation.HIT;
        }
        return Animation.IDLE;
    }
}
