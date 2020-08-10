using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AddToLevel : MonoBehaviour
{
    public void LoadAddOnClick(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
}