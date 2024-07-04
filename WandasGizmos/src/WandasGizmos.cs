using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace WandasGizmos
{
    public class WandasGizmos : ModSystem
    {

        ICoreAPI _api;
        MeshRef ropeMesh;
        float[] modelMat = Mat4f.Create();

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            _api = api;

            api.RegisterEntity("EntityHook", typeof(EntityHook));
            //api.RegisterEntity("EntityRope", typeof(EntityRope));
            api.RegisterItemClass("ItemGrapplingHook", typeof(ItemGrapplingHook));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Event.RegisterRenderer(EntityHook, EnumRenderStage.Opaque);
            api.Event.RegisterRenderer(this, EnumRenderStage.ShadowFar);
            api.Event.RegisterRenderer(this, EnumRenderStage.ShadowNear);
            api.Event.KeyDown += (keyEvent) =>
            {
                if (keyEvent.KeyCode == (int)GlKeys.LShift)
                {
                    if (api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Id == 3336)
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("pull", true);
                    }
                }
                else if (keyEvent.KeyCode == (int)GlKeys.Tab)
                {
                    if (api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Id == 3336)
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("push", true);
                    }
                }
            };
            api.Event.KeyUp += (keyEvent) =>
            {                       
                if (keyEvent.KeyCode == (int)GlKeys.LShift)
                {
                    if (api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Id == 3336)
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("pull", false);
                    }
                }
                else if (keyEvent.KeyCode == (int)GlKeys.Tab)
                {
                    if (api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Id == 3336)
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("push", false);
                    }
                }
            };
            base.StartClientSide(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

        }
    }
}


