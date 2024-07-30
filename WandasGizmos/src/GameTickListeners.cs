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
    internal class GameTickListeners
    {
        public void OnFixedTick(float dt)
        {
            ICoreClientAPI icoreClientApi = DataFields.getICoreClientAPI();
            IClientPlayer player = DataFields.getIClientPlayerWorld().Player;
            HelperBlockDetection.WritePositionalBlocksToField(icoreClientApi, player);
            BehaviorClimbing.Climbing(player, icoreClientApi);
            BehaviorCrawling.Crawling(player, icoreClientApi);
        }
    }
}


