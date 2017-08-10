using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelicTool {
    public partial class RewardForm : Form {
        public RewardForm() {
            InitializeComponent();
        }
        private static FontFamily fontFamily = new FontFamily("Trebuchet MS");
        private Font font = new Font(
           fontFamily,
           14,
           FontStyle.Bold,
           GraphicsUnit.Pixel);
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (MainBehavior.RewardScreen) {
                using (var brush = new SolidBrush(Color.FromArgb(255, 202, 202, 202))) {
                    e.Graphics.FillRectangle(brush, 0, 0, 700, 160);
                }

                var blackbrush = new SolidBrush(Color.Black);
                for(int i = 0; i < MainBehavior.rewards.Length; i++) {
                    Item item = MainBehavior.rewards[i];
                    if (item == null)
                        continue;
                    e.Graphics.DrawString("Name: " + item.GetLocalization() + " Rarity: " + item.GetRarity() + " Ducats: " + item.GetDucats() + " Plats: " + item.GetPlats(), font, blackbrush, new Point(5, 20 + i * 30));
                }
            }
        }

        public void RequestRefresh() {
            this.Invalidate();
        }
    }
}
