#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine;

namespace MOYV
{
    public class PNGQuantCompress
    {
        private const string TEMP_ROOT =
            "Editor/TextureTools/PNGQuant/";

        private static string ASSET_TEMP_ROOT
        {
            get { return Application.dataPath + "/Game/Framework/Base/" + TEMP_ROOT; }
        }

        private static string PACKAGE_TEMP_ROOT
        {
            get { return Path.GetFullPath("Assets/Game/Framework/Base/" + TEMP_ROOT); }
        }

        public static string RootPath
        {
            get
            {
                bool isPackage = Directory.Exists(Path.GetFullPath("Packages/com.library.base"));
                return isPackage ? PACKAGE_TEMP_ROOT : ASSET_TEMP_ROOT;
            }
        }

        private static bool Compress(string[] texturePath, string qualityPara, int quality)
        {
#if UNITY_EDITOR_WIN
            var shell = RootPath;
#else
            var shell = "mono " + RootPath;
#endif
            shell = shell + "pngquant.exe ";
            var strCmdText = "--quality=" + qualityPara + " --ext .png " + " --force -- " +
                             string.Join(" ", texturePath);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = shell;
            startInfo.Arguments = strCmdText;
            Debug.Log("CMD: " + shell + strCmdText);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            if (ExitCode != 0)
            {
                if (ExitCode == 99 && quality < 79)
                {
                    return Compress(texturePath, $"{quality}-{quality + 20}", quality + 10);
                }
                Debug.LogError("Run CMD CompressTexture Failed : " + ExitCode + "  " +
                               process.StandardError.ReadToEnd() + "\n" + string.Join(" ", texturePath));
                return false;
            }

            return true;
        }

        public static bool Compress(string[] texturePath, int quality)
        {
            var qualityPara = $"{quality - 10}-{quality + 10}";
            return Compress(texturePath, qualityPara, quality);
        }


        public static bool Compress(string[] texturePath, bool lowQuality = false)
        {
            int quality = lowQuality ? 25 : 99;
            var qualityPara = lowQuality ? "20-30" : "80-99";
            return Compress(texturePath, qualityPara, quality);
        }
    }
}