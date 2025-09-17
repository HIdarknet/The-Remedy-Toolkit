using Remedy.Framework;
using UnityEngine;

namespace Remedy.UI
{
    public class UISprite : UIComponent
    {
        MeshRenderer Renderer;

        public static UISprite New(string name, Transform parent = null, int layer = 0)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "SelectionWheelQuad";
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localScale = Vector3.one;
            quad.name = name;
            var sprite = quad.AddComponent<UISprite>();

            sprite.Renderer = quad.GetComponent<MeshRenderer>();
            if (sprite.Renderer != null)
            {
                sprite.Renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                {
                    color = Color.white
                };
            }

            sprite.Layer = layer;

            if (parent != null) sprite.transform.parent = parent;
            return sprite;
        }

        /// <summary>
        /// Sets the Material of the Sprite
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(Material material)
        {
            Renderer.sharedMaterial = material;
        }

        /// <summary>
        /// Sets the Main Texture value of the Material.
        /// </summary>
        /// <param name="texture"></param>
        public void SetTexture(Texture texture)
        {
            if (Renderer != null && Renderer.sharedMaterial != null)
            {
                Renderer.sharedMaterial.SetTexture("_BaseMap", texture);
            }
        }

        /// <summary>
        /// Extracts the Texture from a Sprite and assigns this UI Sprite's Texture to that.
        /// </summary>
        /// <param name="sprite"></param>
        public void SetSprite(Sprite sprite)
        {
            SetTexture(sprite.ToTexture());
        }
    }
}