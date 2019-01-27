using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlurObject {
	public Sprite sprite;
	public int blurSize;
	public int blurIterations;

	public BlurObject(Sprite image, int size, int iterations) {
		sprite = image;
		blurSize = size;
		blurIterations = iterations;
	}
}

[System.Serializable]
public class BlurManager {
	static public Dictionary<Sprite, BlurObject> dictionary = new Dictionary<Sprite, BlurObject>();

	static public Sprite RequestSprite(Sprite originalSprite, int blurSize, int blurIterations) {
		BlurObject blurObject = null;

		bool exist = dictionary.TryGetValue(originalSprite, out blurObject);

		if (exist) {
			if (blurObject.sprite == null || blurObject.sprite.texture == null) {
				dictionary.Remove(originalSprite);

				blurObject.sprite = LinearBlur.Blur(originalSprite, blurSize, blurIterations, Color.white);
				blurObject.blurSize = blurSize;
				blurObject.blurIterations = blurIterations;

				dictionary.Add(originalSprite, blurObject);
			} else if (blurObject.blurSize != blurSize || blurObject.blurIterations != blurIterations){
				blurObject.sprite = LinearBlur.Blur(originalSprite, blurSize, blurIterations, Color.white);
				blurObject.blurSize = blurSize;
				blurObject.blurIterations = blurIterations;
			}
			return(blurObject.sprite);
		} else {		
			Sprite sprite = LinearBlur.Blur(originalSprite, blurSize, blurIterations, Color.white);

			blurObject = new BlurObject(sprite, blurSize, blurIterations);

			dictionary.Add(originalSprite, blurObject);

			return(blurObject.sprite);
		}
	}
}

[ExecuteInEditMode]
public class LightingSpriteRenderer2D : MonoBehaviour
{
	public enum Type {Particle, WhiteMask, BlackMask};
	public enum SpriteMode {Custom, SpriteRenderer};
	public Type type = Type.Particle;
	public SpriteMode spriteMode = SpriteMode.Custom;
    public Sprite sprite;
    public Color color = new Color(0.5f, 0.5f, 0.5f, 1f);

	[Range(0, 1)]
	public float alpha = 1f;
    public bool flipX = false;
    public bool flipY = false;

	public Vector2 offsetPosition = new Vector2(0, 0);
	public Vector2 offsetScale = new Vector2(1, 1);
	public float offsetRotation = 0;

	[Range(1, 10)]
	public int blurSize = 1;

	[Range(1, 10)]
	public int blurIterations = 1;

	public bool applyBlur = false;

	public bool applyAdditive = false;
	
	public bool applyTransformRotation = true;

	public VirtualSpriteRenderer spriteRenderer = new VirtualSpriteRenderer();

	public static List<LightingSpriteRenderer2D> list = new List<LightingSpriteRenderer2D>();

	SpriteRenderer spriteRendererComponent;

	Material additiveMaterial;

	public Material GetMaterial() {
		if (additiveMaterial == null) {
			additiveMaterial = new Material (Shader.Find (Max2D.shaderPath + "Particles/Additive"));
		}
		additiveMaterial.mainTexture = GetSprite().texture;
		additiveMaterial.SetColor ("_TintColor", color);
		return(additiveMaterial);
	}

	public bool InCamera() {
		float verticalSize = Camera.main.orthographicSize;
        float horizontalSize = Camera.main.orthographicSize * ((float)Screen.width / Screen.height);

		return(Vector2.Distance(transform.position, Camera.main.transform.position) < Mathf.Sqrt((verticalSize) * (horizontalSize)) + GetSize() * 2 );
	}

	public Sprite GetSprite() {
		if (applyBlur) {
			return(BlurManager.RequestSprite(sprite, blurSize, blurIterations));
		} else {
			return(sprite);
		}
	}

	public SpriteRenderer GetSpriteRenderer() {
		if (spriteRendererComponent == null) {
			spriteRendererComponent = GetComponent<SpriteRenderer>();
		}
		return(spriteRendererComponent);
	}

	public void OnEnable() {
		list.Add(this);

		color.a = 1f;
	}

	public void OnDisable() {
		list.Remove(this);
	}

