﻿using HarmonyLib;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Elephant.WandasGizmos
{
    class ItemGrapplingHook : Item
    {
        public double ItemRopeCount;
        public EntityPlayer FiredBy;
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;
            handling = EnumHandHandling.PreventDefault;
            if (byEntity.WatchedAttributes.GetAsBool("fired")) { byEntity.WatchedAttributes.SetBool("fired", false); }
            if (api.World.GetEntityById(byEntity.EntityId) is EntityPlayer entityById)
            {
                this.FiredBy = entityById;
            }
            byEntity.Attributes.SetInt("aiming", 1);
            byEntity.Attributes.SetInt("aimingCancel", 0);
            byEntity.StartAnimation("toss");
            byEntity.StartAnimation("aim");
        }
        public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            byEntity.Attributes.SetInt("aiming", 0);
            byEntity.StopAnimation("aim");
            FiredBy = api.World.GetEntityById(byEntity.EntityId) as EntityPlayer;
            FiredBy.StopAnimation("swing");
            slot.Itemstack.Attributes.SetInt("renderVariant", 1); //full
            slot.MarkDirty();
            FiredBy.StopAnimation("aim");
            FiredBy.WatchedAttributes.SetBool("fired", false);
            FiredBy.WatchedAttributes.MarkAllDirty();
            if (cancelReason != EnumItemUseCancelReason.ReleasedMouse)
            {
                byEntity.Attributes.SetInt("aimingCancel", 1);
            }

            return true;
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            dsc.AppendLine("Snakes... Why'd it have to be snakes!");
        }
        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            //byEntity.StartAnimation("idle1");
            base.OnHeldIdle(slot, byEntity);
            
            if (byEntity.WatchedAttributes.GetAsBool("fired"))
            {
                slot.Itemstack.Attributes.SetInt("renderVariant", 2); //empty
                slot.MarkDirty();
            }
            else
            {
                slot.Itemstack.Attributes.SetInt("renderVariant", 1); //full
                slot.MarkDirty();
            }
            if (!byEntity.CollidedVertically) //&& slot.Itemstack.Attributes.HasAttribute("used"))
            {
                byEntity.StopAnimation("walk");
                byEntity.StartAnimation("swing");
                
            }
            else
            {
                byEntity.StopAnimation("swing");
            }
        }
        public override void OnHeldDropped(IWorldAccessor world, IPlayer byPlayer, ItemSlot slot, int quantity, ref EnumHandling handling)
        {
            base.OnHeldDropped(world, byPlayer, slot, quantity, ref handling);
            FiredBy = api.World.GetEntityById(byPlayer.Entity.EntityId) as EntityPlayer;
            FiredBy.StopAnimation("swing");
            slot.Itemstack.Attributes.SetInt("renderVariant", 1); //full
            slot.MarkDirty();
            FiredBy.StopAnimation("aim");
            FiredBy.WatchedAttributes.SetBool("fired", false);
            FiredBy.WatchedAttributes.MarkAllDirty();
            FiredBy.WatchedAttributes.MarkAllDirty();
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            Console.WriteLine("Break1");
            if (byEntity.Attributes.GetInt("aimingCancel") == 1) return;
            byEntity.StopAnimation("toss");
            Console.WriteLine("Break1");
            if (secondsUsed < 0.75f) return;
            else if (secondsUsed > 2.5f) secondsUsed = 2.5f;
            double power = secondsUsed / 2.5;
            slot.Itemstack.Attributes.SetInt("renderVariant", 2); //empty
            byEntity.WatchedAttributes.SetBool("fired", true);
            byEntity.Attributes.MarkAllDirty();
            slot.MarkDirty();
            slot.Itemstack.Collectible.DamageItem(byEntity.World, byEntity, slot);
            int leftDurability = slot.Itemstack == null ? 1 : slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack);
            if (leftDurability <= 0)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
                FiredBy.WatchedAttributes.SetBool("fired", false);
                return;
            }
            EntityProperties EnhkType = byEntity.World.GetEntityType(Code);
            EntityHook enhk = byEntity.World.ClassRegistry.CreateEntity(EnhkType) as EntityHook;
            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch, byEntity.Pos.Yaw);
            Vec3d velocity = (aimPos - pos);
            Vec3d spawnPos = byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(byEntity.LocalEyePos.X, byEntity.LocalEyePos.Y - 0.2, byEntity.LocalEyePos.Z);
            enhk.ServerPos.SetPos(spawnPos);
            enhk.ServerPos.Motion.Set(velocity * power);
            enhk.FiredById = byEntity.EntityId;
            enhk.ProjectileStack = slot.Itemstack;

            enhk.Pos.SetFrom(enhk.ServerPos);
            enhk.World = byEntity.World;
            enhk.SetRotation();

            byEntity.World.SpawnEntity(enhk);
            //slot.Itemstack.Attributes.SetBool("used", true);
            
            
            
            //byEntity.World.SpawnEntity(enrp);
            //byEntity.StartAnimation("swing");
            //ItemGrapplingHook.InitializeHook(slot, enhk);
            //ItemStack stack = slot.TakeOut(1);
            //slot.MarkDirty();
            //byEntity.StartAnimation("throw");
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
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