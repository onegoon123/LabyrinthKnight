using UnityEngine;
using TMPro;

public class StatDisplay : MonoBehaviour
{
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
    
    public void SetStat(string statName, string statValue)
    {
        if (statNameText != null)
            statNameText.text = statName;
            
        if (statValueText != null)
            statValueText.text = statValue;
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
