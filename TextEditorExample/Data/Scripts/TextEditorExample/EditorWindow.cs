﻿using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI;
using RichHudFramework;

namespace TextEditorExample
{
    /// <summary>
    /// Example Text Editor window
    /// </summary>
    public partial class TextEditor : WindowBase
    {
        private readonly EditorToolBar toolBar;
        private readonly EditorTextBox textBox;

        /// <summary>
        /// Initializes a new Text Editor window and registers it to the specified parent element.
        /// You can leave the parent null and use the parent element's register method if you prefer.
        /// </summary>
        public TextEditor(HudParentBase parent = null) : base(parent)
        {
            textBox = new EditorTextBox(body)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                DimAlignment = DimAlignments.Width
            };

            toolBar = new EditorToolBar(header)
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
                Format = textBox.text.Format,
                BulderMode = textBox.text.BuilderMode,
            };

            toolBar.FormatChanged += ChangeFormat;
            toolBar.BuildModeChanged += ChangeBuilderMode;

            // Window styling:
            BodyColor = new Color(41, 54, 62, 150);
            BorderColor = new Color(58, 68, 77);

            header.Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1.08f);
            header.Height = 30f;

            HeaderText = "Example Text Editor";
            Size = new Vector2(500f, 300f);
        }

        /// <summary>
        /// Updates text box formatting in response to input from the toolbar
        /// </summary>
        private void ChangeFormat()
        {
            textBox.text.Format = toolBar.Format;

            // Apply new formatting to selected text range
            if (!textBox.text.SelectionEmpty)
            {
                ITextBoard textBoard = textBox.text.TextBoard;
                textBoard.SetFormatting(textBox.text.SelectionStart, textBox.text.SelectionEnd, textBoard.Format);
            }
        }

        /// <summary>
        /// Changes text box builder mode based on toolbar input
        /// </summary>
        private void ChangeBuilderMode(object sender, EventArgs args)
        {
            textBox.text.BuilderMode = toolBar.BulderMode;
        }

        protected override void Draw()
        {
            // Set window minimum width to prevent it from becoming narrower than the toolbar's minimum width
            MinimumSize = new Vector2(Math.Max(toolBar.MinimumWidth, MinimumSize.X), MinimumSize.Y);

            // Match text box height to body height less toolbar height
            textBox.Height = body.Height - toolBar.Height;
        }
    }
}
