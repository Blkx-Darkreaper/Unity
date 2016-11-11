using UnityEngine;
using System.Collections;

public class _AddToLevel : MonoBehaviour {

    public void LoadAddOnClick(int levelIndex)
    {
        Application.LoadLevelAdditive(levelIndex);
    }
}
