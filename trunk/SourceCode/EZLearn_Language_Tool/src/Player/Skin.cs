/*
Usage:
                // Palm Skin XML 設定檔測試
				string SkinDir=this.ExecutationFileDir+"\\skin\\Palm_by_fugenet";
				mySkin=new Skin(SkinDir,this,textBox1,label2,label3,button1,PlayButton,PauseButton,IndexListButton,CloseButton,hScrollBar1,l1,label1);
				mySkin.conLoadPalmSkin();				
				// end of XML 測試
  
 Check list: 
 *   1. 若要使用 skin, 則必須要把 Form 的 Auto Size 設定為 false

				
*/

using System;
using System.Windows.Forms; // for Form
using System.Drawing; // for point, Region
using System.Drawing.Drawing2D; // for  GraphicsPath
using System.Collections; // for ArrayList

//  XML 存檔/讀檔 使用
using System.IO;// for save skin infomation 
using System.Xml;

namespace Player {
	/// <summary>
	/// Summary description for Skin.
	/// </summary>
	public class Skin {
		const bool bUseFadeIn=true;
		System.Windows.Forms.Form myForm; // 要更改 skin 的目標視窗
		// AVPlayer myForm;

		string SkinDir; // Skin 目錄
		System.Collections.Hashtable BottomImageTable=new System.Collections.Hashtable();  // 按鈕影像資料

		// 受管理的元件
		JingTextEdit1 Input;
		System.Windows.Forms.Label StatusLabel;
		System.Windows.Forms.Label FilenameLabel;
		System.Windows.Forms.Label TotalLengthLabel;

		System.Windows.Forms.HScrollBar ScrollBar;
		NumberLabel TimeLabel;


		public System.Drawing.Bitmap  combo_PlayImage;
		public System.Drawing.Bitmap  combo_PauseImage;
		System.Windows.Forms.Button PlayPauseButton;
		System.Windows.Forms.Button PlayButton;
		System.Windows.Forms.Button PauseButton;
		System.Windows.Forms.Button IndexListButton;
		System.Windows.Forms.Button  CloseButton;
		// end of 受管理的元件
		
		// 三個主要步驟: 更改  Skin 
		// Step 0: 
		//  ========================== 一個 Skin 物件對應一個 Form =====================
		public Skin(string SkinDir,AVPlayer aPlayer) {
			myForm=aPlayer;
			this.SkinDir=SkinDir;

            myForm.AutoSize = false; // 這個 Form 視窗的大小由我們來處理
		}

		// Step 1
		// ========================== 改變指定 Form 的形狀 =====================
		public void ChangeFormShape(string maskFile,byte[] BGColor){
			ChangeFormShape(maskFile,BGColor,true);
		}
		public void ChangeFormShape(string maskFile,byte[] BGColor,bool bChangeShape){
			if(bUseFadeIn)		
				myForm.Opacity=0.2;

			string fullmaskFile=SkinDir+"\\"+maskFile;
			ApplyMainSkin(fullmaskFile);   // 先載入圖形
			if(bChangeShape==true){
				//byte bR=255,bG=69,bB=245;
				byte bR,bG,bB;
				bR=BGColor[0];
				bG=BGColor[1];
				bB=BGColor[2];
				bool[][] bMask=createMask(fullmaskFile,bR,bG,bB);

				int H=bMask.Length ;
				int W=bMask[0].Length;

				myForm.Height=H+30;
				myForm.Width=W;

			
			
				Region myShape=getRegion(bMask); // 接著再改變輪廓
				myForm.Region=myShape;
			}else{
				// 沒有 shape 的情況, 其視窗大小由 BG Image 決定
				//myForm.Height=myForm.BackgroundImage.Height;
				// myForm.Width=myForm.BackgroundImage.Width;
				
				myForm.Height=136+16;
				myForm.Width=312;
			}
			
		}
		// ========================== end of 改變指定 Form 的形狀 =====================


