using UnityEngine;
using System.Collections;

public class SelectionHighlighter: MonoBehaviour 
{
    public Material mainMaterial;
    public Material selectionMaterial;

	void Update () 
    {
        int gameObjectCount = Monitor.allGameObjects.Length;
        for (uint i = 0; i < gameObjectCount; ++i)
        {
            GameObject sceneGameObject = Monitor.allGameObjects[i];
            ScriptableBehaviour scriptableBehaviour = sceneGameObject.GetComponent<ScriptableBehaviour>();
            if (scriptableBehaviour)
            {
                MeshRenderer meshRenderer = sceneGameObject.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    if(scriptableBehaviour.isSelected())
                    {
                        meshRenderer.material = selectionMaterial;
                    }
                    else
                    {
                        meshRenderer.material = mainMaterial;
                    }
                }
            }
        }
	}
};