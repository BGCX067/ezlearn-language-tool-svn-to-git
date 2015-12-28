// 專門用來處理 Form 的特效
//    1. 漸漸的淡出(消失), 的視覺效果 ==> FadeOut
//    2. 暫停效果 (亮, 暗)   ==> Flash


/*
	OpacityUtility.FadeOut_Close(this,30); //淡出關閉特效
	OpacityUtility.FadeIn_Only(MyIndexList,30); // 淡入
*/
using System;


namespace Player
{ 
	/// <summary>
	/// Summary description for OpacityUtility.
	/// </summary>
	/// 

	public class OpacityUtility {

		class KKTimer:System.Windows.Forms.Timer{
			public System.Windows.Forms.Form tForm;
			public double OpacityLevel=1; // 漸漸的淡出(消失), 的視覺效果變數
			public int FlashC=0; // 控制  暫停效果 (亮, 暗) 變數
			public bool FlashFirstRun=true; // 是否第一次執行閃爍
            public float FadeOutFinalValue = 0; 
			public KKTimer(System.Windows.Forms.Form tForm) {
				this.tForm=tForm;
			}
		}

		class KKTimer_Label:System.Windows.Forms.Timer{
			public System.Windows.Forms.Form tForm;
			public System.Windows.Forms.Label tLabel;
			
			public int FlashC=0; // 控制  暫停效果 (亮, 暗) 變數
			public bool FlashFirstRun=true; // 是否第一次執行閃爍

			public KKTimer_Label(System.Windows.Forms.Form tForm,System.Windows.Forms.Label tLabel) {
				this.tForm=tForm;
				this.tLabel=tLabel;
			}
		}

		

		public OpacityUtility() {
			//tForm=target;
		}

