using UnityEngine;
using UnityEngine.UI;

public class ChecklistItem : MonoBehaviour
{
    public string taskName;
    public Image tickboxImage;
    public Sprite checkmarkSprite; 

    void OnEnable() => TutorialManager.OnTaskComplete += UpdateUI;
    void OnDisable() => TutorialManager.OnTaskComplete -= UpdateUI;

    private void UpdateUI(string completedTask)
    {
        if (completedTask == taskName)
        {
            tickboxImage.sprite = checkmarkSprite;
            tickboxImage.color = Color.green; 
        }
    }
}