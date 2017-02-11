using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Strikeforce;

namespace Strikeforce
{
    public class OptionsMenu : Menu
    {
        protected override void Awake()
        {
            // Hookup back button first
            Button backButton = GlobalAssets.GetChildComponentWithTag<Button>(gameObject, Tags.BUTTON);
            AddButtonHandler(backButton, BACK);

            base.Awake();
        }

        protected override void SetButtonNames()
        {
            
        }
    }
}