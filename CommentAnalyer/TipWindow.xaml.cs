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

namespace CommentAnalyer
{
    /// <summary>
    /// TipWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TipWindow : Window
    {
        //所有文件个数
        public int AllFolderCount
        {
            get { return (int)GetValue(AllFolderCountProperty); }
            set { SetValue(AllFolderCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllFileCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllFolderCountProperty =
            DependencyProperty.Register("AllFolderCount", typeof(int), typeof(TipWindow), new UIPropertyMetadata(0));

        public TipWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += new RoutedEventHandler(TipWindow_Loaded);
        }

        void TipWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Binding bd = new Binding();
            bd.Source = this.Owner as MainWindow;
            bd.Path = new PropertyPath("AllFolderCount");
            bd.Mode = BindingMode.TwoWay;
            bd.NotifyOnSourceUpdated = true;
            this.SetBinding(TipWindow.AllFolderCountProperty, bd);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("停止搜索");
        }
    }
}
