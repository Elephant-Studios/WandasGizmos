using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using WandasGizmos.src;
using static OpenTK.Graphics.OpenGL.GL;

namespace WandasGizmos
{
    internal class DataFields
    {
        public static bool climbingEnabled = true;
        public static bool crawlingEnabled = true;
        public static bool autoStepEnabled = true;
        public static bool invGUIopen = false;
        public static bool climbMode = false;
        public static bool isClimbing = false;
        public static bool isCrawling = false;
        public static int maxStamina;
        public static int fullStaminaCircle = 100;
        public static int currentStamina = DataFields.maxStamina;
        public static int numStaminaRings;
        public static int staminaRegenDelay = 20;
        public static int staminaDeltaTDrained = DataFields.staminaRegenDelay;
        public static int staminaRegenerationValue = 1;
        public static int drainStaminaClimbingValue = 1;
        public static ICoreAPI CoreAPI;
        public static ICoreClientAPI iCoreClientAPI;
        public static IClientWorldAccessor iclientWorldAccessor;
        public static Block blockAtPlayerPos;
        public static Block blockAbovePlayerPos;
        public static Block blockDirXplus;
        public static Block blockDirXmin;
        public static Block blockDirZplus;
        public static Block blockDirZmin;
        public static bool metalBlockClimbable = false;
        public static bool stoneBlockClimbable = true;
        public static bool woodBlockClimbable = true;
        public static bool collidedWithSolidClimbable = false;
        public static bool collidedWithMetalClimbable = false;
        public static bool collidedWithStoneClimbable = false;
        public static bool collidedWithWoodClimbable = false;
        public static bool collidedWithClimbable = false;

        public static void setCoreAPI(ICoreAPI api) => DataFields.CoreAPI = api;

        public static ICoreAPI coreAPI() => DataFields.CoreAPI;

        public static void setIClientPlayerWorld(IClientWorldAccessor world)
        {
            DataFields.iclientWorldAccessor = world;
        }

        public static IClientWorldAccessor getIClientPlayerWorld() => DataFields.iclientWorldAccessor;

        public static void setICoreClientAPI(ICoreClientAPI api) => DataFields.iCoreClientAPI = api;

        public static ICoreClientAPI getICoreClientAPI() => DataFields.iCoreClientAPI;
    }
}


