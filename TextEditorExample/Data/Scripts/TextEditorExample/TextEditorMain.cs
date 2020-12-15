using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
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
    public class TextEditorMain : ModBase
    {
        private TextEditor textEditor;
        private IBindGroup editorBinds;

        public TextEditorMain() : base(false, true)
        {
            ExceptionHandler.ModName = "Text Editor Example"; // The name of the mod as it will appear in any chat messages, popups or error messages
            LogIO.FileName = "editorLog.txt"; // The name of the log file in local storage

            ExceptionHandler.PromptForReload = true; // I generally prefer that the user be prompted before allowing the mod to reload
            ExceptionHandler.RecoveryLimit = 4; // The number of times the mod will be allowed to reload as a result of an unhandled exception
        }

        protected override void AfterInit()
        {
            RichHudClient.Init(ExceptionHandler.ModName, HudInit, Reload);
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

            RichHudTerminal.Root.Enabled = true;
            RichHudTerminal.Root.Add(new ControlPage()
            {
                CategoryContainer =
                {
                    new ControlCategory()
                    {
                        new ControlTile()
                        {
                            new TerminalCheckbox(),
                            new TerminalButton()
                            {
                                Name = "Toggle Text Editor",
                                ControlChangedHandler = (sender, args) => ToggleEditor(),
                            },
                            new TerminalDropdown<int>()
                            {
                                Name = "Example Dropdown",
                                List = 
                                {
                                    { "Entry 1", 1 },
                                    { "Entry 2", 2 },
                                    { "Entry 3", 3 },
                                    { "Entry 4", 4 },
                                    { "Entry 5", 5 }
                                }
                            }
                        },
                        new ControlTile()
                        {
                            new TerminalColorPicker(),
                        },
                    },
                    new ControlCategory()
                    {
                        new ControlTile()
                        {
                            new TerminalList<int>()
                            {
                                Name = "Test List",
                                List =
                                {
                                    { "Entry 1", 1 },
                                    { "Entry 2", 2 },
                                    { "Entry 3", 3 },
                                    { "Entry 4", 4 },
                                    { "Entry 5", 5 }
                                }
                            },
                        },
                        new ControlTile()
                        {
                            new TerminalOnOffButton(),
                            new TerminalOnOffButton(),
                            new TerminalSlider(),
                        },
                        new ControlTile()
                        {
                            new TerminalTextField()
                        }
                    }
                },
            });

            editorBinds[0].OnNewPress += ToggleEditor;
        }

        private void ToggleEditor() =>
            textEditor.Visible = !textEditor.Visible;

        public override void BeforeClose()
        {
            if (ExceptionHandler.Reloading)
                RichHudClient.Reset(); //using reset like this could make the client reload twice
        }
    }
}
