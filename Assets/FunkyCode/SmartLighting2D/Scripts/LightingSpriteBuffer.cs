using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingSpriteBuffer
{
   
    public static void Draw(Vector2D offset, float z)
    {
		List<LightingSpriteRenderer2D> list = LightingSpriteRenderer2D.GetList();
		LightingSpriteRenderer2D id;

		Material material;
		Vector2 position, scale;
		float rot;
		Color color;

        for(int i = 0; i < list.Count; i++) {
			id = list[i];

			if (id.GetSprite() == null) {
				continue;
			}

			if (id.InCamera() == false) {
				continue;
			}

			LightingManager2D.LightingDebug.SpriteRenderersDrawn ++;

			position = id.transform.position;

			scale = id.transform.lossyScale;
			scale.x *= id.offsetScale.x;
			scale.y *= id.offsetScale.y;

			rot = id.offsetRotation;
			if (id.applyTransformRotation) {
				rot += id.transform.rotation.eulerAngles.z;
			}
	
			switch(id.type) {
				case LightingSpriteRenderer2D.Type.Particle: 

					color = id.color;
					color.a = id.alpha;

					material = LightingManager2D.Get().additiveMaterial;
					material.mainTexture = id.GetSprite().texture;
					material.SetColor ("_TintColor", color);

					Max2D.DrawSpriteRenderer(material, id.spriteRenderer, offset.ToVector2() + position + id.offsetPosition, scale, rot, z);
					
					material.mainTexture = null;
				
					break;

				case LightingSpriteRenderer2D.Type.WhiteMask:

					material = LightingManager2D.Get().whiteSpriteMaterial;
					material.mainTexture = id.GetSprite().texture;
					
					Max2D.DrawSpriteRenderer(material, id.spriteRenderer, offset.ToVector2() + position + id.offsetPosition, scale, rot, z);
					
					material.mainTexture = null;
				
					break;

				case LightingSpriteRenderer2D.Type.BlackMask:

					material = LightingManager2D.Get().blackSpriteMaterial;
					material.mainTexture = id.sprite.texture;

					Max2D.DrawSpriteRenderer(material, id.spriteRenderer, offset.ToVector2() + position + id.offsetPosition, scale, rot, z);
					
					material.mainTexture = null;
				
					break;
			}
		}
    }
}
