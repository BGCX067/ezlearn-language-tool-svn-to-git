namespace Player
{
    partial class FileListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileListForm));
            this.listBox_FileList = new System.Windows.Forms.ListBox();
            this.checkBox_Loop = new System.Windows.Forms.CheckBox();
            this.Button_Load = new System.Windows.Forms.Button();
            this.Button_Save = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox_FileList
            // 
            this.listBox_FileList.FormattingEnabled = true;
            this.listBox_FileList.Location = new System.Drawing.Point(2, 7);
            this.listBox_FileList.Name = "listBox_FileList";
            this.listBox_FileList.Size = new System.Drawing.Size(301, 238);
            this.listBox_FileList.TabIndex = 0;
            this.listBox_FileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox_FileList_MouseDoubleClick);
            this.listBox_FileList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listBox_FileList_MouseClick);
            this.listBox_FileList.SelectedIndexChanged += new System.EventHandler(this.listBox_FileList_SelectedIndexChanged);
            this.listBox_FileList.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox_FileList_DragDrop);
            this.listBox_FileList.MouseEnter += new System.EventHandler(this.listBox_FileList_MouseEnter);
            this.listBox_FileList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox_FileList_MouseDown);
            this.listBox_FileList.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_FileList_DragEnter);
            this.listBox_FileList.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listBox_FileList_KeyPress);
            this.listBox_FileList.MouseLeave += new System.EventHandler(this.listBox_FileList_MouseLeave);
            this.listBox_FileList.DragLeave += new System.EventHandler(this.listBox_FileList_DragLeave);
            this.listBox_FileList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox_FileList_KeyDown);
            // 
            // checkBox_Loop
            // 
            this.checkBox_Loop.AutoSize = true;
            this.checkBox_Loop.Location = new System.Drawing.Point(94, 256);
            this.checkBox_Loop.Name = "checkBox_Loop";
            this.checkBox_Loop.Size = new System.Drawing.Size(200, 17);
            this.checkBox_Loop.TabIndex = 5;
            this.checkBox_Loop.Text = "單一檔案片段播放完畢, 繼續播放";
            this.checkBox_Loop.UseVisualStyleBackColor = true;
            this.checkBox_Loop.MouseClick += new System.Windows.Forms.MouseEventHandler(this.checkBox_Loop_MouseClick);
            this.checkBox_Loop.CheckedChanged += new System.EventHandler(this.checkBox_Loop_CheckedChanged);
            // 
            // Button_Load
            // 
            this.Button_Load.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.Button_Load.Image = ((System.Drawing.Image)(resources.GetObject("Button_Load.Image")));
            this.Button_Load.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.Button_Load.Location = new System.Drawing.Point(1, 263);
            this.Button_Load.Name = "Button_Load";
            this.Button_Load.Size = new System.Drawing.Size(25, 27);
            this.Button_Load.TabIndex = 2;
            this.Button_Load.UseVisualStyleBackColor = true;
            this.Button_Load.Click += new System.EventHandler(this.Button_Load_Click);
            // 
            // Button_Save
            // 
            this.Button_Save.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.Button_Save.Image = ((System.Drawing.Image)(resources.GetObject("Button_Save.Image")));
            this.Button_Save.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.Button_Save.Location = new System.Drawing.Point(32, 263);
            this.Button_Save.Name = "Button_Save";
            this.Button_Save.Size = new System.Drawing.Size(25, 27);
            this.Button_Save.TabIndex = 1;
            this.Button_Save.UseVisualStyleBackColor = true;
            this.Button_Save.Click += new System.EventHandler(this.Button_Save_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(94, 273);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(74, 17);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "片段播放";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            this.checkBox1.Click += new System.EventHandler(this.checkBox1_Click);
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(63, 263);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 27);
            this.button1.TabIndex = 7;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FileListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 290);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.checkBox_Loop);
            this.Controls.Add(this.Button_Load);
            this.Controls.Add(this.Button_Save);
            this.Controls.Add(this.listBox_FileList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(311, 322);
            this.Name = "FileListForm";
            this.Text = "聽力轟炸檔案列表";
            this.Deactivate += new System.EventHandler(this.FileListForm_Deactivate);
            this.Load += new System.EventHandler(this.FileListForm_Load);
            this.MouseEnter += new System.EventHandler(this.FileListForm_MouseEnter);
            this.Activated += new System.EventHandler(this.FileListForm_Actived);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FileListForm_FormClosed);
            this.MouseLeave += new System.EventHandler(this.FileListForm_MouseLeave);
            this.Move += new System.EventHandler(this.FileListForm_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox_FileList;
        private System.Windows.Forms.Button Button_Save;
        private System.Windows.Forms.Button Button_Load;
        private System.Windows.Forms.CheckBox checkBox_Loop;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
    }
}