using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Player
{
    public partial class FileListForm : Form
    {
        // [標準操作]
        // 目前播放的檔案位置 
        private int curFileIndex=-1;
        private bool bFileLoopPlay = false;
        string ezuDir = null; // Ezu 路徑紀錄 (用於組合出相對路徑的語音檔案) Method :PlayCurrentFile

        public bool bMyFileLoopPlay
        {
            set
            {
                bFileLoopPlay = value;
            }
            get
            {
                return bFileLoopPlay;
            }
        }

        // 系統管理物件
        AVPlayer myAVPlayer;
        public bool bClosed = false;  // 目前這個視窗是否已經關閉 (我們自己處理 WM_CLOSE 事件)
        public DockUtility myDockHelp = new DockUtility(30); // 吸引視窗公用物件
        public System.Windows.Forms.Form MyMother = null; // 記錄要連接的對象
        public Point oldLocation;  // 儲存正常視窗時的位置. (母類別縮小回復正常時, 需要將目前視窗恢復正常位置)
        System.Collections.ArrayList FullFileNameList = new System.Collections.ArrayList();
        public bool bActived=false;
        ToolTip toolTip1=null;

        public FileListForm(AVPlayer aAVPlayer)
        {
            // 系統管理片段
            this.myAVPlayer = aAVPlayer;
            this.Opacity = 0.0; // 配合淡入淡出視覺效果, 透明程度交給 FadeIn 與 FadeOut 兩個 function 來控制
            this.ShowInTaskbar = false; // 因為是子視窗, 所以不在 Task Bar 中顯示
            //this.Owner = aAVPlayer;

            InitializeComponent();

          
           

            // 拖拉功能啟動
            this.listBox_FileList.AllowDrop = true;
            listBox_oldColor = this.listBox_FileList.BackColor;


            MyMother = this.myAVPlayer;


            // ToolTips 
            // 設定 數字輸入區的 ToolTips	
            toolTip1 = new ToolTip();
            toolTip1.UseAnimation = true;
            toolTip1.AutoPopDelay = 10000;// Set up the delays for the ToolTip.
            toolTip1.InitialDelay = 10;
            toolTip1.ReshowDelay = 500;

            toolTip1.UseFading = true;
            toolTip1.ShowAlways = true;


            // 語言選項
            FileListFormLanguage();

           
        }

        // 語言選項
        public void FileListFormLanguage()
        {
            this.Text = myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_Title");
            this.checkBox_Loop.Text = myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_FileLoop"); // this.checkBox_Loop.Text = "檔案循環";
            this.checkBox1.Text = myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_SectionLoop");//  this.checkBox1.Text = "片段播放"; 


            // Tool Tips
            string[] FileListToolTipString ={
                 this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_Tool_OpenFileList"),
                 this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_Tool_SaveFileList"),
                 this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_Tool_ClearFileList")
            };

            toolTip1.SetToolTip(this.Button_Load, FileListToolTipString[0]);// "開啟檔案列");
            toolTip1.SetToolTip(this.Button_Save, FileListToolTipString[1]);//"儲存目前檔案串列");
            toolTip1.SetToolTip(this.button1, FileListToolTipString[2]);//"清除列表");
        }

        bool bFirst = true;

        // [標準操作] 移除所有檔案
        public void Clear_Files()
        {
            listBox_FileList.Items.Clear(); // 清除顯示部分
            FullFileNameList.Clear(); // 清除資料本體部分
        }

        // [標準操作] 
        // 加入多檔 (可能是目錄, ezu, 或單一語音檔)
        public void Add_Files(string[] files)
        {
            foreach (string file in files)
            {
                AddFile_Main(file);
            }
        }

        // 加入一個檔案或者是一個目錄
        public void AddFile_Main(string curFilename)
        {
            // 若 filename 是檔案
            if (System.IO.Directory.Exists(curFilename) == false)
            {
                // 檢查是否為合法的聲音檔名
                if (this.bAllow_AudioFile(curFilename) == true)
                {
                    // 加入單一檔案
                    Add_Files(curFilename); // 聽力轟炸面版, 加入單一語音檔 或 ezu 檔案
                }
            }
            else
            {
                // 若是目錄, 則開啟目錄中的每一個檔案加入
                string[] FileInDir = System.IO.Directory.GetFileSystemEntries(curFilename);
                for (int i = 0; i < FileInDir.Length; i++)
                {
                    AddFile_Main(FileInDir[i]); // 循環呼叫自己
                }
            }
        }

        // 加入 單一語音檔 或 ezu 檔案
        public void Add_Files(string file)
        {
            string curShortFilename = getShortFilename(file);// 顯示簡短檔名

            // 若檔案為 ezu 檔, 則要展開
            if(curShortFilename.EndsWith("ezu")){
                Load_EZU_File(file); // 交給讀取 EZU 檔案函式, 進行處理
            }else{
                // 標準載入語音檔程序
                listBox_FileList.Items.Add(curShortFilename); // 顯示簡短檔名
                FullFileNameList.Add(file);// 儲存完整檔名資料
            }

            
        }

        // 取得短檔名
        public string getShortFilename(string fullfilename)
        {
            // 顯示簡短檔名
            string curFilename = fullfilename;
            int startindex = curFilename.LastIndexOf("\\");
            string curShortFilename = curFilename.Substring(startindex + 1);
            return curShortFilename;
        }

        // 取得目錄
        public string getDirFilename(string fullfilename)
        {
            // 取得目錄
            string curFilename = fullfilename;
            int endindex = curFilename.LastIndexOf("\\");
            int length = endindex + 1;
            string curDir = curFilename.Substring(0, length);
            return curDir;
        }


        // 下面是系統管理片段
        public bool GetMyTopLevel()
        {
            return this.GetTopLevel();
        }
        private void FileListForm_Load(object sender, EventArgs e)
        {
            
            // 系統管理片段

            // 讀取母視窗的 Z-Order, 決定自己的 Z-Order
            this.TopMost = this.myAVPlayer.MyTopMost;
           //this.BringToFront();
            // 隨時記錄目前位置, 
            if (WindowState == FormWindowState.Normal)
                oldLocation = this.Location;

            if (bFirst == true)
            {
                // 移動目前位置到母視窗的左邊, 會自動呼叫 WndProc 
                // 會起動黏貼功能.
                // DockUtility.MoveToMotherLeftMiddle(this, myAVPlayer);
                // DockUtility.MoveToMotherRightMiddle(this, this.MyMother);
                SetDefaultLocation();
                bFirst = false;
            }

            // end of 系統管理片段



        }

        public void SetDefaultLocation()
        {
            // 移動目前位置到母視窗的左邊, 會自動呼叫 WndProc 
            // 會起動黏貼功能.
            DockUtility.MoveToMotherRightMiddle(this, this.MyMother);
        }
        // 當視窗移動時, 隨時記錄目前位置 (因為縮小時, 會需要這個位置)
        private void FileListForm_Move(object sender, EventArgs e)
        {
            if (Location.X >= 0 && Location.Y >= 0)
                oldLocation = this.Location;
        }
        

        // 系統管理物件:  吸引視窗公用物件 -- DockUtility
        protected override void WndProc(ref Message m)
        {
            // 這個視窗要處理自己的 Close 事件 
            const int WM_CLOSE = 0x0010;// 這個常數是在 winuser.h 找到的 or http://www.mazama.net/scheme/v12/w32message.scm
           // const int WM_SIZING = 0x0214;

           // const int SIZE_MINIMIZED = 1; // 這個參數在 winuser.h or http://doc.ddart.net/msdn/header/include/winuser.rh.html
            if (m.Msg == WM_CLOSE)
            {
                bClosed = true;
                this.Opacity = 0.0;
                return;
            }
            // end of Close 事件處理
            /*
            // 自己處理 Minimum 事件
            if (m.Msg == WM_SIZING)
            {
               // if (m.WParam.ToInt32() == SIZE_MINIMIZED)
                {
                    this.Opacity = 0.0;
                    this.myAVPlayer.ConnectedWindow--;
                    return;
                }

            }
            */
            
                // 動態決定 Mother 是誰
                // 當目前還沒有連接時, 才作動態的決定
                // 原理: 在 WinProc 隨時偵測與哪一個視窗最接近, 接近的設定為 Mother
                // 其他視窗: 移動時, 隨時檢測自己是否為別人的 Mother
                //     是: 則順便移動 child
                //     否: 不動作
                if (this.Bounds.Y >= 0 || Bounds.X >= 0) // 因為有可能視窗目前是最小化, y=-65535 與 x=-65535, 這種狀況必須避開
                {
                    // bClosed = false;

                   
                    if (this.myAVPlayer.MyIndexList != null && myDockHelp.isConnected == false)
                    {
                        int d1 = getDistance(this.Bounds, this.myAVPlayer.Bounds);
                        int d2 = getDistance(this.Bounds, this.myAVPlayer.MyIndexList.Bounds);
                        if (d1 < d2)
                        {
                            if(myAVPlayer.Opacity!=0) //母視窗不能看不見, 我們不能黏到一個看不見的視窗
                                MyMother = myAVPlayer;
                        }
                        else
                        {
                            if (this.myAVPlayer.MyIndexList.Opacity != 0)//母視窗不能看不見, 我們不能黏到一個看不見的視窗
                                MyMother = this.myAVPlayer.MyIndexList;
                        }
                    }

                    
                }

                System.Drawing.Rectangle SelfBound = this.Bounds; // 目前視窗的位置
                System.Drawing.Rectangle MotherBound = this.MyMother.Bounds; // 連結母視窗的位置

                
                int[] retNewLoc = new int[2];  // 回傳的新位置
                myDockHelp.WndProc2(ref m, SelfBound, this.MyMother, retNewLoc, this);
            
            base.WndProc(ref m);
        }

        public void getCenter(System.Drawing.Rectangle r,int[] cyx)
        {
            cyx[0]=r.Top+(r.Bottom -r.Top )/2;
            cyx[1]=r.Left +(r.Right -r.Left)/2;
        }
        public int getDistance(System.Drawing.Rectangle r1, System.Drawing.Rectangle r2)
        {
            int[] r1cyx = new int[2];
            getCenter(r1, r1cyx);

            int[] r2cyx= new int[2];
            getCenter(r2, r2cyx);

            int dx = (r1cyx[1] - r2cyx[1]) * (r1cyx[1] - r2cyx[1]);
            int dy = (r1cyx[0] - r2cyx[0]) * (r1cyx[0] - r2cyx[0]);
            return dx + dy;
        }

        // 系統管理函式: 視覺效果 -- 淡入 OpacityUtility
        public void FadeIn()
        {
            if (this.Opacity == 0)
            { // 當完全透明時,才動作
                OpacityUtility myOpacityObj = new OpacityUtility();
                myOpacityObj.FadeIn_Only(this, 20, 1.0);// 漸漸顯示出來

            }
        }
        // 系統管理函式: 視覺效果 -- 淡出 OpacityUtility
        public void FadeOut()
        {
            FadeOut(30);
        }

        public void FadeOut(int Interval)
        {
            if (this.Opacity == 1)
            { // 當完全顯示時, 才動作
                OpacityUtility.FadeOut_Only(this, Interval); // 漸漸變透明
            }
        }

        private void FileListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Opacity = 0;
        }

        // 是否為合法的聲音檔名
        public bool bAllow_AudioFile(string file)
        {
            file=file.ToLower();
            int start = file.LastIndexOf(".");
            string suffix = file.Substring(start, file.Length - start);
            if (AVPlayer.EZlearnCanPlaySuffix.IndexOf(suffix) == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool FileDragEnter(object sender, DragEventArgs e)
        {
            // 確定使用者抓進來的是檔案
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                // 檢查使用者抓來的檔案是可以播放的檔案
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i].ToLower();

                    // 若拉近來的為檔案, 則要檢查是否符合接受的聲音檔
                    // 若是目錄, 則等到 Drop 時, 要展開
                    if (System.IO.Directory.Exists(file) == false)
                    {
                        if (bAllow_AudioFile(file) == false)
                        {
                            return false;
                        }

                    }
                }

                // 允許拖拉動作繼續 (這時滑鼠游標應該會顯示 +)
                e.Effect = DragDropEffects.All;
                return true;
            }
            return false;
        }

        // 系統管理函式: 拖拉效果 -- DragEnter
        Color listBox_oldColor;
        private void listBox_FileList_DragEnter(object sender, DragEventArgs e)
        {
            if (FileDragEnter(sender, e))
            {
                // 改變顏色提醒使用者
                listBox_oldColor = this.listBox_FileList.BackColor;
                this.listBox_FileList.BackColor = Color.LightBlue;
            }
 
        }

        // 系統管理函式: 拖拉效果 -- DragLeave
        private void listBox_FileList_DragLeave(object sender, EventArgs e)
        {
            this.listBox_FileList.BackColor = listBox_oldColor;
        }

        // 系統管理函式: 拖拉效果 -- 讀取檔案名稱
        private void listBox_FileList_DragDrop(object sender, DragEventArgs e)
        {
            // 把顏色改回來
            this.listBox_FileList.BackColor = listBox_oldColor;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // 之前若沒有任何檔案, 則立即播放剛剛拉進來的檔案
            bool bPlayImmediatly = false;
            if (FullFileNameList.Count == 0)
            {
                bPlayImmediatly = true;
            }
            
            Add_Files(files);// 累加所選的檔案

            if (bPlayImmediatly)
            {
                // this.curFileIndex = 0; // 剛拉近來, 播放指標歸零
                // listBox_FileList.SelectedIndex = 0;
                // 播放第一個媒體檔案
                PlayTheFirstFile();
            }

        }

        public bool PlayTheFirstFile()
        {
            
            this.curFileIndex = 0; // 播放指標歸零
            //listBox_FileList.SelectedIndex = 0; // 顯示指標歸零
            return PlayCurrentFile();
        }

        // 播放語音核心函式
        // 參數: bFirstSection 
        //       true = 是否由第一個語音片段開始播放
        //       false= 由最後一個語音片段開始播放
        public bool bChangeFromOtherForm = false; // 標示這個改變來自其他地方
        public bool PlayCurrentFile(bool bFristSection)
        {
            if (listBox_FileList.Items.Count > 0)
            {

                string filename = (string)FullFileNameList[curFileIndex];

                // 若為相對路徑檔案, 則使用 ezu 目前 Dir 取得完整檔案名稱
                string fullfileanme;
                string curDir=this.getDirFilename(filename);
                if (curDir.Equals("") || ezuDir==null)
                    fullfileanme = ezuDir + filename;
                else
                    fullfileanme = filename;

                myAVPlayer.Play_a_File(fullfileanme, bFristSection);

                // 若來自其他的控制, 則要自己處理 index Selected 
                if (bChangeFromOtherForm)
                {
                    this.listBox_FileList.SelectedIndex = curFileIndex;
                    bChangeFromOtherForm = false;
                }
                return true;
            }
            else
                return false;
        }

        public bool PlayCurrentFile()
        {
            bool bFristSection = true;
            return PlayCurrentFile(bFristSection);
        }


        bool bJustShowSelectIndex = false; // 控制 SelectChange 程式動作變數
        // 顯示 IndexList 的指標, 不啟動任何 Select Change 程式動作
        public void MoveDownTheSelectedIndex_NoAction()
        {
            // 檢查是否可以 Move Down
            // int lastIndex=listBox_FileList.SelectedItems.Count-1;
            int lastIndex = listBox_FileList.Items.Count - 1; // 最後一個 file 索引位置
            if (listBox_FileList.SelectedIndex + 1 > lastIndex)
                return;

            bJustShowSelectIndex = true; //只是顯示指標, 不啟動任何 Select Change 程式動作
            listBox_FileList.SelectedIndex++; // 調整顯示指標
        }

        // 顯示 IndexList 的指標, 不啟動任何 Select Change 程式動作
        public void MoveUpTheSelectedIndex_NoAction()
        {
            // 檢查是否可以往上
            int FirstIndex = 0;
            if (listBox_FileList.SelectedIndex - 1 < 0)
            {
                return;
            }

            bJustShowSelectIndex = true; //只是顯示指標, 不啟動任何 Select Change 程式動作
            listBox_FileList.SelectedIndex--; // 調整顯示指標
        }

        public bool PlayTheNextFile()
        {

            // 若目前為處於語音檔重複播放狀態, 則播放第一個檔案 (循環播放)
            if (bMyFileLoopPlay)
            {
                // this.PlayTheFirstFile();
                this.PlayCurrentFile();   // 目前設定為 指定循環播放同一個檔案
                this.myAVPlayer.ShowState(this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_FileRepeatMessage")); // this.myAVPlayer.ShowState("檔案循環播放");
                return true; // 傳回成功處理
            }

            // 若下面還有檔案, 則開啟下一個檔案
            if (curFileIndex != listBox_FileList.Items.Count - 1)
            {

                // listBox_FileList.SelectedIndex++; // 調整顯示指標 (不要自己控制 SelectedIndex) 更正理由: 若不調整, FileList 指標不會顯示
                MoveDownTheSelectedIndex_NoAction();
                curFileIndex++;
                PlayCurrentFile();// 播放目前指標的檔案
                return true;
            }


            // 回傳沒有下一個檔案
            return false;
        }

        // 播放前一個語音檔
        public bool PlayThePreviouseFile()
        {
            // 若現在是第一個檔案, 則傳回 false
            if (curFileIndex <= 0)
                return false;
            else
            {
                // 否則直接播放目前的檔案
                // listBox_FileList.SelectedIndex--;  // 不要自己控制 SelectedIndex
                MoveUpTheSelectedIndex_NoAction(); // 顯示指標, 不動作
                curFileIndex--;

                bool bFristSection = false;
                PlayCurrentFile(bFristSection); // 由上一個片段開始播放
                return true;
            }
        }
        bool bDelete = false;
        private void listBox_FileList_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Up:
                    PlayThePreviouseFile();
                    //string filename = (string)FullFileNameList[listBox_FileList.SelectedIndex];
                    //myAVPlayer.Play_a_File(filename);

                    break;
                case Keys.Down:
                    PlayTheNextFile();
                    //string filename2 = (string)FullFileNameList[listBox_FileList.SelectedIndex];
                    //myAVPlayer.Play_a_File(filename2);
                    break;
                case Keys.Delete:

                    int index = listBox_FileList.SelectedIndex;
                    if (index < 0)
                        return;

                    // 立即刪除資料
                    FullFileNameList.RemoveAt(index); // 串列的部分
                    this.listBox_FileList.Items.RemoveAt(index);  // 顯示的部分

                    // 調整下一個索引
                    int newIndex = index - 1;
                    if (newIndex < 0 && this.listBox_FileList.Items.Count > 0)
                        newIndex = 0;
                    this.curFileIndex = newIndex;

                    this.listBox_FileList.SelectedIndex = curFileIndex; // 選擇新的索引

                    //  立即播放指定位置的資料
                    if (FullFileNameList.Count != 0)
                    {
                        this.PlayCurrentFile();
                    }
                    else
                    {
                        myAVPlayer.AllStop(); // 若沒有檔案可以播放, 則停止撥放
                    }

                    /*
                    int i = listBox_FileList.SelectedIndex;
                    bDelete = true;

                  

                    
                    
                    // 設定刪除後播放的檔案名稱索引
                    if (i == FullFileNameList.Count - 1)
                        this.curFileIndex = i - 1;  // 若目前正在播放的是最後一個檔案, 則現在改成上一個
                    else
                    {
                        this.curFileIndex = i;
                    }

                    listBox_FileList.Items.RemoveAt(i);
                    this.FullFileNameList.RemoveAt(i);

                   
                    

                    if (FullFileNameList.Count == 0)
                    {
                        this.myAVPlayer.AllStop(); // 若沒有檔案可以播放, 則停止撥放
                    }
                    else
                    {
                        listBox_FileList.SelectedIndex = curFileIndex; // 取得前一個可播放的檔案
                        this.PlayCurrentFile();
                    }
                     */
                    break;
            }


        }


        // 播放索引選擇的檔案
        public void PlaySelectedIndex(int index)
        {
            if (index >= 0 && index < listBox_FileList.Items.Count)
            {
                // listBox_FileList.SelectedIndex = index; // 我們自己控制播放程序, 不要交給事件. 可以使問題簡化
                curFileIndex = index;
                this.PlayCurrentFile();
            }
        }
        private void listBox_FileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 目前的改變只是為了顯示, 所以不執行下面的動作
            if (bJustShowSelectIndex == true)
            {
                bJustShowSelectIndex = false;
                return;
            }

            if (!bDelete)
            {
                // 若 Selected Index 的改變是來自使用者點選滑鼠
                if (this.bFromMosueDown)
                {
                    curFileIndex = listBox_FileList.SelectedIndex;
                    bFromMosueDown = false;
                    PlayCurrentFile();
                }
                else
                {
                    // 不明原因造成 SelectedValueChanged 誤動作, 在這裡要強制同步
                    if (curFileIndex < this.listBox_FileList.Items.Count)
                        listBox_FileList.SelectedIndex = curFileIndex;
                }
                
            }
            else
            {
                bDelete = false;
                
            }
            
        }

        // 載入檔案列表設定檔: *.ezu
        private void Button_Load_Click(object sender, EventArgs e)
        {
            
            if(Load_EZU_File())
                PlayTheFirstFile(); // 立即播放第一個檔案
        }

        // 開啟 ezu 檔案
        public bool Load_EZU_File()
        {
            // 詢問存檔名稱
            OpenFileDialog ofdOpen = new OpenFileDialog();
            ofdOpen.Filter = "EZU Files (*.ezu)|*.ezu"; // 指定 EZLearn 專用多檔描述附檔名

            ofdOpen.Title = this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_OpenEZUFile"); // ofdOpen.Title = "請指定檔案名稱";
            if (ofdOpen.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
            {
                // 使用者沒有選檔案
                return false;
            }
            string LoadFilename = ofdOpen.FileName; // 選擇的完整路徑
            this.Clear_Files(); // 先清掉原先的資料
            Load_EZU_File(LoadFilename);
            return true;
        }
        public void Load_EZU_File(string LoadFilename)
        {
            try
            {
                // 建立 XML 讀取器
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments=true; // 不處理註解
                settings.IgnoreWhitespace=true; // 跳過空白
                settings.ValidationType=ValidationType.None; // 不驗證任何資料
                XmlReader reader = XmlTextReader.Create(LoadFilename,settings);

               

                // 進入讀取主要部分
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            string LocalName = reader.LocalName; // 取得標籤名稱
                            if (LocalName.Equals("FileInfo"))
                            {
                                string Name = reader["Name"];
                                string Address = reader["Address"]; // 讀取 Full Filename Name
                                this.Add_Files(Address);
                            }
                            
                            break;
                    }
                }
                reader.Close();

                ezuDir = getDirFilename(LoadFilename); // 紀錄 ezu 路徑, 方便之後開啟相對路徑時使用
                
            }
            catch (System.Xml.XmlException ee)
            {
                // System.Windows.Forms.MessageBox.Show("XML 語法檢查錯誤");
                System.Windows.Forms.MessageBox.Show(this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_XMLError"));
                Console.WriteLine("Exception " + ee);
            }
        }

        // 存成 ezu 檔案
        public void Save_As_EZU_File()
        {
            // 詢問使用者存檔名稱
            SaveFileDialog ofdOpen=new SaveFileDialog();
            ofdOpen.Filter = "EZU Files (*.ezu)|*.ezu"; // 指定 EZLearn 專用多檔描述附檔名

            ofdOpen.Title = this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_SaveEZUFile"); //  ofdOpen.Title = "請設定檔案名稱";
            if (ofdOpen.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
            {
                // 使用者沒有選檔案
                return;
            }
            string SaveFilename = ofdOpen.FileName; // 選擇的完整路徑

            // 取得使用者指定的目錄
            string TargetDir = getDirFilename(SaveFilename);


            // string SaveFilename = "c:\\test" + ".ezu";
            using (System.IO.StreamWriter writer = System.IO.File.CreateText(SaveFilename))
            {
                string[] xml_comment ={"<!-- 聽力轟炸: 多檔描述設定檔",
									 " ",
									 "    功能: 紀錄檔案列表名稱",
									 "    格式範例 (Format):",
									 "           <FullFileName=\"c:\test\" />",
									 " ",
									 "    手動編輯工具: NotePad.exe 存檔格式: UTF-8",
                                     " ",
                                     " English:",
                                     " The Listening Bomb Describtion File",
                                     " ",
                                     " Functions: storing the filelist data",
                                     " Format:",
                                     "           <FullFileName=\"c:\test\" />",
                                     " ",
                                     " Manually Editing: You can use NotePad.exe to modify the content. Remember use UTF-8 format to save it",
									 "-->"};
                string xml_head = "<EZLearn_MultiFile_List Ver=\"#20070417\" >";
                string xml_tail = "</EZLearn_MultiFile_List>";

                // 寫入註解
                for (int i = 0; i < xml_comment.Length; i++)
                {
                    writer.WriteLine(xml_comment[i]);
                }

                // 寫入資料本體
                writer.WriteLine(xml_head);
                for (int i = 0; i < FullFileNameList.Count; i++)
                {
                    string curFullFilename = (string)FullFileNameList[i];
                    string shortFilename=getShortFilename(curFullFilename);
                    string fileDir = getDirFilename(curFullFilename);

                    // 若目標存檔路徑和語音檔相同, 則存相對路徑: shortFilename
                    // 否則存絕對路徑
                    string saveFilename;
                    if (fileDir.Equals(TargetDir))
                        saveFilename = shortFilename;
                    else
                        saveFilename = curFullFilename;
                    string xml_Format = "<FileInfo Name=" + "\"" + shortFilename + "\"" + " Address=" + "\"" + saveFilename + "\"" + "/>";
                    writer.WriteLine(xml_Format);
                }
                writer.WriteLine(xml_tail);
            }

            // MessageBox.Show("多檔資訊存檔成功","聽力轟炸面版資訊");
            MessageBox.Show(this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_SaveOK"), this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_Title"));
            
        }
        private void Button_Save_Click(object sender, EventArgs e)
        {
            Save_As_EZU_File();
        }

        private void FileListForm_Actived(object sender, EventArgs e)
        {
            this.myAVPlayer.iStat++;

            // 讀取母視窗的 Z-Order, 決定自己的 Z-Order
            this.TopMost = this.myAVPlayer.MyTopMost;
            //this.BringToFront();
            // 隨時記錄目前位置, 
            if (WindowState == FormWindowState.Normal)
                oldLocation = this.Location;
        }

        private void FileListForm_Deactivate(object sender, EventArgs e)
        {
            
        }

        private void listBox_FileList_KeyPress(object sender, KeyPressEventArgs e)
        {
          
        }

        bool bFromMosueDown = false;
        private void listBox_FileList_MouseDown(object sender, MouseEventArgs e)
        {
            

            bFromMosueDown = true;
        }

        private void listBox_FileList_MouseClick(object sender, MouseEventArgs e)
        {/*
            switch (e.Button)
            {
                case MouseButtons.Left:
                    //bMouseLDown = true;
                    break;

            }
          */
        }

        private void listBox_FileList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //bMouseLDown = false;

            //this.PlaySelectedIndex(listBox_FileList.SelectedIndex);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           
            
        }
        public void UpdateSectionBasedPlayStatus(bool Enable)
        {
            this.checkBox1.Checked = Enable; // 顯示目前是 Section Based 播放
        }
        private void checkBox1_Click(object sender, EventArgs e)
        {
            this.myAVPlayer.bMySectionBasedPlay = this.checkBox1.Checked; // 啟動語音片段為基礎的播放

            // 若使用者立即指定要片段為基礎播放, 則立即播放第一段語音
            if (this.myAVPlayer.bMySectionBasedPlay)
            {
                this.myAVPlayer.MyIndexList.PlayTheFirstSection();
            }
        }

        private void checkBox_Loop_MouseClick(object sender, MouseEventArgs e)
        {
            bMyFileLoopPlay = this.checkBox_Loop.Checked;

            
        }

        private void checkBox_Loop_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_Loop.Checked)
            {
                // this.myAVPlayer.ShowState("檔案循環 ON");
                this.myAVPlayer.ShowState(this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_FileLoopOnMessage"));
            }
            else
            {
                // this.myAVPlayer.ShowState("檔案循環 OFF");
                this.myAVPlayer.ShowState(this.myAVPlayer.L_Manager.getMessage("LesteningBombWindow_System_FileLoopOffMessage"));
            }
        }

        // 清除所有的列表
        private void button1_Click(object sender, EventArgs e)
        {
            this.listBox_FileList.Items.Clear(); // 清除顯示
            this.FullFileNameList.Clear(); // 清除資料本體
            this.myAVPlayer.AllStop(); // 停止播放
        }

        public void MyMouseIn()
        {
            this.Opacity = 1.0;
        }
        public void MyMouseLeave()
        {
            this.Opacity = 0.7;
        }
        OpacityUtility myOpacityObj = new OpacityUtility();
        private void FileListForm_MouseEnter(object sender, EventArgs e)
        {
            //MyMouseIn();
            //myOpacityObj.FadeIn_Only(this, 20);// 漸漸顯示出來
        }

        private void FileListForm_MouseLeave(object sender, EventArgs e)
        {
           // MyMouseLeave();
            //OpacityUtility.FadeOut_Only(this, 20,0.2f);
        }

        private void listBox_FileList_MouseEnter(object sender, EventArgs e)
        {
            //MyMouseIn();
        }

        private void listBox_FileList_MouseLeave(object sender, EventArgs e)
        {
            //MyMouseLeave();
        }


    }
}