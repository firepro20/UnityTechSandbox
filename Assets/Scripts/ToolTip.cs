using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    // Public Variables
    public string toolTipText;

    // Private Variables
    private string currentToolTipText;
    private GUIStyle guiStyleFore;
    private GUIStyle guiStyleBack;

    private void Start()
    {
        guiStyleFore = new GUIStyle();
        guiStyleFore.normal.textColor = Color.white;
        guiStyleFore.alignment = TextAnchor.UpperCenter;
        guiStyleFore.wordWrap = true;
        guiStyleBack = new GUIStyle();
        guiStyleBack.normal.textColor = Color.black;
        guiStyleBack.alignment = TextAnchor.UpperCenter;
        guiStyleBack.wordWrap = true;
    }
    void OnMouseEnter()
    {
        currentToolTipText = toolTipText;
    }

    void OnMouseExit()
    {
        currentToolTipText = "";
    }

    void OnGUI()
    {
        if (currentToolTipText != "")
        {
            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;
            GUI.Label(new Rect(x - 150, y + 20, 300, 60), currentToolTipText, guiStyleBack);
            GUI.Label(new Rect(x - 140, y + 20, 300, 60), currentToolTipText, guiStyleFore);
        }
    }
}
