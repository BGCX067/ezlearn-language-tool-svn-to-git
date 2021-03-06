// 程式設計技巧:
//     1. 若要使用 skin, 則必須要把 Form 的 Auto Size 設定為 false

//     1. 若要使用 Drag-Drop 則必須要 enable 該元件的 this.AllowDrop=true;  (可以動寫 code 或者是 在 Dynamic properties 中設定)
//     2. 載入圖檔注意事項: 
//              利用 FileStream 開啟檔案串流, 載入圖檔完畢後, 記得要關閉串流,
//              否則就會發生下列現象:
//                                   (奇怪, 已經按照 MSDN 所說的 Dispose , 為何 圖檔還會被 lock ?) 
//     
//                                                                                              [8/17/2005]

// For developer
// [Debug]
//      Please choose "Debug" model to trace the code.
// [Release]
//      choose "Release" model but installation is necessary.

// International Multi-Language aspect
//    - AudioVideoPlayer.cs::MainPannelLanguage(): for initializing the text
//    - MessageManager.cs:: for International Characteracter Setting table

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
using System.Xml;

namespace Player {
    /// <summary>
    /// Summary description for Form1.
    /// </summary>




    public class AVPlayer : System.Windows.Forms.Form {
        static string strPrechar = "版本:";
        //static string strProjectName = "EZLearn 語言學習機 ";
        // 注意: 與專案網頁下載 LanguageTool_20050628_GreenInstall.zip 相關連
        //       號碼一定要一樣, 
        //       否則 client 端會一直顯示舊的版本
        static public bool bBeta = false; // 修改後, 會顯示 Beta 字樣
        static public string strVersion_subNo = "20080308";//"20071118"; // 20061226
        static string strVersionNum = strPrechar + strVersion_subNo;
        // end of 注意

        // 訊息顯示相關
        public int LabelMessage_DefaultDisplay_Time = 4000; // 3 sec

        // 多檔選擇介面 (Multi-File Selection Object)
        public FileListForm myFileListForm = null;     // 新的策略是: 開啟視窗後, 直接產生多檔選擇介面. 關閉只是隱藏而已

        // 多國語言選擇介面 (Multi Language Selection Object)
        public MessageManager L_Manager = null;

        static bool Now_is_PreReleased = false; // 這個功能目前已經不用了

        static KKTimer myTimer;

        public int state = 0; // 預設狀態是 0:執行  1: 暫停  2: 停止播放

        // 若現在不是以語音片段為基礎播放, 則取消所有索引功能 
        // 恢復語音片段播放
        // 1. 只要使用者輸入新的索引. [下 Enter 指令] 詳情請看 Method: textBox1_KeyDown
        // 2. 在 IndexListForm 上的 item 滑鼠操作 
        // 3. 在 聽力轟炸面版上, 直接指定 bSectionBasedPlay 值
        private bool bSectionBasedPlay = false; // 以語音片段為基礎播放,還是正常播放為基礎 ?
        WebBrowser EZwebBrowser = new WebBrowser();

        public bool bMySectionBasedPlay {
            set {
                // 若目前狀態和要設定的狀態一樣, 則不需要處理
                if (value != bSectionBasedPlay) {
                    if (myFileListForm != null)
                        this.myFileListForm.UpdateSectionBasedPlayStatus(value); //  更新顯示結果
                    if (value == true) {
                        // this.label3.Text = "片段播放 ON"; // this.ShowState("以片段為基礎播放");
                        string Info = L_Manager.getMessage("AudioSection_Section_On");// "片段播放 ON";
                        MessageManager.ShowInformation(label3, Info, LabelMessage_DefaultDisplay_Time);
                    } else {
                        //  this.label3.Text = "片段播放 OFF"; // this.ShowState("一般播放");
                        string Info = L_Manager.getMessage("AudioSection_Section_Off"); // 片段播放 OFF"
                        MessageManager.ShowInformation(label3, Info, LabelMessage_DefaultDisplay_Time);
                    }
                    bSectionBasedPlay = value;
                }
            }
            get {
                return bSectionBasedPlay;
            }
        }


        //public bool bLyricReady=false;
        //SwiftlyLabel mySwiftlabel; // 字幕機
        public const string EZLearnFileSuffix = "*.ezu";
        public const string AudioFileSuffix = "*.ezu; *.wav; *.mpa; *.mp2; *.mp3; *.au; *.aif; *.aiff; *.snd; *.wma";
        public const string EZlearnCanPlaySuffix = EZLearnFileSuffix + ";" + AudioFileSuffix;
        public const string filterText = "Audio Files (" + EZlearnCanPlaySuffix + ")|" + EZlearnCanPlaySuffix + "|" + "All Files (*.*)|*.*";

        /*
        public string filterText =
            "Audio files (*.ezu; *.wav; *.mpa; *.mp2; *.mp3; *.au; *.aif; *.aiff; *.snd; *.wma)|*.ezu; *.wav; *.mpa; *.mp2; *.mp3; *.au; *.aif; *.aiff; *.snd; *.wma|" +
            "Video Files (*.avi; *.qt; *.mov; *.mpg; *.mpeg; *.m1v; *.wmv)|*.avi; *.qt; *.mov; *.mpg; *.mpeg; *.m1v; *.wmv|" +
            "MIDI Files (*.mid, *.midi, *.rmKKTimeri)|*.mid; *.midi; *.rmi|" +
            "Image Files (*.jpg, *.bmp, *.gif, *.tga)|*.jpg; *.bmp; *.gif; *.tga|" +
            "All Files (*.*)|*.*";
        */
        private Video ourVideo = null;
        public Audio ourAudio = null;

        public string ExecutationFileDir = System.Environment.CurrentDirectory; // 執行檔本身所在目錄
        // string AudioDefaultDir=System.Environment.CurrentDirectory; // 語音檔預設開啟目錄
        public string FullPropertyFileName = System.Environment.CurrentDirectory + "\\Property.txt"; // 目前開 Property 檔目錄
        public IndexListForm MyIndexList = null;
        private IContainer components;
        #region Winforms variables

        private System.Windows.Forms.MainMenu mnuMain;

        public System.Windows.Forms.OpenFileDialog ofdOpen;
        private System.Windows.Forms.MenuItem menuItem3;



        private System.Windows.Forms.MenuItem mnuOpen;
        // private System.Windows.Forms.MenuItem mnuFile;
        // private System.Windows.Forms.MenuItem menuItem4;
        // private System.Windows.Forms.MenuItem menuItem9;
        // private System.Windows.Forms.MenuItem menuItem11;
        // private System.Windows.Forms.MenuItem menuItem22;
        private JingMenuItem1 mnuFile;
        private JingMenuItem1 menuItem4;
        private JingMenuItem1 menuItem9;
        private JingMenuItem1 menuItem11;
        private JingMenuItem1 menuItem22;

        public JingTextEdit1 textBox1;
        // public System.Windows.Forms.TextBox textBox1;

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        // private System.Windows.Forms.Button button2;
        JingButton button2;
        JingButton button3;
        //private System.Windows.Forms.Button button3;



        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        public System.Windows.Forms.Label label3;
        private System.Windows.Forms.Splitter splitter1;
        public System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        //private System.Windows.Forms.Button button6;
        JingButton button6;

        private System.Windows.Forms.MenuItem menuItem12;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem18;

        private System.Windows.Forms.MenuItem menuItem21;

        private System.Windows.Forms.MenuItem menuItem23;
        private System.Windows.Forms.MenuItem menuItem24;
        private System.Windows.Forms.MenuItem menuItem25;
        private System.Windows.Forms.MenuItem menuItem26;
        // private System.Windows.Forms.Button button1;
        JingButton button1;

        private System.Windows.Forms.MenuItem menuItem2;

        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItem20;
        private System.Windows.Forms.MenuItem menuItem14;

        /*
        private JingMenuItem1 menuItem1;
        private JingMenuItem1 menuItem8;
        private JingMenuItem1 menuItem20;
        private JingMenuItem1 menuItem14;
        */

        private System.Windows.Forms.MenuItem menuItem27;
        private System.Windows.Forms.MenuItem menuItem29;
        private System.Windows.Forms.MenuItem menuItem13;
        private System.Windows.Forms.MenuItem menuItem19;
        private System.Windows.Forms.MenuItem menuItem30;
        private System.Windows.Forms.MenuItem menuItem31;
        private System.Windows.Forms.MenuItem menuItem32;

        // private System.Windows.Forms.Button button7;
        private JingButton button7;

        private System.Windows.Forms.MenuItem menuItem15;
        private System.Windows.Forms.MenuItem menuItem16;
        private System.Windows.Forms.MenuItem menuItem17;
        private System.Windows.Forms.MenuItem menuItem33;
        private System.Windows.Forms.MenuItem menuItem10;
        private System.Windows.Forms.MenuItem menuItem28;
        private System.Windows.Forms.MenuItem menuItem34;
        private System.Windows.Forms.MenuItem menuItem35;
        private System.Windows.Forms.MenuItem menuItem36;
        private System.Windows.Forms.MenuItem menuItem37;
        private System.Windows.Forms.MenuItem menuItem38;
        private System.Windows.Forms.MenuItem menuItem39;
        private MenuItem menuItem40;
        private MenuItem menuItem41;
        private MenuItem menuItem42;
        private MenuItem menuItem43;
        private MenuItem menuItem44;
        private MenuItem menuItem45;
        private MenuItem menuItem46;
        private MenuItem menuItem47;
        private MenuItem menuItem49;
        private MenuItem menuItem48;
        private MenuItem menuItem50;
        private System.Windows.Forms.MenuItem mnuExit;
        #endregion

        public void setText() {
            string strTitle = this.L_Manager.getMessage("ProgramTitle");
            if (Now_is_PreReleased) {

                this.Text = strTitle + "(Beta)";
            } else {
                //this.Text = strProjectName + strVersionNum;
                this.Text = strTitle;
            }
        }

        public NumberLabel l1; // 播放秒數
        public UpdateNotifiyForm myUpdateCheck; // 線上更新視窗 [兩秒鐘後,自動啟動]

        public System.Drawing.Bitmap[] imgNumArray;

        public LyricClass myLyricer;

        string DictionaryConfigFile;
        bool bFirst = true; // 保證只執行一次

        // 定義自己的 TopMost 處理方式
        public bool MyTopMost {
            set {

                if (myFileListForm != null)
                    this.myFileListForm.TopMost = value; // FileListForm 與母視窗同步
                if (MyIndexList != null)
                    this.MyIndexList.TopMost = value; // IndexListForm 與母視窗同步

                TopMost = value; // 先將自己設定資料

            }
            get {
                return TopMost;
            }
        }

        // 當 Form 完全可以顯示工作後, 然後才載入面版
        public void MainFirstActivated(object sender, EventArgs e) {
            // =============== 保證只執行一次
            if (bFirst == false) {
                return;
            }
            bFirst = false;
            // =============================
           
            EZwebBrowser.Navigate(@"http://mqjing.twbbs.org.tw/~ching/Course/JapaneseLanguageLearner/__page/UsageCounter.html");



            // 立即載入 skin  (7/19/2005)
            this.ApplyPlayerSkin((string)PropertyTable["Player主面版"], false);




            // 顯示版本資訊
            // this.label2.Text = "版本: " + strVersion_subNo;
            this.label2.Text = L_Manager.getMessage("Version_String") + strVersion_subNo;// "版本: " + strVersion_subNo; 


            // 開啟語音檔
            OpenFile(); // 開啟語音檔

        }

        // RAR 壓縮方式範例
        /*
               Schematrix.Unrar unrar=new Schematrix.Unrar();
               string fileName="C:\\test.rar";
               unrar.Open(fileName, Schematrix.Unrar.OpenMode.List);
               // Read each header, skipping directory entries
               while(unrar.ReadHeader()) {
                   if(!unrar.CurrentFile.IsDirectory) {
                       ListViewItem item=new ListViewItem(unrar.CurrentFile.FileName);
                       item.SubItems.Add(unrar.CurrentFile.UnpackedSize.ToString());
                       item.SubItems.Add(unrar.CurrentFile.PackedSize.ToString());
                       item.SubItems.Add(unrar.CurrentFile.FileTime.ToString());
                       item.Checked=true;
                       //fileList.Items.Add(item);
                   }
                   unrar.Skip();
               }

               // Cleanup and enable buttons if no exception was thrown
               unrar.Close();
               unrar=null;
        */




