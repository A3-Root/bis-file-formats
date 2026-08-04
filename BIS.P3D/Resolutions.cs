using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.P3D
{
    /// <summary>Specifies lod name values.</summary>
    public enum LodName
    {
        /// <summary>Specifies the view gunner value.</summary>
        ViewGunner,
        /// <summary>Specifies the view pilot value.</summary>
        ViewPilot,
        /// <summary>Specifies the view cargo value.</summary>
        ViewCargo,
        /// <summary>Specifies the geometry value.</summary>
        Geometry,
        /// <summary>Specifies the memory value.</summary>
        Memory,
        /// <summary>Specifies the land contact value.</summary>
        LandContact,
        /// <summary>Specifies the roadway value.</summary>
        Roadway,
        /// <summary>Specifies the paths value.</summary>
        Paths,
        /// <summary>Specifies the hit points value.</summary>
        HitPoints,
        /// <summary>Specifies the view geometry value.</summary>
        ViewGeometry,
        /// <summary>Specifies the fire geometry value.</summary>
        FireGeometry,
        /// <summary>Specifies the view cargo geometry value.</summary>
        ViewCargoGeometry,
        /// <summary>Specifies the view cargo fire geometry value.</summary>
        ViewCargoFireGeometry,
        /// <summary>Specifies the view commander value.</summary>
        ViewCommander,
        /// <summary>Specifies the view commander geometry value.</summary>
        ViewCommanderGeometry,
        /// <summary>Specifies the view commander fire geometry value.</summary>
        ViewCommanderFireGeometry,
        /// <summary>Specifies the view pilot geometry value.</summary>
        ViewPilotGeometry,
        /// <summary>Specifies the view pilot fire geometry value.</summary>
        ViewPilotFireGeometry,
        /// <summary>Specifies the view gunner geometry value.</summary>
        ViewGunnerGeometry,
        /// <summary>Specifies the view gunner fire geometry value.</summary>
        ViewGunnerFireGeometry,
        /// <summary>Specifies the sub parts value.</summary>
        SubParts,
        /// <summary>Specifies the shadow volume view cargo value.</summary>
        ShadowVolumeViewCargo,
        /// <summary>Specifies the shadow volume view pilot value.</summary>
        ShadowVolumeViewPilot,
        /// <summary>Specifies the shadow volume view gunner value.</summary>
        ShadowVolumeViewGunner,
        /// <summary>Specifies the wreck value.</summary>
        Wreck,
        /// <summary>Specifies the phys x value.</summary>
        PhysX,
        /// <summary>Specifies the shadow volume value.</summary>
        ShadowVolume,
        /// <summary>Specifies the resolution value.</summary>
        Resolution,
        /// <summary>Specifies the undefined value.</summary>
        Undefined
    }

    /// <summary>Represents resolution.</summary>
    public static class Resolution
    {
        private const float specialLod = 1e15f;

        /// <summary>Stores the geometry value.</summary>
        public const float GEOMETRY = 1e13f;
        /// <summary>Stores the buoyancy value.</summary>
        public const float BUOYANCY = 2e13f;
        /// <summary>Stores the physxold value.</summary>
        public const float PHYSXOLD = 3e13f;
        /// <summary>Stores the physx value.</summary>
        public const float PHYSX = 4e13f;

        /// <summary>Stores the memory value.</summary>
        public const float MEMORY = 1e15f;
        /// <summary>Stores the landcontact value.</summary>
        public const float LANDCONTACT = 2e15f;
        /// <summary>Stores the roadway value.</summary>
        public const float ROADWAY = 3e15f;
        /// <summary>Stores the paths value.</summary>
        public const float PATHS = 4e15f;
        /// <summary>Stores the hitpoints value.</summary>
        public const float HITPOINTS = 5e15f;

        /// <summary>Stores the view geometry value.</summary>
        public const float VIEW_GEOMETRY = 6e15f;
        /// <summary>Stores the fire geometry value.</summary>
        public const float FIRE_GEOMETRY = 7e15f;

        /// <summary>Stores the view geometry cargo value.</summary>
        public const float VIEW_GEOMETRY_CARGO = 8e15f;
        /// <summary>Stores the view geometry pilot value.</summary>
        public const float VIEW_GEOMETRY_PILOT = 13e15f;
        /// <summary>Stores the view geometry gunner value.</summary>
        public const float VIEW_GEOMETRY_GUNNER = 15e15f;
        /// <summary>Stores the fire geometry gunner value.</summary>
        public const float FIRE_GEOMETRY_GUNNER = 16e15f;

        /// <summary>Stores the subparts value.</summary>
        public const float SUBPARTS = 17e15f;

        /// <summary>Stores the shadowvolume cargo value.</summary>
        public const float SHADOWVOLUME_CARGO = 18e15f;
        /// <summary>Stores the shadowvolume pilot value.</summary>
        public const float SHADOWVOLUME_PILOT = 19e15f;
        /// <summary>Stores the shadowvolume gunner value.</summary>
        public const float SHADOWVOLUME_GUNNER = 20e15f;

        /// <summary>Stores the wreck value.</summary>
        public const float WRECK = 21e15f;

        /// <summary>Stores the view commander value.</summary>
        public const float VIEW_COMMANDER = 10e15f;
        /// <summary>Stores the view gunner value.</summary>
        public const float VIEW_GUNNER = 1000f;
        /// <summary>Stores the view pilot value.</summary>
        public const float VIEW_PILOT = 1100f;
        /// <summary>Stores the view cargo value.</summary>
        public const float VIEW_CARGO = 1200f;

        /// <summary>Stores the shadowvolume value.</summary>
        public const float SHADOWVOLUME = 10000.0f;
        /// <summary>Stores the shadowbuffer value.</summary>
        public const float SHADOWBUFFER = 11000.0f;

        /// <summary>Stores the shadow min value.</summary>
        public const float SHADOW_MIN = 10000.0f;
        /// <summary>Stores the shadow max value.</summary>
        public const float SHADOW_MAX = 20000.0f;

        /// <summary>
        /// Tells us if the current LOD with given resolution has normal NamedSelections (returns true) or empty ones (return false)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool KeepsNamedSelections(float r)
        {
            return r == MEMORY || r == FIRE_GEOMETRY || r == GEOMETRY
                || r == VIEW_GEOMETRY || r == VIEW_GEOMETRY_PILOT || r == VIEW_GEOMETRY_GUNNER
                || r == VIEW_GEOMETRY_CARGO || r == PATHS || r == HITPOINTS
                || r == PHYSX || r == BUOYANCY;
        }

        /// <summary>Gets lod type.</summary>
        /// <param name="res">The res value.</param>
        /// <returns>The resulting value.</returns>
        public static LodName GetLODType(this float res)
        {
            if (res == specialLod) return LodName.Memory;
            if (res == 2 * specialLod) return LodName.LandContact;
            if (res == 3 * specialLod) return LodName.Roadway;
            if (res == 4 * specialLod) return LodName.Paths;

            if (res == 5 * specialLod) return LodName.HitPoints;
            if (res == 6 * specialLod) return LodName.ViewGeometry;
            if (res == 7 * specialLod) return LodName.FireGeometry;
            if (res == 8 * specialLod) return LodName.ViewCargoGeometry;
            if (res == 9 * specialLod) return LodName.ViewCargoFireGeometry;
            if (res == 10 * specialLod) return LodName.ViewCommander;
            if (res == 11 * specialLod) return LodName.ViewCommanderGeometry;
            if (res == 12 * specialLod) return LodName.ViewCommanderFireGeometry;
            if (res == 13 * specialLod) return LodName.ViewPilotGeometry;
            if (res == 14 * specialLod) return LodName.ViewPilotFireGeometry;
            if (res == 15 * specialLod) return LodName.ViewGunnerGeometry;
            if (res == 16 * specialLod) return LodName.ViewGunnerFireGeometry;
            if (res == 17 * specialLod) return LodName.SubParts;
            if (res == 18 * specialLod) return LodName.ShadowVolumeViewCargo;
            if (res == 19 * specialLod) return LodName.ShadowVolumeViewPilot;
            if (res == 20 * specialLod) return LodName.ShadowVolumeViewGunner;
            if (res == 21 * specialLod) return LodName.Wreck;

            if (res == 1000.0f) return LodName.ViewGunner;
            if (res == 1100.0f) return LodName.ViewPilot;
            if (res == 1200.0f) return LodName.ViewCargo;

            if (res == 1e13f) return LodName.Geometry;
            if (res == 4e13f) return LodName.PhysX;

            if (res >= 10000.0 && res <= 20000.0) return LodName.ShadowVolume;

            return LodName.Resolution;
        }

        /// <summary>Gets lod name.</summary>
        /// <param name="res">The res value.</param>
        /// <returns>The resulting value.</returns>
        public static string GetLODName(this float res)
        {
            var lodType = res.GetLODType();

            if (lodType == LodName.Resolution)
                return res.ToString("0.000");
            if (lodType == LodName.ShadowVolume)
                return "ShadowVolume" + (res - 10000f).ToString("0.000");
            else
                return Enum.GetName(typeof(LodName), lodType);
        }

        /// <summary>Performs the is resolution operation.</summary>
        /// <param name="r">The r value.</param>
        /// <returns>The resulting value.</returns>
        public static bool IsResolution(float r)
        {
            return r < SHADOW_MIN;
        }

        /// <summary>Performs the is shadow operation.</summary>
        /// <param name="r">The r value.</param>
        /// <returns>The resulting value.</returns>
        public static bool IsShadow(float r)
        {
            return (r >= SHADOW_MIN && r < SHADOW_MAX) ||
                r == SHADOWVOLUME_GUNNER ||
                r == SHADOWVOLUME_PILOT ||
                r == SHADOWVOLUME_CARGO;
        }

        /// <summary>Performs the is visual operation.</summary>
        /// <param name="r">The r value.</param>
        /// <returns>The resulting value.</returns>
        public static bool IsVisual(float r)
        {
            return IsResolution(r) ||
                r == VIEW_CARGO ||
                r == VIEW_GUNNER ||
                r == VIEW_PILOT ||
                r == VIEW_COMMANDER;
        }
    }
}
