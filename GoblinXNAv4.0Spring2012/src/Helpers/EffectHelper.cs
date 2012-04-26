using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.Graphics;
using Model = GoblinXNA.Graphics.Model;

namespace GoblinXNA.Helpers
{
    /// <summary>
    /// A helper class for various Effect related operations.
    /// </summary>
    public class EffectHelper
    {
        /// <summary>
        /// Alters a model so it will draw using a custom effect, while preserving
        /// whatever textures were set on it as part of the original effects.
        /// </summary>
        public static void ReplaceModelEffect(Model model, Effect replacementEffect,
            String alpha, String diffuseColor, String emissiveColor, String specularColor, 
            String specularPower, String texture, String textureEnabled)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            foreach (ModelMesh mesh in model.Mesh)
            {
                // Scan over all the effects currently on the mesh.
                foreach (BasicEffect oldEffect in mesh.Effects)
                {
                    // If we haven't already seen this effect...
                    if (!effectMapping.ContainsKey(oldEffect))
                    {
                        // Make a clone of our replacement effect. We can't just use
                        // it directly, because the same effect might need to be
                        // applied several times to different parts of the model using
                        // a different texture each time, so we need a fresh copy each
                        // time we want to set a different texture into it.
                        Effect newEffect = replacementEffect.Clone();

                        try
                        {
                            if (alpha.Length > 0)
                                newEffect.Parameters[alpha].SetValue(oldEffect.Alpha);
                            if (diffuseColor.Length > 0)
                                newEffect.Parameters[diffuseColor].SetValue(
                                    new Vector4(oldEffect.DiffuseColor, oldEffect.Alpha));
                            if (emissiveColor.Length > 0)
                                newEffect.Parameters[emissiveColor].SetValue(new Vector4(oldEffect.EmissiveColor, 1));
                            if (specularColor.Length > 0)
                                newEffect.Parameters[specularColor].SetValue(new Vector4(oldEffect.SpecularColor, 1));
                            if (specularPower.Length > 0)
                                newEffect.Parameters[specularPower].SetValue(oldEffect.SpecularPower);
                            if (texture.Length > 0)
                                newEffect.Parameters[texture].SetValue(oldEffect.Texture);
                            if (textureEnabled.Length > 0)
                                newEffect.Parameters[textureEnabled].SetValue(oldEffect.TextureEnabled);
                        }
                        catch (Exception)
                        {
                            throw new GoblinException("One of the parameter name does not exist " +
                                "in your replacement effect");
                        }

                        effectMapping.Add(oldEffect, newEffect);
                    }
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effectMapping[meshPart.Effect];
                }
            }
        }

        /// <summary>
        /// Replaces
        /// </summary>
        /// <param name="model"></param>
        /// <param name="replacementEffect"></param>
        /// <param name="texture"></param>
        /// <param name="textureEnabled"></param>
        public static void ReplaceModelEffect(Model model, Effect replacementEffect,
            String texture, String textureEnabled)
        {
            ReplaceModelEffect(model, replacementEffect, "", "", "", "", "", texture,
                textureEnabled);
        }

        public static void ModifyEffectTechniques(Model model, string techniqueName)
        {
            foreach (ModelMesh mesh in model.Mesh)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques[techniqueName];
                }
            }
        }
    }
}
