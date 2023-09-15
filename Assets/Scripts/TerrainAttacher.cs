using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainAttacher : MonoBehaviour {

    // Use this for initialization
    GameObject go;
	void Start() {
        GameObject go = this.gameObject;
        // Set Names 
        int N = 36;
        int E = 26;
        foreach(Transform child in go.transform)
        {
            if(E == 46)
            {
                N++;
                E = 26;
            }
            child.name = N.ToString() + "N0" + E.ToString() + "E";
            E++;
        }
        // Set Positions
        int posx = 0;
        int posz = 0;
        foreach (Transform child in go.transform)
        {
            if (posx == 20000)
            {
                posz += 1000;
                posx = 0;
            }
            child.transform.position = new Vector3(posx, 0, posz);
            posx += 1000;
        }
    }
}
