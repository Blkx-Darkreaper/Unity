using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MenuManager : MonoBehaviour
    {
        public Menu currentMenu;

        public void Start()
        {
            ShowMenu(currentMenu);
        }

        public virtual void ShowMenu(Menu menu)
        {
            if (currentMenu != null)
            {
                currentMenu.IsOpeningMenu = false;
            }

            currentMenu = menu;
            currentMenu.IsOpeningMenu = true;
        }
    }
}