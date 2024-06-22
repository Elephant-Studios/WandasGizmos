using Microsoft.VisualBasic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GrappleParkour
{
    class ItemGrapplingHook : Item
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.Handled;
            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch + pitch, byEntity.Pos.Yaw + yaw);
            Vec3d velocity = (aimPos - pos) * 10;
            byEntity.Pos.SetFrom(byEntity.ServerPos);
            byEntity.Pos.Motion.Set(velocity);
            byEntity.WatchedAttributes.MarkPathDirty("servercontrols");
        }
    }
}
