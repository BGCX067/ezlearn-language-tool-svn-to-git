using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace Player
{
	/// <summary>
	/// Summary description for PhoneticSymbolForm.
	/// </summary>
	public class PhoneticSymbolForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private int LabelNum=5;
		private System.Windows.Forms.Label[] PhoneticSymbol=null;

		PhoneticAudioUtility symbolVoice; // 日語發音元件

		public AVPlayer myPlayer=null;

        string[] symbols ={   "あ（安） い（以） う（宇） え（衣） お（于）",
								  "ア（阝） イ（亻） ウ（宀） エ（工） オ（方)",

								  "か（加） き（幾） く（久） け（計） こ（己）",
								  "カ（力） キ（幾） ク（久） ケ（介） コ（己）",


								  "さ（左） し（之） す（寸） せ（世） そ（曾）",
								  "サ（散） シ（之） ス（须） セ（世） ソ（曾）",     

								  "た（太） ち（知） つ（川） て（天） と（止）", 
								  "タ（多） チ（千） ツ（川） テ（天） ト（止）", 

								  "な（奈） に（仁） ぬ（奴） ね（檷） の（乃）",
								  "ナ（奈） ニ（仁） ヌ（奴） ネ（礻） ノ（奈）", 

								  "は（波） ひ（比） ふ（布） へ（部） ほ (保)",
								  "ハ（八） ヒ（比） フ（不） ヘ（部） ホ（保）", 

								  "ま（末） み（美） む（武） め（女） も（毛）",
								  "マ（末） ミ（三） ム（牟） メ（女） モ（毛）", 

								  "や（也） ゆ（由） よ（與）",
								  "ヤ（也） ユ（由） ヨ（与）", 

								  "ら（良） り（利） る（留） れ（礼） ろ（吕）",
								  "ラ（良） リ（刂） ル（流） レ（礼） ロ（呂）", 

								  "わ（和） を（遠）", 
								  "ワ（和） ヲ（乎）"};



        string[] symbol2 ={   "あ（　） い（　） う（　） え（　） お（　）",
								  "ア（　） イ（　） ウ（　） エ（　） オ（　)",

								  "か（　） き（　） く（　） け（　） こ（　）",
								  "カ（　） キ（　） ク（　） ケ（　） コ（　）",


								  "さ（　） し（　） す（　） せ（　） そ（　）",
								  "サ（　） シ（　） ス（　） セ（　） ソ（　）",     

								  "た（　） ち（　） つ（　） て（　） と（　）", 
								  "タ（　） チ（　） ツ（　） テ（　） ト（　）", 

								  "な（　） に（　） ぬ（　） ね（　） の（　）",
								  "ナ（　） ニ（　） ヌ（　） ネ（　） ノ（　）", 

								  "は（　） ひ（　） ふ（　） へ（　） ほ (　)",
								  "ハ（　） ヒ（　） フ（　） ヘ（　） ホ（　）", 

								  "ま（　） み（　） む（　） め（　） も（　）",
								  "マ（　） ミ（　） ム（　） メ（　） モ（　）", 

								  "や（　） ゆ（　） よ（　）",
								  "ヤ（　） ユ（　） ヨ（　）", 

								  "ら（　） り（　） る（　） れ（　） ろ（　）",
								  "ラ（　） リ（　） ル（　） レ（　） ロ（　）", 

								  "わ（　） を（　）", 
								  "ワ（　） ヲ（　）"};


		public void setPlayerControl(AVPlayer myPlayer){
			this.myPlayer=myPlayer;
		}
        private bool old_Player_WindowOrder;
		public PhoneticSymbolForm(AVPlayer myPlayer)
		{
			setPlayerControl(myPlayer);

            // 先把播放視窗的 Z-order 調整回原來的樣子, 等到關閉後, 再調整回去.
            old_Player_WindowOrder = myPlayer.TopMost;
            myPlayer.TopMost = false; 

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

            /* 列出所有字形
            FontFamily[] Families=FontFamily.Families;
            for(int i=0;i<Families.Length;i++){
                string k=Families.ToString();
            }
           
            string []hansymbols={   "安以宇衣于",
                                    "工方",

                                    "加幾久計己",
                                    "力幾久介己",

                                    "左之寸世曾",
                                    "散之世曾",

                                    "太知川天止",
                                    "多千川天止",

                                    "奈仁奴檷乃",
                                    "奈仁奴奈",

                                    "波比布部保",
                                    "八比不部保",
                                    "末美武女毛",
                                    "末三牟女毛",

                                    "也由與",
                                    "也由与",

                                    "良利留",
                                    "良流呂",

                                    "和遠",
                                    "和乎"
                                };
            
             */
            		  
							

			LabelNum=symbols.Length;
            ShowSymbol(symbols); // 顯示日文字母 (預設顯示漢字輔助)

			// 加入日文符號語音工具
			
			if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Playing==true){// 如果現在 myPlayer 正在播放, 則必須暫停
				if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Disposed!=true){
					// myPlayer.ourAudio.Pause();
					this.myPlayer.PauseOrPlay();
                    
				}
			}// end of 暫停 myPlayer
			

			// string strPhoneticDirectory=(string)myPlayer.PropertyTable["音標檔預設目錄"];
            string strPhoneticDirectory = (string)myPlayer.ExecutationFileDir;
			string strPhoneticFilename=(string)myPlayer.PropertyTable["音標檔名稱"];// "PhoneticAudio.wma";
			symbolVoice=new PhoneticAudioUtility(strPhoneticDirectory,strPhoneticFilename);

			// 播放清音測試
			// symbolVoice.PlaySymbol(0,1); // ''

			
			// end of 日文符號語音工具
		}

        private void ShowSymbol(string [] SymbolStringArray) {
            bool bFirstRun=false;
            if (PhoneticSymbol == null) {
                bFirstRun = true;
                PhoneticSymbol = new Label[SymbolStringArray.Length];
            }

            int k_even = 0;
            int y = 0, x = 0;
            for (int i = 0; i < LabelNum; i++) {
                if (bFirstRun) {
                    PhoneticSymbol[i] = new Label();
                    PhoneticSymbol[i].Font = new Font("標楷體", 24); // 24
                    PhoneticSymbol[i].AutoSize = true;
                    PhoneticSymbol[i].Name = "" + i;
                    PhoneticSymbol[i].Location = new Point(x, y);

                    y += 2 * PhoneticSymbol[i].Height;
                    this.Controls.Add(PhoneticSymbol[i]);
                    k_even++;
                    if (k_even == 2) {
                        k_even = 0;
                        y += PhoneticSymbol[i].Height / 10;
                    }

                    // 加入滑鼠事件處理
                    PhoneticSymbol[i].MouseDown += new System.Windows.Forms.MouseEventHandler(this.Symbol_MouseDown);
                    // end of 滑鼠事件處理
                } // end of first run
                
                // 指定顯示符號
                PhoneticSymbol[i].Text = SymbolStringArray[i];
            }

            if (bFirstRun) {
                // 修改顯示顏色
                // 同一種符號, 給定同一種顏色
                Color FirstSymbolLineColor = Color.Black;
                Color SecondSymbolLineColor = Color.Blue;

                for (int i = 0, index = 0; i < LabelNum; i += 2, index++) {
                    // 0-1 是同一符號
                    // 2-3 是同一個符號

                    if (index % 2 == 0) {
                        PhoneticSymbol[i].ForeColor = FirstSymbolLineColor;
                        PhoneticSymbol[i + 1].ForeColor = FirstSymbolLineColor;
                    }
                    else {
                        PhoneticSymbol[i].ForeColor = SecondSymbolLineColor;
                        PhoneticSymbol[i + 1].ForeColor = SecondSymbolLineColor;
                    }
                }
                this.ResumeLayout(true);
            }
        }

		// 滑鼠事件處理函式
		string strTitle="五十音圖 (漢字對照表) -- 按滑鼠左鍵,可聽日文假名原音; 按右鍵 ==> 切換漢字輔助模式";
		/*
		int lstart=0;
		int lend=0;
		int lindex=-1;
		*/
        int MouseLeftKeyCount = 0;
		private void Symbol_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {

            // 當使用者按下滑鼠左鍵 odd 次, 則切換無漢字輔助模式
            if (e.Button.Equals(MouseButtons.Right) == true) {
                MouseLeftKeyCount++;
                if (MouseLeftKeyCount % 2 == 1) {
                    // 左鍵按下奇數次
                    MouseLeftKeyCount = 1;
                    ShowSymbol(symbol2); // 顯示日文字母 (預設顯示漢字輔助)
                }
                else {
                    // 左鍵按下偶數次
                    MouseLeftKeyCount = 0;
                    ShowSymbol(symbols); // 顯示日文字母 (預設顯示漢字輔助)
                }
            }
			
			int y=e.Y;
			int x=e.X;
			string Name=((Label)sender).Name;
			

			int i=Int32.Parse(Name);

			int SymbolNum=5;
			int SymbolBase=i/2*SymbolNum;
			if(i==14 || i==15){
				SymbolNum=3;
			}
			if(i==16 || i==17){
				SymbolNum=5;
				SymbolBase=i/2*SymbolNum-2;
			}
			if(i==18 || i==19) {
				SymbolBase=i/2*5-2;
				SymbolNum=2;
			}

			int SymbolW=PhoneticSymbol[i].Width/SymbolNum;// 計算每個 符號在 form 上的距離
			int SymbolIndex=x/SymbolW; // 計算目前 x 所在的符號

			// 取滑鼠目前對應的符號
			string strSymbol="test";
			if(SymbolIndex*5+5 < PhoneticSymbol[i].Text.Length)
				strSymbol=PhoneticSymbol[i].Text.Substring(SymbolIndex*5,5);
			else{
				if(SymbolIndex*5 <PhoneticSymbol[i].Text.Length)
				strSymbol=PhoneticSymbol[i].Text.Substring(SymbolIndex*5);
			}
			// end of 取滑鼠對應的符號

			 this.Text=strTitle+"  字母:"+strSymbol;

			
			//int SymbolBase=i/2*SymbolNum;
			int SymbolClass=0; // 清音
			
			symbolVoice.PlaySymbol(SymbolClass,SymbolIndex+SymbolBase);
			//Thread.Sleep(100);
			symbolVoice.PlaySymbol(SymbolClass,SymbolIndex+SymbolBase);
	
		}
		// end of 滑鼠事件處理函式

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
            
            myPlayer.TopMost = old_Player_WindowOrder; 

			if(PhoneticSymbol!=null){
				for(int i=0;i<LabelNum;i++){
					PhoneticSymbol[i].Dispose();
				}
			}

			if( disposing )
			{
				if(components != null)
				{
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
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhoneticSymbolForm));
            this.SuspendLayout();
            // 
            // PhoneticSymbolForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 15);
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(712, 574);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PhoneticSymbolForm";
            this.Text = "五十音圖 (漢字對照表) -- 聽原音對照日文假名 ";
            this.Closed += new System.EventHandler(this.PhoneticSymbolForm_Closed);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.PhoneticSymbolForm_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PhoneticSymbolForm_KeyDown);
            this.Load += new System.EventHandler(this.PhoneticSymbolForm_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void PhoneticSymbolForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

			myPlayer.textBox1_KeyDown(sender,e); // 對 Player 的 textbox1 發出 KeyDown 的事件

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
			// myPlayer.textBox1.Text=myPlayer.textBox1.Text+strnum;

			if(bNum==true){
				try{
					int Num=(int)Double.Parse (myPlayer.textBox1.Text+strnum);
					this.myPlayer.textBox1.Text=""+Num;
				}catch(Exception){
				}
			}

			if(e.KeyCode==Keys.Back){
				// 刪除最右邊的數字
				string strTarget=this.myPlayer.textBox1.Text;
				if(strTarget.Length>0){
					string newString=strTarget.Substring(0,strTarget.Length-1);
					this.myPlayer.textBox1.Text=newString;
				}
			}

			// end of 模擬 text 元件的功能
			if(bNum==true || e.KeyCode==Keys.Back)
				this.Text="五十音圖 (漢字對照表) -- 聽原音對照日文假名                (索引 "+myPlayer.textBox1.Text+")";  // 讓使用者 keyin 時, 還能看到數字
			else
				this.Text="五十音圖 (漢字對照表) -- 聽原音對照日文假名                ("+myPlayer.label3.Text+")";    // 當下索引指令時, 看到系統狀態
			
		}

		private void PhoneticSymbolForm_Closed(object sender, System.EventArgs e) {

			
		}
		
		int CloseTime=0;
		private void PhoneticSymbolForm_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			// 吃掉第一次使用者按 x 的 event
			if(CloseTime==0){
				e.Cancel=true;
				CloseTime=1;
			}
			// end of  吃掉第一次使用者按 x 的 event

			OpacityUtility.FadeOut_Close(this,30); // 由程式自動產生 Close event
			

			// 恢復暫停的音訊
			if(myPlayer.ourAudio!=null && myPlayer.ourAudio.Disposed!=true){
				if(this.myPlayer.ourAudio.Paused==true){
					// this.myPlayer.ourAudio.Play();
					this.myPlayer.PauseOrPlay();
				}
			}
			// end of 恢復暫停的音訊
			
		}

		private void PhoneticSymbolForm_Load(object sender, System.EventArgs e) {
		
		}
	}
}
