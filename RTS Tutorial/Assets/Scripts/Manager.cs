using UnityEngine;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using RTS;

public class Manager : MonoBehaviour {

    public static string defaultSaveFolderName = "SavedGames";

    protected static JsonSerializer GetJsonSerializer()
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        return serializer;
    }

    protected static void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}