/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 08/13/2014
 * Time: 20:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CommentAnalyer.Model;
using System.Linq;

namespace CommentAnalyer.View
{
	/// <summary>
	/// Interaction logic for InputOutputWindow.xaml
	/// </summary>
	public partial class InputOutputWindow : Window
	{
        List<VerilogVariable> _variables = null;
        List<VerilogVariable> _variablesAll = null;
        private FilterTypes _filterType = FilterTypes.InputOutput;

        public enum FilterTypes : byte
        {
            All,//全部
            InputOutput,//输入输出
            NotInputOutput//非输入输入（内部变量）
        }

        public InputOutputWindow(List<VerilogVariable> variables,FilterTypes filtertype = FilterTypes.InputOutput)
		{
			InitializeComponent();
            _variablesAll = variables;
            _filterType = filtertype;
            this.Loaded += new RoutedEventHandler(InputOutputWindow_Loaded);
		}

        void InputOutputWindow_Loaded(object sender, RoutedEventArgs e)
        {
            switch (_filterType)
            {
                case FilterTypes.All:
                    filterComboBox.SelectedIndex = 0;
                    _variables = _variablesAll;
                    break;
                case FilterTypes.InputOutput:
                    filterComboBox.SelectedIndex = 1;
                    _variables = _variablesAll.Where(t => t is InputOutputPort).ToList();
                    break;
                case FilterTypes.NotInputOutput:
                    filterComboBox.SelectedIndex = 2;
                    _variables = _variablesAll.Where(t => !(t is InputOutputPort)).ToList();
                    break;
                default:
                    filterComboBox.SelectedIndex = 1;
                    _variables = _variablesAll.Where(t => t is InputOutputPort).ToList();
                    break;
            }

            datagrid.ItemsSource = _variables;
        }      

        private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_variables != null)
            {
                if (filterComboBox.SelectedIndex == 0)
                {
                    datagrid.ItemsSource = _variables;
                }
                else if (filterComboBox.SelectedIndex == 1)
                {
                    datagrid.ItemsSource = _variablesAll.Where(t => t is InputOutputPort).ToList();
                }
                else if (filterComboBox.SelectedIndex == 2)
                {
                    datagrid.ItemsSource = _variables.Where(t => !(t is InputOutputPort)).ToList();
                }
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 导出列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportBtn_Click(object sender, RoutedEventArgs e)
        {

        }

      
		
		
	}
}