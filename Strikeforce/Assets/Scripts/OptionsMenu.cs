using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Strikeforce;
using System;

namespace Strikeforce
{
    public class OptionsMenu : Menu
    {
        protected override void SetButtonNames()
        {
            this.allButtonNames = new string[] { BACK };
        }

        protected override void SetButtonTextValues()
        {
            this.allButtonTextValues = new string[] { BACK };
        }

        protected override string[] GetMenuButtonNamesToAdd()
        {
            return new string[] { };
        }
    }
}