//                                                                                        井民全
// 主要的設計目的: 就是要在不干擾使用者的情況下, 產生更新檢查. 
//                 檢查完畢後, 如果使用者不感興趣時, 自動關閉視窗.

//                 為了因應有些使用者很討厭自動更新視窗煩惱 (包含 我), 
//                 所以一定要有取消這項功能的選項.

// 程式設計關鍵:   .Net Framewowrk 提供兩種 Timer 模式, 第一種是利用 Message 方式模擬多個 thread, 若有高耗時計算, 則會鎖住其他元件的運算
//                 第二種是真正產生一個 thread, 進行處理, 不會影響到其他元件的訊息接收
// 
//                 因為 .Net Windows Form space 的 Timer 主要是使用單一 thread 每隔一段時間, 執行指定的程式碼.
//                 如果直接使用這個來執行網路上的檢查, 會有問題.
//                 當檢查時, 你的主要視窗會被鎖住不能動彈.

//                 主要原因就是你的視窗程式被網路鎖住了. 所以解決方案就是讓網路鎖住另一個 thread
//                 不要讓他鎖住主要視窗 thread.

// 實際作法:       利用 System.Threading.Timer 建立另一條 thread, 由他來執行高耗時的網路檢查工作
// 關鍵程式片段:
/*
            lock(this){
				hasFinish=false;
			}
			// 設定 checkVersion 專屬的 Timer
			CheckVersion myCVObj=new CheckVersion(this);
			System.Threading.TimerCallback timerDelegate = 
				new System.Threading.TimerCallback(myCVObj.CheckStatus);
			myVersionCheckTimer = 
				new System.Threading.Timer(timerDelegate, null, 0, 0); // 馬上開始呼叫 CheckStatus, 並且不重複呼叫
			// end of 設定 checkVersion 專屬的 Timer 

			// 設定 檢查進度 專屬的 Timer
			myUpdateProgressTimer=new FormUpdateTimer(this);
			myUpdateProgressTimer.Tick += new EventHandler(TimerEventProcessor2);	
			myUpdateProgressTimer.Interval = 50; // 每隔 .5 秒鐘, 呼叫 TimerEventProcessor procedure
			myUpdateProgressTimer.Start();
			// end of 設定 checkVersion 專屬的 Timer 
*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices; // for DllImport


namespace Player
{
	/// <summary>
	/// Summary description for UpdateNotifiyForm.
	/// </summary>
	public class UpdateNotifiyForm : System.Windows.Forms.Form
	{
        private IntPtr ParentHandle;

        [DllImport("User32.dll")]
        public static extern int SendMessage(
            IntPtr hWnd,               // handle to destination window
            int Msg,                // message
            int wParam,             // first message parameter
            int lParam);            // second message parameter
       
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.ProgressBar progressBar1;
		public AVPlayer myPlayParent=null;
		

		public NumberLabel l1; //  倒數計時物件
		public static int iErrorCountDown=5;     // 網路發生錯誤時, 停留的秒數
		
		public int CountDownStart=5;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.Label label3; // 倒數計時啟始值

		string strVersionStatus;
        WebBrowser EZwebBrowser; // Tracking 使用量專用
        System.IO.StreamReader PrjStream=null;

		public UpdateNotifiyForm(AVPlayer obj)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			myPlayParent=obj; // 主要視窗 [為了要取得屬性串列]
            ParentHandle = obj.Handle;

            string strCurVersion = this.myPlayParent.L_Manager.getMessage("Version_CurrentVersionString");
            this.label1.Text = strCurVersion + AVPlayer.strVersion_subNo; // this.label1.Text="目前版本資訊:"+AVPlayer.strVersion_subNo;


            this.label2.Text = this.myPlayParent.L_Manager.getMessage("Version_CheckingWaitingMessage"); // this.label2.Text="狀態:"+"  狀態檢查中, 請等一下...";


            MessageManager.ShowInformation(this.myPlayParent.label3, this.label2.Text, 5000, false);
           //  MessageManager.ShowInformation(this.myPlayParent.label3, "目前正在進行版本檢查 ...", 5000,false);

			// 加入倒數計時機制
			l1=new NumberLabel(myPlayParent.imgNumArray);
			// l1=new NumberLabel(myPlayParent.ExecutationFileDir,"Num",0,0,0);   // 指定數字檔名, 指定透明顏色 r,g,b
			l1.Visible=false;
			l1.Left= label1.Right; l1.Top=label1.Top;
			l1.Height=30;//l1.Width=30;
			l1.SetWidth(4); // 有四組數字
			//l1.Text=""+CountDownStart; // 顯示數字
			this.Controls.Add(l1); // 加入母類別
			// end of 倒數計時機制

            

			// 線上檢查
			string strOnlineU=(string)myPlayParent.PropertyTable["是否要線上檢查版本"];
			if(strOnlineU.IndexOf("true")!=-1)
				this.checkBox1.Checked=true;
			else
				this.checkBox1.Checked=false;
			// end of 線上檢查


		}

		System.Threading.Timer myVersionCheckTimer;
		FormUpdateTimer myUpdateProgressTimer;

		public bool hasFinish=false;


		public void FireCheck(){
			// 因為 Form space 中的 Timer 所產生的 Notification 事件, 呼叫事件函式使用的單一 thread
			// 在高耗時運算時, 會使其他視窗動作全部暫停.
			// 所以高耗時函式呼叫時, 必須使用 Thread 版本的 Timer, 建立另一條 thread 執行高耗時工作
			lock(this){
				hasFinish=false;
			}

			// 設定 checkVersion 專屬的 Thread 版本 Timer
			CheckVersion myCVObj=new CheckVersion(this);
			System.Threading.TimerCallback timerDelegate =new System.Threading.TimerCallback(myCVObj.CheckStatus);
			myVersionCheckTimer = new System.Threading.Timer(timerDelegate, null, 0, 0); // 馬上開始呼叫 CheckStatus, 並且不重複呼叫
			// end of 設定 checkVersion 專屬的 Timer 
            /*
			// 設定 檢查進度 專屬的 Timer
			myUpdateProgressTimer=new FormUpdateTimer(this);
			myUpdateProgressTimer.Tick += new EventHandler(TimerEventProcessor2);	
			myUpdateProgressTimer.Interval = 50; // 每隔 .5 秒鐘, 呼叫 TimerEventProcessor procedure
			myUpdateProgressTimer.Start();
            */
			// end of 設定 checkVersion 專屬的 Timer 

		}

		// 另一條 thread 上網檢查版本資訊
		public bool hasNewVersion;
		public void TimerEventProcessor1() {
			hasNewVersion=checkVersion(); // 執行 高耗時 檢查版本資訊
			
			this.myVersionCheckTimer.Dispose();

			// 檢查完成, 更新顯示資訊
			lock(this){
				this.hasFinish=true;// 檢查完成
			}
		}
		

		// 顯示檢查進度
        bool bTrackRun = true;
		bool bCountDown;
		private static void TimerEventProcessor2(Object myObject,
			EventArgs myEventArgs) {

            
			UpdateNotifiyForm myParentForm=((FormUpdateTimer)myObject).myParentForm;

            // Web Tracking 的部分
            /*
             * The WebBrowser control is resource-intensive. 
             * Be sure to call the Dispose method when you are finished using the control to ensure that all resources are released 
             * in a timely fashion. You must call the Dispose method on the same thread that attached the events, which should always
             * be the message or user-interface (UI) thread.
             */
            if (myParentForm.bTrackRun == true) {
                myParentForm.EZwebBrowser = new WebBrowser();
                myParentForm.EZwebBrowser.Navigate(@"http://mqjing.twbbs.org.tw/~ching/Course/JapaneseLanguageLearner/__page/UsageCounter.html");
                myParentForm.bTrackRun = true;


               // myParentForm.EZwebBrowser.Navigate(@"http://debut.cis.nctu.edu.tw/~ching/Course/JapaneseLanguageLearner/__page/Download2.htm");
               // myParentForm.PrjStream = new System.IO.StreamReader(myParentForm.EZwebBrowser.DocumentStream, System.Text.Encoding.GetEncoding(myParentForm.EZwebBrowser.Document.Encoding));

            }


			myParentForm.bCountDown=false; // 是否啟動倒數關閉機制
			bool bF;
			lock(typeof(UpdateNotifiyForm)){
				bF=myParentForm.hasFinish;
			}

			if(!bF){
				// 若進度表 = 100%, 則歸零 繼續
				if(myParentForm.progressBar1.Value == myParentForm.progressBar1.Maximum)
					myParentForm.progressBar1.Value=0;
				myParentForm.progressBar1.Increment(1);
				
			}else{
				// 檢查完成, 更新顯示資訊
				myParentForm.UpdateInfo(myParentForm.hasNewVersion);
				myParentForm.progressBar1.Value=myParentForm.progressBar1.Maximum;

				myParentForm.bCountDown=true; // 啟動倒數關閉機制
				myParentForm.myUpdateProgressTimer.Interval=1000; // 重新設定每隔一秒鐘, 倒數計時一次
				myParentForm.l1.Visible=true;
			}
			
			// 倒數關閉機制
			if(myParentForm.bCountDown){
				myParentForm.l1.Text=""+myParentForm.CountDownStart; // 顯示數字
				if(myParentForm.bAutoClose){ // 只有在自動關閉功能下,會倒數計時. (當使用者 Active 視窗時, 自動關閉功能會被取消)
					string strTitle="版本資訊 ("+myParentForm.CountDownStart+" 秒鐘後自動關閉)";
					myParentForm.Text=strTitle;

					myParentForm.CountDownStart--;

					if(myParentForm.CountDownStart <0){
						myParentForm.myUpdateProgressTimer.Stop();
						myParentForm.myUpdateProgressTimer.Dispose();
						OpacityUtility.FadeOut_Close(myParentForm,30);
					}
				}else{
					myParentForm.Text="版本資訊"+"(倒數關閉暫停)";
				}

			}
		}
		// end of 檢查進度


		public void UpdateInfo(bool hasNewVersion){
			if(hasNewVersion){
				this.label2.Text="狀態:"+strVersionStatus;
				string strPrjaddress=(string)myPlayParent.PropertyTable["專案網址"];
				this.linkLabel1.Visible=true;
			}else{
				this.label2.Text="狀態:"+strVersionStatus;
			}

		}

       
        // 訊息通知公用函式
        public void SendNeedToUpdateMessage(System.Windows.Forms.Form Form)
        {
            //const int WM_APP = 0x8000;
            //const int WM_NeedToUpdateMessage = (WM_APP + 1);
            int wparam = 1; // 1: 表示需要Update, 0 表示不需要 Update, -1 網路有問題
            SendMessage(ParentHandle, AVPlayer.WM_NeedToUpdateMessage, wparam, 0);
        }

        public void SendNetworkError(System.Windows.Forms.Form Form)
        {
            int wparam = -11; // 1: 表示需要Update, 0 表示不需要 Update, -1 網路有問題
            SendMessage(ParentHandle, AVPlayer.WM_NeedToUpdateMessage, wparam, 0);
        }

        public void Send_VersionOK(System.Windows.Forms.Form Form)
        {
            //const int WM_APP = 0x8000;
            //const int WM_NeedToUpdateMessage = (WM_APP + 1);
            int wparam = 0; // 1: 表示需要Update, 0 表示不需要 Update, -1 網路有問題
            SendMessage(ParentHandle, AVPlayer.WM_NeedToUpdateMessage, wparam, 0);
        }

        



		// 檢查版本資訊
		// 高耗時特性
		// System.IO.StreamReader PrjStream;
		public bool checkVersion(){

			CountDownStart=10;
			string strTitle="版本資訊 ("+CountDownStart+" 秒鐘後自動關閉)";
			//this.Text=strTitle;

             
			bool bFoundNewVersion=false;
			bool hasFoundSelf=false;
			// string strPrjAddress=(string)myPlayParent.PropertyTable["專案網址"];
            string strPrjAddress = "http://mqjing.twbbs.org.tw/~ching/Course/JapaneseLanguageLearner/__page/Download2.htm";

            
			try{

               // EZwebBrowser.Navigate(@"http://debut.cis.nctu.edu.tw/~ching/Course/JapaneseLanguageLearner/__page/Download2.htm");
               // PrjStream = new System.IO.StreamReader(EZwebBrowser.DocumentStream, System.Text.Encoding.GetEncoding(EZwebBrowser.Document.Encoding));

                
				System.Net.WebClient PrjClient=new System.Net.WebClient();
				PrjStream=new System.IO.StreamReader(PrjClient.OpenRead(strPrjAddress));
                /*
                while (PrjStream == null)
                {
                    System.Threading.Thread.Sleep(100);
                    // Application.pro
                };
                */
				String line;
					
				
				while ((line = PrjStream.ReadLine()) != null) {
					int i=line.IndexOf(AVPlayer.strVersion_subNo+"_GreenInstall.zip");
					if(i!=-1){
						hasFoundSelf=true;
						break;
					}
				}

				if(hasFoundSelf){
					bFoundNewVersion=false;
                    if (AVPlayer.bBeta == false)
                        this.strVersionStatus = "最新版本";
                    else
                        this.strVersionStatus = "這是 Beta 版本";

                    //MessageManager.ShowInformation(this.myPlayParent.label3, "目前版本是最新版本", 10000);
                    Send_VersionOK(myPlayParent);
					CountDownStart=3;
					
				}else{
					bFoundNewVersion=true;
					
                    if (AVPlayer.bBeta == false)
                        this.strVersionStatus = "  您的軟體版本是舊的, 網路上已經有新的版本";
                    else
                        this.strVersionStatus = "這是 Beta 版本";

                    //MessageManager.ShowInformation(this.myPlayParent.label3, "網路上已經有新的版本", 10000);
					this.myPlayParent.PropertyTable["目前版本狀態"]="舊的"; // 07/21/2005 [功能調整] 
					this.myPlayParent.SaveProperty(); // Update 屬性檔 
                    //this.myPlayParent.Text = strVersionStatus;

                    SendNeedToUpdateMessage(myPlayParent);
                    // SendMessage(this.myPlayParent.Handle , AVPlayer.WM_NeedToUpdateMessage, 0, 0);

					// this.myPlayParent.Text="語言學習機 (網路上已經有新的版本)";  
				}

               

			}catch(Exception e){
				System.Windows.Forms.MessageBox.Show("專案網址:"+strPrjAddress+"\n\n網路似乎有問題:\n"+"\n\n詳細錯誤資訊:"+e.ToString(),"自動更新面版");
				this.strVersionStatus="網路似乎有問題: "+e.Message;
				CountDownStart=iErrorCountDown;
				
				// this.Text="版本資訊檢查中...   ("+CountDownStart+" 秒鐘自動關閉)";;

                SendNetworkError(this.myPlayParent); // 送出網路有問題訊息給母視窗
				return false;
			}

			return bFoundNewVersion;
			// 
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
            // 移除追蹤項
            if(EZwebBrowser!=null)
                EZwebBrowser.Dispose();

			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();

					if(PrjStream!=null)
						PrjStream.Close();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(368, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "版本資訊";
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label2.Location = new System.Drawing.Point(8, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(392, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "狀態";
            // 
            // linkLabel1
            // 
            this.linkLabel1.LinkColor = System.Drawing.Color.Aqua;
            this.linkLabel1.Location = new System.Drawing.Point(144, 88);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(72, 23);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "下載新版本";
            this.linkLabel1.Visible = false;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(8, 72);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(392, 8);
            this.progressBar1.TabIndex = 4;
            // 
            // checkBox1
            // 
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(288, 88);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(10, 10);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.Visible = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label3.Location = new System.Drawing.Point(304, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "每次啟動都檢查";
            // 
            // UpdateNotifiyForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 15);
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.ClientSize = new System.Drawing.Size(416, 102);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "UpdateNotifiyForm";
            this.Text = "版本資訊檢查中...  ";
            this.Deactivate += new System.EventHandler(this.UpdateNotifiyForm_Deactivate);
            this.MouseEnter += new System.EventHandler(this.UpdateNotifiyForm_MouseEnter);
            this.Activated += new System.EventHandler(this.UpdateNotifiyForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.UpdateNotifiyForm_Closing);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UpdateNotifiyForm_MouseDown);
            this.Load += new System.EventHandler(this.UpdateNotifiyForm_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void UpdateNotifiyForm_Load(object sender, System.EventArgs e) {
			
		}

		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) {

			// 取得專案的網址
			string strPrjAddress=(string)myPlayParent.PropertyTable["專案網址"];
			
			// 設定連結目的地
			this.linkLabel1.Links[0].LinkData = strPrjAddress;
			string target = e.Link.LinkData as string;

			// 呼叫 IE 瀏覽器, 連結到指定的網址
			System.Diagnostics.Process.Start("IExplore.exe",target);
		}

		private void UpdateNotifiyForm_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			
		}

		private void checkBox1_CheckedChanged(object sender, System.EventArgs e) {
			if(this.checkBox1.Checked){
				myPlayParent.PropertyTable["是否要線上檢查版本"]="true";
			}else{
				myPlayParent.PropertyTable["是否要線上檢查版本"]="false";
			}
			myPlayParent.SaveProperty();
		}

	    bool bAutoClose=true;
		private void UpdateNotifiyForm_Activated(object sender, System.EventArgs e) {
			// 當使用者對這個視窗有興趣時, 取消自動關閉的功能
			if(this.bCountDown){
				bAutoClose=false;
				this.Opacity=1.0; // 恢復視窗清晰度
			}
		}
		
		private void UpdateNotifiyForm_MouseEnter(object sender, System.EventArgs e) {
			
		}
		 
		private void UpdateNotifiyForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			
		}

		private void UpdateNotifiyForm_Deactivate(object sender, System.EventArgs e) {
			if(this.bCountDown){
				bAutoClose=true;
			}
		}
	}

	class FormUpdateTimer:System.Windows.Forms.Timer {
		public UpdateNotifiyForm myParentForm;
		public FormUpdateTimer(UpdateNotifiyForm showPannel) {
			myParentForm=showPannel;
		}
	}


	class CheckVersion{
		public UpdateNotifiyForm myParentForm;
		public CheckVersion(UpdateNotifiyForm showPannel) {
			myParentForm=showPannel;
		}

        

		// This method is called by the timer delegate.
		public void CheckStatus(Object stateInfo) {
			myParentForm.TimerEventProcessor1(); // 高耗時

		}

       

	}

}
