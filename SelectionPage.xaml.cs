using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WillDevicesSampleApp.Net
{
    /// <summary>
    /// Interaction logic for SelectionPage.xaml
    /// </summary>
    public partial class SelectionPage : System.Windows.Controls.Page
    {
        private bool EngPressed = false;
        private bool ChiPressed = false;
        private string passagestring;
        private string studentname;

        public SelectionPage()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textbox = e.Source as System.Windows.Controls.TextBox;
            studentname = textbox.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (EngPressed)
            {
                passagestring = "English";
            }
            else
            {
                passagestring = "Chinese";
            }
            dynamic o = new ExpandoObject();
            o.passagestring = passagestring;
            o.studentname = studentname;
            NavigationService.Navigate(new RealTimeInkPage(o));
        }

        private void Hover(object sender, MouseEventArgs e)
        {
            StackPanel s = e.Source as StackPanel;
            var name = s.Name;

            Object border = s.FindName("b"+name);
            Object textblock = s.FindName("t" + name);
            if (border is Border )
            {
                if(name == "Eng" && !EngPressed || name == "Chi" && !ChiPressed)
                {

                    // Following executed if Text element was found.
                    Border b = border as Border;
                    b.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#005A34");
                    b.BorderThickness = new Thickness(2);
                }

            }
            if(textblock is TextBlock)
            {
                if (name == "Eng" && !EngPressed || name == "Chi" && !ChiPressed)
                {

                    // Following executed if Text element was found.
                    TextBlock t = textblock as TextBlock;
                    t.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#005A34");
                }
            }
        }

        private void Leave(object sender, MouseEventArgs e)
        {
            StackPanel s = e.Source as StackPanel;
            var name = s.Name;
            Object border = s.FindName("b" + name);
            Object textblock = s.FindName("t" + name);
            if (border is Border)
            {
                if (name == "Eng" && !EngPressed || name == "Chi" && !ChiPressed)
                {
                    Debug.WriteLine("Hover");
                    Debug.WriteLine(name);
                    // Following executed if Text element was found.
                    Border b = border as Border;
                    b.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                    b.BorderThickness = new Thickness(1);
                }
            }
            if (textblock is TextBlock)
            {
                if (name == "Eng" && !EngPressed || name == "Chi" && !ChiPressed)
                {

                    // Following executed if Text element was found.
                    TextBlock t = textblock as TextBlock;
                    t.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#474554");
                }
            }
        }

        private void Press(object sender, MouseButtonEventArgs e)
        {
            StackPanel s = e.Source as StackPanel;
            if(e.Source != null)
            {
                var name = s.Name;
                Object thisborder = new Object();
                Object thistextblock = new Object();
                Object oppoborder = new Object();
                Object oppotextblock = new Object();
                if (name == "Eng")
                {
                    EngPressed = true;
                    ChiPressed = false;
                    thisborder = s.FindName("bEng");
                    thistextblock = s.FindName("tEng");
                    oppoborder = s.FindName("bChi");
                    oppotextblock = s.FindName("tChi");
                }
                else
                {
                    EngPressed = false;
                    ChiPressed = true;
                    thisborder = s.FindName("bChi");
                    thistextblock = s.FindName("tChi");
                    oppoborder = s.FindName("bEng");
                    oppotextblock = s.FindName("tEng");
                }

                Border tb = thisborder as Border;
                Border ob = oppoborder as Border;
                tb.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#005A34");
                tb.BorderThickness = new Thickness(3);
                ob.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                ob.BorderThickness = new Thickness(1);

                TextBlock tt = thistextblock as TextBlock;
                TextBlock ot = oppotextblock as TextBlock;
                tt.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#005A34");
                ot.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#474554");
            }



        }
    }
}
