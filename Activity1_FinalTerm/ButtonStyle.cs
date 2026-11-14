using System.Drawing;
using System.Windows.Forms;

namespace Activity1_FinalTerm
{
    internal static class ButtonStyle
    {
        private static readonly Color ButtonColor = Color.FromArgb(40, 161, 116);
        private static readonly Color ButtonHoverColor = Color.FromArgb(49, 177, 130);

        internal static void Apply(Button button)
        {
            button.BackColor = ButtonColor;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(31, 130, 94);
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 8.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button.ForeColor = Color.White;
            button.Height = 30;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(0);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.UseVisualStyleBackColor = false;
        }
    }
}
