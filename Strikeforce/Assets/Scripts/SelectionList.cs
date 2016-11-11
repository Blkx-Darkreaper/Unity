using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public static class SelectionList
    {
        private static string[] allEntries = { };
        private static int gridIndex = 0;
        private static float scrollValue = 0f;
        private static float listX, listY, listWidth, listHeight;
        private static float rowHeight = 25, sliderWidth = 10, sliderPadding = 5;

        public static void AddAllEntries(string[] entries)
        {
            allEntries = entries;
        }

        public static string GetCurrentEntry()
        {
            string entry = string.Empty;
            if (gridIndex < 0)
            {
                return entry;
            }
            if (gridIndex >= allEntries.Length)
            {
                return entry;
            }

            entry = allEntries[gridIndex];
            return entry;
        }

        public static void SetCurrentEntryToFirstMatch(string entry)
        {
            gridIndex = -1;
            for (int i = 0; i < allEntries.Length; i++)
            {
                string nextEntry = allEntries[i];
                if (!nextEntry.Equals(entry))
                {
                    continue;
                }

                gridIndex = i;
                return;
            }
        }

        public static bool Contains(string entry)
        {
            foreach (string entryToCheck in allEntries)
            {
                bool match = entryToCheck.Equals(entry);
                if (match == false)
                {
                    continue;
                }

                return match;
            }

            return false;
        }

        public static bool MouseDoubleClick()
        {
            Event capturedEvent = Event.current;
            if (capturedEvent == null)
            {
                return false;
            }
            if (capturedEvent.isMouse == false)
            {
                return false;
            }
            if (capturedEvent.type != EventType.MouseDown)
            {
                return false;
            }
            if (capturedEvent.clickCount != 2)
            {
                return false;
            }

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;  // Invert screen position

            float selectionHeight = allEntries.Length * rowHeight;
            float selectionWidth = listWidth;
            if (selectionHeight > listHeight)
            {
                selectionWidth -= (sliderWidth + 2 * sliderPadding);
            }

            bool mouseInSelection = new Rect(listX, listY, selectionWidth, listHeight).Contains(mousePosition);
            if (mouseInSelection == false)
            {
                return false;
            }

            return true;
        }

        public static void Draw(float x, float y, float width, float height)
        {
            listX = x;
            listY = y;
            listWidth = width;
            listHeight = height;
            DrawBox();
        }

        public static void Draw(float x, float y, float width, float height, GUISkin skin)
        {
            GUI.skin = skin;
            Draw(x, y, width, height);
        }

        public static void Draw(Rect drawArea)
        {
            listX = drawArea.x;
            listY = drawArea.y;
            listWidth = drawArea.width;
            listHeight = drawArea.height;
            DrawBox();
        }

        public static void Draw(Rect drawArea, GUISkin skin)
        {
            GUI.skin = skin;
            Draw(drawArea);
        }

        public static void DrawBox()
        {
            float selectionWidth = listWidth;
            float selectionHeight = allEntries.Length * rowHeight;

            GUI.BeginGroup(new Rect(listX, listY, listWidth, listHeight));

            if (selectionHeight > listHeight)
            {
                selectionWidth -= (sliderWidth + 2 * sliderPadding);
            }

            float x = 0;
            float y = 0;
            GUI.Box(new Rect(x, y, selectionWidth, listHeight), string.Empty);

            if (selectionHeight > listHeight)
            {
                float sliderX = selectionWidth + sliderPadding;
                float sliderMin = 0f;
                float sliderMax = selectionHeight - listHeight;
                scrollValue = GUI.VerticalSlider(new Rect(sliderX, y, sliderWidth, listHeight), scrollValue, sliderMin, sliderMax);
                scrollValue -= Input.GetAxis(KeyMappings.SCROLL_WHEEL) * rowHeight;
                scrollValue = Mathf.Clamp(scrollValue, 0f, sliderMax);
            }

            y = 1;
            float height = listHeight - 2;
            GUI.BeginGroup(new Rect(x, y, listWidth, height));

            y = 0 - scrollValue;
            int count = 1;
            gridIndex = GUI.SelectionGrid(new Rect(x, y, selectionWidth, selectionHeight), gridIndex, allEntries, count);

            GUI.EndGroup();
            GUI.EndGroup();
        }
    }
}