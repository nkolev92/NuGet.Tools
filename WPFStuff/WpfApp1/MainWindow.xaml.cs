using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Link p1 = new Link() { FirstName = "rob", LastName = "reglyea" };
            var nl1 = new NonLink() { Text = " and " };
            Link p2 = new Link() { FirstName = "rob4", LastName = "relayea" };
            var nl2 = new NonLink() { Text = " and " };
            Link p4 = new Link() { FirstName = "ro2b4", LastName = "reldyea" };
            Link p3 = new Link() { FirstName = "rob2", LastName = "relygea" };

            IC.Items.Add(p1);
            IC.Items.Add(nl1);
            IC.Items.Add(p2);
            IC.Items.Add(nl2);
            IC.Items.Add(p4);
            IC.Items.Add(p3);
        }

        private void IC_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            //e.Uri.ToString();
        }
    }
}
