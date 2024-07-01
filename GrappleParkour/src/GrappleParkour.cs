using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace GrappleParkour
{
    public class WandasGizmos : ModSystem
    {
        public static ICoreAPI _api;
        public static EntityHook;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            _api = api;

            api.RegisterEntity("EntityHook", typeof(EntityHook));
            api.RegisterItemClass("ItemGrapplingHook", typeof(ItemGrapplingHook));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Event.KeyDown += (keyEvent) =>
            {
                if (keyEvent.KeyCode == (int)GlKeys.J)
                {
                    EntityHook.KillHook(api.World.Player.Entity.PlayerUID);
                }
            };
            base.StartClientSide(api);

        }
    }
}
