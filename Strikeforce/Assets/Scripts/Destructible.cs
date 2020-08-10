using UnityEngine;
using UnityEngine.Networking;

namespace Strikeforce
{
    public class Destructible : Selectable
    {
        public const int MAX_DAMAGE = 999;
        public int maxHitPoints;
        [HideInInspector]
        [SyncVar]
        public int currentHitPoints;
        protected float healthPercentage { get; set; }
        protected GUIStyle healthStyle { get; set; }
        private const int HEALTH_BAR_VERTICAL_OFFSET = 7;
        private const int HEALTH_BAR_HEIGHT = 5;
        protected string explosionName;
        public struct HealthStatus
        {
            public const string PRISTINE = "Pristine";
            public const string MARRED = "Marred";
            public const string DAMAGED = "Damaged";
            public const string CRITICALLY_DAMAGED = "Critically Damaged";
        }
        protected struct DestructibleProperties
        {
            public const string HIT_POINTS = "HitPoints";
        }

        protected override void Awake()
        {
            base.Awake();

            this.healthStyle = new GUIStyle();
            this.currentHitPoints = maxHitPoints;
        }

        protected override void Start()
        {
            base.Start();

            UpdateHealthPercentage();
        }

        protected void DrawHealthBar(Rect selectionBox)
        {
            DrawHealthBarWithLabel(selectionBox, string.Empty);
        }

        protected void DrawHealthBarWithLabel(Rect selectionBox, string label)
        {
            healthStyle.padding.top = -20;
            healthStyle.fontStyle = FontStyle.Bold;

            float x = selectionBox.x;
            float y = selectionBox.y - HEALTH_BAR_VERTICAL_OFFSET;
            float width = selectionBox.width * healthPercentage;
            float height = HEALTH_BAR_HEIGHT;
            GUI.Label(new Rect(x, y, width, height), label, healthStyle);
        }

        protected override void DrawSelectionBox(Rect selectionBox)
        {
            base.DrawSelectionBox(selectionBox);

            UpdateHealthPercentage();
            DrawHealthBar(selectionBox);
        }

        protected virtual void UpdateHealthPercentage()
        {
            UpdateHealthPercentage(0.65f, 0.35f);
        }

        protected virtual void UpdateHealthPercentage(float healthyThreshold, float damagedThreshold)
        {
            if (maxHitPoints == 0)
            {
                this.healthPercentage = 0f;
                return;
            }

            this.healthPercentage = (float)currentHitPoints / (float)maxHitPoints;

            if (healthPercentage > healthyThreshold)
            {
                this.healthStyle.normal.background = GlobalAssets.HealthBarTextures.Healthy;
                return;
            }
            if (healthPercentage > damagedThreshold)
            {
                this.healthStyle.normal.background = GlobalAssets.HealthBarTextures.Damaged;
                return;
            }

            this.healthStyle.normal.background = GlobalAssets.HealthBarTextures.Critical;
        }

        public virtual void TakeDamage(int amount, RaycastHit hit)
        {
            TakeDamage(amount);
        }

        public virtual void TakeDamage(int damage)
        {
            if (isServer == false)
            {
                return;
            }

            this.currentHitPoints -= damage;

            string ownersName = "Neutral";
            if (currentOwner != null)
            {
                ownersName = string.Format("{0}'s", currentOwner.playerId.ToString());
            }

            Debug.Log(string.Format("{0} {1} has taken {2} damage", ownersName, name, damage));

            _RenderFlashing flashComp = GetComponent<_RenderFlashing>();
            if (flashComp)
            {
                flashComp.Flash(Color.red, 5f);
            }

            if (currentHitPoints > 0)
            {
                return;
            }

            Explode();
            DestroyEntity();
        }

        protected void Explode()
        {
            GameObject explosion = GlobalAssets.GetMiscPrefab(explosionName);
            if (explosion == null)
            {
                return;
            }

            Instantiate(explosion, transform.position, transform.rotation);
        }

        protected override void DestroyEntity()
        {
            GameEntityManager.singleton.RemoveEntity(this);

            string ownersName = "Neutral";
            if (currentOwner != null)
            {
                ownersName = string.Format("{0}'s", currentOwner.playerId.ToString());
            }

            Debug.Log(string.Format("{0} {1} has been destroyed", ownersName, name));
        }
    }
}