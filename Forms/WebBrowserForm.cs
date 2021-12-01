﻿using Forms;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using ProductieManager.Rpm.Connection;
using Rpm.Productie;
using Rpm.Various;
using Various;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace ProductieManager.Forms
{
    public partial class WebBrowserForm : Form
    {
        public string[] FilesFormatToOpen { get; set; }
        public List<string> FilesToOpen { get; set; } = new List<string>();
        public bool CloseIfNotFound { get; set; }
        public bool OpenIfFound { get; set; }
        public List<string> FilesNotFound { get; private set; } = new List<string>();
        public bool ShowNotFoundList { get; set; }
        public ProgressArg Arg { get; set; } = new ProgressArg();
        public bool ShowErrorMessage { get; set; } = true;
        public bool StopNavigatingAfterError { get; set; } = true;
        public WebBrowserForm()
        {
            InitializeComponent();
        }

        private void InitNewBrowser()
        {
            try
            {
                this.SuspendLayout();
                //Browser?.Stop();
                this.Controls.Clear();
                Browser = new WebBrowser();
                Browser.ScriptErrorsSuppressed = true;
                Browser.IsWebBrowserContextMenuEnabled = false;
                // Browser.AllowWebBrowserDrop = false;
                Browser.Navigated += xBrowser_Navigated;
                Browser.Dock = DockStyle.Fill;
                this.Controls.Add(Browser);
                this.ResumeLayout(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public WebBrowser Browser { get; private set; }

        protected string NavigatingArtNr;

        public void Navigate(string artnr)
        {
            Arg.Message = $"Navigeren naar {artnr}...";
            Arg.OnChanged(this);
            if (Browser == null)
            {
                if (this.InvokeRequired)
                    this.Invoke(new MethodInvoker(InitNewBrowser));
                else
                    InitNewBrowser();
            }
            Browser.Document?.OpenNew(true);
            NavigatingArtNr = artnr;
            var xlink = AutoDeskHelper.GetTekeningPdfLink(artnr);
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => Browser?.Navigate(xlink, false)));
            }
            else
                Browser?.Navigate(xlink, false);
        }

        public void Navigate()
        {
            var xfirst = FilesToOpen?.FirstOrDefault();
            if (xfirst == null) return;
            Navigate(xfirst);
        }



        private HtmlNodeCollection GetNodeCollection(ref bool valid)
        {
            try
            {
                if (Disposing || IsDisposed) return null;
                if (Arg.IsCanceled) throw new Exception();
                if (string.Equals(Browser.DocumentTitle, "Navigatie is geannuleerd",
                        StringComparison.CurrentCultureIgnoreCase) || Browser.DocumentTitle.Contains("niet bereikbaar"))
                {
                    valid = false;
                    return null;
                }
                var html = Browser.DocumentText;
                var xdoc = new HtmlDocument();
                xdoc.LoadHtml(html);
                var xelements =
                    xdoc.DocumentNode.SelectNodes(
                        "/html[1]/body[1]/div[1]/div[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[1]/table[1]/tbody[1]");
                return xelements;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                valid = false;
                return null;
            }
        }

        private bool _hasFound;

        private async void xBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (_hasFound || FilesToOpen == null || FilesToOpen.Count == 0 ||
                FilesFormatToOpen == null || FilesFormatToOpen.Length == 0) return;
            if (Disposing || IsDisposed) return;
            Arg.Current++;
            Arg.OnChanged(this);
            if (Arg.IsCanceled)
            {
                this.Close();
                return;
            }
            HtmlNodeCollection xelements = null;
            bool isvalid = true;
            await Task.Factory.StartNew(new Action(() =>
            {
                
                for (int i = 0; i < 20; i++)
                {
                    Arg.Message = $"Gegevens laden van {NavigatingArtNr}...";
                    Arg.OnChanged(this);
                    if (Arg.IsCanceled)
                    {
                        break;
                    }
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            var xvalid = true;
                            xelements = GetNodeCollection(ref xvalid);
                            isvalid = xvalid;
                        }));
                    }
                    else xelements = GetNodeCollection(ref isvalid);

                    if (xelements != null || !isvalid) break;

                    Thread.Sleep(250);
                }
            }));
            Arg.OnChanged(this);
            if (Arg.IsCanceled)
            {
                this.Close();
                return;
            }
            try
            {
                if (!isvalid && StopNavigatingAfterError)
                {
                    throw new Exception(
                        "Verbinding kan niet tot stand worden gebracht.\n\nControlleer je netwerkinstellingen voor of je toegang hebt tot de server.");
                }

                FilesToOpen.Remove(NavigatingArtNr);
                var xnext = FilesToOpen.FirstOrDefault();
                if (xelements == null)
                {
                    if (!string.IsNullOrEmpty(xnext))
                        Navigate(xnext);
                    return;
                }
                
                string xdefname = "";
                foreach (var xf in FilesFormatToOpen)
                {
                    var xtitle = string.Format(xf, NavigatingArtNr);
                    Arg.Message = $"laden van {xtitle} ...";
                    Arg.OnChanged(this);
                    if (Arg.IsCanceled)
                    {
                        this.Close();
                        return;
                    }
                    var xfiles = xelements.FirstOrDefault()?.ChildNodes.FirstOrDefault(x => 
                        string.Equals(x.GetAttributeValue("data-name", xdefname), xtitle, StringComparison.CurrentCultureIgnoreCase));
                    if (xfiles == null)
                    {
                        if (FilesNotFound.IndexOf(NavigatingArtNr) < 0)
                            FilesNotFound.Add(NavigatingArtNr);
                        if (CloseIfNotFound)
                        {
                            XMessageBox.Show($"Geen FBR tekening gevonden voor {string.Join(", ", FilesToOpen)}",
                                "Geen Tekening Gevonden!", MessageBoxIcon.Exclamation);
                            if (this.InvokeRequired)
                                this.Invoke(new MethodInvoker(this.Close));
                            else this.Close();
                            return;
                        }
                    }
                    else if (OpenIfFound)
                    {
                        string xid = "";
                        var id = xfiles.GetAttributeValue("data-id", xid);
                        if (!string.IsNullOrEmpty(id))
                        {
                            _hasFound = true;
                            var xbbtn = FindElement("a", new string[] {id});
                            xbbtn?.InvokeMember("click", id);
                            return;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(xnext))
                    Navigate(xnext);
                else this.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //Arg.IsCanceled = true;
                Arg.Current = 0;
                Arg.Message = exception.Message;
                Arg.OnChanged(this);
                if (ShowErrorMessage)
                    XMessageBox.Show(exception.Message, "Fout", MessageBoxIcon.Warning);
                this.Dispose();
            }
        }

        private void ShowNotFountList()
        {
            if (FilesNotFound is {Count: > 0})
            {
                string xtmpfile = Path.Combine(Path.GetTempPath(), "Ont_FBR_tekeningen.txt");
                try
                {
                    using (var sw = new StreamWriter(xtmpfile))
                    {
                        var x1 = FilesNotFound.Count == 1 ? "tekening" : "tekeningen";
                        sw.WriteLine($"Totaal {FilesNotFound.Count} ontbrekende fabrikage {x1}.\n");
                        FilesNotFound = FilesNotFound.OrderBy(x=> x).ToList();
                        FilesNotFound.ForEach(x => sw.WriteLine(x));
                        sw.Close();
                    }

                    Process.Start(xtmpfile);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private HtmlElement FindElement(string tagname, string[] criterias)
        {
            var xelements = Browser.Document?.GetElementsByTagName(tagname);
            if (xelements == null || xelements.Count == 0) return null;
            var element = xelements.Cast<HtmlElement>()
                .FirstOrDefault(x => criterias.Any(c => x.OuterHtml.ToLower().Contains(c.ToLower()) && x.OuterHtml.ToLower().Contains("file_link")));
            return element;
        }


        private void WebBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Arg != null)
            {
                Arg.Current = 0;
                Arg.IsCanceled = true;
                Arg.OnChanged(this);
            }

            if (ShowNotFoundList)
            {
                ShowNotFountList();
            }
            this.SetLastInfo();
            if (Browser is {Disposing: false, IsDisposed: false})
                Browser.Dispose();
            if (!this.Disposing && !this.IsDisposed)
                this.Dispose();
        }

        private void WebBrowserForm_Shown(object sender, EventArgs e)
        {
            this.InitLastInfo();
        }

    }
}
