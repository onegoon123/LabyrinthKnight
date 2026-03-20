using UnityEngine;

public class NavButton : MonoBehaviour
{
    private Animator animator;
    private bool isSelected;
    private NavigationController controller;
    private int index;

    public int Index => index;

    public void Initialize(NavigationController controller, int index)
    {
        this.controller = controller;
        this.index = index;
        animator = GetComponent<Animator>();
    }

    public void OnButtonClick()
    {
        if (controller != null)
        {
            controller.OnButtonClicked(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (animator != null)
        {
            animator.SetBool("Selected", isSelected);
        }
    }
}
