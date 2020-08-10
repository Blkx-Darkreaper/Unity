using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [HideInInspector]
    public Animator animator;

    void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    public IEnumerator SetTrigger(string name)
    {
        if (animator == null)
        {
            Debug.LogWarning(string.Format("No animator attached to {0}", this.name));
            yield break;
        }

        animator.SetTrigger(name);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float speed = info.speed;

        yield return new WaitForSeconds(duration * speed);
    }

    public IEnumerator SetBool(string name, bool value)
    {
        if (animator == null)
        {
            Debug.LogWarning(string.Format("No animator attached to {0}", this.name));
            yield break;
        }

        animator.SetBool(name, value);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float speed = info.speed;

        yield return new WaitForSeconds(duration * speed);
    }

    public IEnumerator SetInteger(string name, int value)
    {
        if (animator == null)
        {
            Debug.LogWarning(string.Format("No animator attached to {0}", this.name));
            yield break;
        }

        animator.SetInteger(name, value);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float speed = info.speed;

        yield return new WaitForSeconds(duration * speed);
    }

    public IEnumerator SetFloat(string name, float value)
    {
        if (animator == null)
        {
            Debug.LogWarning(string.Format("No animator attached to {0}", this.name));
            yield break;
        }

        animator.SetFloat(name, value);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float speed = info.speed;

        yield return new WaitForSeconds(duration * speed);
    }
}