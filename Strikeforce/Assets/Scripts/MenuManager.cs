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

        protected virtual void OnLevelWasLoaded()
        {
            Invoke("HideLoadingScreen", LoadingTransitionTime);

            Resume();
        }

        public virtual void Pause()
        {
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
            CurrentMenu.IsOpening = false;
        }

        public virtual void ShowMenu(Menu menu)
        {
            if(menu == null)
            {
                Debug.Log("Cannot show menu, no menu loaded");
                return;
            }

            if (CurrentMenu != null)
            {
                CurrentMenu.HideMenu();
            }

            CurrentMenu = menu;
            CurrentMenu.ShowMenu();
        }

        public virtual void HideLoadingScreen()
        {
            SetLoadingScreenActive(false);
        }

        public virtual void SetLoadingScreenActive(bool isLoading)
        {
            LoadingScreen.SetActive(isLoading);
        }
    }
}