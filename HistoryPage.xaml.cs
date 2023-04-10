
using SQLitedb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wacom.Ink;
using static WillDevicesSampleApp.Net.ResultPage;
using DataGrid = System.Windows.Controls.DataGrid;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Newtonsoft.Json;
using Button = System.Windows.Controls.Button;
using System.Drawing;
using Brush = System.Windows.Media.Brush;

namespace WillDevicesSampleApp.Net
{
    /// <summary>
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {

        private Brush BtnColor;

        private int SelectedId;

        public HistoryPage()
        {
            InitializeComponent();

            BtnColor = EnglishBtn.BorderBrush;

            BindDataGridForChinese();
        }

        private void BindDataGridForChinese()
        {
            List<ChAssessment> ChineseAssessmentData;
            List<EngAssessment> EngAssessmentData;

            using (var db = new WritingsDbContext())
            {
                EngAssessmentData = db.EngAssessments.OrderByDescending(x => x.TimeStamp).ToList();
                //ChineseAssessmentData = db.ChAssessments.OrderBy(x =>x.TimeStamp ).ToList();
            }

            //ChineseDataGrid.DataContext = ChineseAssessmentData;
            EnglishDataGrid.DataContext = EngAssessmentData;
        }


        private void EnglishBtn_Click(object sender, RoutedEventArgs e)
        {
            EnglishBtn.BorderBrush = BtnColor;
            ChineseBtn.ClearValue(Border.BorderBrushProperty);

            // load English assement data
            ChineseDataGrid.Visibility = Visibility.Hidden;
            EnglishDataGrid.Visibility = Visibility.Visible;

            List<EngAssessment> EngAssessmentData;

            using (var db = new WritingsDbContext())
            {
                EngAssessmentData = db.EngAssessments.OrderByDescending(x => x.TimeStamp).ToList();
            }

            HideViewStroke();

            ViewStrokeBtn.Visibility = Visibility.Hidden;

            EnglishDataGrid.DataContext = EngAssessmentData;

        }
        private void ChineseBtn_Click(object sender, RoutedEventArgs e)
        {
            ChineseBtn.BorderBrush = BtnColor;
            EnglishBtn.ClearValue(Border.BorderBrushProperty);

            ShowChiAssessment();
        }

        private void ShowChiAssessment()
        {
            // load Chinese assement data
            EnglishDataGrid.Visibility = Visibility.Hidden;
            ChineseDataGrid.Visibility = Visibility.Visible;

            List<ChAssessment> ChineseAssessmentData;

            using (var db = new WritingsDbContext())
            {
                ChineseAssessmentData = db.ChAssessments.OrderByDescending(x => x.TimeStamp).ToList();
            }

            HideViewStroke();

            ViewStrokeBtn.Visibility = Visibility.Visible;

            ChineseDataGrid.DataContext = ChineseAssessmentData;
        }



        private void BackToHomeBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AssessmentPlatormMainPage());
        }

        private void ChineseDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            ChAssessment selecteditem = (ChAssessment)dataGrid.SelectedItem;
            
            if (selecteditem != null)
            {
                SelectedId = selecteditem.ChAssessmentId;
                ViewStrokeBtn.IsEnabled = true;
            }

        }

        private void ViewStrokeBtn_Click(object sender, RoutedEventArgs e)
        {
            string inccorectstrokestr = "";
            List<IncorrectStroke> incorrectstrokelist = new List<IncorrectStroke>();
            using (var db = new WritingsDbContext())
            {
                inccorectstrokestr = db.ChAssessments.Where(x => x.ChAssessmentId == SelectedId).Select(x => x.IncorrectStroke).FirstOrDefault();
                if(!string.IsNullOrEmpty(inccorectstrokestr))
                {
                    incorrectstrokelist = JsonConvert.DeserializeObject<List<IncorrectStroke>>(inccorectstrokestr);
                    StrokeDataGrid.DataContext = incorrectstrokelist;
                    ChineseDataGrid.Visibility = Visibility.Hidden;
                    StrokeDataGrid.Visibility = Visibility.Visible;

                    BackToAssessmentBtn.Visibility = Visibility.Visible;
                    ViewStrokeBtn.Visibility = Visibility.Collapsed;
                }
            }


        }

        private void HideViewStroke()
        {
            if(StrokeDataGrid.Visibility == Visibility.Visible)
            {
                StrokeDataGrid.Visibility = Visibility.Hidden;
            }
            if(BackToAssessmentBtn.Visibility == Visibility.Visible)
            {
                BackToAssessmentBtn.Visibility = Visibility.Collapsed;
            }
            if(ViewStrokeBtn.Visibility == Visibility.Hidden)
            {
                ViewStrokeBtn.Visibility= Visibility.Visible;
            }
            SelectedId = -1;
            ViewStrokeBtn.IsEnabled = false;
        }

        private void BackToAssessmentBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowChiAssessment();
        }

        private void ViewStrokeBtn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var btn = (Button)sender;
            if (!btn.IsEnabled)
            {
                btn.Foreground = (Brush)new BrushConverter().ConvertFrom("#a6a6a6");
            }
            else
            {
                btn.Foreground = (Brush)new BrushConverter().ConvertFrom("#ffffff");
            }
        }
    }
}
