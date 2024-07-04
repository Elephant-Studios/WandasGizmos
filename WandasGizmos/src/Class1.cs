using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

public class EntityRope : Entity
{
    private MeshRef modelMesh;
    private TextureAtlasPosition texturePosition;

    public string EntityClassName => "rope";

    // Constructor
    public EntityRope(EntityProperties properties) : base(properties)
    {
        // Load your model and texture here
        modelMesh = properties.Api.Assets.TryGet("yourmodid:models/yourmodel.obj")?.ToObject<MeshRef>();
        texturePosition = properties.Api.BlockTextureAtlas.GetPosition("yourmodid:yourtexture");

        // Assign the loaded texture to the renderer
        EntityControls.TextureSource = texturePosition.TextureSubId;
    }

    public override void OnGameTick(float deltaTime)
    {
        base.OnGameTick(deltaTime);
        // Update logic for your entity each tick
    }

    public override void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        base.OnRenderFrame(deltaTime, stage);

        if (stage == EnumRenderStage.Opaque)
        {
            ICoreClientAPI capi = Api as ICoreClientAPI;
            capi.Render.RenderMesh(modelMesh, new Vec3f((float)Pos.X, (float)Pos.Y, (float)Pos.Z), capi.World.Player.CameraPos);
        }
    }
}