using Photon.Pun;

public interface IInteractable
{
    public bool IsInteractable { get; set; }
    public string InteractionPrompt => "Press E to interact";
    public string Description { get; }
    public void Interact(PhotonView view);
    public void ShowInfo();
    public void HideInfo();
}