		// 標籤閃爍
		public static void Flash_Label(System.Windows.Forms.Form form,System.Windows.Forms.Label  target,int Interval){
			System.Windows.Forms.Label tLabel=target;
			KKTimer_Label myTimer;
			// 設定 專屬的 Timer
			myTimer=new KKTimer_Label(form,tLabel);
			
			myTimer.Tick += new EventHandler(FlashLabel_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 專屬的 Timer
		}

		

		private static void FlashLabel_TimerEventProcessor(Object myObject,
				EventArgs myEventArgs) {
				System.Windows.Forms.Form tForm =((KKTimer_Label)myObject).tForm;
				System.Windows.Forms.Label tLabel =((KKTimer_Label)myObject).tLabel;
				KKTimer_Label mytimer=(KKTimer_Label)myObject;

			
			
				// 檢查是否停止閃爍
				if(mytimer.FlashFirstRun!=true && tForm.Opacity==1.0 ){
					mytimer.Stop();
					mytimer.Dispose();
					return;
				}
				// end of  檢查是否停止閃爍

			
				mytimer.FlashC=(mytimer.FlashC+1)%2;
				int FlashC=mytimer.FlashC;
				if(FlashC==0){
					tLabel.Text="暫停";
				}else{
					tLabel.Text="";
				}
				mytimer.FlashFirstRun=false; // 執行後, 就不是第一次執行閃爍
			
			}
		// end of 暫停效果

		// Title 閃爍 (暫停使用0
		public static void Flash_Title(System.Windows.Forms.Form target,int Interval){
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=1;
			myTimer.Tick += new EventHandler(FlashTitle_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 專屬的 Timer
		}

		private static void FlashTitle_TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {

			System.Windows.Forms.Form tForm =((KKTimer)myObject).tForm;
			KKTimer mytimer=(KKTimer)myObject;

			if(mytimer.FlashFirstRun){
				// 第一次執行時, 降低亮度
				tForm.Opacity=0.7;
			}
			
			// 檢查是否停止閃爍
			if(mytimer.FlashFirstRun!=true && tForm.Opacity==1.0){
				mytimer.Stop();
				mytimer.Dispose();
				return;
			}
			// end of  檢查是否停止閃爍

			
			mytimer.FlashC=(mytimer.FlashC+1)%2;
			int FlashC=mytimer.FlashC;
			if(FlashC==0){
				tForm.Text=" 暫停播放 ... ";
			}else{
				tForm.Text="";
			}
			mytimer.FlashFirstRun=false; // 執行後, 就不是第一次執行閃爍
			
		}
		// end of 暫停效果


        // 暫停效果
		public static void Flash_Form(System.Windows.Forms.Form target,int Interval){
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 FadeOut 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=1;
			myTimer.Tick += new EventHandler(Flash_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 FadeOut 專屬的 Timer 
		}      

		private static void Flash_TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {

			System.Windows.Forms.Form tForm =((KKTimer)myObject).tForm;
			
			// 檢查是否停止閃爍
			if(((KKTimer)myObject).FlashFirstRun!=true && tForm.Opacity >0.99 && tForm.Opacity<=1){
				((KKTimer)myObject).Stop();
				((KKTimer)myObject).Dispose();
				return;
			}
			// end of  檢查是否停止閃爍

			
			((KKTimer)myObject).FlashC=(((KKTimer)myObject).FlashC+1)%2;
			int FlashC=((KKTimer)myObject).FlashC;
			if(FlashC==0){
				tForm.Opacity=0.5;
			}else{
				tForm.Opacity=0.2;
			}
			((KKTimer)myObject).FlashFirstRun=false; // 執行後, 就不是第一次執行閃爍
			
		}
		// end of 暫停效果

		// 淡出效果並且關閉
		// Usage
		/*
		 * OpacityUtility.FadeOut_Close(this,30);
		 */
        
		public static void FadeOut_Close(System.Windows.Forms.Form target,int Interval){
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 FadeOut 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=1;
			myTimer.Tick += new EventHandler(FadeOut_Close_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 FadeOut 專屬的 Timer 
		}
		private static void FadeOut_Close_TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {

			System.Windows.Forms.Form tForm =((KKTimer)myObject).tForm;

			double OpacityLevel=((KKTimer)myObject).OpacityLevel;
            if (OpacityLevel > ((KKTimer)myObject).FadeOutFinalValue)
            {
				tForm.Opacity=((KKTimer)myObject).OpacityLevel-=0.05;
				
			}else{
				((KKTimer)myObject).Stop();
				((KKTimer)myObject).Dispose();
				tForm.Close();
				//tForm.Dispose();
			}
		}
		// end of  淡出效果

		
		// 主有只有淡出效果
		// Usage:
		/*
		 *    OpacityUtility.FadeOut_Only(MyIndexList,30,0);
		 * 
		 */
        public static void FadeOut_Only(System.Windows.Forms.Form target, int Interval, float FadeOutFinalValue)
        {
            System.Windows.Forms.Form tForm = target;
            KKTimer myTimer;
            // 設定 FadeOut 專屬的 Timer
            myTimer = new KKTimer(tForm);
            myTimer.FadeOutFinalValue = FadeOutFinalValue;
            myTimer.OpacityLevel = 1;
            myTimer.Tick += new EventHandler(FadeOut_Close_TimerEventProcessor);
            myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
            myTimer.Start();
            // end of 設定 FadeOut 專屬的 Timer 
        }
		public static void FadeOut_Only(System.Windows.Forms.Form target,int Interval){
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 FadeOut 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=1;
			myTimer.Tick += new EventHandler(FadeOut_Only_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 FadeOut 專屬的 Timer 
		}
		private static void FadeOut_Only_TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {
			System.Windows.Forms.Form tForm =((KKTimer)myObject).tForm;

			double OpacityLevel=((KKTimer)myObject).OpacityLevel;
            // System.Console.WriteLine(tForm.Opacity);
            if (OpacityLevel > ((KKTimer)myObject).FadeOutFinalValue)
            {
				tForm.Opacity=((KKTimer)myObject).OpacityLevel-=0.05;
                
			}else{
				((KKTimer)myObject).Stop();     // 停止計時器
				((KKTimer)myObject).Dispose();  // 關閉計時器 (以免浪費資源)
				tForm.Opacity=((KKTimer)myObject).OpacityLevel=0;// 保證最後透明度 = 0;
			}
		}
		// end of  只有淡出效果



		// 主有只有淡入效果
		// Usage:
		/*
		 *    OpacityUtility.FadeIn_Only(MyIndexList,30);
		 * 
		 */
		public double FadeIn_FinalValue;
		public void FadeIn_Only(System.Windows.Forms.Form target,int Interval){
			FadeIn_FinalValue=1.0;
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 FadeOut 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=0;       // 設定起始透明度 = 0
			myTimer.Tick += new EventHandler(FadeIn_Only_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 FadeOut 專屬的 Timer 
		}
	
		public void FadeIn_Only(System.Windows.Forms.Form target,int Interval,double TargetValue){
			FadeIn_FinalValue=TargetValue;
			System.Windows.Forms.Form tForm=target;
			KKTimer myTimer;
			// 設定 FadeOut 專屬的 Timer
			myTimer=new KKTimer(tForm);
			myTimer.OpacityLevel=0;       // 設定起始透明度 = 0
			myTimer.Tick += new EventHandler(FadeIn_Only_TimerEventProcessor);			
			myTimer.Interval = Interval; // 每隔 1 秒鐘, 呼叫 TimerEventProcessor procedure
			myTimer.Start();
			// end of 設定 FadeOut 專屬的 Timer 
		}

		private void FadeIn_Only_TimerEventProcessor(Object myObject,
			EventArgs myEventArgs) {

			System.Windows.Forms.Form tForm =((KKTimer)myObject).tForm;

			double OpacityLevel=((KKTimer)myObject).OpacityLevel;
			if(OpacityLevel< FadeIn_FinalValue){
				tForm.Opacity=((KKTimer)myObject).OpacityLevel+=0.05;
			}else{
				((KKTimer)myObject).Stop();     // 停止計時器
				((KKTimer)myObject).Dispose();  // 關閉計時器 (以免浪費資源)
				tForm.Opacity=((KKTimer)myObject).OpacityLevel=FadeIn_FinalValue;// 保證最後透明度 = FadeIn_FinalValue;
			}
		}
		// end of  淡入效果

	}
}