		// Step 2
		// ================================= 更改元件的顯示影像與位置配置 ======================
		// 指定想要改變元件的位置 (W or H == -1 則表示 don't care)		
		public void ControlLoc(System.Windows.Forms.Control obj,int[] Loc,int Width,int Height){
			obj.Left=Loc[0];
			obj.Top=Loc[1];
			if(Width!=-1)
				obj.Width=Width;
			if(Height!=-1)
				obj.Height=Height;
		}
		
		// 更換指定按鈕的 skin
		public void ButtonSkin(string strName,System.Windows.Forms.Button aButton,string strNormalBMP,string strUnderBMP,int[] BottonLoc){
			aButton.Left=BottonLoc[0];
			aButton.Top=BottonLoc[1];
			aButton.Visible=true;

			System.Drawing.Bitmap[] PlayImage=(System.Drawing.Bitmap[]) BottomImageTable[aButton.GetHashCode()];
			if(PlayImage==null){
				PlayImage=new System.Drawing.Bitmap[2]; // 建立新的 Image
			}
			LoadSkinImage(strNormalBMP,strUnderBMP,PlayImage);
			BottomImageTable[aButton.GetHashCode()]=PlayImage; // Update 影像

			// 指定目前按鈕顯示影像為一般影像
			System.Drawing.Bitmap NormalForm=PlayImage[0];
			aButton.Image=NormalForm;
			aButton.Height=NormalForm.Height;
			aButton.Width=NormalForm.Width;
			aButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

			// 設定目前按鈕的 Event
			aButton.MouseEnter += new System.EventHandler(ButtonMouseEnterHandle);
			aButton.MouseLeave += new System.EventHandler(ButtonMouseLeaveHandle);
		}
		// end of 按鈕 
		// ================================= end of 更改元件的顯示影像與位置配置 ======================



		
		// Palm Skin 方便函式
		/*
				 手動使用範例 Palm Skin  測試
				string SkinDir=this.ExecutationFileDir+"\\skin\\Palm_by_fugenet";
				mySkin=new Skin(SkinDir,this);
				mySkin.conChangeSkin_PalmLoc("main.bmp",new byte[]{255,69,245},textBox1,label2,label3,button1,PlayButton,PauseButton,IndexListButton,CloseButton,hScrollBar1,l1,label1);
				
		 */
		 
		public void conChangeSkin_PalmLoc(string maskFile,byte[] BGColor,JingTextEdit1 Input,
			System.Windows.Forms.Label StatusLabel,
			System.Windows.Forms.Label FilenameLabel,
			System.Windows.Forms.Button PlayPauseButton,
			System.Windows.Forms.Button PlayButton,
			System.Windows.Forms.Button PauseButton,
			System.Windows.Forms.Button IndexListButton,
			System.Windows.Forms.Button CloseButton,
			System.Windows.Forms.HScrollBar ScrollBar,
			NumberLabel TimeLabel, 
			System.Windows.Forms.Label TotalLengthLabel){

			PlayPauseButton.Visible =false;   // Palm Skin 不需要複合型按鈕

			
	
			// 改變主視窗的形狀
			string fullMaskFile=this.SkinDir+"\\"+maskFile;
			ChangeFormShape(maskFile,BGColor);
			// end of 主視窗的形狀

			// 調整 scroll bar 的位置 (42,125,215,135)
			int[] ScrollLoc=new int[]{42,125,215,135};
			int W=ScrollLoc[2]-ScrollLoc[0];
			int H=ScrollLoc[3]-ScrollLoc[1];
			ControlLoc(ScrollBar,ScrollLoc,W,H);
			// end of scroll bar

			// 調整數字位置 
			int[] TimeLoc=new int[]{103,78};
			ControlLoc(TimeLabel,TimeLoc,-1,-1);

			int x=TimeLabel.Left+TimeLabel.Width+3;
			int y= TimeLabel.Top+5;
			ControlLoc(TotalLengthLabel,new int[]{x,y},-1,-1);
			// end of 調整數字位置
			
			// 調整狀態列的位置 
			int[] StatusLoc=new int[]{119,114};
			ControlLoc(StatusLabel,StatusLoc,-1,-1);

			// 調整檔案名稱位置  
			int[] FilenameLoc=new int[]{44,46};
			ControlLoc(FilenameLabel,FilenameLoc,-1,-1);

			// 調整輸入項位置
			int[] InputLoc=new int[]{68,192,193-68-5};
			W=InputLoc[2];
			ControlLoc(Input,InputLoc,W,-1);
			
			int[] PlayLoc=new int[]{100,147};
			ButtonSkin("Play",PlayButton,"bplayn.bmp","bplayu.bmp",PlayLoc); // 調整 Player 按鈕的位置 
		
			int[] PauseLoc=new int[]{123,146};
			ButtonSkin("Pause",PauseButton,"bpausen.bmp","bpauseu.bmp",PauseLoc);// 處理 Pause 按鈕位置與影像

			int[] IndexListLoc=new int[]{205,256};
			ButtonSkin("IndexList",IndexListButton,"btn9n.bmp","btn9u.bmp",IndexListLoc); // 處理索引紀錄簿按鈕

			int[] CloseLoc=new int[]{189,2};
			ButtonSkin("Close",CloseButton,"bexitn.bmp","bexitu.bmp",CloseLoc); // 處理關閉按鈕


            OpacityUtility myOpacityObj = new OpacityUtility();
            myOpacityObj.FadeIn_Only(myForm, 30);
		}

