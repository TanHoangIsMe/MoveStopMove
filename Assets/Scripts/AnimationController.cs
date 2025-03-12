using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void PlayRunAnimation()
    {
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsDead", false);
        animator.SetBool("IsAttack", false);
        animator.SetBool("IsWin", false);
    }

    public void PlayIdleAnimation()
    {
        animator.SetBool("IsIdle", true);
        animator.SetBool("IsDead", false);
        animator.SetBool("IsAttack", false);
        animator.SetBool("IsWin", false);
        animator.SetBool("IsDance", false);
    }

    public void PlayAttackAnimation()
    {
        animator.SetBool("IsAttack", true);
        animator.SetBool("IsDead", false);       
        animator.SetBool("IsWin", false);
        animator.SetBool("IsUlti", false);
    }

    public void PlayDeathAnimation() 
    {
        animator.SetBool("IsDead", true);
    }

    public void PlayWinAnimation()
    {
        animator.SetBool("IsWin", true);
        animator.SetBool("IsDead", false);
    }
}
