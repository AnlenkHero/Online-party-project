using Photon.Pun;
using UnityEngine;

public class TauntMenuController : MonoBehaviour
{
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private Animator animator;
    [SerializeField] private TauntButton buttonPrefab;
    [SerializeField] private Transform buttonsParent;
    public GameObject canvasGameObject;
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void Start()
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.StartsWith("Taunt_"))
            {
                CreateButtonForAnimation(clip.name);
            }
        }
    }

    private void CreateButtonForAnimation(string animationName)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonsParent);
        var tauntName = animationName.Replace("Taunt_", string.Empty);
        buttonObj.SetData(tauntName, () =>
        {
            if (thirdPersonController.grounded)
            {
                thirdPersonController.isAnimationPlaying = true;
                _photonView.RPC(nameof(PlayAnimation), RpcTarget.All, animationName);
                thirdPersonController.HideTauntMenu();
            }
        });
    }

    [PunRPC]
    private void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
}