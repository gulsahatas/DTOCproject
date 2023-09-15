using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CreateTerrain : MonoBehaviour
{
    int width = 1024;
    int height = 1024;
    float scale = 1;

    Terrain terrain;
    TerrainData terrainData;

    float[,] heights;
    string terrainName;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        terrainData.heightmapResolution = width + 1;
        terrainName = terrain.name;
        
        GenerateTerrain();

        //splat();
        //terrain.Flush();

    }

    public void GenerateTerrain()
    {
        heights = terrainData.GetHeights(0, 0, width+1, height+1);

        var reader = new StreamReader(File.OpenRead("Assets\\Data\\Terrain CSV Files\\"+terrainName + ".csv"));
        // skip first 7 line the header part of the csv file
        for (int i = 0; i < 7; i++)
        {
            var line = reader.ReadLine();
        }
        string line_min = reader.ReadLine();
        string[] line_min_splited = line_min.Split(',');
        int min_elevation_value = int.Parse(line_min_splited[1]);
        string line_max = reader.ReadLine();
        string[] line_max_splited = line_max.Split(',');
        int max_elevation_value = int.Parse(line_max_splited[1]);
        int diff = max_elevation_value - min_elevation_value;
        float terrain_height = 150;
        string[] values = new string[height];
        
        for (int x = 0; x <= width; x++)
        {
            string line = reader.ReadLine();
            values = line.Split(',');
            for (int y = 0; y <= height; y++)
            {
                // normalize values according to Turkey's highest point 
                heights[x, y] = float.Parse(values[y]) * 0.00019f;
            }
        }
        terrainData.size = new Vector3(1000, terrain_height, 1000);
        terrainData.SetHeights(0, 0, heights);
    }

    public void splat()
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float y1 = (float)y / (float)terrainData.alphamapHeight;
                float x1 = (float)x / (float)terrainData.alphamapWidth;
                float terrainHeight = terrainData.GetHeight(Mathf.RoundToInt(y1 * terrainData.heightmapHeight), Mathf.RoundToInt(x1 * terrainData.heightmapWidth));
                float[] splatWeights = new float[terrainData.alphamapLayers];

                if (terrainHeight <= 10f)
                {
                    splatWeights[0] = 1;
                    splatWeights[1] = 0;
                }
                else if (terrainHeight > 10f && terrainHeight <= 20f)
                {
                    splatWeights[0] = 0.8f;
                    splatWeights[1] = 0.2f;
                }
                else if (terrainHeight > 20f && terrainHeight <= 30f)
                {
                    splatWeights[0] = 0.6f;
                    splatWeights[1] = 0.4f;
                }
                else if (terrainHeight > 30f && terrainHeight <= 40f)
                {
                    splatWeights[0] = 0.4f;
                    splatWeights[1] = 0.6f;
                }
                else if (terrainHeight > 40f && terrainHeight <= 49f)
                {
                    splatWeights[0] = 0.2f;
                    splatWeights[1] = 0.8f;
                }
                else
                {
                    splatWeights[1] = 1;
                    splatWeights[0] = 0;
                }

                float zval = splatWeights[0] + splatWeights[1];
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    splatWeights[i] /= zval;

                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}
