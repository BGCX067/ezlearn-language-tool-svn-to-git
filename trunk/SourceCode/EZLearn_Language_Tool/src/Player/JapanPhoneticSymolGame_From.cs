using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

// 播放語音相關
using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
// end of 語音

namespace Player {
	/// <summary>
	/// Summary description for JapanPhoneticSymolGame_From.
	/// </summary>
	public class JapanPhoneticSymolGame_From : System.Windows.Forms.Form {
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		/// 
		private System.ComponentModel.Container components = null;

		// 日文字母符號定義 (平假名,片假名)
		double [][]PhoneticIndex;       // 音標索引檔索引陣列
        private char[] PhoneticSymbols1 ={'あ','い','う','え','お',
										   'か','き','く','け','こ',
										   'さ','し','す','せ','そ',

										   'た','ち','つ','て','と',
										   'な','に','ぬ','ね','の',
										   'は','ひ','ふ','へ','ほ',

										   'ま','み','む','め','も',
										   'や','ゆ','よ',
										   'ら','り','る','れ','ろ',

										   'わ','を',
									   
										   'ア','イ','ウ','エ','オ',
										   'カ','キ','ク','ケ','コ',
										   'サ','シ','ス','セ','ソ',

										   'タ','チ','ツ','テ','ト',
										   'ナ','ニ','ヌ','ネ','ノ',
										   'ハ','ヒ','フ','ヘ','ホ',

										   'マ','ミ','ム','メ','モ',
										   'ヤ','ユ','ヨ',
										   'ラ','リ','ル','レ','ロ',
										   'ワ','ヲ'
									   };

        // 濁音 (平)
        //double [][]PhoneticIndex2;  
        private string P2 = "がぎぐげござじずぜぞだぢづでどばびぶべぼ";
        // 濁音 (片)
        private string P3 = "ガギダゲゴザジズゼゾダヂヅデドバビブべボ";
        private string PhoneticSymbols2;

        // 半濁音(平)
        private string P4 = "ぱぴぷぺぽ";
        //半濁音(片)
        private string P5 = "パピプペポ";
        private string PhoneticSymbols3;

        //拗音(平)
        private string P6 = "きゃ きゅ きょ しゃ しゅ しょ ちゃ ちゅ ちょ にゃ にゅ にょ ひゃ ひゅ ひょ みゃ みゅ みょ りゃ りゅ りょ ぎゃ ぎゅ ぎょ じゃ じゅ じょ びゃ びゅ びょ ぴゃ ぴゅ ぴょ ";
        //拗音(片)
        private string P7 = "キャ キュ キョ シャ シュ ショ チャ チュ チョ ニャ ニュ ニョ ヒャ ヒュ ヒョ ミャ ミュ ミョ リャ リュ リョ ギャ ギュ ギョ ジャ ジュ ジョ ビャ ビュ ビョ ピャ ピュ ピョ ";
        private string PhoneticSymbols4;
		

		private System.Windows.Forms.Label label2;
		// end of 字母符號定義

		Random autoRand; // 亂數物件
		int [] curSymbolIndex=new int[1];
		private System.Windows.Forms.Button button4; // 目前亂數所選擇的字位置

		private Audio ourAudio = null; // 播放語音專用物件
		private AVPlayer myPlayer=null; // 父物件 [reference] 用來儲存參數

		string strPhoneticDirectory;
		string strPhoneticFilename;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.CheckBox checkBox5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox checkBox6;
		private System.Windows.Forms.CheckBox checkBox7;
		bool bPhoneticAudioReady=false;
		

		public JapanPhoneticSymolGame_From(AVPlayer myPlayer) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.myPlayer=myPlayer;
			// 設定 ToolTips
			ToolTip toolTip1 = new ToolTip();
			toolTip1.AutoPopDelay = 5000;// Set up the delays for the ToolTip.
			toolTip1.InitialDelay = 1000;
			toolTip1.ReshowDelay = 500;
			
			toolTip1.ShowAlways = true;// Force the ToolTip text to be displayed whether or not the form is active.
      
