using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class _LoadLevelOnClick : MonoBehaviour
{
    public GameObject loadingScreen;

    public void LoadScene(int levelIndex)
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(levelIndex);
    }
}
