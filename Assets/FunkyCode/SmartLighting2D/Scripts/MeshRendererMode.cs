using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshRendererMode : MonoBehaviour
{
    public static MeshRendererMode instance;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public static MeshRendererMode Get() {
        if (instance != null) {
			return(instance);
		}

		foreach(MeshRendererMode meshModeObject in Object.FindObjectsOfType(typeof(MeshRendererMode))) {
			instance = meshModeObject;
			return(instance);
		}

        if (instance == null) {
            GameObject meshRendererMode = new GameObject("MeshRendererMode");
            instance = meshRendererMode.AddComponent<MeshRendererMode>();
          
            LightingManager2D manager = LightingManager2D.Get();
            meshRendererMode.transform.parent = manager.transform;

            instance.meshRenderer = meshRendererMode.AddComponent<MeshRenderer>();
            instance.meshRenderer.material = manager.mainBuffer.material;
           
            instance.meshRenderer.sortingLayerName = manager.sortingLayerName;
            instance.meshRenderer.sortingLayerID = manager.sortingLayerID;
            instance.meshRenderer.sortingOrder = manager.sortingLayerOrder;

            instance.LateUpdate();

            instance.meshFilter = meshRendererMode.AddComponent<MeshFilter>();
            instance.meshFilter.mesh = LightingManager2D.GetRenderMesh();
        }

        return(instance);
    }

    void LateUpdate()
    {
        if (Camera.main == null) {
            return;
        }
        Vector3 position = Camera.main.transform.position;
        position.z += Camera.main.nearClipPlane + 0.1f;

        transform.position = position;
        transform.rotation = Camera.main.transform.rotation;
        transform.localScale = LightingManager2D.Render_Size();
    }
}
