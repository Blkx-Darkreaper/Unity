using System;
using UnityEngine;

public class Announcer : MonoBehaviour
{
    public struct GeneralActions
    {
        public const string pass = "Passed";
        public const string fail = "Failed";
        public const string score = "Scoring";
        public const string roll = "Rolled";
    }

    public struct ModelActions
    {
        public const string move = "Moved";
        public const string melee = "Attacked";
        public const string wound = "Wounded";
    }

    public struct SquadActions
    {
        public const string assault = "Is assaulting";
        public const string trapped = "Is trapped";
    }

    //#if !UNITY_EDITOR
    public static string myLog = "";
    private string output;
    private string stack;

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        // Remove callback when object goes out of scope
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }

    void OnGUI()
    {
        //if (!Application.isEditor) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
        {
            myLog = GUI.TextArea(new Rect(10, 10, Screen.width - 10, Screen.height - 10), myLog);
        }
    }
    //#endif

    public static void AnnounceAction(string action)
    {
        string output = $"{action}.";
        Debug.Log(output);
    }

    public static void AnnounceSquadAction(Squad subject, string action, Squad other)
    {
        string armyName = subject.army.name;
        string name = subject.name;
        string otherArmy = other.army.name;
        string otherName = other.name;
        string output = $"{armyName}'s {name} {action.ToLower()} {otherArmy}'s {otherName}.";
        Debug.Log(output);
    }

    public static void AnnounceSquadAction(Squad subject, string action)
    {
        string armyName = subject.army.name;
        string name = subject.name;
        string output = $"{armyName}'s {name} {action.ToLower()}.";
        Debug.Log(output);
    }

    public static void AnnounceModelAction(Model subject, string action)
    {
        string name = subject.name;
        string output = $"{name} {action.ToLower()}.";
        Debug.Log(output);
    }

    public static void AnnounceModelAction(Model subject, string action, Model other)
    {
        string name = subject.name;
        string otherName = other.name;
        string output = $"{name} {action.ToLower()} {otherName}.";
        Debug.Log(output);
    }
}