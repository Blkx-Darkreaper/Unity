using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour {

    public GameObject loadingScreen;

    public void LoadSceneByIndex(int buildIndex)
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(buildIndex);
    }

    public void LoadSceneByName(string name)
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(name);
    }
}
