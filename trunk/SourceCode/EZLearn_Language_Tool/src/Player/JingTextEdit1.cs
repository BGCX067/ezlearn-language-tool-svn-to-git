// Advanced .NET Framework Progamming Course
// 程式設計技巧: 沒收某個 Message   (If the control recognized the message, it will be consumed by the preprocess function)
//                                                                                 by 井民全  8/1/2005
// 
// 使用 public override bool PreProcessMessage
// 當要沒收時, 傳回 true .
// 表示這個 control 已經處理這個 message 了, 不需要再進一步的處理
// 注意:
//      你在 WndProc 進行攔截是沒有用的!!


using System;
using System.Windows.Forms;  // for WinProc 

namespace Player
{
	/// <summary>
	/// Summary description for JingTextEdit1.
	/// </summary>
	public class JingTextEdit1:System.Windows.Forms.TextBox {
		AVPlayer myPlayer;

		public bool bIsExtendedMode=false; // 是否目前為 Seek(,其他資訊)* 狀態

		public JingTextEdit1()
		{
			
		}
		public bool isExtendeMode(){
			if(this.Text.IndexOf(",")!=-1)
				return true;
			else
				return false;
		}

	    // ================================== 關鍵性程式片段 ==============================
		public override bool PreProcessMessage(
			ref Message m
			){

           
			const int  WM_KEYDOWN      =       0x0100;
			bool bDeleted=false; // 是否要沒收
			switch (m.Msg) {
				case WM_KEYDOWN:
					System.IntPtr vkc=m.WParam;
					int charCode=(int)vkc;

					if(!(charCode >='0' && charCode <='9')){
						bDeleted=!AllowCode(charCode); // 若不是數字 也不是 允許的控制按鍵, 則沒收
						
						// bDeleted=true;                  // 不是數字的全部被沒收 
					}
					break;

			}

			if(bDeleted){
				return bDeleted;   // 這個 message 被處理掉了, 傳回 false. 後面的動作全部被沒收
			}
		
			return base.PreProcessMessage(ref m);

		}
		// ================================== end of 關鍵性程式片段 ==============================

		// 允許下列功能鍵: 
		//        Ender , 上,下,左,右 鍵 等功能鍵, 要繼續被處理
        // 與 IndexListForm.cs 中的 listBox1_KeyDown method 相關
		private bool AllowCode(int charCode){
			const int Enter=13; // 輸入索引
			const int Up=38; // 索引上
			const int Down=40; // 索引下
            const int ShiftKey = 16; // Shift 表示立即重新聽目前的片段   11/29/2006

			const int Left=37; // 快速向前索引
			const int Right=39; // 快速向後索引
			const int BackSpace=8; //輸入修正
			const int DelKey=46; // 輸入修正      8/18/2005  (暫停已經改成 Ctrl Key)
			const int Space=32; // 暫停
			const int Dot=190; // 小數點
			const int Comma=188; // 逗號 9/10/2005
			const int R='R'; // 向前索引模式 = R 起始秒數,向前索引的秒數,Repeat,註解

            const int Ctrl = 17; // Ctrl 暫停 // 因為空白會被輸入當作 edit 的索引   7/24/2007
            

            // '\' 表示立即重新聽目前的片段   11/29/2006
            if (charCode == ShiftKey) {
                // 若目前是暫停, 所以現在是執行
                if (this.myPlayer.isPause())
                {
                    this.myPlayer.PauseOrPlay(); 
                }
                else
                {  // 否則直接進行索引
                    myPlayer.SeekPlay(); // 進行重新索引的工作
                }
                this.myPlayer.label2.Text = this.myPlayer.L_Manager.getMessage("IndexListForm_System_ListenAgain");// "再聽一次";
                return false; // 沒收 BackSlash 字元不顯示處理
            }

            if (myPlayer.state == 1) { // 預設狀態是 0:執行  1: 暫停  2: 停止播放
				// 暫停時, 只允許 [空白鍵]
				// if(charCode==Space){
                if(charCode==Ctrl){ // 7/24/2007
					return true;
				}else{
					return false; // 沒收 空白 訊息
				}
			}

			// 若使用者按 Enter 時, 要先檢查輸入是否為數字
			if(charCode==Enter){
				if(this.Text.IndexOf(",")==-1){ //  擴充格式狀態: 開放所有輸入, 這個可以讓使用者輸入中文
					try {
						Double.Parse(this.Text);
						return true;
					}catch(Exception){
						this.myPlayer.label2.Text="輸入不合法";
						return false; // 輸入不是數字, 沒收 Enter 訊息
					}
				}

				bIsExtendedMode=false;  // 恢復為原始模式(使用者可能之前是輸入擴充模式, 這會讓所有輸入都允許)
				return true;
			}
			// end of 

            if (charCode == Up || charCode == Down)
            {
                this.myPlayer.bMySectionBasedPlay = true;
            }

			if(charCode==Comma){
				// 現在處於 擴充格式狀態: 開放所有輸入, 這個可以讓使用者輸入中文
				bIsExtendedMode=true;
			}


			if(bIsExtendedMode==false){ // 原始狀態: 只有允許 Seek 資訊進入
				if(charCode==R || charCode==DelKey|| charCode==Dot|| charCode==Up || 
                   charCode==Down || charCode == Left || charCode == Right || charCode==BackSpace ||
                   charCode == Ctrl) {
					return true;
				}
				else{
					return false;
				}
				
			}else{
				return true;
			}

           

		}
		

		/* 
		    // 你在 WndProc 進行攔截是沒有用的!!
			protected override void WndProc(ref Message m) {
				const int  WM_KEYDOWN      =       0x0100;
				bool bValid=true;
				switch (m.Msg) {
					case WM_KEYDOWN:
						System.IntPtr vkc=m.WParam;
						bValid=false;
						string k=m.ToString();
						break;

				}
				if(bValid)
					base.WndProc(ref m);
			}
	*/
		
		
		public void setPlayer(AVPlayer argPlayer){
			myPlayer=argPlayer;
		}

	}
}
