using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureMapData
{
#if UNITY_EDITOR
    private const bool ENABLE_CONSOLE_LOGS = false; //Enabling increases load times drastically.
#endif
    
    public Texture2D Texture2D;

    private Color[,] _pixels;
    public Color[,] Pixels
    {
        get => _pixels;
        set
        {
            _pixels = value;

            var pixels = new Color[value.Length];
            int index = 0;
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    pixels[index] = value[x, y];
                    index++;
                }
            }
            
            Texture2D.SetPixels(0, 0, Width, Height, pixels);
        }
    }
    public float[,] RedValues;
    public float[,] GreenValues;
    public float[,] BlueValues;
    public float[,] AlphaValues;

    public int Width;
    public int Height;

    public TextureMapData() { }

    public TextureMapData(Texture2D texture2D, Color[,] pixels, float[,] redValues, float[,] greenValues, float[,] blueValues, float[,] alphaValues)
    {
        this.Texture2D = texture2D;
        this.Pixels = pixels;
        this.RedValues = redValues;
        this.GreenValues = greenValues;
        this.BlueValues = blueValues;
        this.AlphaValues = alphaValues;
        this.Width = texture2D.width;
        this.Height = texture2D.height;
    }

    public TextureMapData(Texture2D texture2D)
    {
        this.Texture2D = texture2D;
        this.Pixels = TextureToPixelGrid(texture2D, out var r, out var g, out var b, out var a);
        this.RedValues = r;
        this.GreenValues = g;
        this.BlueValues = b;
        this.AlphaValues = a;
        this.Width = texture2D.width;
        this.Height = texture2D.height;
    }
    
    public static Color[,] TextureToPixelGrid(Texture2D texture2D)
    {
        return PixelListToGrid(texture2D.GetPixels(), texture2D.width, texture2D.height, out var r, out var g, out var b, out var a);
    }
    
    public static Color[,] TextureToPixelGrid(Texture2D texture2D, out float[,] redValues, out float[,] greenValues, out float[,] blueValues, out float[,] alphaValues)
    {
        Color[,] pixelGrid = PixelListToGrid(texture2D.GetPixels(), texture2D.width, texture2D.height, out var r, out var g, out var b, out var a);
        redValues = r;
        greenValues = g;
        blueValues = b;
        alphaValues = a;
        return pixelGrid;
    }
    
    private static Color[,] PixelListToGrid(Color[] pixelList, int gridWidth, int gridHeight, out float[,] redValues, out float[,] greenValues, out float[,] blueValues, out float[,] alphaValues)
    {
        Color[,] pixelGrid = new Color[gridWidth, gridHeight];
        float[,] redGrid = new float[gridWidth, gridHeight];
        float[,] greenGrid = new float[gridWidth, gridHeight];
        float[,] blueGrid = new float[gridWidth, gridHeight];
        float[,] alphaGrid = new float[gridWidth, gridHeight];

        int x = 0;
        int y = 0;
        int layerLength = gridWidth;

        for (int index = 0; index < pixelList.Length; index++)
        {
            if (index > layerLength-1)
            {
                x = index % layerLength;
                y = index / layerLength;
            }
            else
            {
                x = index;
                y = 0;
            }
#if UNITY_EDITOR
            if(ENABLE_CONSOLE_LOGS) Debug.Log($"Adding {x+1} of layer {y+1}");
#endif
            pixelGrid[x, y] = pixelList[index];
            redGrid[x, y] = pixelGrid[x, y].r;
            greenGrid[x, y] = pixelGrid[x, y].g;
            blueGrid[x, y] = pixelGrid[x, y].b;
            alphaGrid[x, y] = pixelGrid[x, y].a;
        }

        if (pixelList.Length < pixelGrid.Length)
        {
#if UNITY_EDITOR
            if(ENABLE_CONSOLE_LOGS) Debug.LogError("Couldn't create square texture, supplied pixel list is too small. Filling rest with black.");
#endif
            for (int index = x+1; index < pixelGrid.Length; index++)
            {
                if (index > layerLength-1)
                {
                    x = index % layerLength;
                    y = index / layerLength;
                }
                else
                {
                    x = index;
                    y = 0;
                }
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log($"Adding {x+1} of layer {y+1}");
#endif
                pixelGrid[x, y] = Color.black;
                redGrid[x, y] = pixelGrid[x, y].r;
                greenGrid[x, y] = pixelGrid[x, y].g;
                blueGrid[x, y] = pixelGrid[x, y].b;
                alphaGrid[x, y] = pixelGrid[x, y].a;
            }
        }

        redValues = redGrid;
        greenValues = greenGrid;
        blueValues = blueGrid;
        alphaValues = alphaGrid;
        return pixelGrid;
    }
}
