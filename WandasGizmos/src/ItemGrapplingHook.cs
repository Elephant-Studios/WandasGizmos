using HarmonyLib;
using System;
using System.Diagnostics;
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

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
        {
            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);
            slot.Itemstack.Attributes.RemoveAttribute("used");
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (slot.Itemstack.Attributes.HasAttribute("used"))
            {
                Console.WriteLine("used exists");
                return;
            }
            //Console.WriteLine("fauxBreak1");
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;
            handling = EnumHandHandling.PreventDefault;
            slot.Itemstack.Attributes.RemoveAttribute("renderVariant"); //normal
            slot.Itemstack.Attributes.RemoveAttribute("shapeinventory"); //normal
            //Console.WriteLine("fauxBreak2");
            if (api.World.GetEntityById(byEntity.EntityId) is EntityPlayer entityById)
            {
                this.FiredBy = entityById;
            }
            //Console.WriteLine("fauxBreak3");
            foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
            {
                if (itemSlot?.Itemstack?.Id == 1701)
                {
                    ItemRopeCount += itemSlot.Itemstack.StackSize;
                }
            }
            //Console.WriteLine("fauxBreak4");
            if (ItemRopeCount == 0)
            {
                api.Logger.Chat("requires rope to be used");
                return;
            }
            byEntity.Attributes.SetInt("aiming1", 1);
            byEntity.Attributes.SetInt("aimingCancel", 0);
            //Console.WriteLine("fauxBreak5");
        }
        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            //byEntity.StartAnimation("idle1");
            base.OnHeldIdle(slot, byEntity);
            if (!byEntity.CollidedVertically) //&& slot.Itemstack.Attributes.HasAttribute("used"))
            {
                //Console.WriteLine("in air");
                //byEntity.StartAnimation("idle1");
                byEntity.StopAnimation("walk");
                //byEntity.StartAnimation("fly");
                byEntity.StartAnimation("swing");
                
            }
            else
            {
                byEntity.StopAnimation("swing");
                //Console.WriteLine("ground");
            }
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.Attributes.GetInt("aimingCancel") == 1) return;
            if (secondsUsed < 1f) return;
            else if (secondsUsed > 2.5f) secondsUsed = 2.5f;
            slot.Itemstack.Attributes.SetBool("used", true);
            Console.WriteLine("fauxBreak2");
            byEntity.Attributes.SetInt("aiming1", 0);
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            Console.WriteLine(secondsUsed);
            double power = secondsUsed / 2.5;
            EntityProperties EnhkType = byEntity.World.GetEntityType(new AssetLocation("wgmt:grapplinghook"));
            EntityHook enhk = byEntity.World.ClassRegistry.CreateEntity(EnhkType) as EntityHook;
            Console.WriteLine("fauxBreak3");
            //EntityProperties EnrpType = byEntity.World.GetEntityType(new AssetLocation("wgmt:rope"));
            //EntityRope enrp = byEntity.World.ClassRegistry.CreateEntity(EnrpType) as EntityHook;
            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch, byEntity.Pos.Yaw);
            Vec3d velocity = (aimPos - pos);
            //byEntity.Pos.SetFrom(byEntity.ServerPos);
            Console.WriteLine("fauxBreak4");
            Vec3d spawnPos = byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(byEntity.LocalEyePos.X, byEntity.LocalEyePos.Y - 0.2, byEntity.LocalEyePos.Z);
            enhk.ServerPos.SetPos(spawnPos);
            enhk.ServerPos.Motion.Set(velocity * power);
            enhk.FiredById = byEntity.EntityId;
            enhk.HookSlot = slot;
            enhk.RopeCount = ItemRopeCount * 3;
            enhk.ProjectileStack = slot.Itemstack;

            enhk.Pos.SetFrom(enhk.ServerPos);
            enhk.World = byEntity.World;
            enhk.SetRotation();
            enhk.RopeCount = ItemRopeCount * 3;
            /*
            enrp.ServerPos.SetPos(spawnPos);
            enrp.ServerPos.Motion.Set(velocity);
            enrp.FiredById = byEntity.EntityId;
            enrp.HookSlot = slot;
            enrp.ProjectileStack = slot.Itemstack;

            enrp.Pos.SetFrom(enhk.ServerPos);
            enrp.World = byEntity.World;
            enrp.SetRotation();*/
            //enhk.SetHook(slot, api);
            //enpr.TrueClient = IClientPlayer;

            byEntity.World.SpawnEntity(enhk);
            //slot.Itemstack.Attributes.SetBool("used", true);
            slot.Itemstack.Attributes.SetInt("renderVariant", 1); //empty
            slot.Itemstack.Attributes.SetInt("shapeinventory", 1);
            Console.WriteLine("changed it");
            //byEntity.World.SpawnEntity(enrp);
            byEntity.StartAnimation("throw");
            //byEntity.StartAnimation("swing");
            //ItemGrapplingHook.InitializeHook(slot, enhk);
            //ItemStack stack = slot.TakeOut(1);
            //slot.MarkDirty();
            //byEntity.StartAnimation("throw");
            Debug.Print("working");
            Console.WriteLine("fauxBreak5");
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            return true;
        }

        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            byEntity.Attributes.SetInt("aiming1", 0);
            //byEntity.StopAnimation("aim");

            if (cancelReason != EnumItemUseCancelReason.ReleasedMouse)
            {
                byEntity.Attributes.SetInt("aimingCancel", 1);
            }

            return true;
        }
        
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