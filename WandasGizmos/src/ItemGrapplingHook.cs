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
using static OpenTK.Graphics.OpenGL.GL;

namespace WandasGizmos
{
    class ItemGrapplingHook : Item
    {
        public int ItemRopeCount;
        public EntityPlayer FiredBy;

        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
        {
            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);
            byEntity.WatchedAttributes.SetBool("fired", false);
        }
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
            foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
            {
                if (itemSlot?.Itemstack?.Id == 1701)
                {
                    ItemRopeCount += itemSlot.Itemstack.StackSize;
                }
            }
            if (ItemRopeCount == 0)
            {
                api.Logger.Chat("requires rope to be used");
                return;
            }
            byEntity.StartAnimation("toss");
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
        public override void OnHeldDropped(IWorldAccessor world, IPlayer byPlayer, ItemSlot slot, int quantity, ref EnumHandling handling)
        {
            base.OnHeldDropped(world, byPlayer, slot, quantity, ref handling);
            FiredBy = api.World.GetEntityById(byPlayer.Entity.EntityId) as EntityPlayer;
            FiredBy.StopAnimation("swing");
            slot.Itemstack.Attributes.SetInt("renderVariant", 1); //full
            slot.MarkDirty();
            FiredBy.WatchedAttributes.SetBool("fired", false);
            FiredBy.WatchedAttributes.MarkAllDirty();
        }
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
            byEntity.StopAnimation("toss");
            if (secondsUsed < 1f) return;
            else if (secondsUsed > 2.5f) secondsUsed = 2.5f;
            Console.WriteLine(secondsUsed);
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
                return;
            }
            EntityProperties EnhkType = byEntity.World.GetEntityType(Code);
            EntityHook enhk = byEntity.World.ClassRegistry.CreateEntity(EnhkType) as EntityHook;
            //EntityProperties EnrpType = byEntity.World.GetEntityType(new AssetLocation("wgmt:rope"));
            //EntityRope enrp = byEntity.World.ClassRegistry.CreateEntity(EnrpType) as EntityHook;
            double pitch = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            double yaw = byEntity.WatchedAttributes.GetDouble("aimingRandYaw", 1);
            Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y - 0.2, 0);
            Vec3d aimPos = pos.AheadCopy(1, byEntity.Pos.Pitch, byEntity.Pos.Yaw);
            Vec3d velocity = (aimPos - pos);
            //byEntity.Pos.SetFrom(byEntity.ServerPos);
            Vec3d spawnPos = byEntity.ServerPos.BehindCopy(0.21).XYZ.Add(byEntity.LocalEyePos.X, byEntity.LocalEyePos.Y - 0.2, byEntity.LocalEyePos.Z);
            enhk.ServerPos.SetPos(spawnPos);
            enhk.ServerPos.Motion.Set(velocity * power);
            enhk.FiredById = byEntity.EntityId;
            enhk.RopeCount = ItemRopeCount * 3;
            enhk.ProjectileStack = slot.Itemstack;

            enhk.Pos.SetFrom(enhk.ServerPos);
            enhk.World = byEntity.World;
            enhk.SetRotation();
            enhk.RopeCount = ItemRopeCount * 3;

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