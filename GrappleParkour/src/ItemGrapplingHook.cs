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
            Vec3d pos = byEntity.ServerPos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.ServerPos.Pitch + pitch, byEntity.ServerPos.Yaw + yaw);
            Vec3d velocity = (aimPos - pos) * 0.65;
            byEntity.ServerPos.Motion.Set(velocity);
            byEntity.Pos.SetFrom(byEntity.ServerPos);
            byEntity.WatchedAttributes.MarkPathDirty("servercontrols");
        }
    }
}