		// 讀取 XML 設定檔版本

		// 注意: 讀取的 XML 檔案必須要用 UTF-8 格式存檔
	
		/*
		 xml 範例:
		 <EZLearn_Skin_Layout Project_Code="EZLearn 語言學習機">
				 <MainWindow Filename="main.bmp" />
				 <Scroll_Bar left="42" top="125" right="215" bottom="135" />
				 </EZLearn_Skin_Layout >
		 */
		
		// Step 1: 建構子
		public Skin(string SkinDir,AVPlayer aPlayer,JingTextEdit1 Input,
			System.Windows.Forms.Label StatusLabel,
			System.Windows.Forms.Label FilenameLabel,
			System.Windows.Forms.Button PlayPauseButton,
			System.Windows.Forms.Button PlayButton,
			System.Windows.Forms.Button PauseButton,
			System.Windows.Forms.Button IndexListButton,
			System.Windows.Forms.Button CloseButton,
			System.Windows.Forms.HScrollBar ScrollBar,
			NumberLabel TimeLabel, 
			System.Windows.Forms.Label TotalLengthLabel){

			this.combo_PlayImage=combo_PlayImage;
			this.combo_PauseImage=combo_PauseImage;

			myForm=aPlayer;
			this.SkinDir=SkinDir;

			this.Input=Input;
			this.StatusLabel =StatusLabel;
			this.FilenameLabel=FilenameLabel;
			this.ScrollBar=ScrollBar;
			this.TimeLabel=TimeLabel;
			this.TotalLengthLabel=TotalLengthLabel;


			this.PlayPauseButton=PlayPauseButton;
			this.PlayPauseButton.Visible =false;
			this.PlayButton=PlayButton;
			this.PlayButton.Visible =false;

			this.PauseButton=PauseButton;
			this.PauseButton.Visible =false;

			this.IndexListButton=IndexListButton;
			this.IndexListButton.Visible=false;

			this.CloseButton=CloseButton;
			this.CloseButton.Visible=false;

			
		}

	
		
		public void conLoadPalmSkin(){
			string strFullSkinConfigFile=this.SkinDir+"\\skin.xml";
			LoadSkin(strFullSkinConfigFile);
		}

		public void conLoadHelloKittySkin(){
			string strFullSkinConfigFile=this.SkinDir+"\\skin.xml";
			LoadSkin(strFullSkinConfigFile);
		}

		public void LoadSkin(){
			string strFullSkinConfigFile=this.SkinDir+"\\skin.xml";
			LoadSkin(strFullSkinConfigFile);
		}


		

