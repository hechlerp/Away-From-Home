using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingTile {
	public Sprite sprite;
	public List<Polygon2D> polygons = new List<Polygon2D>();
	Mesh tileMesh = null;

	public void GeneratePhysicsShape() {
		int count = sprite.GetPhysicsShapeCount();

		for(int i = 0; i < count; i++) {
			List<Vector2> points = new List<Vector2>();
			sprite.GetPhysicsShape(0, points);
			
			Polygon2D newPolygon = new Polygon2D();
			foreach(Vector2 p in points) {
				newPolygon.AddPoint(p);
			}
			polygons.Add(newPolygon);
		}
	}

	public Mesh GetTileDynamicMesh() {
		if (tileMesh == null) {
			if (polygons.Count > 0) {
				Polygon2D tilePoly = polygons[0];
				tileMesh = tilePoly.CreateMesh(Vector2.zero, Vector2.zero);	
				//Polygon2D.CreateFromRect(new Vector2(0.5f + 0.01f, 0.5f + 0.01f));
			}
		}
		return(tileMesh);
	}

	public static Mesh staticTileMesh = null;

	public static Mesh GetStaticTileMesh() {
		if (staticTileMesh == null) {
			Polygon2D tilePoly = Polygon2D.CreateFromRect(new Vector2(0.5f + 0.01f, 0.5f + 0.01f));
			staticTileMesh  = tilePoly.CreateMesh(Vector2.zero, Vector2.zero);	
		}
		return(staticTileMesh);
	}
}