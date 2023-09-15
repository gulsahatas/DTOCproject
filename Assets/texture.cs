using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class texture : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Terrain terrain = GetComponent<Terrain>();//Terrain.activeTerrain;

        TerrainData terrainData = terrain.terrainData;//Terrain.activeTerrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));
                //height range: 0-50;

                //float[] splatWeights = new float[terrainData.alphamapLayers];
                float[] splatWeights = new float[2];
                Debug.Log(terrainData.GetHeight(x,y));
                if (height <= 10f)//44f) //veya 43 ??
                {
                    splatWeights[0] = 1;
                    splatWeights[1] = 0;
                }

                else if (height >10f && height <= 20f)//45.5f)
                {
                    splatWeights[1] = 0.2f;
                    splatWeights[0] = 0.8f;
                }
                else if (height > 20f && height <= 30f)//47f)
                {
                    splatWeights[1] = 0.4f;
                    splatWeights[0] = 0.6f;
                }
                else if (height > 30f && height <= 40f)//48.5f)
                {
                    splatWeights[1] = 0.6f;
                    splatWeights[0] = 0.4f;
                }

                else //10, 50,,
                {
                    splatWeights[1] = 1;
                    splatWeights[0] = 0;
                }

                float z = splatWeights[0] + splatWeights[1];

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    splatWeights[i] /= z;

                    splatmapData[x, y, i] = splatWeights[i];

                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
	

}


