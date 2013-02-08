using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace datx02_rally.ModelPresenters
{
    static class CarShadingPresenter
    {
        public static void SetCarShadingParameters(this Effect effect, CarShadingSettings settings)
        {
            EffectParameterCollection param = effect.Parameters;

            /*param["MaterialDiffuse"].SetValue(settings.MaterialDiffuse);
            param["MaterialAmbient"].SetValue(settings.MaterialAmbient);
            param["MaterialSpecular"].SetValue(settings.MaterialSpecular);
            */
            param["MaterialReflection"].SetValue(settings.MaterialReflection);
            param["MaterialShininess"].SetValue(settings.MaterialShininess);

            param["World"].SetValue(settings.World);
            param["View"].SetValue(settings.View);
            param["Projection"].SetValue(settings.Projection);

            param["NormalMatrix"].SetValue(settings.NormalMatrix);

            param["EnvironmentMap"].SetValue(settings.EnvironmentMap);
            param["EyePosition"].SetValue(settings.EyePosition);

            param["LightPosition"].SetValue(settings.LightPosition);
            param["LightDiffuse"].SetValue(settings.LightDiffuse);
            param["LightRange"].SetValue(settings.LightRange);
            param["NumLights"].SetValue(settings.NumLights);

            param["DirectionalLightDirection"].SetValue(settings.DirectionalLightDirection);
            param["DirectionalLightDiffuse"].SetValue(settings.DirectionalLightDiffuse);
            param["DirectionalLightAmbient"].SetValue(settings.DirectionalLightAmbient);
        }

        public static T ConvertExamp1<T>(object input)
        {
            return (T)Convert.ChangeType(input, typeof(T));
        }
    }

    class CarShadingSettings
    {
        public Vector3 MaterialDiffuse { get; set; }
        public Vector3 MaterialAmbient { get; set; }
        public Vector3 MaterialSpecular { get; set; }

        public float MaterialReflection { get; set; }
        public float MaterialShininess { get; set; }

        public Matrix World { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public Matrix NormalMatrix { get; set; }

        public TextureCube EnvironmentMap { get; set; }
        public Vector3 EyePosition { get; set; }

        /// <summary>
        /// Max 10.
        /// </summary>
        public Vector3[] LightPosition { get; set; }
        public Vector3[] LightDiffuse { get; set; }
        public float[] LightRange { get; set; }
        public int NumLights { get; set; }

        public Vector3 DirectionalLightDirection { get; set; }
        public Vector3 DirectionalLightDiffuse { get; set; }
        public Vector3 DirectionalLightAmbient { get; set; }
        
        public CarShadingSettings()
        {
            MaterialDiffuse = Vector3.One;
            MaterialAmbient = Vector3.One;
            MaterialSpecular = Vector3.One;

            MaterialReflection = 0f;
            MaterialShininess = 0f;
            World = Matrix.Identity;

            LightPosition = new Vector3[10];
            LightDiffuse = new Vector3[10];
            LightRange = new float[10];
            NumLights = 0;
        }
    }
}