			// Set up the ToolTip text for the Button and Checkbox.
			toolTip1.SetToolTip(this.button1, "播放解答");
			toolTip1.SetToolTip(this.button2, "下一個字");
			toolTip1.SetToolTip(this.button3, "離開");
			toolTip1.SetToolTip(this.button4, "載入自行定義的語音播放檔(索引檔為=filename+.txt)");
			// end of ToolTips

			// 建立亂數物件
			autoRand = new Random( );
			PhoneticSymbols2=P2+P3; // 濁音 符號表
			PhoneticSymbols3=P4+P5; // 半濁音 符號表
			PhoneticSymbols4=P6+P7; // 拗音 符號表
			
			// 取出第一個亂數字母
			Perm_init(); // 產生亂數排列陣列
			ShowRandomSymbol(curSymbolIndex);

			// 如果現在 myPlayer 正在播放, 則必須暫停
			
			if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Playing==true){
				if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Disposed!=true){
					// myPlayer.ourAudio.Pause();
					myPlayer.PauseOrPlay();
				}
			}
			
			// end of 暫停 myPlayer

			// 設定音標檔預設路徑與檔名
			// strPhoneticDirectory=(string)myPlayer.PropertyTable["音標檔預設目錄"];
            strPhoneticDirectory = (string)myPlayer.ExecutationFileDir;
			strPhoneticFilename=(string)myPlayer.PropertyTable["音標檔名稱"];// "PhoneticAudio.wma";

            

			bPhoneticAudioReady=Phonetic_AudioReady();
			if(bPhoneticAudioReady!=true){
				string message="因為著作權的關係,\n1. 你必須要有自己的日語語音聲音檔: filename.xxx\n2. 自行指定各符號的唸法在語音檔所在的位置: filename.xxx.txt\n      起始秒數:結束秒數";
				MessageBox.Show(message);
				this.button1.Enabled=false;
			}

			if(bPhoneticAudioReady==true){
				bool bOk=ReadPhoneticIndex(); // 載入索引檔
				if(bOk!=true){
					string message="語音檔格式錯誤";
					MessageBox.Show(message);
					this.button1.Enabled=false;
				}
			}
			
			// end of 音標檔 資料
			this.label2.Text="現在請試試看發音, 是否正確!!";

			//this.label2.Text="";
		}

		
		private char ShowRandomSymbol(int[] SymbolIndex){
			string [] retClass4Symbol=new string[1];
			char Symbol=getRandomSymbol(SymbolIndex,retClass4Symbol);
			if(SymbolClass==3){
				this.label1.Text=retClass4Symbol[0];
			}
			else{
				this.label1.Text=" "+Symbol;
			}
			return Symbol;
		}


		int r1=0,r2=0,r3=0,r4=0;
		int SymbolClass;
		private char getRandomSymbol(int []Index,string[] retClass4Symbol){
			
			bool bSymbolReSelect;
			// 如果使用者全部都不選, 則預設情況為 清音
			if(checkBox2.Checked!=true && checkBox3.Checked!=true && checkBox4.Checked!=true&&checkBox5.Checked!=true){
				checkBox2.Checked=true;
			}
			do{
				bSymbolReSelect=false;
				SymbolClass=this.autoRand.Next(4); // 選擇四類符號的種類
				switch(SymbolClass){
					case 0:// 檢查是否清音被允許
						if(checkBox2.Checked!=true)
							bSymbolReSelect=true; // 重新再選一個 symbol
						break;
					case 1:// 檢查是否濁音被允許
						if(checkBox3.Checked!=true)
							bSymbolReSelect=true; // 重新再選一個 symbol
						break;
					case 2:// 檢查是否半濁音被允許
						if(checkBox4.Checked!=true)
							bSymbolReSelect=true; // 重新再選一個 symbol
						break;
					case 3:// 檢查是否拗音被允許
						if(checkBox5.Checked!=true)
							bSymbolReSelect=true; // 重新再選一個 symbol
						break;
				}
				
			}while(bSymbolReSelect);

			//SymbolClass=3;
			
			/*
			if(this.checkBox1.Checked!=true){ // 若使用者選擇不要全部 [Default], 則只播放清音的部分
				SymbolClass=0;
			}
			*/


			//SymbolClass=3;
			int index;
			
			int[] RandIndexClass;
			
			int Length;
			char c;
			switch(SymbolClass){
				case 0: // 清音
					RandIndexClass=RandIndex;
					Length=PhoneticSymbols1.Length;
					
					
					index=RandIndexClass[r1];
					r1=(r1+1)%Length;

					Index[0]=index;
					c=PhoneticSymbols1[index];
				break;
				case 1: // 濁音
					RandIndexClass=RandIndex2;
					Length=PhoneticSymbols2.Length;
					
					
					index=RandIndexClass[r2]; // 選擇的符號位置
					r2=(r2+1)%Length;
				
					c=PhoneticSymbols2[index]; // 取出符號
					Index[0]=index; //  傳回給語音播放資訊
					
					break;

				case 2: // 半濁音
					RandIndexClass=RandIndex3;
					Length=PhoneticSymbols3.Length;
					
					
					index=RandIndexClass[r3];
					r3=(r3+1)%Length;

					c=PhoneticSymbols3[index]; // 取出符號
					Index[0]=index; //  傳回給語音播放資訊
					break;

				case 3:
					RandIndexClass=RandIndex4;
					Length=PhoneticSymbols4.Length/3;
					
					
					index=RandIndexClass[r4]; // 選擇的符號位置
					r4=(r4+1)%Length;
		
					//index=34;
					//index=1;
					int s=index*3;
					retClass4Symbol[0]=PhoneticSymbols4.Substring(s,2);

					c='a';
					Index[0]=index; //  傳回給語音播放資訊
					

					break;
					
				default:
					RandIndexClass=RandIndex;
					Length=PhoneticSymbols1.Length;
					
					
					index=RandIndexClass[r1];
					r1=(r1+1)%Length;
					Index[0]=index;
					c=PhoneticSymbols1[index];
					break;
			}
			return c;
			
			
			
/*
			index=RandIndex[ri];
			ri=(ri+1)%PhoneticSymbols1.Length;

			Index[0]=index;
			return PhoneticSymbols1[index];
			*/
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(JapanPhoneticSymolGame_From));
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.button4 = new System.Windows.Forms.Button();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox3 = new System.Windows.Forms.CheckBox();
			this.checkBox4 = new System.Windows.Forms.CheckBox();
			this.checkBox5 = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkBox6 = new System.Windows.Forms.CheckBox();
			this.checkBox7 = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("DFKai-SB", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(136)));
			this.label1.Location = new System.Drawing.Point(16, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(240, 96);
			this.label1.TabIndex = 0;
			this.label1.Text = "";
			// 
			// button1
			// 
			this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
			this.button1.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.button1.Location = new System.Drawing.Point(296, 8);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(40, 40);
			this.button1.TabIndex = 1;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			this.button1.MouseEnter += new System.EventHandler(this.button1_MouseEnter);
			this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button1_MouseDown);
			// 
			// button2
			// 
			this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
			this.button2.Location = new System.Drawing.Point(280, 48);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(56, 40);
			this.button2.TabIndex = 2;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			this.button2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button2_MouseDown);
			// 
			// button3
			// 
			this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
			this.button3.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.button3.Location = new System.Drawing.Point(256, 216);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(80, 48);
			this.button3.TabIndex = 3;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("DFKai-SB", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(136)));
			this.label2.Location = new System.Drawing.Point(16, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(152, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "這個字怎麼唸?";
			// 
			// button4
			// 
			this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
			this.button4.Location = new System.Drawing.Point(0, 192);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(24, 16);
			this.button4.TabIndex = 5;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// checkBox2
			// 
			this.checkBox2.Checked = true;
			this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox2.Location = new System.Drawing.Point(8, 16);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(56, 24);
			this.checkBox2.TabIndex = 7;
			this.checkBox2.Text = "清音";
			// 
			// checkBox3
			// 
			this.checkBox3.Location = new System.Drawing.Point(56, 16);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new System.Drawing.Size(56, 24);
			this.checkBox3.TabIndex = 8;
			this.checkBox3.Text = "濁音";
			// 
			// checkBox4
			// 
			this.checkBox4.Location = new System.Drawing.Point(104, 16);
			this.checkBox4.Name = "checkBox4";
			this.checkBox4.Size = new System.Drawing.Size(64, 24);
			this.checkBox4.TabIndex = 9;
			this.checkBox4.Text = "半濁音";
			// 
			// checkBox5
			// 
			this.checkBox5.Location = new System.Drawing.Point(176, 16);
			this.checkBox5.Name = "checkBox5";
			this.checkBox5.Size = new System.Drawing.Size(56, 24);
			this.checkBox5.TabIndex = 10;
			this.checkBox5.Text = "拗音";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkBox5);
			this.groupBox1.Controls.Add(this.checkBox4);
			this.groupBox1.Controls.Add(this.checkBox3);
			this.groupBox1.Controls.Add(this.checkBox2);
			this.groupBox1.Location = new System.Drawing.Point(8, 216);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(240, 48);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "字母分類";
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(288, 88);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(56, 24);
			this.checkBox1.TabIndex = 11;
			this.checkBox1.Text = "所有";
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(272, 184);
			this.groupBox2.TabIndex = 12;
			this.groupBox2.TabStop = false;
			// 
			// checkBox6
			// 
			this.checkBox6.Location = new System.Drawing.Point(288, 112);
			this.checkBox6.Name = "checkBox6";
			this.checkBox6.Size = new System.Drawing.Size(56, 24);
			this.checkBox6.TabIndex = 13;
			this.checkBox6.Text = "順序";
			this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
			// 
			// checkBox7
			// 
			this.checkBox7.Location = new System.Drawing.Point(288, 136);
			this.checkBox7.Name = "checkBox7";
			this.checkBox7.Size = new System.Drawing.Size(56, 24);
			this.checkBox7.TabIndex = 14;
			this.checkBox7.Text = "發音";
			// 
			// JapanPhoneticSymolGame_From
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 15);
			this.ClientSize = new System.Drawing.Size(344, 266);
			this.ControlBox = false;
			this.Controls.Add(this.checkBox7);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBox6);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "JapanPhoneticSymolGame_From";
			this.Text = "認識日文字母工具(亂數字母練習)";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.JapanPhoneticSymolGame_From_Closing);
			this.Load += new System.EventHandler(this.JapanPhoneticSymolGame_From_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void JapanPhoneticSymolGame_From_Load(object sender, System.EventArgs e) {
		
		}

		// 關閉視窗
		private void button3_Click(object sender, System.EventArgs e) {
			this.button3.Enabled=false;
			if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Disposed!=true){
				if(this.myPlayer.ourAudio.Paused==true){
					// this.myPlayer.ourAudio.Play();
					this.myPlayer.PauseOrPlay();
				}
			}
			CloseGame();
		}

		private void CloseGame(){
			//ourAudio.Dispose();
			if(ourAudio!=null && ourAudio.Disposed!=true){
				if(ourAudio.Playing==true){
					ourAudio.Stop();
				}
				ourAudio.Dispose();
				ourAudio=null; // 避免 Audio 已經被 Dispose 後, 其他程式繼續使用
			}
			OpacityUtility.FadeOut_Close(this,30);
		}

		

		// 播放指定的語音
		// 注意: 因為著作權的關係, 
		//      1. 你必須要有自己的日語語音聲音檔: filename.xxx
		//		2. 自行指定各符號的唸法在語音檔所在的位置: filename.xxx_index.txt
		//              起始秒數:結束秒數
		string strPhoneticIndexFilename;
		private bool Phonetic_AudioReady(){
			// 判斷音標檔是否存在
			string strPhoneticFullFilename=strPhoneticDirectory+"\\"+strPhoneticFilename;
			strPhoneticIndexFilename=strPhoneticDirectory+"\\"+strPhoneticFilename+"_index.txt";

			if(System.IO.File.Exists(strPhoneticFullFilename) && System.IO.File.Exists(strPhoneticIndexFilename)){
				ourAudio = new Audio(strPhoneticFullFilename); // 載入音標檔
				ourAudio.Ending += new System.EventHandler(this.ClipEnded); // 設定當播放結尾時, 要如何處理 (應該不會發生)
				// ourAudio.Play();
				return true;
			}else{
				if(System.IO.File.Exists(strPhoneticFullFilename) !=true){
					MessageBox.Show("音標檔不存在: "+strPhoneticFullFilename+ "\n\n看看是不是漏掉了?");
				}

				if(System.IO.File.Exists(strPhoneticIndexFilename) !=true){
					MessageBox.Show("索引檔不存在: "+strPhoneticIndexFilename+"\n\n索引檔格式:  filename.xxx_index.txt");	
				}
				return false;
			}
		}
		
		int VoiceIndexNum=0; // 實際由檔案中讀到的語音資料
		private bool ReadPhoneticIndex(){
			// 設定記憶體
			int TotalVoiceNum=PhoneticSymbols1.Length/2+P2.Length+P4.Length+P6.Length;
			PhoneticIndex=new double[TotalVoiceNum][];
			for(int i=0;i<TotalVoiceNum;i++){
				PhoneticIndex[i]=new double[2];
			}
			// end of 記憶體

			// 由檔案中, 載入 property
			try {
				//string FullIndexFilename=strPhoneticDirectory+"//"+strPhoneticFilename+".txt";
				using (System.IO.StreamReader sr = new System.IO.StreamReader(strPhoneticIndexFilename)) {
					String line;
					
					int i=0;
					while ((line = sr.ReadLine()) != null) {
						// 跳過註解
						if(line.StartsWith("//") || line.StartsWith(" ") ||line.Length<3)
							continue;
						// end of 註解
						

						int StartIndex=line.LastIndexOf(":");
						string strStart=line.Substring(0,StartIndex);// 取出起始秒數
						string strEnd=line.Substring(StartIndex+1);// 結束秒數

						PhoneticIndex[i][0]=Double.Parse(strStart);
						PhoneticIndex[i][1]=Double.Parse(strEnd);
						i++;
						VoiceIndexNum++;
					}
				}
				return true;
			}
			catch (Exception e) {
				Console.WriteLine("索引檔格式不正確");
				Console.WriteLine(e.Message);
				return false;
			}
			
		}

		private void PlaySymbol(int index){
			
			int ClassstartBase=0;

			if(bPhoneticAudioReady!=true){
				string message="因為著作權的關係,\n1. 你必須要有自己的日語語音聲音檔: filename.xxx\n2. 自行指定各符號的唸法在語音檔所在的位置: filename.xxx.txt\n      起始秒數:結束秒數";
				MessageBox.Show(message);
			}else{
				
				switch(SymbolClass){
					case 0:
						ClassstartBase=0;
						index=index%(PhoneticSymbols1.Length/2);
						break;
					case 1:
						ClassstartBase=PhoneticSymbols1.Length/2; // (濁音) 在清音的下面
						index=index%(PhoneticSymbols2.Length/2);
						break;
					case 2:
						ClassstartBase=PhoneticSymbols1.Length/2+P2.Length; // 半濁音
						index=index%(PhoneticSymbols3.Length/2);
						break;
					case 3:
						ClassstartBase=PhoneticSymbols1.Length/2+P2.Length+P4.Length; // 拗音
						index=index%(P6.Length/3);
						break;
					default:
						break;
				}
				index=index+ClassstartBase;

				if(index < VoiceIndexNum){
					double start=PhoneticIndex[index][0];   // 10.5 sec
					double end=PhoneticIndex[index][1];

					double StartSeek=start*10000000;
					double EndSeek=end*10000000;
				
					ourAudio.SeekCurrentPosition(StartSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
					ourAudio.SeekStopPosition(EndSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
					
					// 直接索引到 start, 避免無聲情況發生 07/11/2005
					ourAudio.CurrentPosition=start;
					// end of 避免無聲情況發生

					ourAudio.Play();
				}else{
					System.Windows.Forms.MessageBox.Show("這個字的聲音片段尚未指定\n索引檔=PhoneticAudio.wma123_index.txt");
				}
			}
		}
		
		// 指定音標檔名
		private void button4_Click(object sender, System.EventArgs e) {
			System.Windows.Forms.OpenFileDialog ofdOpen=new System.Windows.Forms.OpenFileDialog();
            ofdOpen.Filter = AVPlayer.filterText;
			ofdOpen.Title = "請開啟音標檔";
			ofdOpen.ShowDialog(this);
			ofdOpen.InitialDirectory=(string)myPlayer.PropertyTable["音標檔預設目錄"];
            

			if ((ofdOpen.FileName != null) && (ofdOpen.FileName != string.Empty)) {
				// 預設語音目錄的部分 (將目前使用者指定的音標檔目錄記起來)
				strPhoneticDirectory=ofdOpen.FileName.Substring(0,ofdOpen.FileName.LastIndexOf("\\"));// 剖析目錄
				myPlayer.PropertyTable["音標檔預設目錄"]=strPhoneticDirectory;   // Update 目前使用者載入目錄
				myPlayer.PropertyTable["音標檔名稱"]=ofdOpen.FileName.Substring(ofdOpen.FileName.LastIndexOf("\\")+1);
				myPlayer.SaveProperty();// 自動存檔
				// end of 參數處理

				if(System.IO.File.Exists(strPhoneticDirectory+"\\"+strPhoneticFilename)!=true){
					string shortName=ofdOpen.FileName.Substring(ofdOpen.FileName.LastIndexOf("\\")+1);
					
					string message="請建立音標位置檔\n\n"+ 
						"檔名: "+ shortName+".txt\n\n"+ 
						"格式:  起始秒數:結束秒數\n\n"+
						"       細節請看範例: PhoneticAudio.wma.txt\n\n\n"+
						"1. 將這個檔案放在你剛剛指定的語音檔相同的位置\n\n"+
						"2. 重新執行日文字母發音工具\n\n"+
						"3. 這樣就會有語音解答了\n\n\n"+
						" 現在語音解答的功能暫時關閉"; 
							   
					MessageBox.Show(message);
				}
				//this.Close();
			}

		}
		private void ClipEnded(object sender, System.EventArgs e) {
			if(ourAudio!=null && ourAudio.Disposed!=true){
				if(ourAudio.Playing==true){
					// 可能是因為 AudioVideoPlayback 的啟動太慢, 會來不及發音,導致有時候使用者可能會以為沒有發音
					// 修正: 改成循環播放空白語音段看看是否能解決這個問題
					//ourAudio.Stop(); 

					int start=70; // 空白起始點
					int end=72;   // 空白結束點
					double StartSeek=start*10000000;
					double EndSeek=end*10000000;
				
					ourAudio.SeekCurrentPosition(StartSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
					ourAudio.SeekStopPosition(EndSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
				}
			}

			//button1.Enabled=true;
			//button2.Enabled=true;
		}

		private void button1_Click(object sender, System.EventArgs e) {
			/*
				if(ourAudio.Playing==true)
					ourAudio.Stop();
				int SymbolIndex=curSymbolIndex[0]%(PhoneticSymbols1.Length/2);
				this.Text="索引"+SymbolIndex;
			
				PlaySymbol(SymbolIndex);
				*/
		}

		private void JapanPhoneticSymolGame_From_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			//CloseGame();
		}


		// 吸引視窗主要功能
		public DockUtility myDockHelp=new DockUtility(30); // 吸引視窗公用物件
		protected override void WndProc(ref Message m) {
			int[] NewLoc=new int[2];
			System.Drawing.Rectangle SelfBound=this.Bounds;
			System.Drawing.Rectangle MotherBound=this.myPlayer.Bounds;
			myDockHelp.WndProc(ref m,SelfBound,MotherBound,NewLoc,this);

			base.WndProc(ref m);
		}

		private void button1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			PlaySymbol();
			//PlaySymbol();
		}

		private void PlaySymbol(){
			if(ourAudio!=null && ourAudio.Disposed!=true){
				/*
				if(ourAudio.Playing==true)
					ourAudio.Stop();
				*/

				// int SymbolIndex=curSymbolIndex[0]%(PhoneticSymbols1.Length/2);
				//this.Text="索引"+SymbolIndex;
			
				PlaySymbol(curSymbolIndex[0]);
			}
		}

		private void button2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			ShowRandomSymbol(curSymbolIndex);
			if(ourAudio!=null && ourAudio.Disposed!=true){
				this.label2.Text="這個字怎麼唸?";

				// 如果使用者有設定聽聲音
				if(this.checkBox7.Checked==true){
					PlaySymbol();
					//PlaySymbol();
				}
				// end of 聽聲音
			}
		}
		// end of 吸引視窗

		int[] RandIndex;
		int[] RandIndex2, RandIndex3,RandIndex4;
		private void Perm_init(){
			Perm_init(true);
		}
		private void Perm_init(bool bRandom){

			// 清音
			//RandIndex=null;
			if(RandIndex==null)
				RandIndex=new int[PhoneticSymbols1.Length];
			for(int i=0;i<PhoneticSymbols1.Length;i++){
				RandIndex[i]=i;
			}
 
			// 濁音 
			//RandIndex2=null;
			if(RandIndex2==null)
				RandIndex2=new int[PhoneticSymbols2.Length];
			for(int i=0;i<PhoneticSymbols2.Length;i++){
				RandIndex2[i]=i;
			}

			// 半濁音
			//RandIndex3=null;
			if(RandIndex3==null)
				RandIndex3=new int[PhoneticSymbols3.Length];
			for(int i=0;i<PhoneticSymbols3.Length;i++){
				RandIndex3[i]=i;
			}

			//拗音
			//RandIndex4=null;
			if(RandIndex4==null)
				RandIndex4=new int[PhoneticSymbols4.Length/3];
			for(int i=0;i<PhoneticSymbols4.Length/3;i++){
				RandIndex4[i]=i;
			}
			
			if(bRandom)
				Permutation();
		}

		private void Permutation(){
			int r=100*PhoneticSymbols1.Length;
			for(int i=0;i<r;i++){
				int swIndex,t;

				int SymbolLength;
				int[] targetIndex;

				// 清音
				swIndex=this.autoRand.Next(PhoneticSymbols1.Length);
				t=RandIndex[swIndex];
				RandIndex[swIndex]=RandIndex[(swIndex+1)%PhoneticSymbols1.Length];
				RandIndex[(swIndex+1)%PhoneticSymbols1.Length]=t;

				// 濁音 
				SymbolLength=PhoneticSymbols2.Length;
				targetIndex=RandIndex2;

				swIndex=this.autoRand.Next(SymbolLength);
				t=targetIndex[swIndex];
				targetIndex[swIndex]=targetIndex[(swIndex+1)%SymbolLength];
				targetIndex[(swIndex+1)%SymbolLength]=t;


				// 半濁音
				SymbolLength=PhoneticSymbols3.Length;
				targetIndex=RandIndex3;

				swIndex=this.autoRand.Next(SymbolLength);
				t=targetIndex[swIndex];
				targetIndex[swIndex]=targetIndex[(swIndex+1)%SymbolLength];
				targetIndex[(swIndex+1)%SymbolLength]=t;
				//拗音
				SymbolLength=PhoneticSymbols4.Length/3;
				targetIndex=RandIndex4;

				swIndex=this.autoRand.Next(SymbolLength);
				t=targetIndex[swIndex];
				targetIndex[swIndex]=targetIndex[(swIndex+1)%SymbolLength];
				targetIndex[(swIndex+1)%SymbolLength]=t;
			}
		}

		private void button1_MouseEnter(object sender, System.EventArgs e) {
			PlaySymbol();
			//PlaySymbol();
		}

		private void button2_Click(object sender, System.EventArgs e) {
		
		}


		private void checkBox1_CheckedChanged_1(object sender, System.EventArgs e) {
			// 若[全部]目前被設定, 則把其他類別自動設定為 ture
			if(this.checkBox1.Checked==true){
				checkBox2.Checked=true;
				checkBox3.Checked=true;
				checkBox4.Checked=true;
				checkBox5.Checked=true;
			}else{
				checkBox2.Checked=true;
				checkBox3.Checked=false;
				checkBox4.Checked=false;
				checkBox5.Checked=false;
			}
		}

		private void checkBox6_CheckedChanged(object sender, System.EventArgs e) {
			// 若 [順序] 按下, 所有亂數排列全部順序排列
			if(this.checkBox6.Checked==true){
				r1=0;r2=0;r3=0;r4=0;
				this.Perm_init(false);
			}else{
				this.Perm_init(true);
			}

			ShowRandomSymbol(curSymbolIndex); // 顯示第一個字母

		}
		

	}
}
	
