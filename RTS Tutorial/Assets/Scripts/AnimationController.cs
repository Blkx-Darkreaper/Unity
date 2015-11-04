using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour {

    public string animationName { get; private set; }
    protected int nextFrame = 0;
    protected int lastFrame = 0;
    protected Dictionary<int, AnimationFrame> keyframes;
    public Texture2D image { get { return keyframes[nextFrame].image; } }
    public AudioClip soundEffect { get { return keyframes[nextFrame].audio; } }

    public AnimationController(string name)
    {
        animationName = name;
        keyframes = new Dictionary<int, AnimationFrame>();
    }

    public void AddFrame()
    {
        lastFrame++;
    }

    public void AddFrame(Texture2D image, AudioClip audio)
    {
        if (image == null && audio == null)
        {
            AddFrame();
        }

        AnimationFrame toAdd = new AnimationFrame(lastFrame, image, audio);
        lastFrame++;
        keyframes.Add(lastFrame, toAdd);
    }

    public void Next()
    {
        if (HasNext() == false)
        {
            nextFrame = 0;
            return;
        }

        nextFrame++;
    }

    private bool HasNext()
    {
        if (nextFrame >= lastFrame)
        {
            return false;
        }

        return true;
    }

    public class AnimationFrame
    {
        public int frameIndex { get; private set; }
        public Texture2D image { get; private set; }
        public AudioClip audio { get; private set; }

        public AnimationFrame(int index, Texture2D image, AudioClip audio)
        {
            frameIndex = index;
            this.image = image;
            this.audio = audio;
        }
    }
}
