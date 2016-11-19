using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Rank
    {
        public int Value { get; protected set; }

        [JsonConstructor]
        public Rank()
        {
        }
    }
}