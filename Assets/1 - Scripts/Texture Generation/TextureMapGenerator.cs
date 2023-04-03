using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*public static class TextureMapGenerator
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
                List<float> redValues = new List<float>();

                float pixelRedValue = heightMapData.Pixels[x, y].r;
                
                try { redValues.Add(heightMapData.Pixels[x - 1, y + 1].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x + 1, y + 1].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x    , y + 1].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x - 1, y    ].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x + 1, y    ].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x    , y    ].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x - 1, y - 1].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x + 1, y - 1].r - pixelRedValue); }catch{ /*ignored#1# }
                try { redValues.Add(heightMapData.Pixels[x    , y - 1].r - pixelRedValue); }catch{ /*ignored#1# }
                
                float averageValue = AverageValue(redValues.ToArray());
                
                float highestValue = MaxValue(redValues.ToArray());
                
                /*Color topLeftHP;
                Color topRightHP;
                Color topMiddleHP;
                Color middleLeftHP;
                Color middleRightHP;
                Color middleMiddleHP;
                Color bottomLeftHP;
                Color bottomRightHP;
                Color bottomMiddleHP;
                
                try { topLeftHP = heightMapData.Pixels[x-1, y+1]; }catch{ topLeftHP = Color.black; }
                try { topRightHP = heightMapData.Pixels[x+1, y+1]; }catch{ topRightHP = Color.black; }
                try { topMiddleHP = heightMapData.Pixels[x, y+1]; }catch{ topMiddleHP = Color.black; }
                try { middleLeftHP = heightMapData.Pixels[x-1, y]; }catch{ middleLeftHP = Color.black; }
                try { middleRightHP = heightMapData.Pixels[x+1, y]; }catch{ middleRightHP = Color.black; }
                try { middleMiddleHP = heightMapData.Pixels[x, y]; }catch{ middleMiddleHP = Color.black; }
                try { bottomLeftHP = heightMapData.Pixels[x-1, y-1]; }catch{ bottomLeftHP = Color.black; }
                try { bottomRightHP = heightMapData.Pixels[x+1, y-1]; }catch{ bottomRightHP = Color.black; }
                try { bottomMiddleHP = heightMapData.Pixels[x, y-1]; }catch{ bottomMiddleHP = Color.black; }

                /*float highestValue = MaxValue(new float[] { 
                    leftHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    rightHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    topHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    bottomHeightPixel.r - heightMapData.Pixels[x, y].r });
                
                newPixels[x, y] = new Color(highestValue*10, 0, 0, 0);#2#
                
                /*float averageValue = AverageValue(new float[] { 
                    leftHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    rightHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    topHeightPixel.r - heightMapData.Pixels[x, y].r, 
                    bottomHeightPixel.r - heightMapData.Pixels[x, y].r });#2#

                float averageValue = AverageValue(new float[]
                {
                    topLeftHP.r - middleMiddleHP.r,
                    topRightHP.r - middleMiddleHP.r,
                    topMiddleHP.r - middleMiddleHP.r,
                    middleLeftHP.r - middleMiddleHP.r,
                    middleRightHP.r - middleMiddleHP.r,
                    bottomLeftHP.r - middleMiddleHP.r,
                    bottomRightHP.r - middleMiddleHP.r,
                    bottomMiddleHP.r - middleMiddleHP.r
                });#1#

                //newPixels[x, y] = new Color(averageValue*500, 0, 0, 0);
                newPixels[x, y] = new Color(highestValue*10, 0, 0, 0);
            }
        }

        slopeMapData.Pixels = newPixels;
        return slopeMapData;
    }

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
                    /*Color topLeftPixel;
                    Color topRightPixel;
                    Color topMiddlePixel;
                    Color middleLeftPixel;
                    Color middleRightPixel;
                    Color middleMiddlePixel;
                    Color bottomLeftPixel;
                    Color bottomRightPixel;
                    Color bottomMiddlePixel;
                
                    try { topLeftPixel = mapData.Pixels[x-1, y+1]; }catch{ topLeftPixel = Color.black; }
                    try { topRightPixel = mapData.Pixels[x+1, y+1]; }catch{ topRightPixel = Color.black; }
                    try { topMiddlePixel = mapData.Pixels[x, y+1]; }catch{ topMiddlePixel = Color.black; }
                    try { middleLeftPixel = mapData.Pixels[x-1, y]; }catch{ middleLeftPixel = Color.black; }
                    try { middleRightPixel = mapData.Pixels[x+1, y]; }catch{ middleRightPixel = Color.black; }
                    try { middleMiddlePixel = mapData.Pixels[x, y]; }catch{ middleMiddlePixel = Color.black; }
                    try { bottomLeftPixel = mapData.Pixels[x-1, y-1]; }catch{ bottomLeftPixel = Color.black; }
                    try { bottomRightPixel = mapData.Pixels[x+1, y-1]; }catch{ bottomRightPixel = Color.black; }
                    try { bottomMiddlePixel = mapData.Pixels[x, y-1]; }catch{ bottomMiddlePixel = Color.black; }
                
                    float averageValue = AverageValue(new float[]
                    {
                        topLeftPixel.r,
                        topRightPixel.r,
                        topMiddlePixel.r,
                        middleLeftPixel.r,
                        middleRightPixel.r,
                        middleMiddlePixel.r,
                        bottomLeftPixel.r,
                        bottomRightPixel.r,
                        bottomMiddlePixel.r
                    });#1#
                    
                    List<float> redValues = new List<float>();

                    try { redValues.Add(mapData.Pixels[x - 1, y + 1].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x + 1, y + 1].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x    , y + 1].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x - 1, y    ].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x + 1, y    ].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x    , y    ].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x - 1, y - 1].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x + 1, y - 1].r); }catch{ /*ignored#1# }
                    try { redValues.Add(mapData.Pixels[x    , y - 1].r); }catch{ /*ignored#1# }

                    float averageValue = AverageValue(redValues.ToArray());
                    
                    newPixels[x, y] = new Color(averageValue, 0, 0, 0);
                }
            }
            
            mapData.Pixels = newPixels;
            index++;
        }
        
        EditorUtility.ClearProgressBar();
        return mapData;
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
                //newPixels[x, y] = Color.green;
                //newPixels[x, y] = CombineColors(new Color[] { Color.green/5, (Color.grey * slopeMapData.Pixels[x, y].r)*10 });
                newPixels[x, y] = Color.Lerp(Color.green, Color.grey, slopeMapData.Pixels[x, y].r*10);
            }
        }

        textureMapData.Pixels = newPixels;
        return textureMapData;
    }

    public static Texture2D[] ChunkTexture(Texture2D texture2D)
    {
        return ChunkTexture(new TextureMapData(texture2D));
    }
    
    public static Texture2D[] ChunkTexture(TextureMapData textureMapData)
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
                        var oriX = px + cx * sectionWidth;
                        var oriY = py + cy * sectionHeight;
                        
                        var l = px + py * sectionWidth;
                        chunkPixels[l] = textureMapData.Pixels[oriX, oriY];
                    }
                }
                
                chunks[i].SetPixels(chunkPixels);
                chunks[i].Apply();
            }
        }
        
        return chunks;
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

    private static float AverageValue(float[] floatArray)
    {
        var sum = 0f;
        for (int i = 0; i < floatArray.Length; i++)
        {
            sum += floatArray[i];
        }
        //Debug.Log(sum / floatArray.Length);
        return sum / floatArray.Length;
    }
}*/
