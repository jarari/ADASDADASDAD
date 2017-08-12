using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelicTool {
    class MainBehavior {
        private int rewardcount = 0;
        private long lastrun = DateTime.Now.Ticks;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public static bool RewardScreen = false;
        public static MainBehavior instance;
        public static OCRManager ocrmanager;
        public static Item[] rewards = new Item[4];
        public IntPtr Warframe = GameFinder.Warframe;

        public void Initialize() {
            if (instance == null)
                instance = this;
            
            ocrmanager = new OCRManager();
            Warframe = GameFinder.Warframe;
        }

        public async void CheckScreen() {
            string captured = await ocrmanager.ExtractTextAsync();
            captured = captured.Replace(" ", "");
            if (captured.Contains(Database.RewardText.Replace(" ", "")) && !RewardScreen) {
                RewardScreen = true;
                Main.rewardForm.RequestRefresh();
                lastrun = DateTime.Now.Ticks;
                Console.Write("Entering reward screen..\n");
            }
            if (RewardScreen) {
                long elapsed = DateTime.Now.Ticks - lastrun;
                TimeSpan elapsedSpan = new TimeSpan(elapsed);
                if (elapsedSpan.TotalSeconds >= 25) {
                    RewardScreen = false;
                    rewards = new Item[4];
                    rewardcount = 0;
                    Main.rewardForm.RequestRefresh();
                    return;
                }
                if (rewardcount >= 4)
                    return;
                foreach (Item item in Database.itemList) {
                    if (captured.Contains(item.GetLocalization().Replace(" ", ""))) {
                        bool exists = false;
                        foreach(Item rewarditem in rewards) {
                            if (rewarditem == null)
                                continue;
                            if (string.Equals(item.GetLocalization(), rewarditem.GetLocalization())) {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists && rewardcount < 4) {
                            rewards[rewardcount] = item;
                            rewardcount++;
                            item.SetPlat((int)await MarketPuller.GetCheapest(item.GetName()));
                            Main.rewardForm.RequestRefresh();
                        }
                    }
                }
            }
            
            //Console.WriteLine(captured);
        }

    }
}
