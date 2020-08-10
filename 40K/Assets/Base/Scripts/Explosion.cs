using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Explosion : Impermanent
{
    public AudioClip sfx;
    protected Animator animator;

    public void Awake()
    {
        this.animator = GetComponent<Animator>();

        AnimationClip[] allAnimations = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip animation in allAnimations)
        {
            switch (animation.name)
            {
                case "Explosion":
                    this.lifetime = animation.length;
                    break;

                //case "Idle":
                //    idleTime = animation.length;
                //    break;

                default:
                    this.lifetime = animation.length;
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        AudioSource.PlayClipAtPoint(sfx, gameObject.transform.position);

        animator.Play("Explosion", -1, 0f);

        Invoke("Disable", lifetime);
    }

    public override void Disable()
    {
        gameObject.SetActive(false);
    }

    public override void Destroy()
    {
        
    }
}