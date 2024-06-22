using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace GrappleParkour
{
    public class GrappleParkour : ModSystem
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
            public override void Start(ICoreAPI api)
            {
                base.Start(api);
                api.RegisterItemClass("ItemGrapplingHook", typeof(ItemGrapplingHook));
            }
    }
}
