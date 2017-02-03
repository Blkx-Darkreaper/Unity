using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strikeforce
{
    public class Selectable : Body
    {
        public Texture2D BuildImage { get; set; }
        public float BuildTime { get; set; }
        public int Cost { get; set; }
        public int SellValue { get; set; }
        public string[] Actions { get; set; }
        protected bool isSelected { get; set; }
        public Bounds SelectionBounds { get; protected set; }
        public Player Owner { get; set; }
        private List<Material> oldMaterials { get; set; }

        protected override void Awake()
        {
            base.Awake();

            SelectionBounds = GlobalAssets.InvalidBounds;
            //UpdateBounds();
        }

        protected virtual void Start()
        {
            SetOwnership();
        }

        protected virtual void OnGUI()
        {
            if (isSelected == false)
            {
                return;
            }

            DrawSelection();
        }

        protected virtual void Update() { }

        public virtual void SetSpawner(Structure spawner)
        {
        }

        protected void SetOwnership()
        {
            Owner = GetComponentInParent<Player>();
            if (Owner == null)
            {
                //Debug.Log(string.Format("{0} has no owner", entityName));
                return;
            }

            //Debug.Log(string.Format("{0} belongs to {1}", entityName, player.username));

            SetTeamColour();
        }

        protected void SetTeamColour()
        {
            FactionColour[] allTeamColours = GetComponentsInChildren<FactionColour>();
            foreach (FactionColour teamColour in allTeamColours)
            {
                if (Owner == null)
                {
                    teamColour.GetComponent<Renderer>().material.color = GameManager.Singleton.DefaultColour;
                    continue;
                }

                teamColour.GetComponent<Renderer>().material.color = Owner.Colour;
            }
        }

        public bool IsOwnedByPlayer(Player player)
        {
            if (Owner == null)
            {
                return false;
            }

            return Owner.Equals(player);
        }

        public virtual void SetSelection(bool selected)
        {
            isSelected = selected;
            if (selected == false)
            {
                return;
            }

            this.playingArea = Hud.GetPlayingArea();
        }

        private void DrawSelection()
        {
            GUI.skin = GlobalAssets.SelectionBoxSkin;
            Rect selectionBox = GameManager.CalculateSelectionBox(SelectionBounds, playingArea);

            GUI.BeginGroup(playingArea);
            DrawSelectionBox(selectionBox);
            GUI.EndGroup();
        }

        protected virtual void DrawSelectionBox(Rect selectionBox)
        {
            GUI.Box(selectionBox, string.Empty);
        }

        public void UpdateBounds()
        {
            Bounds updatedBounds = new Bounds(transform.position, Vector3.zero);
            GameObject meshes = transform.Find(BodyProperties.MESHES).gameObject;
            Renderer[] allRenderers = meshes.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                bool enabled = renderer.enabled;
                if (enabled == false)
                {
                    continue;
                }

                updatedBounds.Encapsulate(renderer.bounds);
            }

            SelectionBounds = updatedBounds;
        }

        public virtual void PerformAction(string actionToPerform) { }

        public virtual void MouseClick(GameObject hitGameObject, Vector3 hitPoint, Player player)
        {
        //    if (isSelected == false)
        //    {
        //        return;
        //    }
        //    if (hitGameObject == null)
        //    {
        //        return;
        //    }
        //    bool isGround = hitGameObject.CompareTag(Tags.GROUND);
        //    if (isGround == true)
        //    {
        //        return;
        //    }

        //    Destructible hitEntity = hitGameObject.GetComponentInParent<Destructible>();
        //    if (hitEntity == null)
        //    {
        //        return;
        //    }
        //    if (hitEntity == this)
        //    {
        //        return;
        //    }

        //    bool readyToAttack = IsAbleToAttack();
        //    if (readyToAttack == false)
        //    {
        //        ChangeSelection(hitEntity, player);
        //        return;
        //    }

        //    if (hitEntity.MaxHitPoints == 0)
        //    {
        //        ChangeSelection(hitEntity, player);
        //        return;
        //    }

        //    Player hitEntityOwner = hitEntity.Owner;
        //    if (hitEntityOwner != null)
        //    {
        //        bool samePlayer = Owner.PlayerId == hitEntityOwner.PlayerId;
        //        if (samePlayer == true)
        //        {
        //            ChangeSelection(hitEntity, player);
        //            return;
        //        }
        //    }

        //    SetAttackTarget(hitEntity);
        }

        private void ChangeSelection(Selectable otherEntity, Player player)
        {
            if (otherEntity == this)
            {
                return;
            }

            SetSelection(false);
            if (player.SelectedEntity != null)
            {
                player.SelectedEntity.SetSelection(false);
            }

            player.SelectedEntity = otherEntity;
            playingArea = Hud.GetPlayingArea();
            otherEntity.SetSelection(true);
        }

        public virtual void SetHoverState(GameObject gameObjectUnderMouse)
        {
        //    if (Owner == null)
        //    {
        //        return;
        //    }
        //    if (Owner.IsNPC == true)
        //    {
        //        return;
        //    }
        //    if (isSelected == false)
        //    {
        //        return;
        //    }

        //    bool isGround = gameObjectUnderMouse.CompareTag(Tags.GROUND);
        //    if (isGround == true)
        //    {
        //        return;
        //    }

        //    Owner.PlayerHud.SetCursorState(CursorState.select);

        //    bool canAttack = IsAbleToAttack();
        //    if (canAttack == false)
        //    {
        //        return;
        //    }

        //    Destructible entityUnderMouse = gameObjectUnderMouse.GetComponentInParent<Destructible>();
        //    if (entityUnderMouse == null)
        //    {
        //        return;
        //    }

        //    if (entityUnderMouse.MaxHitPoints == 0)
        //    {
        //        return;
        //    }

        //    Player entityOwner = entityUnderMouse.Owner;
        //    if (entityOwner != null)
        //    {
        //        bool samePlayer = Owner.PlayerId == entityOwner.PlayerId;
        //        if (samePlayer == true)
        //        {
        //            return;
        //        }
        //    }

        //    Owner.PlayerHud.SetCursorState(CursorState.attack);
        }

        public void SetTransparencyMaterial(Material transparencyMaterial, bool saveCurrentMaterial)
        {
            if (saveCurrentMaterial == true)
            {
                oldMaterials.Clear();
            }

            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                if (saveCurrentMaterial == true)
                {
                    oldMaterials.Add(renderer.material);
                }

                renderer.material = transparencyMaterial;
            }
        }

        public void RestoreMaterials()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            int totalRenderers = allRenderers.Length;
            if (oldMaterials.Count != totalRenderers)
            {
                return;
            }

            for (int i = 0; i < totalRenderers; i++)
            {
                allRenderers[i].material = oldMaterials[i];
            }
        }
    }
}