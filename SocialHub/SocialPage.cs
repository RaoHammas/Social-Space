using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace SocialSpace
{
    public class SocialPage : INotifyPropertyChanged
    {
        private WebView2 _webView2;
        private string _url;

        public SocialPage(string url)
        {
            Url = url;
            WebView2 = new WebView2();
            InitializeAsync();
        }


        private async void InitializeAsync()
        {
            await WebView2.EnsureCoreWebView2Async(null);
            WebView2.NavigationCompleted += WebView2OnNavigationCompleted;
            WebView2.CoreWebView2.Navigate(Url);
        }

        private void WebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                WebView2.CoreWebView2.Settings.IsStatusBarEnabled = false;
                WebView2.CoreWebView2.ExecuteScriptAsync(GetScrollbarStyleJs());
            }
            catch (Exception exception)
            {
            }
        }

        private string GetScrollbarStyleJs()
        {
            return @"
                        var style = document.createElement('style');
                        style.type = 'text/css';
                        style.innerHTML = `

::-webkit-scrollbar-track
{
	-webkit-box-shadow: inset 0 0 6px rgba(0,0,0,0.3);
	border-radius: 10px;
	background-color: #E3EEF9;
}

::-webkit-scrollbar
{
	width: 8px;
	background-color: #E3EEF9;
}

::-webkit-scrollbar-thumb
{
	border-radius: 10px;
	-webkit-box-shadow: inset 0 0 6px rgba(0,0,0,.3);
	background-color: #007ACC
}
                                            
                                            `;
document.getElementsByTagName('body')[0].appendChild(style);";
        }

        public WebView2 WebView2
        {
            get => _webView2;
            set
            {
                _webView2 = value;
                OnPropertyChanged(nameof(WebView2));
            }
        }

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}