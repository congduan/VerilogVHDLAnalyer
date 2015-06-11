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
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;

namespace CommentAnalyer
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {      
        public SettingsWindow()
        {
            InitializeComponent();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThresholdValueTextBox.Text = (this.Owner as MainWindow).ThresholdValue.ToString();
            TextEditorTextBox.Text = (this.Owner as MainWindow).TextEditor;
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyChange();
            this.Close();
        }

        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplyChange()
        {
            (this.Owner as MainWindow).ThresholdValue = Double.Parse(ThresholdValueTextBox.Text);
            (this.Owner as MainWindow).TextEditor = TextEditorTextBox.Text;
        }

        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 浏览编辑器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowserButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = @"文本编辑器(*.exe)|*.exe";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            bool? result = openFileDialog.ShowDialog();
            if (result == false)
            {
                return;
            }
            //文件名
            TextEditorTextBox.Text = openFileDialog.FileName;
        }          
    }
}
