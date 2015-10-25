using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

    public MenuController currentMenu;

    public void Start()
    {
        ShowMenu(currentMenu);
    }

    public void ShowMenu(MenuController menu)
    {
        if (currentMenu != null)
        {
            currentMenu.IsOpen = false;
        }

        currentMenu = menu;
        currentMenu.IsOpen = true;
    }
}
