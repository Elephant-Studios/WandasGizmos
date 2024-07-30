using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
    internal class BehaviorCrawling
    {
        private static double lastNotificationTime;

        public static void Crawling(IClientPlayer player, ICoreClientAPI capi)
        {
            if (DataFields.isCrawling)
                ((Entity)((IPlayer)player).Entity).Stats.Set("walkspeed", "crawlingModifier", -0.8f, true);
            if (!DataFields.isCrawling || !((EntityAgent)((IPlayer)player).Entity).Controls.Jump && !((EntityAgent)((IPlayer)player).Entity).Controls.Sprint && ((Entity)((IPlayer)player).Entity).CollidedVertically && !((Entity)((IPlayer)player).Entity).FeetInLiquid && !((EntityAgent)((IPlayer)player).Entity).Controls.FloorSitting)
                return;
            BehaviorCrawling.StopCrawling(capi.World);
        }

        public static void StartCrawling(IClientWorldAccessor world)
        {
            DataFields.isCrawling = true;
            ((Entity)((IPlayer)world.Player).Entity).Properties.EyeHeight = 179.0 / 256.0;
            ((Entity)((IPlayer)world.Player).Entity).Properties.CollisionBoxSize = new Vec2f(307f / 512f, 307f / 512f);
            ((Entity)((IPlayer)world.Player).Entity).Properties.CanClimb = false;
            ((Entity)((IPlayer)world.Player).Entity).StartAnimation("ammcrawl");
            NetworkSyncHandler.SendAnimationStartMessage("ammcrawl", world);
        }

        public static void StopCrawling(IClientWorldAccessor world)
        {
            if (DataFields.blockAbovePlayerPos == ((IWorldAccessor)world).GetBlock(new AssetLocation("air")) || ((CollectibleObject)DataFields.blockAbovePlayerPos).IsLiquid() || DataFields.blockAbovePlayerPos.Climbable)
            {
                DataFields.isCrawling = false;
                ((Entity)((IPlayer)world.Player).Entity).Properties.EyeHeight = 435.0 / 256.0;
                ((Entity)((IPlayer)world.Player).Entity).Properties.CollisionBoxSize = new Vec2f(307f / 512f, 1.849609f);
                ((Entity)((IPlayer)world.Player).Entity).Properties.CanClimb = true;
                ((Entity)((IPlayer)world.Player).Entity).Stats.Remove("walkspeed", "crawlingModifier");
                ((Entity)((IPlayer)world.Player).Entity).StopAnimation("ammcrawl");
                NetworkSyncHandler.SendAnimationStopMessage("ammcrawl", world);
            }
            else
            {
                double num = (double)((IWorldAccessor)world).Api.World.ElapsedMilliseconds / 1000.0;
                if (num - BehaviorCrawling.lastNotificationTime < 2.0)
                    return;
                BehaviorCrawling.lastNotificationTime = num;
                world.Player.ShowChatNotification("You can't stand up here");
            }
        }

        public static void ToggleCrawling(IClientWorldAccessor world)
        {
            if (!DataFields.crawlingEnabled)
                return;
            if (!DataFields.isCrawling)
                BehaviorCrawling.StartCrawling(world);
            else
                BehaviorCrawling.StopCrawling(world);
        }
    }
}


