
namespace TimerControl
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.FileListView = new System.Windows.Forms.ListView();
            this.FileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CreateDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FileType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FileImageList = new System.Windows.Forms.ImageList(this.components);
            this.FilePath = new System.Windows.Forms.TextBox();
            this.ConnList = new System.Windows.Forms.ComboBox();
            this.Btn_UpLevel = new System.Windows.Forms.Button();
            this.Btn_DeleteFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FileListView
            // 
            this.FileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileName,
            this.CreateDate,
            this.FileType,
            this.Size});
            this.FileListView.FullRowSelect = true;
            this.FileListView.HideSelection = false;
            this.FileListView.HoverSelection = true;
            this.FileListView.Location = new System.Drawing.Point(12, 110);
            this.FileListView.MultiSelect = false;
            this.FileListView.Name = "FileListView";
            this.FileListView.Size = new System.Drawing.Size(776, 294);
            this.FileListView.SmallImageList = this.FileImageList;
            this.FileListView.TabIndex = 0;
            this.FileListView.UseCompatibleStateImageBehavior = false;
            this.FileListView.View = System.Windows.Forms.View.Details;
            this.FileListView.DoubleClick += new System.EventHandler(this.FileListView_DoubleClick);
            // 
            // FileName
            // 
            this.FileName.Text = "名称";
            this.FileName.Width = 294;
            // 
            // CreateDate
            // 
            this.CreateDate.Text = "创建时间";
            this.CreateDate.Width = 183;
            // 
            // FileType
            // 
            this.FileType.Text = "类型";
            this.FileType.Width = 145;
            // 
            // Size
            // 
            this.Size.Text = "大小";
            this.Size.Width = 150;
            // 
            // FileImageList
            // 
            this.FileImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("FileImageList.ImageStream")));
            this.FileImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.FileImageList.Images.SetKeyName(0, "Word.png");
            this.FileImageList.Images.SetKeyName(1, "Excel.jpg");
            this.FileImageList.Images.SetKeyName(2, "TxtIcon.jpg");
            this.FileImageList.Images.SetKeyName(3, "Rar.png");
            this.FileImageList.Images.SetKeyName(4, "Sql.jpg");
            this.FileImageList.Images.SetKeyName(5, "C#.jpg");
            this.FileImageList.Images.SetKeyName(6, "NullIcon.jpg");
            this.FileImageList.Images.SetKeyName(7, "FolderIcon.jpg");
            this.FileImageList.Images.SetKeyName(8, "HDD.png");
            // 
            // FilePath
            // 
            this.FilePath.Location = new System.Drawing.Point(12, 84);
            this.FilePath.Name = "FilePath";
            this.FilePath.Size = new System.Drawing.Size(776, 20);
            this.FilePath.TabIndex = 1;
            this.FilePath.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FilePath_KeyUp);
            // 
            // ConnList
            // 
            this.ConnList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ConnList.FormattingEnabled = true;
            this.ConnList.Location = new System.Drawing.Point(632, 12);
            this.ConnList.Name = "ConnList";
            this.ConnList.Size = new System.Drawing.Size(156, 21);
            this.ConnList.TabIndex = 2;
            this.ConnList.SelectedIndexChanged += new System.EventHandler(this.ConnList_SelectedIndexChanged);
            // 
            // Btn_UpLevel
            // 
            this.Btn_UpLevel.Location = new System.Drawing.Point(84, 411);
            this.Btn_UpLevel.Name = "Btn_UpLevel";
            this.Btn_UpLevel.Size = new System.Drawing.Size(75, 23);
            this.Btn_UpLevel.TabIndex = 3;
            this.Btn_UpLevel.Text = "上一级";
            this.Btn_UpLevel.UseVisualStyleBackColor = true;
            this.Btn_UpLevel.Click += new System.EventHandler(this.UpLevel_Click);
            // 
            // Btn_DeleteFile
            // 
            this.Btn_DeleteFile.Location = new System.Drawing.Point(189, 411);
            this.Btn_DeleteFile.Name = "Btn_DeleteFile";
            this.Btn_DeleteFile.Size = new System.Drawing.Size(75, 23);
            this.Btn_DeleteFile.TabIndex = 4;
            this.Btn_DeleteFile.Text = "删除";
            this.Btn_DeleteFile.UseVisualStyleBackColor = true;
            this.Btn_DeleteFile.Click += new System.EventHandler(this.Btn_DeleteFile_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Btn_DeleteFile);
            this.Controls.Add(this.Btn_UpLevel);
            this.Controls.Add(this.ConnList);
            this.Controls.Add(this.FilePath);
            this.Controls.Add(this.FileListView);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView FileListView;
        private System.Windows.Forms.ColumnHeader FileName;
        private System.Windows.Forms.ColumnHeader CreateDate;
        private System.Windows.Forms.ColumnHeader FileType;
        private System.Windows.Forms.ColumnHeader Size;
        private System.Windows.Forms.TextBox FilePath;
        private System.Windows.Forms.ComboBox ConnList;
        private System.Windows.Forms.ImageList FileImageList;
        private System.Windows.Forms.Button Btn_UpLevel;
        private System.Windows.Forms.Button Btn_DeleteFile;
    }
}

