using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace TextEditorExample
{
    public partial class TextEditor
    {
        /// <summary>
        /// Scrollable text box component that forms the core editing area of the text editor.
        /// Contains the editable text field, horizontal and vertical scrollbars, and a status readout
        /// showing caret position and total line count.
        /// </summary>
        private class EditorTextBox : HudElementBase
        {
            /// <summary>
            /// Primary editable text field where the document content is displayed and modified.
            /// </summary>
            public readonly TextBox content;

            /// <summary>
            /// Vertical scrollbar controlling the Y-offset of the text content.
            /// </summary>
            private readonly ScrollBar vertScroll;

            /// <summary>
            /// Horizontal scrollbar controlling the X-offset of the text content.
            /// </summary>
            private readonly ScrollBar horzScroll;

            /// <summary>
            /// Small status label displaying current caret line/column and total line count.
            /// </summary>
            private readonly LabelBox posBox;

            public EditorTextBox(HudParentBase parent = null) : base(parent)
            {
                // Main text editing area
                content = new TextBox
                {
                    Padding = new Vector2(8f),
                    // Prevent automatic selection clearing when the textbox loses focus (required for formatting operations)
                    ClearSelectionOnLoseFocus = false,
                    // Use '|' as newline character because Enter is reserved for in-game chat
                    NewLineChar = '|',
                    Format = GlyphFormat.White,
                    // Align text to the top rather than vertically centering it
                    VertCenterText = false,
                    // Disable automatic resizing so the size can be controlled explicitly by layout chains
                    AutoResize = false
                };

                // Vertical scrollbar (controls vertical text offset)
                vertScroll = new ScrollBar
                {
                    Padding = new Vector2(8f),
                    Width = 18f,
                    Vertical = true,
                    UpdateValueCallback = UpdateVerticalScroll
                };

                // Horizontal chain: places the text box and vertical scrollbar side-by-side
                var horzChain = new HudChain
                {
                    // Chain height is determined by its tallest member; members stretch horizontally to fill chain width
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    CollectionContainer = { { content, 1f }, vertScroll } // content takes remaining space, vertScroll uses fixed width
                };

                // Horizontal scrollbar (controls horizontal text offset)
                horzScroll = new ScrollBar
                {
                    Padding = new Vector2(8f),
                    Height = 18f,
                    Vertical = false,
                    UpdateValueCallback = UpdateHorizontalScroll
                };

                // Status readout showing caret position (line, column) and total lines
                posBox = new LabelBox
                {
                    AutoResize = false,           // Width will be dictated by the parent chain
                    Height = 18f,                 // Fixed height matching the horizontal scrollbar
                    TextPadding = new Vector2(8f, 0f),
                    Padding = new Vector2(8f),
                    Format = GlyphFormat.Blueish.WithSize(0.8f),
                    Color = TerminalFormatting.DarkSlateGrey
                };

                // Vertical chain: arranges main content area, horizontal scrollbar, and status line from top to bottom
                var vertChain = new HudChain(this)
                {
                    // Element fills the entire EditorTextBox area minus its own padding
                    DimAlignment = DimAlignments.UnpaddedSize,
                    // Members stretch horizontally to fill the chain width
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    CollectionContainer = { { horzChain, 1f }, horzScroll, posBox }
                    // horzChain expands vertically to fill available space; scrollbars and status have fixed height
                };

                // Mouse-wheel scrolling support when the cursor is over the text content
                var scrollBinds = new BindInputElement(this)
                {
                    // Only process scroll input while the text box itself is under the mouse
                    InputPredicate = () => content.IsMousedOver,
                    CollectionInitializer =
                    {
                        { SharedBinds.MousewheelUp,   ScrollUp   },
                        { SharedBinds.MousewheelDown, ScrollDown }
                    }
                };
            }

            /// <summary>
            /// Handles mouse-wheel up events; scrolls the visible text upward by one line.
            /// </summary>
            private void ScrollUp(object sender, EventArgs args)
            {
                ITextBoard board = content.TextBoard;
                Vector2I range = board.VisibleLineRange;
                board.MoveToChar(new Vector2I(range.X - 1, 0));
            }

            /// <summary>
            /// Handles mouse-wheel down events; scrolls the visible text downward by one line.
            /// </summary>
            private void ScrollDown(object sender, EventArgs args)
            {
                ITextBoard board = content.TextBoard;
                Vector2I range = board.VisibleLineRange;
                board.MoveToChar(new Vector2I(range.Y + 1, 0));
            }

            /// <summary>
            /// Callback invoked when the vertical scrollbar value changes.
            /// Updates the text board's Y offset accordingly.
            /// </summary>
            private void UpdateVerticalScroll(object sender, EventArgs args)
            {
                Vector2 textOffset = content.TextBoard.TextOffset;
                textOffset.Y = ((ScrollBar)sender).Value;
                content.TextBoard.TextOffset = textOffset;
            }

            /// <summary>
            /// Callback invoked when the horizontal scrollbar value changes.
            /// Updates the text board's X offset (negative direction because scrollbar moves opposite to content).
            /// </summary>
            private void UpdateHorizontalScroll(object sender, EventArgs args)
            {
                Vector2 textOffset = content.TextBoard.TextOffset;
                textOffset.X = -((ScrollBar)sender).Value;
                content.TextBoard.TextOffset = textOffset;
            }

            /// <summary>
            /// Called during layout pass. Updates the status display and synchronizes scrollbar ranges
            /// and visible percentages with the current text dimensions.
            /// </summary>
            protected override void Layout()
            {
                // Update caret position and line count display
                Vector2 caretPos = content.CaretPosition;
                posBox.TextBoard.SetText($"Ln: {caretPos.X}, Col: {caretPos.Y} Lines:{content.TextBoard.Count}");

                ITextBoard textBoard = content.TextBoard;

                // Horizontal scrollbar configuration
                horzScroll.Max = Math.Max(0f, textBoard.TextSize.X - textBoard.Size.X);
                horzScroll.Value = -textBoard.TextOffset.X; // Negative because offset is opposite to scroll direction
                horzScroll.VisiblePercent = textBoard.Size.X / Math.Max(textBoard.TextSize.X, 1f);

                // Vertical scrollbar configuration
                vertScroll.Max = Math.Max(0f, textBoard.TextSize.Y - textBoard.Size.Y);
                vertScroll.Value = textBoard.TextOffset.Y;
                vertScroll.VisiblePercent = textBoard.Size.Y / Math.Max(textBoard.TextSize.Y, 1f);
            }
        }
    }
}