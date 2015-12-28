using System;
// 吸引視窗功用類別
/* Usage:
 * Step 1: 子視窗要黏住母視窗, 只要 override 子視窗的 WinProc 即可
 
 public DockUtility myDockHelp=new DockUtility(30); // 吸引視窗公用物件
 protected override void WndProc(ref Message m) {
			int[] NewLoc=new int[2];
			System.Drawing.Rectangle SelfBound=this.Bounds;
			System.Drawing.Rectangle MotherBound=this.myAVPlayer.Bounds;
            myDockHelp.WndProc2(ref m, SelfBound, this.myAVPlayer, NewLoc, this);
			base.WndProc(ref m);
 }
 
  
 * Step 2: 若本身移動時, 也要移動連接的子視窗 , 只要在母視窗的 OnMove 事件中加入下面這行即可
  
  private void AVPlayer_Move(object sender, System.EventArgs e) {
			if(MyIndexList!=null && MyIndexList.myDockHelp.isConnected){
				int movY=this.Location.Y+MyIndexList.myDockHelp.DMY;
				int movX=this.Location.X+MyIndexList.myDockHelp.DMX;
				MyIndexList.Location=new System.Drawing.Point(movX,movY); 
			}
		}
 
 * Step 3: 處理母視窗放大縮小時, 子視窗的對應淡入淡出, 則加入下面這行
        // 當視窗上面 Control Box 的 MinimumBox 被按下時, 會呼叫下面這個 Resize Event
        bool bLastMin = false;
        private void AVPlayer_Resize(object sender, EventArgs e)
        {
       
          switch(this.WindowState){
              case FormWindowState.Minimized:
                  // 當母視窗縮小時, 子視窗直接 FadeOut
                   this.myFileListForm.FadeOut();
             
                   bLastMin = true;
                  break;
              case FormWindowState.Normal:
                  if (bLastMin == true)
                  {
                     // 當母視窗回復正常時, 子視窗取回原來的位置. 並直接 FadeIn
                     this.myFileListForm.Location = this.myFileListForm.oldLocation;  //<< 注意: 子視窗必須要記住自己的位置 (Load, Move Event)
                     this.myFileListForm.FadeIn();
                    
                      bLastMin = false;
                  }
                  break;
          }
          
           
        }

       // 子視窗: Load 範例
        private void FileListForm_Load(object sender, EventArgs e)
        {
            // 隨時記錄目前位置, 
            if (WindowState == FormWindowState.Normal)
                oldLocation = this.Location;
        }
    
        // 子視窗: Move 範例
        // 當視窗移動時, 隨時記錄目前位置 (因為縮小時, 會需要這個位置)
        private void FileListForm_Move(object sender, EventArgs e)
        {
            if (Location.X >= 0 && Location.Y >= 0)
                oldLocation = this.Location;
        }
 
 
*/

namespace Player
{
	/// <summary>
	/// Summary description for DuckUtility.
	/// </summary>
	/// 

	
	public class DockUtility
	{
		
		System.Drawing.Rectangle myRec,recDockToTarget;
		int thYGap;

		public bool isConnected=false;
		public DockUtility(int thYGap)
		{
			
			this.thYGap=thYGap;
		}
	
		// 直接設定為黏住
		public bool setDock(System.Drawing.Point newLoc,System.Drawing.Rectangle MyBound,System.Drawing.Rectangle recDockToTarget){
			return this.isDock(newLoc.Y,newLoc.X,new int[]{newLoc.Y,newLoc.X},MyBound,recDockToTarget);
		}

