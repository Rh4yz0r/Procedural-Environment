using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FinalTextureGenerator
{
    #region Maps

    public static TextureMapData GenerateSlopeMap(Texture2D heightMap)
    {
        Texture2D slopeMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.R16, heightMap.mipmapCount, false);
        SetTextureToColor(slopeMap, Color.black);

        var heightMapData = new TextureMapData(heightMap);
        var slopeMapData = new TextureMapData(slopeMap);
        Color[,] newPixels = new Color[slopeMapData.Width, slopeMapData.Height];
        
        for (int y = 0; y < heightMapData.Height; y++)
        {
            for (int x = 0; x < heightMapData.Width; x++)
            {
                List<float> redValues = new List<float>();

                float pixelRedValue = heightMapData.Pixels[x, y].r;
                
                try { redValues.Add(heightMapData.Pixels[x - 1, y + 1].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x + 1, y + 1].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x    , y + 1].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x - 1, y    ].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x + 1, y    ].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x    , y    ].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x - 1, y - 1].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x + 1, y - 1].r - pixelRedValue); }catch{ /*ignored*/ }
                try { redValues.Add(heightMapData.Pixels[x    , y - 1].r - pixelRedValue); }catch{ /*ignored*/ }
                
                //float averageValue = MathExtensions.AverageValue(redValues.ToArray());
                //newPixels[x, y] = new Color(averageValue*500, 0, 0, 0);
                
                //Check de value van de vorige chunk of de volgende chunk.
                //Als niet 9 in array, breek dan uit deze functie, aangezien we aan de zijkant zitten

                float highestValue = MathExtensions.MaxValue(redValues.ToArray());
                newPixels[x, y] = new Color(highestValue*10, 0, 0, 0);
            }
        }

        slopeMapData.Pixels = newPixels;
        return slopeMapData;
    }

    public static TextureMapData GenerateTextureMap(Texture2D heightMap, Texture2D slopeMap)
    {
        Texture2D textureMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.RGBA32, heightMap.mipmapCount, false);
        SetTextureToColor(textureMap, Color.black);
        
        var slopeMapData = new TextureMapData(slopeMap);
        var textureMapData = new TextureMapData(textureMap);
        Color[,] newPixels = new Color[textureMapData.Width, textureMapData.Height];

        for (int x = 0; x < slopeMapData.Width; x++)
        {
            for (int y = 0; y < slopeMapData.Height; y++)
            {
                newPixels[x, y] = Color.Lerp(Color.green, Color.grey, slopeMapData.Pixels[x, y].r*10);
            }
        }

        textureMapData.Pixels = newPixels;
        return textureMapData;
    }
    
    #endregion

    #region Extensions

    public static TextureMapData SmoothMap(Texture2D map, int iterations = 1)
    {
        var mapData = new TextureMapData(map);
        
        int index = 0;
        while (index < iterations)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Spawning Trees",
                    $"Busy Spawning Trees: {index}/{iterations}",
                    (float)index / iterations))
            {
                Debug.LogWarning($"Aborted smoothing at {index} iterations!");
                break;
            }
            
            Color[,] newPixels = new Color[mapData.Width, mapData.Height];
            
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    List<float> redValues = new List<float>();

                    try { redValues.Add(mapData.Pixels[x - 1, y + 1].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x + 1, y + 1].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x    , y + 1].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x - 1, y    ].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x + 1, y    ].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x    , y    ].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x - 1, y - 1].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x + 1, y - 1].r); }catch{ /*ignored*/ }
                    try { redValues.Add(mapData.Pixels[x    , y - 1].r); }catch{ /*ignored*/ }

                    float averageValue = MathExtensions.AverageValue(redValues.ToArray());
                    
                    newPixels[x, y] = new Color(averageValue, 0, 0, 0);
                }
            }
            
            mapData.Pixels = newPixels;
            index++;
        }
        
        EditorUtility.ClearProgressBar();
        return mapData;
    }

    public static Texture2D[] ChunkTexture(Texture2D texture2D, out Vector3[] chunkOffsets)
    {
        var chunks = ChunkTexture(new TextureMapData(texture2D), out var offsets);
        chunkOffsets = offsets;
        return chunks;
    }
    
    public static Texture2D[] ChunkTexture(TextureMapData textureMapData, out Vector3[] chunkOffsets)
    {
        bool widthFound = false;
        bool heightFound = false;
        int widthSections = 1;
        int heightSections = 1;
        
        while (!widthFound && !heightFound)
        {
            if (textureMapData.Width / widthSections <= 256) widthFound = true;
            else widthSections++;

            if (textureMapData.Height / heightSections <= 256) heightFound = true;
            else heightSections++;
        }

        int sectionWidth = textureMapData.Width / widthSections;
        int sectionHeight = textureMapData.Height / heightSections;
        int sectionAmount = widthSections * heightSections;

        Texture2D[] chunks = new Texture2D[sectionAmount];
        Vector3[] offsets = new Vector3[sectionAmount];
        
        for (int i = 0, cy = 0; cy < heightSections; cy++)
        {
            for (int cx = 0; cx < widthSections; cx++, i++)
            {
                chunks[i] = new Texture2D(sectionWidth, sectionHeight, TextureFormat.R16, -1, false){ name = $"Section: {cx},{cy}" };
                var chunkPixels = new Color[sectionWidth*sectionHeight];

                for (int py = 0; py < sectionHeight; py++)
                {
                    for (int px = 0; px < sectionWidth; px++)
                    {
                        var oriX = px + cx * sectionWidth - cx; //<-------------- De -cx zorgt dat de randen hetzelfde blijven, maar je verliest er wel pixels mee
                        var oriY = py + cy * sectionHeight - cy;
                        
                        var l = px + py * sectionWidth;
                        chunkPixels[l] = textureMapData.Pixels[oriX, oriY];
                    }
                }

                offsets[i] = new Vector3(cx*sectionWidth - cx, 0, cy*sectionHeight - cy);
                chunks[i].SetPixels(chunkPixels);
                chunks[i].Apply();
            }
        }

        chunkOffsets = offsets;
        return chunks;
    }
    
    public static Texture2D GenerateBlackTexture(int resolution, TextureFormat textureFormat, int mipCount = -1, bool linear = false)
    {
        Texture2D texture2D = new Texture2D(resolution, resolution, textureFormat, mipCount, linear);
        SetTextureToColor(texture2D, Color.black);
        return texture2D;
    }
    
    public static Color CombineColors(params Color[] aColors)
    {
        Color result = new Color(0,0,0,0);
        foreach(Color c in aColors)
        {
            result += c;
        }
        result /= aColors.Length;
        return result;
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

    #endregion
}
