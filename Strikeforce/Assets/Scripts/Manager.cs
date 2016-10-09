using UnityEngine;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Strikeforce;

public class Manager : NetworkBehaviour {

    public static string defaultSaveFolderName = "SavedGames";

    protected static void SetJsonSerializerIgnoreNulls()
    {
        SetJsonSerializerNullValueHandling(NullValueHandling.Ignore);
    }

    private static void SetJsonSerializerNullValueHandling(NullValueHandling value)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = value;
    }

    protected static void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}