using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using System.Configuration;
using System;
using System.Windows.Media.Imaging;

namespace RelicTool {
    public class OCRManager {
        public async Task<string> ExtractTextAsync() {
            MemoryStream memoryStream = new MemoryStream();
            InMemoryRandomAccessStream randStream = new InMemoryRandomAccessStream();
            string result = "";
            try {
                await Task.Run(() => ScreenCapture.SaveScreenshot(MainBehavior.instance.Warframe, memoryStream));
                await randStream.WriteAsync(memoryStream.ToArray().AsBuffer());
                if (!OcrEngine.IsLanguageSupported(new Language(Main.instance.GetLanguage())))
                    Console.Write("This language is not supported!");
                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(new Language(Main.instance.GetLanguage()));
                if (ocrEngine == null) {
                    ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                }
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(randStream);
                OcrResult ocrResult = await ocrEngine.RecognizeAsync(await decoder.GetSoftwareBitmapAsync());
                result = ocrResult.Text;
                return result;
            }
            finally {
                memoryStream.Dispose();
                randStream.Dispose();
                GC.Collect(0);
            }
        }
    }
}
