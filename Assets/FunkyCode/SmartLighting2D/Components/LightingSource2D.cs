using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum LightingSourceTextureSize {px2048, px1024, px512, px256, px128};
public enum LightRenderingOrder {Default, Depth, YAxis};
public enum LightingLayer {Layer1, Layer2, Layer3, Layer4, Layer5, Layer6};


[System.Serializable]
public class LayerSetting {
	public LightRenderingOrder renderingOrder = LightRenderingOrder.Default;
	public LightingLayer layerID = LightingLayer.Layer1;
}

[ExecuteInEditMode]
public class LightingSource2D : MonoBehaviour {
	
	public enum LightSprite {Default, Custom};

	
	//public LightingMaskLayer lightingMaskLayer = LightingMaskLayer.Layer1;
	//public LightingCollisionLayer lightingCollisionLayer = LightingCollisionLayer.Layer1;

	public bool eventHandling = false;

	public Color lightColor = new Color(.5f,.5f, .5f, 1);
	public float lightAlpha = 1f;
	public float lightSize = 5f;
	public float lightCoreSize = 0.5f;
	public LightingSourceTextureSize textureSize = LightingSourceTextureSize.px1024;
	public bool enableCollisions = true;
	public bool rotationEnabled = false;

	public int layerCount = 1;
	public LayerSetting[] layerSetting = new LayerSetting[8];

	public bool additive = false;
	public float additive_alpha = 0.25f;

	public bool drawInsideCollider = false;

	private bool inScreen = false;

	public LightSprite lightSprite = LightSprite.Default;

	public LightingBuffer2D buffer = null;

	public Sprite sprite;
	private Material material;

	/////
	private Vector3 updatePosition = Vector3.zero;
	private Color updateColor = Color.white;
	private float updateRotation = 0;
	private float updateSize = 0;
	private float updateAlpha = 0.5f;
	
	public bool update = true;

	public bool move = false;
	public Vector3 movePosition = Vector3.zero;

	//public bool staticUpdated = false; // Not Necessary

	public float occlusionSize = 15;

	public static Sprite defaultSprite = null;
	static public Sprite GetDefaultSprite() {
		if (defaultSprite == null) {
			defaultSprite = Resources.Load <Sprite> ("Sprites/gfx_light");
		}
		return(defaultSprite);
	}

	public static List<LightingSource2D> list = new List<LightingSource2D>();

	public void SetPosition(Vector3 position) {
		movePosition = position;
		move = true;
	}

	public void OnEnable() {
		list.Add(this);
	}

	public void OnDisable() {
		list.Remove(this);

		///// Free Buffer!
		FBOManager.FreeBuffer(buffer);
		buffer = null;
		inScreen = false;
	}

	static public List<LightingSource2D> GetList() {
		return(list);
	}

	public bool InCamera() {
		return(Vector2.Distance(transform.position, Camera.main.transform.position) < Mathf.Sqrt((Camera.main.orthographicSize * 2f) * (Camera.main.orthographicSize* 2f)) + lightSize );
	}

	void Start () {
		SetMaterial ();

		for(int i = 0; i < layerCount; i++) {
			if (layerSetting[i] == null) {
				layerSetting[i] = new LayerSetting();
				layerSetting[i].layerID = LightingLayer.Layer1;
			}
		}
	}

	public void SetMaterial() {
		GetMaterial();

		update = true;
	}

	public Sprite GetSprite() {
		if (sprite == null) {
			sprite = GetDefaultSprite();
		}
		return(sprite);
	}
		
	public Material GetMaterial() {
		if (material == null) {
			material = new Material (Shader.Find (Max2D.shaderPath + "Particles/Multiply"));
			material.mainTexture = GetSprite().texture;
		}
		return(material);
	}

	void CheckIfUpdateNeeded() {
		if (updatePosition != transform.position) {
			updatePosition = transform.position;

			update = true;
		}

		if (updateRotation != transform.rotation.eulerAngles.z) {
			updateRotation = transform.rotation.eulerAngles.z;

			update = true;
		}

		if (updateSize != lightSize) {
			updateSize = lightSize;

			update = true;
		}

		if (updateColor.Equals(lightColor) == false) {
			updateColor = lightColor;
		}

		if (updateAlpha != lightAlpha) {
			updateAlpha = lightAlpha;
		}

		if (move == true) {
			if (updatePosition != movePosition) {
				updatePosition = movePosition;

				transform.position = movePosition;

				update = true;
			}
			move = false;
		}
	}

