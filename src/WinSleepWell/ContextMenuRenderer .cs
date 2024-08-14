
namespace WinSleepWell
{
    public class ContextMenuRenderer : ToolStripProfessionalRenderer
    {
        private bool _isDarkMode;

        public ContextMenuRenderer(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (_isDarkMode)
            {
                Color backColor = e.Item.Selected ? Color.FromArgb(65, 65, 65) : Color.FromArgb(43, 43, 43);
                e.Graphics.FillRectangle(new SolidBrush(backColor), e.Item.ContentRectangle);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (_isDarkMode)
            {
                e.TextColor = Color.White;
            }
            e.TextRectangle = new Rectangle(e.TextRectangle.X, e.TextRectangle.Y + 10, e.TextRectangle.Width, e.TextRectangle.Height);
            base.OnRenderItemText(e);
        }
    }

}
