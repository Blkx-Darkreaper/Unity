using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Manager : NetworkBehaviour
    {
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
}