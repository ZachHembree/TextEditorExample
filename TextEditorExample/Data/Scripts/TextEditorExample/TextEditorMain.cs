﻿using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.Input;
using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.UI.Client;
using RichHudFramework.UI;
using RichHudFramework.IO;

namespace TextEditorExample
{
    /// <summary>
    /// Example Text Editor Mod used to demonstrate the usage of the Rich HUD Framework.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class TextEditorMain : MySessionComponentBase
    {
        private TextEditor textEditor;
        private IBindGroup editorBinds;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            RichHudClient.Init("Text Editor Example", HudInit, ClientReset);
        }

        private void HudInit()
        {
            /* There are three(-ish) ways to register a HUD element to a parent element. By calling the parent's RegisterChild() method
             * and passing in the child element, by calling the child element's Register() method and passing in the parent or by
             * passing the parent into the child's constructor. */
            textEditor = new TextEditor(HudMain.Root)
            { 
                Visible = false, // I don't want this to be visible on init.
            };

            editorBinds = BindManager.GetOrCreateGroup("editorBinds");
            editorBinds.RegisterBinds(new BindGroupInitializer() 
            {
                { "editorToggle", MyKeys.Home }
            });

            editorBinds[0].NewPressed += ToggleEditor;
        }

        private void ToggleEditor()
        {
            textEditor.Visible = !textEditor.Visible;
            HudMain.EnableCursor = textEditor.Visible;
        }

        public override void Draw()
        {
            if (RichHudClient.Registered)
            {
                /* If you need to update framework members externally, then 
                you'll need to make sure you don't start updating until your
                mod client has been registered. */

                // This will scale up the window for resolutions > 1080p to compensate for
                // high dpi displays
                textEditor.LocalScale = HudMain.ResScale;
            }
        }

        private void ClientReset()
        {
            /* At this point, your client has been unregistered and all of 
            your framework members will stop working.

            This will be called in one of three cases:
            1) The game session is unloading.
            2) An unhandled exception has been thrown and caught on either the client
            or on master.
            3) RichHudClient.Reset() has been called manually.
            */
        }
    }
}
