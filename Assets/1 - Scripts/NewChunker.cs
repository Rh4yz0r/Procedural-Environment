using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NewChunker
{
    public static Texture2D[] ChunkTexture(Texture2D texture2D, out Vector3[] chunkOffsets)
    {
        var chunks = ChunkTexture(new TextureMapData(texture2D), out var offsets);
        chunkOffsets = offsets;
        return chunks;
    }
    
    public static Texture2D[] ChunkTexture(TextureMapData textureMapData, out Vector3[] chunkOffsets)
    {
        int maxChunkWidth = 256;
        int maxChunkHeight = 256;
        
        bool isWidthFound = false;
        bool isHeightFound = false;
        int widthChunkAmount = 1;
        int heightChunkAmount = 1;
        
        while (!isWidthFound && !isHeightFound)
        {
            if (textureMapData.Width / widthChunkAmount <= maxChunkWidth) isWidthFound = true;
            else widthChunkAmount++;

            if (textureMapData.Height / heightChunkAmount <= maxChunkHeight) isHeightFound = true;
            else heightChunkAmount++;
        }

        int chunkWidth = (textureMapData.Width / widthChunkAmount) + 2;
        int chunkHeight = (textureMapData.Height / heightChunkAmount) + 2;
        int chunkAmount = widthChunkAmount * heightChunkAmount;

        TextureFormat chunkTextureFormat = TextureFormat.R16;
        int chunkMipCount = -1;
        bool chunkLinear = false;

        Texture2D[] chunks = new Texture2D[chunkAmount];
        Vector3[] offsets = new Vector3[chunkAmount];

        for (int i = 0, chunkY = 0; chunkY < heightChunkAmount; chunkY++)
        {
            for (int chunkX = 0; chunkX < widthChunkAmount; chunkX++, i++)
            {
                chunks[i] = new Texture2D(chunkWidth, chunkHeight, chunkTextureFormat, chunkMipCount, chunkLinear){ name = $"Section: {chunkX},{chunkY}" };
                var chunkPixels = new Color[chunkWidth*chunkHeight];
                for (int chunkPixelIndex = 0; chunkPixelIndex < chunkPixels.Length; chunkPixelIndex++)
                {
                    chunkPixels[chunkPixelIndex] = Color.black;
                }

                for (int chunkPixelY = 0; chunkPixelY < chunkHeight; chunkPixelY++)
                {
                    for (int chunkPixelX = 0; chunkPixelX < chunkWidth; chunkPixelX++)
                    {
                        var originalTexturePixelX = chunkPixelX + chunkX * (chunkWidth-2) - chunkX; //<-------------- De -cx zorgt dat de randen hetzelfde blijven, maar je verliest er wel pixels mee
                        var originalTexturePixelY = chunkPixelY + chunkY * (chunkHeight-2) - chunkY;
                        
                        int pixelListIndex;
                        int iX = chunkPixelX;
                        int iY = chunkPixelY;
                        
                        /*pixelListIndex = (chunkPixelX +1) + (chunkPixelY+1) * chunkWidth;*/

                        if (chunkX == 0) iX = chunkPixelX + 1;
                        if (chunkY == 0) iY = chunkPixelY + 1;

                        pixelListIndex = iX + (iY * chunkWidth);
                        
                        try
                        {
                            chunkPixels[pixelListIndex] =
                                textureMapData.Pixels[originalTexturePixelX, originalTexturePixelY];
                        }
                        catch
                        {
                            /*ignored*/
                        }
                    }
                }

                offsets[i] = new Vector3(chunkX*(chunkWidth-2) - chunkX, 0, chunkY*(chunkHeight-2) - chunkY);
                chunks[i].SetPixels(chunkPixels);
                chunks[i].Apply();
            }
        }

        chunkOffsets = offsets;
        return chunks;
    }

    public static Texture2D[] NewChunkTexture(Texture2D texture2D, out Vector3[] chunkOffsets)
    {
        var chunks = NewChunkTexture(new TextureMapData(texture2D), out var offsets);
        chunkOffsets = offsets;
        return chunks;
    }
    
    public static Texture2D[] NewChunkTexture(TextureMapData textureMapData, out Vector3[] chunkOffsets)
    {
        #region Create Chunk Grid
        
        int maxChunkWidth = 128;
        int maxChunkHeight = 128;
        
        int originalWidth = textureMapData.Width;
        int originalHeight = textureMapData.Height;
        
        bool isWidthFound = false;
        bool isHeightFound = false;

        Texture2D[,] chunkGrid;
        int chunkGridXSize = 1;
        int chunkGridYSize = 1;
        int chunkTotalAmount;

        Vector3[,] offsetGrid;
        
        while (!isWidthFound && !isHeightFound)
        {
            if (originalWidth / chunkGridXSize <= maxChunkWidth) isWidthFound = true;
            else chunkGridXSize++;

            if (originalHeight / chunkGridYSize <= maxChunkHeight) isHeightFound = true;
            else chunkGridYSize++;
        }

        chunkTotalAmount = chunkGridXSize * chunkGridYSize;
        chunkGrid = new Texture2D[chunkGridXSize, chunkGridYSize];
        offsetGrid = new Vector3[chunkGridXSize, chunkGridYSize];
        
        #endregion

        #region Create Chunks
        
        int newWidth = textureMapData.Width / chunkGridXSize;
        int newHeight = textureMapData.Height / chunkGridYSize;

        int newExtendedWidth = newWidth + 1;
        int newExtendedHeight = newHeight + 1; //<--------- increase for increase overlap? 2 overlaps by 1

        TextureFormat textureFormat = textureMapData.Texture2D.format;
        int mipCount = -1;
        bool linear = false;
        TextureWrapMode wrapMode = textureMapData.Texture2D.wrapMode;

        for (int chunkGridYIndex = 0; chunkGridYIndex < chunkGridYSize; chunkGridYIndex++)
        {
            for (int chunkGridXIndex = 0; chunkGridXIndex < chunkGridXSize; chunkGridXIndex++)
            {
                Texture2D chunk = new Texture2D(newExtendedWidth, newExtendedHeight, textureFormat, mipCount, linear){name = $"Chunk: {chunkGridXIndex},{chunkGridYIndex}", wrapMode = wrapMode};
                FinalTextureGenerator.SetTextureToColor(chunk, Color.black);
                TextureMapData chunkMapData = new TextureMapData(chunk);

                Color[,] newPixels = new Color[chunkMapData.Width, chunkMapData.Height];

                int xLength = newExtendedWidth;
                int yLength = newExtendedHeight;

                if (chunkGridXIndex == 0 || chunkGridXIndex == chunkGridXSize - 1) xLength--;
                if (chunkGridYIndex == 0 || chunkGridYIndex == chunkGridYSize - 1) yLength--;

                for (int y = 0; y < yLength; y++)
                {
                    for (int x = 0; x < xLength; x++)
                    {
                        int chunkStartX = x;
                        int chunkStartY = y;
                        
                        int oriStartX = x + (chunkGridXIndex * newWidth);
                        int oriStartY = y + (chunkGridYIndex * newHeight);

                        if (chunkGridXIndex == 0) chunkStartX++;
                        if (chunkGridYIndex == 0) chunkStartY++;
                        
                        if (chunkGridXIndex != 0) oriStartX--;
                        if (chunkGridYIndex != 0) oriStartY--;

                        newPixels[chunkStartX, chunkStartY] = textureMapData.Pixels[oriStartX, oriStartY];
                    }
                }

                chunkMapData.Pixels = newPixels;
                chunkGrid[chunkGridXIndex, chunkGridYIndex] = chunkMapData.Texture2D;
                offsetGrid[chunkGridXIndex, chunkGridYIndex] = new Vector3(chunkGridXIndex * newWidth, 0, chunkGridYIndex * newHeight);
            }
        }
        
        #endregion

        #region Convert Grids

        Texture2D[] chunkArray = new Texture2D[chunkTotalAmount];
        Vector3[] offsetArray = new Vector3[chunkTotalAmount];
        
        for (int y = 0; y < chunkGridYSize; y++)
        {
            for (int x = 0; x < chunkGridXSize; x++)
            {
                int listIndex = x + (y * chunkGridYSize);

                chunkArray[listIndex] = chunkGrid[x, y];
                offsetArray[listIndex] = offsetGrid[x, y];
            }
        }

        #endregion

        chunkOffsets = offsetArray;
        return chunkArray;
    }
}