	public void Update() {
		if (spriteMode == SpriteMode.SpriteRenderer) {
			SpriteRenderer renderer = GetSpriteRenderer();
			if (renderer != null) {
				sprite = renderer.sprite;
			}
		}

		spriteRenderer.flipX = flipX;
		spriteRenderer.flipY = flipY;

		spriteRenderer.sprite = GetSprite();
		spriteRenderer.color = color;

		if (applyAdditive) {
			DrawMesh();
		}
	}

	public void DrawMesh() {
		float rotation = offsetRotation;
		if (applyTransformRotation) {
			rotation += transform.rotation.eulerAngles.z;
		}

		Vector2 position = transform.position;
		position.x += offsetPosition.x;
		position.y += offsetPosition.y;

		Vector2 size = offsetScale;

		if (applyBlur) {
			size.x *= 2;
			size.y *= 2;
		}

		float spriteSheetUV_X = (float)(sprite.texture.width) / sprite.rect.width;
		float spriteSheetUV_Y = (float)(sprite.texture.height) / sprite.rect.height;

		Rect rect = sprite.rect;
		//Rect uvRect = new Rect((float)rect.x / sprite.texture.width, (float)rect.y / sprite.texture.height, (float)rect.width / sprite.texture.width , (float)rect.height / sprite.texture.height);

		Vector2 scale = new Vector2(spriteSheetUV_X * rect.width / sprite.pixelsPerUnit, spriteSheetUV_Y * rect.height / sprite.pixelsPerUnit);

		scale.x = (float)sprite.texture.width / sprite.rect.width;
		scale.y = (float)sprite.texture.height / sprite.rect.height;

		size.x /= scale.x;
		size.y /= scale.y;

		size.x *= (float)sprite.texture.width / (sprite.pixelsPerUnit * 2);
		size.y *= (float)sprite.texture.height / (sprite.pixelsPerUnit * 2);
		
		if (spriteRenderer.flipX) {
			size.x = -size.x;
		}

		if (spriteRenderer.flipY) {
			size.y = -size.y;
		}

		Vector2 pivot = sprite.pivot;
		pivot.x /= sprite.rect.width;
		pivot.y /= sprite.rect.height;
		pivot.x -= 0.5f;
		pivot.y -= 0.5f;

		pivot.x *= size.x * 2;
		pivot.y *= size.y * 2;

		float pivotDist = Mathf.Sqrt(pivot.x * pivot.x + pivot.y * pivot.y);
		float pivotAngle = Mathf.Atan2(pivot.y, pivot.x);

		//float rectAngle = Mathf.Atan2(size.y, size.x);
		//float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);
		float rot = rotation * Mathf.Deg2Rad + Mathf.PI;
	
		// Pivot Pushes Position
		position.x += Mathf.Cos(pivotAngle + rot) * pivotDist;
		position.y += Mathf.Sin(pivotAngle + rot) * pivotDist;

		// Scale
		Vector2 scale2 = transform.lossyScale;
	
		scale2.x *= size.x;
		scale2.y *= size.y;

		Graphics.DrawMesh(LightingManager2D.GetRenderMesh(), Matrix4x4.TRS(position, Quaternion.Euler(0, 0, rotation),  scale2), GetMaterial(), 0);
	}

	float GetSize() {
		Vector2 size = offsetScale;

		float spriteSheetUV_X = (float)(sprite.texture.width) / sprite.rect.width;
		float spriteSheetUV_Y = (float)(sprite.texture.height) / sprite.rect.height;

		Rect rect = sprite.rect;
		Vector2 scale = new Vector2(spriteSheetUV_X * rect.width / sprite.pixelsPerUnit, spriteSheetUV_Y * rect.height / sprite.pixelsPerUnit);

		scale.x = (float)sprite.texture.width / sprite.rect.width;
		scale.y = (float)sprite.texture.height / sprite.rect.height;

		size.x /= scale.x;
		size.y /= scale.y;

		size.x *= (float)sprite.texture.width / (sprite.pixelsPerUnit * 2);
		size.y *= (float)sprite.texture.height / (sprite.pixelsPerUnit * 2);
		
		Vector2 scale2 = transform.lossyScale;
	
		scale2.x *= size.x;
		scale2.y *= size.y;
		return(Mathf.Sqrt(scale2.x * scale2.x + scale2.y * scale2.y));
	}

    static public List<LightingSpriteRenderer2D> GetList() {
		return(list);
	}

}
