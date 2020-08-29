namespace Cobol_SourceAnalysis
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.refBtn = new System.Windows.Forms.Button();
            this.execBtn = new System.Windows.Forms.Button();
            this.msgLabel = new System.Windows.Forms.Label();
            this.fileList = new System.Windows.Forms.ListBox();
            this.clearBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // refBtn
            // 
            this.refBtn.AllowDrop = true;
            this.refBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refBtn.Location = new System.Drawing.Point(26, 14);
            this.refBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.refBtn.Name = "refBtn";
            this.refBtn.Size = new System.Drawing.Size(87, 30);
            this.refBtn.TabIndex = 1;
            this.refBtn.Text = "ファイル選択";
            this.refBtn.UseVisualStyleBackColor = true;
            this.refBtn.Click += new System.EventHandler(this.refBtn_Click);
            // 
            // execBtn
            // 
            this.execBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.execBtn.Location = new System.Drawing.Point(304, 14);
            this.execBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.execBtn.Name = "execBtn";
            this.execBtn.Size = new System.Drawing.Size(87, 30);
            this.execBtn.TabIndex = 3;
            this.execBtn.Text = "実行";
            this.execBtn.UseVisualStyleBackColor = true;
            this.execBtn.Click += new System.EventHandler(this.execBtn_Click);
            // 
            // msgLabel
            // 
            this.msgLabel.Location = new System.Drawing.Point(23, 198);
            this.msgLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.msgLabel.Name = "msgLabel";
            this.msgLabel.Size = new System.Drawing.Size(367, 32);
            this.msgLabel.TabIndex = 3;
            // 
            // fileList
            // 
            this.fileList.FormattingEnabled = true;
            this.fileList.HorizontalScrollbar = true;
            this.fileList.ItemHeight = 15;
            this.fileList.Location = new System.Drawing.Point(26, 54);
            this.fileList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(364, 139);
            this.fileList.TabIndex = 4;
            this.fileList.TabStop = false;
            // 
            // clearBtn
            // 
            this.clearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearBtn.Location = new System.Drawing.Point(125, 14);
            this.clearBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(87, 30);
            this.clearBtn.TabIndex = 2;
            this.clearBtn.Text = "クリア";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.ClientSize = new System.Drawing.Size(419, 240);
            this.Controls.Add(this.clearBtn);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.msgLabel);
            this.Controls.Add(this.execBtn);
            this.Controls.Add(this.refBtn);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "Form1";
            this.Text = "COBOLソース解析";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button refBtn;
        private System.Windows.Forms.Button execBtn;
        private System.Windows.Forms.Label msgLabel;
        private System.Windows.Forms.ListBox fileList;
        private System.Windows.Forms.Button clearBtn;
    }
}

