using UnityEngine;
using System.Collections;

public class _ChangeMusic : MonoBehaviour {

    public AudioClip[] backgroundMusic;
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnLevelWasLoaded(int levelIndex)
    {
        if (levelIndex != 0)
        {
            return;
        }

        AudioClip music = backgroundMusic[levelIndex];
        if (music == null)
        {
            return;
        }

        source.clip = music;
    }
}
