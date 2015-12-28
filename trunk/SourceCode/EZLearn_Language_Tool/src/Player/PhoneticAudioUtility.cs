// 日語發音元件

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
// 播放語音相關
using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;
// end of 語音

// Usage
/*
strPhoneticDirectory=(string)myPlayer.PropertyTable["音標檔預設目錄"];
strPhoneticFilename=(string)myPlayer.PropertyTable["音標檔名稱"];// "PhoneticAudio.wma";
PhoneticAudioUtility symbolVoice=new PhoneticAudioUtility(strPhoneticDirectory,strPhoneticFilename);

// 播放清音 
symbolVoice.PlaySymbol(0,1); // 'ゆ'
*/

namespace Player
{
	/// <summary>
	/// Summary description for PhoneticAudioUtility.
	/// </summary>
	public class PhoneticAudioUtility
	{
		public Audio ourAudio; // 播放語音專用物件
		string strPhoneticDirectory; // 音標檔預設目錄
		string strPhoneticFilename; // 音標檔預設檔名 "PhoneticAudio.wma";
		string strPhoneticIndexFilename; // 索引檔 (記錄每個字母的音波索引位置) [由 Phonetic_AudioReady 產生]
		double [][]PhoneticIndex;       // 音標索引檔索引陣列資料 [由 ReadPhoneticIndex 產生資料]

		private bool bPhoneticAudioReady; // 語音檔是否準備好了 [PlaySymbol 會檢查]


		// 日文字母符號定義 (平假名,片假名)
		private char[]PhoneticSymbols1={'や','ゆ','よ','り','れ',
										   'ろ','わ','ゑ','ん','ア',
										   'イ','ウ','エ','オ','ガ',

										   'ギ','グ','コ','サ','シ',
										   'ス','ズ','セ','ゼ','ソ',
										   'ゾ','チ','ツ','デ','ナ',

										   'ネ','ノ','ハ','バ','パ',
										   'ビ','フ','プ',
										   'ヘ','ベ','ペ','ホ','ボ',

										   'マ','メ',
									   
										   'ヤ','ユ','ヨ','リ','レ',
										   'ロ','ワ','ヱ','ン','ヵ',
										   'Д','Ё','З','Й','Л',

										   'У','Х','Ш','Ъ','Ь',
										   'Ю','Я','а','б','в',
										   'г','ё','и','л','о',

										   'с','т','у','ф','х',
										   'ч','щ','ы',
										   'ь','э','ю','я','①',
										   '③','⑥'
									   };

		// 濁音 (平)
		private string P2="ゎゐをァィゥェォカキクケゴザジタヂヅトニ";
		// 濁音 (片)
		private string P3="ヮヰФヴヶЕЖИКМФЦЩЫЭджйトп";
		private string PhoneticSymbols2;

		// 半濁音(平)
		private string P4="ダッテドヌ";
		//半濁音(片)
		private string P5="езкнр";
		private string PhoneticSymbols3;

		//拗音(平)
		private string P6="わヒ わピ わブ ウヒ ウピ ウブ グヒ グピ グブ ズヒ ズピ ズブ チヒ チピ チブ ノヒ ノピ ノブ ベヒ ベピ ベブ ゐヒ ゐピ ゐブ ェヒ ェピ ェブ ヂヒ ヂピ ヂブ ッヒ ッピ ッブ ";
		//拗音(片)
		private string P7="ワц ワш ワъ Ёц Ёш Ёъ Хц Хш Хъ Яц Яш Яъ ёц ёш ёъ тц тш тъ эц эш эъ ヰц ヰш ヰъ Жц Жш Жъ жц жш жъ зц зш зъ ";
		private string PhoneticSymbols4;
		// end of 字母符號定義

		public PhoneticAudioUtility(string strPhoneticDirectory,string strPhoneticFilename)
		{
			this.strPhoneticDirectory=strPhoneticDirectory;
			this.strPhoneticFilename=strPhoneticFilename;

			// 所有日文字母
			PhoneticSymbols2=P2+P3; // 濁音 符號表
			PhoneticSymbols3=P4+P5; // 半濁音 符號表
			PhoneticSymbols4=P6+P7; // 拗音 符號表
			// end of 所有日文字母

			// Step 1:
			bPhoneticAudioReady=Phonetic_AudioReady();
			if(bPhoneticAudioReady!=true){
				string message="因為著作權的關係,\n1. 你必須要有自己的日語語音聲音檔: filename.xxx\n2. 自行指定各符號的唸法在語音檔所在的位置: filename.xxx.txt\n      起始秒數:結束秒數";
				MessageBox.Show(message);
			}else{

			// Step 2:
				bool bOk=ReadPhoneticIndex(); // 載入索引檔
				if(bOk!=true){
					string message="語音檔格式錯誤";
					MessageBox.Show(message);
				}
			}			
		}

		// Step 1: 檢查音標檔與索引檔是否存在
		// 注意: 因為著作權的關係, 
		//      1. 你必須要有自己的日語語音聲音檔: filename.xxx
		//		2. 自行指定各符號的唸法在語音檔所在的位置: filename.xxx_index.txt
		//              起始秒數:結束秒數
		private bool Phonetic_AudioReady(){
			// 判斷音標檔是否存在
			string strPhoneticFullFilename=strPhoneticDirectory+"\\"+strPhoneticFilename;
			strPhoneticIndexFilename=strPhoneticDirectory+"\\"+strPhoneticFilename+"_index.txt";

			if(System.IO.File.Exists(strPhoneticFullFilename) && System.IO.File.Exists(strPhoneticIndexFilename)){
				ourAudio = new Audio(strPhoneticFullFilename); // 載入音標檔
				ourAudio.Ending += new System.EventHandler(this.ClipEnded); // 設定當播放結尾時, 要如何停止播放
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

		// Step 2: 讀取索引檔
		private int VoiceIndexNum=0; // 實際由檔案中讀到的語音資料
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

		// Step 3: 播放聲音
		// 參數說明:
		// SymbolClass=0 清音, 1 濁音, 2 半濁音, 3 拗音
		// index 符號位置
		public void PlaySymbol(int SymbolClass, int index){
			
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
		}// end of play

		private void ClipEnded(object sender, System.EventArgs e) {
			
			if(ourAudio!=null && ourAudio.Disposed!=true){
				if(ourAudio.Playing==true){
					// 可能是因為 AudioVideoPlayback 的啟動太慢, 會來不及發音,導致有時候使用者可能會以為沒有發音
					// 修正: 改成循環播放空白語音段看看是否能解決這個問題
					//ourAudio.Stop(); 

					int start=70;
					int end=72;
					double StartSeek=start*10000000;
					double EndSeek=end*10000000;
				
					ourAudio.SeekCurrentPosition(StartSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
					ourAudio.SeekStopPosition(EndSeek,Microsoft.DirectX.AudioVideoPlayback.SeekPositionFlags.AbsolutePositioning);
					
					ourAudio.CurrentPosition=start;
					
				}
			}
			
			
		}
	}
}
