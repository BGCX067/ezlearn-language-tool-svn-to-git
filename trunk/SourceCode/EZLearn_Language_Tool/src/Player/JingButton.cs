using System;
using System.Windows.Forms;

namespace Player
{
	/// <summary>
	/// Summary description for JingButton.
	/// </summary>
	public class JingButton:System.Windows.Forms.Button
	{
		bool bSelfPaint=false;
		public JingButton()
		{
			
		}
		public void setSelfPaint(){
			bSelfPaint=true;
		}
		protected override void OnPaint(PaintEventArgs e){
			if(this.Image != null && bSelfPaint) {
				e.Graphics.DrawImage(this.Image,new System.Drawing.Point(0,0));
			}else{
				base.OnPaint(e);
			}
		}
	}
}
