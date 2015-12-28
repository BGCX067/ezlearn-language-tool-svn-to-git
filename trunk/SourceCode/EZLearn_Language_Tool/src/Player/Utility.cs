using System;
using Microsoft.Win32;
namespace Player
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	public class Utility
	{
		public Utility()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		
		public static string getDefaultBrowser() {
			string browser = string.Empty;
			RegistryKey key = null;
			try {
				key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

				//trim off quotes
				browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
				if (!browser.EndsWith("exe")) {
					//get rid of everything after the ".exe"
					browser = browser.Substring(0, browser.LastIndexOf(".exe")+4);
				}
			}
			finally {
				if (key != null) key.Close();
			}
			return browser;
		}

        // 設定精確度核心函式
        // d 原資料, i= 100. 小數點兩位數精確度
        public static double getPreciseData(double d, int i)
        {
            return ((double)((int)(d * 100))) / 100;

        }
        public static double getPreciseData(double d)
        {
            return getPreciseData(d, 100);
        }

        // 取得短檔名
        public static string getShortFilename(string fullfilename)
        {
            // 顯示簡短檔名
            string curFilename = fullfilename;
            int startindex = curFilename.LastIndexOf("\\");
            string curShortFilename = curFilename.Substring(startindex + 1);
            return curShortFilename;
        }

	}
}
