// Advanced .NET Framework Progamming Course

// Target: 希望能夠產生 Hello Kitty 風格的 Menu Item

// 問題:
//   1. Main 如何知道 每一個 Item 的大小 ?
//   MeasureItem Event ?
// Knowledge:
//   1. 可以使用 MenuItem.DrawItem Event 自訂 Draw Item 的形式 (OwnerDraw = true) [1,2]


// Reference: 
// 1. ms-help://MS.MSDNQTR.2004JAN.1033/cpref/html/frlrfsystemwindowsformsmenuitemclassownerdrawtopic.htm
// 2. ms-help://MS.MSDNQTR.2004JAN.1033/cpref/html/frlrfsystemwindowsformsmenuitemclassdrawitemtopic.htm
// 3. .Net Framework Source Code http://msdn.microsoft.com/library/default.asp?url=/library/en-us/Dndotnet/html/mssharsourcecli.asp

using System;
using System.Windows.Forms; // for   DrawItemEventArgs 
using System.Drawing; // for Font

namespace Player
{
	/// <summary>
	/// Summary description for JingMenuItem1.
	/// </summary>
	public class JingMenuItem1:System.Windows.Forms.MenuItem {
		Font aFont;

		public JingMenuItem1() {
			//
			// TODO: Add constructor logic here
			//	
			SetCustomDraw();
		}

		
		
		private void SetCustomDraw(){
			this.OwnerDraw=true; // 設定要自己畫 item 
			this.DrawItem += new DrawItemEventHandler(DrawCustomMenuItem); // 設定自己畫 item 的外觀
			this.MeasureItem+=new MeasureItemEventHandler(MeasureItemEventHandler); // 設定 重劃後的 Item 大小
			this.Popup+=new EventHandler(PopupEventHandler);
			// 設定字形
			aFont = new Font("細明體", 9, FontStyle.Regular, GraphicsUnit.Point);
		}

		// Step 1: 設定目前 Item 的大小
		public  void MeasureItemEventHandler(object sender,	MeasureItemEventArgs e){
			MenuItem customItem = (MenuItem) sender;
			
			SizeF stringSize = e.Graphics.MeasureString(customItem.Text, aFont);


			// Set the height and width of the item
			e.ItemHeight = (int)stringSize.Height;
			e.ItemWidth = (int)stringSize.Width;
		}
	
		// Step 2: 自己畫
		private void DrawCustomMenuItem(object sender, 			DrawItemEventArgs e) {
			MenuItem customItem = (MenuItem) sender;
			System.Drawing.Color BGColor=System.Drawing.SystemColors.Control;
			System.Drawing.Color FGColor=System.Drawing.Color.Green;
			
			
			// Step 2: 取得字形的大小, 等一下要畫橢圓圍繞他.
			SizeF astringSize = e.Graphics.MeasureString(customItem.Text, aFont);
			


			// 取得目前  Item 的狀態
			
			if((e.State & DrawItemState.HotLight)==DrawItemState.HotLight){
				// 若目前是被 Hot Track
				// Step 1: 建立 筆刷
				System.Drawing.Brush aBrush = System.Drawing.Brushes.DarkMagenta;

				// Step 3: 畫出方框
				e.Graphics.DrawRectangle(new Pen(FGColor),new Rectangle(e.Bounds.X+1, e.Bounds.Y+1, 
					e.Bounds.Width-1,
					e.Bounds.Height-1)); 

				e.Graphics.DrawString(customItem.Text, aFont, aBrush, e.Bounds.X+4, e.Bounds.Y+4);
				
			}else{
				// Step 1: 建立 筆刷
				System.Drawing.Brush aBrush = System.Drawing.Brushes.Black;

				// Step 3: 清除方框
				e.Graphics.DrawRectangle(new Pen(BGColor),new Rectangle(e.Bounds.X+1, e.Bounds.Y+1, 
					e.Bounds.Width-1,
					e.Bounds.Height-1)); 
				

				e.Graphics.DrawString(customItem.Text, aFont, aBrush, e.Bounds.X+4, e.Bounds.Y+4);
			}

			


			

			

			/*
			 * 
			 
			// Step 2: 取得字形的大小, 等一下要畫橢圓圍繞他.
			SizeF astringSize = e.Graphics.MeasureString(customItem.Text, aFont);
			SizeF BigSize= e.Graphics.MeasureString(customItem.Text, aFont);
			
			// 並且在外面畫上橢圓
			e.Graphics.DrawEllipse(new Pen(System.Drawing.Color.Black, 2),
				new Rectangle(e.Bounds.X, e.Bounds.Y, 
				(System.Int32)stringSize.Width,
				(System.Int32)stringSize.Height));
			
			*/
		}

		
		// PopupEventHandler

		private void PopupEventHandler(object sender, System.EventArgs e){
			
		}


		



	}
}
