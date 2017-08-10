using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Windows.Globalization;
using Windows.Media.Ocr;

class iniUtil {

    private string iniPath;

    public iniUtil(string path) {
        this.iniPath = path;  //INI 파일 위치를 생성할때 인자로 넘겨 받음
    }

    [DllImport("kernel32.dll")]
    private static extern int GetPrivateProfileString(    // GetIniValue 를 위해
        String section,
        String key,
        String def,
        StringBuilder retVal,
        int size,
        String filePath);



    [DllImport("kernel32.dll")]
    private static extern long WritePrivateProfileString(  // SetIniValue를 위해
        String section,
        String key,
        String val,
        String filePath);


    // INI 값을 읽어 온다. 
    public String GetIniValue(String Section, String Key) {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
        return temp.ToString();
    }

    // INI 값을 셋팅
    public void SetIniValue(String Section, String Key, String Value) {
        WritePrivateProfileString(Section, Key, Value, iniPath);
    }

}

namespace RelicTool {
    class RelicTool {
        [STAThread]
        static void Main() {
            if (!File.Exists("config.ini")) {
                FileInfo exefileinfo = new FileInfo(Application.ExecutablePath);
                string path = exefileinfo.Directory.FullName.ToString();
                string fileName = @"\config.ini";
                iniUtil ini = new iniUtil(path + fileName);
                ini.SetIniValue("General", "Language", "Korean");
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
