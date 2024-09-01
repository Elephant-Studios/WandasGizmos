using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using static OpenTK.Graphics.OpenGL.GL;

namespace WandasGizmos
{
    public class EntityHook : Entity
    {

        bool beforeCollided;
        bool stuck;

        long msLaunch;
        long msCollide;

        Vec3d motionBeforeCollide = new();

        CollisionTester collTester = new();

        public long FiredById;
        public EntityPlayer FiredBy;
        public float Weight = 0.1f;
        public float Damage;
        public ItemStack ProjectileStack;
        public ICoreClientAPI capi;
        public ICoreServerAPI sapi;
        public float DropOnImpactChance = 0f;
        public bool DamageStackOnImpact = false;
        public float SpringConst = 0.5f;
        public double MaxLength;
        public int RopeCount;
        //private readonly Dictionary<string, bool> OnlinePlayers = new();
        public double FunConstant = 0.01f;
        public Vec3d anchorPoint;
        public Vec3d originalPoint;

        //public int totalRope;


        Cuboidf collisionTestBox;


        public override bool ApplyGravity
        {
            get { return !stuck; }
        }

        public override bool IsInteractable
        {
            get { return false; }
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
            if (api is ICoreClientAPI clientApi) capi = clientApi;
            if (api is ICoreServerAPI serverApi) sapi = serverApi;
            if (api.World.GetEntityById(FiredById) is EntityPlayer player)
            {
                FiredBy = player;
            }
            else
            {
                Die();
            }
            if (sapi != null) sapi.Event.PlayerDisconnect += EventOnPlayerDisconnect;
            //OnlinePlayers[FiredBy.PlayerUID] = true;
            msLaunch = World.ElapsedMilliseconds;
            collisionTestBox = SelectionBox.Clone().OmniGrowBy(0.15f);
            GetBehavior<EntityBehaviorPassivePhysics>().OnPhysicsTickCallback = onPhysicsTickCallback;
            GetBehavior<EntityBehaviorPassivePhysics>().collisionYExtra = 0f; // Slightly cheap hax so that stones/arrows don't collid with fences

        }
        private void EventOnPlayerDisconnect(IServerPlayer byplayer)
        {
            if (byplayer.PlayerUID == FiredBy.PlayerUID) Die();
        }
        private void onPhysicsTickCallback(float dtFac)
        {
            if (ShouldDespawn || !Alive) return;
            if (World.ElapsedMilliseconds <= msCollide + 500) return;

            EntityPos pos = SidedPos;

            Cuboidd projectileBox = SelectionBox.ToDouble().Translate(pos.X, pos.Y, pos.Z);

            if (pos.Motion.X < 0) projectileBox.X1 += pos.Motion.X * dtFac;
            else projectileBox.X2 += pos.Motion.X * dtFac;
            if (pos.Motion.Y < 0) projectileBox.Y1 += pos.Motion.Y * dtFac;
            else projectileBox.Y2 += pos.Motion.Y * dtFac;
            if (pos.Motion.Z < 0) projectileBox.Z1 += pos.Motion.Z * dtFac;
            else projectileBox.Z2 += pos.Motion.Z * dtFac;
        }