		//int newY,newX;
		public bool isDock(int curY,int curX,int[] NewYX,System.Drawing.Rectangle MyBound,System.Drawing.Rectangle recDockToTarget){
			this.myRec=MyBound;
			this.recDockToTarget=recDockToTarget;

			NewYX[0]=curY;
			NewYX[1]=curX;
			isConnected=false;
			if(isBottomNearMTop(thYGap,curY,myRec,recDockToTarget)){
				// 判斷 X 座標是否在母視窗的左右範圍內
				if(curX+myRec.Width >= recDockToTarget.Left && curX <= recDockToTarget.Left+recDockToTarget.Width){
					NewYX[1]=curX;
					NewYX[0]=recDockToTarget.Location.Y-myRec.Height;
					isConnected=true;
					return true;
				}else{
					isConnected=false;
					return false;
				}
			}

			if(isTopNearMBottom(thYGap,curY,myRec,recDockToTarget)){
				// 判斷 X 座標是否在母視窗的左右範圍內
				if(curX+myRec.Width >= recDockToTarget.Left && curX <= recDockToTarget.Left+recDockToTarget.Width){
					NewYX[0]=recDockToTarget.Top+recDockToTarget.Height;
					NewYX[1]=curX;
					isConnected=true;
					return true;
				}else{
					isConnected=false;
					return false;
				}
			}

			if(isLeftNearMRight(thYGap,curX,myRec,recDockToTarget)){
				// 判斷 Y 座標是否在母視窗的上下範圍內
				if(curY>= recDockToTarget.Top-myRec.Height && curY <= recDockToTarget.Top+recDockToTarget.Height){
					NewYX[1]=recDockToTarget.Location.X+recDockToTarget.Width;
					NewYX[0]=curY;
					isConnected=true;
					return true;
				}else{
					isConnected=false;
					return false;
				}
			}

			if(isRightNearMLeft(thYGap,curX,myRec,recDockToTarget)){
				// 判斷 Y 座標是否在母視窗的上下範圍內
				if(curY>= recDockToTarget.Top-myRec.Height && curY <= recDockToTarget.Top+recDockToTarget.Height){
					NewYX[1]=recDockToTarget.Location.X-myRec.Width;
					NewYX[0]=curY;
					isConnected=true;
					return true;
				}else{
					isConnected=false;
					return false;
				}
			}
			return false;
		}
		
		private bool isBottomNearMTop(int thYGap,int myY,System.Drawing.Rectangle myRec,System.Drawing.Rectangle recDockToTarget){
			//int myY=this.Location.Y+this.Height;
			if(System.Math.Abs(myY+myRec.Height-recDockToTarget.Location.Y) < thYGap){
				return true;
			}
			return false;
		}
		private bool isTopNearMBottom(int thYGap,int myY,System.Drawing.Rectangle myRec,System.Drawing.Rectangle recDockToTarget){
			// int myY=this.Location.Y;
			if(System.Math.Abs(myY-(recDockToTarget.Location.Y+recDockToTarget.Height)) < thYGap){
				return true;
			}
			return false;
		}

		private bool isLeftNearMRight(int thXGap,int myX,System.Drawing.Rectangle myRec,System.Drawing.Rectangle recDockToTarget){
			// int myX=this.Location.X;
			if(System.Math.Abs(myX-(recDockToTarget.Location.X+recDockToTarget.Width)) < thXGap){
				return true;
			}
			return false;
		}


		private bool isRightNearMLeft(int thXGap,int myX,System.Drawing.Rectangle myRec,System.Drawing.Rectangle recDockToTarget){
			// int myX=this.Location.X+this.Width;
			if(System.Math.Abs(myX+myRec.Width-recDockToTarget.Location.X) < thXGap){
				return true;
			}
			return false;
		}

		public  struct WINDOWPOS {
			public System.IntPtr hwnd;
			public	System.IntPtr hwndInsertAfter;
			public	int x;
			public	int y;
			public	int cx;
			public	int cy;
			public	System.UInt32 flags;
		}

		public int DMY,DMX;
		public unsafe void WndProc(ref System.Windows.Forms.Message m,System.Drawing.Rectangle myRec,System.Drawing.Rectangle recDockToTarget,int[] NewLoc,System.Windows.Forms.Form test) {
			
			// 這個常數是在 winuser.h 找到的
			const int  WM_WINDOWPOSCHANGING     =       0x0046;
			//const int SWP_NOSIZE          = 0x0001;
            //const int SWP_NOMOVE       =   0x0002;

			switch (m.Msg) {
				case WM_WINDOWPOSCHANGING:
					WINDOWPOS* pos=(WINDOWPOS*)m.LParam;
					int y=pos->y;
					int x=pos->x;
						
					//  判斷 目前收到的訊息為 Z-order 的轉換, 不用改變 isConnected 的狀態
					if(pos->flags==3)
						break;

					if(isDock(y,x,NewLoc,myRec,recDockToTarget)){
						// 更改 Message 中 視窗位置
						pos->y=NewLoc[0];
						pos->x=NewLoc[1];
						 // end of 更改視窗位置

						isConnected=true;

						DMY=NewLoc[0]-recDockToTarget.Location.Y; // 與母視窗的距離
						DMX=NewLoc[1]-recDockToTarget.Location.X; // 與母視窗的距離

						//test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;
					}else{

						//test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;
						
						isConnected=false;
                        
						
					}
					break;		
		
				//case WM_NCPaint:
					
				//	break;
			}
		
			// base.WndProc(ref m);
		}

        
        // 輸入目前座標, 目前幾何資訊, Parent幾何資訊
        // 自動調整位置黏到 Parent
        public bool DockMain(System.Windows.Forms.Form myForm,int curY, int curX, System.Drawing.Rectangle myRec, System.Drawing.Rectangle MotherFormRec, int[] retNewLoc)
        {
            if (isDock(curY, curX, retNewLoc, myRec, MotherFormRec))
            {
                

                isConnected = true;

               /* 
                * DMY = retNewLoc[0] - recDockToTarget.Location.Y; // 與母視窗的距離
                DMX = retNewLoc[1] - recDockToTarget.Location.X; // 與母視窗的距離
                */
                myForm.Left = retNewLoc[1];
                myForm.Top = retNewLoc[0];
                // 除錯資訊
                //test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;

                return true;
            }
            else
            {
                // 除錯資訊
                //test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;

                isConnected = false;

                return false;
            }
        }
        

