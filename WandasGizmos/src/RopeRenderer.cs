using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WandasGizmos.src
{
    public class RopeRenderer : EntityShapeRenderer
    {
        public MeshRef ropeLineMesh;
        public int lineTexture;
        public EntityHook hook;
        public float[] normalFov;
        public float[] handFov;
        public Matrixf matrix = new Matrixf();
        public Vec3f offset = new Vec3f();

        public RopeRenderer(Entity entity, ICoreClientAPI api)
          : base(entity, api)
        {
            this.CreateCrossFishingLine(0.025f);
            this.hook = (EntityHook)entity;
            this.lineTexture = ((EntityRenderer)this).capi.Render.GetOrLoadTexture(new AssetLocation("wgmt:rope/rope.png"));
            float num = (float)((EntityRenderer)this).capi.Settings.Int["fpHandsFoV"] * ((float)Math.PI / 180f);
            this.normalFov = (float[])((EntityRenderer)this).capi.Render.CurrentProjectionMatrix.Clone();
            ((EntityRenderer)this).capi.Render.Set3DProjection(((EntityRenderer)this).capi.Render.ShaderUniforms.ZFar, num);
            this.handFov = (float[])((EntityRenderer)this).capi.Render.CurrentProjectionMatrix.Clone();
            ((EntityRenderer)this).capi.Render.Reset3DProjection();
        }

        public virtual void DoRender3DOpaque(float dt, bool isShadowPass)
        {
            base.DoRender3DOpaque(dt, isShadowPass);
            if (!(((IWorldAccessor)((EntityRenderer)this).capi.World).GetEntityById(this.hook.FiredById) is EntityPlayer entityById) || !(entityById.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible is ItemGrapplingHook collectible))
                return;
            EntityPlayer entity = ((IPlayer)((EntityRenderer)this).capi.World.Player).Entity;
            bool flag = entityById == entity && ((EntityRenderer)this).capi.World.Player.CameraMode == EnumCameraMode.ThirdPerson && !isShadowPass;
            AttachmentPointAndPose attachmentPointPose = ((Entity)entityById).AnimManager.Animator?.GetAttachmentPointPose("RightHand");
            if (attachmentPointPose == null)
                return;
            AttachmentPoint attachPoint = attachmentPointPose.AttachPoint;
            double num1 = attachPoint.RotationX * (Math.PI / 180.0);
            double num2 = attachPoint.RotationY * (Math.PI / 180.0);
            double num3 = attachPoint.RotationZ * (Math.PI / 180.0);
            this.matrix.Identity();
            this.matrix.RotateX(((Entity)entityById).Pos.Roll);
            this.matrix.RotateY(((EntityAgent)entityById).BodyYaw);
            if (flag)
            {
                float num4 = ((EntityAgent)entityById).Controls.IsFlying ? -1f : -0.75f;
                this.matrix.Translate(0.0f, ((EntityRenderer)this).capi.Settings.Float["fpHandsYOffset"], 0.0f);
                this.matrix.Translate(0.0, ((Entity)entityById).LocalEyePos.Y, 0.0);
                this.matrix.RotateZ((((Entity)entityById).Pos.Pitch - 3.14159274f) * num4);
                this.matrix.Translate(0.0, -((Entity)entityById).LocalEyePos.Y, 0.0);
            }
            this.matrix.RotateX((float)num1).RotateY((float)num2).RotateZ((float)num3).Translate(attachPoint.PosX / 16.0 - 0.5, attachPoint.PosY / 16.0, attachPoint.PosZ / 16.0 - 0.5).Mul(attachmentPointPose.AnimModelMatrix);
            //this.matrix.Translate(-(double)collectible.lineOffset, 0.1, 0.0);
            Vec4f vec4f1 = this.matrix.TransformVector(new Vec4f(0.0f, 0.0f, 0.0f, 1f));
            if (flag)
            {
                Vec4f vec4f2 = new Matrixf().Set(this.handFov).Mul(((EntityRenderer)this).capi.Render.CameraMatrixOriginf).TransformVector(vec4f1);
                vec4f1 = new Matrixf(((EntityRenderer)this).capi.Render.CameraMatrixOriginf).Invert().Mul(new Matrixf(this.normalFov).Invert()).TransformVector(vec4f2);
            }
            Vec3d vec3d = new Vec3d();
            if (entityById == entity)
                vec3d.Set((double)vec4f1.X, (double)vec4f1.Y, (double)vec4f1.Z);
            else
            {
                vec3d.Set((double)vec4f1.X + ((Entity)entityById).Pos.X - entity.CameraPos.X, (double)vec4f1.Y + ((Entity)entityById).Pos.Y - entity.CameraPos.Y, (double)vec4f1.Z + ((Entity)entityById).Pos.Z - entity.CameraPos.Z);
            }
            /*if (isShadowPass)
            {
                IShaderProgram currentActiveShader = ((EntityRenderer)this).capi.Render.CurrentActiveShader;
                currentActiveShader.Stop();
                IShaderProgram fishingLineShadow = FishingMod.FishingLineShadow;
                fishingLineShadow.Use();
                Matrixf matrixf = new Matrixf().Translate(vec3d.X, vec3d.Y, vec3d.Z);
                float[] numArray = Mat4f.Mul(matrixf.Values, ((EntityRenderer)this).capi.Render.CurrentModelviewMatrix, matrixf.Values);
                Mat4f.Mul(numArray, ((EntityRenderer)this).capi.Render.CurrentProjectionMatrix, numArray);
                fishingLineShadow.UniformMatrix("mvpMatrix", numArray);
                this.offset.Set((float)(((EntityRenderer)this).entity.Pos.X - entity.CameraPos.X - vec3d.X), (float)(((EntityRenderer)this).entity.Pos.Y + 0.10000000149011612 - entity.CameraPos.Y - vec3d.Y), (float)(((EntityRenderer)this).entity.Pos.Z - entity.CameraPos.Z - vec3d.Z));
                fishingLineShadow.Uniform("offset", this.offset);
                fishingLineShadow.Uniform("droop", 1f);
                ((EntityRenderer)this).capi.Render.GlDisableCullFace();
                ((EntityRenderer)this).capi.Render.RenderMesh(this.fishingLineMesh);
                ((EntityRenderer)this).capi.Render.GlEnableCullFace();
                fishingLineShadow.Stop();
                currentActiveShader.Use();
            }*/
            if (!isShadowPass)
            {
                IShaderProgram ropeLine = WandasGizmos.RopeLine;
                ropeLine.Use();
                ropeLine.UniformMatrix("modelMatrix", new Matrixf().Translate(vec3d.X, vec3d.Y, vec3d.Z).Values);
                ropeLine.UniformMatrix("viewMatrix", ((EntityRenderer)this).capi.Render.CameraMatrixOriginf);
                ropeLine.UniformMatrix("projectionMatrix", ((EntityRenderer)this).capi.Render.CurrentProjectionMatrix);
                ropeLine.BindTexture2D("tex2d", this.lineTexture, 0);
                this.offset.Set((float)(((EntityRenderer)this).entity.Pos.X - entity.CameraPos.X - vec3d.X), (float)(((EntityRenderer)this).entity.Pos.Y + 0.10000000149011612 - entity.CameraPos.Y - vec3d.Y), (float)(((EntityRenderer)this).entity.Pos.Z - entity.CameraPos.Z - vec3d.Z));
                ropeLine.Uniform("offset", this.offset);
                ropeLine.Uniform("color", new Vec3f(0.8f, 0.8f, 0.8f));
                ropeLine.Uniform("droop", 1f);
                ropeLine.Uniform("rgbaAmbientIn", ((EntityRenderer)this).capi.Render.AmbientColor);
                ropeLine.Uniform("rgbaLightIn", this.lightrgbs);
                ropeLine.Uniform("rgbaFogIn", ((EntityRenderer)this).capi.Render.FogColor);
                ropeLine.Uniform("fogMinIn", ((EntityRenderer)this).capi.Render.FogMin);
                ropeLine.Uniform("fogDensityIn", ((EntityRenderer)this).capi.Render.FogDensity);
                ((EntityRenderer)this).capi.Render.GlDisableCullFace();
                ((EntityRenderer)this).capi.Render.RenderMesh(this.ropeLineMesh);
                ((EntityRenderer)this).capi.Render.GlEnableCullFace();
                ((EntityRenderer)this).capi.Render.CurrentActiveShader.Stop();
            }
        }

        public virtual void Dispose()
        {
            base.Dispose();
            this.ropeLineMesh?.Dispose();
        }

        public void CreateCrossFishingLine(float width)
        {
            this.ropeLineMesh?.Dispose();
            MeshData meshData1 = new MeshData(42, 42, false, true, false, false);
            meshData1.SetMode((EnumDrawMode)0);
            for (int index = 0; index < 21; ++index)
            {
                float num = (float)index / 20f;
                meshData1.AddVertex(-width, width, 0.0f, num, 0.1f);
                meshData1.AddVertex(width, -width, 0.0f, num, -0.1f);
            }
            for (int index = 0; index < 20; ++index)
            {
                int num = 2 * index;
                meshData1.AddIndex(num);
                meshData1.AddIndex(num + 3);
                meshData1.AddIndex(num + 2);
                meshData1.AddIndex(num);
                meshData1.AddIndex(num + 1);
                meshData1.AddIndex(num + 3);
            }
            MeshData meshData2 = new MeshData(42, 42, false, true, false, false);
            meshData2.SetMode((EnumDrawMode)0);
            for (int index = 0; index < 21; ++index)
            {
                float num = (float)index / 20f;
                meshData2.AddVertex(-width, -width, 0.0f, num, 0.1f);
                meshData2.AddVertex(width, width, 0.0f, num, -0.1f);
            }
            for (int index = 0; index < 20; ++index)
            {
                int num = 2 * index;
                meshData2.AddIndex(num);
                meshData2.AddIndex(num + 3);
                meshData2.AddIndex(num + 2);
                meshData2.AddIndex(num);
                meshData2.AddIndex(num + 1);
                meshData2.AddIndex(num + 3);
            }
            meshData1.AddMeshData(meshData2);
            this.ropeLineMesh = ((EntityRenderer)this).capi.Render.UploadMesh(meshData1);
        }
    }
}
