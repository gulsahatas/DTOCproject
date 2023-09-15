using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

//using MIConvexHull;

public class JSON2GameObject : MonoBehaviour {
    // Arguments
    public int latitude;
    public int longitude;
    public float buildingHeight = 0.5f;
    public float randomConstant = 0.5f;
    public bool textures;

    private float currentHeightBuilding;
    // Class Variables
    GameObject anchorBuilding;
    Terrain mainTerrain;
    // Data which read from JSON file will be kept in this array 
    List<Vector3[]>[] buildingsMap;
    GameObject go;
    string nameObject;
    void Start () {
        go = this.gameObject;
        nameObject = go.name;
        mainTerrain = GetComponent<Terrain>();
        anchorBuilding = new GameObject(nameObject + "Buildings");
        anchorBuilding.transform.position = new Vector3(mainTerrain.transform.position.x, mainTerrain.transform.position.y , mainTerrain.transform.position.z);
        anchorBuilding.transform.SetParent(mainTerrain.transform);
        ImportJSON();
        CreateAreas();
        AlignAnchorBuilding();
    }
    void CreateTheArea(List<Vector3[]> area, int xOffset, int zOffset)
    {
        int countOfPointLists = 0;  
        float cx, cz;
        int vertexCounter = 0;
        List<Vector3> listVertices = new List<Vector3>();
        List<int> listTriangles = new List<int>();
        
        foreach (Vector3[] points in area)
        {
            countOfPointLists++;
            int length = points.Length;
            // Calculate Center Point to sort vertices
            cx = 0;
            cz = 0;
            for (int i = 0; i < length; i++)
            {
                cx += points[i].x;
                cz += points[i].z;
            }
            cx = cx / length;
            cz = cz / length;
            // Find degree between point and center , then sort points as clockwise order. 
            Vector3[] basePoints = points.OrderBy(x => System.Math.Atan2(x.z - cz, x.x - cx)).ToArray();
            
            // Calculate building height with random extra
            currentHeightBuilding = buildingHeight + (float)(new System.Random().NextDouble() * randomConstant);
            Vector3 vec = new Vector3();
            foreach (Vector3 point in basePoints)
            {
                vec = new Vector3(point.x*1000,0,point.z*1000);
                vec.y = Terrain.activeTerrain.SampleHeight(vec)+0.00001f;
                listVertices.Add(vec);   
            }
            foreach (Vector3 point in basePoints)
            {
                vec = new Vector3(point.x * 1000, 0, point.z * 1000);
                vec.y = Terrain.activeTerrain.SampleHeight(vec) + currentHeightBuilding;
                listVertices.Add(vec);
            }
            listTriangles.AddRange(MakeTrianglesEar(basePoints.ToList(),vertexCounter));
            vertexCounter += (length*2);
            
            if (listTriangles.Count > 750 || countOfPointLists == area.Count)
            {
                // calculate position for new object
                Vector3 objectPosition = new Vector3(mainTerrain.transform.position.x + xOffset, mainTerrain.transform.position.y, mainTerrain.transform.position.z + zOffset);
                
                // Create New Object
                CreateAndConfigGameObject(listVertices,listTriangles,objectPosition,xOffset,zOffset);

                // Clear old containers
                vertexCounter = 0;
                listVertices.Clear();
                listTriangles.Clear();
            }
            
        }   
    }
    List<int> MakeTrianglesEar(List<Vector3> points,int vertexCounter)
    {
        List<int> listTriangles = new List<int>();
        int length = points.Count();
        listTriangles.AddRange(Assets.Scripts.EarClipping.TriangulateConcavePolygon(points, vertexCounter));
        for (int i = 0; i < length; i++) // walls lower triangles
        {
            if (i == length - 1)
            {
                listTriangles.Add(vertexCounter + length);
                listTriangles.Add(vertexCounter);
                listTriangles.Add(vertexCounter + i);
                break;
            }
            listTriangles.Add(vertexCounter + i + 1 + length);
            listTriangles.Add(vertexCounter + i + 1);
            listTriangles.Add(vertexCounter + i);
        }
        for (int i = 0; i < length; i++) // walls upper triangles
        {
            if (i == length - 1)
            {
                listTriangles.Add(vertexCounter + i);
                listTriangles.Add(vertexCounter + i + length);
                listTriangles.Add(vertexCounter + length);
                break;
            }
            listTriangles.Add(vertexCounter + i);
            listTriangles.Add(vertexCounter + i + length);
            listTriangles.Add(vertexCounter + i + 1 + length);
        }
        return listTriangles;
    }
    // Create GameObject and a mesh to attach it.
    // Set Position of the object
    void CreateAndConfigGameObject(List<Vector3> vertices, List<int> triangles, Vector3 position, float xOffset, float zOffset)
    {
        Debug.Log("Start");
        GameObject newObject = new GameObject("Latitude" + (latitude +xOffset*0.01).ToString() + "Longitude" + (longitude + zOffset * 0.01).ToString());
        Mesh mesh = new Mesh();
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        
        mesh = newObject.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles.ToArray(),0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        /**/
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = mesh.vertices[i];
        }
        mesh.uv = uvs;
        /**/
        Material mat;
        if (textures)
            mat = Resources.Load<Material>("Materials/HouseMaterial" + new System.Random().Next(1,4).ToString());
        else
            mat = Resources.Load<Material>("Materials/onlyMaterial");
        //mat.mainTextureScale = new Vector2((int)(vertices.Count / 7), (int)(currentHeightBuilding * 12f));
        newObject.GetComponent<MeshRenderer>().material = mat;
        UnityEditor.MeshUtility.Optimize(mesh);
        // Sets the position
        newObject.transform.position = new Vector3(0f,0f,0f);
        newObject.transform.SetParent(anchorBuilding.transform);
    }
    // Send each area to be created
    void CreateAreas()
    {
        int index = 0;
        foreach (List<Vector3[]> area in buildingsMap) {
            CreateTheArea(area,index/101,index%101);
            index++;
        }
    }

    // This function reads already created JSON file and transfer data to buildingsMap List
    void ImportJSON()
    {
        string jsonText = System.IO.File.ReadAllText("Assets/Data/JSON Files/Buildings/" + nameObject + ".json");
        buildingsMap = new List<Vector3[]>[10201];
        buildingsMap = JsonConvert.DeserializeObject<List<Vector3[]>[]>(jsonText);
    }

    //  Will be used to align position of gameObject with terrain
    void AlignAnchorBuilding()
    {
        //this.transform.position = new Vector3(0,0,0);
        anchorBuilding.transform.parent = this.transform;
    }
}