		public unsafe void WndProc2(ref System.Windows.Forms.Message m,System.Drawing.Rectangle myRec,System.Windows.Forms.Form MotherForm,int[] NewLoc,System.Windows.Forms.Form test) {
			
			// 這個常數是在 winuser.h 找到的
			const int  WM_WINDOWPOSCHANGING     =       0x0046;
			//const int SWP_NOSIZE          = 0x0001;
			//const int SWP_NOMOVE       =   0x0002;
            System.Drawing.Rectangle MotherFormRec;
            if (MotherForm.Menu == null)
            {
                // int MainMenuHeight=32;
                int MainMenuHeight = 0;
                // MotherFormRec = new System.Drawing.Rectangle(MotherForm.Left, MotherForm.Top, MotherForm.Width, MotherForm.Height - MainMenuHeight);
                MotherFormRec = MotherForm.Bounds;
            }
            else
            {
                // System.Drawing.Rectangle MotherFormRec = new System.Drawing.Rectangle(MotherForm.Left, MotherForm.Top, MotherForm.Width, MotherForm.Height);
                MotherFormRec = MotherForm.Bounds;
            }

            // 只處理正常狀況 (其中有一個視窗還沒出現, 都不能用來判斷是否連接)
            if (myRec.Y < 0 || myRec.X < 0 || MotherFormRec.X < 0 || MotherFormRec.Y < 0)
                return;

			switch (m.Msg) {
				case WM_WINDOWPOSCHANGING:
					WINDOWPOS* pos=(WINDOWPOS*)m.LParam;
											
					//  判斷 目前收到的訊息為 Z-order 的轉換, 不用改變 isConnected 的狀態
					if(pos->flags==3)
						break;

                    int y = pos->y;
                    int x = pos->x;
                    
                    if (x != 0 && y != 0)
                    {
                        if (isDock(y, x, NewLoc, myRec, MotherFormRec))
                        {
                            // 直接更改 Message 中的視窗位置 (unsafe 的原因)
                            pos->y = NewLoc[0];
                            pos->x = NewLoc[1];
                            // end of 更改視窗位置

                            isConnected = true;

                            DMY = NewLoc[0] - recDockToTarget.Location.Y; // 與母視窗的距離
                            DMX = NewLoc[1] - recDockToTarget.Location.X; // 與母視窗的距離

                            // 除錯資訊
                            //test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;
                        }
                        else
                        {
                            // 除錯資訊
                            //test.Text="c="+this.isConnected+" DMY="+this.DMY+" DMX="+this.DMX;

                            isConnected = false;

                        }
                    }
					break;		
		
					//case WM_NCPaint:
					
					//	break;
			}
		
			// base.WndProc(ref m);
		}

        // 將目前的 Form 黏到 parent 的左邊中間的位置
        public static void MoveToMotherLeftMiddle(System.Windows.Forms.Form curForm, System.Windows.Forms.Form MotherForm)
        {
            curForm.Left = MotherForm.Left - curForm.Width;
            curForm.Top = MotherForm.Top + MotherForm.Height / 2 - curForm.Height / 2;
        }

        // 將目前的 Form 黏到 parent 的右邊中間的位置
        public static void MoveToMotherRightMiddle(System.Windows.Forms.Form curForm, System.Windows.Forms.Form MotherForm)
        {
            curForm.Left = MotherForm.Left + MotherForm.Width;
            curForm.Top = MotherForm.Top + MotherForm.Height / 2 - curForm.Height / 2;
        }

        // 將目前的 Form 黏到 parent 的下面位置
        public static void MoveToMotherBottom(System.Windows.Forms.Form curForm, System.Windows.Forms.Form MotherForm)
        {
            curForm.Left = MotherForm.Left;
            curForm.Top = MotherForm.Bottom;
        }
	}// end of Utility
}
