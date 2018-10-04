using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var list = new List<Text>();

            list.Add(new LicenseText("MIT", new Uri("https://spdx.org/licenses/MIT.html")));
            list.Add(new FreeText(" AND "));
            list.Add(new LicenseText("Apache-2.0", new Uri("https://spdx.org/licenses/Apache-2.0.html")));

            DataContext = list;

        }

        private void OnViewLicenseTermsRequestNavigate(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            if (hyperlink != null
                && hyperlink.NavigateUri != null)
            {
                OpenUri(hyperlink.NavigateUri);
                e.Handled = true;
            }
        }

        private void OpenUri(Uri url)
        {

            if (url == null
              || !url.IsAbsoluteUri)
            {
                return;
            }

            // mitigate security risk
            if (url.IsFile
                || url.IsLoopback
                || url.IsUnc)
            {
                return;
            }

            if (IsHttpUrl(url))
            {
                // REVIEW: Will this allow a package author to execute arbitrary program on user's machine?
                // We have limited the url to be HTTP only, but is it sufficient?
                // If browser has issues opening the url, unhandled exceptions may crash VS
                try
                {
                    Process.Start(url.AbsoluteUri);
                }
                catch
                {
                }
            }
        }
        private static bool IsHttpUrl(Uri uri)
        {
            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
