using UnityEngine;
using System.Collections;
using RTS;

public class MenuController : MonoBehaviour {

    protected PlayerController player;
    protected string[] buttons;

    protected virtual void Start()
    {
        player = transform.root.GetComponent<PlayerController>();
        buttons = new string[] { };
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    protected virtual void Resume()
    {
        Time.timeScale = 1f;
        GetComponent<PauseMenuController>().enabled = false;
        if (player != null)
        {
            player.GetComponent<UserInput>().enabled = true;
        }
        Cursor.visible = false;
        GameManager.activeInstance.isMenuOpen = false;
    }

    protected virtual void ExitGame()
    {
        string playerName = "Unknown player";
        if(player != null) {
            playerName = player.username;
        }

        Debug.Log(string.Format("{0} has left the game", playerName));
        Application.Quit();
    }
}