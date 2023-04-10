using SQLitedb;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WillDevicesSampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //SourceInitialized += OnSourceInitialized;
            InitializeComponent();

            using (var db = new WritingsDbContext())
            {
                var tests = db.StrokeDatas.ToList();
                foreach (var test in tests)
                {
                    Debug.WriteLine(test.Character);
                    //Debug.WriteLine(test.StrokeCount);
                }
                /*
                var newStroke = new StrokeData();
                newStroke.Character = "荷";
                newStroke.StrokeCount = 11;
                db.StrokeDatas.Add(newStroke);
                db.SaveChanges();
                */
            }

        }

        private void CloseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

#if false
    private void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = frmContent.Content is MainPage;
    }

    private void PropsCmdExecuted(object target, ExecutedRoutedEventArgs e)
    {
      WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
    }

    private void PropsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }

#endif
    }
}
