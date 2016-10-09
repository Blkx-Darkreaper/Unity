using UnityEngine;
using System.Collections;

public class AddToLevel : MonoBehaviour {

    public void LoadAddOnClick(int levelIndex)
    {
        Application.LoadLevelAdditive(levelIndex);
    }
}
