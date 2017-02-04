using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class Entity : NetworkBehaviour
    {
        public int EntityId { get; set; }
        //public static string nameProperty { get { return ItemProperties.NAME; } }
        public struct EntityProperties
        {
            public const string ID = "Id";
            public const string NAME = "Name";
        }

        protected virtual void Awake()
        {
            GameManager.Singleton.RegisterEntity(this);
        }

        protected virtual void DestroyEntity()
        {
            GameManager.Singleton.DestroyEntity(this);

            Debug.Log(string.Format("{0} has been destroyed", name));
        }
    }
}