using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MylaFlower
{
    public static class SpriteLoader
    {
        public static Dictionary<string, Texture2D> Textures;

        public static Texture2D GetTexture(string imgName)
        {
            return Textures[imgName];
        }

        public static void LoadTextures()
        {
            Textures = new();
            foreach (string resource in typeof(SpriteLoader).Assembly.GetManifestResourceNames().Where(name => name.ToLower().EndsWith(".png")))
            {
                try
                {
                    using Stream stream = typeof(SpriteLoader).Assembly.GetManifestResourceStream(resource);
                    if (stream == null)
                    {
                        continue;
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    // Create texture from bytes
                    Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
                    tex.LoadImage(buffer, true);
                    tex.filterMode = FilterMode.Point;

                    string resName = Path.GetFileNameWithoutExtension(resource);
                    string[] pieces = resName.Split('.');
                    resName = pieces[pieces.Length - 1];

                    // Create sprite from texture
                    Textures.Add(resName, tex);
                }
                catch (Exception e)
                {
                    MylaFlower.instance.LogError(e);
                }
            }
        }

    }
}
