using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class MenuManager : MonoBehaviour
    {
        public Menu CurrentMenu;
        public bool IsMenuOpen { get; protected set; }
        public float LoadingTransitionTime = 0.1f;
        public GameObject LoadingScreen;

        public void Awake()
        {
            this.IsMenuOpen = true;
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
            Time.timeScale = 0f;
            Cursor.visible = true;
            CurrentMenu.IsOpening = true;
        }

        public virtual void Resume()
        {
            this.IsMenuOpen = false;
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
            SetLoadingScreenActive(false);
        }

        public virtual void SetLoadingScreenActive(bool isLoading)
        {
            LoadingScreen.SetActive(isLoading);
        }

        public virtual void LoadGame()
        {
            SetLoadingScreenActive(true);

            PauseMenu pauseMenu = FindObjectOfType(typeof(PauseMenu)) as PauseMenu;
            this.CurrentMenu = pauseMenu;
            this.CurrentMenu.IsOpening = false;
        }
    }
}