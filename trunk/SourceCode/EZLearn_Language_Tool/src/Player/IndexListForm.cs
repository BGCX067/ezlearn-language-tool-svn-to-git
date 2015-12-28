using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

// 使用 DockUtility 吸引視窗公用物件 -- 請直接參考 DockUtility.cs 


namespace Player {
	/// <summary>
	/// Summary description for IndexListForm.
	/// </summary>
	public class IndexListForm : System.Windows.Forms.Form {
		
		// 語言處理器相關
		public System.Collections.ArrayList LastTextList=new  System.Collections.ArrayList(); // 使用者輸入索引資料串
		private int curTextIndex=-1; // 目前索引位置

        public Point oldLocation; // 目前視窗位置 (處理母視窗放大縮小時, 子視窗的對應淡入淡出)

		public AVPlayer myAVPlayer=null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;

		private bool bFirst=true;
		private string Filename;
		string curAudioFileName;
		string strLoadDir,curAudioFullFileName,strFullLoadFileName;
		string strSaveDir,strFullSaveFileName;

		public DockUtility myDockHelp=new DockUtility(30); // 吸引視窗公用物件
		
		static int CommandsTotalNum=4;
		string[] strSeekTime_End_Repeat_Comment=new string[CommandsTotalNum]; // 儲存目前指令列的資訊
		public int RepeatCount=0;
		double curSeekTime=0;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.CheckBox checkBox1_SectionAutioRepeat;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button7;
		public System.Windows.Forms.CheckBox cbAllRepeat;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		double curEndTime=0;

        public void UpdateFileData(string Filename)
        {

            curAudioFullFileName = Filename;
            curAudioFileName = curAudioFullFileName.Substring(curAudioFullFileName.LastIndexOf("\\") + 1);

            // 設定語音索引檔的存檔位置
            // 先檢查語音檔位置是否可以存檔, 若不行的話才設定存檔位置為專案目錄
            string curAudioFileDir = curAudioFullFileName.Substring(0, curAudioFullFileName.Length - curAudioFileName.Length - 1);

            //System.IO.Directory curAudioFileDirObj = System.IO.Directory.GetParent(curAudioFullFileName);



            strLoadDir = this.myAVPlayer.ExecutationFileDir;

            strLoadDir = curAudioFileDir;
            strFullLoadFileName = strLoadDir + "\\" + curAudioFileName;

            strSaveDir = strLoadDir;
            strFullSaveFileName = strSaveDir + "\\" + curAudioFileName + "_EZ.txt";


            this.Text = curAudioFileName + ".txt";
            Filename = this.Text;
        }

        // 語言相關
        public void IndexListFormLanguage(){
            this.label1.Text = this.myAVPlayer.L_Manager.getMessage("IndexListForm_System_Start"); // this.label1.Text = "起始
            this.label2.Text = this.myAVPlayer.L_Manager.getMessage("IndexListForm_System_End"); // this.label1.Text = "結束:";

            // ToolTips

            string[] IndexListFormToolTipsString ={
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_ModifySection"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_SectionAutoRepeat"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_RepeatPlay"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_Delete"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_Sorting"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_DeleteAllSection"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_OpenLesteningBombWindow"),
                myAVPlayer.L_Manager.getMessage("IndexListForm_Tool_AddnewSection")
            };

            toolTip1.SetToolTip(this.textBox1, IndexListFormToolTipsString[0]);// "修改語音片段 ");
            toolTip1.SetToolTip(this.checkBox1_SectionAutioRepeat, IndexListFormToolTipsString[1]);// "語音片段自動循環 (從第一段開始播放)");
            toolTip1.SetToolTip(this.cbAllRepeat, IndexListFormToolTipsString[2]);// "無限重複播放");
            toolTip1.SetToolTip(this.button6, IndexListFormToolTipsString[3]);//"刪除->目前的片段 [Del]");
            toolTip1.SetToolTip(this.button5, IndexListFormToolTipsString[4]);//"排序->語音片段(由小到大)");
            toolTip1.SetToolTip(this.button3, IndexListFormToolTipsString[5]);//"刪除所有的片段");
            toolTip1.SetToolTip(this.button2, IndexListFormToolTipsString[6]);//"開啟聽力轟炸面版");
            toolTip1.SetToolTip(this.button7, IndexListFormToolTipsString[7]);//"新增->目前的語音片段");

        }
        ToolTip toolTip1 = null;
		public IndexListForm(AVPlayer myAVPlayer,string filename) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            // 專案相關
            this.myAVPlayer = myAVPlayer;



            // 更新播放音樂檔時, 必須一併更新
            // this.myAVPlayer.ofdOpen.FileName
            UpdateFileData(filename);


			// 加入 ToolTips
			toolTip1 = new ToolTip();
			toolTip1.AutoPopDelay = 5000;// Set up the delays for the ToolTip.
			toolTip1.InitialDelay = 1000;
			toolTip1.ReshowDelay = 500;
			toolTip1.ShowAlways = true;// Force the ToolTip text to be displayed whether or not the form is active.
      
			// Set up the ToolTip text for the Button and Checkbox.



			// Repeat 相關
			string strSectionAutoRepeat=(string)this.myAVPlayer.PropertyTable["SectionAutoRepeat"]; // 語音片段自動播放 (最後一筆語音片段播放完畢,自動從第一筆開始播放)
			bool bstrSectionAutoRepeat=bool.Parse(strSectionAutoRepeat);
			this.checkBox1_SectionAutioRepeat.Checked=bstrSectionAutoRepeat;
			


            // 語言相關
            IndexListFormLanguage();
		}

        // 將 Section 指令清空
        // 因為聽力轟炸面版換檔播放時, 需要把前一個檔案的指令清空
        public void ResetSectionInfo()
        {
           // this.ClearList(); // 清除指令串列
            ClearCurSectionInfo(); // 清除目前指令 
           
        }

        // 清除目前指令 
        public void ClearCurSectionInfo()
        {  
            for (int i = 0; i < CommandsTotalNum; i++)
            {
                strSeekTime_End_Repeat_Comment[i] = null;
            }
        }
        
		public string[] getCurSectionInfo(){
			return strSeekTime_End_Repeat_Comment;
		}

		public bool hasEndInfo(){
			if(strSeekTime_End_Repeat_Comment[1]==null)
				return false;
			else{
				return true;
			}
		}

		public bool hasRepeatInfo(){
			if(strSeekTime_End_Repeat_Comment[2]==null)
				return false;
			else{
				return true;
			}
		}

		public bool hasCommentInfo(){
			if(strSeekTime_End_Repeat_Comment[3]==null)
				return false;
			else{
				return true;
			}
		}
		public string getCommentInfo(){
			return strSeekTime_End_Repeat_Comment[3];
		}

		// 取得目前系統索引狀態
		public double getCurSeekTime(){
			return curSeekTime;
		}

		public double getCurEndtime(){
			return curEndTime;
		}

		// 遞減 Repeat
		public int RepeatCountDown(){
			RepeatCount--;
			return RepeatCount;
		}


		public static bool CheckCommand(string SectionCommand){
			double[] retData_Seek_End_Repeat=new double[IndexListForm.CommandsTotalNum];
			string[] SectionInfo_Seek_End_Repeat=new string[IndexListForm.CommandsTotalNum];
			return CheckCommand(SectionCommand,retData_Seek_End_Repeat,SectionInfo_Seek_End_Repeat);
		}	

