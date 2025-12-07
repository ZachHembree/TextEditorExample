using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using RichHudFramework.UI;
using EventHandler = RichHudFramework.EventHandler;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// Text editor toolbar
        /// </summary>
        private class EditorToolBar : HudElementBase
        {
            /// <summary>
            /// Invoked when a change is made to the text format
            /// </summary>
            public event Action FormatChanged;

            /// <summary>
            /// Invoked when the set text builder mode is changed
            /// </summary>
            public event EventHandler BuildModeChanged
            {
                add { textBuilderModes.ValueChanged += value; }
                remove { textBuilderModes.ValueChanged -= value; }
            }

            /// <summary>
            /// Current glyph format set by the toolbar
            /// </summary>
            public GlyphFormat Format 
            { 
                get { return _format; } 
                set 
                {
                    FontStyles style = value.FontStyle;
                    boldToggle.Selected = style.HasFlag(FontStyles.Bold);
                    underlineToggle.Selected = style.HasFlag(FontStyles.Underline);
                    italicToggle.Selected = style.HasFlag(FontStyles.Italic);

                    fontList.SetSelection(value.Font);
                    sizeList.SetSelection(value.TextSize);

                    _format = value;
                    FormatChanged?.Invoke();
                }
            }

            /// <summary>
            /// Current toolbar text builder mode.
            /// </summary>
            public TextBuilderModes BulderMode { get { return textBuilderModes.Value.AssocMember; } set { textBuilderModes.SetSelection(value); } }

            // The width of the HudChain containing the controls is determined by the total width
            // of every element in the chain
            public float MinimumWidth => layout.Width + Padding.X;

            private readonly HudChain layout;
            private readonly EditorDropdown<TextBuilderModes> textBuilderModes;
            private readonly EditorDropdown<float> sizeList;
            private readonly EditorToggleButton boldToggle, italicToggle, underlineToggle;
            private readonly EditorDropdown<IFontMin> fontList;

            private static readonly float[] textSizes = new float[] { .75f, .875f, 1f, 1.125f, 1.25f, 1.375f, 1.5f };
            private GlyphFormat _format;

            public EditorToolBar(HudParentBase parent = null) : base(parent)
            {
                var background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = TerminalFormatting.DarkSlateGrey
                };

                // Font selection
                fontList = new EditorDropdown<IFontMin>()
                {
                    Height = 24f,
                    Width = 140f,
                };
                
                // Add Label+IFontMin pairs, with labels styled to match the corresponding font
                foreach (IFontMin font in FontManager.Fonts)
                    fontList.Add(new RichText(font.Name, GlyphFormat.White.WithFont(font.Regular)), font);

                // Text size
                sizeList = new EditorDropdown<float>()
                {
                    Height = 24f,
                    Width = 60f,
                };

                for (int n = 0; n < textSizes.Length; n++)
                    sizeList.Add(textSizes[n].ToString(), textSizes[n]);

                // Builder mode
                textBuilderModes = new EditorDropdown<TextBuilderModes>()
                {
                    Height = 24f,
                    Width = 140f,
                    ListContainer = 
                    { 
                        { "Unlined", TextBuilderModes.Unlined },
                        { "Lined", TextBuilderModes.Lined },
                        { "Wrapped", TextBuilderModes.Wrapped }
                    }
                };

                // Font style toggle
                GlyphFormat buttonFormat = GlyphFormat.White
                    .WithAlignment(TextAlignment.Center)
                    .WithFont("AbhayaLibreMedium");

                boldToggle = new EditorToggleButton()
                {
                    Format = buttonFormat,
                    Text = "B",
                };

                underlineToggle = new EditorToggleButton()
                {
                    Format = buttonFormat.WithStyle(FontStyles.Underline),
                    Text = "U",
                };

                italicToggle = new EditorToggleButton()
                {
                    Format = buttonFormat.WithStyle(FontStyles.Italic),
                    Text = "I",
                };

                /* HudChain is useful for organizing collections of elements into straight lines with regular spacing, 
                 * either vertically horizontally. In this case, I'm organizing elements horizontally from left to right
                 * in the same order indicated by the collection initializer below. 
                 * 
                 * HudChain and its related types, like ScrollBox and the SelectionBox types, are powerful tools for 
                 * organizing UI elements, especially when used in conjunction with oneanother. 
                 */
                layout = new HudChain(false, this) // Set to alignVertical false to align the elements horizontally
                {
                    // Automatically resize the height of the elements to match that of the chain and allow the chain to be
                    // wider than the total size of the members
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                    // Match the height of the chain and its children to the toolbar
                    DimAlignment = DimAlignments.UnpaddedHeight,
                    // The width of the parent could very well be greater than the width of the controls.
                    ParentAlignment = ParentAlignments.PaddedInnerLeft,
                    // The order the elements will appear on the toolbar from left to right.
                    CollectionContainer = { fontList, sizeList, boldToggle, underlineToggle, italicToggle, textBuilderModes }
                };

                fontList.ValueChanged += UpdateFormat;
                sizeList.ValueChanged += UpdateFormat;
                boldToggle.MouseInput.LeftClicked += UpdateFormat;
                underlineToggle.MouseInput.LeftClicked += UpdateFormat;
                italicToggle.MouseInput.LeftClicked += UpdateFormat;

                Height = 30f;
                Padding = new Vector2(16f, 0f);
                _format = GlyphFormat.White;
            }

            protected override void Measure()
            {
                // The width of the toolbar should not be less than the total width of the controls
                // it contains.
                Width = Math.Max(Width, layout.Width + Padding.X);
            }

            /// <summary>
            /// Updates formatting based on input from the toolbar controls
            /// </summary>
            private void UpdateFormat(object sender, EventArgs args)
            {
                if (sizeList.Value != null && fontList.Value != null)
                {
                    float textSize = sizeList.Value.AssocMember;
                    IFontMin font = fontList.Value.AssocMember;
                    FontStyles style = FontStyles.Regular;

                    // Bolding requires a separate set of texture atlases, and as such, isn't always
                    // available
                    boldToggle.Enabled = font.IsStyleDefined(FontStyles.Bold);

                    if (boldToggle.Selected)
                        style |= FontStyles.Bold;

                    // Underlining and italics are rendered as effects and are available
                    // for every font
                    if (underlineToggle.Selected)
                        style |= FontStyles.Underline;

                    if (italicToggle.Selected)
                        style |= FontStyles.Italic;

                    _format = new GlyphFormat(_format.Color, _format.Alignment, textSize, font.GetStyleIndex(style));
                    FormatChanged?.Invoke();
                }
            }
        }
    }
}
