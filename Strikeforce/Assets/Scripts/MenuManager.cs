using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MenuManager : MonoBehaviour
    {
        public Menu CurrentMenu;
        public float LoadingTransitionTime = 0.1f;
        public GameObject LoadingScreen;

        public void Start()
        {
            ShowMenu(CurrentMenu);
        }

        protected virtual void OnLevelWasLoaded()
        {
            Invoke("HideLoadingScreen", LoadingTransitionTime);

            Resume();
        }

        public virtual void Pause()
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            CurrentMenu.IsOpening = true;
        }

        public virtual void Resume()
        {
            Time.timeScale = 1f;
            //Cursor.visible = false;
            CurrentMenu.IsOpening = false;
        }

        public virtual void ShowMenu(Menu menu)
        {
            if (CurrentMenu != null)
            {
                CurrentMenu.HideMenu();
            }

            CurrentMenu = menu;
            CurrentMenu.ShowMenu();
        }

        public virtual void HideLoadingScreen()
        {
            ToggleLoadingScreen(false);
        }

        public virtual void ToggleLoadingScreen(bool isLoading)
        {
            LoadingScreen.SetActive(isLoading);
        }
    }
}