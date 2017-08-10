using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelicTool {
    public class Item {
        private string name;
        private string rarity;
        private string localized;
        private int ducats;
        private int plats;

        public Item() {
            name = "";
            rarity = "";
            ducats = 0;
            plats = 0;
            localized = "";
        }

        public Item(string n, string r, int d) {
            name = n;
            rarity = r;
            ducats = d;
            localized = n;
            plats = 0;
        }

        public string GetName() {
            return name;
        }
        public string GetRarity() {
            return rarity;
        }
        public int GetDucats() {
            return ducats;
        }

        public int GetPlats() {
            return plats;
        }

        public string GetLocalization() {
            return localized;
        }

        public void Localize(string l) {
            localized = l;
        }

        public void SetPlat(int p) {
            plats = p;
        }
    }
    public static class Database {
        public static List<Item> itemList = new List<Item>();
        public static string RewardText = "Select A Reward";
        public static void Initialize() {
            if (!File.Exists(Application.StartupPath + "\\Data\\Ducats.txt")) {
                MessageBox.Show("Data file missing!");
                Environment.Exit(Environment.ExitCode);
            }
            else {
                string[] lines = File.ReadAllLines(Application.StartupPath + "\\Data\\Ducats.txt");
                foreach(string line in lines) {
                    string[] datas = line.Split(';');
                    int ducats = 0;
                    int.TryParse(datas[2], out ducats);
                    itemList.Add(new Item(datas[0], datas[1], ducats));
                }
            }
        }

        public static void Localization(string language) {
            if (string.Equals(language, "English")) {
                foreach (Item item in itemList) {
                    item.Localize(item.GetName());
                }
                RewardText = "Select A Reward";
                return;
            }
            if (!File.Exists(Application.StartupPath + "\\Localization\\" + language + ".txt")) {
                MessageBox.Show("Localization data missing!");
            }
            else {
                string[] lines = File.ReadAllLines(Application.StartupPath + "\\Localization\\" + language + ".txt", Encoding.UTF8);
                RewardText = lines[0].Split(';')[1];
                foreach (string line in lines) {
                    string[] datas = line.Split(';');
                    foreach(Item item in itemList) {
                        if (string.Equals(item.GetName(), datas[0]))
                            item.Localize(datas[1]);
                    }
                }
            }
        }
    }
}
