using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace GrappleParkour
{
    public class GrappleParkour : ModSystem
    {
        public static ICoreAPI _api;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            _api = api;

            api.RegisterItemClass("ItemGrapplingHook", typeof(ItemGrapplingHook));
            api.RegisterEntity("GrappleParkour:EntityHook", typeof(EntityHook));
        }
    }
}
