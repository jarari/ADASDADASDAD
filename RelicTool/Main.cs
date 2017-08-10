using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RelicTool {
    public partial class Main : Form {
        private static GameFinder gameFinder;
        private static MainBehavior mainBehavior;
        public static RewardForm rewardForm;
        public static Main instance;
        public Main() {
            instance = this;
            InitializeComponent();

            FileInfo exefileinfo = new FileInfo(Application.ExecutablePath);
            string path = exefileinfo.Directory.FullName.ToString();
            string fileName = @"\config.ini";
            iniUtil ini = new iniUtil(path + fileName);
            string lang = ini.GetIniValue("General", "Language");
            if (lang == null) {
                ini.SetIniValue("General", "Language", "Korean");
                LanguageBox.SelectedIndex = 0;
            }
            else {
                if (string.Equals(lang, "Korean"))
                    LanguageBox.SelectedIndex = 0;
                else
                    LanguageBox.SelectedIndex = 1;
            }

            Application.ApplicationExit += new EventHandler(this.OnAppExit);

            Database.Initialize();
            Database.Localization(LanguageBox.SelectedItem.ToString());
            gameFinder = new GameFinder();
            Thread gameFinderThread = new Thread(gameFinder.StartThread);
            gameFinderThread.Start();

            rewardForm = new RewardForm();
            rewardForm.Show();
            rewardForm.Location = new Point(0, 0);
            rewardForm.Width = Screen.PrimaryScreen.Bounds.Width;
            rewardForm.Height = Screen.PrimaryScreen.Bounds.Height;
        }

        private void OnAppExit(object sender, EventArgs e) {
            Environment.Exit(Environment.ExitCode);
        }

        public void SetStatus(string str) {
            if (StatusLabel.InvokeRequired)
                StatusLabel.Invoke(new MethodInvoker(delegate { StatusLabel.Text = str; }));
            else
                StatusLabel.Text = str;
        }

        private string language = "en-US";
        public string GetLanguage() {
            return language;
        }

        public void RequestGameFinderThread() {
            Thread gameFinderThread = new Thread(gameFinder.StartThread);
            gameFinderThread.Start();
        }

        public void RequestMainBehavior() {
            mainBehavior = MainBehavior.instance;
            if (mainBehavior == null)
                mainBehavior = new MainBehavior();
            mainBehavior.Initialize();
        }

        private const int CS_DROPSHADOW = 0x00020000;
        protected override CreateParams CreateParams {
            get {
                // add the drop shadow flag for automatically drawing
                // a drop shadow around the form
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private bool CloseClicked = false;
        private int ButtonSize = 10;
        private int ButtonPadding = 10;
        protected override void OnPaintBackground(PaintEventArgs e) {
            base.OnPaintBackground(e);
            //배경 그리기
            using (var brush = new SolidBrush(Color.White)) {
                e.Graphics.FillRectangle(brush, 0, 0, this.Width, this.Height);
            }
            //손잡이
            using (var brush = new SolidBrush(Color.Black)) {
                e.Graphics.FillRectangle(brush, 0, 0, this.Width, 30);
            }
            int ShadowHeight = 3;
            //손잡이 그림자
            using (var brush = new LinearGradientBrush(new Point(0, 29),
                new Point(0, 29 + ShadowHeight),
                Color.FromArgb(255, 0, 0, 0),
                Color.FromArgb(5, 0, 0, 0)))
                e.Graphics.FillRectangle(brush, 0, 29, this.Width, ShadowHeight);
            //제목
            e.Graphics.DrawString("Warframe Relic Assistant", this.Font, new SolidBrush(Color.White), new Point(5, 5));
        }
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //닫기 버튼
            using (Pen pen = new Pen(Color.White), pen2 = new Pen(Color.Black))
            using (Brush brush = new SolidBrush(Color.White)) {
                if (CloseClicked) {
                    e.Graphics.FillRectangle(brush, this.Width - 30, 0, 30, 30);
                    e.Graphics.DrawLine(pen2, new Point(this.Width - ButtonPadding - ButtonSize, (30 - ButtonSize) / 2), new Point(this.Width - ButtonPadding, 30 - (30 - ButtonSize) / 2));
                    e.Graphics.DrawLine(pen2, new Point(this.Width - ButtonPadding, (30 - ButtonSize) / 2), new Point(this.Width - ButtonPadding - ButtonSize, 30 - (30 - ButtonSize) / 2));
                }
                else {
                    e.Graphics.DrawLine(pen, new Point(this.Width - ButtonPadding - ButtonSize, (30 - ButtonSize) / 2), new Point(this.Width - ButtonPadding, 30 - (30 - ButtonSize) / 2));
                    e.Graphics.DrawLine(pen, new Point(this.Width - ButtonPadding, (30 - ButtonSize) / 2), new Point(this.Width - ButtonPadding - ButtonSize, 30 - (30 - ButtonSize) / 2));
                }
            }
        }
        private bool FormMove = false;
        private Point PreviousLocation;
        private void Main_MouseDown(object sender, MouseEventArgs e) {
            if(e.Y <= 30 && e.Button == MouseButtons.Left) {
                if (e.X >= this.Width - 30 && e.X <= this.Width) {
                    CloseClicked = true;
                    this.Refresh();
                }
                else {
                    FormMove = true;
                    PreviousLocation = e.Location;
                }
            }
        }

        private void Main_MouseUp(object sender, MouseEventArgs e) {
            if (CloseClicked)
                Environment.Exit(Environment.ExitCode);
            FormMove = false;
            this.Refresh();
        }

        private void Main_MouseMove(object sender, MouseEventArgs e) {
            if (!FormMove)
                return;
            Point difference = e.Location - (Size)PreviousLocation;
            Location += (Size)difference;
        }

        private void LanguageBox_TextUpdate(object sender, EventArgs e) {
            FileInfo exefileinfo = new FileInfo(Application.ExecutablePath);
            string path = exefileinfo.Directory.FullName.ToString();
            string fileName = @"\config.ini";
            iniUtil ini = new iniUtil(path + fileName);
            ini.SetIniValue("General", "Language", LanguageBox.SelectedItem.ToString());
            Database.Localization(LanguageBox.SelectedItem.ToString());

            if (LanguageBox.SelectedIndex == 0)
                language = "ko";
            else
                language = "en-US";
        }
    }
}
