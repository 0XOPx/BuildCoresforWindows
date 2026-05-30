using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace BuildCoresforWindows
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chromeBrowser;

        public Form1()
        {
            InitializeComponent();
            InitializeChromium();
        }

        private void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            if (Cef.IsInitialized == false)
            {
                Cef.Initialize(settings);
            }

            chromeBrowser = new ChromiumWebBrowser("https://buildcores.com/");
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;
        }

        private async void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                try
                {
                    string script = @"
                        (function() {
                            var link = document.querySelector(""link[rel*='icon']"");
                            return link ? link.href : '';
                        })();";

                    JavascriptResponse response = await e.Frame.EvaluateScriptAsync(script);

                    if (response.Success && response.Result != null && !string.IsNullOrEmpty(response.Result.ToString()))
                    {
                        string iconUrl = response.Result.ToString();

                        using (HttpClient client = new HttpClient())
                        {
                            byte[] data = await client.GetByteArrayAsync(iconUrl);
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                using (Bitmap bitmap = new Bitmap(ms))
                                {
                                    IntPtr hIcon = bitmap.GetHicon();
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        this.Icon = Icon.FromHandle(hIcon);
                                    }));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}