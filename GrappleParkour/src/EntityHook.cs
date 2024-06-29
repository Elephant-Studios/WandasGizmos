using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace GrappleParkour
{
    public class EntityHook : Entity
    {
        bool beforeCollided;
        bool stuck;

        long msLaunch;
        long msCollide;

        Vec3d motionBeforeCollide = new Vec3d();

        CollisionTester collTester = new CollisionTester();

        public EntityAgent FiredBy;
        public float Weight = 0.1f;
        public float Damage;
        public ItemStack ProjectileStack;
        public float DropOnImpactChance = 0f;
        public bool DamageStackOnImpact = false;
        public float SpringConst = -0.075f;
        public long EntityID;
        public Vec3d anchorPoint;

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
            FiredBy = Api.World.GetEntityById(EntityID) as EntityAgent;
            msLaunch = World.ElapsedMilliseconds;
            //anchorPoint = FiredBy.Pos.XYZ;
            collisionTestBox = SelectionBox.Clone().OmniGrowBy(0.05f);

            GetBehavior<EntityBehaviorPassivePhysics>().OnPhysicsTickCallback = onPhysicsTickCallback;
            GetBehavior<EntityBehaviorPassivePhysics>().collisionYExtra = 0f; // Slightly cheap hax so that stones/arrows don't collid with fences
        }

        private void onPhysicsTickCallback(float dtFac)
        {
            if (ShouldDespawn || !Alive) return;
            if (World.ElapsedMilliseconds <= msCollide + 500) return;

            var pos = SidedPos;
            
            Cuboidd projectileBox = SelectionBox.ToDouble().Translate(pos.X, pos.Y, pos.Z);

            if (pos.Motion.X < 0) projectileBox.X1 += pos.Motion.X * dtFac;
            else projectileBox.X2 += pos.Motion.X * dtFac;
            if (pos.Motion.Y < 0) projectileBox.Y1 += pos.Motion.Y * dtFac;
            else projectileBox.Y2 += pos.Motion.Y * dtFac;
            if (pos.Motion.Z < 0) projectileBox.Z1 += pos.Motion.Z * dtFac;
            else projectileBox.Z2 += pos.Motion.Z * dtFac;
        }

        bool grappled = false;
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
            if (ShouldDespawn) return;
            EntityPos pos = SidedPos;
            if (FiredBy == null) FiredBy = (Api as ICoreClientAPI)?.World.Player.Entity;
            if (anchorPoint == null) return;
            if (FiredBy != null && collTester.IsColliding(World.BlockAccessor, collisionTestBox, pos.XYZ) && !grappled)
            {
                Vec3d velocity = SpringConst * (FiredBy.Pos.XYZ - anchorPoint);
                FiredBy.ServerPos.Motion.Set(velocity);
                FiredBy.Pos.Motion.Set(velocity);
                Console.WriteLine(Api.Side);
                Console.WriteLine(FiredBy.ServerPos.Motion);
                grappled = true;
            }
            stuck = Collided || collTester.IsColliding(World.BlockAccessor, collisionTestBox, pos.XYZ) || WatchedAttributes.GetBool("stuck");
            if (Api.Side == EnumAppSide.Server) WatchedAttributes.SetBool("stuck", stuck);

            double impactSpeed = Math.Max(motionBeforeCollide.Length(), pos.Motion.Length());

            if (stuck)
            {
                if (Api.Side == EnumAppSide.Client) ServerPos.SetFrom(Pos);
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


        public override void OnCollided()
        {
            EntityPos pos = SidedPos;
            anchorPoint = pos.XYZ;
            IsColliding(SidedPos, Math.Max(motionBeforeCollide.Length(), pos.Motion.Length()));
            motionBeforeCollide.Set(pos.Motion.X, pos.Motion.Y, pos.Motion.Z);
        }


        private void IsColliding(EntityPos pos, double impactSpeed)
        {
            pos.Motion.Set(0, 0, 0);

            if (!beforeCollided && World is IServerWorldAccessor && World.ElapsedMilliseconds > msCollide + 500)
            {
                if (impactSpeed >= 0.07)
                {
                    World.PlaySoundAt(new AssetLocation("sounds/arrow-impact"), this, null, false, 32);

                    // Resend position to client
                    WatchedAttributes.MarkAllDirty();

                    if (DamageStackOnImpact)
                    {
                        ProjectileStack.Collectible.DamageItem(World, this, new DummySlot(ProjectileStack));
                        int leftDurability = ProjectileStack == null ? 1 : ProjectileStack.Collectible.GetRemainingDurability(ProjectileStack);
                        if (leftDurability <= 0)
                        {
                            Die();
                        }
                    }
                }

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
                fromPlayer = (FiredBy as EntityPlayer).Player as IServerPlayer;
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
                    Die();
                }

                if (FiredBy is EntityPlayer && didDamage)
                {
                    World.PlaySoundFor(new AssetLocation("sounds/player/projectilehit"), (FiredBy as EntityPlayer).Player, false, 24);
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
            return Alive && World.ElapsedMilliseconds - msLaunch > 1000 && ServerPos.Motion.Length() < 0.01;
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
            writer.Write(EntityID);
            writer.Write(beforeCollided);
            ProjectileStack.ToBytes(writer);
        }

        public override void FromBytes(BinaryReader reader, bool fromServer)
        {
            base.FromBytes(reader, fromServer);
            EntityID = reader.ReadInt64();
            beforeCollided = reader.ReadBoolean();
            ProjectileStack = new ItemStack(reader);
        }
    }
}