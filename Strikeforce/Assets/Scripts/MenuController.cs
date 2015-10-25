using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

    private Animator animator;
    private CanvasGroup group;

    public bool IsOpen
    {
        get { return animator.GetBool("IsOpen"); }
        set { animator.SetBool("IsOpen", value); }
    }

    public void Awake()
    {
        animator = GetComponent<Animator>();
        group = GetComponent<CanvasGroup>();

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.offsetMax = rectTransform.offsetMin = new Vector2(0, 0);
    }

    public void Update()
    {
        bool isActive = animator.GetCurrentAnimatorStateInfo(0).IsName("Open");
        group.blocksRaycasts = group.interactable = isActive;
    }
}
