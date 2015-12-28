using System;
using System.Collections;
using System.Windows.Forms;

namespace Player
{
	/// <summary>
	/// Summary description for LyricClass.
	/// </summary>
	public class LyricClass
	{
		AVPlayer myAudioPlayer; // 語音撥放器
		Form myShowPannel; // 顯示這個元件的 pannel
		public Label nextlab; // 顯示下一句的 label
		int myAudioPlayer_oldHeight=0;
		int hScrollBar1_oldTop=0;

		public bool bVisible=true;

		private bool bLyricReady; //  現在歌詞元件是否已經 Ready
		public  System.Collections.ArrayList LyricInfoArray=new System.Collections.ArrayList();// 歌詞串列
		private bool bLoadingLyric=false; // 現在是否正在載入歌詞, 為了避免 IndexListForm 因為改變而產生 Seek Audio Player 的動作

		public SwiftlyLabel mySwiftlabel; // 播放歌詞移動的元件
		public int Top;

		public LyricClass(AVPlayer AudioPlayer,int locy, Form ShowPannel){
			myAudioPlayer = AudioPlayer;
			myShowPannel=ShowPannel;
			Top=locy;

			
		}

		public void Prepare(){
			// 跑馬燈元件測試  (字幕機使用)
			bLyricReady=ReadLyricFile();
			if(bLyricReady){
				/*
				parent.SetStyle(ControlStyles.DoubleBuffer | 
					ControlStyles.UserPaint | 
					ControlStyles.AllPaintingInWmPaint
					,true);	
					*/

				// 1. 建立跑馬燈
				mySwiftlabel=new SwiftlyLabel(myAudioPlayer.Width);
				mySwiftlabel.setInfoLabel(myAudioPlayer.label2);
				mySwiftlabel.setMyLyricer(this);
				mySwiftlabel.Top=Top; // 設定高度為 label3 的位置

				myAudioPlayer_oldHeight=myAudioPlayer.Height;
				hScrollBar1_oldTop=myAudioPlayer.hScrollBar1.Top;
				Visible_Enable();
				

				PlugIn(myShowPannel); // 將元件裝在顯示器上
				// end of 跑馬燈元件測試

				// 字幕專用計時器
				LyricTimer myTimer2=new LyricTimer(myAudioPlayer,this);
				myTimer2.Tick += new EventHandler(TimerEventProcessor2);			
				myTimer2.Interval = 10; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
				myTimer2.Start();
				// end of 字幕專用計時器

				// 加入顯示下一句的 label

				// 設定 label 的字型
				nextlab =new Label();
				// 指定字形
					nextlab.Font=new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
					nextlab.ForeColor=System.Drawing.Color.White;
				// end of 字型

				mySwiftlabel.Controls.Add(nextlab);
				nextlab.Top=nextlab.Height; // 設定為第二行
				nextlab.AutoSize=true;

			}

			

		}

		public void Resume(){
			if(this.bLyricReady){
				this.mySwiftlabel.Start();
			}
		}

		public void Pause(){
			if(this.bLyricReady){
				this.mySwiftlabel.Stop();
			}
		}

		// 重新設定顯示字幕
		public void ClearShowState(){
			for(int i=0;i<LyricInfoArray.Count ;i++){
				string[] InfoNode= (string[]) (LyricInfoArray[i]);
				InfoNode[2]="false";
			}
			this.mySwiftlabel.ClearAllLabel();
		}

		// 字幕專用 檢查事件函示
		private static void TimerEventProcessor2(Object myObject,
			EventArgs myEventArgs) {
			AVPlayer myPlayer=((LyricTimer)myObject).myPlayer;
			LyricClass myLyric=((LyricTimer)myObject).myLyric;
			// 字幕處理
			if(myPlayer.ourAudio !=null) {
				// ipos
				if(myLyric.isLyricReady()==true){
					double pos=myPlayer.ourAudio.CurrentPosition;
					for(int i=0;i<myLyric.LyricInfoArray.Count ;i++){
						string[] InfoNode= (string[]) (myLyric.LyricInfoArray[i]);
						double s=Double.Parse(InfoNode[0]);

						string strLyric=InfoNode[1]; // 讀取目前時刻的歌詞
						string nextLyric=" "; // 下一個要唱歌詞

						bool bHasShow=InfoNode[2].CompareTo("false")==0;
						double mods=s+myLyric.mySwiftlabel.Latency;

						if(pos > mods  && bHasShow){
							// 設定字幕顯示速度 (因為如果字太長, 會造成後面的字幕在演唱時出不來)
							// 讀取下一句開始的時間
							int next=i+1;
							int ut=0;
							bool bShowNow=true;
							if(next < myLyric.LyricInfoArray.Count){ // 如果有下一個歌詞
								// 計算可容許播放這個句子的時間
								string[] InfoNode2= (string[]) (myLyric.LyricInfoArray[next]);
								double nextSs=Double.Parse(InfoNode2[0]);
								
								double Dur=nextSs-s;
								// end of 容許時間

								// 顯示下一句
								nextLyric=InfoNode2[1];
								if(myLyric.bShowNext){
									myLyric.nextlab.Text=nextLyric;
								}
								// end of  顯示下一句

								// 計算每個字至少需要 t 秒顯示
								if(strLyric.Length >0){
									ut=(int)((Dur/(strLyric.Length+20))*1000);
									if(ut > 100)
										ut=100;
									myLyric.mySwiftlabel.Speed(ut);
								}
								// end of 計算

								// 計算是否馬上要顯示, 因為有可能 pos 被索引跳過好幾個句子
								if(nextSs < pos){
									bShowNow=false; // 下一個句子比目前位置早
								}

								
								
							}
							

							// end of 顯示速度
							InfoNode[2]="true";// 設定已經顯示
							if(bShowNow)
								myLyric.mySwiftlabel.AddLabel(strLyric); // 馬上顯示

							// myPlayer.label3.Text="歌詞索引"+InfoNode[0];//+" Speed="+ut;
                            string Info = "歌詞索引" + InfoNode[0];
                            MessageManager.ShowInformation(myPlayer.label3, Info, myPlayer.LabelMessage_DefaultDisplay_Time);
						}
					}
				
				}
				// end of 字幕處理
			}
		}
		// end of 檢查事件函示


