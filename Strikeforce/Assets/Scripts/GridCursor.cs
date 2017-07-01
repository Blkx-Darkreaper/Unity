using UnityEngine;
using System.Drawing;

namespace Strikeforce
{
    public class GridCursor : MonoBehaviour
    {
        protected Vector2 location { get; set; }
        protected Sprite sprite { get; set; }
        public Rectangle Bounds = new Rectangle();

        protected void Awake()
        {
            this.sprite = GetComponent<Sprite>();
        }

        public virtual void Move(float x, float z)
        {
            if (x == 0f)
            {
                if (z == 0f)
                {
                    return;
                }
            }

            int signX = (int)Mathf.Sign(x);
            x = signX;

            int signZ = (int)Mathf.Sign(z);
            z = signZ;

            GlobalAssets.KeepInBounds(Bounds, location.x, location.y, ref x, ref z);

            transform.Translate(x, 0, z);
            this.location = new Vector2(transform.position.x, transform.position.z);
        }

        public virtual void DefaultSize()
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        public void Resize(Vector2 size)
        {
            Resize(size.x, size.y);
        }

        public virtual void Resize(float width, float height)
        {
            float depth = transform.localScale.y;
            transform.localScale = new Vector3(width, depth, height);
        }

        public void CenterOnLocation(Vector2 location)
        {
            CenterOnLocation(location.x, location.y);
        }

        public virtual void CenterOnLocation(float x, float z)
        {
            float y = transform.position.y;
            transform.position = new Vector3(x, y, z);
        }

        public virtual void ExpandAndCenterAroundSelectable(Selectable selectable)
        {
            Bounds selectionBounds = selectable.SelectionBounds;

            float selectionX = selectable.transform.position.x;
            float selectionY = selectable.transform.position.z;
            //float selectionWidth = selectionBounds.width;
            //float selectionHeight = selectionBounds.height;
            float selectionWidth = selectable.transform.lossyScale.x;
            float selectionHeight = selectable.transform.lossyScale.z;

            // Keep from shrinking smaller than 1 tile
            selectionWidth = Mathf.Clamp(selectionWidth, 1, selectionWidth);
            selectionHeight = Mathf.Clamp(selectionHeight, 1, selectionHeight);

            CenterOnLocation(selectionX, selectionY);
            Resize(selectionWidth, selectionHeight);
        }
    }
}