		public static bool CheckCommand(string SectionCommand,double[] retData_Seek_End_Repeat,string[] strSeekTime_End_Repeat_Comment){
			string[] SectionInfo_Seek_End_Repeat=strSeekTime_End_Repeat_Comment;
			Parse_getSeekTime_End_Repeat(SectionCommand,SectionInfo_Seek_End_Repeat);
			double dSeekTime=0,dEndTime=0,dRepeatCount=0;
			try{
				dSeekTime=double.Parse(SectionInfo_Seek_End_Repeat[0]);
				if(dSeekTime<0) // SeekTime 不能是負數
					return false;

				if(SectionInfo_Seek_End_Repeat[1]!=null){
					dEndTime=double.Parse(SectionInfo_Seek_End_Repeat[1]);
					if(dEndTime<=dSeekTime) // End Time 不能比 Start Seek time 還早
						return false;
				}

				if(SectionInfo_Seek_End_Repeat[2]!=null){
					dRepeatCount=int.Parse(SectionInfo_Seek_End_Repeat[2]);
					if(dRepeatCount<0){ 	// Repeat 不能是負數
						return false;
					}
				}
			}catch(Exception ){
				return false;
			}
			retData_Seek_End_Repeat[0]=dSeekTime;
			retData_Seek_End_Repeat[1]=dEndTime;
			retData_Seek_End_Repeat[2]=dRepeatCount; 

			return true;
		}
		
		public bool UpdateSeekTime_End_Repeat(string SectionCommand){
			curSeekTime=0;curEndTime=0;RepeatCount=0;

			double[] retData_Seek_End_Repeat=new double[IndexListForm.CommandsTotalNum];

			// 檢查 Command 是否正確
			bool bOk= CheckCommand(SectionCommand,retData_Seek_End_Repeat,strSeekTime_End_Repeat_Comment); 
		
			if(bOk){
				// Update 資料
				curSeekTime=retData_Seek_End_Repeat[0];
				curEndTime=retData_Seek_End_Repeat[1];
				RepeatCount=(int)retData_Seek_End_Repeat[2];
			}

			return bOk;


			/*

			Parse_getSeekTime_End_Repeat(SectionCommand,strSeekTime_End_Repeat_Comment);
			curSeekTime=double.Parse(strSeekTime_End_Repeat_Comment[0]);

			if(strSeekTime_End_Repeat_Comment[1]!=null){
				curEndTime=double.Parse(strSeekTime_End_Repeat_Comment[1]);
				// End Time 不能比 Start Seek time 還早
				if(curEndTime<=curSeekTime)
					return false;
			}

			if(strSeekTime_End_Repeat_Comment[2]!=null){
				RepeatCount=int.Parse(strSeekTime_End_Repeat_Comment[2]);
				// Repeat 不能是負數
				if(RepeatCount<0){
					return false;
				}
			}

			return true;
			*/
			
		}


		

		
	
		// 當使用者 用 tab 切換到 list 視窗時, 主視窗也要一併帶出來
		
		
		/*
				public void FirstRun(){
					this.Opacity=100; // 透明度 0;   (一開始是看不見的) 
					this.Show(); // 顯示視窗
				}

				public void InitialListBox(){

			
				}
				*/

		// 淡入
		public void  FadeIn(){
			
			//  如果在一開始程式建立時, 就設定透明度 =0 , 則將來就算是設定透明度 =100 , 其包含的元件的透明度將不會改回來 [Bug]?
			if(bFirst==true){  
				this.Opacity=0.0; // 透明度 0;   (一開始是看不見的) 
				
				this.Show(); // 顯示視窗 (一定要顯示視窗,才能改變位置)
				bFirst=false;

                /* menu 問題已經解決, 主要的原因在於: 更換 skin 時, 並沒有考慮同時重新設定 視窗的高度.  參考: AudioVideoPlayer:: LoadSkin method
                int MainMenuHeight=0;
                // 考慮到 menu 的高度
                if(myAVPlayer.Menu==null){
                    MainMenuHeight=32;
                }else{
                    MainMenuHeight=0;
                }
                // end of menu 的高度

                // this.Location=new System.Drawing.Point(myAVPlayer.Left,myAVPlayer.Top+myAVPlayer.Height-MainMenuHeight);
                 */
                OpacityUtility myOpacityObj = new OpacityUtility();
                myOpacityObj.FadeIn_Only(this, 20);// 漸漸顯示出來
				this.listBox1.Focus();
			}else{
                if (this.Opacity == 0)
                { // 當完全透明時,才動作
                    OpacityUtility myOpacityObj = new OpacityUtility();
                    myOpacityObj.FadeIn_Only(this, 20);// 漸漸顯示出來
                }
			}
		}

