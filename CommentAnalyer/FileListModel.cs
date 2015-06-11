using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CommentAnalyer
{
    public class FileListModel: INotifyPropertyChanged
    {
        private string _filename;
        private bool _isVisiable;

        public string FileName
        {
            set
            {
                _filename = value;
                OnPropertyChanged("FileName");
            }
            get
            {
                return _filename;
            }
        }

        /// <summary>
        /// 文件完整名称
        /// </summary>
        public string FullFileName
        {
            set;
            get;
        }

        public bool IsVisiable 
        {
            set
            {
                _isVisiable = value;
                OnPropertyChanged("IsVisiable");
            }
            get
            {
                return _isVisiable;
            }
        }

        public FileListModel(string filename,string fullname)
        {
            FileName = filename;
            FullFileName = fullname;
            IsVisiable = true;
        }

        public override string ToString()
        {
            return FullFileName;
        }

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
    }
}
