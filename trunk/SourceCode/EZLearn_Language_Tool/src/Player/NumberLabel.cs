// 自訂控制元件範例: 顯示自己畫的數字 label
//                                                                井民全
// 數字 bmp 命名格式: 
//       Num_0.bmp,..., Num_9.bmp
// Usage:
/*
            // 因為 Bitmap 元件讀取圖檔時, 會把圖檔鎖住直到 Dispose 才會放開,
			// 所以必須把圖檔與數字操作邏輯分開處理.
			//  1. 先讀取(鎖住)圖檔 ==> 當作資源
			//  2. 交給數字操作邏輯 顯示
			
            // 自訂數字元件
			imgNumArray=NumberLabel.CreateNumImage(ExecutationFileDir,"Num",0,0,0);
			l1=new NumberLabel(imgNumArray);
			l1.Left= 136;
			l1.Top=48;
			l1.Height=30;   // <<--  不應該由使用者指定高度 (更新時, 這裡應該首先更新)
			l1.SetWidth(4); // 有四組數字
			this.Controls.Add(l1);
			// end of 自訂數字
 */
using System;
using System.Windows.Forms;   // 使用 PaintEventArgs

namespace Player {
	/// <summary>
	/// Summary description for NumberLabel.
	/// </summary>
	public class NumberLabel:System.Windows.Forms.Label {
		
		public int NumTab=2; // 符號與符號之間的距離
		
		// 畫出元件視覺表象
		protected override void OnPaint(PaintEventArgs e) {
			// base.OnPaint(e); // 畫出原來的東西 (先畫出登記要畫的程式)
			

			/*
			 System.Drawing.Rectangle rec=e.ClipRectangle;
			// 測試畫方框
			e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Red),0,0,rec.Width/2,rec.Height/2);
			// end of 畫方框
			
			// 標出元件的範圍
			e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Blue,1),0,0,rec.Width-1,rec.Height-1);
			// end of 元件	
			
             */

			// 測試畫影像
			PaintText(e.Graphics,0,0);
			// end of 畫影像

				
		}
		// end of 元件

		


		// 基本顏色設定
		private void init(){
			this.BackColor=System.Drawing.Color.Transparent; // 指定背景顏色為透明色
			
		}

		

		
		// 數字元件的建構子
		System.Drawing.Bitmap[] imgNumArray;
		public NumberLabel(System.Drawing.Bitmap[] imgNumArray){
			init(); // 基本顏色設定,陣列配置
			this.imgNumArray=imgNumArray;
		}
		// end of 建構子

		// 依據要顯示的數字個數決定元件的寬度
		public void SetWidth(int SymbolNum){
			int W=SymbolNum*(imgNumArray[0].Width +NumTab);
			this.Width=W;
		}

		public static System.Drawing.Bitmap[] CreateNumImage(string ImagePath,string strNumImage,int bgR,int bgG,int bgB){
			return LoadNumImage(ImagePath,strNumImage,System.Drawing.Color.FromArgb(bgR,bgG,bgB));
		}

		// ===========================   數字影像處理的部分 ========================
		
		
		// 載入數字影像, 指定透明色
		private static System.Drawing.Bitmap[]  LoadNumImage(string ImagePath,string strNumImage,System.Drawing.Color TransparentColor){ 
			const int imgNum=10;
			System.Drawing.Bitmap[] imgNumArray;
/*
			if(imgNumArray==null){
				imgNumArray=new System.Drawing.Bitmap[imgNum];
			}
			*/
			//int NeedWidth=0;
			imgNumArray=new System.Drawing.Bitmap[imgNum];
			
				for(int i=0;i<imgNum;i++){
					string filename= strNumImage+"_"+i+".bmp";//  Num_1
					// 檔會被 lock, 直到物件被銷毀
					// 檔案可能被其他元件鎖住
					//     imgNumArray[i]=new System.Drawing.Bitmap(filename); // 目前只載入 imgNum 張影像
					System.IO.FileStream  r=new System.IO.FileStream(ImagePath+"\\"+filename,System.IO.FileMode.Open);
					imgNumArray[i]=new System.Drawing.Bitmap(r);
					imgNumArray[i].MakeTransparent(TransparentColor); // 指定某一個顏色為透明顏色
					r.Close(); // 關閉串流    8/17/2005
				}
		
			return imgNumArray;
		}

		// 畫出一個數字  (會被 PaintText 呼叫)
		private void PaintNumImage(System.Drawing.Graphics g,int y,int x,int NumIndex){
			System.Drawing.Image img=imgNumArray[NumIndex];
			if(img!=null){
				g.DrawImage(img,new System.Drawing.Point(x,y));
			}
			
		}

		
		// 畫出 Text  (會被 OnPaint 呼叫)
		private void PaintText(System.Drawing.Graphics g,int y,int x){
			string strTarget=this.Text;
			int xc=x;
			for(int i=0;i<Text.Length;i++){
				char c=strTarget[i];
				if(c >='0' && c<='9'){
					int NumIndex=c-(int)'0'; // 取出數字

					if(NumIndex<imgNumArray.Length ){ // 檢查數字影像是否已經載入
						PaintNumImage(g,y,xc,NumIndex); // 畫出數字
						xc+=imgNumArray[NumIndex].Width+NumTab; // 移動畫下一個
					}
					
				}
			}
		}

		//===========================   end of 數字影像處理的部分 ========================

		// 最右邊數字的x座標 (相對於自己左邊) 可提供其他物件, 黏在後面顯示的數據)
		// k = 第 k 個位置
		// Usage:
		/*
				string strSec=""+ipos; // 秒數資訊
				myPlayer.label1.Left=myPlayer.l1.Left+myPlayer.l1.getRightMostX(strSec.Length); // 將長度資訊黏在 l1 秒數資訊後面
				myPlayer.label1.Text=""+ilength+" sec";
				myPlayer.l1.Text=strSec;  // 自訂數字元件顯示
		 */
		public int getRightMostX(int k){
			int xc=0;
			for(int i=0;i<k;i++){
				xc+=imgNumArray[i].Width+NumTab; // 移動到下一個
			}
			return xc;
		}


		

		/*
		// 記憶體管理方面還不是很熟,要確實瞭解 Dispose 的運作
		// managed object 需要 自己 dispose 嗎?
		bool disposed=false;
		protected override void Dispose( bool disposing ){
			if(!this.disposed){
				if(disposing){
					// 刪除 managed code 所配置的資源
					//FreeImage(disposing );
				}
				base.Dispose();
				disposed=true;
			}	
		}
	   */

		/*
		// 釋放影像記憶體 (會被 Dispose 呼叫)
		private void FreeImage( bool disposing ) {
			if(!this.disposed){
				for(int i=0;i<imgNumArray.Length;i++){
					if(imgNumArray[i]!=null){
						imgNumArray[i].Dispose();// 釋放影像
						imgNumArray[i]=null;
					}
				}
				disposed=true;
			}
		}
		 */

		
		/*
		// 處理背景顏色
		protected override void OnPaintBackground(PaintEventArgs pevent){
			// 1. 如果不作任何動作, 將會是 黑色的背景
			this.BackColor=System.Drawing.Color.Transparent; // 指定背景顏色為透明色
			base.OnPaintBackground(pevent);
			
		}
		*/


	}
}