using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : PoolManager<AudioSource>
{
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;

    public void PlaySound(params AudioClip[] allClips)
    {
        AudioSource source = GetNextObject();

        int randomIndex = Random.Range(0, allClips.Length);
        AudioClip clip = allClips[randomIndex];
        source.clip = clip;

        source.Play();
        PlaySoundForDuration(source, clip.length);
    }

    public void PlayRandomizedSound(params AudioClip[] allClips)
    {
        AudioSource source = GetNextObject();

        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        source.pitch = randomPitch;

        int randomIndex = Random.Range(0, allClips.Length);
        AudioClip clip = allClips[randomIndex];
        source.clip = clip;

        source.Play();
        PlaySoundForDuration(source, clip.length);
    }

    protected IEnumerator PlaySoundForDuration(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);

        //ReleaseObject(source);
    }

    public override AudioSource CreateObject()
    {
        AudioSource source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        source.enabled = false;
        return source;
    }

    public override void DestroyObject(AudioSource obj)
    {
        Destroy(obj);
    }

    protected override void CleanUpObject(AudioSource obj)
    {
        obj.enabled = false;
        obj.clip = null;
        obj.pitch = 1;

        obj.Stop();
    }

    public override bool IsObjectAvailable(AudioSource obj)
    {
        return obj.isActiveAndEnabled;
    }
}