		public bool isLyricReady(){
			return bLyricReady;
		}

		public bool isLoadingLyric(){
			return bLoadingLyric;
		}

		// 顯示元件的大小 與幾何位置
		public int getHeight(){
			return mySwiftlabel.Height;
		}

		public int getBottom(){
			return mySwiftlabel.Bottom;
		}
		// end of 顯示元件

		// 把自己加到父元件
		public void PlugIn(Form showPannel){
			showPannel.Controls.Add(mySwiftlabel);
			mySwiftlabel.Visible=true;
		}

		public string VisibleSwitch(){
			String Infotext;
			this.bVisible=!this.bVisible;
			if(this.bVisible==true){
				// menuItem24.Text="不顯示字幕";
				Infotext="不顯示字幕";
				Visible_Enable();
			}else{
				//menuItem24.Text="顯示字幕";
				Infotext="顯示字幕";
				Visible_Disable();
			}
			return Infotext;
		}

		public void Visible_Enable(){
			if(mySwiftlabel!=null){
				this.bVisible=true;
			
				mySwiftlabel.Visible=true;
				// 重新調整 Form 的高度與 scrollbar 的位置
				myAudioPlayer.hScrollBar1.Top=mySwiftlabel.Bottom+2;
				myAudioPlayer.Height=myAudioPlayer_oldHeight+mySwiftlabel.Height+myAudioPlayer.hScrollBar1.Height;	
				// end of 調整相關元件的位置
			}
		}

		public void Visible_Disable(){
			if(mySwiftlabel!=null){   // 只有在 跑馬燈元件被建立的情況下, 才有意義
				this.bVisible=false;
				mySwiftlabel.Visible=false;
				// 重新調整 Form 的高度與 scrollbar 的位置
				myAudioPlayer.Height=myAudioPlayer_oldHeight;
				myAudioPlayer.hScrollBar1.Top=hScrollBar1_oldTop;
				// end of 調整相關元件的位置
			}

		}

		public bool bShowNext=false; // 目前狀態是否為顯示下一句歌詞
		public void showNext(){
			if(!bShowNext){
				mySwiftlabel.Height=mySwiftlabel.Height*2;

				this.Visible_Enable();

				bShowNext=true;
			}
		}
		public void DisableShowNext(){
			if(bShowNext){
				mySwiftlabel.Height=mySwiftlabel.Height/2;
				this.Visible_Enable();
				bShowNext=false;
			}
		}

		
		// Lyric File format (字幕)
		// filename: [lyric]_filename.txt
		// content:
		// [second] text
		private bool ReadLyricFile(){
			string curAudioFullFileName=myAudioPlayer.ofdOpen.FileName;
			string curAudioFileName=curAudioFullFileName.Substring(curAudioFullFileName.LastIndexOf("\\")+1);
			string strFullLyricFileName=myAudioPlayer.ExecutationFileDir+"\\[lyric]_"+curAudioFileName+".txt";

			IndexListForm MyIndexList=myAudioPlayer.MyIndexList;
			// 由檔案中, 載入歌詞
			try {
				using (System.IO.StreamReader sr = new System.IO.StreamReader(strFullLyricFileName)) {
					
					MyIndexList.ClearList(false);

					String line;
					bLoadingLyric=true;
					while ((line = sr.ReadLine()) != null) {
						// 檢查是否有 顯示起始秒數
						int s=line.IndexOf("[");
						if(s!=-1){
							int e=line.IndexOf("]");
							string strNum=line.Substring(s+1,e-s-1);
							MyIndexList.AddItem(strNum);

							string strLyric=line.Substring(e-s+1);
							
							//mySwiftlabel.AddLabel(strLyric);

							// 加入資訊串列
							string[] InfoNode=new String[3];
							InfoNode[0]=strNum; // 起使時間
							InfoNode[1]=strLyric; // 歌詞
							InfoNode[2]="false"; // 是否已經顯示 (剛剛加進來, 所以一定沒有顯示)
							
							LyricInfoArray.Add(InfoNode);
						}
						
					}
					MyIndexList.InitialSelect();

					bLoadingLyric=false; // 標記結束載入歌詞 (與 MyIndexList 相關)
					//label3.Text=label3.Text+" (有歌詞)";
					
					return true;
				}
			}
			catch (Exception e) {
				// Let the user know what went wrong.
				//Console.WriteLine("The file could not be read:");
				//Console.WriteLine(e.Message);
				return false;
			}
		
		}
	}

	class LyricTimer:System.Windows.Forms.Timer {
		public LyricClass myLyric;
		public AVPlayer myPlayer;
		public LyricTimer(AVPlayer showPannel,LyricClass myLyric) {
			myPlayer=showPannel;
			this.myLyric=myLyric;
		}
	}
}
