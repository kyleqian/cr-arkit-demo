using System.IO;
using UnityEngine;

public static class UtilitiesCR
{
    static public Sprite LoadNewSprite(string path, float pixelsPerUnit = 100.0f)
    {
        Texture2D texture = LoadTexture(path);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), pixelsPerUnit);
    }

    static Texture2D LoadTexture(string path)
    {
        Texture2D tex2D;
        byte[] fileData;
        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
            tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (tex2D.LoadImage(fileData))           // Load the imagedata into the texture (size is set automatically)
            {
                return tex2D;                 // If data = readable -> return texture
            }
        }
        return null;                     // Return null if load failed
    }
}
