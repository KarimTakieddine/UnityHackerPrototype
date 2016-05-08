using UnityEngine;
using System.Collections;

public class Monitor : MonoBehaviour 
{
    public static GameObject[] allGameObjects { get; private set; }

	void Update () 
    {
        allGameObjects = FindObjectsOfType<GameObject>();
	}
};