using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject activeIndicator; // optional: z.B. kleines Underline-Objekt
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.cyan;

    public void Setup(string label, System.Action onClick)
    {
        text.text = label;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetActiveVisual(bool active)
    {
        if (activeIndicator) activeIndicator.SetActive(active);
        if (text) text.color = active ? activeColor : normalColor;
    }
}