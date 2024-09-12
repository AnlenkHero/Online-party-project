using UnityEngine;

public class PopUpHandler : MonoBehaviour
{
    public bool isPopUpShown;
    public Vector3 popUpOffset;
    public Sprite icon;

    public void ShowPopUp(string description, Transform parent)
    {
        if (isPopUpShown) return;

        PopupManager.ShowPanelAboveObject(parent, popUpOffset, description, icon);
        isPopUpShown = true;
    }

    public void HidePopUp()
    {
        PopupManager.HidePanel();
        isPopUpShown = false;
    }
}