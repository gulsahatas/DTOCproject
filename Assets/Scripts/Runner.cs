using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour {
    GameObject go;
	// Use this for initialization
	void Start () {
        go = this.gameObject;
        int count = 0;
        foreach (Transform child in go.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<JSON2GameObject>();
            count++;
        }
    }
}
