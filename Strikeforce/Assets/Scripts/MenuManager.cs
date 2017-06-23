using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MenuManager : Manager
    {
        public static MenuManager Singleton = null;
        public Menu CurrentMenu;
        public bool IsMenuOpen { get; protected set; }
        public float LoadingTransitionTime = 0.1f;
        public GameObject LoadingScreen;

        public void Awake()
        {
            if (Singleton == null)
            {
                DontDestroyOnLoad(gameObject);
                Singleton = this;
            }
            if (Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            this.IsMenuOpen = true;
            Cursor.visible = false;
        }

        public void Start()
        {
            ShowMenu(CurrentMenu);
        }

        public virtual void Pause()
        {
            if(CurrentMenu == null)
            {
                return;
            }

            this.IsMenuOpen = true;
            //Time.timeScale = 0f;
            //Cursor.visible = true;
            CurrentMenu.IsOpening = true;
        }

        public virtual void Resume()
        {
            this.IsMenuOpen = false;
            //Time.timeScale = 1f;
            //Cursor.visible = false;

            if(CurrentMenu == null)
            {
                return;
            }

            CurrentMenu.IsOpening = false;
        }

        public virtual void SetCurrentMenu(Menu menu)
        {
            if (menu == null)
            {
                Debug.Log("Cannot show menu, no menu loaded");
                return;
            }

            if (CurrentMenu != null)
            {
                CurrentMenu.HideMenu();
            }

            CurrentMenu = menu;
            CurrentMenu.ReadyMenu();
        }

        public virtual void ShowMenu(Menu menu)
        {
            SetCurrentMenu(menu);
            CurrentMenu.ShowMenu();

            // Select the previously selected button
            CurrentMenu.SelectCurrentMenuButton();
        }

        public virtual void ShowLoadingScreen()
        {
            SetLoadingScreenActive(true);
            this.IsMenuOpen = false;
        }

        public virtual void HideLoadingScreenDelayed()
        {
            Invoke("HideLoadingScreen", LoadingTransitionTime);
        }

        public virtual void HideLoadingScreen()
        {
            SetLoadingScreenActive(false);
            if(CurrentMenu.IsOpen == false)
            {
                return;
            }

            this.IsMenuOpen = true;
        }

        public virtual void SetLoadingScreenActive(bool isLoading)
        {
            LoadingScreen.SetActive(isLoading);
        }
    }
}