using UnityEngine;
using System.Collections;

public class QuitApplication : MonoBehaviour {

    public void QuitOnClick() {
        Debug.Log("Exiting application");
        Application.Quit();
    }
}