		public void LoadSkin(string strFullSkinConfigFile){
			XmlValidatingReader reader = null;

			try {
				XmlTextReader txtreader = new XmlTextReader(strFullSkinConfigFile);
				txtreader.WhitespaceHandling = WhitespaceHandling.None;

				//Implement the validating reader over the text reader. 
				reader = new XmlValidatingReader(txtreader);
				reader.ValidationType = ValidationType.None;

  
				//Parse the XML fragment.  If they exist, display the   
				//prefix and namespace URI of each element.
				while (reader.Read()){
					switch (reader.NodeType){
						case XmlNodeType.Element:
							Console.WriteLine("<{0}>", reader.LocalName);

							LoadSkin(reader);
							// 列出 Attribute
							int num=reader.AttributeCount;
							for(int i=0;i<num;i++){
								reader.MoveToAttribute(i);
								Console.WriteLine("    Attribute Name {0} ={1}", reader.Name,reader.Value);
							}

							// 列出 Element 
							reader.MoveToElement();
							break;
					}
				}

                if (bUseFadeIn)
                {
                    OpacityUtility myOpacityObj = new OpacityUtility();
                    myOpacityObj.FadeIn_Only(myForm, 30);
                }

			}catch(System.Xml.XmlException ee){
				Console.WriteLine("Exception "+ee);
			}

			finally {
				if (reader != null)
					reader.Close();
			}

		}

		//===========================================  XML 公用部分 =========================================
		private void LoadSkin(XmlValidatingReader reader){
			string ElementName=reader.LocalName;
			//string Normal=null,Under=null;
		    int iRight=-1,width=-1;
			string strRight=null;
			
			switch(ElementName){
				case "MainWindow":
					// 讀取 mask 檔案名稱
					string maskFile=reader["Shape"];
					string fullMaskFile=null;
					fullMaskFile=this.SkinDir+"\\"+maskFile;

					string bstrShape=reader.GetAttribute("NOSHAPE");
					if(bstrShape==null){
						// 讀取背景顏色數值
						byte[] BGColor=new byte[3];
						BGColor[0]=byte.Parse(reader["BG_R"]);
						BGColor[1]=byte.Parse(reader["BG_G"]);
						BGColor[2]=byte.Parse(reader["BG_B"]);

						ChangeFormShape(maskFile,BGColor);
					}else{
						// 沒有 shape information , 只有背景資料
						ChangeFormShape(maskFile,null,false);
					}
					break;
				case "Scroll_Bar":
					int[] ScrollLoc=new int[4];
					ScrollLoc[0]=int.Parse(reader["left"]);
					ScrollLoc[1]=int.Parse(reader["top"]);
					ScrollLoc[2]=int.Parse(reader["right"]);
					ScrollLoc[3]=int.Parse(reader["bottom"]);

					int W=ScrollLoc[2]-ScrollLoc[0];
					int H=ScrollLoc[3]-ScrollLoc[1];
					ControlLoc(ScrollBar,ScrollLoc,W,H);
					break;

				case "TimeLabel":
					int[] TimeLoc=new int[2];
					TimeLoc[0]=int.Parse(reader["x"]);
					TimeLoc[1]=int.Parse(reader["y"]);

					ControlLoc(TimeLabel,TimeLoc,-1,-1);

					int x=TimeLabel.Left+TimeLabel.Width+3;
					int y= TimeLabel.Top+5;
					ControlLoc(TotalLengthLabel,new int[]{x,y},-1,-1);
					SetLabelColor(TotalLengthLabel,reader); // 設定顏色
					break;

				case "StatusLabel":
					int[] StatusLoc=new int[2];
					StatusLoc[0]=int.Parse(reader["x"]);
					StatusLoc[1]=int.Parse(reader["y"]);
					strRight=reader["right"];

					if(strRight!=null){
						iRight=int.Parse(strRight);
					    width=iRight-StatusLoc[0];
					}else{
						width=-1;
					}
					ControlLoc(StatusLabel,StatusLoc,width,-1);

					SetLabelColor(StatusLabel,reader); // 設定顏色

					break;

				case "FilenameLabel":
					int[] FilenameLoc=new int[2];
					FilenameLoc[0]=int.Parse(reader["x"]);
					FilenameLoc[1]=int.Parse(reader["y"]);

					strRight=reader["right"];
					if(strRight!=null){
						iRight=int.Parse(strRight);
						width=iRight-FilenameLoc[0];
					}else{
						width=-1;
					}
					ControlLoc(FilenameLabel,FilenameLoc,width,-1);

					SetLabelColor(FilenameLabel,reader); // 設定顏色
					break;

				case "Input":
					int[] InputLoc=new int[3];
					InputLoc[0]=int.Parse(reader["x"]);
					InputLoc[1]=int.Parse(reader["y"]);
					InputLoc[2]=int.Parse(reader["width"]);

					W=InputLoc[2];
					ControlLoc(Input,InputLoc,W,-1);
					break;

				case "PlayPause":
					ComboxPlayPauseSkin(ElementName,PlayPauseButton,reader);
					/*
										PlayPauseButton.Visible =true;

										int[] PalyPauseLoc=new int[2];
										PalyPauseLoc[0]=int.Parse(reader["x"]);
										PalyPauseLoc[1]=int.Parse(reader["y"]);

										try{
											Normal=reader["Normal_Shape"];
											Under=reader["Under_Shape"];
											ButtonSkin(ElementName,PlayPauseButton,Normal,Under,PalyPauseLoc); // 調整 PalyPause 按鈕的位置 
										}catch(Exception){
											// 若使用者沒有指定圖, 則使用預設資料
											//PlayPauseButton.setSelfPaint();
											ControlLoc(PlayPauseButton,PalyPauseLoc,-1,-1);
										}
					*/
					break;

				case "Play":
					SetCommonButtonSkin(ElementName,PlayButton,reader);// 一般 PlayButton 按鈕
					break;
				case "Pause":
					SetCommonButtonSkin(ElementName,PauseButton,reader);// 一般 PauseButton 按鈕
					break;

				case "IndexList":
					SetCommonButtonSkin(ElementName,IndexListButton,reader);// 一般 IndexList 按鈕
					break;
				case "Close":
					SetCommonButtonSkin(ElementName,CloseButton,reader); // 一般 Close 按鈕
					break;
			}
		}
		//===========================================  end of XML 公用部分 =========================================
		
