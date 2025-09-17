using Cysharp.Threading.Tasks;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Remedy.Framework
{
    public static class AnimationExtensions
    {
        /// <summary>
        /// A Task that waits for the given Animation to finish playing
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        public async static UniTask PlayAndWaitForFinish(this Animation animation, AnimationClip clip)
        {
            animation.clip = clip;
            animation.Play();

            while (animation.isPlaying)
            {
                await UniTask.DelayFrame(1);
            }

            return;
        }

        public static Texture2D ToTexture(this Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width || sprite.rect.height != sprite.texture.height)
            {
                // Create a new texture with the size of the sprite
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.textureRect.x,
                    (int)sprite.textureRect.y,
                    (int)sprite.textureRect.width,
                    (int)sprite.textureRect.height);
                newTex.SetPixels(pixels);
                newTex.Apply();
                return newTex;
            }
            else
            {
                return sprite.texture;
            }
        }
    }
}