using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class Entity : NetworkBehaviour
    {
        public int entityId { get; set; }
        public Level currentLevel;
        public struct EntityProperties
        {
            public const string ID = "Id";
            public const string NAME = "Name";
        }

        protected virtual void Awake()
        {
            if(currentLevel == null)
            {
                throw new UnityException(string.Format("{0} cannot be registered without having an associated level", name));
            }

            GameEntityManager.singleton.RegisterEntity(this);
        }

        protected virtual void DestroyEntity()
        {
            GameEntityManager.singleton.RemoveEntity(this);

            Debug.Log(string.Format("{0} has been destroyed", name));
        }
    }
}