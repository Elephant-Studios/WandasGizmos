using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;

namespace Elephant.WandasGizmos
{
    public class WandasGizmos : ModSystem
    {

        ICoreAPI _api;
        public ICoreServerAPI _sapi;
        public ICoreClientAPI capi;
        private readonly Dictionary<string, bool> OnlinePlayers = new();

        public static IShaderProgram RopeLine { get; set; }
        public static IShaderProgram RopeLineShadow { get; set; }
        
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            _api = api;

            api.RegisterEntity("EntityHook", typeof(EntityHook));
            api.RegisterItemClass("ItemGrapplingHook", typeof(ItemGrapplingHook));
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            api.RegisterEntityRendererClass("RopeRenderer", typeof(RopeRenderer));
            base.StartClientSide(api);
            capi = api;
            capi.Input.RegisterHotKey("hoist", "Hoist", GlKeys.CapsLock, HotkeyType.MovementControls);
            capi.Input.SetHotKeyHandler("hoist", combo => false);

            capi.Input.RegisterHotKey("rappell", "Rappell", GlKeys.LShift, HotkeyType.MovementControls);
            capi.Input.SetHotKeyHandler("rappell", combo => false);
            capi.Event.ReloadShader += () =>
            {
                RopeLineShadow = RegisterShader("ropelineshadow", "ropelineshadow");
                RopeLine = RegisterShader("ropeline", "ropeline");
                return true;
            };
        }

        public static bool IsKeyComboActive(ICoreClientAPI api, string key)
        {
            KeyCombination combo = api.Input.GetHotKeyByCode(key).CurrentMapping;
            return api.Input.KeyboardKeyState[combo.KeyCode];
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

            shader.Compile();

            return shader;
        }
    }
}