        public AVPlayer() {
            this.Activated += new EventHandler(MainFirstActivated);

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();


            // Loading the Property Data
            // 安裝後,使用者第一次執行本應用程式 (先作 Setup)
            if (System.IO.File.Exists(FullPropertyFileName) != true) {
                if (strVersion_subNo.Equals("20061226") == true) {
                    MessageBox.Show("在目前版本或將來的版本中,\n語音片段索引記錄檔存放在執行檔目錄的方式將改成\n\n語音片段索引記錄檔和語音檔, 將存放在同一個目錄中!!\n(若語音檔是 DVD 唯讀的媒體, 則索引檔會放在執行檔目錄中)", "記錄檔位置更改通知");
                }
                MessageBox.Show("You can choose your favored Language on the Menu\n\nOps->Language (Support: 正體中文, 简体中文, English)", "歡迎使用 EZLearn 語言學習機");
                CreateNewPropertyFile();
            } else {
                // 非第一次執行
                // 載入預設資料
                LoadDefaultPropertyFile();
            }

            ToolTipsHandling();

            // 多國語言實驗程式片段
            bool bMultiLanguageTest = true;
            if (bMultiLanguageTest) {
                MultiLanguageSetup(); // 指定元件語言
                this.Text = this.L_Manager.getMessage("ProgramTitle");
            }
            setText();


            this.AllowDrop = true;




            // 自訂 Menu 元件
            // mnuFile.Test(this);
            // mnuFile.GetMainMenu().
            // end of 自訂 Menu 



            // 自訂數字元件
            imgNumArray = NumberLabel.CreateNumImage(ExecutationFileDir, "Num", 0, 0, 0);
            l1 = new NumberLabel(imgNumArray);
            l1.Left = 136;
            l1.Top = 48;
            l1.Height = 30;
            l1.SetWidth(4); // 有四組數字
            this.Controls.Add(l1);
            // end of 自訂數字


            MouseEventHandlingProcess(); // 將滑鼠事件代管給包含他的元件處理 





            // 設定 AVPlay 專屬的 Timer
            myTimer = new KKTimer(this);
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 1000; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
            myTimer.Start();
            // end of 設定 AVPlay 專屬的 Timer 

            // 片段結尾檢查專用 Timer
            KKTimer EndTimer = new KKTimer(this);
            EndTimer.Tick += new EventHandler(CheckEnd_TimerEventProcessor);
            EndTimer.Interval = 200; // 每隔 0.2 秒鐘, 呼叫 TimerEventProcessor procedure
            EndTimer.Start();
            // end of 片段結尾










            // 檢查更新資料
            bNeedtoCheckVersion = isNeedtoCheckVersion();  // 檢查是否需要上網檢查版本資訊 (7/11/2005)
            bNeedtoCheckVersion = true;

            if (bNeedtoCheckVersion) {
                myUpdateCheck = new UpdateNotifiyForm(this);
                //myUpdateCheck.Visible = false;
                // this.label3.Text = "目前正在進行版本檢查 ...";

                DateTime dt = DateTime.Now;
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US"); // 指定顯示模式為 February 1, 2001 is formatted as 2/1/2001
                string strUpdateDate = dt.ToString("d", ci);
                this.PropertyTable["最近的檢查日期"] = strUpdateDate; // 指定目前日期為檢查日期
                this.SaveProperty();
            }
            // end of 檢查更新資料


            this.textBox1.setPlayer(this);



            // Combine the pause and run function into button1
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AVPlayer));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));



            // Dictionary Item 處理
            bool[] bXMLOk = new bool[1];
            DictionaryConfigFile = ExecutationFileDir + "\\Dictionary.xml";
            Dictionary_Update_DictionaryMenu(DictionaryConfigFile, bXMLOk);



            // 取得 TopMost 記錄資料
            ReadTheTopMostData();



        }// end of 建構子結束

        public void ReadLanguageData() {
            string strLanguage = this.getProperty("Language", "English");
            L_Manager.strDefaultLanguage = strLanguage;
        }
        public void SaveLanguageData(string strLanguage) {
            this.setProperty("Language", strLanguage);
        }

        // 讀取 TopMost 資料
        public void ReadTheTopMostData() {
            string strTopMost = this.getProperty("TopMost", "True"); // 預設值為 True
            if (strTopMost.Equals("True")) {
                this.MyTopMost = true;
            } else {
                this.MyTopMost = false;
            }
        }

        // 將 TopMost 資料存檔
        public void SaveTopMostData(bool bData) {
            if (bData) {
                this.setProperty("TopMost", "True");
            } else {
                this.setProperty("TopMost", "False");
            }
        }



        // 載入多國語言設定檔 20070411
        public void MultiLanguageSetup() {
            L_Manager = new MessageManager();

            //L_Manager.strDefaultLanguage = "S_Chinese";

            L_Manager.CreateNewMessageTable("Language.xml"); // 先自動建立, 最後其實是要刪除這行
            L_Manager.Load_MultiLanguage("Language.xml");

            // 網路更新暫停: 先使用 local Language.xml 處理語言記錄的問題
            //  L_Manager.LoadLanguageFromNet(); 
            // L_Manager.Load_MultiLanguage("http://ezlearnlanguage.wiki.zoho.com/HomePage.html");

            // L_Manager.strDefaultLanguage = "English";
            ReadLanguageData(); // 載入預設語言資料

            ResetLanguage(); // immediatly change the language setting
        }

        public void ResetLanguage() {
            // 紀錄 Language Setting
            SaveLanguageData(this.L_Manager.strDefaultLanguage);

            // Change the language run-time
            MainPannelLanguage();
            if (myFileListForm != null)
                this.myFileListForm.FileListFormLanguage();
            if (myOptionsForm != null)
                this.myOptionsForm.OptionsFormLanguage();
            if (MyIndexList != null)
                this.MyIndexList.IndexListFormLanguage();
        }

        public void MainPannelLanguage() {
            // ToolTips Language

            ToolTipsLanguage();

            // System Menu Language
            String strTitles = L_Manager.getMessage("menuItem34.Text");
            this.Text = L_Manager.getMessage("ProgramTitle");
            // 系統資訊
            this.label1.Text = L_Manager.getMessage("System_Second"); //秒數



            // 主面版
            this.mnuFile.Text = L_Manager.getMessage("MainMenu_File"); // 檔案
            this.mnuOpen.Text = L_Manager.getMessage("MainMenu_Open"); // 開啟語音檔
            this.mnuExit.Text = L_Manager.getMessage("MainMenu_Close"); // 關閉


            this.menuItem9.Text = L_Manager.getMessage("MainMenu_Function"); // 功能
            this.menuItem22.Text = L_Manager.getMessage("Version_Update");  //"更新";

            // 語言
            this.menuItem44.Text = L_Manager.getMessage("Language__Item"); // 語言
            this.menuItem45.Text = L_Manager.getMessage("Language__TranChinese"); // "正體中文";
            this.menuItem47.Text = L_Manager.getMessage("Language__SimpleChinese"); //"簡體中文";
            this.menuItem46.Text = L_Manager.getMessage("Language__English");  // "English";

            // Help
            this.menuItem4.Text = L_Manager.getMessage("Help_String"); // 手冊
            this.menuItem5.Text = L_Manager.getMessage("Help__1"); // "1. 按數字後, 按 Enter 可索引播放位置";
            this.menuItem6.Text = L_Manager.getMessage("Help__2"); // "2. 重複播放 --> 按 [Enter] 鍵 or [Shift] 鍵";
            this.menuItem7.Text = L_Manager.getMessage("Help__3"); // "3. 索引之前的語音位置 --> 按 [上/下] 鍵";
            this.menuItem17.Text = L_Manager.getMessage("Help__4"); // "4. 暫停 --> 按 [空白] 鍵";
            this.menuItem26.Text = L_Manager.getMessage("Help__5"); // "5. 可直接輸入數字, 不必清除前面的數字
            this.menuItem38.Text = L_Manager.getMessage("Help__FigureHelp"); // 圖解說明
            this.menuItem40.Text = L_Manager.getMessage("Help__OnLineDocument"); // 線上說明
            this.menuItem42.Text = L_Manager.getMessage("Help__DevelopInfo");// EZLearn 開發資訊
            this.menuItem43.Text = L_Manager.getMessage("Help__OtherProduct_1");//其他產品: 朗文英英字典輔助工具


            // 元件指定語言開始
            this.menuItem34.Text = L_Manager.getMessage("On_Line_Dictionary");// "網際網路上的電子字典";
            this.menuItem35.Text = L_Manager.getMessage("Collegite_Dictionary"); // "Collegite Dictionary (英英字典 有聲音) ";
            this.menuItem36.Text = L_Manager.getMessage("CAMBRIDGE_Dictionary");  // "CAMBRIDGE Dictionary(英英字典 解釋比較簡單)";
            this.menuItem37.Text = L_Manager.getMessage("Chinese_Dictionary");  // "國語字典";
            this.menuItem28.Text = L_Manager.getMessage("Always_On_Top_On"); // "永遠在最上層 (開啟)";
            this.menuItem2.Text = L_Manager.getMessage("Option"); // "選項 ...";
            this.menuItem23.Text = L_Manager.getMessage("Lyric_Option"); //"字幕選項";
            this.menuItem24.Text = L_Manager.getMessage("Lyric_Function_On_Off"); // "顯示/關閉 字幕功能 (預設是開啟)";

            this.menuItem25.Text = L_Manager.getMessage("Lyric_Function_Show_Next_Text"); // "顯示下一句";
            this.menuItem11.Text = L_Manager.getMessage("Test_Main");// "測驗";
            this.menuItem12.Text = L_Manager.getMessage("Test_Japaness_alphabet"); //"認識日文字母";
            this.menuItem33.Text = L_Manager.getMessage("Test_Japaness_Syllabary");// "日文五十音圖 (漢字對照表)";


            // 2007/11/13
            this.menuItem50.Text = L_Manager.getMessage("AudioRecorder");// 錄音;
        }

        // 2005/10/17
        ToolTip toolTip1 = null;
        public void ToolTipsHandling() {
            // 設定 數字輸入區的 ToolTips	
            toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 10000;// Set up the delays for the ToolTip.
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;

            toolTip1.ShowAlways = true;// Force the ToolTip text to be displayed whether or not the form is active.
            // end of ToolTips


        }
        public void ToolTipsLanguage() {
            // Set up the ToolTip text for the Button and Checkbox.
            string[] ToolTipsString ={
                 this.L_Manager.getMessage("MainToolTips__Input"),
                 this.L_Manager.getMessage("MainToolTips__Pause"),
                 this.L_Manager.getMessage("MainToolTips__Resume"),
                this.L_Manager.getMessage("MainToolTips__Scroll"),
                 this.L_Manager.getMessage("MainToolTips__ClearAudioSection"),
                this.L_Manager.getMessage("MainToolTips__ClearCurSection"),
                this.L_Manager.getMessage("MainToolTips__DisplaySectionWindow")
            };

            toolTip1.SetToolTip(this.textBox1, ToolTipsString[0]);
            //toolTip1.SetToolTip(this.button1, "進行索引");
            toolTip1.SetToolTip(this.button2, ToolTipsString[1]);
            toolTip1.SetToolTip(this.button3, ToolTipsString[2]);
            toolTip1.SetToolTip(this.hScrollBar1, ToolTipsString[3]);
            toolTip1.SetToolTip(this.button4, ToolTipsString[4]);
            toolTip1.SetToolTip(this.button5, ToolTipsString[5]);
            toolTip1.SetToolTip(this.button6, ToolTipsString[6]);

        }

        public void MouseEventHandlingProcess() {
            // 將滑鼠事件代管給包含他的元件處理  (主要是為了讓滑鼠移動視窗的動作不會因為在某個元件上空而失去連貫性) 8/12/2005
            System.Windows.Forms.MouseEventHandler HMouseMove = new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseMove);
            System.Windows.Forms.MouseEventHandler HMouseDown = new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseDown);
            System.Windows.Forms.MouseEventHandler HMouseUp = new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseUp);


            l1.MouseMove += HMouseMove;
            l1.MouseDown += HMouseDown;
            l1.MouseUp += HMouseUp;


            label1.MouseMove += HMouseMove;
            label1.MouseDown += HMouseDown;
            label1.MouseUp += HMouseUp;

            label2.MouseMove += HMouseMove;
            label2.MouseDown += HMouseDown;
            label2.MouseUp += HMouseUp;

            label3.MouseMove += HMouseMove;
            label3.MouseDown += HMouseDown;
            label3.MouseUp += HMouseUp;

            // end of 代管處理
        }

        public void setProperty(string key, string newString) {
            string strProperty;
            strProperty = (string)PropertyTable[key];

            // Check for exist
            if (strProperty == null) {
                this.PropertyTable.Add(key, newString);
            } else {
                PropertyTable[key] = newString;
            }

            this.SaveProperty();
        }

        // get the property
        public string getProperty(string key, string defaultString) {
            // At first, checking the key is exist
            string strProperty;


            strProperty = (string)PropertyTable[key];

            if (strProperty == null) {

                // there is no property in the PropertyTable
                // 1. create default and save it to the property file

                this.PropertyTable.Add(key, defaultString); // create a new proerty for the new key
                strProperty = defaultString;

                this.SaveProperty(); // save the property
            }
            return strProperty;
        }

        public void LoadDefaultPropertyFile() {
            this.LoadProperty();// 讀取上次檔案的資料
            // end of 載入預設資料

            // 2005/10/17
            // 檢查 Property 檔案的版本是否正確
            // 使用者可能直接把執行檔複製到目錄中, 導致使用舊版本的 Property 檔
            // 修正方式:
            // 增加設定檔 Property.txt 版本資訊, 當主程式為新版本時, 
            // 會自動更新舊的設定檔的內容, 以防止程式因為更新而產生意外失敗的情況.

            // 完整方式:
            // 程式啟動時, 應該檢查所有必要的檔案
            //   1. 是否存在
            //   2. 版本是否正確
            string strPropertyVersion = (string)PropertyTable["版本號碼"];
            if (strPropertyVersion.Equals(strVersion_subNo) != true) {
                // 刪除版本不符合的設定檔, 並且自動產生新檔
                System.IO.File.Delete(FullPropertyFileName);
                CreateNewPropertyFile();
            } else {
                // 立即載入 skin  (7/19/2005)
                // this.ApplyPlayerSkin((string)PropertyTable["Player主面版"], false);
                // end of skin

                // 檢查目前版本狀態, 以方便確認標題
                string strVersionState = (string)PropertyTable["目前版本狀態"];
                if (strVersionState.IndexOf("最新") == -1) {
                    if (AVPlayer.bBeta) {
                        // this.Text = "語言學習機 (Beta 版本)";  // 更新標題
                        this.Text = this.L_Manager.getMessage("Version_This_Is_Beta");
                    } else {
                        this.menuItem22.Visible = true;// 目前是舊的版本, 顯示 [更新] 按鈕
                        // this.Text = "語言學習機 (網路上已經有新的版本)";  // 更新標題
                        this.Text = this.L_Manager.getMessage("Version_NewVersionMessage");
                    }
                } else {
                    this.menuItem22.Visible = false;
                }
            }
        }
        public void CreateNewPropertyFile() {
            this.PropertyTable.Clear();
            // 設定預設資料
            this.PropertyTable.Add("版本號碼", strVersion_subNo); // 增加設定檔標注版本號碼

            string AudioDefaultDir = System.Environment.CurrentDirectory; // 語音檔預設開啟目錄
            this.PropertyTable.Add("語音檔預設目錄", AudioDefaultDir);
            this.PropertyTable.Add("音標檔預設目錄", AudioDefaultDir); // 音標檔預設開啟目錄

            string AudioDefaultFilename = "PhoneticAudio.wma123"; // 預設音標檔檔名
            this.PropertyTable.Add("音標檔名稱", AudioDefaultFilename); // 音標檔預設開啟目錄

            // 設定版本資訊預設資料
            string bOnlineCheck = "true"; // 預設線上檢查版本
            string strPrjAddress = "http://debut.cis.nctu.edu.tw/~ching/Course/JapaneseLanguageLearner/__page/JapaneseLanguageLearner.htm"; // 專案網址
            this.PropertyTable.Add("是否要線上檢查版本", bOnlineCheck); // 預設要線上檢查版本
            this.PropertyTable.Add("專案網址", strPrjAddress);// 預設專案網址

            string strHowManyDaysTocheck = "" + 3; // 預設一星期檢查一次
            this.PropertyTable.Add("幾天檢查一次", strHowManyDaysTocheck);

            DateTime dt = DateTime.Now;
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US"); // 指定顯示模式為 February 1, 2001 is formatted as 2/1/2001
            string strUpdateDate = dt.ToString("d", ci);
            this.PropertyTable.Add("最近的檢查日期", strUpdateDate); // 指定第一次安裝的日期為檢查日期

            // 目前版本狀態
            string strVersionState = "最新";
            this.PropertyTable.Add("目前版本狀態", strVersionState); // 指定第一次安裝的日期為檢查日期
            this.menuItem22.Visible = false;
            // end of 版本資訊

            // Player面版 skin 資料 (7/19/2005)
            string skinDir = "skin"; // ExecutationFileDir + "\\skin"; // 目前 skin 目錄
            string PlayerSkinFilname = "PlayerSkin_Default.jpg";
            string Playerfull = skinDir + "\\" + PlayerSkinFilname;
            this.PropertyTable.Add("Player主面版", Playerfull);

            // 立即載入 skin
            this.ApplyPlayerSkin((string)PropertyTable["Player主面版"], false);
            // end of skin

            // Repeat Play 
            this.PropertyTable.Add("RepeatPlay", "false"); // 語音檔案自動播放 (語音撥完, 自動由 0 秒 開始播放)
            this.PropertyTable.Add("SectionAutoRepeat", "false"); // 語音片段自動播放 (最後一筆語音片段播放完畢,自動從第一筆開始播放)
            // end of 預設資料


            // 預設指令
            this.PropertyTable.Add("DefaultCommand_Repeat", "20");
            this.PropertyTable.Add("DefaultCommand_Index", "+10");
            this.PropertyTable.Add("DefaultCommand_Blank", "1");
            // end of 預設指令


            // 預設資料存檔
            this.SaveProperty();
        }

        public void Disable_Title_MainMenu() {
            DisableMenu();
            this.FormBorderStyle = FormBorderStyle.None;
        }
        public void Enable_Title_MainMenu() {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            EnableMenu();
        }

        public bool bNeedtoCheckVersion = false;

        // 檢查是否需要上網檢查版本資訊 (7/11/2005)
        private bool isNeedtoCheckVersion() {
            // 1. 檢查使用者是否有設定網路 update 功能
            bool bEnableCheck = false;
            string strOnlineCheck = (string)this.PropertyTable["是否要線上檢查版本"];
            if (strOnlineCheck.IndexOf("true") != -1) {
                bEnableCheck = true;
            } else {
                return bEnableCheck;
            }// end of 網路 update enable

            // 2. 檢查週期是否已經超過
            bool bOutofDate = false;

            string strLastUpdateDate = (string)PropertyTable["最近的檢查日期"];

            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US"); // 
            DateTime myLastUpdateDate =
                DateTime.Parse(strLastUpdateDate,
                culture,
                System.Globalization.DateTimeStyles.NoCurrentDateDefault);

            // 設定檢查是否超過下次要檢查的時間
            string strHowManyDaysTocheck = (string)PropertyTable["幾天檢查一次"];
            int iHowManyDaysTocheck = Int32.Parse(strHowManyDaysTocheck);
            DateTime nextUpDateTime = myLastUpdateDate.AddDays(iHowManyDaysTocheck); // 下次檢查的時間
            if (DateTime.Compare(DateTime.Now, nextUpDateTime) > 0) {
                bOutofDate = true;
            }
            // end of 檢查週期是否已經超過

            return bOutofDate;

        }// end of 檢查版本資訊

        public static void Show_Second_Information(AVPlayer myPlayer) {
            double pos = myPlayer.ourAudio.CurrentPosition;

            // 顯示秒數資訊
            int ipos = (int)pos;
            double dlength = Utility.getPreciseData(myPlayer.ourAudio.Duration, 100); // ((double)((int)(myPlayer.ourAudio.Duration * 100))) / 100; // 取得 audio clip 的長度

            string strSec = "" + ipos; // 秒數資訊
            myPlayer.label1.Left = myPlayer.l1.Left + myPlayer.l1.getRightMostX(strSec.Length); // 將長度資訊黏在 l1 秒數資訊後面
            myPlayer.label1.Text = "" + dlength + " sec";
            myPlayer.l1.Text = strSec;  // 自訂數字元件顯示
            myPlayer.hScrollBar1.Value = (int)ipos; // 設定 scroll bar 位置
            // end of 顯示秒數資訊
        }

        // 事件處理函式:  每隔 1 秒鐘, 讀取 audio play  的位置, 顯示相關資訊
        static long ccc = 0; // set focus 使用
        static long p4 = 0; // 定期清除 label 摽示控制變數
        static long p5 = 0; // 定期清除 label2 摽示控制變數
        public bool bUpdate = false;
        private static void TimerEventProcessor(Object myObject,
            EventArgs myEventArgs) {

            AVPlayer myPlayer = ((KKTimer)myObject).myPlayer;

            if (myPlayer.ourAudio != null) {

                // 顯示秒數資訊
                Show_Second_Information(myPlayer);

                // 最後設定數字輸入區為 focus  (因為不明原因,導致無法在 程式一啟動的時候就設定 輸入區為 focus)
                // 所以只好在啟動的前 time 秒, 持續下達設定 focus的指令
                ccc++;
                const int time = 2;
                if (ccc <= time) {
                    myPlayer.textBox1.Focus();

                    // 檢查更新資料
                    if (myPlayer.bUpdate == false && myPlayer.bNeedtoCheckVersion == true && myPlayer.myUpdateCheck != null) { // 只有第一次啟動時,才會開啟更新視窗
                        string strOnlineCheck = (string)myPlayer.PropertyTable["是否要線上檢查版本"];
                        if (strOnlineCheck.IndexOf("true") != -1) {
                            // OpacityUtility.FadeOut_Only(myPlayer.myUpdateCheck,30);

                            myPlayer.myUpdateCheck.Opacity = 0.0;
                            //  myPlayer.myUpdateCheck.Show();

                            int DH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height; // 取得桌面的高與寬
                            int DW = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                            // myPlayer.myUpdateCheck.Location=new Point(DW/2-myPlayer.myUpdateCheck.Width/2,DH-myPlayer.myUpdateCheck.Height-20);
                            // Update 視窗放在右上角
                            int y = 0;
                            int x = DW - myPlayer.myUpdateCheck.Width;
                            myPlayer.myUpdateCheck.Location = new Point(x, y);

                            OpacityUtility myOpacityObj = new OpacityUtility();
                            myOpacityObj.FadeIn_Only(myPlayer.myUpdateCheck, 30, 0.7); // 淡入
                            myPlayer.myUpdateCheck.FireCheck();
                            myPlayer.bUpdate = true;
                        }
                    }
                    // end of 檢查更新資料

                }
                if (ccc > time + 1) {
                    ccc = 4;
                }
                // end of set focus

                // 定期清除 label2 摽示
                if (myPlayer.label2.Text.CompareTo(" ") != 0) {
                    p4++;
                    if (p4 % 4 == 3) {
                        myPlayer.label2.Text = " ";
                        p4 = 0;
                    }
                }
                // 定期清除 label2 摽示

                /* 理由: 現在 Label3 已經交給 MessageManager 物件專門處理
                // 定期清除 label3 摽示
                const int l3cleart = 10;
                if (myPlayer.label3.Text.CompareTo(" ") != 0) {
                    p5++;
                    if (p5 % l3cleart == l3cleart - 1) {
                        myPlayer.label3.Text = " ";  // 檔案資訊也清除, 以保持版面的乾淨
                        p5 = 0;
                    }
                }
                // 定期清除 label3 摽示
                 */

                /*
                                // 遭遇語音結束點時, 移到下一個段點
                                string[] strSeekTime_End_Repeat=myPlayer.MyIndexList.getCurSectionInfo();  // 取得目前片段資訊
				
                                // 檢查是否有結束標記, 有則不斷檢查目前位置是否已經結束
                                if(strSeekTime_End_Repeat[1]!=null){  
                                    pos=myPlayer.ourAudio.CurrentPosition;
                                    double EndOfSection=myPlayer.MyIndexList.getCurEndtime();
                                    if(ipos >= EndOfSection && ipos <= EndOfSection+1 ){
                                        // 目前播放已經到達該片段的結尾
                                        // 是否 Repeat
                                        //指令: Seek,End ==> 到達 End , 換下一個 Section
                                        if(strSeekTime_End_Repeat[2]==null){
                                            // 沒有 Repeat => 移到下一個片段
                                            if(myPlayer.MyIndexList.hasNext()){
                                                myPlayer.MyIndexList.MoveDown();
                                            }
                                        }else{
                                            // 有 Repeat => 執行 Repeat
                                            // 指令: Seek,End,Repeat ==> 到達 End, 重新 Repeat
                                            myPlayer.MyIndexList.DoRepeat();
                                        }// end of 執行 Repeat
                                    }// end of 到達該片段的結尾
                                }// end of 擴充指令 處理

                                // end of End point test

                */

            }
        }
        // end of  事件處理函式 



        // 遭遇語音片段結尾的處理方式
        // 回傳: true: 已經處理完成
        //       false: 這個函式沒有處理語音
        private static bool SecionCommand_EndProcess(AVPlayer myPlayer) {
            string[] strSeekTime_End_Repeat = myPlayer.MyIndexList.getCurSectionInfo();  // 取得目前片段資訊


            // 目前播放已經到達該片段的結尾
            // 是否 Repeat
            if (myPlayer.MyIndexList.cbAllRepeat.Checked == false) { // 無限迴圈按鈕沒有按下
                //指令: Seek,End ==> 到達 End , 換下一個 Section
                if (strSeekTime_End_Repeat[2] == null) {
                    // 沒有 Repeat => 移到下一個片段
                    if (myPlayer.MyIndexList.hasNext()) {
                        // myPlayer.MyIndexList.MoveDown();
                        myPlayer.MyIndexList.bChangeFromProgramm = true;
                        myPlayer.MyIndexList.PlayNextSection();
                        return true; // 正確執行下一個片段索引工作
                    } else {
                        return false; // 沒有下一個片段, 而且也沒有 Repeat, 所以交給其他函式處理聲音播放的工作
                    }
                } else {
                    // 有 Repeat => 執行 Repeat
                    // 指令: Seek,End,Repeat ==> 到達 End, 重新 Repeat
                    myPlayer.MyIndexList.DoRepeat();
                    return true; // 正確執行 Repeat 的工作
                }// end of 執行 Repeat
            } else {
                // 無限迴圈播放
                bool bCountDown = false;
                myPlayer.MyIndexList.DoRepeat(bCountDown); // 不會對倒數減一的動作
                return true; // 永遠重複播放聲音
            }
        }

        public const int WM_APP = 0x8000;
        public const int WM_NeedToUpdateMessage = (WM_APP + 1);
        protected override void WndProc(ref Message m) {
            string strVersion = this.L_Manager.getMessage("Version_This_is_new"); // 
            string strFireWallProblem = this.L_Manager.getMessage("Version_FirewallProglem");  // 版本更新檢查, 有問題 (請檢查防火牆) ...

            switch (m.Msg) {
                case WM_NeedToUpdateMessage:
                switch ((int)m.WParam) {
                    case 0:
                    MessageManager.ShowInformation(label3, strVersion, 20000);
                    break;
                    case 1:
                    // 根據不同版本, 顯示不同的標題
                    if (AVPlayer.bBeta == false) {
                        this.Text = this.L_Manager.getMessage("Version_NewVersionMessage"); // this.Text = "EZLearn 已經有新版本了";
                    } else {
                        this.Text = this.L_Manager.getMessage("Version_This_Is_Beta"); //this.Text = "EZLearn (Beta 版本)";
                    }

                    break;
                    case -1:
                    MessageManager.ShowInformation(label3, strFireWallProblem, 20000); // MessageManager.ShowInformation(label3, "版本更新檢查, 有問題 (請檢查防火牆) ...", 20000);
                    break;
                    default:
                    MessageManager.ShowInformation(label3, strFireWallProblem, 20000);  //MessageManager.ShowInformation(label3, "版本更新檢查, 有問題 (請檢查防火牆) ...", 20000);
                    break;

                }
                break;

            }
            base.WndProc(ref m);
        }

        // 檢查是否已經到達, 語音檔中的某一片段的結尾. (注意: 一個語音檔包含一個以上的語音片段)
        private static void CheckEnd_TimerEventProcessor(Object myObject,
            EventArgs myEventArgs) {

            // 若現在主面版還沒準備好, 則不進行任何動作
            AVPlayer myPlayer = ((KKTimer)myObject).myPlayer;
            if (myPlayer.ourAudio == null)
                return;

            // 若現在不是片段為基礎的語音播放, 則取消所有檢查語音片段的工作
            if (!myPlayer.bMySectionBasedPlay)
                return;

            // 若現在 IndexList 中沒有任何語音片段, 則不進行任何動作
            if (myPlayer.MyIndexList != null) {
                if (myPlayer.MyIndexList.IsEmpty())
                    return;
            }

            // 遭遇語音結束點時, 移到下一個段點
            string[] strSeekTime_End_Repeat = myPlayer.MyIndexList.getCurSectionInfo();  // 取得目前片段資訊
            double pos;


            // 檢查是否有結束標記, 有則不斷檢查目前位置是否已經結束
            if (strSeekTime_End_Repeat[1] != null) {
                pos = Utility.getPreciseData(myPlayer.ourAudio.CurrentPosition, 100);

                double EndOfSection = myPlayer.MyIndexList.getCurEndtime();
                double End = Utility.getPreciseData(myPlayer.ourAudio.Duration, 100);
                // if (pos >= EndOfSection && pos <= EndOfSection + 1) {
                if (pos >= EndOfSection || pos >= End) {
                    SecionCommand_EndProcess(myPlayer); //遭遇語音片段結尾的處理方式
                }// end of 到達該片段的結尾
            }// end of 擴充指令 處理

            // end of End point test

        }



        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (myPhoneticSymbolForm != null)
                myPhoneticSymbolForm.Dispose();

            CleanupObjects();



            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }



        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        /// 
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AVPlayer));
            this.mnuMain = new System.Windows.Forms.MainMenu(this.components);
            this.mnuFile = new Player.JingMenuItem1();
            this.mnuOpen = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mnuExit = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new Player.JingMenuItem1();
            this.menuItem34 = new System.Windows.Forms.MenuItem();
            this.menuItem35 = new System.Windows.Forms.MenuItem();
            this.menuItem36 = new System.Windows.Forms.MenuItem();
            this.menuItem37 = new System.Windows.Forms.MenuItem();
            this.menuItem28 = new System.Windows.Forms.MenuItem();
            this.menuItem44 = new System.Windows.Forms.MenuItem();
            this.menuItem45 = new System.Windows.Forms.MenuItem();
            this.menuItem47 = new System.Windows.Forms.MenuItem();
            this.menuItem46 = new System.Windows.Forms.MenuItem();
            this.menuItem49 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem48 = new System.Windows.Forms.MenuItem();
            this.menuItem23 = new System.Windows.Forms.MenuItem();
            this.menuItem24 = new System.Windows.Forms.MenuItem();
            this.menuItem25 = new System.Windows.Forms.MenuItem();
            this.menuItem50 = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new Player.JingMenuItem1();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuItem33 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new Player.JingMenuItem1();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.menuItem26 = new System.Windows.Forms.MenuItem();
            this.menuItem39 = new System.Windows.Forms.MenuItem();
            this.menuItem38 = new System.Windows.Forms.MenuItem();
            this.menuItem40 = new System.Windows.Forms.MenuItem();
            this.menuItem41 = new System.Windows.Forms.MenuItem();
            this.menuItem42 = new System.Windows.Forms.MenuItem();
            this.menuItem43 = new System.Windows.Forms.MenuItem();
            this.menuItem22 = new Player.JingMenuItem1();
            this.ofdOpen = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem27 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuItem19 = new System.Windows.Forms.MenuItem();
            this.menuItem29 = new System.Windows.Forms.MenuItem();
            this.menuItem20 = new System.Windows.Forms.MenuItem();
            this.menuItem21 = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuItem18 = new System.Windows.Forms.MenuItem();
            this.menuItem30 = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuItem32 = new System.Windows.Forms.MenuItem();
            this.menuItem31 = new System.Windows.Forms.MenuItem();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button7 = new Player.JingButton();
            this.button1 = new Player.JingButton();
            this.button6 = new Player.JingButton();
            this.button3 = new Player.JingButton();
            this.button2 = new Player.JingButton();
            this.textBox1 = new Player.JingTextEdit1();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFile,
            this.menuItem9,
            this.menuItem11,
            this.menuItem4,
            this.menuItem22});
            // 
            // mnuFile
            // 
            this.mnuFile.Index = 0;
            this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuOpen,
            this.menuItem3,
            this.mnuExit});
            this.mnuFile.OwnerDraw = true;
            this.mnuFile.Text = "檔案";
            // 
            // mnuOpen
            // 
            this.mnuOpen.Index = 0;
            this.mnuOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.mnuOpen.Text = "開啟語音檔";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "-";
            // 
            // mnuExit
            // 
            this.mnuExit.Index = 2;
            this.mnuExit.Text = "關閉";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 1;
            this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem34,
            this.menuItem28,
            this.menuItem44,
            this.menuItem49,
            this.menuItem2,
            this.menuItem48,
            this.menuItem23,
            this.menuItem50});
            this.menuItem9.OwnerDraw = true;
            this.menuItem9.Text = "功能";
            this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
            // 
            // menuItem34
            // 
            this.menuItem34.Index = 0;
            this.menuItem34.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem35,
            this.menuItem36,
            this.menuItem37});
            this.menuItem34.Text = "網際網路上的電子字典";
            // 
            // menuItem35
            // 
            this.menuItem35.Index = 0;
            this.menuItem35.Text = "Collegite Dictionary (英英字典 有聲音) ";
            this.menuItem35.Click += new System.EventHandler(this.menuItem35_Click);
            // 
            // menuItem36
            // 
            this.menuItem36.Index = 1;
            this.menuItem36.Text = "CAMBRIDGE Dictionary(英英字典 解釋比較簡單)";
            this.menuItem36.Click += new System.EventHandler(this.menuItem36_Click);
            // 
            // menuItem37
            // 
            this.menuItem37.Index = 2;
            this.menuItem37.Text = "國語字典";
            this.menuItem37.Click += new System.EventHandler(this.menuItem37_Click);
            // 
            // menuItem28
            // 
            this.menuItem28.Index = 1;
            this.menuItem28.Text = "永遠在最上層 (開啟)";
            this.menuItem28.Click += new System.EventHandler(this.menuItem28_Click_1);
            // 
            // menuItem44
            // 
            this.menuItem44.Index = 2;
            this.menuItem44.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem45,
            this.menuItem47,
            this.menuItem46});
            this.menuItem44.Text = "語言";
            // 
            // menuItem45
            // 
            this.menuItem45.Index = 0;
            this.menuItem45.Text = "正體中文";
            this.menuItem45.Click += new System.EventHandler(this.menuItem45_Click);
            // 
            // menuItem47
            // 
            this.menuItem47.Index = 1;
            this.menuItem47.Text = "簡體中文";
            this.menuItem47.Click += new System.EventHandler(this.menuItem47_Click);
            // 
            // menuItem46
            // 
            this.menuItem46.Index = 2;
            this.menuItem46.Text = "English";
            this.menuItem46.Click += new System.EventHandler(this.menuItem46_Click);
            // 
            // menuItem49
            // 
            this.menuItem49.Index = 3;
            this.menuItem49.Text = "-";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 4;
            this.menuItem2.Text = "選項 ...";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem48
            // 
            this.menuItem48.Index = 5;
            this.menuItem48.Text = "-";
            // 
            // menuItem23
            // 
            this.menuItem23.Index = 6;
            this.menuItem23.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem24,
            this.menuItem25});
            this.menuItem23.Text = "字幕選項";
            // 
            // menuItem24
            // 
            this.menuItem24.Index = 0;
            this.menuItem24.Text = "顯示/關閉 字幕功能 (預設是開啟)";
            this.menuItem24.Click += new System.EventHandler(this.menuItem24_Click);
            // 
            // menuItem25
            // 
            this.menuItem25.Enabled = false;
            this.menuItem25.Index = 1;
            this.menuItem25.Text = "顯示下一句";
            this.menuItem25.Click += new System.EventHandler(this.menuItem25_Click);
            // 
            // menuItem50
            // 
            this.menuItem50.Index = 7;
            this.menuItem50.Text = "錄音";
            this.menuItem50.Click += new System.EventHandler(this.menuItem50_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 2;
            this.menuItem11.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem12,
            this.menuItem33});
            this.menuItem11.OwnerDraw = true;
            this.menuItem11.Text = "測驗";
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 0;
            this.menuItem12.Text = "認識日文字母";
            this.menuItem12.Click += new System.EventHandler(this.menuItem12_Click);
            // 
            // menuItem33
            // 
            this.menuItem33.Index = 1;
            this.menuItem33.Text = "日文五十音圖 (漢字對照表)";
            this.menuItem33.Click += new System.EventHandler(this.menuItem33_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 3;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem5,
            this.menuItem6,
            this.menuItem7,
            this.menuItem17,
            this.menuItem26,
            this.menuItem39,
            this.menuItem38,
            this.menuItem40,
            this.menuItem41,
            this.menuItem42,
            this.menuItem43});
            this.menuItem4.OwnerDraw = true;
            this.menuItem4.Text = "手冊";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "1. 按數字後, 按 Enter 可索引播放位置 (以秒為單位)";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Text = "2. 重複播放 --> 按 [Enter] 鍵 or [Shift] 鍵";
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.Text = "3. 索引之前的語音位置 --> 按 [上/下] 鍵";
            // 
            // menuItem17
            // 
            this.menuItem17.Index = 3;
            this.menuItem17.Text = "4. 暫停 --> 按 [空白] 鍵";
            // 
            // menuItem26
            // 
            this.menuItem26.Index = 4;
            this.menuItem26.Text = "5. 可直接輸入數字, 不必清除前面的數字";
            // 
            // menuItem39
            // 
            this.menuItem39.Index = 5;
            this.menuItem39.Text = "-";
            // 
            // menuItem38
            // 
            this.menuItem38.Index = 6;
            this.menuItem38.Text = "圖解說明";
            this.menuItem38.Click += new System.EventHandler(this.menuItem38_Click);
            // 
            // menuItem40
            // 
            this.menuItem40.Index = 7;
            this.menuItem40.Text = "線上說明";
            this.menuItem40.Click += new System.EventHandler(this.menuItem40_Click);
            // 
            // menuItem41
            // 
            this.menuItem41.Index = 8;
            this.menuItem41.Text = "-";
            // 
            // menuItem42
            // 
            this.menuItem42.Index = 9;
            this.menuItem42.Text = "EZLearn 開發資訊";
            this.menuItem42.Click += new System.EventHandler(this.menuItem42_Click);
            // 
            // menuItem43
            // 
            this.menuItem43.Index = 10;
            this.menuItem43.Text = "朗文英英字典輔助工具";
            this.menuItem43.Click += new System.EventHandler(this.menuItem43_Click);
            // 
            // menuItem22
            // 
            this.menuItem22.Index = 4;
            this.menuItem22.OwnerDraw = true;
            this.menuItem22.Text = "更新";
            this.menuItem22.Click += new System.EventHandler(this.menuItem22_Click);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.label1.Location = new System.Drawing.Point(136, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "秒數";
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("PMingLiU", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.label2.Location = new System.Drawing.Point(96, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 14);
            this.label2.TabIndex = 3;
            this.label2.Text = "版本:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label2_MouseMove);
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.label3.Location = new System.Drawing.Point(8, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(280, 14);
            this.label3.TabIndex = 6;
            this.label3.Text = "Info";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 96);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Cursor = System.Windows.Forms.Cursors.Default;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 76);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(304, 7);
            this.hScrollBar1.TabIndex = 8;
            this.hScrollBar1.CursorChanged += new System.EventHandler(this.hScrollBar1_CursorChanged);
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem8,
            this.menuItem20,
            this.menuItem14,
            this.menuItem32,
            this.menuItem31});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem27});
            this.menuItem1.Text = "檔案";
            // 
            // menuItem27
            // 
            this.menuItem27.Index = 0;
            this.menuItem27.Text = "開啟語音檔";
            this.menuItem27.Click += new System.EventHandler(this.menuItem27_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 1;
            this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem13,
            this.menuItem29});
            this.menuItem8.Text = "功能";
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 0;
            this.menuItem13.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem19});
            this.menuItem13.Text = "字幕選項";
            // 
            // menuItem19
            // 
            this.menuItem19.Index = 0;
            this.menuItem19.Text = "顯示/關閉 字幕功能 (預設是開啟)";
            // 
            // menuItem29
            // 
            this.menuItem29.Index = 1;
            this.menuItem29.Text = "選項...";
            this.menuItem29.Click += new System.EventHandler(this.menuItem29_Click);
            // 
            // menuItem20
            // 
            this.menuItem20.Index = 2;
            this.menuItem20.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem21,
            this.menuItem10});
            this.menuItem20.Text = "測驗";
            this.menuItem20.Click += new System.EventHandler(this.menuItem20_Click);
            // 
            // menuItem21
            // 
            this.menuItem21.Index = 0;
            this.menuItem21.Text = "認識日文字母";
            this.menuItem21.Click += new System.EventHandler(this.menuItem21_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 1;
            this.menuItem10.Text = "日文五十音圖 (漢字對照表)";
            this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click_1);
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 3;
            this.menuItem14.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem18,
            this.menuItem30,
            this.menuItem15,
            this.menuItem16});
            this.menuItem14.Text = "更換主面版背景";
            // 
            // menuItem18
            // 
            this.menuItem18.Index = 0;
            this.menuItem18.Text = "草原面版(Default)";
            this.menuItem18.Click += new System.EventHandler(this.menuItem18_Click);
            // 
            // menuItem30
            // 
            this.menuItem30.Index = 1;
            this.menuItem30.Text = "PDA 形式";
            this.menuItem30.Click += new System.EventHandler(this.menuItem30_Click);
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 2;
            this.menuItem15.Text = "HelloKitty";
            this.menuItem15.Click += new System.EventHandler(this.menuItem15_Click_1);
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 3;
            this.menuItem16.Text = "iPod-mini";
            this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click_1);
            // 
            // menuItem32
            // 
            this.menuItem32.Index = 4;
            this.menuItem32.Text = "-";
            // 
            // menuItem31
            // 
            this.menuItem31.Index = 5;
            this.menuItem31.Text = "離開";
            this.menuItem31.Click += new System.EventHandler(this.menuItem31_Click);
            // 
            // button5
            // 
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.Location = new System.Drawing.Point(16, 35);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(16, 14);
            this.button5.TabIndex = 10;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Transparent;
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.Location = new System.Drawing.Point(0, 35);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(16, 14);
            this.button4.TabIndex = 9;
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(40, 14);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(40, 21);
            this.button7.TabIndex = 13;
            this.button7.Text = "close";
            this.button7.Visible = false;
            this.button7.Click += new System.EventHandler(this.button7_Click_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(208, 14);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(16, 21);
            this.button1.TabIndex = 12;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button6
            // 
            this.button6.Image = ((System.Drawing.Image)(resources.GetObject("button6.Image")));
            this.button6.Location = new System.Drawing.Point(224, 21);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(16, 14);
            this.button6.TabIndex = 11;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Location = new System.Drawing.Point(16, 14);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(16, 21);
            this.button3.TabIndex = 5;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(0, 14);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(16, 21);
            this.button2.TabIndex = 4;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox1.Location = new System.Drawing.Point(104, 14);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.VisibleChanged += new System.EventHandler(this.textBox1_VisibleChanged);
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            this.textBox1.Validated += new System.EventHandler(this.textBox1_Validated);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseDown);
            // 
            // AVPlayer
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(305, 96);
            this.ContextMenu = this.contextMenu1;
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Menu = this.mnuMain;
            this.Name = "AVPlayer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.AVPlayer_Deactivate);
            this.Load += new System.EventHandler(this.AVPlayer_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseUp);
            this.Activated += new System.EventHandler(this.AVPlayer_Activated);
            this.VisibleChanged += new System.EventHandler(this.AVPlayer_VisibleChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.AVPlayer_DragDrop_1);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AVPlayer_Closing);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.AVPlayer_DragEnter);
            this.Resize += new System.EventHandler(this.AVPlayer_Resize);
            this.Validated += new System.EventHandler(this.AVPlayer_Validated);
            this.BackgroundImageChanged += new System.EventHandler(this.AVPlayer_BackgroundImageChanged);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AVPlayer_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AVPlayer_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void DisableMenu() {

            this.Menu = null;
        }
        private void EnableMenu() {
            this.Menu = this.mnuMain;
        }

        private void CleanupObjects() {
            if (myTimer != null) {           // 要先把 myTimer 停住
                myTimer.Stop();
                myTimer.Dispose();
            }

            if (ourVideo != null)
                ourVideo.Dispose();



            if (ourAudio != null) {
                ourAudio.Dispose();
            }
        }

        public void Play_a_File(string FileName) {
            bool bFirstSection = true;
            Play_a_File(FileName, bFirstSection);
        }

        // 播放語音核心函式
        public void Play_a_File(string FileName, bool bFirstSection) {
            this.textBox1.Clear(); // 清除之前的指令
            if (MyIndexList != null) {

                this.MyIndexList.ResetSectionInfo();
            }

            bool bAudioPlayIsReady = false;

            try {
                // 啟動時必要的檢查步驟
                if (ourAudio != null) {   // 非第一次開啟檔案, 可能現在正在播放語音
                    // 因為使用者選擇要播放另外的語音
                    // 若目前已經有聲音正在播放, 則停止播放
                    ourAudio.Stop();
                    ourAudio.Open(FileName);
                    System.Console.WriteLine(FileName);
                } else {
                    // 第一次開啟, 直接建立語音撥放器
                    this.textBox1.Text = "0";  // 只接受 audio 檔案的讀取 (按照原 code, 會造成 video NullReference exception)
                    ourAudio = new Audio(FileName);   // 若 FileName 讀取失敗, 則會發生例外
                    ourAudio.Ending += new System.EventHandler(this.ClipEnded);
                }
                // end of 檢查



                // 設定目前狀態為播放狀態
                bAudioPlayIsReady = true;

                // 設定 Scroll Bar 的長度符合語音檔的長度
                hScrollBar1.Minimum = 0;
                hScrollBar1.Maximum = (int)ourAudio.Duration;

                // 更新預設語音目錄  (將目前使用者指定的語音檔目錄記起來)
                int DirSymbolIndex = FileName.LastIndexOf("\\");
                string AudioDefaultDir = FileName.Substring(0, DirSymbolIndex);
                this.PropertyTable["語音檔預設目錄"] = AudioDefaultDir;   // Update 目前使用者載入目錄
                this.SaveProperty();
                // end of 預設語音目錄的部分


                // 顯示目前開啟的檔名
                string curFilename = FileName;
                int startindex = curFilename.LastIndexOf("\\");
                string curShortFilename = curFilename.Substring(startindex + 1);
                // label3.Text = curShortFilename;

                MessageManager.ShowInformation(label3, curShortFilename, LabelMessage_DefaultDisplay_Time);

            } catch {
                MessageBox.Show("This file could not be opened.", "Invalid file.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }




            if (bAudioPlayIsReady) {
                // 產生索引視窗:
                // 若之前已經有視窗了, 則檢查是否要儲存新的片段資訊
                if (MyIndexList != null) {
                    if (MyIndexList.bModify) {
                        bool bSave = MyIndexList.AskSaveList();
                        if (bSave) {
                            MyIndexList.SaveList();
                        }

                        MyIndexList.bModify = false; // 已經通知使用者了, 所以之後也就不通知了

                    }
                    MyIndexList.UpdateFileData(FileName); // 更新檔案資料
                } else {   // 第一次執行, 建立新的索引視窗
                    MyIndexList = new IndexListForm(this, FileName);
                    MyIndexList.Owner = this; // 設定 Owner 為母視窗, 則使用者按 Tab 鍵時, 將只會看到母視窗
                    MyIndexList.TopMost = this.TopMost;
                    MyIndexList.BringToFront();
                    MyIndexList.Show();
                    MyIndexList.FadeIn();
                    this.Move += new System.EventHandler(this.AVPlayer_Move);
                }

                // 載入片段檔
                MyIndexList.LoadList();


                // 若目前為以片段為播放基礎, 則從第一個片段開始播放
                if (this.bMySectionBasedPlay) {
                    if (bFirstSection) {
                        MyIndexList.bChangeFromProgramm = true;
                        MyIndexList.PlayTheFirstSection();
                    } else {
                        MyIndexList.bChangeFromProgramm = true;
                        MyIndexList.PlayTheLastSection();
                    }
                }


                // 字幕的部分
                if (myLyricer == null)
                    myLyricer = new LyricClass(this, this.hScrollBar1.Bottom + 2, this);
                myLyricer.Prepare(); // 載入字幕所需的所有資源
                if (myLyricer.isLyricReady()) {
                    this.menuItem23.Enabled = true; // 主選單的部分
                    this.menuItem13.Enabled = true; // context Menu 的部分
                    myLyricer.Visible_Enable();
                } else {
                    this.menuItem23.Enabled = false; // 主選單的部分
                    this.menuItem13.Enabled = false; // context Menu 的部分
                    myLyricer.Visible_Disable();
                }
                // end of 字幕的部分


                // 語音播放
                string strSectionAutoRepeat = (string)this.PropertyTable["SectionAutoRepeat"];
                bool bSectionAutoRepeat = bool.Parse(strSectionAutoRepeat);
                //檢查是否要從 第一個 Section 開始播放
                if (bSectionAutoRepeat) {
                    // 移到第一個 Section 播放
                    MyIndexList.MoveFirst();

                    this.label2.Text = this.L_Manager.getMessage("AudioSection_Section_AutoRepeatOn"); // this.label2.Text = "語音片段自動循環" + "  ON";
                }

                // end of 語音播放
                EnableAllFunction();
                this.AllStart();


            } else {
                /*
                // 使用者沒有選檔案
                if(this.ourAudio==null){
                    DisableAllFunction();
                }
                */
            }
        }



        // 直接由 Drag Drop 或者是 Menu 來的開檔指令
        public void OpenFile(string[] FileNames) {
            // 檔案管理物件相關
            if (myFileListForm == null) {
                myFileListForm = new FileListForm(this);
                // myFileListForm.Show();
                myFileListForm.Owner = this;
            }

            if (myFileListForm.Visible == false) {
                myFileListForm.Show();
                myFileListForm.FadeIn();
                myFileListForm.bClosed = false;
            }

            // 將檔案資料加進來
            if (FileNames != null) {
                myFileListForm.Clear_Files(); // 使用者用直接開啟檔案, 則清除前面的檔案選擇列表 (// 因為是直接由 Drag Drop 來的開檔指令, 所以要清除前面的檔案列表)
                myFileListForm.Add_Files(FileNames); // 加入多檔 (可能是目錄, ezu, 或單一語音檔)

                // 馬上進行播放, 第一個檔案
                this.myFileListForm.bChangeFromOtherForm = true;
                this.myFileListForm.PlaySelectedIndex(0);
            }


            // 5 秒後, 自動隱藏以保持桌面整潔
            KKTimer AutoHindeTimer = new KKTimer(this);
            AutoHindeTimer.Tick += new EventHandler(TimerEventProcessor_AutoHindeTimer);
            AutoHindeTimer.Interval = 5000;
            AutoHindeTimer.Start();
        }

        private static void TimerEventProcessor_AutoHindeTimer(Object myObject, EventArgs myEventArgs) {
            KKTimer AutoHindeTimer = (KKTimer)myObject;
            AVPlayer myPlayer = AutoHindeTimer.myPlayer;
            AutoHindeTimer.Stop();

            ChildAutoHind(myPlayer);
        }

        public static void ChildAutoHind(AVPlayer myPlayer) {
            if (myPlayer.myFileListForm != null)
                myPlayer.myFileListForm.FadeOut(150);
            if (myPlayer.MyIndexList != null)
                myPlayer.MyIndexList.FadeOut(150);
        }

        private void OpenFile() {
            if ((ofdOpen.InitialDirectory == null) || (ofdOpen.InitialDirectory == string.Empty)) {
                ofdOpen.InitialDirectory = (string)this.PropertyTable["語音檔預設目錄"];  // 語音檔預設開啟目錄
            }

            ofdOpen.Filter = filterText;
            ofdOpen.Title = "請開啟語音檔";
            ofdOpen.Multiselect = true; // 允許選取多檔案
            int oldHeight = this.Height;



            if (ofdOpen.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel) {
                // 使用者沒有選檔案
                if (this.ourAudio == null) {
                    DisableAllFunction();
                }
                return;
            }

            // 開檔
            OpenFile(ofdOpen.FileNames);



        }

        public void EnableAllFunction() {
            System.Windows.Forms.Button ListButton = this.button6;
            System.Windows.Forms.Button PlayPauseButton = this.button1;
            System.Windows.Forms.Button PlayButton = this.button3;
            System.Windows.Forms.Button PauseButton = this.button2;



            ListButton.Enabled = true;
            PlayPauseButton.Enabled = true;
            PlayButton.Enabled = true;
            PauseButton.Enabled = true;
            this.hScrollBar1.Enabled = true;
        }

        public void DisableAllFunction() {
            System.Windows.Forms.Button ListButton = this.button6;
            System.Windows.Forms.Button PlayPauseButton = this.button1;
            System.Windows.Forms.Button PlayButton = this.button3;
            System.Windows.Forms.Button PauseButton = this.button2;



            ListButton.Enabled = false;
            PlayPauseButton.Enabled = false;
            PlayButton.Enabled = false;
            PauseButton.Enabled = false;

            this.hScrollBar1.Enabled = false;
        }

        public System.Collections.Hashtable PropertyTable = new System.Collections.Hashtable();

        public void SaveProperty() {

            // 把整條 property hash table 存到檔案中
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(FullPropertyFileName)) { // 建立新的 property 檔案
                IDictionaryEnumerator myEnumerator = PropertyTable.GetEnumerator();

                while (myEnumerator.MoveNext()) {
                    string key = (string)myEnumerator.Key;
                    string PropartyValue = (string)myEnumerator.Value;

                    string WriteString = key + "=" + PropartyValue;
                    sw.WriteLine(WriteString);
                }
            }
            // end of  hash table 存到檔案中

            // this.label2.Text="屬性已經儲存";
        }

        public void LoadProperty() {

            PropertyTable.Clear(); // 先把原來的資料清空

            // 由檔案中, 載入 property
            try {

                using (System.IO.StreamReader sr = new System.IO.StreamReader(FullPropertyFileName)) {
                    String line;

                    while ((line = sr.ReadLine()) != null) {
                        int StartIndex = line.LastIndexOf("=");
                        string key = line.Substring(0, StartIndex);
                        string strValue = line.Substring(StartIndex + 1);

                        PropertyTable.Add(key, strValue);
                    }
                }
            } catch (Exception e) {
                // Let the user know what went wrong.
                //Console.WriteLine("The file could not be read:");
                //Console.WriteLine(e.Message);
            }
            // end of 載入 property 

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            AVPlayer myplayer = new AVPlayer();
            if (myplayer.textBox1.Focus() != true) {
                int kk = 0;
                kk++;
            }
            /*
            myplayer.textBox1.Focus();
            if(myplayer.Focused){
                int kk=0;
                kk++;
            }*/
            Application.Run(myplayer);//new AVPlayer());
        }


        public bool Option_Get_RepeatPlayState() {
            string strRepeat = (string)this.PropertyTable["RepeatPlay"];
            bool bRepeat = bool.Parse(strRepeat);
            return bRepeat;
        }

        public void Option_Set_RepeatPlayState(bool bRepeat) {
            if (bRepeat) {
                this.PropertyTable["RepeatPlay"] = "true";
            } else {
                this.PropertyTable["RepeatPlay"] = "false";
            }
        }

        // 目前檔案播放完畢
        private void ClipEnded(object sender, System.EventArgs e) {
            // 先檢查所有的 Command 指令是否正確的執行
            // 若沒有可以執行, 則交給聽力轟炸面版物件處理.

            // 當目前是語音片段播放模式時, 語音片段 Repeat 檢測工作
            if (this.bMySectionBasedPlay) {
                // 若語音片段指令有 repeat 相關設定, 則不作任何處理
                if (SecionCommand_EndProcess(this))
                    return;
            }


            if (ourAudio != null) {
                //  若參數面版直接下達指令是重複播放, 則重播目前的語音檔
                if (this.Option_Get_RepeatPlayState() == true) {
                    ourAudio.Stop();
                    ourAudio.Play();
                    this.L_Manager.getMessage("AudioSection_Section_RepeatPlay");// this.label2.Text = "重複播放";
                } else {
                    // 檔案結尾, 直接交給聽力轟炸面版物件處理. 
                    myFileListForm.bChangeFromOtherForm = true;
                    if (this.myFileListForm.PlayTheNextFile() == false)
                        this.AllStop();
                }

                // 字幕處理
                if (myLyricer.isLyricReady())
                    myLyricer.ClearShowState();
                // end of  字幕處理
            }
        }

        private void mnuOpen_Click(object sender, System.EventArgs e) {
            this.OpenFile();

        }

        private void mnuPlay_Click(object sender, System.EventArgs e) {
            if (ourVideo != null)
                ourVideo.Play();
            else {
                if (ourAudio != null)
                    ourAudio.Play();
            }
        }

        private void mnuStop_Click(object sender, System.EventArgs e) {
            if (ourVideo != null)
                ourVideo.Stop();
            else {
                if (ourAudio != null)
                    ourAudio.Stop();
            }
        }

        private void mnuPause_Click(object sender, System.EventArgs e) {
            if (ourVideo != null)
                ourVideo.Pause();
            else {
                if (ourAudio != null)
                    ourAudio.Pause();
            }
        }

        private void mnuExit_Click(object sender, System.EventArgs e) {
            this.Dispose();
        }

        private void mnuFull_Click(object sender, System.EventArgs e) {
            if (ourVideo != null)
                ourVideo.Fullscreen = !ourVideo.Fullscreen;
        }
        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e) {
            if ((e.Alt) && (e.KeyCode == System.Windows.Forms.Keys.Return)) {
                //mnuFull_Click(mnuFull, null);
            }

            // Allow the control to handle the keystroke now
            base.OnKeyDown(e);
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e) {
            //
        }

        private void button1_Click(object sender, System.EventArgs e) {
            SeekPlay();
            if (bSeekOk == false) {
                // this.label3.Text="狀態: 錯誤的索引";
                this.L_Manager.getMessage("AudioSection_Section_Error_WrongCommand"); // this.label2.Text = "狀態: 錯誤的索引";
            }
        }


        public void setStepRightIndexValue(double value) {
            StepRL = value;
        }
        public void setStepLeftIndexValue(double value) {
            StepLL = value;
        }

        private System.Windows.Forms.KeyEventArgs lastKey = null;
        private System.Collections.ArrayList LastTextList = new System.Collections.ArrayList();
        //private int curTextIndex=-1;
        private int iSpaceKeyC = 0; // 空白鍵被按的次數
        private string beforePauseText = "";
        private double StepRL = 1;
        private double StepLL = 1;

        public void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {


            // 使用者輸入數字完畢, 播放指定秒數的片段
            if (e.KeyCode == Keys.Enter) {
                bMySectionBasedPlay = true; // 語音片段為基礎播放功能啟動

                SeekPlay(); // 立即播放 textBox1.Text 指定秒數的語音
                if (bSeekOk == true) {
                    if (lastKey != null && MyIndexList.checkExist(this.textBox1.Text) != true) {//if(lastKey!=null && lastKey.KeyCode!=Keys.Enter){
                        // 使用者新輸入索引
                        MyIndexList.AddItem(this.textBox1.Text);

                    }
                    // 使用者希望重新聽一次
                    string strStatusInfo = this.textBox1.Text;
                    if (MyIndexList.hasCommentInfo())
                        strStatusInfo = MyIndexList.getCommentInfo();

                    this.label2.Text = this.L_Manager.getMessage("AudioSection_Section_IndexMessage") + strStatusInfo;// this.label2.Text = "索引位置 " + strStatusInfo;

                    this.textBox1.SelectAll(); // 讓使用者下次可以直接輸入數字,不用再用 [Backspace] 清除上次的資料


                    // this.label2.Text="[Enter] 再聽一次";
                } else {
                    // this.label2.Text = "狀態: 錯誤的索引";
                    this.label2.Text = this.L_Manager.getMessage("AudioSection_Section_Error_WrongCommand");
                }
                isUserEditing = false; // 結束編輯狀態
            }


            // 檢驗數字鍵 (若使用者最近一次按的 key 是 Enter 上/下鍵, 則下次按數字鍵時要清除之前的數字)
            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.D1 || e.KeyCode == Keys.D2 || e.KeyCode == Keys.D3 || e.KeyCode == Keys.D4 || e.KeyCode == Keys.D5 || e.KeyCode == Keys.D6 || e.KeyCode == Keys.D7 || e.KeyCode == Keys.D8 || e.KeyCode == Keys.D9) {
                if (lastKey != null && isUserEditing == false) {
                    switch (lastKey.KeyCode) { // 研判上次輸入的 Key
                        case Keys.Enter:     // 檢查上一次 ==> 是否有 Enter  鍵
                        case Keys.Up:        // 檢查上一次 ==> 是否有 按上/ 下 鍵
                        case Keys.Down:
                        case Keys.Space:     //  檢查上一次 ==> 是否有 Space 鍵
                        this.textBox1.Clear();
                        break;
                    }

                }

            }

            // 使用者按上鍵, 取出上一個儲存秒數
            if (e.KeyCode == Keys.Up) {

                MyIndexList.bChangeFromProgramm = true; // 標示這個改變來自其他地方
                MyIndexList.PlayPreviousSection();
            }

            // 使用者按下鍵, 取出上一個儲存秒數
            if (e.KeyCode == Keys.Down) {

                MyIndexList.bChangeFromProgramm = true; // 標示這個改變來自其他地方
                MyIndexList.PlayNextSection();
            }

            // 使用者按右鍵, 快速往下索引 1 秒 [07/11/2005 新增加功能]

            if (e.KeyCode == Keys.Right) {

                double p = ourAudio.CurrentPosition;
                p += StepRL;
                if (p < ourAudio.Duration) {
                    ourAudio.CurrentPosition = p;

                    Show_Second_Information(this);
                    /*
                    double pos = ourAudio.CurrentPosition;
                    int ipos = (int)pos;
                    int ilength = (int)ourAudio.Duration; // 取得 audio clip 的長度
                    String kk = "" + ipos + " / " + ilength + " sec";
                    label1.Text = kk;
                     */
                    //ourAudio.CurrentPosition=p;
                }

            }

            // 使用者按左鍵, 快速往上索引 1 秒 [07/11/2005 新增加功能]
            if (e.KeyCode == Keys.Left) {
                double p = ourAudio.CurrentPosition;
                p -= StepLL;
                if (p >= 0) {
                    ourAudio.CurrentPosition = p;

                    Show_Second_Information(this);
                    /*
                                        double pos = ourAudio.CurrentPosition;
                                        int ipos = (int)pos;
                                        int ilength = (int)ourAudio.Duration; // 取得 audio clip 的長度
                                        String kk = "" + ipos + " / " + ilength + " sec";
                                        label1.Text = kk;
                     */
                }
            }



            // 按空白鍵暫停(奇數)/恢復播放(偶數)  --> 改為 Ctrl 鍵 (不知為什麼? 按下 Ctrl 會產生 shift key code)
            // if (e.KeyCode == Keys.Space && this.textBox1.isExtendeMode() == false) {
            if (e.Control && this.textBox1.isExtendeMode() == false) {
                PauseOrPlay();
            }

            // 把上一次的 key 記錄起來,
            lastKey = e;
        }

        public void PlayNext() {
            // 相當於按 下
        }


        public void AllStop() {
            iSpaceKeyC = 1;
            if (this.mySkin == null || this.mySkin.combo_PlayImage == null

                || this.mySkin.combo_PauseImage == null) {
                // 使用系統預設圖案
                System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AVPlayer));

                AllStop_Sub();
                this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));// 標示可以播放圖案

            } else {
                // 使用自己的影像
                AllStop_Sub();
                this.button1.Image = this.mySkin.combo_PlayImage;// 標示可以播放圖案


            }


        }

        public bool isPause() {
            return iSpaceKeyC != 0;
        }
        public bool isPlay() {
            return iSpaceKeyC == 0;
        }
        public void PauseOrPlay() {
            iSpaceKeyC = (iSpaceKeyC + 1) % 2;

            if (this.mySkin == null || this.mySkin.combo_PlayImage == null || this.mySkin.combo_PauseImage == null) {
                // 使用系統預設圖案
                System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AVPlayer));
                if (iSpaceKeyC == 0) {
                    AllResume();
                    this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));// 標示可以暫停圖案
                } else {
                    AllPause();
                    this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));// 標示可以播放圖案
                }
            } else {
                // 使用自己的影像
                if (iSpaceKeyC == 0) {
                    AllResume();
                    this.button1.Image = this.mySkin.combo_PauseImage;// 標示可以暫停圖案
                } else {
                    AllPause();
                    this.button1.Image = this.mySkin.combo_PlayImage;// 標示可以播放圖案
                }

            }

        }

        private void AllStart() {
            if (this.ourAudio != null) {
                this.ourAudio.Play();
                this.label2.Text = L_Manager.getMessage("System_AllStart"); // this.label2.Text = "開始播放";
                setText(); // 恢復標題
                this.Opacity = 1; // 恢復視窗樣式

                this.myLyricer.Resume(); // 字幕重新開始處理

                state = 0; // 執行

                button2.Enabled = true; // Pause button Enable
                button3.Enabled = false; //Play button Disable
            }

        }

        private void AllResume() {
            if (this.ourAudio != null) {
                this.ourAudio.Play();
                this.label2.Text = L_Manager.getMessage("System_Resume");  // this.label2.Text = "繼續";

                setText(); // 恢復標題
                this.Opacity = 1; // 恢復視窗樣式

                this.myLyricer.Resume(); // 字幕重新開始處理

                // 所有子視窗隱藏
                this.FadeIn_AllChild();

                state = 0; // 執行
            }

        }

        private void AllStop_Sub() {
            state = 2; // 停止
            this.ourAudio.Stop();
            this.label2.Text = L_Manager.getMessage("System_AllStopMessage");  // this.label2.Text = "語音檔播放停止";
            this.Text = L_Manager.getMessage("System_AllStopTitle"); // this.Text = "播放停止";

            // 數字的正規化處理 (去除包含空白的數字)
            try {
                if (this.textBox1.Text.IndexOf(",") == -1) {
                    int SeekNum = System.Int32.Parse(this.textBox1.Text);
                    beforePauseText = "" + SeekNum;
                } else {
                    // 擴充模式不動作
                }
            } catch (System.Exception) {
                // 若this.textBox1不是數字, 則不處理
                // this.textBox1.Text = "0";
            }
            // end of 數字的正規化處理



            this.myLyricer.Pause();  // 字幕暫停
        }

        public void AllPause() {
            if (this.ourAudio != null) {
                state = 1; // 暫停

                this.ourAudio.Pause();
                this.label2.Text = this.L_Manager.getMessage("System_PauseMessage"); // this.label2.Text = "暫停";
                this.Text = this.L_Manager.getMessage("System_PauseTitle"); // this.Text = "暫停播放";

                // 數字的正規化處理 (去除包含空白的數字)
                try {
                    if (this.textBox1.Text.IndexOf(",") == -1) {
                        int SeekNum = System.Int32.Parse(this.textBox1.Text);
                        beforePauseText = "" + SeekNum;
                    } else {
                        // 擴充模式不動作
                    }
                } catch (System.Exception) {
                    // 若this.textBox1不是數字, 則不處理
                    // this.textBox1.Text = "0";
                }
                // end of 數字的正規化處理


                // this.Opacity=0.75; // 暫停時, 視窗透明特效
                // OpacityUtility.Flash_Form(this,800);
                OpacityUtility.Flash_Title(this, 800);
                OpacityUtility.Flash_Label(this, this.label2, 800);	// 閃爍狀態列

                // 所有子視窗隱藏
                this.FadeOut_AllChild();

                this.myLyricer.Pause();  // 字幕暫停
            }
        }



        // 索引播放核心函式
        bool bSeekOk = false;
        public double SeekPlay(bool bRepeat) {
            // 若目前是暫停狀態, 使用者希望立即播放指定索引聲音,則立即啟動播放功能
            if (this.ourAudio.Paused == true) {
                ourAudio.Play();
            }

            // 若現在不是以語音片段為基礎播放, 則取消所有索引功能 
            // 恢復語音片段播放
            // 1. 只要使用者輸入新的索引
            // 2. 在 IndexListForm 上的 item 滑鼠操作
            // 3. 在 聽力轟炸面版上, 直接指定 bSectionBasedPlay 值
            if (!bMySectionBasedPlay) {
                // bSeekOk = false;
                return ourAudio.CurrentPosition;
            }

            // 解析指令, 取出索引位置: SeekPos
            try {
                string SectionCommand = textBox1.Text;
                MyIndexList.UpdateCommandLabel(SectionCommand); // 04/06/2006 顯示 起始與結束秒數
                double SeekPos;
                if (!bRepeat) {
                    // 使用者 Seek 狀態: 則要 Update 資料
                    bSeekOk = this.MyIndexList.UpdateSeekTime_End_Repeat(SectionCommand);  // 若不是 Repeat 狀態, 則不需要 Update Section 資料
                    if (!bSeekOk) {
                        label2.Text = this.L_Manager.getMessage("AudioSection_Section_Error_WrongCommand");//label2.Text = "命令錯誤";

                        return this.ourAudio.CurrentPosition;
                    }
                    SeekPos = this.MyIndexList.getCurSeekTime();
                } else {
                    // Repeat 狀態, 單純由系統取出 Seek 位置
                    SeekPos = this.MyIndexList.getCurSeekTime();

                }

                // 進行索引播放
                double d = SeekPos;
                double ilength = this.ourAudio.Duration; // 取得 audio clip 的長度
                if (d < ilength) {
                    ourAudio.CurrentPosition = d; // 索引到指定的位置
                    bSeekOk = true;

                    // 字幕處理
                    if (myLyricer.isLyricReady())
                        myLyricer.ClearShowState();
                    // end of 字幕處理
                    return d;
                } else {
                    bSeekOk = false;
                    return ourAudio.CurrentPosition;
                }
            } catch (FormatException) {
                /*
                MessageBox.Show("你輸入的位置不是數字\n\n詳細除錯資訊如下:\n"
                    +e);
                    */
                // label2.Text = "輸入格式錯誤";
                label2.Text = this.L_Manager.getMessage("AudioSection_Section_Error_WrongCommand");
                bSeekOk = false;
                return ourAudio.CurrentPosition;
            }

        }

        public double SeekPlay() {
            setText(); // 重新設定標題, 因為有可能播放完畢後, 使用者會點選 Seek, 導致視窗標題還顯示著播放完畢的字樣.
            return SeekPlay(false);
            /*
            label1.Text=textBox1.Text;
            try {
                double d=Double.Parse(label1.Text);
				

                double ilength=this.ourAudio.Duration; // 取得 audio clip 的長度
                if(d<ilength){
                    ourAudio.CurrentPosition=d; // 索引到指定的位置
                    bSeekOk=true;

                    // 字幕處理
                    if(myLyricer.isLyricReady())
                        myLyricer.ClearShowState();
                    // end of 字幕處理
                    return d;
                }else{
                    bSeekOk=false;
                    return ourAudio.CurrentPosition;
                }
            }
            catch(FormatException e) {
                MessageBox.Show("你輸入的位置不是數字\n\n詳細除錯資訊如下:\n"
                    +e);
                bSeekOk=false;
                return ourAudio.CurrentPosition;
            }
            */

        }



        private void textBox1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e) {
            // 不知道為何 Drag Drop 無法執行, 但是 DragEnter 則會被正常呼叫
            int kk = 0;
            kk++;


        }

        private void textBox1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e) {
            /*
                int kk=0;
                kk++;
			
                IDataObject myDataObject=e.Data;
                string[] FormatList=myDataObject.GetFormats();
                string strTarget="FileName";
                int iTargetIndex=-1;
                for(int i=0;i<FormatList.Length;i++){
                    if(FormatList[i].IndexOf(strTarget)!=-1){
                        iTargetIndex=i;
                        break;
                    }
                }// end of search
                Type t=e.GetType();
                string strt=e.ToString();
                bool b=myDataObject.GetDataPresent(DataFormats.Text,true);
                string strFilename=myDataObject.GetData(DataFormats.Text,true).ToString();
                //System.Console.WriteLine(strFilename);
                */
        }

        private void AVPlayer_DragOver(object sender, System.Windows.Forms.DragEventArgs e) {
            int kk = 0;
            kk++;
        }

        private void button2_Click(object sender, System.EventArgs e) {
            this.button2.Enabled = false; // Pause button disable
            this.button3.Enabled = true;  // Play Button enable

            AllPause();
        }

        private void button3_Click(object sender, System.EventArgs e) {
            this.button2.Enabled = true; // Pause button enable
            this.button3.Enabled = false;  // Play Button disable

            this.AllResume();
            /*
            mnuPlay_Click(sender,e);
            this.myLyricer.Resume();
            */
        }

        private void menuItem5_Click(object sender, System.EventArgs e) {

        }

        public int iStat = 0;
        public int ConnectedWindow = 3;
        private void AVPlayer_Deactivate(object sender, EventArgs e) {
            // 當所有的視窗都被 Actived 一次, 
            if (iStat >= ConnectedWindow) // 目前連結管理的視窗包含自己共有 3 個
            {
                iStat = 0;
            }
        }



        // 當視窗被帶上來後, 會呼叫這個 Activated Method
        private void AVPlayer_Activated(object sender, System.EventArgs e) {

            //System.Console.WriteLine("iStat=" + iStat);

            // 只要有一個沒有顯示出來, 就要加入顯示顯示
            if (iStat < ConnectedWindow - 1) {
                iStat = 1;
                if (myFileListForm != null)
                    this.myFileListForm.Focus(); // 當 Form Actived 後, 會將 iStat 

                if (MyIndexList != null && MyIndexList.IsDisposed == false)
                    MyIndexList.Focus();  // 有可能被別的視窗隱藏起來 (當 Form Actived 後, 會將 iStat)


            }

            this.BringToFront(); // 把自己弄上來. 
            this.textBox1.Focus();

        }

        private void trackBar1_Scroll(object sender, System.EventArgs e) {

        }

        private void hScrollBar1_CursorChanged(object sender, System.EventArgs e) {

        }

        private void hScrollBar1_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e) {

            if (ourAudio != null) {
                ourAudio.CurrentPosition = e.NewValue;

                Show_Second_Information(this);
                /*
                                double pos = ourAudio.CurrentPosition;
                                int ipos = (int)pos;
                                int ilength = (int)ourAudio.Duration; // 取得 audio clip 的長度
                                String kk = "" + ipos + " / " + ilength + " sec";
                                label1.Text = kk;
                 */
            }



        }

        //OptionsForm myOptionsForm=null;
        private void menuItem8_Click(object sender, System.EventArgs e) {
            //myOptionsForm=new OptionsForm(this);

            //myOptionsForm.Show();
        }

        private void menuItem9_Click(object sender, System.EventArgs e) {

        }

        // 顯示日本 50 音漢字對應圖
        //PhoneticSymbolForm myPhoneticSymbolForm=null;
        private void menuItem10_Click(object sender, System.EventArgs e) {
            //myPhoneticSymbolForm=new PhoneticSymbolForm(this);
            //myPhoneticSymbolForm.Show();
        }

        private void textBox1_Validated(object sender, System.EventArgs e) {
            this.textBox1.Focus();
        }

        private void AVPlayer_Validated(object sender, System.EventArgs e) {

        }

        private void AVPlayer_VisibleChanged(object sender, System.EventArgs e) {

        }

        private void textBox1_VisibleChanged(object sender, System.EventArgs e) {
            /*	if(this.textBox1.Visible==true){
                    this.textBox1.Focus();
                }
                */
        }

        private void button4_Click(object sender, System.EventArgs e) {
            this.MyIndexList.ClearList();
        }


        private void textBox1_TextChanged_1(object sender, System.EventArgs e) {
            // 理由: 會造成使用者無法輸入, 因為一直顯示前面的資料, "" 或 其他數字, 導致選項索引失靈
            /*
            // 暫停的時候, 要顯示之前輸入的資料 (因為使用者可能把他清除掉了)
            if (iSpaceKeyC == 1) {
                if (beforePauseText.Equals("") != true && beforePauseText.Equals("0") != true)
                    this.textBox1.Text = beforePauseText;
            }
           */
        }

        private void AVPlayer_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ExitProgram();
        }

        private void ExitProgram() {
            this.MyTopMost = false; // 恢復視窗原狀, 以免造成顯示介面無法秀出的問題.
            if (MyIndexList != null) {
                if (MyIndexList.bModify && myLyricer.isLyricReady() == false) {
                    bool bSave = MyIndexList.AskSaveList();
                    if (bSave) {
                        MyIndexList.SaveList();
                    }
                }
            }
        }

        private void button5_Click(object sender, System.EventArgs e) {
            if (isNum(this.textBox1.Text)) {
                bool isOk = MyIndexList.RemoveItem(this.textBox1.Text);
                if (isOk) {
                    // this.label2.Text = this.textBox1.Text + " 已從串列中移除";
                    this.label2.Text = this.textBox1.Text + this.L_Manager.getMessage("AudioSection_Section_RemovedMessage");
                } else {
                    this.label2.Text = this.textBox1.Text + this.L_Manager.getMessage("AudioSection_Section_DoNotRemoveMessage"); // " 不在串列中, 不動作";
                }

            } else {
                // this.label2.Text = "輸入區資料不正確, 不動作";
                this.label2.Text = this.L_Manager.getMessage("AudioSection_Section_Error_WrongCommand");
            }
        }


        private bool isNum(string Text) {
            try {
                System.Int32.Parse(Text);
                return true;
            } catch (System.Exception) {
                return false;
            }
        }

        public void FadeIn_AllChild() {
            MyIndexList.FadeIn();   // 將 List form 帶進來

            if (myFileListForm.bClosed == false) // 當使用者關閉時, 自然無法讓 FileList Form 顯示進來, 你只能用 IndexList Form 的按鈕讓他開啟
                myFileListForm.FadeIn();
        }
        public void FadeOut_AllChild() {
            // MyIndexList.Opacity = 0.0;
            MyIndexList.FadeOut();  //  將 List form 帶出去 

            // myFileListForm.Opacity = 0.0;
            myFileListForm.FadeOut();
        }
        private void button6_Click(object sender, System.EventArgs e) {
            if (MyIndexList != null) {
                MyIndexList.BringToFront(); // 有可能被別的視窗隱藏起來
                this.Focus();

                if (MyIndexList.Opacity == 0 || MyIndexList.Visible == false) {

                    FadeIn_AllChild(); // 將所有子視窗全部帶進來

                    this.textBox1.Focus();
                } else {
                    FadeOut_AllChild(); // 將所有子視窗全部帶出去
                }
            }

        }

        private void AVPlayer_Move(object sender, System.EventArgs e) {

            // 若本身移動時, List 視窗也要移動
            if (MyIndexList != null && MyIndexList.myDockHelp.isConnected) {
                int movY = this.Location.Y + MyIndexList.myDockHelp.DMY;
                int movX = this.Location.X + MyIndexList.myDockHelp.DMX;
                MyIndexList.Location = new System.Drawing.Point(movX, movY);
            }
            //this.Text="c"+ MyIndexList.myDockHelp.isConnected;

            // 若本身移動時, Game 視窗也要移動
            if (myJapanPhoneticSymolGame_From != null && myJapanPhoneticSymolGame_From.myDockHelp.isConnected) {
                int movY = this.Location.Y + myJapanPhoneticSymolGame_From.myDockHelp.DMY;
                int movX = this.Location.X + myJapanPhoneticSymolGame_From.myDockHelp.DMX;
                myJapanPhoneticSymolGame_From.Location = new System.Drawing.Point(movX, movY);
            }

            // 若本身移動時, MultiFileList 視窗也要移動
            if (myFileListForm.MyMother == this) {
                if (myFileListForm != null && myFileListForm.myDockHelp.isConnected) {
                    int movY = this.Location.Y + myFileListForm.myDockHelp.DMY;
                    int movX = this.Location.X + myFileListForm.myDockHelp.DMX;
                    myFileListForm.Location = new System.Drawing.Point(movX, movY);
                }
            }


        }

        // 訓練: 五十音練習
        JapanPhoneticSymolGame_From myJapanPhoneticSymolGame_From;
        private void menuItem12_Click(object sender, System.EventArgs e) {
            myJapanPhoneticSymolGame_From = new JapanPhoneticSymolGame_From(this);
            myJapanPhoneticSymolGame_From.Show();
        }

        private void label2_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {

        }

        private void AVPlayer_BackgroundImageChanged(object sender, System.EventArgs e) {

        }

        private void button7_Click(object sender, System.EventArgs e) {
            this.DisableMenu();
            //this.Height=this.mnuMain.
        }

        private void menuItem13_Click(object sender, System.EventArgs e) {

        }



        bool isUserEditing = false;
        private void textBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            isUserEditing = true;  // 當使用者按下 Enter 時, Editing 狀態 =false
        }




        public void ApplyPlayerSkin(string full, bool bsave) {
            if (Skin_CheckUserDefine(full) == false) {
                this.Region = null;
                try {
                    System.Drawing.Bitmap imgPlayerBG = new System.Drawing.Bitmap(full); // 目前載入影像
                    BackgroundImage = imgPlayerBG;

                    PropertyTable["Player主面版"] = full;

                    if (bsave) {
                        this.SaveProperty(); // 儲存起來,下次可以用
                        this.label2.Text = this.L_Manager.getMessage("Skin_SaveMessage"); // this.label2.Text = "面版更新完成";
                    }
                } catch (Exception ee) {
                    MessageBox.Show(full + " 可能不存在\n詳細資訊: " + ee.ToString());
                }
            }
        }

        // 載入Player 主面版
        public void ApplyPlayerSkin(string ImagePath, string filename, bool bsave) {
            string full = ImagePath + "\\" + filename;
            ApplyPlayerSkin(full, bsave);
        }

        private bool Skin_CheckUserDefine(string skinName) {
            string fullSkinXMLSetupFile = null;
            bool bUserDefine = false;
            if (skinName.Equals("iPod-mini")) {
                fullSkinXMLSetupFile = "\\skin\\iPod_by_Jing";
                fullSkinXMLSetupFile = this.ExecutationFileDir + fullSkinXMLSetupFile;
                LoadSkin(fullSkinXMLSetupFile);
                bUserDefine = true;
            }
            if (skinName.Equals("HelloKitty")) {
                fullSkinXMLSetupFile = "\\skin\\HelloKitty_by_Jing";
                fullSkinXMLSetupFile = this.ExecutationFileDir + fullSkinXMLSetupFile;
                LoadSkin(fullSkinXMLSetupFile);
                bUserDefine = true;
            }
            if (skinName.Equals("Palm")) {
                fullSkinXMLSetupFile = "\\skin\\Palm_by_fugenet";
                fullSkinXMLSetupFile = this.ExecutationFileDir + fullSkinXMLSetupFile;
                LoadSkin(fullSkinXMLSetupFile);
                bUserDefine = true;
            }
            if (skinName.Equals("Default")) {
                DefaultSkin();
                /*
                fullSkinXMLSetupFile = "\\skin\\Default_by_Jing";
                fullSkinXMLSetupFile = this.ExecutationFileDir + fullSkinXMLSetupFile;
                bool bDisableMainMenu = false;
                LoadSkin(fullSkinXMLSetupFile, bDisableMainMenu);
                bUserDefine = true;
                */
                bUserDefine = true;
            }
            return bUserDefine;
        }

        Skin mySkin = null;

        // iPod-Mini
        private void menuItem16_Click_1(object sender, System.EventArgs e) {
            iPodSkin();
        }

        // Hello Kitty Skin
        private void menuItem15_Click_1(object sender, System.EventArgs e) {
            HelloKitySkin();
        }

        private void menuItem30_Click(object sender, System.EventArgs e) {
            PalmSkin();
        }

        // Default Skin
        private void menuItem18_Click(object sender, System.EventArgs e) {
            DefaultSkin();
        }

        public void iPodSkin() {
            string fullSkinXMLSetupFile = this.ExecutationFileDir + "\\skin\\iPod_by_Jing";
            LoadSkin(fullSkinXMLSetupFile);
            PropertyTable["Player主面版"] = "iPod-mini";
            this.SaveProperty();
        }

        public void HelloKitySkin() {
            string fullSkinXMLSetupFile = this.ExecutationFileDir + "\\skin\\HelloKitty_by_Jing";
            LoadSkin(fullSkinXMLSetupFile);
            PropertyTable["Player主面版"] = "HelloKitty";
            this.SaveProperty();
        }

        public void PalmSkin() {
            string fullSkinXMLSetupFile = this.ExecutationFileDir + "\\skin\\Palm_by_fugenet";
            LoadSkin(fullSkinXMLSetupFile);
            PropertyTable["Player主面版"] = "Palm";
            this.SaveProperty();
        }

        public void DefaultSkin() {
            string fullSkinXMLSetupFile = this.ExecutationFileDir + "\\skin\\Default_by_Jing";
            bool bDisableMainMenu = false;
            LoadSkin(fullSkinXMLSetupFile, bDisableMainMenu);
            PropertyTable["Player主面版"] = "Default";
            this.SaveProperty();
        }

        private void LoadSkin(string fullSkinXMLSetupFile) {
            LoadSkin(fullSkinXMLSetupFile, true);
        }
        private void LoadSkin(string fullSkinXMLSetupFile, bool bDisableMainMenu) {
            if (bDisableMainMenu) {
                Disable_Title_MainMenu();// 關閉 主 menu
            } else {
                Enable_Title_MainMenu();
            }
            JingButton PlayPauseButton = button1;
            JingButton PlayButton = button3;
            JingButton PauseButton = button2;
            JingButton IndexListButton = button6;
            JingButton CloseButton = button7;

            JingTextEdit1 Input = this.textBox1;
            System.Windows.Forms.Label TotalLengthLabel = this.label1;
            NumberLabel TimeLabel = this.l1;
            System.Windows.Forms.Label FilenameLabel = this.label3;
            System.Windows.Forms.Label StatusLabel = this.label2;

            PauseButton.setSelfPaint();
            PlayButton.setSelfPaint();
            PlayPauseButton.setSelfPaint();
            IndexListButton.setSelfPaint();
            CloseButton.setSelfPaint();

            // string SkinDir=this.ExecutationFileDir+"\\skin\\HelloKitty_by_Jing";
            string SkinDir = fullSkinXMLSetupFile;
            mySkin = new Skin(SkinDir, this, Input, StatusLabel, FilenameLabel, PlayPauseButton, PlayButton, PauseButton, IndexListButton, CloseButton, hScrollBar1, TimeLabel, TotalLengthLabel);
            // mySkin.conLoadHelloKittySkin();
            mySkin.LoadSkin();


            // 重新設定視窗的大小
            //int MenuHeight2 = this.Menu.GetForm().Height;
            if (this.Menu == null) {
                int MenuHeight = 32;
                System.Drawing.Rectangle newBound = new System.Drawing.Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height - MenuHeight);
                this.Bounds = newBound;
            }

            // 重新設定子視窗的預設位置
            if (myFileListForm != null)
                this.myFileListForm.SetDefaultLocation();
            if (MyIndexList != null)
                this.MyIndexList.SetDefaultLocation();



        }

        private void button7_Click_1(object sender, System.EventArgs e) {
            ExitProgram();
            this.Dispose();
        }

        private void AVPlayer_Load(object sender, System.EventArgs e) {


        }

        private void menuItem31_Click(object sender, System.EventArgs e) {
            this.Dispose();
        }


        // [滑鼠功能] 當滑鼠移動時
        bool DRAGGING = true;
        bool m_dwFlags = false;
        int Py, Px;
        private void AVPlayer_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (m_dwFlags && DRAGGING) {
                this.Left += e.X - Px;
                this.Top += e.Y - Py;
            }
        }

        //[滑鼠功能] 當滑鼠左鍵彈起時
        private void AVPlayer_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            switch (e.Button) {
                case MouseButtons.Left:
                if (m_dwFlags && DRAGGING) {
                    // 若目前是 dragging 狀態
                    // m_dwFlags &= ~DRAGGING;
                    m_dwFlags = !DRAGGING;
                }
                break;
            }
        }


        // [滑鼠功能] 當滑鼠左鍵按下時
        private void AVPlayer_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            switch (e.Button) {
                case MouseButtons.Left:
                // if ( !(m_dwFlags & DRAGGING) ) {
                if (!(m_dwFlags && DRAGGING)) {
                    // 若目前不是 dragging 狀態
                    Py = e.Y;
                    Px = e.X;

                    // m_dwFlags |= DRAGGING;
                    m_dwFlags = DRAGGING;
                }
                break;
            }

        }




        private void menuItem27_Click(object sender, System.EventArgs e) {
            this.OpenFile();
        }

        private void menuItem28_Click(object sender, System.EventArgs e) {
            //myPhoneticSymbolForm=new PhoneticSymbolForm(this);
            //myPhoneticSymbolForm.Show();
        }

        private void menuItem29_Click(object sender, System.EventArgs e) {
            myOptionsForm = new OptionsForm(this);
            myOptionsForm.Show();
        }

        OptionsForm myOptionsForm = null;
        private void menuItem2_Click(object sender, System.EventArgs e) {

            myOptionsForm = new OptionsForm(this);
            myOptionsForm.Show();
        }


        private void button1_Click_1(object sender, System.EventArgs e) {

            PauseOrPlay();
        }

        private void menuItem25_Click(object sender, System.EventArgs e) {
            this.myLyricer.bVisible = !this.myLyricer.bVisible;
            if (!this.myLyricer.bShowNext) {
                menuItem25.Text = "不顯示下一句歌詞";
                myLyricer.showNext();
            } else {
                menuItem25.Text = "顯示下一句歌詞";
                myLyricer.DisableShowNext();
            }

        }

        private void menuItem24_Click(object sender, System.EventArgs e) {
            menuItem24.Text = this.myLyricer.VisibleSwitch();
        }

        private void label2_Click(object sender, System.EventArgs e) {

        }

        private void menuItem22_Click(object sender, System.EventArgs e) {
            string target = (string)PropertyTable["專案網址"];
            System.Diagnostics.Process.Start("IExplore.exe", target);
        }

        private void menuItem21_Click(object sender, System.EventArgs e) {
            myJapanPhoneticSymolGame_From = new JapanPhoneticSymolGame_From(this);
            myJapanPhoneticSymolGame_From.Show();
        }

        private void menuItem20_Click(object sender, System.EventArgs e) {

        }

        private void menuItem19_Click(object sender, System.EventArgs e) {

        }



        // 大理石面版
        private void menuItem17_Click(object sender, System.EventArgs e) {
            string skinDir = ExecutationFileDir + "\\skin"; // 目前 skin 目錄
            string PlayerSkinFilname = "PlayerSkin_Marble.jpg";
            string Playerfull = skinDir + "\\" + PlayerSkinFilname;
            this.ApplyPlayerSkin(Playerfull, true);

        }

        // 褐色木質面版
        private void menuItem16_Click(object sender, System.EventArgs e) {
            string skinDir = ExecutationFileDir + "\\skin"; // 目前 skin 目錄
            string PlayerSkinFilname = "PlayerSkin_DarkWood.jpg";
            string Playerfull = skinDir + "\\" + PlayerSkinFilname;
            this.ApplyPlayerSkin(Playerfull, true);
        }

        // 木質面版
        private void menuItem15_Click(object sender, System.EventArgs e) {
            string skinDir = ExecutationFileDir + "\\skin"; // 目前 skin 目錄
            string PlayerSkinFilname = "PlayerSkin_Wood.jpg";
            string Playerfull = skinDir + "\\" + PlayerSkinFilname;
            this.ApplyPlayerSkin(Playerfull, true);
        }

        PhoneticSymbolForm myPhoneticSymbolForm = null;
        private void menuItem33_Click(object sender, System.EventArgs e) {
            myPhoneticSymbolForm = new PhoneticSymbolForm(this);
            myPhoneticSymbolForm.Show();

        }

        private void menuItem10_Click_1(object sender, System.EventArgs e) {
            myPhoneticSymbolForm = new PhoneticSymbolForm(this);
            myPhoneticSymbolForm.Show();
        }

        // 檢查是否有舊的版本: old property
        private void CheckOldVersion() {
            // 檢查 Property.txt
            if (System.IO.File.Exists(FullPropertyFileName) == true) {
                System.IO.File.Delete(FullPropertyFileName);
            }
        }

        public void ShowState(string msg) {
            label2.Text = msg;
        }

        private bool bAlwaysOnTop = true;
        private void menuItem28_Click_1(object sender, System.EventArgs e) {
            bAlwaysOnTop = !bAlwaysOnTop;
            if (bAlwaysOnTop == true) {
                menuItem28.Text = L_Manager.getMessage("Always_On_Top_On");// menuItem28.Text = "永遠在最上層 (開啟)";
                this.MyTopMost = true;
                SaveTopMostData(MyTopMost); // 資料存檔
            } else {
                menuItem28.Text = L_Manager.getMessage("Always_On_Top_Off"); //menuItem28.Text = "永遠在最上層 (關閉)";
                this.MyTopMost = false;
                SaveTopMostData(MyTopMost); // 資料存檔
            }
        }


        private void menuItem35_Click(object sender, System.EventArgs e) {
            string target = @"http://www.m-w.com/";
            // System.Diagnostics.Process.Start("IExplore.exe",target);
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target); // Default browser will be lanuched to the target target.
            //System.Diagnostics.Process.Start("http://www.google.com");  // 會發生例外
        }

        private void menuItem36_Click(object sender, System.EventArgs e) {
            string target = "http://dictionary.cambridge.org/";
            // System.Diagnostics.Process.Start("IExplore.exe",target);
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }

        private void menuItem37_Click(object sender, System.EventArgs e) {

            string target = "http://140.111.1.43/";
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }



        ArrayList Dictionary_ItemList = null;
        private void Dictionary_menuItem_Click(object sender, System.EventArgs e) {
            MenuItem Me = (MenuItem)sender;
            int i = Me.Index;
            string[] Item_Title_Address = (string[])Dictionary_ItemList[i];
            Me.Text = Item_Title_Address[0];
            string target = Item_Title_Address[1];
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }

        private ArrayList Dictionary_Update_DictionaryMenu(string DictionaryConfigFile, bool[] bXMLOk) {
            int i;
            if (System.IO.File.Exists(DictionaryConfigFile) == false) {
                // 建造立新的 Dictionary.txt
                // (內容為預設值)
                Dictionary_Create_New_Config_File(DictionaryConfigFile);
            }// end of create a new file

            MenuItem DictionarRoot = this.menuItem34;
            int SubItemNum = DictionarRoot.MenuItems.Count; // 原來的 item 數量
            for (i = 0; i < SubItemNum; i++) {
                DictionarRoot.MenuItems.RemoveAt(0);
            }
            // 舊的資料全部移除完畢

            Dictionary_ItemList = Dictionary_Read_All_Item(DictionaryConfigFile, bXMLOk);
            System.EventHandler DictionaryMenuEventHandler = new System.EventHandler(this.Dictionary_menuItem_Click);

            for (i = 0; i < Dictionary_ItemList.Count; i++) {
                string[] Title_Address = (string[])Dictionary_ItemList[i];
                MenuItem Item = new MenuItem();
                Item.Index = i;
                Item.Text = Title_Address[0];
                Item.Click += DictionaryMenuEventHandler;// 改為專屬的事件處理器
                DictionarRoot.MenuItems.Add(Item);

            }

            // 加入最後一個可以手動設定 menu 的按鈕
            Dictionary_Add_ConfigItem(DictionarRoot, i);
            return Dictionary_ItemList;
        }



        private void Dictionary_Add_ConfigItem(MenuItem DictionarRoot, int i) {
            MenuItem Item = new MenuItem();
            Item.Index = i;
            Item.Text = L_Manager.getMessage("Add_New_Item");
            // Item.Text = "(+) 新增項目 ...";

            Item.Click += new System.EventHandler(this.Dictionary_ConfigItem_Click);
            ;// 改為專屬的事件處理器
            DictionarRoot.MenuItems.Add(Item);


        }
        private void Dictionary_ConfigItem_Click(object sender, System.EventArgs e) {
            System.Diagnostics.Process.Start("NotePad.exe", DictionaryConfigFile);
            bool oldTopMost = this.MyTopMost;
            this.MyTopMost = false;

            System.Windows.Forms.MessageBox.Show("請先修改 Dictionary.xml設定檔\n再按[確定]");



            bool[] bXMLOk = new bool[1];
            Dictionary_Update_DictionaryMenu(DictionaryConfigFile, bXMLOk);
            if (bXMLOk[0] == true)
                System.Windows.Forms.MessageBox.Show("字典資料載入完成");
            this.MyTopMost = oldTopMost;

        }

        private void Dictionary_Create_New_Config_File(string DictionaryConfigFile) {
            using (System.IO.StreamWriter writer = System.IO.File.CreateText(DictionaryConfigFile)) {
                string[] Default ={"<!-- 字典連結設定檔",
									 " ",
									 "    功能: 你可以加入自己發現的字典",
									 "    格式範例:",
									 "           <MenuItem Title=\"國語辭典\" Address=\"http://140.111.1.43/\"/>",
									 " ",
									 "    手動編輯工具: NotePad.exe 存檔格式: UTF-8",
									 "-->",
									 "<EZLearn_Dictionary_List CodeName=\"Default_Dictionary_Data\" >",
									 " ",
									 "<MenuItem Title=\"Collegite Dictionary (英英字典 有聲音)\" Address=\"http://www.m-w.com/\"/>",
									 "<MenuItem Title=\"CAMBRIDGE Dictionary (英英字典 解釋比較簡單)\" Address=\"http://dictionary.cambridge.org/\"/>",
									 "<MenuItem Title=\"國語辭典\" Address=\"http://140.111.1.43/\"/>",
									 " ",
									 "</EZLearn_Dictionary_List>"};
                for (int i = 0; i < Default.Length; i++) {
                    writer.WriteLine(Default[i]);
                }

            }// end of using
        }

        private ArrayList Dictionary_Read_All_Item(string DictionaryConfigFile, bool[] bXMLOk) {
            XmlValidatingReader reader = null;
            ArrayList Dictionary_ItemList = new ArrayList();

            try {
                XmlTextReader txtreader = new XmlTextReader(DictionaryConfigFile);
                txtreader.WhitespaceHandling = WhitespaceHandling.None;

                //Implement the validating reader over the text reader. 
                reader = new XmlValidatingReader(txtreader);
                reader.ValidationType = ValidationType.None;


                //Parse the XML fragment.  If they exist, display the   
                //prefix and namespace URI of each element.
                string[] Item_Title_Address = null;
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                        Console.WriteLine("<{0}>", reader.LocalName); // Debug Information
                        Item_Title_Address = new string[2];
                        bool bOk = Dictionary_Read_Item(reader, Item_Title_Address);
                        if (bOk == true)
                            Dictionary_ItemList.Add(Item_Title_Address);
                        break;
                    }
                }
                txtreader.Close();
                bXMLOk[0] = true;
            } catch (System.Xml.XmlException ee) {
                System.Windows.Forms.MessageBox.Show("XML 語法檢查錯誤\n\n請修正 Dictionary.xml內容.\n\n如果想要回復預設值,\n請刪除 Dictionary.xml 檔即可");
                Console.WriteLine("Exception " + ee);
                bXMLOk[0] = false;
            }
            return Dictionary_ItemList;
        }

        private bool Dictionary_Read_Item(XmlValidatingReader reader, string[] Item_Title_Address) {

            string ElementName = reader.LocalName;

            bool bok = false;
            switch (ElementName) {
                case "MenuItem":
                string strTitle = reader["Title"];// 讀取 Dictionary Title 名稱
                string strAddress = reader["Address"];// 讀取 http 位置
                Item_Title_Address[0] = strTitle;
                Item_Title_Address[1] = strAddress;
                // Console.WriteLine("<{0}><{1}> ", strTitle, strAddress);
                bok = true;
                break;
            }
            return bok;
        }

        private void menuItem38_Click(object sender, System.EventArgs e) {
            HelpForm MyHelpForm = new HelpForm();
            MyHelpForm.Show();
        }



        // 系統管理函式: 處理母視窗放大縮小時, 子視窗的對應動作
        // 當視窗上面 Control Box 的 MinimumBox 被按下時, 會呼叫下面這個 Resize Event
        bool bLastMin = false;
        private void AVPlayer_Resize(object sender, EventArgs e) {

            switch (this.WindowState) {
                case FormWindowState.Minimized:
                // 當母視窗縮小時, 子視窗直接 FadeOut
                if (myFileListForm != null)
                    this.myFileListForm.FadeOut();

                if (MyIndexList != null)
                    this.MyIndexList.FadeOut();
                bLastMin = true;
                break;
                case FormWindowState.Normal:
                if (bLastMin == true) {
                    // 當母視窗回復正常時, 子視窗取回原來的位置. 並直接 FadeIn
                    if (this.myFileListForm != null) {
                        this.myFileListForm.Location = this.myFileListForm.oldLocation;
                        this.myFileListForm.FadeIn();
                    }
                    if (this.MyIndexList != null) {
                        this.MyIndexList.Location = this.MyIndexList.oldLocation;
                        this.MyIndexList.FadeIn();
                    }
                    bLastMin = false;
                }
                break;
            }


        }


        private void AVPlayer_DragEnter(object sender, DragEventArgs e) {
            if (myFileListForm == null) {
                myFileListForm = new FileListForm(this);
                myFileListForm.Owner = this; // 設定 Owner 會導致 myFileListForm 和母視窗連動 (收到一樣的 Message Event)

            }

            myFileListForm.FileDragEnter(sender, e);
        }

        private void AVPlayer_DragDrop_1(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            OpenFile(files);
            AutoShowTheChild();
        }


        // 效果: 顯示一下,之後又消失 
        public void AutoShowTheChild() {
            if (this.myFileListForm != null)
                myFileListForm.FadeIn();
            if (this.MyIndexList != null)
                MyIndexList.FadeIn();
            ChildAutoHind(this);
        }

        private void AVPlayer_KeyDown(object sender, KeyEventArgs e) {

        }

        // 線上說明
        private void menuItem40_Click(object sender, EventArgs e) {
            string target = "http://mqjing.twbbs.org.tw/~ching/Course/JapaneseLanguageLearner/__page/Usage.htm";
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }

        // EZLearn 開發資訊
        private void menuItem42_Click(object sender, EventArgs e) {
            string target = "http://ezlearnhelper.blogspot.com/";
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }

        // 朗文英英字典輔助工具
        private void menuItem43_Click(object sender, EventArgs e) {
            string target = "http://mqjing.twbbs.org.tw/~ching/Course/Longman%20Dictionary%20Helper/page/LongmanDictionaryHelper.htm";
            System.Diagnostics.Process.Start(Utility.getDefaultBrowser(), target);
        }

        private void menuItem45_Click(object sender, EventArgs e) {
            L_Manager.strDefaultLanguage = "T_Chinese";
            ResetLanguage();
        }

        private void menuItem47_Click(object sender, EventArgs e) {
            L_Manager.strDefaultLanguage = "S_Chinese";
            ResetLanguage();
        }

        private void menuItem46_Click(object sender, EventArgs e) {
            L_Manager.strDefaultLanguage = "English";
            ResetLanguage();
        }

        // Audio Recorder function
        private void menuItem50_Click(object sender, EventArgs e) {
            string strAudioRecorderUtilityFullFilename = ExecutationFileDir + @"\AudioRecorder.exe";
            string strWaveDestFilterDir = ExecutationFileDir;
            System.Diagnostics.Process.Start(strAudioRecorderUtilityFullFilename, strWaveDestFilterDir);
        }



    }

    class KKTimer : System.Windows.Forms.Timer {
        public AVPlayer myPlayer;
        public KKTimer(AVPlayer showPannel) {
            myPlayer = showPannel;
        }
    }
}

