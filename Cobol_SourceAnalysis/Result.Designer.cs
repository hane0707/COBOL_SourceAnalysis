namespace Cobol_SourceAnalysis
{
    partial class Result
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
            this.resultFileList = new System.Windows.Forms.ListBox();
            this.resultOkBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resultFileList
            // 
            this.resultFileList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.resultFileList.FormattingEnabled = true;
            this.resultFileList.HorizontalScrollbar = true;
            this.resultFileList.ItemHeight = 15;
            this.resultFileList.Location = new System.Drawing.Point(15, 21);
            this.resultFileList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.resultFileList.Name = "resultFileList";
            this.resultFileList.Size = new System.Drawing.Size(529, 169);
            this.resultFileList.TabIndex = 5;
            this.resultFileList.TabStop = false;
            this.resultFileList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.resultFileList_DrawItem);
            // 
            // resultOkBtn
            // 
            this.resultOkBtn.AllowDrop = true;
            this.resultOkBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resultOkBtn.Location = new System.Drawing.Point(457, 196);
            this.resultOkBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.resultOkBtn.Name = "resultOkBtn";
            this.resultOkBtn.Size = new System.Drawing.Size(87, 30);
            this.resultOkBtn.TabIndex = 6;
            this.resultOkBtn.Text = "OK";
            this.resultOkBtn.UseVisualStyleBackColor = true;
            this.resultOkBtn.Click += new System.EventHandler(this.resultOkBtn_Click);
            // 
            // Result
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(559, 240);
            this.Controls.Add(this.resultOkBtn);
            this.Controls.Add(this.resultFileList);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Result";
            this.Text = "実行結果";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox resultFileList;
        private System.Windows.Forms.Button resultOkBtn;
    }
}