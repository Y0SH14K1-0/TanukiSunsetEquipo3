using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class MapLocalizationUpdater : MonoBehaviour
{
    public LocalizedTexture assetRef;
    public MeshRenderer meshRenderer;
/*     float counter = 0f; */

    private void Start()
    {
        StartCoroutine(UpdateMap());
    }

/*     private void Update()
    {
        if(counter >= 3f)
        {
            counter = 0f;
            StartCoroutine(UpdateMap());
        }
        counter += Time.deltaTime;
    } */

    public IEnumerator UpdateMap()
    {
        yield return  new WaitForSeconds(1f);
        
        var loadOperation = assetRef.LoadAssetAsync();

        yield return loadOperation; // Wait for loading.

        // Check the loading was successful.
        if (loadOperation.IsDone)
        {
            if (loadOperation.Result == null) yield break;
            meshRenderer.material.SetTexture("_MainTex", loadOperation.Result);
            //Debug.Log("Updating map");
        }
    }
}