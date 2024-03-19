using System;
using System.Linq;
using Photon.Pun;


public class ReviveAnimation : AnimationManager
{
    public static event Action OnPlayerRevived;

    [PunRPC]
    public override void AnimationStartInteraction()
    {
        StartCutscene();
    }

    [PunRPC]
    protected override void CallbackAfterAllAnimations()
    {
        photonView.RPC(nameof(EndCutscene), RpcTarget.All);
        photonView.RPC(nameof(RevivePlayers), RpcTarget.All);
    }

    [PunRPC]
    private void StartCutscene()
    {
        CutsceneManager.OnCutsceneStarted?.Invoke();
    }

    [PunRPC]
    private void EndCutscene()
    {
        CutsceneManager.OnCutsceneEnded?.Invoke();
    }

    [PunRPC]
    private void RevivePlayers()
    {
        OnPlayerRevived?.Invoke();
    }
}