using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player singleton;
    public string username;
    public bool isNPC;
    public Squad selectedSquad;
    public Army army;

    #region Unity
    public void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        }
        if (singleton != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion Unity

    #region Squad Selection and Orders
    public bool IsOwnedByPlayer(Squad squad)
    {
        return army.allSquads.Contains(squad);
    }

    public void LeftMouseClick()
    {
        // Check if mouse on screen
        bool mouseInBounds = CursorManager.singleton.IsMouseInPlayArea();
        if (mouseInBounds == false)
        {
            return;
        }

        Vector3 hitPoint = CursorManager.GetHitPoint();
        if (hitPoint == CursorManager.invalidPoint)
        {
            return;
        }

        GameObject hitObject = CursorManager.GetHitGameObject();
        if (hitObject == null)
        {
            return;
        }

        Deselect();

        bool isGround = hitObject.CompareTag(Global.Tags.GROUND);
        if (isGround == true)
        {
            return;
        }

        Model model = hitObject.GetComponentInParent<Model>();
        if (model == null)
        {
            return;
        }

        Squad squad = model.squad;
        if (squad == null)
        {
            return;
        }

        SelectSquad(squad);
    }

    #region Left Mouse Click
    protected void Deselect()
    {
        if (selectedSquad == null)
        {
            return;
        }

        selectedSquad.SetSelection(false);
        selectedSquad = null;
    }

    protected void SelectSquad(Squad squadToSelect)
    {
        if (selectedSquad != null)
        {
            Deselect();
        }

        this.selectedSquad = squadToSelect;
        squadToSelect.SetSelection(true);
    }
    #endregion Left Mouse Click

    public void RightMouseClick()
    {
        bool mouseInBounds = CursorManager.singleton.IsMouseInPlayArea();
        if (mouseInBounds == false)
        {
            return;
        }

        if (selectedSquad == null)
        {
            return;
        }

        // Get clicked point
        Vector3 hitPoint = CursorManager.GetHitPoint();
        if (hitPoint == CursorManager.invalidPoint)
        {
            return;
        }

        selectedSquad.MoveToLocation(hitPoint);
    }

    public void MouseOver()
    {

    }
    #endregion Squad Selection and Orders
}