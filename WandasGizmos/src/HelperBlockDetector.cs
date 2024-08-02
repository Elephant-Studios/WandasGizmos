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
    internal class HelperBlockDetection
    {
        /*public static void WritePositionalBlocksToField(ICoreClientAPI capi, IClientPlayer player)
        {
            DataFields.blockAtPlayerPos = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X, (int)((Entity)((IPlayer)player).Entity).Pos.Y, (int)((Entity)((IPlayer)player).Entity).Pos.Z);
            DataFields.blockAbovePlayerPos = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X, (int)((Entity)((IPlayer)player).Entity).Pos.Y + 1, (int)((Entity)((IPlayer)player).Entity).Pos.Z);
            DataFields.blockDirXplus = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X + 1, (int)((Entity)((IPlayer)player).Entity).Pos.Y, (int)((Entity)((IPlayer)player).Entity).Pos.Z);
            DataFields.blockDirXmin = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X - 1, (int)((Entity)((IPlayer)player).Entity).Pos.Y, (int)((Entity)((IPlayer)player).Entity).Pos.Z);
            DataFields.blockDirZplus = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X, (int)((Entity)((IPlayer)player).Entity).Pos.Y, (int)((Entity)((IPlayer)player).Entity).Pos.Z + 1);
            DataFields.blockDirZmin = ((IWorldAccessor)capi.World).BlockAccessor.GetBlock((int)((Entity)((IPlayer)player).Entity).Pos.X, (int)((Entity)((IPlayer)player).Entity).Pos.Y, (int)((Entity)((IPlayer)player).Entity).Pos.Z - 1);
        }

        public static void CheckCollidedwithClimbable(IClientPlayer player, ICoreClientAPI api)
        {
            if (DataFields.collidedWithSolidClimbable && (DataFields.collidedWithMetalClimbable || DataFields.collidedWithStoneClimbable || DataFields.collidedWithWoodClimbable))
                DataFields.collidedWithClimbable = true;
            else
                DataFields.collidedWithClimbable = false;
        }

        public static void CheckBlockMatterSolid(IClientPlayer player, ICoreClientAPI api)
        {
            if (((CollectibleObject)DataFields.blockDirXmin).MatterState != 2 && ((CollectibleObject)DataFields.blockDirXplus).MatterState != 2 && ((CollectibleObject)DataFields.blockDirZmin).MatterState != 2 && ((CollectibleObject)DataFields.blockDirZplus).MatterState != 2)
                DataFields.collidedWithSolidClimbable = false;
            else
                DataFields.collidedWithSolidClimbable = true;
        }

        public static void CheckBlockMaterialMetal(IClientPlayer player, ICoreClientAPI api)
        {
            if (!DataFields.metalBlockClimbable)
                DataFields.collidedWithMetalClimbable = false;
            else if (DataFields.blockDirXmin.BlockMaterial != 11 && DataFields.blockDirXplus.BlockMaterial != 11 && DataFields.blockDirZmin.BlockMaterial != 11 && DataFields.blockDirZplus.BlockMaterial != 11)
                DataFields.collidedWithMetalClimbable = false;
            else
                DataFields.collidedWithMetalClimbable = true;
        }

        public static void CheckBlockMaterialStone(IClientPlayer player, ICoreClientAPI api)
        {
            if (!DataFields.stoneBlockClimbable)
                DataFields.collidedWithStoneClimbable = false;
            else if (DataFields.blockDirXmin.BlockMaterial != 6 && DataFields.blockDirXplus.BlockMaterial != 6 && DataFields.blockDirZmin.BlockMaterial != 6 && DataFields.blockDirZplus.BlockMaterial != 6)
                DataFields.collidedWithStoneClimbable = false;
            else
                DataFields.collidedWithStoneClimbable = true;
        }

        public static void CheckBlockMaterialWood(IClientPlayer player, ICoreClientAPI api)
        {
            if (!DataFields.woodBlockClimbable)
                DataFields.collidedWithWoodClimbable = false;
            else if (DataFields.blockDirXmin.BlockMaterial != 4 && DataFields.blockDirXplus.BlockMaterial != 4 && DataFields.blockDirZmin.BlockMaterial != 4 && DataFields.blockDirZplus.BlockMaterial != 4)
                DataFields.collidedWithWoodClimbable = false;
            else
                DataFields.collidedWithWoodClimbable = true;
        }*/
    }
}


