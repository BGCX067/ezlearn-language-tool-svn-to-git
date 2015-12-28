using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Player
{
	


	/// <summary>
	/// Summary description for SwiftlyLabel.
	/// </summary>
	public class SwiftlyLabel : System.Windows.Forms.UserControl
	{
		int cWidth;
		SwiftTimer myTimer; // 移動用計時器
		public bool bRun=true; // 記錄現在是否正在移動
		private System.Drawing.Font myFont;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SwiftlyLabel(int Width)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			Myinit();
			Start();
			this.cWidth=Width;
			this.Width =Width;

			SetStyle(ControlStyles.DoubleBuffer | 
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint
				,true);	
				
		}

		Label myInfoLabel=null; // 顯示狀態用
		public void setInfoLabel(Label InfoLabel){
			myInfoLabel=InfoLabel;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem1,
																						 this.menuItem2,
																						 this.menuItem4,
																						 this.menuItem5,
																						 this.menuItem3});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "歌詞快一秒出現";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "歌詞慢一秒出現";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 4;
			this.menuItem3.Text = "恢復預設值";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "關閉字幕";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 3;
			this.menuItem5.Text = "";
			// 
			// SwiftlyLabel
			// 
			this.BackColor = System.Drawing.Color.Gray;
			this.ContextMenu = this.contextMenu1;
			this.Name = "SwiftlyLabel";
			this.Size = new System.Drawing.Size(336, 16);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e) {
			AddLabel("Baby you would take away everything good in my life,");
		    Start();
		}

		
		private void Myinit(){
			// 設定 AVPlay 專屬的 Timer
			myTimer=new SwiftTimer(this);
			myTimer.Tick += new EventHandler(TimerEventProcessor);			
			myTimer.Interval = 100; // 每隔 0.1 秒鐘, 呼叫 TimerEventProcessor procedure
			// end of 設定 AVPlay 專屬的 Timer
 		
			// 指定字形
			myFont=new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			// end of 字形
		}
		
		// 停止跑馬燈
		public void Stop(){
			bRun=false;
		}

		// 開始跑馬燈
		public  void Start(){
			bRun=true;
			myTimer.Start(); // 啟動移動
		}

		ArrayList myAL = new ArrayList(); // 存放 label 的元件	
		public void ClearAllLabel(){
			for(int i=0;i<myAL.Count;i++){
				Label m=(Label)myAL[i];
				m.Text="";
			}
			//myAL.Clear();
		}
		public void AddLabel(string title){
			Label newlabel=new MyLabel();
			//newlabel.Top=this.button1.Top;

			

			// 指定 要顯示的文字
			newlabel.Text=title;
			// end of 文字

			// 設定 label 的字型
			newlabel.Font=this.myFont;
			newlabel.ForeColor=System.Drawing.Color.White;
			// end of 字型

			// 出現的起始位置
			// int left=this.button1.Left  - 20;
			int left=this.cWidth  - 20;
			if(myAL.Count>0 && left < ((Label)myAL[myAL.Count-1]).Right){
				left=((Label)myAL[myAL.Count-1]).Right; // 找出 list 中最後面的那個 label
			}
			newlabel.Left=left; // 一開始是在最右邊
			// end of 起始位置

			

			//newlabel.BackColor = System.Drawing.Color.Transparent; // 設定 label 的背景是透明的, 否則會蓋住後面的 label
			
			// 設定 label 元件的寬度
			newlabel.AutoSize=true;
			// end of 元件的寬度
			
			myAL.Add(newlabel);
			this.Controls.Add(newlabel);

			newlabel.Visible=true;
		}
		public void Speed(int u){
			myTimer.Interval=u;
		}

		// 每隔一秒鐘會呼叫
		private static void TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {

			SwiftlyLabel st=((SwiftTimer)myObject).myParent;
			if(st.bRun){
			
				int step=5;
				ArrayList myAL =st.myAL;
				// 移動
				for(int i=0;i<myAL.Count;i++){
					Label l=(Label)(myAL[i]);
					l.Left-=step;
				}
				// end of 移動

				// 檢查
				for(int i=0;i<myAL.Count;i++){
					Label l=(Label)(myAL[i]);
					if(l.Right <0){
						myAL.Remove(l);
						i--;
					}
				}
				// end of 檢查
			}	
		}

		// 改變速度測試
		private void button2_Click(object sender, System.EventArgs e) {
			myTimer.Interval=100;
		}
		
		public int Latency=0;
		private void menuItem1_Click(object sender, System.EventArgs e) {
			Latency=Latency-1; // 加快一秒出現
			if(myInfoLabel!=null)
				myInfoLabel.Text="歌詞早 "+Latency+" 秒 設定";
		}

		private void menuItem2_Click(object sender, System.EventArgs e) {
			Latency=Latency+1; // 加快一秒出現
			if(myInfoLabel!=null)
				myInfoLabel.Text="歌詞晚 "+Latency+" 秒 設定";
		}

		private void menuItem3_Click(object sender, System.EventArgs e) {
			Latency=0;
		}

		LyricClass myLyric=null;
		public void setMyLyricer(LyricClass Lyric){
			myLyric=Lyric;
		}


		private void menuItem4_Click(object sender, System.EventArgs e) {
			if(myLyric!=null)
				myLyric.VisibleSwitch();
		}
		

	}

	// 移動專用 timmer
	class SwiftTimer:System.Windows.Forms.Timer {
		public SwiftlyLabel myParent;
		public SwiftTimer(SwiftlyLabel showPannel) {
			myParent=showPannel;
		}
	}
	// end of  移動專用 timmer

	class MyLabel:Label{
		public MyLabel(){
			SetStyle(ControlStyles.DoubleBuffer | 
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint
				,true);	
		}
	}

}


