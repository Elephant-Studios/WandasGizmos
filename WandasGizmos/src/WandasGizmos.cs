using System.Drawing;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using WandasGizmos.src;

namespace WandasGizmos
{
    public class WandasGizmos : ModSystem
    {

        ICoreAPI _api;
        MeshRef ropeMesh;
        public ICoreClientAPI capi;
        //float[] modelMat = Mat4f.Create();

        public static IShaderProgram RopeLine { get; set; }
        public static IShaderProgram RopeLineShadow { get; set; }

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
            api.RegisterEntityRendererClass("RopeRenderer", typeof(RopeRenderer));
            base.StartClientSide(api);
            capi = api;
            api.Event.KeyDown += (keyEvent) =>
            {
                if (keyEvent.KeyCode == (int)GlKeys.LShift)
                {
                    if (WildcardUtil.Match("*grapplinghook-*", api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Item?.Code.ToString() ?? ""))
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("pull", true);
                    }
                }
                else if (keyEvent.KeyCode == (int)GlKeys.Tab)
                {
                    if (WildcardUtil.Match("*grapplinghook-*", api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Item?.Code.ToString() ?? ""))
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("push", true);
                    }
                }
            };
            api.Event.KeyUp += (keyEvent) =>
            {                       
                if (keyEvent.KeyCode == (int)GlKeys.LShift)
                {
                    if (WildcardUtil.Match("*grapplinghook-*", api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Item?.Code.ToString() ?? ""))
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("pull", false);
                    }
                }
                else if (keyEvent.KeyCode == (int)GlKeys.Tab)
                {
                    if (WildcardUtil.Match("*grapplinghook-*", api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack?.Item?.Code.ToString() ?? ""))
                    {
                        api.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.SetBool("push", false);
                    }
                }
            };
            capi.Event.ReloadShader += () =>
            {
                //Squiggly = RegisterShader("squiggly", "squiggly");
                RopeLineShadow = RegisterShader("ropelineshadow", "ropelineshadow");
                RopeLine = RegisterShader("ropeline", "ropeline");
                //Color = RegisterShader("color", "color");
                return true;
            };
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

        }


        public IShaderProgram RegisterShader(string shaderPath, string shaderName)
        {
            IShaderProgram shader = capi.Shader.NewShaderProgram();

            MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
            object[] vertParams = new object[] { shader, capi.Assets.Get($"wgmt:shaders/{shaderPath}.vert").ToText(), null! };
            object[] fragParams = new object[] { shader, capi.Assets.Get($"wgmt:shaders/{shaderPath}.frag").ToText(), null! };

            shader.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
            shader.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

            shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
            shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;

            capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

            shader.Compile(); // Returns bool.

            return shader;
        }
    }
}


