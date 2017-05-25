using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FanpageTool.ViewModel
{
    public class MainWindowViewModel :INotifyPropertyChanged
    {
        private StringBuilder m_commandText = new StringBuilder("Fanpage Facebook Tool");
        private bool m_isInit = false;

        public MainWindowViewModel() 
        {
            IsInit = false;
        }

        public bool IsInit
        {
            get
            {
                return m_isInit;
            }
            set
            {
                m_isInit = value;
                OnPropertyChanged("IsInit");
            }
        }

        public StringBuilder CommandText 
        {
            get
            {
                return m_commandText;
            }
            set
            {
                m_commandText = value;
                OnPropertyChanged("CommandText");
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event to which the view's controls will subscribe.
        /// This will enable them to refresh themselves when the binded property changes provided you fire this event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// When property is changed call this method to fire the PropertyChanged Event
        /// </summary>
        /// <param name="propertyName"></param>
        /// 
        //! OnPropertyChanged function for data binding
        /*!
         \param string propertyName - property name for update GUI
         \return void
        */
        public void OnPropertyChanged(string propertyName)
        {
            //Fire the PropertyChanged event in case somebody subscribed to it
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
