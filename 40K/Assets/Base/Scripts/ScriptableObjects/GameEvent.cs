using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameEvent", menuName = "Game Event", order = 1)]
public class GameEvent : ScriptableObject
{
    [HideInInspector]
    public List<GameEventListener> allListeners = new List<GameEventListener>();

    public void Raise()
    {
        foreach(GameEventListener listener in allListeners)
        {
            listener.OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        allListeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {
        allListeners.Remove(listener);
    }
}