using Microsoft.VisualBasic;
using System.Diagnostics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace GrappleParkour
{
    class ItemGrapplingHook : Item
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.Handled;
            string hookEntityCode = Attributes["hookEntityCode"].AsString();
            api.Logger.Debug($"hookEntityCode: {hookEntityCode}");
            EntityProperties type = byEntity.World.GetEntityType(new AssetLocation(Attributes["hookEntityCode"].AsString()));
            EntityHook enpr = byEntity.World.ClassRegistry.CreateEntity(type) as EntityHook;
            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch, byEntity.Pos.Yaw);
            Vec3d velocity = (aimPos - pos);
            //byEntity.Pos.SetFrom(byEntity.ServerPos);
            byEntity.Pos.Motion.Add(velocity);
            Vec3d spawnPos = byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(byEntity.LocalEyePos.X, byEntity.LocalEyePos.Y - 0.2, byEntity.LocalEyePos.Z);

            enpr.ServerPos.SetPos(spawnPos);
            enpr.ServerPos.Motion.Set(velocity);


            enpr.Pos.SetFrom(enpr.ServerPos);
            enpr.World = byEntity.World;
            enpr.SetRotation();

            byEntity.World.SpawnEntity(enpr);
            byEntity.StartAnimation("throw");
            //byEntity.WatchedAttributes.MarkPathDirty("servercontrols");
            
            Debug.Print("working"); 
        }
    }
}