        //bool grappled = false;
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
            if (ShouldDespawn || !Alive) return;
            if (FiredBy is null)
            {
                FiredBy = Api.World.GetEntityById(FiredById) as EntityPlayer;
                if (FiredBy is null) FiredBy.WatchedAttributes.SetBool("fired", false); Console.WriteLine("despawn case 1"); return;
            }
            //Console.WriteLine(Api.Side + "M " + MaxLength);
            EntityPos pos = SidedPos;
            if (!FiredBy.WatchedAttributes.GetAsBool("fired"))
            {
                Die();
            }
            if (anchorPoint == null)
            {
                if (WatchedAttributes.GetBool("stuck"))
                {
                    anchorPoint = SidedPos.XYZ;
                    stuck = true;
                }
                else
                {
                    return;
                }

            }
            if (capi != null)
            {
                if (capi.Input.KeyboardKeyState[capi.Input.GetHotKeyByCode("hoist").CurrentMapping.KeyCode])
                {
                    if (MaxLength > 2) MaxLength -= 0.15;
                }
                if (capi.Input.KeyboardKeyState[capi.Input.GetHotKeyByCode("rappell").CurrentMapping.KeyCode])
                {
                    MaxLength += 0.3;
                }
            }
            double L = FiredBy.Pos.DistanceTo(anchorPoint);
            if (MaxLength == 0) MaxLength = FiredBy.Pos.DistanceTo(Pos);
            FiredBy.PositionBeforeFalling = FiredBy.Pos.XYZ;
            if (FiredBy != null && collTester.IsColliding(World.BlockAccessor, collisionTestBox, pos.XYZ)) //&& !grappled)
            {
                if (L > MaxLength && L > 2) // + 0.2
                {
                    double theta = Math.Atan2(FiredBy.Pos.X - anchorPoint.X, FiredBy.Pos.Y - anchorPoint.Y);
                    double phi = Math.Atan2(FiredBy.Pos.Z - anchorPoint.Z, FiredBy.Pos.Y - anchorPoint.Y);
                    Vec3d radialDistance = FiredBy.Pos.XYZ.SubCopy(anchorPoint);
                    double radialDistanceMag = radialDistance.Length();
                    Vec3d radialDirection = radialDistance.Normalize();
                    Vec3d acceleration = radialDirection * SpringConst * Math.Abs(radialDistanceMag - MaxLength);
                    double damping = 2f * Math.Sqrt(SpringConst);
                    Vec3d TangVel = FiredBy.Pos.Motion - GetProjectionOn(FiredBy.Pos.Motion, radialDirection);
                    //FiredBy.ServerPos.Motion.Add(acceleration * dt);
                    FiredBy.Pos.Motion.Add(acceleration * dt);
                    //FiredBy.ServerPos.Motion.Add(-damping * GetProjectionOn(FiredBy.Pos.Motion, radialDirection));
                    FiredBy.Pos.Motion.Add(-damping * GetProjectionOn(FiredBy.Pos.Motion, radialDirection));
                    double ThetaDegrees = theta * 180 / Math.PI;
                    double PhiDegrees = phi * 180 / Math.PI;
                    if (capi != null)
                    {
                        if (!capi.Input.KeyboardKeyState[(int)GlKeys.Space])
                        {
                            if (FiredBy.ServerPos.Motion.Length() < 0.3f && ((ThetaDegrees > 135 && ThetaDegrees < 180) || (ThetaDegrees > -180 && ThetaDegrees < -135)))
                            {
                                //FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                                FiredBy.SidedPos.Motion.Add(TangVel * FunConstant);
                            }
                            if (FiredBy.ServerPos.Motion.Length() < 0.3f && ((PhiDegrees > 135 && PhiDegrees < 180) || (PhiDegrees > -180 && PhiDegrees < -135)))
                            {
                                //FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                                FiredBy.SidedPos.Motion.Add(TangVel * FunConstant);
                            }
                        }
                        else
                        {
                            if (FiredBy.ServerPos.Motion.Length() < 0.3f && ((ThetaDegrees > 135 && ThetaDegrees < 180) || (ThetaDegrees > -180 && ThetaDegrees < -135)))
                            {
                                //FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                                FiredBy.SidedPos.Motion.Add(-1 * TangVel * FunConstant);
                            }
                            if (FiredBy.ServerPos.Motion.Length() < 0.3f && ((PhiDegrees > 135 && PhiDegrees < 180) || (PhiDegrees > -180 && PhiDegrees < -135)))
                            {
                                //FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                                FiredBy.SidedPos.Motion.Add(-1 * TangVel * FunConstant);
                            }
                        }
                    }
                }
            }
            stuck = Collided || collTester.IsColliding(World.BlockAccessor, collisionTestBox, pos.XYZ) || WatchedAttributes.GetBool("stuck");
            if (Api.Side == EnumAppSide.Server) WatchedAttributes.SetBool("stuck", stuck);

            double impactSpeed = Math.Max(motionBeforeCollide.Length(), pos.Motion.Length());

            if (stuck)
            {
                if (Api.Side == EnumAppSide.Client) ServerPos.SetFrom(pos);
                IsColliding(pos, impactSpeed);
                return;
            }
            else
            {
                SetRotation();
            }

            beforeCollided = false;
            motionBeforeCollide.Set(pos.Motion.X, pos.Motion.Y, pos.Motion.Z);
        }

        public static Vec3d GetProjectionOn(Vec3d vector, Vec3d direction)
        {
            return direction * (vector.Dot(direction) / Math.Sqrt(direction.Dot(direction)));
        }
        public override void OnCollided()
        {
            RopeCount = 0;
            if (FiredBy is null)
            {
                FiredBy = Api.World.GetEntityById(FiredById) as EntityPlayer;
                if (FiredBy is null) FiredBy.WatchedAttributes.SetBool("fired", false); return;
            }
            foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
            {
                if (itemSlot?.Itemstack?.Item.Code.ToString() == "game:rope")
                {
                    RopeCount += itemSlot.Itemstack.StackSize * 3 / 2;
                }
            }
            if (this.ServerPos.DistanceTo(FiredBy.ServerPos) > RopeCount) // > totalRope);
            {
                FiredBy.WatchedAttributes.SetBool("fired", false);
                WatchedAttributes.MarkAllDirty();
            }
            EntityPos pos = SidedPos;
            pos.Motion.Set(0, 0, 0);

            anchorPoint = Pos.XYZ;
            
            if (Api.Side == EnumAppSide.Server) MaxLength = FiredBy.ServerPos.DistanceTo(ServerPos);
            else MaxLength = FiredBy.Pos.DistanceTo(Pos);
            WatchedAttributes.MarkAllDirty();
            motionBeforeCollide.Set(pos.Motion.X, pos.Motion.Y, pos.Motion.Z);
        }