		private void SetLabelColor(Label label,XmlValidatingReader reader){
			// 顏色的設定

			string R=reader.GetAttribute("R");
			string G=reader.GetAttribute("G");
			string B=reader.GetAttribute("B");

			if(R!=null && G!=null && B!=null){
				int iR=int.Parse(R);
				int iG=int.Parse(G);
				int iB=int.Parse(B);
				label.ForeColor=System.Drawing.Color.FromArgb(iR,iG,iB);
			}
			// end of 顏色的設定
		}

		private void ComboxPlayPauseSkin(String ElementName,Button bt,XmlValidatingReader reader){
			bt.Visible =true;

			int[] Loc=new int[2];
			Loc[0]=int.Parse(reader["x"]);
			Loc[1]=int.Parse(reader["y"]);

			PlayPauseButton.Visible =true;

			int[] PalyPauseLoc=new int[2];
			PalyPauseLoc[0]=int.Parse(reader["x"]);
			PalyPauseLoc[1]=int.Parse(reader["y"]);

			

			string strCombo_PlayImage=this.SkinDir+"\\"+reader["Combo_Play"];
			string strCombo_PauseImage=this.SkinDir+"\\"+reader["Combo_Pause"];
			// 載入影像
			System.Drawing.Bitmap imgCombo_Play=new System.Drawing.Bitmap(strCombo_PlayImage);
			System.Drawing.Bitmap imgCombo_Pause=new System.Drawing.Bitmap(strCombo_PauseImage);

			// 設定 PlayPause 影像
			this.combo_PlayImage=imgCombo_Play;
			this.combo_PauseImage =imgCombo_Pause;
			bt.Image=combo_PauseImage;
			bt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			bt.Height=imgCombo_Play.Height;
			bt.Width=imgCombo_Play.Width;
				
			ControlLoc(PlayPauseButton,PalyPauseLoc,-1,-1); // 移動 Control 的位置
			
		}

