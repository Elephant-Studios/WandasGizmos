using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace WandasGizmos
{
    public class EntityHook : Entity
    {
        bool beforeCollided;
        bool stuck;

        long msLaunch;
        long msCollide;

        Vec3d motionBeforeCollide = new Vec3d();

        CollisionTester collTester = new CollisionTester();

        public long FiredById;
        public EntityPlayer FiredBy = null!;
        public float Weight = 0.1f;
        public float Damage;
        public ItemStack ProjectileStack;
        public float DropOnImpactChance = 0f;
        public bool DamageStackOnImpact = false;
        public float SpringConst = 0.5f;
        public double MaxLength;
        public int RopeCount;
        public double FunConstant = 0.01f;
        public Vec3d anchorPoint;
        public ItemSlot HookSlot = null!;
        public ItemStack hook = null!;
        public Entity CollidedEntity;

        Cuboidf collisionTestBox;

        EntityPartitioning ep;

        public override bool ApplyGravity
        {
            get { return stuck; }
        }

        public override bool IsInteractable
        {
            get { return false; }
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
            //for (FiredBy.GearInventory[0]) march through inventory on initialize
            //FiredBy = Api.World.GetEntityById(EntityID) as EntityAgent;
            if (api.World.GetEntityById(this.FiredById) is EntityPlayer entityById)
            {
                this.FiredBy = entityById;
            }
            else
            {
                Console.WriteLine("death: not fired by player");
                Die();
            }   
        msLaunch = World.ElapsedMilliseconds;
            //anchorPoint = FiredBy.Pos.XYZ;
            collisionTestBox = SelectionBox.Clone().OmniGrowBy(0.05f);
            GetBehavior<EntityBehaviorPassivePhysics>().OnPhysicsTickCallback = onPhysicsTickCallback;
            GetBehavior<EntityBehaviorPassivePhysics>().collisionYExtra = 0f; // Slightly cheap hax so that stones/arrows don't collid with fences
            ep = api.ModLoader.GetModSystem<EntityPartitioning>();
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
            ep.WalkEntities(pos.XYZ, 5f, (e) => {
                if (e.EntityId == this.EntityId || (FiredBy != null && e.EntityId == FiredBy.EntityId && World.ElapsedMilliseconds - msLaunch < 500) || !e.IsInteractable) return true;

                Cuboidd eBox = e.SelectionBox.ToDouble().Translate(e.ServerPos.X, e.ServerPos.Y, e.ServerPos.Z);

                if (eBox.IntersectsOrTouches(projectileBox))
                {
                    foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
                    {
                        if (itemSlot?.Itemstack?.Id == 1701)
                        {
                            RopeCount += itemSlot.Itemstack.StackSize;
                        }
                    }
                    RopeCount *= 3;
                    if (this.ServerPos.DistanceTo(FiredBy.Pos) > RopeCount) // > totalRope);
                    {
                        Console.WriteLine("death: player too far");
                        //HookSlot.Itemstack.Attributes.RemoveAttribute("hookId");
                        Die();
                    }
                    anchorPoint = pos.XYZ;
                    MaxLength = FiredBy.Pos.DistanceTo(anchorPoint);
                    CollidedEntity = e;
                    ImpactOnEntity(e);
                    return false;
                }

                return true;
            }, EnumEntitySearchType.Creatures);
        }

        //bool grappled = false;
        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
            if (FiredBy == null) FiredBy = (Api as ICoreClientAPI)?.World.Player.Entity;
            if (HookSlot == null) HookSlot = FiredBy.Player.InventoryManager.ActiveHotbarSlot;
            //Console.WriteLine(HookSlot.Itemstack.Id);

            if (CollidedEntity != null) //if it has hit something, actively set it to that
            {
                //Console.WriteLine("hit " + CollidedEntity.EntityId);
                anchorPoint = CollidedEntity.Pos.XYZ;
                //MaxLength = FiredBy.Pos.DistanceTo(anchorPoint);
                //Console.WriteLine("L " + MaxLength);
                //Console.WriteLine("A" + anchorPoint.ToString());
            }
            //else if (anchorPoint == null) Die(); //if it hasn't hit something, AND anchorPoint is undefined, die.
            
            if (HookSlot.Itemstack?.Attributes.TryGetBool("pull") == true)
            {
                MaxLength -= 0.5;
            }
            else if (HookSlot.Itemstack?.Attributes.TryGetBool("push") == true)
            {
                Console.WriteLine(RopeCount + " r");
                Console.WriteLine(MaxLength + " m");   
                if (MaxLength + -0.1 < RopeCount)
                {
                    MaxLength += 0.5;
                }  
            }
            else if (this.FiredBy.Player.InventoryManager.ActiveHotbarSlot.Itemstack != this.HookSlot.Itemstack)
            {
                //this.HookSlot.Itemstack.Attributes.RemoveAttribute("hookId");
                Console.WriteLine("death: switched hotbar slots");
                Die();
            }
            if (ShouldDespawn) return;
            EntityPos pos = SidedPos;
            if (anchorPoint == null) return;
            if (FiredBy != null && (collTester.IsColliding(World.BlockAccessor, collisionTestBox, pos.XYZ) || CollidedEntity != null)) //&& !grappled)
            {
                double CurrentLength = FiredBy.Pos.DistanceTo(anchorPoint);
                Console.WriteLine((CurrentLength >= MaxLength + 0.3) + " " + CurrentLength + " " + MaxLength);
                if (CurrentLength >= MaxLength + 0.2)
                {
                    double theta = Math.Atan2(FiredBy.Pos.X - anchorPoint.X, FiredBy.Pos.Y - anchorPoint.Y);
                    double phi = Math.Atan2(FiredBy.Pos.Z - anchorPoint.Z, FiredBy.Pos.Y - anchorPoint.Y);
                    Vec3d radialDistance = FiredBy.Pos.XYZ.SubCopy(anchorPoint);
                    double radialDistanceMag = radialDistance.Length();
                    Vec3d radialDirection = radialDistance.Normalize();
                    Vec3d acceleration = radialDirection * SpringConst * Math.Abs(radialDistanceMag - MaxLength);
                    var damping = 2f * Math.Sqrt(SpringConst);
                    Vec3d TangVel = FiredBy.Pos.Motion - GetProjectionOn(FiredBy.Pos.Motion, radialDirection);
                    FiredBy.ServerPos.Motion.Add(acceleration * dt);
                    FiredBy.Pos.Motion.Add(acceleration * dt);
                    FiredBy.ServerPos.Motion.Add(-damping * GetProjectionOn(FiredBy.Pos.Motion, radialDirection));
                    FiredBy.Pos.Motion.Add(-damping * GetProjectionOn(FiredBy.Pos.Motion, radialDirection));
                    var ThetaDegrees = theta * 180 / Math.PI;
                    var PhiDegrees = phi * 180 / Math.PI;
                    if (FiredBy.Pos.Motion.Length() < 0.3f && ((ThetaDegrees > 135 && ThetaDegrees < 180) || (ThetaDegrees > -180 && ThetaDegrees < -135)))
                    {
                        FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                        FiredBy.Pos.Motion.Add(TangVel * FunConstant);
                        //Console.WriteLine("theta multiplier used");
                    }
                    if (FiredBy.Pos.Motion.Length() < 0.3f && ((PhiDegrees > 135 && PhiDegrees < 180) || (PhiDegrees > -180 && PhiDegrees < -135)))
                    {
                        FiredBy.ServerPos.Motion.Add(TangVel * FunConstant);
                        FiredBy.Pos.Motion.Add(TangVel * FunConstant);
                        //Console.WriteLine("phi multiplier used");
                    }
                }
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

        public static Vec3d GetProjectionOn(Vec3d vector, Vec3d direction)
        {
            return direction * (vector.Dot(direction) / Math.Sqrt(direction.Dot(direction)));
        }
        public override void OnCollided()
        {
            foreach (ItemSlot itemSlot in FiredBy.Player.InventoryManager.GetHotbarInventory())
            {
                if (itemSlot?.Itemstack?.Id == 1701)
                {
                    RopeCount += itemSlot.Itemstack.StackSize;
                }
            }
            RopeCount *= 3;
            if (this.ServerPos.DistanceTo(FiredBy.Pos) > RopeCount) // > totalRope);
            {
                Console.WriteLine("death: player too far");
                //HookSlot.Itemstack.Attributes.RemoveAttribute("hookId");
                Die();
            }
            EntityPos pos = SidedPos;
            anchorPoint = pos.XYZ;
            MaxLength = FiredBy.Pos.DistanceTo(anchorPoint);
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
                            Console.WriteLine("death: durability");
                            Die();
                        }
                    }
                }

                msCollide = World.ElapsedMilliseconds;

                beforeCollided = true;
            }
        }

        bool TryAttackEntity(double impactSpeed)
        {
            if (World is IClientWorldAccessor || World.ElapsedMilliseconds <= msCollide + 250) return false;
            if (impactSpeed <= 0.01) return false;

            EntityPos pos = SidedPos;

            Cuboidd projectileBox = SelectionBox.ToDouble().Translate(ServerPos.X, ServerPos.Y, ServerPos.Z);

            // We give it a bit of extra leeway of 50% because physics ticks can run twice or 3 times in one game tick 
            if (ServerPos.Motion.X < 0) projectileBox.X1 += 1.5 * ServerPos.Motion.X;
            else projectileBox.X2 += 1.5 * ServerPos.Motion.X;
            if (ServerPos.Motion.Y < 0) projectileBox.Y1 += 1.5 * ServerPos.Motion.Y;
            else projectileBox.Y2 += 1.5 * ServerPos.Motion.Y;
            if (ServerPos.Motion.Z < 0) projectileBox.Z1 += 1.5 * ServerPos.Motion.Z;
            else projectileBox.Z2 += 1.5 * ServerPos.Motion.Z;

            Entity entity = World.GetNearestEntity(ServerPos.XYZ, 5f, 5f, (e) => {
                if (e.EntityId == this.EntityId || !e.IsInteractable) return false;

                if (FiredBy != null && e.EntityId == FiredBy.EntityId && World.ElapsedMilliseconds - msLaunch < 500)
                {
                    return false;
                }

                Cuboidd eBox = e.SelectionBox.ToDouble().Translate(e.ServerPos.X, e.ServerPos.Y, e.ServerPos.Z);

                return eBox.IntersectsOrTouches(projectileBox);
            });

            if (entity != null)
            {
                ImpactOnEntity(entity);
                return true;
            }


            return false;
        }


        private void ImpactOnEntity(Entity entity)
        {
            if (!Alive) return;
            Console.WriteLine("hit entity");

            EntityPos pos = SidedPos;

            IServerPlayer fromPlayer = null;
            if (FiredBy is EntityPlayer)
            {
                fromPlayer = (FiredBy as EntityPlayer).Player as IServerPlayer;
            }

            bool targetIsPlayer = entity is EntityPlayer;
            bool targetIsCreature = entity is EntityAgent;
            bool canDamage = false;

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
                    Console.WriteLine("death: random break chance");
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