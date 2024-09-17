using Photon.Pun;

public interface IInteractable
{
    public bool IsInteractable { get; set; }
    public bool IsUiInteraction { get; set; }
    public string Description { get; }
    public void Interact(PhotonView view);
    public void ShowInfo();
    public void HideInfo();
}