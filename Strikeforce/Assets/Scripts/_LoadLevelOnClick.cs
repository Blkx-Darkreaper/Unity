using UnityEngine;
using System.Collections;

public class _LoadLevelOnClick : MonoBehaviour
{
    public GameObject loadingScreen;

    public void LoadScene(int levelIndex)
    {
        loadingScreen.SetActive(true);
        Application.LoadLevel(levelIndex);
    }
}
