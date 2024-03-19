using System.Collections;
using UnityEngine;

public class DissolveMaterialController : MonoBehaviour
{
    public Material dissolveMaterial;
    private Material _instanceMaterial;

    void Start()
    {
        _instanceMaterial = new Material(dissolveMaterial);
        
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = _instanceMaterial;
        }
        else
        {
            Debug.LogError("MeshRenderer component not found.");
        }
    }

    public void StartDissolveEffect(float target, float duration)
    {
        if (_instanceMaterial == null)
        {
            Debug.LogError("Dissolve material instance is not created.");
            return;
        }

        UpdateDissolveEffect(target, duration);
    }

    private void UpdateDissolveEffect(float target, float duration)
    {
        StartCoroutine(DissolveRoutine(target, duration));
    }

    private IEnumerator DissolveRoutine(float target, float duration)
    {
        float startAmount = _instanceMaterial.GetFloat("_Dissolve");
        float time = 0.0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float dissolveAmount = Mathf.Lerp(startAmount, target, time / duration);
            _instanceMaterial.SetFloat("_Dissolve", dissolveAmount);
            yield return null;
        }
    }
}