using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

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
            var model = new LicenseInformationModel(
                "Newtonsoft",
                "James NK",
                new List<Text>() {
                    new LicenseText("MIT", new Uri("https://spdx.org/licenses/MIT.html")) ,
                    new FreeText(" AND "),
                    new LicenseText("Apache-2.0", new Uri("https://spdx.org/licenses/Apache-2.0.html"))
                    }
                );
            var model2 = new LicenseInformationModel(
                "NuGet.Protocol",
                "NuGet",
                new List<Text>() {
                    new LicenseText("Adobe-Glyph", new Uri("https://spdx.org/licenses/Adobe-Glyph.html")) ,
                    }
                );

            var models = new List<LicenseInformationModel>() { model2, model };

            DataContext = models;

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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

        }
    }
}
