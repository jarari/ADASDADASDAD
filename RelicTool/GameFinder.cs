using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace RelicTool {
    class GameFinder {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string sClass, string sWindow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        private long lastrun = DateTime.Now.Ticks;
        private System.Timers.Timer timer;

        public static GameFinder instance;
        public static IntPtr Warframe;

        public void StartThread() {
            if (instance == null)
                instance = this;

            timer = new System.Timers.Timer();
            timer.Interval = 1500;
            timer.Elapsed += new ElapsedEventHandler(AttachToGame);
            timer.Start();
        }

        public void StopThread() {
            timer.Stop();
        }

        public void AttachToGame(object sender, ElapsedEventArgs e) {
            if(IsWindow(Warframe)) {
                Main.instance.SetStatus("Attached");
                Main.instance.RequestMainBehavior();
                if (MainBehavior.instance != null)
                    MainBehavior.instance.CheckScreen();
            }
            else {
                Console.WriteLine("Game Finder: Finding game...");
                Warframe = FindWindow(null, "WARFRAME");
                Main.instance.SetStatus("Inactive");
            }
        }
    }
}
