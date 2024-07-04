using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Config;
public class RopeRenderer : IRenderer
{
    private readonly ICoreClientAPI _api;

    private readonly float[] _modelMat = Mat4f.Create();
    private Vec3d APoint;
    private Vec3d BPoint;
    private MeshRef modelMesh;
    private TextureAtlasPosition texturePosition;

    private readonly IRenderAPI _rpi;

    public double RenderOrder => 0.41;

    public int RenderRange => 200;


    //setting up variables, api, and renderer
    public RopeRenderer(ICoreClientAPI api, Vec3d APoint , Vec3d BPoint)
    {
        _api = api;
        _rpi = _api.Render;
        this.APoint = APoint;
        this.BPoint = BPoint;
        CreateRopeMesh();
    }


    private void CreateRopeMesh()
    {
        modelMesh = Api.Assets.TryGet("WGMT:shapes/rope.json")?.ToObject<MeshRef>();
        texturePosition = properties.Api.BlockTextureAtlas.GetPosition("WGMT:textures/rope/rope");

        // Assign the loaded texture to the renderer
        EntityControls.TextureSource = texturePosition.TextureSubId;
        RenderRope(prog, rend, animator); //render tool
        prog?.Stop();
    }
    private void RenderRope(IStandardShaderProgram prog, EntityShapeRenderer rend, ClientAnimator animator)
    {
        var toolTextureId = _api.EntityTextureAtlas; //set the texture

        
        //this is just retrieving the transform info for rendering
        Mat4f.Copy(_modelMat, rend.ModelMat);

        Mat4f.Translate(_modelMat, _modelMat, _modelMat., toolTransform.Origin.Y, toolTransform.Origin.Z);
        Mat4f.Scale(_modelMat, _modelMat, toolTransform.ScaleXYZ.X, toolTransform.ScaleXYZ.Y, toolTransform.ScaleXYZ.Z);
        Mat4f.Translate(
            _modelMat, _modelMat, (float)ap.PosX / 16f + toolTransform.Translation.X + offsetX,
            (float)ap.PosY / 16f + toolTransform.Translation.Y, (float)ap.PosZ / 16f + toolTransform.Translation.Z);
        Mat4f.RotateX(_modelMat, _modelMat, (float)(ap.RotationX + toolTransform.Rotation.X) * GameMath.DEG2RAD);
        Mat4f.RotateY(_modelMat, _modelMat, (float)(ap.RotationY + toolTransform.Rotation.Y) * GameMath.DEG2RAD);
        Mat4f.RotateZ(_modelMat, _modelMat, (float)(ap.RotationZ + toolTransform.Rotation.Z) * GameMath.DEG2RAD);
        Mat4f.Translate(_modelMat, _modelMat, -toolTransform.Origin.X, -toolTransform.Origin.Y, -toolTransform.Origin.Z);

        var currentShader = _rpi.CurrentActiveShader;
        //setting up shaders
        if (prog != null)
        {
            prog.UniformMatrix("modelMatrix", _modelMat);
            prog.UniformMatrix("viewMatrix", _rpi.CameraMatrixOriginf);
            prog.DontWarpVertices = 0;
            prog.RgbaTint = ColorUtil.WhiteArgbVec;
            _api.Render.RenderMultiTextureMesh(_playerTools[slotId], "tex");
        }
        else if (currentShader != null)
        {
            Mat4f.Mul(_modelMat, _rpi.CurrentShadowProjectionMatrix, _modelMat);
            currentShader.UniformMatrix("mvpMatrix", _modelMat);
            currentShader.Uniform("origin", rend.OriginPos);
            currentShader.BindTexture2D("tex2d", toolTextureId, 0);

            _api.Render.RenderMultiTextureMesh(_playerTools[slotId], "tex2d");

        }
    }
    //to prevent memory leaks
}