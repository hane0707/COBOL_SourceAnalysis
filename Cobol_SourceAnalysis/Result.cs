using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cobol_SourceAnalysis
{
    public partial class Result : Form
    {
        List<int> ngFileIndex = new List<int>();
        public Result(List<bool> successFlg, List<string> fileList)
        {
            InitializeComponent();

            for (int i = 0; i < successFlg.Count; i++)
            {
                resultFileList.Items.Add((successFlg[i]?"ＯＫ　": "ＮＧ　") + fileList[i]);

                if(!successFlg[i])
                    ngFileIndex.Add(i);
            }
        }

        private void resultOkBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void resultFileList_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush brush = new SolidBrush(Color.Black);
            foreach (var index in ngFileIndex)
            {
                if (e.Index == index)
                    brush = new SolidBrush(Color.Red);
            }

            e.Graphics.DrawString(resultFileList.Items[e.Index].ToString()
                , e.Font, brush, e.Bounds, StringFormat.GenericDefault);
        }
    }
}
