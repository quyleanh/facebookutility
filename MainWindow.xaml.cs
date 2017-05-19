using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using IronPython.Hosting;

namespace FanpageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string editedScriptFile = "../facebook/temp/get_fb_posts_fb_page.py";
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Get Post
        private void GetPostBtn_Click(object sender, RoutedEventArgs e)
        {
            string number = NumberPost.Text;
            //UpdatePyFile(number);

            RunGetPostCommand();
        }

        private void RunGetPostCommand()
        {
            editedScriptFile = "../facebook/print.py";

            string pyScript = "\"" + ParentDirectory() + @"\facebook\print.py" + "\"";
            string pyExecute = "\"" + ParentDirectory() + @"\python\python.exe" + "\"";

            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = pyExecute + " " + pyScript;

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WorkingDirectory = ParentDirectory();

            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            Console.WriteLine(strOutput);
            CommandTextBlock.Text = strOutput;
            pProcess.WaitForExit();
        }

        // Get Parent Directory
        private string ParentDirectory()
        {
            // Get parent directory
            DirectoryInfo info;
            string configDirectory = Directory.GetCurrentDirectory();
            try
            {
                info = Directory.GetParent(Directory.GetCurrentDirectory());
                configDirectory = info.FullName;
                return configDirectory;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot get parent directory!", "", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                Console.WriteLine(ex.Message);
                return configDirectory;
            }
        }

        // Update Python file to get number of post
        private void UpdatePyFile(string number)
        {
            string filePath = "../facebook/py3.5_get_fb_posts_fb_page.py";
            string text = File.ReadAllText(filePath);

            if (text != "" && text.Contains("number_gotten_page = "))
            {
                int index = text.IndexOf("number_gotten_page = ");
                if (index >= 0)
                {
                    int startIndexOfValue = index + "number_gotten_page = ".Length;
                    string textAfter = text.Substring(startIndexOfValue, text.Length - startIndexOfValue);
                    int indexN = textAfter.IndexOf(@"access_token = app_id");
                    textAfter = textAfter.Substring(indexN, textAfter.Length - indexN);

                    text = text.Substring(0, startIndexOfValue);
                    text += number;
                    text += "\n\n";
                    text += textAfter;
                }
            }
            File.WriteAllText(editedScriptFile, text);
        }
        #endregion

        #region Get Comment
        private void GetCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            string number = NumberPost.Text;

            string getPostPyScript = @"../facebook/py3.5_get_fb_comments_from_fb.py";
            //string getPostPyScript = @"test.py";

            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                //paths.Add("../Lib");
                //engine.SetSearchPaths(paths);
                engine.ImportModule("../Lib/urllib.request");
                engine.ImportModule("../Lib/json");
                engine.ImportModule("../Lib/datetime");
                engine.ImportModule("../Lib/csv");
                engine.ImportModule("../Lib/time");
                engine.ExecuteFile(getPostPyScript);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Textbox Control
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void NumberPost_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        #endregion

        private void FilterCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            int rw = 0;
            int cl = 0;

            xlApp = new Excel.Application();

            #region Get File to filter
            string filePath = "comments.xlsx";
            DirectoryInfo info;
            string configDirectory = null;

            try
            {
                info = Directory.GetParent(Directory.GetCurrentDirectory());
                configDirectory = info.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            try
            {
                File.ReadAllBytes(configDirectory + "\\" + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            #endregion

            xlWorkBook = xlApp.Workbooks.Open(configDirectory + "\\" + filePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            cl = range.Columns.Count;

            string filteredNumber = FilteredNumberTextbox.Text;
            string defaultFilterRule = @"\d{NUM}(?=\D|$)(?<=(\D|^)\d{NUM})";
            string filterRule = defaultFilterRule.Replace("NUM", filteredNumber);
            filterRule = filterRule.Replace(@"\\", @"\");

            int columnToGet = 1;
            int ColumnToWrite = cl + 1;
            for (int rCnt = 1; rCnt <= rw; rCnt++)
            {
                if (range.Cells[rCnt, columnToGet] != null && range.Cells[rCnt, columnToGet].Value2 != null
                    && range.Cells[rCnt, 4].Value2 != null)
                {
                    // Get text from a specific column
                    string text = range.Cells[rCnt, columnToGet].Value2.ToString();
                    // Filter number in each row of column
                    string resultString = Regex.Match(text, filterRule.Replace(@"\\", @"\")).Value;

                    Console.WriteLine("===={0}======{1} \n", text, resultString);

                    range.Cells[rCnt, ColumnToWrite].Value2 = resultString;
                }

                range.Cells[0, ColumnToWrite].Value2 = "filtered_number";
            }
            xlWorkBook.SaveAs(configDirectory + "\\GiveAwayResult.xlsx");
            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

            Process.Start(configDirectory + "\\GiveAwayResult.xlsx");
        }


    }
}
