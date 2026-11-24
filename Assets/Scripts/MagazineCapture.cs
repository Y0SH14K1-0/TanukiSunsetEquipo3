using UnityEngine;
using System.IO;

public class MagazineCapture : MonoBehaviour
{
    public void CaptureMagazine()
    {
        string dataPath = GetPath();
        Texture2D magazineTex = ScreenCapture.CaptureScreenshotAsTexture();
        //magazineTex.Resize(1280, 720);
        File.WriteAllBytes(dataPath, magazineTex.EncodeToPNG());
    }

    public Texture2D GetMagazineTexture()
    {
        string dataPath = GetPath();
        Texture2D result =  new Texture2D(1280, 720);
        if (File.Exists(dataPath))
        {
            ImageConversion.LoadImage(result, File.ReadAllBytes(dataPath));
        }
        return result;
    }

    public bool MagazineTextureExists()
    {
        return File.Exists(GetPath());
    }

    public string GetPath()
    {
        return Application.persistentDataPath + "/SaveData/" + SaveSystem.GetUserFolderName() + "/png-tanuki-sunset-magazine.png";
    }
}
