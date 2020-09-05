using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;

namespace Cobol_SourceAnalysis
{
    public partial class Form1 : Form
    {
        #region 定数
        const int RETURN_OK = 0;
        const int RETURN_ERR_100 = 100;
        const int RETURN_ERR_200 = 200;
        #endregion

        #region 変数
        string OutFileName = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\result.xlsx";
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void refBtn_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if(dr == DialogResult.OK)
            {
                fileList.Items.Clear();
                foreach (var file in openFileDialog1.FileNames)
                {
                    fileList.Items.Add(file);
                }
            }
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            fileList.Items.Clear();
            msgLabel.Text = string.Empty;
        }

        private void execBtn_Click(object sender, EventArgs e)
        {
            msgLabel.Text = string.Empty;
            // ファイルの指定チェック
            if (fileList.Items.Count < 1)
            {
                SetMessage("ファイルを選択してください。", true);
                return;
            }

            List<bool> resultFlg = new List<bool>();
            List<string> resultFileList = new List<string>();
            try
            {
                // リストボックスに指定されたファイル数分繰り返す
                foreach (var item in fileList.Items)
                {
                    // exeを実行
                    Process proc = new Process();
                    proc.StartInfo.FileName =
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + ConfigurationManager.AppSettings["ExecFilePath"];
                    proc.StartInfo.Arguments = item.ToString();
                    proc.Start();
                    proc.WaitForExit();

                    // 処理結果を受け取る
                    bool ret = (proc.ExitCode == RETURN_OK) ? true : false;

                    resultFlg.Add(ret);
                    resultFileList.Add(item.ToString());

                    if (proc.ExitCode == RETURN_ERR_200)
                        break;
                }
                if(String.IsNullOrEmpty(msgLabel.Text))
                    SetMessage("処理終了。", false);
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, true);
            }
            finally
            {
                // 処理結果ウィンドウを表示
                Result result = new Result(resultFlg, resultFileList);
                result.Show();
            }
        }

        /// <summary>
        /// メッセージセット
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="errorFlg"></param>
        private void SetMessage(string msg, bool errorFlg)
        {
            if (String.IsNullOrEmpty(msg))
                return;

            if (errorFlg)
                msgLabel.ForeColor = Color.Red;
            else
                msgLabel.ForeColor = Color.Empty;

            msgLabel.Text = msg;
        }
    }
}
