using System.Collections.Generic;
using UnityEngine;

public class NavigationController : MonoBehaviour
{
    [Header("Navigation Elements")]
    public List<NavButton> navButtons;
    public List<GameObject> panels;

    private void Start()
    {
        // Ensure lists are same size
        if (navButtons.Count != panels.Count)
        {
            Debug.LogError("NavigationController: Number of buttons and panels must match!");
            return;
        }

        // Initialize buttons
        for (int i = 0; i < navButtons.Count; i++)
        {
            navButtons[i].Initialize(this, i);
        }
    }

    public void OnButtonClicked(NavButton clickedButton)
    {
        int index = clickedButton.Index;
        
        bool isAlreadySelected = false;
        if (index >= 0 && index < panels.Count)
        {
             if (panels[index] != null) isAlreadySelected = panels[index].activeSelf;
        }

        for (int i = 0; i < navButtons.Count; i++)
        {
            bool isTarget = (i == index) && !isAlreadySelected;
            
            if (navButtons[i] != null) navButtons[i].SetSelected(isTarget);
            if (panels[i] != null) panels[i].SetActive(isTarget);
        }
    }

    public void DeselectAll()
    {
        for (int i = 0; i < navButtons.Count; i++)
        {
            if (navButtons[i] != null) navButtons[i].SetSelected(false);
            if (panels[i] != null) panels[i].SetActive(false);
        }
    }
}
