using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColliderLineRenderer2D : MonoBehaviour {
	public Color color = Color.white;
	public float lineWidth = 1;

	private bool edgeCollider2D = false; // For Edge Collider

	private Polygon2D polygon = null;
	private Mesh mesh = null;
	private float lineWidthSet = 1;
	private Material material;

	const float lineOffset = -0.01f;

	void Start () {
		edgeCollider2D = (GetComponent<EdgeCollider2D>() != null);
		
		Max2D.Check();
		material = new Material(Max2D.lineMaterial);

		GenerateMesh();
		Draw();
	}
	
	public void Update() {
		if (lineWidth != lineWidthSet) {
			if (lineWidth < 0.01f) {
				lineWidth = 0.01f;
			}
			GenerateMesh();
		}

		Draw();
	}

	public Polygon2D GetPolygon() {
		if (polygon == null) {
			List<Polygon2D> polygons = Polygon2DList.CreateFromGameObject (gameObject);
			if (polygons.Count > 0) {
				polygon = polygons[0];
			} else {
				Debug.LogWarning("SmartUtilities: CollideLineRenderer object is missing Collider Component");
			}
		}
		return(polygon);
	}

	public void GenerateMesh() {
		lineWidthSet = lineWidth;
		
		if (GetPolygon() != null) {
			mesh = Max2DMesh.GeneratePolygon2DMeshNew(transform, GetPolygon(), lineOffset, lineWidth, edgeCollider2D == false);
		}
	}

	public void Draw() {
		//material.color = color;
		material.SetColor ("_Emission", color);

		if (mesh == null) {
			return;
		}

		Max2DMesh.Draw(mesh, transform, material);
	}
}