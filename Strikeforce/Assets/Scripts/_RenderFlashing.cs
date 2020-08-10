using UnityEngine;

namespace Strikeforce
{
    public class _RenderFlashing : MonoBehaviour
    {
        protected MeshRenderer mesh { get; set; }
        protected bool isFlashing = false;
        protected float flashDuration { get; set; }

        protected void Awake()
        {
            this.mesh = GetComponentInChildren<MeshRenderer>();
        }

        protected virtual void Update()
        {
            FlashFade();
        }

        public virtual void Flash(Color colour, float duration)
        {
            mesh.material.color = colour;

            this.isFlashing = true;
            this.flashDuration = duration;
        }

        public virtual void FlashFade()
        {
            if (isFlashing == false)
            {
                return;
            }

            if (mesh.material.color == Color.clear)
            {
                isFlashing = false;
                return;
            }

            mesh.material.color = Color.Lerp(mesh.material.color, Color.clear, flashDuration * Time.deltaTime);
        }
    }
}