		// 淡出
        public void FadeOut(int Interval)
        {
			if(this.Opacity==1) // 當完全顯示時, 才動作
                OpacityUtility.FadeOut_Only(this, Interval); // 漸漸變透明
		}
        public void FadeOut()
        {
            FadeOut(30);
        }
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			
			LastTextList=null;

			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );

		}

		// 加入新的 item 到索引串列中
		public bool bModify=false;
		public bool AddItem(String strIndex){
			if(checkExist(strIndex)!=true){
				// 把最新的指定秒數記錄起來
				LastTextList.Add(strIndex);  // 儲存 reference 還是 新的實體 ?
				curTextIndex=LastTextList.Count-1;  // 設定為最後一筆資料的 index, 按上鍵會索引上一個輸入

				this.listBox1.Items.Add(strIndex); // 設定 List 顯示
				this.listBox1.SelectedIndex=curTextIndex;
				
				if(this.Text.EndsWith("*")!=true){
					this.Text=this.Text+"*";
				}
				bModify=true;
				this.button1.Visible=true;

				return true;  // 加入 item 成功
			}
			return false; // 串列中已經存在 item, 回傳失敗
		}// end of 加入新的 item

		

		// 編輯 item
		public bool EditItem(String strIndex){
			// 檢查  Command 是否合格
			bool bOk=CheckCommand(strIndex);
			if(bOk){
				LastTextList[this.listBox1.SelectedIndex]=this.textBox1.Text;// 變更資料主體
				this.listBox1.Items[this.listBox1.SelectedIndex]=this.textBox1.Text; // 變更顯示
			
				SetModify();
				return true;  // 加入 item 成功		
			}else{
				return false;
			}
		}// end of 編輯 item

		public void SetModify(){
			if(this.Text.EndsWith("*")!=true){
				this.Text=this.Text+"*";
			}
			bModify=true;
			this.button1.Visible=true;
		}

		//向上索引
		public void MoveUP(){
			curTextIndex--;
			if(curTextIndex<0){
				if(LastTextList.Count >0){
					curTextIndex=0;
					myAVPlayer.SeekPlay();
				}
				else
					curTextIndex=-1;
				myAVPlayer.label2.Text="資料最頂端";

                myAVPlayer.myFileListForm.PlayThePreviouseFile();
			}
			else{ 
				/*
				string strNum=(string) LastTextList[curTextIndex]; // 設定好要播放的指定秒數;
				myAVPlayer.textBox1.Text=strNum;
				myAVPlayer.SeekPlay();// 進行播放
				myAVPlayer.label3.Text="索引到 "+strNum;
				myAVPlayer.label2.Text="[Enter] 再聽一次";
				*/

				this.listBox1.SelectedIndex=curTextIndex; // 會引起選擇改變事件
			}
		}// end of 向上索引
		
		// 直接索引位置
		public void Seek(int i){
			if(i<LastTextList.Count){
				curTextIndex=i;
				this.listBox1.SelectedIndex=curTextIndex;  // 會引起選擇改變事件
			}
		}
		public void InitialSelect(){
			curTextIndex=-1;
		}
		public bool hasNext2(){

            if (CurrentPlayIndex >= LastTextList.Count - 1)
			// if(curTextIndex>=LastTextList.Count-1)
				return false;
			else
				return true;
		}

		// 是否還有下一筆資料
		public bool hasNext(){
			if(curTextIndex+1>LastTextList.Count)
				return false;
			else
				return true;
		}

		// 改變歌曲選擇
		public void ChangeToSelect(){
			this.listBox1.SelectedIndex=curTextIndex;  // 會引起選擇改變事件
		}
		
		// 索引到第一個片段
		public bool MoveFirst(){
			// 檢查是否有第一筆資料
			if(this.LastTextList.Count>0){
				myAVPlayer.label2.Text="第一筆資料";
				curTextIndex=0;
				if(this.listBox1.SelectedIndex!=curTextIndex){
					this.listBox1.SelectedIndex=curTextIndex;// 會引起選擇改變事件
				}else{
					// 當 list 中只有一筆資料時, curTextIndex 會與 this.listBox1.SelectedIndex 值相同
					// 這導致 直接設定 this.listBox1.SelectedIndex 不會引發 選擇改變事件
					// 所以當 這時就要直接進行索引
					SetupAudioSection();
				}
				return true;
			}else{
				// 沒有資料, 不動作
				return false;
			}
				
		}

        // 下面沒有任何語音片段的處理
        public void MoveDown_SectionNoMoreNext()
        {
            myAVPlayer.label2.Text = "最後一筆資料"; // 顯示最後一筆資料
            myAVPlayer.myFileListForm.PlayTheNextFile(); // 若目前語音片段已經播放完畢, 請播放下一個檔案的語音片段.
        }

		// 向下索引
		public void MoveDown(){
			curTextIndex++;

            // 若下面已經沒有任何資料
			if(hasNext()!=true){
                curTextIndex = LastTextList.Count - 1; // 若超過最後一個, 則不動指標
                MoveDown_SectionNoMoreNext();
			}else{
				ChangeToSelect();  // 會引起選擇改變事件
			}
		}// end of 向下索引

		// 清空索引串列
		public void ClearList(){
			ClearList(false);
		}
		public void ClearList(bool bShowMessage){
			if(bShowMessage){
				string message = "清空索引串列 ?";
				string caption = "注意事項";
				MessageBoxButtons buttons = MessageBoxButtons.YesNo;
				DialogResult result;

				result = MessageBox.Show(this, message, caption, buttons,
					MessageBoxIcon.Question);
				if(result==DialogResult.No){   // 使用者不想清空索引串列
					return;
				}
			}

			curTextIndex=-1;
			LastTextList.Clear();
			this.listBox1.Items.Clear();
			if(this.Text.EndsWith("*")!=true){
				this.Text=this.Text+"*";
			}

			this.textBox1.Text="";
		}// end of 清空索引串列

		// 是否要儲存索引串列
		public bool AskSaveList(){
			string message = "是否要儲存索引串列 ?";
			string caption = "索引串列已經修改";
			MessageBoxButtons buttons = MessageBoxButtons.YesNo;
			DialogResult result;

			result = MessageBox.Show(this, message, caption, buttons,
				MessageBoxIcon.Question);
			if(result==DialogResult.No){   // 使用者不想清空索引串列
				return false;
			}else{
				return true;
			}
		}// end of 儲存索引串列
		

		public string GetSeekTime(string Command){
			string[] SectionInfo_Seek_End_Repeat=new string[IndexListForm.CommandsTotalNum];
			Parse_getSeekTime_End_Repeat(Command,SectionInfo_Seek_End_Repeat);
			return SectionInfo_Seek_End_Repeat[0];
		}

		// 檢查 Seek Time 是否已經存在
		public bool checkExist_SeekTime(string strNum){
			for(int i=0;i<LastTextList.Count;i++){
				string Text=(string) LastTextList[i];
				string SeekTime_Item=GetSeekTime(Text);
				string SeekTime_strNum=GetSeekTime(strNum);
				if(SeekTime_Item.CompareTo(SeekTime_strNum)==0){
					return true;
				}
			}
			return false;
		}

		// 檢查 item 是否存在
		public bool checkExist(string strNum){
			for(int i=0;i<LastTextList.Count;i++){
				string Text=(string) LastTextList[i];
				if(Text.CompareTo(strNum)==0){
					return true;
				}
			}
			return false;
		}
		
		public bool checkExist(string strNum,int[] index){
			for(int i=0;i<LastTextList.Count;i++){
				string Text=(string) LastTextList[i];
				if(Text.CompareTo(strNum)==0){
					index[0]=i;
					return true;
				}
			}
			return false;
		}// end of 檢查 item 是否存在
	
		// 移除串列中的語音索引
		public bool RemoveItem(string strNum){
			int[] index=new int[1];
			if(checkExist(strNum,index)){
                // 立即刪除資料
				LastTextList.RemoveAt(index[0]); // 串列的部分
				this.listBox1.Items.RemoveAt(index[0]);  // 顯示的部分

                // 調整下一個索引
                int newIndex = index[0] - 1;
                if (newIndex < 0 && this.listBox1.Items.Count>0)
                    newIndex = 0;
                this.curTextIndex = newIndex;
                CurrentPlayIndex = curTextIndex;
				this.listBox1.SelectedIndex=curTextIndex; // 選擇上一個資料

                //  立即播放指定位置的資料
                this.PlayCurrentSection2();

                // 顯示資料已經改變
				if(this.Text.EndsWith("*")!=true){
					this.Text=this.Text+"*";
				}
				bModify=true;
				this.button1.Visible=true;
				return true;
			}else{
				return false;
			}
		}// end of 移除

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		
		// System.EventHandler MoveHandler;
		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IndexListForm));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.checkBox1_SectionAutioRepeat = new System.Windows.Forms.CheckBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.cbAllRepeat = new System.Windows.Forms.CheckBox();
            this.button8 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.button1.Location = new System.Drawing.Point(280, 215);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 21);
            this.button1.TabIndex = 1;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(280, 194);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(24, 21);
            this.button2.TabIndex = 2;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // listBox1
            // 
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(304, 171);
            this.listBox1.TabIndex = 3;
            this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDoubleClick);
            this.listBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            this.listBox1.SelectedValueChanged += new System.EventHandler(this.listBox1_SelectedValueChanged);
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            // 
            // button3
            // 
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Location = new System.Drawing.Point(256, 215);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(24, 21);
            this.button3.TabIndex = 4;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(208, 229);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(16, 7);
            this.button4.TabIndex = 6;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(8, 194);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(184, 20);
            this.textBox1.TabIndex = 7;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // button5
            // 
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.button5.Location = new System.Drawing.Point(256, 194);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(24, 21);
            this.button5.TabIndex = 8;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // checkBox1_SectionAutioRepeat
            // 
            this.checkBox1_SectionAutioRepeat.Image = ((System.Drawing.Image)(resources.GetObject("checkBox1_SectionAutioRepeat.Image")));
            this.checkBox1_SectionAutioRepeat.Location = new System.Drawing.Point(192, 215);
            this.checkBox1_SectionAutioRepeat.Name = "checkBox1_SectionAutioRepeat";
            this.checkBox1_SectionAutioRepeat.Size = new System.Drawing.Size(32, 21);
            this.checkBox1_SectionAutioRepeat.TabIndex = 9;
            this.checkBox1_SectionAutioRepeat.Visible = false;
            this.checkBox1_SectionAutioRepeat.CheckedChanged += new System.EventHandler(this.checkBox1_SectionAutioRepeat_CheckedChanged);
            // 
            // button6
            // 
            this.button6.Image = ((System.Drawing.Image)(resources.GetObject("button6.Image")));
            this.button6.Location = new System.Drawing.Point(232, 215);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(24, 21);
            this.button6.TabIndex = 10;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Image = ((System.Drawing.Image)(resources.GetObject("button7.Image")));
            this.button7.Location = new System.Drawing.Point(232, 194);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(24, 21);
            this.button7.TabIndex = 12;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // cbAllRepeat
            // 
            this.cbAllRepeat.Font = new System.Drawing.Font("PMingLiU", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cbAllRepeat.Image = ((System.Drawing.Image)(resources.GetObject("cbAllRepeat.Image")));
            this.cbAllRepeat.Location = new System.Drawing.Point(192, 194);
            this.cbAllRepeat.Name = "cbAllRepeat";
            this.cbAllRepeat.Size = new System.Drawing.Size(32, 21);
            this.cbAllRepeat.TabIndex = 13;
            this.cbAllRepeat.CheckStateChanged += new System.EventHandler(this.cbAllRepeat_CheckStateChanged);
            // 
            // button8
            // 
            this.button8.Image = ((System.Drawing.Image)(resources.GetObject("button8.Image")));
            this.button8.Location = new System.Drawing.Point(8, 215);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(24, 21);
            this.button8.TabIndex = 14;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Location = new System.Drawing.Point(48, 215);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 14);
            this.label1.TabIndex = 15;
            this.label1.Text = "起始:";
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label2.Location = new System.Drawing.Point(120, 215);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 14);
            this.label2.TabIndex = 16;
            this.label2.Text = "結束:";
            // 
            // IndexListForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(306, 278);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.cbAllRepeat);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.checkBox1_SectionAutioRepeat);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.HelpButton = true;
            this.Name = "IndexListForm";
            this.ShowInTaskbar = false;
            this.Text = "使用者輸入索引紀錄";
            this.Deactivate += new System.EventHandler(this.IndexListForm_Deactivate);
            this.Load += new System.EventHandler(this.IndexListForm_Load);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.IndexListForm_MouseUp);
            this.Activated += new System.EventHandler(this.IndexListForm_Activated);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.IndexListForm_MouseDown);
            this.Move += new System.EventHandler(this.IndexListForm_Move);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.IndexListForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e) {
			SaveList();
			this.myAVPlayer.label2.Text="索引資料儲存成功";

			this.button1.Visible=false;
			this.myAVPlayer.textBox1.Focus();
			//button2_Click(sender,e); // 關閉視窗
		}


        /* 程式變數參考資料
			string strLoadDir=this.myAVPlayer.ExecutationFileDir;
			string curAudioFullFileName=this.myAVPlayer.ofdOpen.FileName;
			string curAudioFileName=curAudioFullFileName.Substring(curAudioFullFileName.LastIndexOf("\\")+1);
			string strFullLoadFileName=	strLoadDir+	curAudioFileName+".txt";
        */
        // 載入串列資料 (先檢查是否在語音檔目錄, 若不在則開啟專案檔目錄中的語音索引檔)
		public void LoadList(){
			// 開啟語音檔目錄所在位置, 是否有索引檔
            if (System.IO.File.Exists(strFullLoadFileName+"_EZ.txt") == true) {
                LoadList(strFullLoadFileName+"_EZ.txt");
            }
            else {
                // 檢查是否有舊的語音索引檔版本存在
                if (System.IO.File.Exists(strFullLoadFileName + ".txt") == true) {
                    LoadList(strFullLoadFileName + ".txt");
                }
                else {
                    // 檢查專案目錄中,是否存在語音索引檔
                    // 開啟專案檔目錄中的語音索引檔
                    strLoadDir = this.myAVPlayer.ExecutationFileDir;
                    strFullLoadFileName = strLoadDir + "\\" + curAudioFileName + "_EZ.txt";
                    LoadList(strFullLoadFileName);
                }
            }
		}

        private string EatBlank(string line) {
            string command;
            int i;
            for (i = 0; i < line.Length; i++) {
                if (line[i] != ' ')
                    break;
            }
            
            command = line.Substring(i);
            return command;
        }
        private void LoadList(string FullFilename) {
           
            LastTextList.Clear(); // LastTextList先把串列資料清空
            this.listBox1.Items.Clear();
            this.curTextIndex = -1;

            
            // 由檔案中, 載入 property
            try {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(FullFilename)) {
                    String line;
                    while ((line = sr.ReadLine()) != null) {

                        string strcommand = EatBlank(line);

                        // 若 line 是 整條空白, 則 strcommand 長度會等於 0
                        if (strcommand.Length == 0)
                            continue;

                        // 若遭遇註解, 則跳過
                        if (strcommand.StartsWith("#")) { 
                            continue;
                        }

                        
                        LastTextList.Add(line);
                        this.listBox1.Items.Add(line);
                    }
                    // MessageBox.Show("before");
                    //this.Text = this.Filename;
                }
            }
            catch (Exception e) {
                // Let the user know what went wrong.
                Console.WriteLine("屬性檔載入失敗");
                Console.WriteLine(e.Message);
            }

          //  MessageBox.Show("2");
        }

        // 存檔介面程式
        // 預設目錄先存,如果是唯讀媒體, 則存到專案目錄中
		public void SaveList(){
            try {
                SaveList(strFullSaveFileName);
            }
            catch (Exception e) {
                // 發生例外, 改存到專案目錄中
                strSaveDir = this.myAVPlayer.ExecutationFileDir; 
                strFullSaveFileName = strSaveDir + "\\" + curAudioFileName + "_EZ.txt";
                SaveList(strFullSaveFileName);
            }
			
		}

        // 存檔核心程式
        private void SaveList(string FullFilename){
          
            // 存檔
            if (System.IO.File.Exists(FullFilename) == true) {
                bool oldWindowOrder = this.myAVPlayer.TopMost;
               // this.myAVPlayer.TopMost = false;

                string message = "是否要覆蓋 " + strFullSaveFileName + " ?";
                string caption = "注意事項";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(this, message, caption, buttons,
                    MessageBoxIcon.Question);

               // this.myAVPlayer.TopMost = oldWindowOrder;

                if (result == DialogResult.No) {   // 使用者不想存檔
                    return;
                }
            }

            // 把整條 list 存到檔案中
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(FullFilename)) {// 建立新的 property 檔案
                sw.WriteLine("# 這是 EZLearn 語言學習機的語音索引檔");
                sw.WriteLine("# 專案網址: http://mqjing.twbbs.org.tw/~ching/Course/JapaneseLanguageLearner/__page/JapaneseLanguageLearner.htm");
                sw.WriteLine("# 索引檔搜尋順序: (1) 語音檔所在目錄   (2) 若搜尋不到, 才會搜尋 EZLearn.exe 執行檔所在目錄");
                sw.WriteLine("#                                             版本資訊: " + AVPlayer.strVersion_subNo);
                sw.WriteLine("             ");
                for (int i = 0; i < LastTextList.Count; i++) {
                    string strIndexValue = (string)LastTextList[i];
                    string WriteString = strIndexValue;
                    sw.WriteLine(WriteString);
                }
            }

           
            this.Filename = Utility.getShortFilename(FullFilename);
            this.Text = this.Filename;
            bModify = false;
            // end of 存檔
        }

        // 開啟聽力轟炸面版
        // bool bShowOrCloseFileListForm = true;
		private void button2_Click(object sender, System.EventArgs e) {
            if (this.myAVPlayer.myFileListForm.bClosed == true)
            {
                //OpacityUtility.FadeOut_Only(this,30);
                this.myAVPlayer.myFileListForm.FadeIn();

                // 若聽力轟炸面版是關閉的, 則打開, 並且重新調整位置. 因為母面版可能已經移動位置了
                if (this.myAVPlayer.myFileListForm.bClosed == true)
                {
                    this.myAVPlayer.myFileListForm.bClosed = false;
                    this.myAVPlayer.myFileListForm.SetDefaultLocation(); // 移動到預設位置
                }
                
                // bShowOrCloseFileListForm = true;
            }
            else
            {
                // 隱藏聽力轟炸面版
                this.myAVPlayer.myFileListForm.FadeOut();
                this.myAVPlayer.myFileListForm.bClosed = true;
                // bShowOrCloseFileListForm = false; // 標記目前是關起來
            }

            //this.myAVPlayer.Focus();
		}

		
		
		private void IndexListForm_Move(object sender, System.EventArgs e) {
            // 因為 Minmin 會讓 x, y <0 , 所以我們只記錄 Normal 狀態的視窗
            if (Location.X >= 0 && Location.Y >= 0)
            {
                oldLocation = this.Location;


                if (this.myAVPlayer.myFileListForm.MyMother == this)
                {
                    if (this.myAVPlayer.myFileListForm != null && this.myAVPlayer.myFileListForm.myDockHelp.isConnected)
                    {
                        int movY = this.Location.Y + myAVPlayer.myFileListForm.myDockHelp.DMY;
                        int movX = this.Location.X + myAVPlayer.myFileListForm.myDockHelp.DMX;
                        myAVPlayer.myFileListForm.Location = new System.Drawing.Point(movX, movY);
                    }
                }
            }
		}




		private void IndexListForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			int kk=0;
			kk++;
		}

		
		int lastIndex=-1;
        // 與 JingTextEdit1.cs 中的 AllowCode method 相關
		private void listBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            this.myAVPlayer.bMySectionBasedPlay = true; // 啟動語音片段為基礎的播放
			switch(e.KeyCode){
					// 06/29/2005 Bug 修正 (對 list box 按上下鍵, 會移動兩次的 bug)
					// 原因: listbox 會自動處理 UP 與 Down, 這引發了 value changed 事件
					//       進而導致 curTextIndex 加減資料兩次
				case Keys.Up:
                    
                    this.PlayPreviousSection();
                    //System.Console.WriteLine("CurrentPlayIndex=" + CurrentPlayIndex);
                    /*
					// this.MoveUP(); //在 list box 不處理: 因為上下鍵 list 自動會處理, 我們只要在 changed value 改變即可
					if(curTextIndex==lastIndex){
						myAVPlayer.SeekPlay();
						myAVPlayer.label2.Text="資料最頂端";
                        myAVPlayer.myFileListForm.PlayThePreviouseFile();
					}
					lastIndex=curTextIndex;
                     * */
					break;
				case Keys.Down:
                    
                    this.PlayNextSection();
                    //System.Console.WriteLine("CurrentPlayIndex=" + CurrentPlayIndex);
                    /*
                    // 事件引發流程:
                    // 在 listBox 按下鍵, 會先引發 Keys.Down 事件, 
                    // 如果有真的有下降, 就會引發 value changed 事件

                    // 若目前索引已經是最後一個了, 這時使用者又按下 "下" 鍵
                    // 則播放下一個檔案的第一語音片段
                    if (this.listBox1.SelectedIndex == this.listBox1.Items.Count - 1)
                    {
                        MoveDown_SectionNoMoreNext();
                    }
					*/
                    /*
					if(curTextIndex==lastIndex){ // 若超過最後一個, 則不動指標
						myAVPlayer.label2.Text="最後一筆資料";
						myAVPlayer.SeekPlay();

                        myAVPlayer.myFileListForm.PlayTheNextFile();
					}
					lastIndex=curTextIndex;
                     */
					break;
					
				case Keys.Delete:
                    if (CurrentPlayIndex >= 0)
                    {
                        string strNum = (string)LastTextList[CurrentPlayIndex]; // 設定好要播放的指定秒數;
						this.RemoveItem(strNum);
					}
					break;

				
				case Keys.Enter: 
					// myAVPlayer.SeekPlay();
                    PlayCurrentSection2();
					break;

                case Keys.ShiftKey:  // 20061213 修正 在 IndexList Form 上按下 Shift 鍵無效的功能
                    // myAVPlayer.label2.Text = "再聽一次";

                    MessageManager.ShowInformation(myAVPlayer.label2, this.myAVPlayer.L_Manager.getMessage("IndexListForm_System_ListenAgain"), 5000, false);
                    PlayCurrentSection2();
                    // myAVPlayer.SeekPlay();
                    break;
			}

            /*
			this.myAVPlayer.textBox1_KeyDown(sender,e); // 對 Player 的 textbox1 發出 KeyDown 的事件
			// 模擬 text 元件的功能
			string strnum="";
			
			bool bNum=true;
			switch(e.KeyCode){
				case Keys.D0:
					strnum="0";
					break;
				case Keys.D1:
					strnum="1";
					break;
				case Keys.D2:
					strnum="2";
					break;
				case Keys.D3:
					strnum="3";
					break;
				case Keys.D4:
					strnum="4";
					break;
				case Keys.D5:
					strnum="5";
					break;
				case Keys.D6:
					strnum="6";
					break;
				case Keys.D7:
					strnum="7";
					break;
				case Keys.D8:
					strnum="8";
					break;
				case Keys.D9:
					strnum="9";
					break;
				default:
					bNum=false;
					break;
			}

			if(bNum==true){
				try{
					int Num=(int)Double.Parse (myAVPlayer.textBox1.Text+strnum);
					this.myAVPlayer.textBox1.Text=""+Num;
				}catch(Exception){
				}
			}

			if(e.KeyCode==Keys.Back){
				// 刪除最右邊的數字
				string strTarget=this.myAVPlayer.textBox1.Text;
				if(strTarget.Length>0){
					string newString=strTarget.Substring(0,strTarget.Length-1);
					this.myAVPlayer.textBox1.Text=newString;
				}
			}
			// end of 模擬 text 元件的功能
            */
			
		}

		// 起始點到 逗號之間的字串
		private static string getStart_Comma_String(int s,string SectionCommand,int[]retCommaPos){
			int CommaPos=SectionCommand.IndexOf(",",s);
			retCommaPos[0]=CommaPos;
			string strTimeInfo=SectionCommand.Substring(s,CommaPos-s);
			return strTimeInfo;
		}
		private static string getLastCommandString(int s, string SectionCommand){
			// 取得 End Time
			string strCommand=SectionCommand.Substring(s);
			return strCommand;
		}

		public static double getSeekTime(string SectionCommand){
			string[] SectionInfo=new string[IndexListForm.CommandsTotalNum];
			Parse_getSeekTime_End_Repeat(SectionCommand,SectionInfo);
			double dSeekTime=double.Parse(SectionInfo[0]);
			return dSeekTime; 
		}


		// 由指令 Seek[,End][,Repeat][,Comment] 解析出 Seek Time
		public static void Parse_getSeekTime_End_Repeat(string SectionCommand,string[] SectionInfo){
			// 檢查現在到底有幾個 , 號
			int Num=0;
			for(int i=0;i<SectionCommand.Length;i++){
				if(SectionCommand[i]==','){
					Num++;
				}
			}

			int[] retCommaPos=new int[1];
			string strSeek=null,strEnd=null,strRepeat=null,strComment=null;



			
			string[] NewCommand=new string[1];
			switch(Num){
				case 0:
					// 原始只有 Seek 版本
					// 指令形式= Seek
					string strNum=SectionCommand; // 設定好要播放的指定秒數;
					SectionInfo[0]=strNum;
					// end of 原始版本

					break;
				case 1:
					if(IsRelateMode(SectionCommand,NewCommand)){ // 檢查目前輸入是否為 Reverse Mode
						bool bHasRepeat=false;
						RelateModeProcess(SectionInfo,NewCommand[0],false,bHasRepeat);
						
					}else{
						// 指令形式= Seek,End
						strSeek=getStart_Comma_String(0,SectionCommand,retCommaPos); // 起始點到逗號之間的字串
						SectionInfo[0]=strSeek;

						// 取得 End Time
						strEnd=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
						SectionInfo[1]=strEnd;
					}
					break;
				case 2:
					if(IsRelateMode(SectionCommand,NewCommand)){ // 檢查目前輸入是否為 Reverse Mode
						bool bHasRepeat=true;
						RelateModeProcess(SectionInfo,NewCommand[0],false,bHasRepeat);
						
					}else{
						// 指令形式=  Seek,End, Repeat
						strSeek=getStart_Comma_String(0,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
						SectionInfo[0]=strSeek;

						strEnd=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
						SectionInfo[1]=strEnd;


						strRepeat=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
						SectionInfo[2]=strRepeat;
					}
					break;
				case 3:
					if(IsRelateMode(SectionCommand,NewCommand)){ // 檢查目前輸入是否為 Reverse Mode
						bool bHasRepeat=true;
						RelateModeProcess(SectionInfo,NewCommand[0],true,bHasRepeat);
						
					}else{
						// 指令形式=  Seek,End, Repeat, Comment
						strSeek=getStart_Comma_String(0,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
						SectionInfo[0]=strSeek;

						strEnd=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
						SectionInfo[1]=strEnd;

						strRepeat=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
						SectionInfo[2]=strRepeat;

						strComment=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
						SectionInfo[3]=strComment;
					}
					break;
			}
			
			for(int i=Num+1;i<SectionInfo.Length;i++){
				SectionInfo[i]=null;
			}		
		}

		public static bool IsRelateMode(String strSeek,string[] NewCommand){
			int i;
			for(i=0;i<strSeek.Length;i++){
				if(strSeek[i]==',')
					return false;
				if(strSeek[i]==' ')
					continue;
				if(strSeek[i]=='R' || strSeek[i]=='r')
					break;
			}

			if(strSeek[i]=='r' || strSeek[i]=='R'){
				i++;
				for(;i<strSeek.Length;i++){
					NewCommand[0]=NewCommand[0]+strSeek[i];
				}
				return true;
			}else{
				return false;
			}
		}
   
		// 取得正常狀況的 Seek, End, Repeat 資訊
		public static void RelateModeProcess(string[] SectionInfo,string SectionCommand,bool bHasCommend,bool bHasRepeat){
			int[] retCommaPos=new int[1];
			// 指令形式=  REnd,Back Second, Repeat
			// EX:
			//         R100,-10,3,從第100秒的前三秒開始播放, 連續三次
			//         R20,+5,10  <--- 從 20 秒開始, 直到往後五秒的片段重複 10 次
			
			string strStartup=getStart_Comma_String(0,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
			
			string strBack=null;
			if(bHasRepeat==true)
				strBack=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
			else
				strBack=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
			double iBack=Double.Parse(strBack);
			double iStart=Double.Parse(strStartup);
			double newSeek=iStart+iBack;
			if(newSeek<0){
				MessageBox.Show("起始索引<0");
			}

			double newEnd=iStart;
			double newStart=newSeek;

			if(newEnd < newStart){// 若指令為 R20,5,10  <--- 從 20 秒開始, 直到往後五秒的片段重複 10 次
				double temp=newEnd;
				newEnd=newStart;
				newStart=temp;
			}
			string strSeek=""+newStart;
			string strEnd=""+newEnd;
			SectionInfo[0]=strSeek;
			

			
			if(bHasRepeat){
				SectionInfo[1]=strEnd;
				if(bHasCommend){
					string strRepeat=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
					SectionInfo[2]=strRepeat;
					string strComment=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
					SectionInfo[3]=strComment;
				}else{
					string strRepeat=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
					SectionInfo[2]=strRepeat;
				}
			}else{
				// 取得 End Time
				SectionInfo[1]=strEnd;
			}
		}

		public static bool IsForwardMode(String strSeek,string[] NewCommand){
			int i;
			for(i=0;i<strSeek.Length;i++){
				if(strSeek[i]==',')
					return false;
				if(strSeek[i]==' ')
					continue;
				if(strSeek[i]=='f' || strSeek[i]=='F')
					break;
				
			}

			if(strSeek[i]=='f' || strSeek[i]=='F'){
				i++;
				for(;i<strSeek.Length;i++){
					NewCommand[0]=NewCommand[0]+strSeek[i];
				}
				return true;
			}else{
				return false;
			}
		}

		// 取得正常狀況的 Seek, End, Repeat 資訊
		// Forward Mode 指令格式: F起使秒數,Forward秒數,Repeat
		public static void FowardModeProcess(string[] SectionInfo,string SectionCommand,bool bHasCommend,bool bHasRepeat){
			int[] retCommaPos=new int[1];
			// 指令形式=  B End,Back Second, Repeat
			
			string strStartup=getStart_Comma_String(0,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
			string strForward=null;
			if(bHasRepeat==true)
				strForward=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
			else
				strForward=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
			double iForward=Double.Parse(strForward);
			double iStart=Double.Parse(strStartup);

			double newSeek=iStart;
			double newEnd=iStart+iForward;
			

			string strSeek=""+newSeek;
			string strEnd=""+newEnd;
			SectionInfo[0]=strSeek;
			
			if(bHasRepeat){
				SectionInfo[1]=strEnd;
				if(bHasCommend){
					string strRepeat=getStart_Comma_String(retCommaPos[0]+1,SectionCommand,retCommaPos); // 起始點到 逗號之間的字串
					SectionInfo[2]=strRepeat;

					string strComment=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
					SectionInfo[3]=strComment;
				}else{
					string strRepeat=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
					SectionInfo[2]=strRepeat;
				}
			}else{
				// 取得 End Time
				//strEnd=getLastCommandString(retCommaPos[0]+1,SectionCommand); // 取得最後一個字串
				SectionInfo[1]=strEnd;
			}
		}

        public bool IsEmpty()
        {
            if (this.listBox1.Items.Count == 0)
                return true;
            else
                return false;
        }

        // 播放第一個片段的語音
        public bool PlayTheFirstSection()
        {
            if (IsEmpty())
                return false;
            //this.listBox1.SelectedIndex = 0;
            //SetupAudioSection();

            CurrentPlayIndex = 0;
            this.PlayCurrentSection2();
            return true;
        }

        // 播放第一個片段的語音
        public bool PlayTheLastSection()
        {
            if (IsEmpty())
                return false;
           // this.listBox1.SelectedIndex = listBox1.Items.Count-1;
            //SetupAudioSection();

            CurrentPlayIndex = listBox1.Items.Count - 1;
            this.PlayCurrentSection2();
            return true;
        }

        public bool PlayPreviousSection()
        {
            // 若不是第一個, 則可以播放
            if (CurrentPlayIndex > 0)
            {
                CurrentPlayIndex--;
                PlayCurrentSection2();
                return true;
            }
            else
            {

                this.myAVPlayer.label2.Text = "資料最頂端";

                this.myAVPlayer.myFileListForm.bChangeFromOtherForm = true; // 標示來自其他地方, 我們要自己顯示反白
                bool bOk = this.myAVPlayer.myFileListForm.PlayThePreviouseFile();
                if (bOk == false)
                {
                    // this.myAVPlayer.AllStop();
                }

                return bOk;
            }
        }
        public bool PlayNextSection()
        {
            // 若不是最後一個, 則可以播放
            if (CurrentPlayIndex < this.listBox1.Items.Count - 1)
            {
                CurrentPlayIndex++;
                PlayCurrentSection2();
                return true;
            }
            else
            {
                this.myAVPlayer.label2.Text = "資料最底端";
                this.myAVPlayer.myFileListForm.bChangeFromOtherForm = true; // 標示來自其他地方, 我們要自己顯示反白
                bool bOk=this.myAVPlayer.myFileListForm.PlayTheNextFile();
                if (bOk == false)
                {
                    this.myAVPlayer.AllStop();
                }
                return bOk;
            }
        }
        public int CurrentPlayIndex = -1;

        public bool bChangeFromProgramm = false;
        public void PlayCurrentSection2()
        {
            if (CurrentPlayIndex >= 0 && CurrentPlayIndex <= this.listBox1.Items.Count - 1)
            {
                // 立即播放
                string strNum = (string)LastTextList[CurrentPlayIndex]; // 設定好要播放的指定秒數;
                myAVPlayer.textBox1.Text = strNum;
                myAVPlayer.SeekPlay();

                // 顯示指令註解資訊
                string strStatusInfo = myAVPlayer.textBox1.Text;// 讀取是否有相關於 索引資料的 Comment 資訊
                if (this.hasCommentInfo())
                    strStatusInfo = this.getCommentInfo();
                myAVPlayer.label2.Text = "索引到 " + strStatusInfo;

                // 設定編輯視窗
                this.textBox1.Text = strNum;

                // 若來自其他的控制, 則要自己處理 index Selected 的顯示
                if (bChangeFromProgramm)
                {
                    this.listBox1.SelectedIndex = CurrentPlayIndex; // 更新顯示
                    bChangeFromProgramm = false;
                }
            }
        }

        // 播放目前索引的語音片段, 並且作必要的資料顯示與更新動作
        public void PlayCurrentSection()
        {
            int index = this.listBox1.SelectedIndex;
            string strNum = (string)LastTextList[index]; // 設定好要播放的指定秒數;
            myAVPlayer.textBox1.Text = strNum;
            myAVPlayer.SeekPlay();

            // 顯示指令註解資訊
            string strStatusInfo = myAVPlayer.textBox1.Text;// 讀取是否有相關於 索引資料的 Comment 資訊
            if (this.hasCommentInfo())
                strStatusInfo = this.getCommentInfo();
            myAVPlayer.label2.Text = "索引到 " + strStatusInfo;


            // 更新目前索引狀態
            curTextIndex = this.listBox1.SelectedIndex;


            // 設定編輯視窗
            this.textBox1.Text = strNum;
            // end of 編輯視窗
        }
		public void SetupAudioSection(){
			if(this.listBox1.SelectedIndex >=0 && myAVPlayer.myLyricer.isLoadingLyric()==false){
				string strNum=(string) LastTextList[this.listBox1.SelectedIndex]; // 設定好要播放的指定秒數;
				myAVPlayer.textBox1.Text=strNum;
				myAVPlayer.SeekPlay();

                // 顯示指令註解資訊
                string strStatusInfo = myAVPlayer.textBox1.Text;// 讀取是否有相關於 索引資料的 Comment 資訊
				if(this.hasCommentInfo())
					strStatusInfo=this.getCommentInfo();
				myAVPlayer.label2.Text="索引到 "+strStatusInfo;

				
                // 更新目前索引狀態
				curTextIndex=this.listBox1.SelectedIndex;


				// 設定編輯視窗
				this.textBox1.Text=strNum;
				// end of 編輯視窗
			}
		}

		// 當索引位置改變時
        bool bFromSelectedValueChanged = false;
		private void listBox1_SelectedValueChanged(object sender, System.EventArgs e) {
            
            // 如果改變是來自滑鼠點選, 則需要立即播放
            if (bFromMouseDown)
            {
                CurrentPlayIndex = this.listBox1.SelectedIndex;
                this.PlayCurrentSection2();
                bFromMouseDown = false;

            }
            else
            {   
                // 不明原因造成 SelectedValueChanged 誤動作, 在這裡要強制同步
                if (CurrentPlayIndex < this.listBox1.Items.Count)
                {
                    if (bFromSelectedValueChanged == false)
                    {
                        bFromSelectedValueChanged = true;
                        this.listBox1.SelectedIndex = CurrentPlayIndex;
                        bFromSelectedValueChanged = false;
                    }
                }
            }
           
		}

		private void IndexListForm_SizeChanged(object sender, System.EventArgs e) {
			//listBox1.Size.Height=this.button1
		}

		private void button3_Click(object sender, System.EventArgs e) {
			this.ClearList(true);
			button1.Visible=true;
			this.myAVPlayer.textBox1.Focus();
		}

		private void button4_Click(object sender, System.EventArgs e) {
			this.LoadList();
			this.myAVPlayer.textBox1.Focus();
		}

		private void IndexListForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
		}


		
		
		
		protected override void WndProc(ref Message m) {
			int[] NewLoc=new int[2];
			System.Drawing.Rectangle SelfBound=this.Bounds;
			System.Drawing.Rectangle MotherBound=this.myAVPlayer.Bounds;
			// myDockHelp.WndProc(ref m,SelfBound,MotherBound,NewLoc,this);
			myDockHelp.WndProc2(ref m,SelfBound,this.myAVPlayer,NewLoc,this);

			base.WndProc(ref m);

			//this.Text="c="+myDockHelp.isConnected+" DMY="+myDockHelp.DMY+" DMX="+myDockHelp.DMX;
		}

		private void IndexListForm_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			
			
		}
        public bool bActived=false;

        bool bFirstLoad = true;
		private void IndexListForm_Load(object sender, System.EventArgs e) {
            // 讀取母視窗的 Z-Order, 決定自己的 Z-Order
            this.TopMost = this.myAVPlayer.MyTopMost;

            // 隨時記錄目前位置, 
            if (WindowState == FormWindowState.Normal)
                oldLocation = this.Location;

            // 設定初始位置
            if (bFirstLoad)
            {
                SetDefaultLocation();
                bFirstLoad = false;
            }
		}

        public void SetDefaultLocation()
        {
            // 移動目前位置到母視窗的下面, 會自動呼叫 WndProc 
            // 會起動黏貼功能. 
            DockUtility.MoveToMotherBottom(this, this.myAVPlayer);
        }

		private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			// 使用者編輯完畢, Update 目前 item list 的資料
			bool bOk=false;
			if(e.KeyCode==Keys.Enter) {
				// 先檢查目前輸入是否存在
				bool Exist=checkExist_SeekTime(this.textBox1.Text);
				if(Exist==true){
					// 存在時, 進入編輯模式
					if(this.textBox1.Text.IndexOf(",")==-1){
						// 傳統輸入, 一定是數字
						try{
							int.Parse(this.textBox1.Text);
							bOk=EditItem(this.textBox1.Text);
						
						}catch{
						}
					}else{
						// 擴充格式 
						bOk=EditItem(this.textBox1.Text);	
					}

                    if (!bOk)
                    {
                        this.myAVPlayer.label2.Text = "格式錯誤";
                    }
                    else
                    {
                        PlayCurrentSection2(); // 修改過後, 立即播放
                    }
				}else{
					// 目前字串不存在, 進入新增模式
					this.AddItem(this.textBox1.Text);
				}
			}

			if(e.KeyCode==Keys.Up){
				// this.MoveUP();
                this.bChangeFromProgramm = true; // 自己處理反白
                this.PlayPreviousSection();
			}

			if(e.KeyCode==Keys.Down){
				// this.MoveDown();
                this.bChangeFromProgramm = true; // 自己處理反白
                this.PlayNextSection();
			}

		}
		
		class Section_Compare:System.Collections.IComparer{

			public int Compare(
				object x,
				object y
				){

				// 取出 x 的起使索引
				string Commandx=(string)x;
				double dSeekX=IndexListForm.getSeekTime(Commandx);
				string Commandy=(string)y;
				double dSeekY=IndexListForm.getSeekTime(Commandy);

				return (int)(dSeekX-dSeekY);
			}
		}

		private void button5_Click(object sender, System.EventArgs e) {
			
			if(LastTextList.Count >0){
				LastTextList.Sort(new Section_Compare()); // 進行排序

				this.listBox1.Items.Clear(); // 清除顯示面版的內容

				// 將 LastTextList 資料一一加入面版 
				for(int i=0;i<LastTextList.Count;i++){
					string sCommand=(string)LastTextList[i];
					this.listBox1.Items.Add(sCommand);      // 設定 List 顯示
				}

				curTextIndex=0; // 排序完後,指定第一筆資料為起始點
				this.listBox1.SelectedIndex=curTextIndex;

				SetModify();
			}else{
				// 沒有資料不動作
			}

			
		}

		private void checkBox1_SectionAutioRepeat_CheckedChanged(object sender, System.EventArgs e) {
			this.myAVPlayer.PropertyTable["SectionAutoRepeat"]=""+this.checkBox1_SectionAutioRepeat.Checked;
			this.myAVPlayer.SaveProperty();

			if(this.checkBox1_SectionAutioRepeat.Checked){
				this.MoveFirst();
				this.myAVPlayer.label2.Text ="語音片段自動循環"+"  ON";
			}else{
				this.myAVPlayer.label2.Text ="語音片段自動循環"+"  OFF";
			}
		}

		public bool isTheLast(){
			return !hasNext2();
		}

		// 檢查是否到達
		double WaitCount=0;
		bool NowWaitToNext(){
			// 讀取預設 Repeat  等待時間
			string strDefaultBlank=(string)this.myAVPlayer.PropertyTable["DefaultCommand_Blank"];
			double BlankSecnod=Double.Parse(strDefaultBlank);
			if(WaitCount<BlankSecnod){
				WaitCount+=0.2;
				int WaitBlankSecnod=(int)(BlankSecnod-WaitCount)+1;
				myAVPlayer.ShowState(WaitBlankSecnod+" 秒後重播");
				myAVPlayer.ourAudio.Pause();
				return true;
			}else{
				WaitCount=0;
				myAVPlayer.ourAudio.Play();
				return false;
			}
		}

		// 執行 Repeat 的動作
		public void DoRepeat(){
			if(NowWaitToNext()==true)
				return;
			DoRepeat(true);
		}

		public void DoRepeat(bool bCountDown){
			if(bCountDown){
				int curRepeatCount=this.RepeatCountDown();
				
				if(curRepeatCount>0){
					bool bRepeat=true;
					myAVPlayer.SeekPlay(bRepeat);
					myAVPlayer.ShowState("重播: "+curRepeatCount);
				}else{
					// Repeat 播放完畢,的處理事項
					EndOfRepeat();			
				}
			}else{
				bool bRepeat=true;
				myAVPlayer.SeekPlay(bRepeat);
				myAVPlayer.ShowState("無限循環");
			}
		}

		public void EndOfRepeat(){
			// Repeat 播放完畢

            // 是否為最後語音片段
            if (isTheLast() == true)
            {
                // 是否單一檔案要循環播放所有的片段
                if (this.checkBox1_SectionAutioRepeat.Checked == true)
                {
                    // 執行循環片段播放
                    this.MoveFirst();
                    this.myAVPlayer.label2.Text = "自動循環";
                }
                else
                {    // 否則交給聽力轟炸面版處理: 當所有檔案中的語音片段都播放完畢, 則停止播放, 否則播出下一個檔案中的第一個語音片段
                    if (this.myAVPlayer.myFileListForm.PlayTheNextFile() == false)
                        this.myAVPlayer.AllStop();
                }
                return;
            }

            // 非最後語音片段的一般情況處理
            this.myAVPlayer.ShowState("繼續播放");
            if (this.hasNext2())
            {
                // this.MoveDown();
                this.bChangeFromProgramm = true; // 需要自己處理顯示反白
                this.PlayNextSection();
            }
            else
            {
                // .. do not thing
            }


            /*

			bool bNormalRepeat=false;

			// 是否為最後語音片段
			if(isTheLast()==true){
				// 檢查是否有語音片段循環播放
				if(this.checkBox1_SectionAutioRepeat.Checked==true){
					// 執行循環片段播放
					this.MoveFirst();
					this.myAVPlayer.label2.Text ="自動循環";
				}else{
					bNormalRepeat=true;
				}

			}else{
				bNormalRepeat=true;
			}// end of 一般片段結束

			// 執行一般 Repeat 結束 動作
			if(bNormalRepeat){
				this.myAVPlayer.ShowState("繼續播放");
				if(this.hasNext2()){
					this.MoveDown();
				}else{
					// .. do not thing
				}
			}
             */
		}

		private void button6_Click(object sender, System.EventArgs e) {
			if(this.listBox1.SelectedIndex >=0 && this.textBox1.Text!=null){
				string strNum=(string) LastTextList[this.listBox1.SelectedIndex]; // 設定好要播放的指定秒數;
				this.RemoveItem(strNum);
				this.myAVPlayer.label2.Text="刪除成功";
			}else{
				this.myAVPlayer.label2.Text="片段不在串列中";
			}
		}

		private void button7_Click(object sender, System.EventArgs e) {
			if(CheckCommand(this.textBox1.Text)==true){
				if(checkExist(this.textBox1.Text)!=true){
					AddItem(this.textBox1.Text);
				}else{
					this.myAVPlayer.label2.Text="片段已經存在";
				}
			}else{
				this.myAVPlayer.label2.Text="格式錯誤";
			}
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e) {
			this.button6.Enabled=true; 
			this.button7.Enabled=true; 
		}

		private void cbAllRepeat_CheckStateChanged(object sender, System.EventArgs e) {
			if(this.cbAllRepeat.Checked==true){
				myAVPlayer.ShowState("無限循環");
				System.Drawing.Bitmap imgRepeatOn=new System.Drawing.Bitmap(myAVPlayer.ExecutationFileDir+"\\RepeatOn.gif"); // 目前載入影像
				this.cbAllRepeat.Image=imgRepeatOn;

				this.checkBox1_SectionAutioRepeat.Enabled =false;
			}
			else{
				myAVPlayer.ShowState("無限循環取消");
				System.Drawing.Bitmap imgRepeatOff=new System.Drawing.Bitmap(myAVPlayer.ExecutationFileDir+"\\RepeatOff.gif"); // 目前載入影像
				this.cbAllRepeat.Image=imgRepeatOff;
				this.checkBox1_SectionAutioRepeat.Enabled =true;
			}
		}

		// 當使用者按下預設指令按鈕
		private void button8_Click(object sender, System.EventArgs e) {
			string Repeat=(string) myAVPlayer.PropertyTable["DefaultCommand_Repeat"];
			string Index=(string) myAVPlayer.PropertyTable["DefaultCommand_Index"];
			double curPos=(int)(myAVPlayer.ourAudio.CurrentPosition*100)/100.0;

			string Command=GetDefaultCommand(curPos.ToString(),Repeat,Index);
			this.textBox1.Text=Command;
			
			bool	bOk=AddItem(this.textBox1.Text);
			if(bOk){
				//UpdateCommandLabel(this.textBox1.Text);
			}
			
		}// end of method

        

        // 更新標示
        public void UpdateCommandLabel(string Command)
        {
			string[] SectionInfo_Seek_End_Repeat=new string[IndexListForm.CommandsTotalNum];
			Parse_getSeekTime_End_Repeat(Command,SectionInfo_Seek_End_Repeat);
			label1.Text="起始: "+SectionInfo_Seek_End_Repeat[0];
			label2.Text="結束: "+SectionInfo_Seek_End_Repeat[1];
		}

		public string GetDefaultCommand(string strCurPos,string Repeat,string RIndex){
			return "R"+strCurPos+","+RIndex+","+Repeat+", 這是預設指令";
		}

        private void IndexListForm_Activated(object sender, EventArgs e)
        {
            bActived = true;
            this.myAVPlayer.iStat++;

            //System.Console.WriteLine("iStat=" + this.myAVPlayer.iStat);
        }

        private void IndexListForm_Deactivate(object sender, EventArgs e)
        {
            
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
          
        }
        bool bFromMouseDown = false;
        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            this.myAVPlayer.bMySectionBasedPlay = true; // 啟動語音片段為基礎的播放
            
            // this.PlayCurrentSection2();
            // 滑鼠按下去的時候, 自然會啟動選擇 listBox1_SelectedValueChanged
            bFromMouseDown = true;
        }

	}
}
