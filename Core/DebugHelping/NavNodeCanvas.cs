using TMPro;
using UnityEngine;

public class NavNodeCanvas : MonoBehaviour
{
    public TextMeshProUGUI navTextBox;
    public TextMeshProUGUI cellTextBox;

    public void SetNavText(string text)
    {
        navTextBox.text = text;
    }

    public void SetCellText(string text)
    {
        cellTextBox.text = text;
    }

    public void SetNavColor(Color color)
    {
        navTextBox.color = color;
    }

    public void SetCellColor(Color color)
    {
        cellTextBox.color = color;
    }
}