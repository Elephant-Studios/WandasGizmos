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
using Vintagestory.Common;
using Vintagestory.GameContent;
using WandasGizmos.src;

namespace WandasGizmos
{
    public class WandasGizmos : ModSystem
    {

        ICoreAPI _api;
        public ICoreServerAPI _sapi;
        public ICoreClientAPI _capi;
        private readonly Dictionary<string, bool> OnlinePlayers = new();
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
            _capi = api;
            _capi.Input.RegisterHotKey("hoist", "Hoist", GlKeys.CapsLock, HotkeyType.MovementControls);
            _capi.Input.SetHotKeyHandler("hoist", combo => false);

            _capi.Input.RegisterHotKey("rappell", "Rappell", GlKeys.LShift, HotkeyType.MovementControls);
            _capi.Input.SetHotKeyHandler("rappell", combo => false);
            _capi.Event.ReloadShader += () =>
            {
                //Squiggly = RegisterShader("squiggly", "squiggly");
                RopeLineShadow = RegisterShader("ropelineshadow", "ropelineshadow");
                RopeLine = RegisterShader("ropeline", "ropeline");
                //Color = RegisterShader("color", "color");
                return true;
            };
            this._capi = api;
            api.Network.RegisterChannel("AnimationSync").RegisterMessageType<AnimationStartMessage>().RegisterMessageType<AnimationStopMessage>();
            DataFields.setCoreAPI(this._api);
            DataFields.setICoreClientAPI(this._capi);
            DataFields.setIClientPlayerWorld(this._capi.World);
            //api.Input.RegisterHotKey("toggleClimbing", "Toggle wall-climbing", (GlKeys)88, (HotkeyType)4, false, false, true);
            //api.Input.SetHotKeyHandler("toggleClimbing", Wandas.\u003C\u003EO.\u003C0\u003E__ToggleClimbMode ?? (MoveLikeKajiModSystem.\u003C\u003EO.\u003C0\u003E__ToggleClimbMode = new ActionConsumable<KeyCombination>((object)null, __methodptr(ToggleClimbMode))));
            //api.Input.RegisterHotKey("toggleCrawling", "Crawl on the floor", (GlKeys)85, (HotkeyType)4, false, false, true);
            //api.Input.SetHotKeyHandler("toggleCrawling", MoveLikeKajiModSystem.\u003C\u003EO.\u003C1\u003E__ToggleCrawling ?? (MoveLikeKajiModSystem.\u003C\u003EO.\u003C1\u003E__ToggleCrawling = new ActionConsumable<KeyCombination>((object)null, __methodptr(ToggleCrawling))));
            //api.Input.RegisterHotKey("toggleSettingsGUI", "Mod-Settings for 'Move like Kaji'", (GlKeys)94, (HotkeyType)0, false, false, false);
            //api.Input.AddHotkeyListener(MoveLikeKajiModSystem.\u003C\u003EO.\u003C3\u003E__OnSpecificHotkeyPressed ?? (MoveLikeKajiModSystem.\u003C\u003EO.\u003C3\u003E__OnSpecificHotkeyPressed = new OnHotKeyDelegate((object)null, __methodptr(OnSpecificHotkeyPressed))));
            GameTickListeners gameTickListerners = new GameTickListeners();
            //((IEventAPI)api.Event).RegisterGameTickListener(new Action<float>(gameTickListerners.OnFixedTick), 50, 0);
            //api.Event.PlayerDeath += new PlayerEventDelegate((object)this, __methodptr(Event_PlayerDeath));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

        }
        /*private void Event_PlayerDeath(IClientPlayer byPlayer)
        {
            BehaviorClimbing.ToggleClimbMode(this.capi.World);
        }*/

        public static bool IsKeyComboActive(ICoreClientAPI api, string key)
        {
            KeyCombination combo = api.Input.GetHotKeyByCode(key).CurrentMapping;
            return api.Input.KeyboardKeyState[combo.KeyCode];
        }
        public IShaderProgram RegisterShader(string shaderPath, string shaderName)
        {
            IShaderProgram shader = _capi.Shader.NewShaderProgram();

            MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
            object[] vertParams = new object[] { shader, _capi.Assets.Get($"wgmt:shaders/{shaderPath}.vert").ToText(), null! };
            object[] fragParams = new object[] { shader, _capi.Assets.Get($"wgmt:shaders/{shaderPath}.frag").ToText(), null! };

            shader.VertexShader = _capi.Shader.NewShader(EnumShaderType.VertexShader);
            shader.FragmentShader = _capi.Shader.NewShader(EnumShaderType.FragmentShader);

            shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
            shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;

            _capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

            shader.Compile(); // Returns bool.

            return shader;
        }
    }
}


