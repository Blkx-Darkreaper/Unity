using UnityEngine;
using System.Collections;

public class ConfirmDialogController
{

    public bool isConfirming { get; private set; }
    public bool hasClickedYes { get; private set; }
    public bool hasClickedNo { get; private set; }
    public bool hasMadeSelection
    {
        get
        {
            bool selectionMade = hasClickedYes || hasClickedNo;
            return selectionMade;
        }
    }

    private Rect confirmationBox;
    private float buttonWidth = 50, buttonHeight = 20, padding = 10;
    private Vector2 messageDimensions;

    public ConfirmDialogController()
    {
        isConfirming = false;
        hasClickedYes = false;
        hasClickedNo = false;
    }

    public void Confirm()
    {
        isConfirming = true;
        hasClickedYes = false;
        hasClickedNo = false;
    }

    public void Done()
    {
        isConfirming = false;
        hasClickedYes = false;
        hasClickedNo = false;
    }

    public void ShowDialog(string message, GUISkin skin)
    {
        GUI.skin = skin;
        ShowDialog(message);
    }

    public void ShowDialog(string message)
    {
        messageDimensions = GUI.skin.GetStyle("window").CalcSize(new GUIContent(message));

        float width = messageDimensions.x + 2 * padding;
        float height = messageDimensions.y + buttonHeight + 2 * padding;
        float x = Screen.width / 2 - width / 2;
        float y = Screen.height / 2 - height / 2;
        confirmationBox = new Rect(x, y, width, height);
        confirmationBox = GUI.Window(0, confirmationBox, Dialog, message);
    }

    private void Dialog(int windowId)
    {
        float x = messageDimensions.x / 2 - buttonWidth - padding / 2;
        float y = messageDimensions.y + padding;

        bool buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Yes");
        if (buttonPressed == true)
        {
            isConfirming = false;
            hasClickedYes = true;
        }

        x += buttonWidth + padding;
        buttonPressed = GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "No");
        if (buttonPressed == true)
        {
            isConfirming = false;
            hasClickedNo = true;
        }
    }
}