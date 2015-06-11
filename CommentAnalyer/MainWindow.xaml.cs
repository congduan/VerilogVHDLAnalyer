using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using CommentAnalyer.Commands;
using CommentAnalyer.View;
using CommentAnalyer.Model;
using Microsoft.Win32;
using Visifire.Charts;
using CommentAnalyer.StructsEnums;
using System.Linq;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace CommentAnalyer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region  private字段
        private object dummyNode = null;
        private string _filename, _totalinfo;
        private string[] _drivers;//驱动器列表
        private string _currentDriver = "C:\\";//默认为C盘
        private string _currentFileFilter = "*.*";
        private string _currentFolder = null;
        private ObservableCollection<FileListModel> _fileList;
        private ObservableCollection<CodeRowInfo> _fileInfoList;
        //和分析结果有关的变量
        private CodeRowInfo _codeRowInfo = new CodeRowInfo();

        #region 是否开始复杂注释
        private bool _isStartComplexComment = false;
        private float _progress;
        #endregion

        private int _fileListCount;
        private int _allFolderCount;

        #region 设置
        private bool _containsSubFolders = false;
        private double _thresholdValue;
        #endregion

        #endregion

        #region public字段
        //默认文本编辑器
        public string TextEditor = "C:\\Windows\\notepad.exe";
        #endregion

        #region 属性

        public string SelectedImagePath { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                OnPropertyChanged("FileName");
            }
        }
        
        /// <summary>
        /// 当前代码文件
        /// </summary>
        //private string _CodeText;
        public string CodeText
        {
        	get
        	{
        		//将每一行代码转换成为整个文本
        		StringBuilder sb = new StringBuilder();
        		foreach (string element in CodeTextLines) {
        			sb.Append(element).Append("\n");
        		}
        		return sb.ToString();
        	}
        }
       
        
        private List<string> _CodeTextLines;
        public List<string> CodeTextLines
        {
        	set
        	{
        		_CodeTextLines = value;
        		OnPropertyChanged("CodeTextLines");
        	}
        	get
        	{
        		if(_CodeTextLines == null)
        			_CodeTextLines = new List<string>();
        		return _CodeTextLines;
        	}
        }

        /// <summary>
        /// 所有信息
        /// </summary>
        public String TotalInfo
        {
            get
            {
                return _totalinfo;
            }
            set
            {
                _totalinfo = value;
                OnPropertyChanged("TotalInfo");
            }
        }

        /// <summary>
        /// 驱动器列表
        /// </summary>
        public string[] Drivers
        {
            set
            {
                _drivers = value;
                OnPropertyChanged("Drivers");
            }
            get
            {
                return Directory.GetLogicalDrives();
            }
        }

        /// <summary>
        /// 当前驱动器
        /// </summary>        
        public string CurrentDriver
        {
            set
            {
                _currentDriver = value;
                ChangeDriver();
                OnPropertyChanged("CurrentDriver");
            }
            get
            {
                return _currentDriver;
            }
        }

        /// <summary>
        /// 当前目录
        /// </summary>
        public string CurrentFolder
        {
            set
            {
                _currentFolder = value;
                ChangeFolder();
                OnPropertyChanged("CurrentFolder");
            }
            get
            {
                return _currentFolder;
            }
        }

        private List<string> _fileFilters;
        /// <summary>
        /// 搜索文件过滤规则
        /// </summary>
        public List<string> FileFilters
        {
            set
            {
                _fileFilters = value;
            }
            get
            {
                if (_fileFilters == null)
                {
                    _fileFilters = new List<string>();
                    _fileFilters.Add("*.v");
                    //list.Add("*.c");
                    //list.Add("*.cpp");
                    _fileFilters.Add("*.vhd");
                }
                return _fileFilters;
            }
        }

        /// <summary>
        /// 当前过滤规则
        /// </summary>
        public string CurrentFileFilter
        {
            set
            {
                _currentFileFilter = value;
                ChangeFileFilter();
                OnPropertyChanged("CurrentFileFilter");
            }
            get
            {
                return _currentFileFilter;
            }
        }

        /// <summary>
        /// 文件列表
        /// </summary>
        public ObservableCollection<FileListModel> FileList
        {
            set
            {
                _fileList = value;
                OnPropertyChanged("FileList");
            }
            get
            {
                if (_fileList == null)
                    _fileList = new ObservableCollection<FileListModel>();
                return _fileList;
            }
        }

        /// <summary>
        /// 目录文件信息列表
        /// </summary>
        public ObservableCollection<CodeRowInfo> FileInfoList
        {
            set
            {
                _fileInfoList = value;
                OnPropertyChanged("FileList");
            }
            get
            {
                if (_fileInfoList == null)
                    _fileInfoList = new ObservableCollection<CodeRowInfo>();
                return _fileInfoList;
            }
        }

        /// <summary>
        /// 文件数目
        /// </summary>
        public int FileListCount
        {
            set
            {
                _fileListCount = value;
                OnPropertyChanged("FileListCount");
            }
            get
            {
                return _fileListCount;
            }
        }

        /// <summary>
        /// 是否包含子目录
        /// </summary>
        public bool ContainsSubFolders
        {
            set
            {
                _containsSubFolders = value;
                //ChangeFolder();
                OnPropertyChanged("ContainsSubFolders");
            }
            get
            {
                return _containsSubFolders;
            }
        }

        /// <summary>
        /// 注释率阈值
        /// </summary>
        public double ThresholdValue
        {
            set
            {
                _thresholdValue = value;
                OnThresholdValueChanged();
                OnPropertyChanged("ThresholdValue");
            }
            get
            {
                return _thresholdValue;
            }
        }

        /// <summary>
        /// 分析目录所有文件的进度
        /// </summary>
        public float Progress
        {
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
            get
            {
                return _progress;
            }
        }

        //所有文件个数
        public int AllFolderCount
        {
            set
            {
                _allFolderCount = value;
                OnPropertyChanged("AllFolderCount");
            }
            get
            {
                return _allFolderCount;
            }
        }

        #endregion
        
        #region Commands
        /// <summary>
		/// 提取输入输出端口
		/// </summary>
		private ICommand _ListInputsOutputs;
		public ICommand ListInputsOutputs
        {
            get
            {
                if (_ListInputsOutputs == null)
                    _ListInputsOutputs = new RelayCommand(param =>
                    {
                	   //提取输入输出端口
                	   if(CodeTextLines.Count>0)
                       {
                            #region 提取所有变量
                            List<VerilogVariable> variables = new List<VerilogVariable>();
                            //正则表达式提取变量
                            Regex regexStr = new Regex(@"(?<type>reg|wire)[ \t]*(\[(?<width1>\d+)\:(?<width2>\d+)\])?\s+(?<name>\w+)", RegexOptions.IgnoreCase);
                            MatchCollection result = regexStr.Matches(CodeText);
                            int width1 = 1, width2 = 1;
                            foreach (Match element in result)
                            {
                                //MessageBox.Show(element.Groups["name"].ToString());
                                width1 = string.IsNullOrEmpty(element.Groups["width1"].Value) ? 1 : int.Parse(element.Groups["width1"].Value);
                                width2 = string.IsNullOrEmpty(element.Groups["width2"].Value) ? 1 : int.Parse(element.Groups["width2"].Value);

                                variables.Add(new VerilogVariable(element.Groups["type"].Value.ToLower().Equals("reg") ? VariableType.Reg : VariableType.Wire
                                                                , element.Groups["name"].Value
                                                                , Math.Abs(width1 - width2) + 1));
                            }
                            #endregion

                            List<VerilogVariable> ports = new List<VerilogVariable>();
	                	    //正则表达式提取端口
                            regexStr = new Regex(@"(?<type>input|output)[ \t]*(\[(?<width1>\d+)\:(?<width2>\d+)\])?\s+(?<name>\w+)", RegexOptions.IgnoreCase);
	                	    result = regexStr.Matches(CodeText);
                            width1 = 1; width2 = 1;
	                	    foreach (Match element in result) {
	                	   	    //MessageBox.Show(element.Groups["name"].ToString());
                                width1 = string.IsNullOrEmpty(element.Groups["width1"].Value) ? 1 : int.Parse(element.Groups["width1"].Value);
                                width2 = string.IsNullOrEmpty(element.Groups["width2"].Value) ? 1 : int.Parse(element.Groups["width2"].Value);

                                ports.Add(new InputOutputPort(element.Groups["type"].Value.ToLower().Equals("input") ? PortType.Input : PortType.Output
                                                                , element.Groups["name"].Value
                                                                , Math.Abs(width1 - width2) + 1));
	                	    }

                            //求交集（查找端口的reg/wire类型
                            VerilogVariable vv;
                            foreach (var item in ports)
                            {
                                try
                                {
                                    vv = variables.First(t => t.Name == item.Name);
                                    if (vv != null)
                                        item.VariableType = vv.VariableType;
                                }
                                catch (InvalidOperationException) { }
                            }

                	   	    //显示列表
                	   	    InputOutputWindow window = new InputOutputWindow(ports,InputOutputWindow.FilterTypes.InputOutput);
                       	    window.ShowDialog();
                	   }
                    },
                    param =>
                    {
                        return true;
                    });
                return _ListInputsOutputs;
            }
        }
		
		/// <summary>
		/// 提取reg/wire
		/// </summary>
		private ICommand _ListRegWire;
		public ICommand ListRegWire
        {
            get
            {
                if (_ListRegWire == null)
                    _ListRegWire = new RelayCommand(param =>
                    {
                	   //提取reg/wire
                	   if(CodeTextLines.Count>0)
                	   {
                            List<VerilogVariable> ports = new List<VerilogVariable>();
	                	   	//正则表达式提取变量
                            Regex regexStr = new Regex(@"(?<type>reg|wire)[ \t]*(\[(?<width1>\d+)\:(?<width2>\d+)\])?\s+(?<name>\w+)", RegexOptions.IgnoreCase);
                            MatchCollection result = regexStr.Matches(CodeText);
                            int width1 = 1, width2 = 1;
                            foreach (Match element in result)
                            {
                                //MessageBox.Show(element.Groups["name"].ToString());
                                width1 = string.IsNullOrEmpty(element.Groups["width1"].Value) ? 1 : int.Parse(element.Groups["width1"].ToString());
                                width2 = string.IsNullOrEmpty(element.Groups["width2"].ToString()) ? 1 : int.Parse(element.Groups["width2"].ToString());

                                ports.Add(new VerilogVariable(element.Groups["type"].ToString().ToLower().Equals("reg") ? VariableType.Reg : VariableType.Wire
                                                              , element.Groups["name"].ToString()
                                                              , Math.Abs(width1 - width2) + 1));
                            }
                            //显示列表
                            InputOutputWindow window = new InputOutputWindow(ports, InputOutputWindow.FilterTypes.NotInputOutput);
                            window.ShowDialog();
                	   }
                    },
                    param =>
                    {
                        return true;
                    });
                return _ListRegWire;
            }
        }
		
		/// <summary>
		/// 格式化代码
		/// </summary>
		private ICommand _FormatCodeCommand;
		public ICommand FormatCodeCommand
        {
            get
            {
                if (_FormatCodeCommand == null)
                    _FormatCodeCommand = new RelayCommand(param =>
                    {
                	   switch (param.ToString()) {
                	   	case "beginend":
                	   		//格式化begin...end
                	   		string tempcodetext = CodeText;
                	   		tempcodetext = Regex.Replace(tempcodetext,@"^begin(.*)end$","");
                	   		
                	   		break;
                	   	default:
                	   		
                	   		break;
                	   }
                    },
                    param =>
                    {
                        return true;
                    });
                return _FormatCodeCommand;
            }
        }
        #endregion

        #region 加载与退出
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            loadCustomHighLighting();
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
            Environment.Exit(0);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            ChangeDriver();
        }
        #endregion
        
        /// <summary>
        /// 更改过滤器
        /// </summary>
        private void ChangeFileFilter()
        {
            //var filelistbackup = FileList;
        }

        #region 改变驱动器和文件夹
        /// <summary>
        /// 改变当前驱动器
        /// </summary>
        void ChangeDriver()
        {
            foldersItem.Items.Clear();
            try
            {
                foreach (string s in Directory.GetDirectories(CurrentDriver))
                {
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                    subitem.Tag = s;
                    subitem.FontWeight = FontWeights.Normal;
                    subitem.Items.Add(dummyNode);
                    subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                    foldersItem.Items.Add(subitem);
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 目录展开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }
        /// <summary>
        /// 选择目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }
            //show user selected path
            //MessageBox.Show(SelectedImagePath);
            CurrentFolder = CurrentDriver + SelectedImagePath;
        }
        /// <summary>
        /// 改变当前目录
        /// </summary>
        private void ChangeFolder()
        {
            //在下面区域显示所有文件
            DisplayFileList(CurrentFolder);
            //分析目录所有文件
            //analyzeFolder(CurrentFolder);
        }

        /// <summary>
        /// 开始分析目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startAnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            //在下面区域显示所有文件
            //DisplayFileList(CurrentFolder);
            //分析目录所有文件
            analyzeFolder(CurrentFolder);
        }

        #endregion

        /// <summary>
        /// 显示文件列表
        /// </summary>
        private void DisplayFileList(string path)
        {
            //显示搜索提示            
            TipWindow tipWindow = new TipWindow();
            if (this.IsLoaded == true)
            {
                tipWindow.Owner = this;
                tipWindow.Show();
            }
            //this.IsEnabled = false;
            try
            {
                FileList.Clear();

                if (ContainsSubFolders)
                {
                    if (CurrentFolder != null)
                    {
                        FileHelper fh = new FileHelper();
                        fh.ListFiles(new DirectoryInfo(CurrentFolder), FileFilters);
                        FileList = fh.filelist;
                    }
                }
                else
                {
                    if (path != null)
                    {
                        foreach (var v in FileFilters)
                        {
                            string[] files = Directory.GetFiles(path, v);
                            if (files.Length > 0)
                            {
                                foreach (string filename in files)
                                {
                                    FileInfo inf = new FileInfo(filename);
                                    FileList.Add(new FileListModel(inf.Name, inf.FullName));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e1.Message);
            }
            //this.IsEnabled = true;
            if (this.IsLoaded == true)
            {
                tipWindow.Close();
            }
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = @"所有文件(*.v;*.c;*.cpp)|*.v;*.c;*.cpp|verilog文件(*.v)|*.v|c文件(*.c)|*.c|c++文件(*.cpp)|*.cpp";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = CurrentFolder;
            bool? result = openFileDialog.ShowDialog();
            if (result == false)
            {
                return;
            }
            
            //文件名
            FileName = openFileDialog.FileName;
            //文件信息
            _codeRowInfo = analyzeComment(FileName);
            //显示结果
            TotalInfo = _codeRowInfo.Display();
            
            #region 显示文件中的代码
            if (FileName != null)
            {
            	CodeTextLines.Clear();
            	//CodeText = File.ReadAllText(FileName);
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fs,Encoding.Default,true))
                    {
                        //使用StreamReader类来读取文件
                        streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                        // 从数据流中读取每一行，直到文件的最后一行
                        string strLine = streamReader.ReadLine();
                        while (strLine != null)
                        {
                        	//处理该行
                        	CodeTextLines.Add(strLine);
                        	//读取下一行
                         	strLine = streamReader.ReadLine();
                        }
                    }
                }
                OnPropertyChanged("CodeText");
                //打开到编辑器
                //fctb.Text = CodeText;
                loadFile(FileName);
            }
            #endregion
            
            //显示饼图            
            ShowPieChart();
        }

        private void loadFile(string filename)
        {
            textEditor.Load(filename);
            foldingStrategy = new BraceFoldingStrategy();
            if (foldingManager == null)
                foldingManager = FoldingManager.Install(textEditor.TextArea);
            UpdateFoldings();
            //textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(filename));
        }

        /// <summary>
        /// 显示饼图
        /// </summary>
        /// <param name="dataSeries"></param>
        private void ShowPieChart()
        {
            DataSeries dataSeries = new DataSeries();
            DataPoint dataPoint = new DataPoint() { AxisXLabel = "有效行数", YValue = (double)_codeRowInfo.ValidCodeRowNum };
            dataSeries.DataPoints.Add(dataPoint);
            dataPoint = new DataPoint() { AxisXLabel = "空行数", YValue = (double)_codeRowInfo.BlankLineRowNum };
            dataSeries.DataPoints.Add(dataPoint);
            if ((new FileInfo(FileName)).Extension == ".v")
            {
                dataPoint = new DataPoint() { AxisXLabel = "//注释行数", YValue = (double)_codeRowInfo.SimpleCommentRowNum };
                dataSeries.DataPoints.Add(dataPoint);
                dataPoint = new DataPoint() { AxisXLabel = "/*...*/注释行数", YValue = (double)_codeRowInfo.ComplexCommentRowNum };
                dataSeries.DataPoints.Add(dataPoint);
            }
            else if ((new FileInfo(FileName)).Extension == ".vhd")
            {
                dataPoint = new DataPoint() { AxisXLabel = "--注释行数", YValue = (double)_codeRowInfo.VhdlCommentRowNum };
                dataSeries.DataPoints.Add(dataPoint);
            }

            // Create a new instance of Chart
            Chart chart = new Chart();
            chart.View3D = true;
            chart.Width = 500;
            chart.Height = 300;
            chart.AnimationEnabled = false;
            chart.ZoomingEnabled = true;
            chart.IndicatorEnabled = true;
            chart.ToolBarEnabled = true;

            // Create a new instance of Title
            Title title = new Title();
            // Set title property
            title.Text = FileName + "  注释分析";
            // Add title to Titles collection
            chart.Titles.Add(title);
           
            // Set DataSeries property
            dataSeries.RenderAs = RenderAs.Pie;
            dataSeries.AxisXType = AxisTypes.Primary;            
            dataSeries.MarkerEnabled = false;                        
            // Add dataSeries to Series collection.
            chart.Series.Add(dataSeries);

            // Add chart to LayoutRoot
            chartContainer.Children.Add(chart);
        }

        #region 分析代码规范
        int startRow = 0, endRow = 0;
        /// <summary>
        /// 分析注释信息：注释行数，总行数
        /// </summary>
        private CodeRowInfo analyzeComment(string filename)
        {
            try
            {
                CodeRowInfo crinfo = new CodeRowInfo((new FileInfo(filename)).Name, filename);
                crinfo.Reset();
                //读取文件内容
                if (filename != null)
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader streamReader = new StreamReader(fs))
                        {
                            //使用StreamReader类来读取文件
                            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                            // 从数据流中读取每一行，直到文件的最后一行
                            string strLine = streamReader.ReadLine();
                            while (strLine != null)
                            {
                                crinfo.TotalRowNum++;
                                //Verilog文件
                                if ((new FileInfo(filename)).Extension == ".v")
                                {
                                    //计算/**/注释行数
                                    if (!_isStartComplexComment)
                                    {
                                        //统计空行
                                        if (string.IsNullOrEmpty(strLine) || string.IsNullOrWhiteSpace(strLine))
                                        {
                                            crinfo.BlankLineRowNum++;
                                        }
                                        if (strLine.Contains("//") && !strLine.Trim().StartsWith("//"))
                                        {
                                            crinfo.CodeWithSimpleCommentRowNum++;
                                        }
                                        if (strLine.Contains("//"))
                                        {
                                            if (strLine.Contains("/*"))
                                            {
                                                if (strLine.IndexOf("//", StringComparison.Ordinal) < strLine.IndexOf("/*", StringComparison.Ordinal))
                                                {
                                                    crinfo.SimpleCommentRowNum++;
                                                }
                                                else
                                                {
                                                    _isStartComplexComment = true;
                                                    startRow = crinfo.TotalRowNum; //起始行数
                                                }
                                            }
                                            else
                                            {
                                                crinfo.SimpleCommentRowNum++;
                                            }
                                        }
                                        else
                                        {
                                            if (strLine.Contains("/*") && !strLine.Contains("*/"))
                                            {
                                                _isStartComplexComment = true;
                                                startRow = crinfo.TotalRowNum; //起始行数
                                            }
                                            if (strLine.Contains("/*") && strLine.Contains("*/"))
                                            {
                                                crinfo.ComplexCommentRowNum++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (strLine.Contains("*/"))
                                        {
                                            _isStartComplexComment = false;
                                            endRow = crinfo.TotalRowNum; //结束行数
                                            crinfo.ComplexCommentRowNum += endRow - startRow + 1;
                                        }
                                    }
                                }
                                //VHDL文件
                                else if ((new FileInfo(filename)).Extension == ".vhd")
                                {
                                    //统计空行
                                    if (string.IsNullOrEmpty(strLine) || string.IsNullOrWhiteSpace(strLine))
                                    {
                                        crinfo.BlankLineRowNum++;
                                    }
                                    if (strLine.Contains("--"))
                                    {
                                        crinfo.VhdlCommentRowNum++;
                                        if (!strLine.Trim().StartsWith("--"))
                                        {
                                            crinfo.CodeWithSimpleCommentRowNum++;
                                        }
                                    }
                                }
                                //读取下一行
                                strLine = streamReader.ReadLine();
                            }
                        }
                    }
                }
                startRow = 0;
                endRow = 0;
                return crinfo;
            }
            catch (Exception)
            {
                startRow = 0;
                endRow = 0;
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 刷新文件列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _codeRowInfo = analyzeComment(FileName);
                //显示结果
                TotalInfo = _codeRowInfo.Display();
            }
            catch (Exception)
            {
                return;
            }
        }

        #region 分析所有文件代码规范
        BackgroundWorker bgw;
        /// <summary>
        /// 分析目录下所有文件
        /// </summary>
        /// <param name="foldername"></param>
        private void analyzeFolder(string foldername)
        {
            FileInfoList.Clear();

            bgw = new BackgroundWorker();
            bgw.WorkerSupportsCancellation = true;
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);

            bgw.RunWorkerAsync();
        }
        /// <summary>
        /// 后台分析整个目录所有文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < FileList.Count; i++)
            {
                CodeRowInfo cri = analyzeComment(FileList[i].FullFileName);
                bgw.ReportProgress((int)((i + 1) / (float)FileList.Count * 100), cri);
            }
        }
        /// <summary>
        /// 进度改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FileInfoList.Add(e.UserState as CodeRowInfo);
            Progress = e.ProgressPercentage;
        }
        /// <summary>
        /// 执行结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show(Progress.ToString());
        }
        #endregion

        /// <summary>
        /// 导出到Txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveTxtBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "(文本文件*.txt)|*.txt|(所有文件*.*)|*.*";
            dialog.DefaultExt = "txt";
            dialog.AddExtension = true;
            bool? result = dialog.ShowDialog();
            if (result == false)
                return;
            //写入txt文件
            string txt_str = File.ReadAllText("SaveTxtTemplate.tpl");
            txt_str = txt_str.Replace("#FileName#", FileName);
            txt_str = txt_str.Replace("#CodeInfo#", TotalInfo);            
            File.WriteAllText(dialog.FileName,txt_str);
        }

        #region 右键菜单
        /// <summary>
        /// 双击文件名打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((sender as TextBlock).Tag != null)
                {
                    try
                    {
                        loadFile((sender as TextBlock).Tag.ToString());
                        tabControl.SelectedIndex = 0;
                    }
                    catch (Exception)
                    {
                        //fctb.Text = File.ReadAllText((sender as TextBlock).Tag.ToString(),Encoding.Default);
                        
                        //Process.Start("C:\\Windows\\Notepad.exe", (sender as TextBlock).Tag.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 右键打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var control = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            if ((control as TextBlock).Text != null)
            {
                //Process.Start((control as TextBlock).Tag.ToString());
                try
                {
                    Process.Start(TextEditor, (control as TextBlock).Tag.ToString());
                }
                catch (Exception)
                {
                    //fctb.Text = File.ReadAllText((control as TextBlock).Tag.ToString(),Encoding.Default);
                    loadFile((sender as TextBlock).Tag.ToString());
                    tabControl.SelectedIndex = 0;
                    //Process.Start("C:\\Windows\\Notepad.exe", (control as TextBlock).Tag.ToString());
                }
            }
        }

        /// <summary>
        /// 右键打开文件所在文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderFileListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFolder != null)
            {
                Process.Start("explorer.exe", this.CurrentFolder);
            }
        }

        /// <summary>
        /// 定位文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocateFileListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var control = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            if ((control as TextBlock).Text != null)
            {
                ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
                psi.Arguments = "/e,/select," + (control as TextBlock).Tag.ToString();
                Process.Start(psi);
            }
        }

        /// <summary>
        /// 右键复制完整路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyPathFileListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var control = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            Clipboard.SetText((control as TextBlock).Tag.ToString());
            MessageBox.Show("路径已经复制到剪切板");
        }

        /// <summary>
        /// 从列表删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteFileListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(fileDataGrid.SelectedItem.ToString());            
            var items = fileDataGrid.SelectedItems;
            List<CodeRowInfo> itemsCopy = new List<CodeRowInfo>();
            foreach (var item in items)
            {
                itemsCopy.Add(item as CodeRowInfo);
            }
            foreach (var row in itemsCopy)
            {
                FileInfoList.Remove(row as CodeRowInfo);
                FileListCount--;
            }
            //var control = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            //FileInfoList.Remove(FileInfoList.First(v => v.FileName == (control as TextBlock).Text.ToString()));
        }
        #endregion

        #region 设置阈值
        /// <summary>
        /// 注释率阈值改变
        /// </summary>
        private void OnThresholdValueChanged()
        {
            foreach (var v in FileInfoList)
            {
                v.ThresholdValue = ThresholdValue;
            }
        }

        /// <summary>
        /// 设置菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow swindow = new SettingsWindow() { Owner = this };
            swindow.ShowDialog();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            //throw new NotImplementedException();
            ThresholdValue = Double.Parse(Utils.ReadFromAppConfig("ThresholdValue"));
            TextEditor = Utils.ReadFromAppConfig("TextEditor");
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void SaveSettings()
        {
            //throw new NotImplementedException();
            Utils.WriteToAppConfig("ThresholdValue", ThresholdValue.ToString());
            Utils.WriteToAppConfig("TextEditor", TextEditor);
        }

        #endregion

        #region 选择文件类型
        /// <summary>
        /// 处理文件过滤，选择Verilog还是VHDL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as ComboBox).SelectedIndex)
            {
                //Verilog和VHDL
                case 0:
                    if (!FileFilters.Contains("*.v"))
                        FileFilters.Add("*.v");
                    if (!FileFilters.Contains("*.vhd"))
                        FileFilters.Add("*.vhd");
                    ChangeFolder();
                    //foreach (var v in FileList)
                    //    v.IsVisiable = true;
                    break;
                //仅Verilog文件
                case 1:
                    //var filelist = from t in FileList where t.FileName.Substring(t.FileName.Length - 2, 2) == ".v" select t;
                    //foreach (var v in filelist)                    
                    //    v.IsVisiable = true;
                    //var filelist1 = from t in FileList where t.FileName.Substring(t.FileName.Length - 4, 4) == ".vhd" select t;
                    //foreach (var v in filelist1)                    
                    //    v.IsVisiable = false;
                    if (FileFilters.Contains("*.vhd"))
                        FileFilters.Remove("*.vhd");
                    if (!FileFilters.Contains("*.v"))
                        FileFilters.Add("*.v");
                    ChangeFolder();
                    break;
                //仅VHDL文件
                case 2:
                    //var filelist2 = from t in FileList where t.FileName.Substring(t.FileName.Length - 4, 4) == ".vhd" select t;
                    //foreach (var v in filelist2)
                    //    v.IsVisiable = true;
                    //var filelist3 = from t in FileList where t.FileName.Substring(t.FileName.Length - 2, 2) == ".v" select t;
                    //foreach (var v in filelist3)                    
                    //    v.IsVisiable = false;
                    if (FileFilters.Contains("*.v"))
                        FileFilters.Remove("*.v");
                    if (!FileFilters.Contains("*.vhd"))
                        FileFilters.Add("*.vhd");
                    ChangeFolder();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

        /// <summary>
        /// 输入目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Directory.Exists((sender as TextBox).Text))
                {
                    CurrentFolder = (sender as TextBox).Text;
                }
                else
                {
                    MessageBox.Show("目录不存在!");
                }
            }
        }

        #region 工具栏

        #region Folding
        FoldingManager foldingManager;
        object foldingStrategy;

        void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textEditor.SyntaxHighlighting == null)
            {
                foldingStrategy = null;
            }
            else
            {
                switch (textEditor.SyntaxHighlighting.Name)
                {
                    case "XML":
                        foldingStrategy = new XmlFoldingStrategy();
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        break;
                    case "C#":
                    case "C++":
                    case "PHP":
                    case "Verilog":
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(textEditor.Options);
                        foldingStrategy = new BraceFoldingStrategy();
                        break;
                    default:
                        textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
                        foldingStrategy = null;
                        break;
                }
            }
            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(textEditor.TextArea);
                UpdateFoldings();
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }
        }

        void UpdateFoldings()
        {
            if (foldingStrategy is BraceFoldingStrategy)
            {
                ((BraceFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
            }
            if (foldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy)foldingStrategy).UpdateFoldings(foldingManager, textEditor.Document);
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// 是否存在多个module
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectMultiModuleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Regex regexStr = new Regex(@"\bmodule\s+(?<name>\w+)\s+\(", RegexOptions.IgnoreCase);
            MatchCollection result = regexStr.Matches(CodeText);
            StringBuilder builder = new StringBuilder();
            foreach (Match res in result)
            {
                builder.Append(res.Groups["name"].Value+"\n");
            }
            if (result.Count > 1)
            {
                MessageBox.Show("存在"+result.Count+"个module：\n" + builder.ToString());
            }
            else if (result.Count == 1)
            {
                MessageBox.Show("只有一个module："+result[0].Groups["name"].Value);
            }
            else
            {
                MessageBox.Show("没有module定义");
            }
        }

        /// <summary>
        /// 文件名与module是否一致
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectFileModuleNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                var ret = MessageBox.Show("是否忽略大小写？","提示",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
                RegexOptions option = RegexOptions.None;//默认不忽略
                if(ret == MessageBoxResult.Cancel)
                    return;
                if(ret == MessageBoxResult.Yes)
                    option = RegexOptions.IgnoreCase;
                Regex regexStr = new Regex(@"\bmodule\s+(?<name>\w+)\s+\(", option);
                MatchCollection result = regexStr.Matches(CodeText);
                StringBuilder builder = new StringBuilder();
                foreach (Match res in result)
                {
                    builder.Append(res.Groups["name"].Value + "\n");
                }
                if (result.Count > 1)
                {
                    MessageBox.Show("该文件有多个module定义");
                }
                else if (result.Count == 1)
                {
                    if (FileName.EndsWith(".v"))
                    {
                        if (FileName.Substring(0,FileName.Length-2).Equals(result[0].Groups["name"].Value))
                            MessageBox.Show("文件名与module名一致");
                        else
                            MessageBox.Show("文件名与module名不一致");
                    }
                    else
                    {
                        MessageBox.Show("目前只支持verilog文件");
                    }
                }
                else
                {
                    MessageBox.Show("该文件没有module定义");
                }
            }
        }

        /// <summary>
        /// 检测过长变量名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectLongNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO 注释中的变量名不能算进去
            List<string> vlist = new List<string>();
            //正则表达式提取变量
            Regex regexStr = new Regex(@"(reg|wire|input|output)[ \t]*(\[\d+\:\d+\])?\s+(?<name>\w+)", RegexOptions.IgnoreCase);
            MatchCollection result = regexStr.Matches(CodeText);
            foreach (Match element in result)
            {
                if (element.Groups["name"].Value.Length > 20)
                {
                    vlist.Add(element.Groups["name"].Value);
                }
            }
            if (vlist.Count > 0)
            {
                vlist = vlist.Distinct().ToList();
                MessageBox.Show("过长(超过" + GlobalData.MaxVariableLength + ")的变量名：\n" + string.Join("\n", vlist.ToArray()));
            }
            else
            {
                MessageBox.Show("变量名均小于" + GlobalData.MaxVariableLength);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gene_StateMachine_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            TabItem tabitem = new TabItem();
            tabitem.Header = "状态机编辑器";
            tabControl.Items.Add(tabitem);
            tabControl.SelectedItem = tabitem;
        }

        /// <summary>
        /// 获取变量列表
        /// </summary>
        /// <returns></returns>
        private List<string> getVarNameList()
        {
            List<string> varlist = new List<string>();
            //正则表达式提取变量
            Regex regexStr = new Regex(@"^\s*(?:(?!//).)*\s*(reg|wire|input|output)[ \t]*(\[\d+\:\d+\])?\s+(?<name>\w+)\s*;", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection result = regexStr.Matches(CodeText);
            foreach (Match element in result)
            {
                varlist.Add(element.Groups["name"].Value);
            }
            if (varlist.Count > 0)
            {
                varlist = varlist.Where(t=>!t.Equals("")).Distinct().ToList();
            }

            return varlist;
        }

        private void DetectIfMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO if嵌套时可能出现检测失误
            //if if if else else else 此时else前面有end
            Regex regexStr = new Regex(@"\bif\b(?:(?!if).)*\belse\b(?:(?!if).)*\belse\b", 
                RegexOptions.IgnoreCase |RegexOptions.Singleline);
            if (regexStr.IsMatch(CodeText))
            {
                MessageBox.Show("超过5个判断分支");
            }
            else
            {
                MessageBox.Show("没有超过5个判断分支的If语句");
            }
        }

        /// <summary>
        /// 检测未使用的变量名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetectUnusedVarMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var list = getVarNameList();
            List<string> notused = new List<string>();
            foreach (string name in list)
            {
                //检测是否使用过
                Regex regex = new Regex(@"(?<=[\s\<\(\)\[\]\!\&\+\-\=;,])" + name + @"(?=[\s\<\(\)\[\]\!\&\+\-\=;,])",
                    RegexOptions.IgnoreCase);
                if (regex.Matches(CodeText).Count==1)
                {
                    notused.Add(name);
                }
            }
            if (notused.Count > 0)
            {
                MessageBox.Show("未使用的变量名：\n" + string.Join("\n", notused.ToArray()));
            }
            else
            {
                MessageBox.Show("没有未使用的变量名");
            }
        }

        CompletionWindow completionWindow;
        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Equals("."))
            {
                completionWindow = new CompletionWindow(textEditor.TextArea);
                // provide AvalonEdit with the data:
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("module"));
                data.Add(new MyCompletionData("endmodule"));
                data.Add(new MyCompletionData("always"));
                data.Add(new MyCompletionData("reg"));
                data.Add(new MyCompletionData("wire"));
                data.Add(new MyCompletionData("input"));
                data.Add(new MyCompletionData("output"));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // do not set e.Handled=true - we still want to insert the character that was typed
        }

        /// <summary>
        /// 加载自定义语法高亮
        /// </summary>
        private void loadCustomHighLighting()
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("CommentAnalyer.CustomHighlighting.xshd"))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Verilog", new string[] { ".cool" }, customHighlighting);

            textEditor.SyntaxHighlighting = customHighlighting;

            textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        }
       
    }
}
