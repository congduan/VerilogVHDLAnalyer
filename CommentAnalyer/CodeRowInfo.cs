using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CommentAnalyer
{
    /// <summary>
    /// 代码行信息
    /// </summary>
    public class CodeRowInfo : INotifyBase
    {
        private string NEW_LINE_STRING = "\r\n";
        private Brush _conclusionColor;
        private double _thresholdValue = -1;        

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            set;
            get;
        }

        public string FullFileName
        {
            set;
            get;
        }

        /// <summary>
        /// 所有行数
        /// </summary>
        public int TotalRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// 空行数
        /// </summary>
        public int BlankLineRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// 简单注释行数：//注释
        /// </summary>
        public int SimpleCommentRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// 复杂注释行数：/*...*/注释
        /// </summary>
        public int ComplexCommentRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// VHDL中的--注释
        /// </summary>
        public int VhdlCommentRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// 有效代码行数，不含空行，/**/注释行，含代码的//行
        /// </summary>
        public int CodeWithSimpleCommentRowNum
        {
            set;
            get;
        }

        /// <summary>
        /// 注释比例
        /// </summary>
        public string Percentage
        {
            get 
            {
                double per = ((double)TotalCommentRowNum / (double)ValidCodeRowNum);
                OnThresholdValueChanged(per);
                return per.ToString("F2");
            }
        }

        /// <summary>
        /// 总注释行数
        /// </summary>
        public int TotalCommentRowNum
        {
            get 
            {
                if (FileName.Length > 4)
                {
                    if (FileName.Substring(FileName.Length - 4, 4) == ".vhd")
                    {
                        return VhdlCommentRowNum;
                    }
                }
                return (SimpleCommentRowNum + ComplexCommentRowNum);
            }
        }

        /// <summary>
        /// 有效代码行数
        /// </summary>
        public int ValidCodeRowNum
        {
            get
            {
                return (TotalRowNum - BlankLineRowNum - SimpleCommentRowNum - VhdlCommentRowNum
                - ComplexCommentRowNum + CodeWithSimpleCommentRowNum);
            }
        }

        /// <summary>
        /// 是否通过检测，不通过为红色
        /// </summary>
        public Brush ConclusionColor
        {
            set 
            {
                _conclusionColor = value;
                OnPropertyChanged("ConclusionColor");
            }
            get
            {
                return _conclusionColor;
                //return Double.Parse(Percentage) < ThresholdValue ? Brushes.Red : Brushes.Transparent;
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
                OnThresholdValueChanged(double.Parse(Percentage));
                OnPropertyChanged("ThresholdValue");
            }
            get
            {
                return _thresholdValue;
            }
        }       

        private void OnThresholdValueChanged(double per)
        {
            if (ThresholdValue < 0)
                ThresholdValue = (App.Current.MainWindow as MainWindow).ThresholdValue;
            ConclusionColor = per < ThresholdValue ? Brushes.Red : Brushes.Transparent;
        }


        public CodeRowInfo()
        {
            Reset();
        }

        public CodeRowInfo(string filename,string fullname)
        {
            FileName = filename;
            FullFileName = fullname;            
            Reset();            
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {            
            TotalRowNum = 0;
            BlankLineRowNum = 0;
            SimpleCommentRowNum = 0;
            ComplexCommentRowNum = 0;
            CodeWithSimpleCommentRowNum = 0;
        }

        /// <summary>
        /// 显示
        /// </summary>
        public string Display()
        {            
            string disptext = null;
            disptext = "代码总行数：" + TotalRowNum + NEW_LINE_STRING;
            disptext += "有效代码行数：" + ValidCodeRowNum + NEW_LINE_STRING;
            disptext += "空行数：" + BlankLineRowNum + NEW_LINE_STRING;
            disptext += "//注释行数：" + SimpleCommentRowNum + NEW_LINE_STRING;
            disptext += "/*...*/注释行数：" + ComplexCommentRowNum + NEW_LINE_STRING;
            disptext += "带//注释的代码行数：" + CodeWithSimpleCommentRowNum + NEW_LINE_STRING;
            disptext += "总注释的代码行数：" + TotalCommentRowNum + NEW_LINE_STRING;
            disptext += "注释比率：" + Percentage + NEW_LINE_STRING;
            return disptext;                       
        }
    }
}