		// 一般按鈕設定: 設定位置 (寬高不處理), 顯示影像, 滑鼠上空影像
		private void SetCommonButtonSkin(String ElementName,Button bt,XmlValidatingReader reader){
			bt.Visible =true;
			string Normal=null,Under=null;

			int[] Loc=new int[2];
			Loc[0]=int.Parse(reader["x"]);
			Loc[1]=int.Parse(reader["y"]);
			try{
				Normal=reader["Normal_Shape"];
				Under=reader["Under_Shape"];
				ButtonSkin(ElementName,bt,Normal,Under,Loc);  // 處理關閉按鈕
			}catch(Exception){
				// 若使用者沒有指定圖, 則使用預設資料
				//CloseButton.setSelfPaint();
				ControlLoc(bt,Loc,-1,-1); // 只處理 x,y 寬高不處理
			}
		}
		/*



		


				// 下面是公用函式

		
		
				public void SaveInfo(){
					// 寫入資料到指定的檔案中
					using (System.IO.StreamWriter sw=new System.IO.StreamWriter("c:\\test.xml")){ // 建立新的 property 檔案
						XmlTextWriter writer = new XmlTextWriter(sw);
						writer.WriteStartElement("EZLearn_Skin_Layout"); // 加入 EZLearn Skin Layout 標籤
						writer.WriteAttributeString("Project_Code", "EZLearn 語言學習機"); // 加入屬性

						writer.Formatting = Formatting.Indented;
						SaveInfo_Sub(writer, "MSFT", 74.125, 5.89, 69020000);
						writer.Close();
					}
					// end of  寫入資料

			
			
				}
		
				// 主視窗的形狀資訊寫入
				private void save_MainShape(XmlTextWriter writer,string strFullMaskFilename){
					writer.WriteStartElement("主視窗形狀"); 
					writer.WriteAttributeString("檔案位置",strFullMaskFilename);
					writer.WriteEndElement();
				}

				private void save_ControlLocYX(string ControlName,int[] Loc){
					writer.WriteStartElement(ControlName); 
					writer.WriteAttributeString("位置_y",Loc[0]);
					writer.WriteAttributeString("位置_x",Loc[1]);
					writer.WriteEndElement();
				}

				private void save_ControlLocRec(string ControlName,int[] Loc){
					writer.WriteStartElement(ControlName); 
					writer.WriteAttributeString("位置_left",Loc[0]);
					writer.WriteAttributeString("位置_top",Loc[1]);
					writer.WriteAttributeString("位置_right",Loc[2]);
					writer.WriteAttributeString("位置_bottom",Loc[3]);
					writer.WriteEndElement();
				}
		

				// 
				private void SaveInfo_Sub(XmlTextWriter writer,string symbol,double price,double change,int volume){
					writer.WriteElementString("Price", XmlConvert.ToString(price));
					writer.WriteStartElement("Sub1"); 
					writer.WriteElementString("Change", XmlConvert.ToString(change));
			
			

					writer.WriteElementString("Volume", XmlConvert.ToString(volume));
					writer.WriteEndElement();
				}

		*/
































		private void ApplyMainSkin(string full){
			myForm.Region=null;
			try{
				System.Drawing.Bitmap imgPlayerBG=new System.Drawing.Bitmap(full); // 目前載入影像
				myForm.BackgroundImage=imgPlayerBG;
				/*
					// PropertyTable["Player主面版"]=full;

					if(bsave){
						this.SaveProperty(); // 儲存起來,下次可以用
						this.label2.Text="面版更新完成";
					
					}
					*/
			}catch(Exception ee){
				MessageBox.Show(full+" 可能不存在\n詳細資訊: "+ee.ToString());	
			}
		}



		// ============================== 按鈕事件 =================================

		// 滑鼠移到按鈕上空處理
		private void ButtonMouseEnterHandle(object sender, System.EventArgs e) {
			System.Windows.Forms.Button myButton=(System.Windows.Forms.Button)sender;

			// 取出 Under form 影像並且顯示出來
			System.Drawing.Bitmap[] PlayImage=(System.Drawing.Bitmap[]) BottomImageTable[myButton.GetHashCode()];
			System.Drawing.Bitmap UnderForm=PlayImage[1];
			myButton.Image=UnderForm;
			// end of 取出 Under form 影像並且顯示
		}
		// end of 滑鼠移到按鈕上空處理