	void Update() {
		CheckIfUpdateNeeded();

		LightingManager2D manager = LightingManager2D.Get();
		bool disabled = manager.disableEngine;

		if (InCamera()) {
			if (update == true) {
				if (inScreen == false) {
					buffer = FBOManager.PullBuffer (LightingManager2D.GetTextureSize(textureSize), this);

					update = false;
					if (buffer != null) {
						if (disabled == false) {
							buffer.bufferCamera.enabled = true; // //UpdateLightBuffer(True)
							buffer.bufferCamera.orthographicSize = lightSize;
						}
					}
					//Debug.Log(3);

					inScreen = true;
				} else {
					update = false;
					if (buffer != null) {
						if (disabled == false) {
							buffer.bufferCamera.enabled = true; // //UpdateLightBuffer(True)
							buffer.bufferCamera.orthographicSize = lightSize;
						}
					}
				}
			} else {
				if (buffer != null) {
				//	Debug.Log(1);
					
				} else {
					buffer = FBOManager.PullBuffer (LightingManager2D.GetTextureSize(textureSize), this);

					update = false;
					if (buffer != null) {
						if (disabled == false) {
							buffer.bufferCamera.enabled = true; // //UpdateLightBuffer(True)
							buffer.bufferCamera.orthographicSize = lightSize;
						}
					}
					
				
					inScreen = true;

					//Debug.Log(4);
				}
			}
		} else {
			///// Free Buffer!
			if (buffer != null) {
				FBOManager.FreeBuffer(buffer);
				buffer = null;
			}
			inScreen = false;
		}



		
		if (eventHandling) {
			Vector2D zero = Vector2D.Zero();	
			float lightSizeSquared = Mathf.Sqrt(lightSize * lightSize + lightSize * lightSize);
	
			List<LightCollision2D> collisions = new List<LightCollision2D>();

			foreach (LightingCollider2D id in LightingCollider2D.GetList()) {
				if (id.colliderType == LightingCollider2D.ColliderType.None) {
					continue;
				}
				if (LightingManager2D.culling && Vector2.Distance(id.transform.position, transform.position) > id.GetCullingDistance() + lightSize) {
					continue;
				}
				LightCollision2D collision = new LightCollision2D();
				collision.lightSource = this;
				collision.collider = id;
				collision.pointsColliding = id.GetShadowCollisionPolygons()[0].ToWorldSpace(id.transform).ToOffset (new Vector2D (-transform.position)).pointsList;
				collisions.Add(collision);
			}
			
			foreach (LightingCollider2D id in LightingCollider2D.GetList()) {
				if (LightingManager2D.culling && Vector2.Distance(id.transform.position, transform.position) > id.GetCullingDistance() + lightSize) {
					continue;
				}

				if (id.colliderType == LightingCollider2D.ColliderType.None) {
					continue;
				}

				List<Polygon2D> polygons = id.GetShadowCollisionPolygons();

				if (polygons.Count < 1) {
					continue;
				}

				foreach(Polygon2D polygon in polygons) {
					Polygon2D poly = polygon.ToWorldSpace (id.gameObject.transform);
					poly.ToOffsetItself (new Vector2D (-transform.position));

					if (poly.PointInPoly (zero)) {
						continue;
					}

					Vector2D vA, pA, vB, pB;
					float angleA, angleB;
					foreach (Pair2D p in Pair2D.GetList(poly.pointsList, false)) {
						vA = p.A.Copy();
						pA = p.A.Copy();

						vB = p.B.Copy();
						pB = p.B.Copy();

						angleA = (float)Vector2D.Atan2 (vA, zero);
						angleB = (float)Vector2D.Atan2 (vB, zero);

						vA.Push (angleA, lightSizeSquared);
						pA.Push (angleA - Mathf.Deg2Rad * occlusionSize, lightSizeSquared);

						vB.Push (angleB, lightSizeSquared);
						pB.Push (angleB + Mathf.Deg2Rad * occlusionSize, lightSizeSquared);

						if (eventHandling) {
							Polygon2D triPoly = new Polygon2D();
							triPoly.AddPoint(p.A);
							triPoly.AddPoint(p.B);
							triPoly.AddPoint(pB);
							triPoly.AddPoint(pA);

							foreach(LightCollision2D col in new List<LightCollision2D>(collisions)) {
								if (col.collider == id) {
									continue;
								}
								foreach(Vector2D point in new List<Vector2D>(col.pointsColliding)) {
									if (triPoly.PointInPoly(point)) {
										col.pointsColliding.Remove(point);
									}
								}
								if (col.pointsColliding.Count < 1) {
									collisions.Remove(col);
								}
							}
						}
					}

					LightingManager2D.LightingDebug.shadowGenerations ++;	
				}
			}

			if (collisions.Count > 0) {
				foreach(LightCollision2D collision in collisions) {
					collision.collider.CollisionEvent(collision);
				}
			}
		}

	}




























































}