        private void IsColliding(EntityPos pos, double impactSpeed)
        {
            pos.Motion.Set(0, 0, 0);

            if (!beforeCollided && World is IServerWorldAccessor)
            {
                WatchedAttributes.MarkAllDirty();
                msCollide = World.ElapsedMilliseconds;
                beforeCollided = true;
            }
        }

        private void ImpactOnEntity(Entity entity)
        {
            if (!Alive) return;

            EntityPos pos = SidedPos;

            IServerPlayer fromPlayer = null;
            if (FiredBy is EntityPlayer)
            {
                fromPlayer = FiredBy.Player as IServerPlayer;
            }

            bool targetIsPlayer = entity is EntityPlayer;
            bool targetIsCreature = entity is EntityAgent;
            bool canDamage = true;

            ICoreServerAPI sapi = World.Api as ICoreServerAPI;
            if (fromPlayer != null)
            {
                if (targetIsPlayer && (!sapi.Server.Config.AllowPvP || !fromPlayer.HasPrivilege("attackplayers"))) canDamage = false;
                if (targetIsCreature && !fromPlayer.HasPrivilege("attackcreatures")) canDamage = false;
            }

            msCollide = World.ElapsedMilliseconds;

            pos.Motion.Set(0, 0, 0);

            if (canDamage && World.Side == EnumAppSide.Server)
            {
                World.PlaySoundAt(new AssetLocation("sounds/arrow-impact"), this, null, false, 24);

                float dmg = Damage;
                if (FiredBy != null) dmg *= FiredBy.Stats.GetBlended("rangedWeaponsDamage");

                bool didDamage = entity.ReceiveDamage(new DamageSource()
                {
                    Source = fromPlayer != null ? EnumDamageSource.Player : EnumDamageSource.Entity,
                    SourceEntity = this,
                    CauseEntity = FiredBy,
                    Type = EnumDamageType.PiercingAttack
                }, dmg);

                float kbresist = entity.Properties.KnockbackResistance;
                entity.SidedPos.Motion.Add(kbresist * pos.Motion.X * Weight, kbresist * pos.Motion.Y * Weight, kbresist * pos.Motion.Z * Weight);

                int leftDurability = 1;
                if (DamageStackOnImpact)
                {
                    ProjectileStack.Collectible.DamageItem(entity.World, entity, new DummySlot(ProjectileStack));
                    leftDurability = ProjectileStack == null ? 1 : ProjectileStack.Collectible.GetRemainingDurability(ProjectileStack);
                }

                if (World.Rand.NextDouble() < DropOnImpactChance && leftDurability > 0)
                {

                }
                else
                {
                    FiredBy.WatchedAttributes.SetBool("fired", false);

                    Die();
                }

                if (FiredBy is EntityPlayer && didDamage)
                {
                    World.PlaySoundFor(new AssetLocation("sounds/player/projectilehit"), FiredBy.Player, false, 24);
                }
            }
        }

        public virtual void SetRotation()
        {
            EntityPos pos = (World is IServerWorldAccessor) ? ServerPos : Pos;

            double speed = pos.Motion.Length();

            if (speed > 0.01)
            {
                pos.Pitch = 0;
                pos.Yaw =
                    GameMath.PI + (float)Math.Atan2(pos.Motion.X / speed, pos.Motion.Z / speed)
                    + GameMath.Cos((World.ElapsedMilliseconds - msLaunch) / 200f) * 0.03f
                ;
                pos.Roll =
                    -(float)Math.Asin(GameMath.Clamp(-pos.Motion.Y / speed, -1, 1))
                    + GameMath.Sin((World.ElapsedMilliseconds - msLaunch) / 200f) * 0.03f
                ;
            }
        }

        public override bool CanCollect(Entity byEntity)
        {
            return false;
            /*if (byEntity is EntityPlayer player) return player.Controls.Sneak;
            return Alive && World.ElapsedMilliseconds - msLaunch > 1000 && ServerPos.Motion.Length() < 0.01;*/
        }
        public override ItemStack OnCollected(Entity byEntity)
        {
            ProjectileStack.ResolveBlockOrItem(World);
            return ProjectileStack;
        }
        public override void OnCollideWithLiquid()
        {
            base.OnCollideWithLiquid();
        }
        public override void ToBytes(BinaryWriter writer, bool forClient)
        {
            base.ToBytes(writer, forClient);
            writer.Write(FiredById);
            writer.Write(beforeCollided);
            ProjectileStack.ToBytes(writer);
        }

        public override void FromBytes(BinaryReader reader, bool fromServer)
        {
            base.FromBytes(reader, fromServer);
            FiredById = reader.ReadInt64();
            beforeCollided = reader.ReadBoolean();
            ProjectileStack = new ItemStack(reader);
        }
    }
}