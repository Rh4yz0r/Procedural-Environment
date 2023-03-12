using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureMapGenerator
{
    public static TextureMapData GenerateRandomHeightMap(int resolution)
    {
        
        
        
        return new TextureMapData();
    }
    
    public static TextureMapData GenerateSlopeMap(Texture2D heightMap)
    {
        Texture2D slopeMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.R16, heightMap.mipmapCount, false);
        SetTextureToColor(slopeMap, Color.black);

        var heightMapData = new TextureMapData(heightMap);
        var slopeMapData = new TextureMapData(slopeMap);
        Color[,] newPixels = new Color[slopeMapData.Width, slopeMapData.Height];

        for (int x = 0; x < heightMapData.Width; x++)
        {
            for (int y = 0; y < heightMapData.Height; y++)
            {
                Color leftHeightPixel= new Color(0, 0, 0, 0);
                Color rightHeightPixel = new Color(0, 0, 0, 0);
                Color topHeightPixel = new Color(0, 0, 0, 0);
                Color bottomHeightPixel = new Color(0, 0, 0, 0);

                if (x == 0)
                {
                    rightHeightPixel = heightMapData.Pixels[x + 1, y];
                }
                else if (x == heightMapData.Width - 1)
                {
                    leftHeightPixel = heightMapData.Pixels[x - 1, y];
                }
                else
                {
                    rightHeightPixel = heightMapData.Pixels[x + 1, y];
                    leftHeightPixel = heightMapData.Pixels[x - 1, y];
                }
                if (y == 0)
                {
                    bottomHeightPixel = heightMapData.Pixels[x, y + 1];
                }
                else if (y == heightMapData.Height - 1)
                {
                    topHeightPixel = heightMapData.Pixels[x, y- 1];
                }
                else
                {
                    topHeightPixel = heightMapData.Pixels[x, y- 1];
                    bottomHeightPixel = heightMapData.Pixels[x, y + 1];
                }

                float highestValue = MaxValue(new float[] { 
                    leftHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    rightHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    topHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    bottomHeightPixel.r - heightMapData.Pixels[x, y].r });

                newPixels[x, y] = new Color(highestValue*10, 0, 0, 0);
            }
        }

        slopeMapData.Pixels = newPixels;
        return slopeMapData;
    }

    public static Texture2D GenerateBlackTexture(int resolution, TextureFormat textureFormat, int mipCount = -1, bool linear = false)
    {
        Texture2D texture2D = new Texture2D(resolution, resolution, textureFormat, mipCount, linear);
        SetTextureToColor(texture2D, Color.black);
        return texture2D;
    }

    public static void SetTextureToColor(Texture2D texture2D, Color color)
    {
        Color fillColor = color;
        var fillColorArray = new Color[texture2D.width * texture2D.height];

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }

        texture2D.SetPixels(fillColorArray);
        texture2D.Apply();
    }
    
    private static float MaxValue (float[] floatArray)
    {
        var max = 0f;
        for (int i = 0; i < floatArray.Length; i++) 
        {
            if (floatArray[i] > max) {
                max = floatArray[i];
            }
        }
        //Debug.Log($"MAX: {max}");
        return max;
    }
}
