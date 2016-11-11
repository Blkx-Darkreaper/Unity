using UnityEngine;
using System.Collections;

public class _QuitApplication : MonoBehaviour
{
    public void QuitOnClick()
    {
        Debug.Log("Exiting application");
        Application.Quit();
    }
}
