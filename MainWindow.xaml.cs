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
using System.Threading.Tasks;
using FanpageTool.ViewModel;
namespace FanpageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum FILE_TYPE
        {
            FILE_POST = 0,
            FILE_COMMENT,
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
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

        #region Get Post Button Click Handle
        private void GetPostBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.IsInit = true;
            vm.CommandText = new StringBuilder("");
            string number = NumberOfPost.Text;
            UpdatePythonFile(number, FILE_TYPE.FILE_POST);
            RunPythonCommand(FILE_TYPE.FILE_POST);
        }
        #endregion

        #region Run Python File
        private void RunPythonCommand(FILE_TYPE fileType)
        {
            Task task = new Task(() =>
            {
                string pyScript = "\"" + ParentDirectory() + @"\facebook\print.py" + "\"";
                switch (fileType)
                {
                    case FILE_TYPE.FILE_POST:
                        pyScript = "\"" + ParentDirectory() + @"\facebook\py3.5_get_fb_posts_fb_page.py" + "\"";
                        break;
                    case FILE_TYPE.FILE_COMMENT:
                        break;
                    default:
                        pyScript = "\"" + ParentDirectory() + @"\facebook\py3.5_get_fb_comments_from_fb.py" + "\"";
                        break;
                }
                string pyExecute = "\"" + ParentDirectory() + @"\python\python.exe" + "\"";

                Process process = new Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "cmd";
                startInfo.Arguments = "/c " + "ping training.tsdv.com.vn -n 10";
                process.StartInfo = startInfo;

                //process.StartInfo.FileName = pyExecute + " " + pyScript;

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += DataReceivedEventHandler;
                process.ErrorDataReceived += DataReceivedEventHandler;
                process.EnableRaisingEvents = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();
            });
            task.Start();
        }

        public void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
                if (e.Data != null)
                {
                    vm.CommandText.Append(e.Data.ToString() + "\n");
                    Console.WriteLine(e.Data.ToString());
                    vm.OnPropertyChanged("CommandText");
                    scrollViewer.ScrollToBottom();
                }
            }));
        }
        #endregion

        private void UpdatePythonFile(string inputData, FILE_TYPE fileType)
        {
            int line_to_edit = 106;
            string destinationFile = "";
            string lineToWrite = "";

            switch (fileType)
            {
                case FILE_TYPE.FILE_POST:
                    line_to_edit = 11;
                    destinationFile = "../facebook/py3.5_get_fb_posts_fb_page.py";
                    if (inputData != null && inputData != "0")
                    {
                        lineToWrite = "number_gotten_page = " + inputData;
                    }
                    else
                    {
                        lineToWrite = "number_gotten_page = 0";
                    }
                    break;
                case FILE_TYPE.FILE_COMMENT:
                    line_to_edit = 106;
                    destinationFile = "../facebook/py3.5_get_fb_comments_from_fb.py";
                    if (inputData != null)
                    {
                        lineToWrite = "reader = [dict(status_id='" + inputData + "')]";
                    }
                    else
                    {
                        lineToWrite = "reader = csv.DictReader(csvfile)";
                    }
                    break;
                default:
                    break;
            }

            // Read the old file.
            string[] lines = File.ReadAllLines(destinationFile);

            // Write the new file over the old file.
            using (StreamWriter writer = new StreamWriter(destinationFile))
            {
                for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                {
                    if (currentLine == line_to_edit)
                    {
                        writer.WriteLine(lineToWrite);
                    }
                    else
                    {
                        writer.WriteLine(lines[currentLine - 1]);
                    }
                }
            }


        }

        private bool IsFileReadable(string fileName)
        {
            string file = fileName + "_facebook_comments.csv";
            string configDirectory = ParentDirectory();
            try
            {
                File.ReadAllBytes(configDirectory + "\\" + file);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        #region Get Comment Button Click Handle
        // Get Comment Button click
        private void GetCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            #region OldSampleCode
            //string getPostPyScript = @"../facebook/py3.5_get_fb_comments_from_fb.py";
            //try
            //{
            //    ScriptEngine engine = Python.CreateEngine();
            //    var paths = engine.GetSearchPaths();
            //    engine.ImportModule("../Lib/urllib.request");
            //    engine.ImportModule("../Lib/json");
            //    engine.ImportModule("../Lib/datetime");
            //    engine.ImportModule("../Lib/csv");
            //    engine.ImportModule("../Lib/time");
            //    engine.ExecuteFile(getPostPyScript);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            #endregion

            string postID = PostIDTextBox.Text;
            UpdatePythonFile(postID, FILE_TYPE.FILE_COMMENT);
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.IsInit = true;
            vm.CommandText = new StringBuilder("");

            string fileID = PageIdComment.Text;
            if (IsFileReadable(fileID))
            {
                RunPythonCommand(FILE_TYPE.FILE_COMMENT);
            }
            else 
            {
                vm.CommandText = new StringBuilder("There is no existent csv file! \n Get Post first!");
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

        #region Filter Comment Button Click Handle
        private void FilterCommentBtn_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            int rw = 0;
            int cl = 0;

            xlApp = new Excel.Application();

            // Check file is readable
            string fileID = PageIdCommentFilter.Text;
            if (!IsFileReadable(fileID))
            {
                return;
            }

            string filePath = fileID + "_facebook_comments.csv";
            xlWorkBook = xlApp.Workbooks.Open(ParentDirectory() + "\\" + filePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            cl = range.Columns.Count;

            // Set filter rule from filter number
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
            xlWorkBook.SaveAs(ParentDirectory() + "\\" + fileID + "_Results.xlsx");
            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

            Process.Start(ParentDirectory() + "\\" + fileID + "_Results.xlsx");
        }
        #endregion

    }
}
