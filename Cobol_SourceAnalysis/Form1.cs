﻿using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cobol_SourceAnalysis
{
    public partial class Form1 : Form
    {
        #region 定数
        const string COM_PREFIX = "*";
        const string METHOD_START = "SECTION";
        const string METHOD_END = "-999";
        const string FONT_NAME = "Meiryo UI";
        const string SHEET_NAME_PGMINFO = "PGM情報";
        const string SHEET_NAME_METHODINFO = "関数情報";
        const string SHEET_NAME_STRUCT = "構造図";
        const string OUT_FILE_NAME = "result.xlsx";
        #endregion

        #region 変数
        Encoding enc = Encoding.GetEncoding("Shift_JIS");
        int InitRow = 2;
        string sheetName = string.Empty;
        ExcelWorksheet WsPgmInfo; // プログラム情報シート
        ExcelWorksheet WsStruct; // 構造図シート
        ExcelWorksheet WsMethodInfo; // 関数情報シート
        Color ColorMethod = Color.RoyalBlue;
        Color ColorModule = Color.DeepPink;
        #endregion

        enum SqlType
        {
            None = 0,
            Select = 1,
            Insert = 2,
            Update = 3,
            Delete = 4
        }

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

            // 出力ファイルが使用中ではないかチェック
            try
            {
                using (Stream st = new FileStream(OUT_FILE_NAME, FileMode.Open)) { }
            }
            catch (Exception)
            {
                SetMessage("出力ファイル(" + OUT_FILE_NAME + ")が使用中です。", true);
                return;
            }

            List<bool> resultFlg = new List<bool>();
            List<string> resultFileList = new List<string>();
            try
            {
                // リストボックスに指定されたファイル数分繰り返す
                foreach (var item in fileList.Items)
                {
                    bool ret = Main(item.ToString());

                    resultFlg.Add(ret);
                    resultFileList.Add(item.ToString());
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

        private bool Main(string file)
        {
            List<Method> methodList = new List<Method>();

            try
            {
                Dictionary<string, string> copyList = new Dictionary<string, string>();
                Dictionary<string, string> sqlDeclareList = new Dictionary<string, string>();
                List<string> calledModuleList = new List<string>();

                // =================================================================
                // 前処理①（関数の一覧をリスト化）
                // =================================================================
                #region 前処理①
                using (StreamReader sr = new StreamReader(file, enc))
                {
                    int fileIndex = 0;
                    int methodIndex = -1;
                    bool inDbDeclareErea = false;
                    bool inMethodErea = false;
                    bool inSqlErea = false;
                    List<string> sql = new List<string>();

                    while (sr.Peek() >= 0)
                    {
                        fileIndex++;

                        // 読み込んだ行を整形する
                        string fmtLine = FormatLine(sr.ReadLine(), false);

                        // テキストをスペースで区切った配列を作成
                        string[] arrWord = fmtLine.Split(' ');

                        // 対象外句はスルー
                        if (!CheckLine(arrWord))
                        {
                            continue;
                        }

                        // コピー句を特定（特定行の1行手前がコメント行だと仮定する）
                        if (arrWord[0] == "COPY")
                        {
                            string copyKey = GetArrayWord(arrWord, 1);
                            string comLine = File.ReadAllLines(file, enc).Skip(fileIndex - 2).Take(1).First();
                            if(!copyList.ContainsKey(copyKey))
                                copyList.Add(copyKey, FormatLine(comLine, true));
                            continue;
                        }

                        // SQL変数宣言部（開始）の特定
                        if (String.Join(" ", arrWord) == "EXEC SQL BEGIN DECLARE SECTION END-EXEC")
                        {
                            inDbDeclareErea = true;
                            continue;
                        }

                        if (inDbDeclareErea)
                        {
                            // SQL変数宣言部の特定（特定行の1行手前がコメント行だと仮定する）
                            string sqlDeclareKey = GetArrayWord(arrWord, 3);
                            if (String.Join(" ", arrWord) == "EXEC SQL COPY " + sqlDeclareKey + " END-EXEC")
                            {
                                string comLine = File.ReadAllLines(file, enc).Skip(fileIndex - 2).Take(1).First();
                                if (!sqlDeclareList.ContainsKey(sqlDeclareKey))
                                    sqlDeclareList.Add(sqlDeclareKey, FormatLine(comLine, true));
                                continue;
                            }

                            // SQL変数宣言部（終了）の特定
                            if (String.Join(" ", arrWord) == "EXEC SQL END DECLARE SECTION END-EXEC")
                            {
                                inDbDeclareErea = false;
                                continue;
                            }
                        }

                        // 関数の開始行を特定
                        if (arrWord[arrWord.Length - 1] == METHOD_START)
                        {
                            methodIndex++;
                            Method m = new Method(arrWord[0], fileIndex, -1);
                            methodList.Add(m);
                            inMethodErea = true;

                            // 関数名の論理名の特定（関数開始行の2行手前がコメント行だと仮定する）
                            // ※関数開始行の3行前までを読み飛ばし、次の1行（＝コメント行）を読み込む
                            string comLine = File.ReadAllLines(file, enc).Skip(fileIndex - 3).Take(1).First();
                            methodList[methodIndex].MethodNameL = FormatLine(comLine, true);
                            continue;
                        }

                        if (inMethodErea)
                        {
                            // 呼出関数・モジュールを特定
                            if ((arrWord[0] == "PERFORM" && arrWord[1] != "VARYING") || arrWord[0] == "CALL")
                            {
                                bool moduleFlg = (arrWord[0] == "CALL") ? true : false;
                                string name = arrWord[1].Replace("'", "");

                                if (moduleFlg)
                                    calledModuleList.Add(name);

                                methodList[methodIndex].CalledMethod.Add(new CalledMethod(name, moduleFlg));
                                continue;
                            }

                            // SQL開始行を特定
                            if (String.Join(" ", arrWord) == "EXEC SQL")
                            {
                                inSqlErea = true;
                                continue;
                            }

                            if (inSqlErea)
                            {
                                // SQLの処理区分を特定
                                switch (arrWord[0])
                                {
                                    case "SELECT":
                                        methodList[methodIndex].ExecSqlType =
                                            (methodList[methodIndex].ExecSqlType == string.Empty) ? arrWord[0] : string.Empty;
                                        break;
                                    case "INSERT":
                                        methodList[methodIndex].ExecSqlType =
                                            (methodList[methodIndex].ExecSqlType == string.Empty) ? arrWord[0] : string.Empty;
                                        break;
                                    case "UPDATE":
                                        methodList[methodIndex].ExecSqlType =
                                            (methodList[methodIndex].ExecSqlType == string.Empty) ? arrWord[0] : string.Empty;
                                        break;
                                    case "DELETE":
                                        methodList[methodIndex].ExecSqlType =
                                            (methodList[methodIndex].ExecSqlType == string.Empty) ? arrWord[0] : string.Empty;
                                        break;
                                    default:
                                        break;
                                }

                                // SQL終了行を特定
                                if (arrWord[0] == "END-EXEC")
                                {
                                    inSqlErea = false;
                                    continue;
                                }
                            }

                            // 関数の終了行を特定
                            if (arrWord[0] == methodList[methodIndex].MethodNameP + METHOD_END)
                            {
                                methodList[methodIndex].CalledMethod = methodList[methodIndex].CalledMethod.Distinct().ToList();
                                methodList[methodIndex].EndIndex = fileIndex;
                                inMethodErea = false;
                                continue;
                            }
                        }
                    }
                }

                // モジュール内に関数がない場合は処理終了
                if (methodList.Count <= 0)
                {
                    SetMessage("関数が一つもありません。", true);
                    return false;
                }
                #endregion

                // =================================================================
                // 前処理②（未使用の関数を特定）
                // =================================================================
                #region 前処理②
                // 関数名がリスト内のその他の関数から呼び出されているかチェックする
                foreach (var method1 in methodList)
                {
                    int index = methodList.IndexOf(method1);
                    if (index == 0) { method1.CalledFlg = true; }
                    foreach (var method2 in methodList)
                    {
                        if (index == methodList.IndexOf(method2)) { continue; }
                        foreach (var cm in method2.CalledMethod)
                        {
                            if (method1.MethodNameP == cm.Name)
                            {
                                method1.CalledFlg = true;

                                // 呼出関数の関数リスト内でのindexをセット
                                cm.MethodListIndex = index;
                                continue;
                            }
                        }
                    }
                }

                // 未使用関数から呼び出される関数のリストを作成
                List<Method> calledMethodList = new List<Method>();
                foreach (var method1 in methodList)
                {
                    // 呼出フラグがfalseかつ、呼出先がある関数が対象
                    if (method1.CalledFlg) { continue; }
                    if (method1.CalledMethod.Count < 1) { continue; }

                    foreach (var cm in method1.CalledMethod)
                    {
                        calledMethodList.Add(methodList[cm.MethodListIndex]);
                    }
                }

                // 呼出先リストにある関数が、呼出フラグfalseの関数からのみ呼び出されていれば呼出フラグfalseにする
                foreach (var method1 in calledMethodList)
                {
                    // 呼出フラグをいったんfalseに
                    method1.CalledFlg = false;
                    int index = methodList.IndexOf(method1);
                    foreach (var method2 in methodList)
                    {
                        if (index == methodList.IndexOf(method2)) { continue; }
                        foreach (var cm in method2.CalledMethod)
                        {
                            if (method1.MethodNameP == cm.Name && method2.CalledFlg)
                            {
                                // 呼出元関数の呼出フラグが一つでもtrueなら呼出先関数もtrueとする
                                method1.CalledFlg = true;
                                break;
                            }
                        }
                        if (method1.CalledFlg) { break; }
                    }
                }
                #endregion

                // =================================================================
                // Excelファイル作成
                // =================================================================
                // 出力ファイル準備（実行ファイルと同じフォルダに出力される）
                FileInfo newFile = new FileInfo(OUT_FILE_NAME);

                // Excelファイル作成
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    bool ret = false;

                    // プログラム情報シート作成・編集（シート名：PGM情報_{読込ファイル名}）
                    sheetName = SHEET_NAME_PGMINFO + "_" + Path.GetFileNameWithoutExtension(file);
                    if (package.Workbook.Worksheets[sheetName] != null)
                        package.Workbook.Worksheets.Delete(sheetName);

                    WsPgmInfo = package.Workbook.Worksheets.Add(sheetName);
                    ret = EditPgmInfoSheet(copyList, sqlDeclareList, calledModuleList);

                    if (!ret) { return false; }

                    // 関数情報シート作成・編集（シート名：関数情報_{読込ファイル名}）
                    sheetName = SHEET_NAME_METHODINFO + "_" + Path.GetFileNameWithoutExtension(file);
                    if (package.Workbook.Worksheets[sheetName] != null)
                        package.Workbook.Worksheets.Delete(sheetName);

                    WsMethodInfo = package.Workbook.Worksheets.Add(sheetName);
                    ret = EditMethodInfoSheet(methodList);

                    if (!ret) { return false; }

                    // 構造図シート作成・編集（シート名：構造図_{読込ファイル名}）
                    sheetName = SHEET_NAME_STRUCT + "_" + Path.GetFileNameWithoutExtension(file);
                    if (package.Workbook.Worksheets[sheetName] != null)
                        package.Workbook.Worksheets.Delete(sheetName);

                    WsStruct = package.Workbook.Worksheets.Add(sheetName);
                    ret = EditStructSheet(methodList);

                    if (!ret) { return false; }

                    // 保存
                    package.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, true);
                return false;
            }
        }

        #region 前処理
        /// <summary>
        /// テキストを整形する
        /// </summary>
        /// <param name="line">指定行のテキスト</param>
        /// <param name="exceptComFlg">整形時に標準領域を除くフラグ</param>
        /// <returns></returns>
        private string FormatLine(string line, bool exceptComFlg)
        {
            int start = 7;
            if (exceptComFlg)
            {
                start = 8;
            }
            // 一連番号領域、プログラム識別領域を取り除く（"000001  hoge   fuga " ⇒ "hoge   fuga "）
            string frmLine = Mid(line.Trim(), start, 66);
            // テキスト前後のスペースを詰め、テキスト内にある複数のスペースを取り除く（"hoge   fuga" ⇒ "hoge fuga"）
            frmLine = Regex.Replace(frmLine.Trim().Replace(".", ""), @"\s{2,}", " ");

            return frmLine;
        }

        /// <summary>
        /// 対象外句の判定
        /// </summary>
        /// <param name="arrWord"></param>
        /// <returns></returns>
        private bool CheckLine(string[] arrWord)
        {
            string checkText = arrWord[0].Replace(".", "");

            // 空行、コメント行、SQL行、その他対象外句はスルー
            if (checkText == string.Empty || Left(checkText, 1) == COM_PREFIX
                    // 見出し部
                    || checkText == "IDENTIFICATION" || checkText == "PROGRAM-ID"
                    || checkText == "AUTHOR" || checkText == "DATE-WRITTEN"
                    || checkText == "DATE-COMPILED"
                    // 環境部
                    || checkText == "ENVIRONMENT" || checkText == "CONFIGURATION"
                    || checkText == "SOURCE-COMPUTER" || checkText == "OBJECT-COMPUTER"
                    || checkText == "INPUT-OUTPUT" || checkText == "FILE-CONTROL"
                    // データ部
                    || checkText == "DATA" || checkText == "FILE"
                    || checkText == "WORKING-STORAGE" || checkText == "LINKAGE"
                    || checkText == "REPORT" || checkText == "SCREEN"
                    // 手続き部
                    || checkText == "DISPLAY")
            {
                return false;
            }

            return true;

        }
        #endregion

        #region プログラム情報描画
        private bool EditPgmInfoSheet(Dictionary<string, string> copyList
                                    , Dictionary<string, string> sqlDeclareList
                                    , List<string> calledModuleList)
        {
            string errorMsg = "(" + sheetName + "シート作成時エラー)";
            if (WsPgmInfo == null)
            {
                SetMessage("Excelシート変数に値が割り当てられませんでした。" + errorMsg, true);
                return false;
            }

            try
            {
                int row = 2; // 行番号

                // 各項目のタイトル部分を書き込む
                // コピー句のタイトルセット
                SetStyleOfTitle(SHEET_NAME_PGMINFO, "B2:C2", Color.SpringGreen);
                WsPgmInfo.Cells[row, 2].Value = "COPY句";
                // SQL変数宣言部のタイトルセット
                SetStyleOfTitle(SHEET_NAME_PGMINFO, "D2:E2", Color.LightSteelBlue);
                WsPgmInfo.Cells[row, 4].Value = "使用テーブル";
                // 呼出モジュールのタイトルセット
                SetStyleOfTitle(SHEET_NAME_PGMINFO, "F2:F2", Color.Pink);
                WsPgmInfo.Cells[row, 6].Value = "呼出モジュール";

                // コピー句リストを書き込む
                foreach (var copy in copyList)
                {
                    row++;
                    WsPgmInfo.Cells[row, 2].Value = copy.Key;
                    WsPgmInfo.Cells[row, 3].Value = copy.Value;
                }

                // SQL変数宣言部リストを書き込む
                row = 2;
                foreach (var sql in sqlDeclareList)
                {
                    row++;
                    WsPgmInfo.Cells[row, 4].Value = sql.Key;
                    WsPgmInfo.Cells[row, 5].Value = sql.Value;
                }

                // 呼出モジュールリストを書き込む
                row = 2;
                foreach (var module in calledModuleList)
                {
                    row++;
                    WsPgmInfo.Cells[row, 6].Value = module;
                }

                WsPgmInfo.Cells.Style.Font.Name = FONT_NAME;
                WsPgmInfo.Cells[WsPgmInfo.Dimension.Address].AutoFitColumns(); // 列幅自動調整
            }
            catch (Exception ex)
            {
                msgLabel.Text = ex.Message + errorMsg;
                return false;
            }

            return true;
        }
        #endregion

        #region 関数情報描画
        /// <summary>
        /// 関数情報シート編集
        /// </summary>
        /// <param name="methodList"></param>
        /// <returns></returns>
        private bool EditMethodInfoSheet(List<Method> methodList)
        {
            string errorMsg = "(" + sheetName +  "シート作成時エラー)";
            if (WsMethodInfo == null)
            {
                SetMessage("Excelシート変数に値が割り当てられませんでした。" + errorMsg, true);
                return false;
            }

            try
            {
                int row = 2; // 行番号
                int col = 2; // 列番号

                // 各項目のタイトル部分を書き込む
                SetStyleOfTitle(SHEET_NAME_METHODINFO, "B2:Y2", Color.SpringGreen);
                WsMethodInfo.Cells[row, col].Value = "関数名(物理)";
                WsMethodInfo.Cells[row, col + 1].Value = "関数名(論理)";
                WsMethodInfo.Cells[row, col + 2].Value = "開始行数";
                WsMethodInfo.Cells[row, col + 3].Value = "終了行数";
                WsMethodInfo.Cells[row, col + 4].Value = "DB更新処理";

                int calledMethodCol = 7;
                for (int i = 1; i <= 20; i++)
                {
                    WsMethodInfo.Cells[row, calledMethodCol].Value = "呼出関数" + i.ToString();
                    calledMethodCol++;
                }

                // 関数リストを書き込む
                foreach (var method in methodList)
                {
                    calledMethodCol = 7;
                    row++;

                    // 関数物理名
                    WsMethodInfo.Cells[row, col].Value = method.MethodNameP;
                    // 関数論理名
                    WsMethodInfo.Cells[row, col + 1].Value = method.MethodNameL;
                    // 開始行数
                    WsMethodInfo.Cells[row, col + 2].Value = method.StartIndex;
                    // 終了行数
                    WsMethodInfo.Cells[row, col + 3].Value = method.EndIndex;
                    // DB更新区分
                    WsMethodInfo.Cells[row, col + 4].Value = method.ExecSqlType;
                    // 呼出関数
                    foreach (var cm in method.CalledMethod)
                    {
                        Color outColor = cm.ModuleFlg ? ColorModule : ColorMethod;
                        WsMethodInfo.Cells[row, calledMethodCol].Value = cm.Name;
                        WsMethodInfo.Cells[row, calledMethodCol].Style.Font.Color.SetColor(outColor);
                        calledMethodCol++;
                    }

                    // 到達不能な関数の場合
                    if (!method.CalledFlg)
                    {
                        // コメントをセット
                        WsMethodInfo.Cells[row, col].AddComment("到達不能な関数です。", "system");
                        // 背景色変更
                        WsMethodInfo.Cells[row, col, row, col + 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        WsMethodInfo.Cells[row, col, row, col + 23].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    }

                }

                WsMethodInfo.Cells.Style.Font.Name = FONT_NAME;
                WsMethodInfo.Cells[WsMethodInfo.Dimension.Address].AutoFitColumns(); // 列幅自動調整
            }
            catch (Exception ex)
            {
                msgLabel.Text = ex.Message + errorMsg;
                return false;
            }

            return true;
        }
        #endregion

        #region 構造図描画
        /// <summary>
        /// 構造図シート編集
        /// </summary>
        /// <param name="methodList"></param>
        /// <returns></returns>
        private bool EditStructSheet(List<Method> methodList)
        {
            string errorMsg = "(" + sheetName + "シート作成時エラー)";
            if (WsStruct == null)
            {
                SetMessage("Excelシート変数に値が割り当てられませんでした。" + errorMsg, true);
                return false;
            }

            try
            {
                int row = 2; // 行番号
                int col = 2; // 列番号

                // セルを方眼紙にする
                WsStruct.DefaultColWidth = 3;

                // 起点となる関数名をExcellに書き込む
                WriteMethod(methodList, 0, row, col, new CalledMethod(string.Empty, false));

                // 呼び出される関数名を再帰的にExcelに書き込む
                GetCalledMethodRecursively(methodList, 0, row + 1, col + 1);

                WsStruct.Cells.Style.Font.Name = FONT_NAME;
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message + errorMsg, true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Excelに関数名を書き込む
        /// </summary>
        /// <param name="methodList"></param>
        /// <param name="index"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void WriteMethod(List<Method> methodList, int index, int row, int col, CalledMethod cm)
        {
            string outText = string.Empty;
            Color outColor = ColorMethod;
            WsStruct.Cells[row, col].Style.Numberformat.Format = "@";
            if(cm.ModuleFlg)
            {
                // モジュール呼出の場合
                outText = cm.Name;
                outColor = ColorModule;
            }
            else if(index < 0 || index > methodList.Count)
            {
                outText = "不正な関数が指定されました。(" + cm.Name + ")";
                outColor = Color.Red;
            }
            else
            {
                // 関数呼出の場合
                outText = methodList[index].MethodNameP;

                // ハイパーリンクの設定
                string linkSheetName = sheetName.Replace(SHEET_NAME_STRUCT, SHEET_NAME_METHODINFO);
                WsStruct.Cells[row, col].Hyperlink
                    = new ExcelHyperLink("#'" + linkSheetName + "'!B" + (index + 3).ToString(), outText);
                WsStruct.Cells[row, col].Style.Font.UnderLine = true;
            }

            WsStruct.Cells[row, col].Value = outText;
            WsStruct.Cells[row, col].Style.Font.Color.SetColor(outColor);

            //ExcelShape es = WsStruct.Drawings.AddShape(outText + shapeNo.ToString(), eShapeStyle.FlowChartProcess);
            //es.SetPosition(row, 0, col, 0);
            //es.SetSize(100, 50);
            //es.Text = outText;
            //es.Fill.Style = eFillStyle.SolidFill;
            //es.Fill.Color = Color.Red;
            //shapeNo++;
        }

        /// <summary>
        /// 再帰的に呼出関数を取得する
        /// </summary>
        /// <param name="methodList"></param>
        /// <param name="index"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void GetCalledMethodRecursively(List<Method> methodList, int index, int row, int col)
        {
            // 関数内から呼び出している他関数・モジュールを読み込む
            foreach (var calledMethod in methodList[index].CalledMethod)
            {
                // 呼出関数名の、関数リスト内での位置を取得
                int calledMethodIndex = GetMethodIndex(methodList, calledMethod);

                // Excelに書き込む
                WriteMethod(methodList, calledMethodIndex, row, col, calledMethod);

                row++;
                InitRow = row;

                if (calledMethodIndex > -1)
                {
                    // 呼出先の関数内から呼び出している関数がないか、再帰的に探索する
                    GetCalledMethodRecursively(methodList, calledMethodIndex, row, col + 1);
                }
                row = InitRow;
            }
        }

        /// <summary>
        /// 指定された関数名の関数リスト内での位置を返す
        /// </summary>
        /// <param name="methodList"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private int GetMethodIndex(List<Method> methodList, CalledMethod cm)
        {
            if (cm.ModuleFlg)
            {
                return -1;
            }

            int i = 0;
            foreach (var method in methodList)
            {
                if (cm.Name == method.MethodNameP)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
        #endregion

        #region 共通
        private void SetStyleOfTitle(string sheetName, string range, Color titleColor)
        {
            switch (sheetName)
            {
                case SHEET_NAME_PGMINFO:
                    WsPgmInfo.Cells[range].Style.Font.Bold = true;
                    WsPgmInfo.Cells[range].Style.Fill.PatternType = ExcelFillStyle.DarkVertical;
                    WsPgmInfo.Cells[range].Style.Fill.BackgroundColor.SetColor(titleColor);
                    break;
                case SHEET_NAME_METHODINFO:
                    WsMethodInfo.Cells[range].Style.Font.Bold = true;
                    WsMethodInfo.Cells[range].Style.Fill.PatternType = ExcelFillStyle.DarkVertical;
                    WsMethodInfo.Cells[range].Style.Fill.BackgroundColor.SetColor(titleColor);
                    break;
                case SHEET_NAME_STRUCT:
                    WsStruct.Cells[range].Style.Font.Bold = true;
                    WsStruct.Cells[range].Style.Fill.PatternType = ExcelFillStyle.DarkVertical;
                    WsStruct.Cells[range].Style.Fill.BackgroundColor.SetColor(titleColor);
                    break;
                default:
                    break;
            }
        }

        private string GetArrayWord(string[] value, int index)
        {
            if (index >= value.Length)
                return string.Empty;
            else
                return value[index];
        }

        /// <summary>
        /// 数値をExcelのカラム文字へ変換します
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string ToColumnName(int source)
        {
            if (source < 1) return string.Empty;
            return ToColumnName((source - 1) / 26) + (char)('A' + ((source - 1) % 26));
        }

        /// <summary>
        /// 文字列の指定した位置から指定した長さを取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="start">開始位置</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string Mid(string str, int start, int len)
        {
            if (start <= 0)
            {
                throw new ArgumentException("引数'start'は1以上でなければなりません。");
            }
            if (len < 0)
            {
                return "";
            }
            if (str == null || str.Length < start)
            {
                return "";
            }
            if (str.Length < (start + len))
            {
                return str.Substring(start - 1);
            }
            return str.Substring(start - 1, len);
        }

        /// <summary>
        /// 文字列の指定した位置から末尾までを取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="start">開始位置</param>
        /// <returns>取得した文字列</returns>
        public static string Mid(string str, int start)
        {
            return Mid(str, start, str.Length);
        }

        /// <summary>
        /// 文字列の先頭から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string Left(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(0, len);
        }

        /// <summary>
        /// 文字列の末尾から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string Right(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(str.Length - len, len);
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
        #endregion
    }

    /// <summary>
    /// 関数管理クラス
    /// </summary>
    public partial class Method
    {
        public string MethodNameP { get; internal set; }
        public string MethodNameL { get; internal set; }
        public int StartIndex { get; internal set; }
        public int EndIndex { get; internal set; }
        public List<CalledMethod> CalledMethod { get; internal set; }
        public bool CalledFlg { get; internal set; }
        public string ExecSqlType { get; internal set; }
        public Method(string methodName, int startIndex, int endIndex)
        {
            MethodNameP = methodName;
            StartIndex = startIndex;
            EndIndex = endIndex;
            CalledMethod = new List<CalledMethod>();
            CalledFlg = false;
            ExecSqlType = string.Empty;
        }
    }

    /// <summary>
    /// 呼出関数管理クラス　※下記参照の上、distinct用の実装箇所あり
    /// 参照：https://qiita.com/Chrowa3/items/51e7033aa687c6274ad4
    /// 参照：https://docs.microsoft.com/ja-jp/dotnet/api/system.linq.enumerable.distinct?redirectedfrom=MSDN&view=netcore-3.1#System_Linq_Enumerable_Distinct__1_System_Collections_Generic_IEnumerable___0__
    /// </summary>
    public partial class CalledMethod:IEquatable<CalledMethod>
    {
        public string Name { get; }
        public bool ModuleFlg { get; }
        public int MethodListIndex { get; internal set; }

        public CalledMethod(string name, bool moduleFlg)
        {
            Name = name;
            ModuleFlg = moduleFlg;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        bool IEquatable<CalledMethod>.Equals(CalledMethod cm)
        {
            if (cm == null)
                return false;
            return (this.Name == cm.Name);
        }
    }
}