		// 滑鼠離開按鈕處理
		private void ButtonMouseLeaveHandle(object sender, System.EventArgs e) {
			System.Windows.Forms.Button myButton=(System.Windows.Forms.Button)sender;

			// 取出 Normal form 影像並且顯示
			System.Drawing.Bitmap[] PlayImage=(System.Drawing.Bitmap[]) BottomImageTable[myButton.GetHashCode()];
			System.Drawing.Bitmap NormForm=PlayImage[0];
			myButton.Image=NormForm;
			// end of 取出 Under form 影像並且顯示
		}
		// end of  滑鼠離開按鈕處理
		// ============================== end of 按鈕事件 =================================


		// 載入按鈕 skin 圖形
		// Note:
		// ButtonImage[0]== Normal Form  ;  ButtonImage[1]== 滑鼠在按鈕上方 Form
		private void LoadSkinImage(string strNormal,string strUnder,System.Drawing.Bitmap[] ButtonImage){
			// 載入 Playn 與 Playu
			// 若之前有載入圖形, 現在放掉
			// 注意: 利用 FileStream 開啟檔案串流, 載入圖檔完畢後, 記得要關閉串流,
			// 否則就會發生下列現象:
			//             (奇怪, 已經按照 MSDN 所說的 Dispose , 為何 圖檔還會被 lock ?)
			for(int i=0;i<2;i++){
				if(ButtonImage[i]!=null)
					ButtonImage[i].Dispose();
			}
			// end of 之前載入圖形

			string fullNor=SkinDir+"\\"+strNormal;
			string fullUnder=SkinDir+"\\"+strUnder;

			// 實際載入圖形
			System.IO.FileStream  rPlayn=new System.IO.FileStream(fullNor,System.IO.FileMode.Open);
			ButtonImage[0]=new System.Drawing.Bitmap(rPlayn);
			System.IO.FileStream  rPlayu=new System.IO.FileStream(fullUnder,System.IO.FileMode.Open);
			ButtonImage[1]=new System.Drawing.Bitmap(rPlayu);
			// end of 實際載入圖形
			
			rPlayu.Close(); // 關閉串流    // 8/17/2005
			rPlayn.Close(); // 關閉串流 
		}


		// bMask  == true 表示內容, false 表示 背景
		private bool[][] createMask(string full,byte bR,byte bG,byte bB){
			bool[][] bMask=null;
			try{
				Bitmap Image=new Bitmap(full); // 目前載入影像

				// 配置 mask 記憶體
				bMask=new bool[Image.Height][];
				for(int y=0;y<Image.Height;y++)
					bMask[y]=new bool[Image.Width];

				// 比對背景資料
				for(int y=0;y<Image.Height;y++){
					for(int x=0;x<Image.Width;x++){
						Color c=Image.GetPixel(x,y);
						bool isMask=c.R==bR && c.B==bB && c.G==bG;
						if(isMask){
							bMask[y][x]=false;
						}
						else{
							bMask[y][x]=true;
						}
					}
				}
			
			}catch(Exception ee){
				MessageBox.Show(full+" 可能不存在\n詳細資訊: "+ee.ToString());	
			}

			return bMask;


		}

		
		


		private Region getRegion(bool[][] bImage ){
			// 利用 小方框 建立 Region
			GraphicsPath myGraphPath=new GraphicsPath();
			int H=bImage.Length ;
			int W=bImage[0].Length;

			for(int y=1;y<H-1;y++){
				for(int x=1;x<W-1;x++){
					bool bLeftEdge=bImage[y][x-1]==true && bImage[y][x];
					bool bRightEdge=bImage[y][x]==true && bImage[y][x+1];
					bool bTopEdge=bImage[y-1][x]==true && bImage[y][x];
					bool bBottomEdge=bImage[y][x]==true && bImage[y+1][x+1];

					if(bLeftEdge || bRightEdge || bTopEdge || bBottomEdge){
						Rectangle R=new Rectangle(x,y,1,1);
						myGraphPath.AddRectangle(R);
					}
				}
			}

			// create the region
			Region myRegion=new Region(myGraphPath);
			return myRegion;
		}
	}
}
