using UnityEngine;

public class AnimPlayOnce : MonoBehaviour
{
    public float delay = 0f;
    protected Animator animator;

    // Use this for initialization
    void Start()
    {
        this.animator = this.GetComponent<Animator>();

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float speed = info.speed;

        Destroy(gameObject, duration + delay);
    }
}