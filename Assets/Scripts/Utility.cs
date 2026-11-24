using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Utility
{
    public static void ReplaceAllShadersTexture(Shader shader, string propertyName, Texture newTexture)
    {
        List<Renderer> renderers = GameObject.FindObjectsOfType<Renderer>().ToList();
        renderers = renderers.Where(x => x.material.shader == shader).ToList();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetTexture(propertyName, newTexture);
        }
    }
}
