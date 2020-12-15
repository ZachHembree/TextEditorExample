using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using RichHudFramework.UI;
using RichHudFramework;
using EventHandler = RichHudFramework.EventHandler;

namespace TextEditorExample
{
    /// <summary>
    /// Example Text Editor window
    /// </summary>
    internal class TextEditor : WindowBase
    {
        private readonly ToolBar toolBar;
        private readonly ScrollableTextBox textBox;

        /// <summary>
        /// Initializes a new Text Editor window and registers it to the specified parent element.
        /// You can leave the parent null and use the parent element's register method if you prefer.
        /// </summary>
        public TextEditor(HudParentBase parent = null) : base(parent)
        {
            textBox = new ScrollableTextBox(body) 
            { 
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
            };

            toolBar = new ToolBar(header) 
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
                Format = GlyphFormat.White,
                BulderMode = textBox.text.BuilderMode,
            };

            toolBar.OnFormatChanged += FormatChanged;
            toolBar.OnBuildModeChanged += BuilderModeChanged;

            // Window styling:
            BodyColor = new Color(41, 54, 62, 150);
            BorderColor = new Color(58, 68, 77);

            Header.Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1.08f);
            header.Height = 30f;

            HeaderText = "Example Text Editor";
            Size = new Vector2(500f, 300f);
        }

        private void FormatChanged()
        {
            textBox.text.Format = toolBar.Format;

            // Apply new formatting to selected text range
            if (!textBox.text.SelectionEmpty)
            {
                ITextBoard textBoard = textBox.text.TextBoard;
                textBoard.SetFormatting(textBox.text.SelectionStart, textBox.text.SelectionEnd, textBoard.Format);
            }
        }

        private void BuilderModeChanged(object sender, EventArgs args)
        {
            textBox.text.BuilderMode = toolBar.BulderMode;
        }

        protected override void Draw(object matrix)
        {
            MinimumSize = new Vector2(Math.Max(toolBar.MinimumWidth, MinimumSize.X), MinimumSize.Y);

            textBox.Width = body.Width;
            textBox.Height = body.Height - toolBar.Height;
        }

        private class ScrollableTextBox : HudElementBase
        {
            public readonly TextBox text;
            private readonly ScrollBar verticalScroll, horizontalScroll;

            public ScrollableTextBox(HudParentBase parent = null) : base(parent)
            {             
                text = new TextBox(this)
                {
                    ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner,
                    Padding = new Vector2(8f, 8f),

                    Format = GlyphFormat.White,
                    VertCenterText = false, // This is a text editor, I don't want the text centered.
                    AutoResize = false, // Allows the text box size to be set manually (or via DimAlignment)
                };

                verticalScroll = new ScrollBar(text)
                {
                    Width = 26f,
                    Padding = new Vector2(4f),
                    ParentAlignment = ParentAlignments.Right,
                    Vertical = true,
                };

                horizontalScroll = new ScrollBar(text)
                {
                    Height = 26f,
                    Offset = new Vector2(8f, 0f),
                    Padding = new Vector2(4f),
                    ParentAlignment = ParentAlignments.Bottom,
                    Vertical = false,
                };
            }

            protected override void Layout()
            {
                ITextBoard textBoard = text.TextBoard;

                verticalScroll.Height = Height - horizontalScroll.Height - Padding.Y;
                horizontalScroll.Width = Width - Padding.X;

                horizontalScroll.slide.SliderWidth = (textBoard.Size.X / textBoard.TextSize.X) * horizontalScroll.Width;
                verticalScroll.slide.SliderHeight = (textBoard.Size.Y / textBoard.TextSize.Y) * verticalScroll.Height;

                text.Width = Width - verticalScroll.Width - Padding.X;
                text.Height = Height - horizontalScroll.Height - Padding.Y;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                /* TextBoard Offsets 101:
                
                The TextBoard allows you to set an offset for the text being rendered starting from the
                center of the element. Text outside the bounds of the element will not be drawn.
                Offset is measured in pixels and updates with changes to scale.
                 
                An offset in the negative direction on the X-axis will offset the text to the left; a positive
                offset will move the text to the right.
                
                On the Y-axis, a negative offset will move the text down and a positive offset will move it in
                the opposite direction.
                
                By default, the visible range of text will start at the first line on the first character.
                It starts in the upper left hand corner.
                
                If you wanted to move to the last line to the top of the element, you'd need to set a Y-offset
                of TextSize.Y - lineSize.Y. If you wanted to move the last character in a line to the right side
                of the element, you'd set an X-offset of -charOffset.X + charSize.X / 2f.
                */

                ITextBoard textBoard = text.TextBoard;
                IMouseInput horzControl = horizontalScroll.slide.MouseInput,
                    vertControl = verticalScroll.slide.MouseInput;

                // If the total width of the text is greater than the size of the element, then I can scroll
                // horiztonally. This value is negative because the text is starts at the right hand side
                // and I need to move it left.
                horizontalScroll.Max = Math.Max(0f, textBoard.TextSize.X - textBoard.Size.X);

                // Same principle, but vertical and moving up. TextBoards start at the first line which means
                // every line that follows lower than the last, so I need to move up.
                verticalScroll.Max = Math.Max(0f, textBoard.TextSize.Y - textBoard.Size.Y);

                // Update the ScrollBar positions to represent the current offset unless they're being clicked.
                if (!horzControl.IsLeftClicked)
                    horizontalScroll.Current = -textBoard.TextOffset.X;

                if (!vertControl.IsLeftClicked)
                    verticalScroll.Current = textBoard.TextOffset.Y;

                textBoard.TextOffset = new Vector2(-horizontalScroll.Current, verticalScroll.Current);
            }
        }

        /// <summary>
        /// Text editor toolbar
        /// </summary>
        private class ToolBar : HudElementBase
        {
            /// <summary>
            /// Invoked when a change is made to the text format
            /// </summary>
            public event Action OnFormatChanged;

            /// <summary>
            /// Invoked when the set text builder mode is changed
            /// </summary>
            public event EventHandler OnBuildModeChanged
            {
                add { textBuilderModes.OnSelectionChanged += value; }
                remove { textBuilderModes.OnSelectionChanged -= value; }
            }

            /// <summary>
            /// Current glyph format set by the toolbar
            /// </summary>
            public GlyphFormat Format { get { return _format; } set { SetFormat(value); } }

            /// <summary>
            /// Current toolbar text builder mode.
            /// </summary>
            public TextBuilderModes BulderMode { get { return textBuilderModes.Selection.AssocMember; } set { textBuilderModes.SetSelection(value); } }

            // The width of the HudChain containing the controls is determined by the total width
            // of every element in the chain
            public float MinimumWidth => layout.Width;

            private readonly HudChain layout;
            private readonly EditorDropdown<TextBuilderModes> textBuilderModes;
            private readonly EditorDropdown<float> sizeList;
            private readonly EditorToggleButton boldToggle, italicToggle;
            private readonly EditorDropdown<IFontMin> fontList;
            private readonly TexturedBox background;

            private static readonly float[] textSizes = new float[] { .75f, .875f, 1f, 1.125f, 1.25f, 1.375f, 1.5f };
            private GlyphFormat _format;

            public ToolBar(HudParentBase parent = null) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = new Color(41, 54, 62),
                };

                // Font selection
                fontList = new EditorDropdown<IFontMin>()
                {
                    Height = 24f,
                    Width = 140f,
                    Format = GlyphFormat.White,
                };
                
                foreach (IFontMin font in FontManager.Fonts)
                    fontList.Add(new RichText(font.Name, GlyphFormat.White.WithFont(font.Regular)), font);

                // Text size
                sizeList = new EditorDropdown<float>()
                {
                    Height = 24f,
                    Width = 60f,
                    Format = GlyphFormat.White,
                };

                for (int n = 0; n < textSizes.Length; n++)
                    sizeList.Add(textSizes[n].ToString(), textSizes[n]);

                // Builder mode
                textBuilderModes = new EditorDropdown<TextBuilderModes>()
                {
                    Height = 24f,
                    Width = 140f,
                    Format = GlyphFormat.White,
                };

                textBuilderModes.Add("Unlined", TextBuilderModes.Unlined);
                textBuilderModes.Add("Lined", TextBuilderModes.Lined);
                textBuilderModes.Add("Wrapped", TextBuilderModes.Wrapped);

                // Font style toggle
                IFontMin abhaya = FontManager.GetFont("AbhayaLibreMedium");
                GlyphFormat buttonFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1.1625f, abhaya.Regular);

                boldToggle = new EditorToggleButton()
                {
                    Format = buttonFormat,
                    Text = "B",
                };

                italicToggle = new EditorToggleButton()
                {
                    Format = buttonFormat,
                    Text = "I",
                };

                layout = new HudChain(false, this) // Set to alignVertical false to align the elements horizontally
                {
                    // Automatically resize the height of the elements to match that of the chain and allow the chain to be
                    // wider than the total size of the members
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampChainAlignAxis,
                    // Match the height of the chain and its children to the toolbar
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    // The width of the parent could very well be greater than the width of the controls.
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding,
                    // The order the elements will appear on the toolbar from left to right.
                    ChainContainer = { fontList, sizeList, boldToggle, italicToggle, textBuilderModes }
                };

                fontList.OnSelectionChanged += UpdateFormat;
                sizeList.OnSelectionChanged += UpdateFormat;
                boldToggle.MouseInput.OnLeftClick += UpdateFormat;
                italicToggle.MouseInput.OnLeftClick += UpdateFormat;

                Height = 30f;
                _format = GlyphFormat.White;
            }

            protected override void Draw(object matrix)
            {
                // The width of the toolbar should not be less than the total width of the controls
                // it contains.
                Width = Math.Max(Width, layout.Width);
            }

            private void SetFormat(GlyphFormat newFormat)
            {
                FontStyles style = newFormat.FontStyle;
                boldToggle.Selected = style.HasFlag(FontStyles.Bold);
                italicToggle.Selected = style.HasFlag(FontStyles.Italic);

                fontList.SetSelection(newFormat.Font);
                sizeList.SetSelection(newFormat.TextSize);

                _format = newFormat;
                OnFormatChanged?.Invoke();
            }

            private void UpdateFormat(object sender, EventArgs args)
            {
                if (sizeList.Selection != null && fontList.Selection != null)
                {
                    float textSize = sizeList.Selection.AssocMember;
                    IFontMin font = fontList.Selection.AssocMember;
                    FontStyles style = FontStyles.Regular;
                    
                    if (boldToggle.Selected)
                    {
                        if (font.IsStyleDefined(FontStyles.Bold))
                            style |= FontStyles.Bold;
                        else
                            boldToggle.Selected = false;
                    }

                    if (italicToggle.Selected)
                        style |= FontStyles.Underline;

                    _format = new GlyphFormat(_format.Color, _format.Alignment, textSize, font.GetStyleIndex(style));
                    OnFormatChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Customized Dropdown whose proportions have been altered to fit in the toolbar.
        /// </summary>
        private class EditorDropdown<T> : Dropdown<T>
        {
            public EditorDropdown(HudParentBase parent = null) : base(parent)
            {
                ScrollBar scrollBar = listBox.scrollBox.scrollBar;

                scrollBar.Padding = new Vector2(12f, 8f);
                scrollBar.Width = 20f;

                //display.divider.Padding = new Vector2(4f, 8f);
                //display.arrow.Width = 22f;

                listBox.Height = 0f;
                listBox.MinVisibleCount = 4;
            }
        }

        /// <summary>
        /// A TextBoxButton modified to serve as a toggle button for the editor.
        /// </summary>
        private class EditorToggleButton : LabelBoxButton
        {
            public bool Selected
            {
                get { return _selected; }
                set
                {
                    if (value)
                        Color = SelectColor;
                    else
                        Color = NormalColor;

                    _selected = value;
                }
            }

            public Color NormalColor { get; set; }
            public Color SelectColor { get; set; }

            private bool _selected;

            public EditorToggleButton(HudElementBase parent = null) : base(parent)
            {
                HighlightEnabled = true;
                AutoResize = false;
                VertCenterText = true;

                NormalColor = new Color(41, 54, 62);
                SelectColor = new Color(58, 68, 77);
                HighlightColor = new Color(68, 78, 87);

                Size = new Vector2(32f, 30f);
                Color = NormalColor;

                MouseInput.OnLeftClick += ToggleEnabled;
            }

            private void ToggleEnabled(object sender, EventArgs args)
            {
                Selected = !Selected;
            }
        }
    }
}
