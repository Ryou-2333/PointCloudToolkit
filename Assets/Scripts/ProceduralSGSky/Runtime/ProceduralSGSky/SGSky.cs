using System;

namespace UnityEngine.Rendering.HighDefinition
{
    [VolumeComponentMenu("Sky/SG Sky")]
    [SkyUniqueID(5)]
    public class SGSky : SkySettings
    {
        public override int GetHashCode()
        {
            int hash = base.GetHashCode();

            unchecked
            {
                hash = hash * 23 + multiplier.GetHashCode();
            }

            return hash;
        }

        public override Type GetSkyRendererType() { return typeof(SGSkyRenderer); }
    }
}
