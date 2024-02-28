using Photon.Pun;
using UnityEngine;

public class TauntMenuController : MonoBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController;
    public Animator animator;
    public TauntButton buttonPrefab;
    public Transform buttonsParent;
    private PhotonView _photonView;

    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.StartsWith("Taunt_"))
            {
                CreateButtonForAnimation(clip.name);
            }
        }
    }

    void CreateButtonForAnimation(string animationName)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonsParent);
        buttonObj.SetData(animationName, () =>
        {
            thirdPersonController._isAnimationPlaying = true;
            _photonView.RPC("PlayAnimation", RpcTarget.All, animationName);
            thirdPersonController.HideTauntMenu();
        });
    }

    [PunRPC]
    void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
}