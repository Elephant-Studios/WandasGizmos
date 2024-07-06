using HarmonyLib;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace WandasGizmos
{
    class ItemGrapplingHook : Item
    {
        public int ItemRopeCount;
        public EntityPlayer FiredBy;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;
            if (slot.Itemstack.Attributes.HasAttribute("hookId")) return;
            if (api.World.GetEntityById(byEntity.EntityId) is EntityPlayer entityById)
            {
                this.FiredBy = entityById;
            }
            foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
            {
                if (itemSlot?.Itemstack?.Id == 1701)
                {
                    ItemRopeCount += itemSlot.Itemstack.StackSize;
                }
            }
            if (ItemRopeCount == 0) api.Logger.Chat("requires rope to be used");
            handling = EnumHandHandling.PreventDefault;
            EntityProperties EnhkType = byEntity.World.GetEntityType(new AssetLocation("wgmt:grapplinghook"));
            EntityHook enhk = byEntity.World.ClassRegistry.CreateEntity(EnhkType) as EntityHook;

            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch, byEntity.Pos.Yaw);
            Vec3d velocity = (aimPos - pos);
            //byEntity.Pos.SetFrom(byEntity.ServerPos);

            Vec3d spawnPos = byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(byEntity.LocalEyePos.X, byEntity.LocalEyePos.Y - 0.2, byEntity.LocalEyePos.Z);

            enhk.ServerPos.SetPos(spawnPos);
            enhk.ServerPos.Motion.Set(velocity);
            enhk.FiredById = byEntity.EntityId;
            enhk.HookSlot = slot;
            enhk.ProjectileStack = slot.Itemstack;

            enhk.Pos.SetFrom(enhk.ServerPos);
            enhk.World = byEntity.World;
            enhk.SetRotation();
            enhk.RopeCount = ItemRopeCount * 3;

            
            //enhk.SetHook(slot, api);
            //enpr.TrueClient = IClientPlayer;

            byEntity.World.SpawnEntity(enhk);
            
            byEntity.StartAnimation("throw");
            //ItemGrapplingHook.InitializeHook(slot, enhk);
            //ItemStack stack = slot.TakeOut(1);
            slot.MarkDirty();
            Debug.Print("working");
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            //ClearHook(slot);
        }

        /*public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            return true;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            return true;
        }*/

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "heldhelp-throw",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

        public static void SwitchMode(bool mode, EntityAgent player)
        {
           
        }
        /*
        public static void InitializeHook(ItemSlot slot, EntityHook hook)
        {
            slot.Itemstack.Attributes.SetLong("hookId", hook.EntityId);
            slot.MarkDirty();
        }

        public static void ClearHook(ItemSlot slot)
        {
            slot.Itemstack.Attributes.RemoveAttribute("hookId");
            slot.MarkDirty();
        }

        public static bool HasHook(ItemSlot slot)
        {
            return slot.Itemstack.Attributes.HasAttribute("hookId");
        }
        */
    }
}