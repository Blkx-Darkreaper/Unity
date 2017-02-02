using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class AssetManager : MonoBehaviour
    {
        public Texture2D healthy, damaged, critical;

        public void Awake()
        {
            // Load all game assets
            LoadHealthBarTextures();
        }

        protected void LoadHealthBarTextures()
        {
            GlobalAssets.HealthBarTextures.Healthy = healthy;
            GlobalAssets.HealthBarTextures.Damaged = damaged;
            GlobalAssets.HealthBarTextures.Critical = critical;
        }
    }
}