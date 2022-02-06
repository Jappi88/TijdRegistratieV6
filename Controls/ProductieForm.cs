using Forms;
using Forms.GereedMelden;
using ProductieManager.Forms;
using ProductieManager.Properties;
using ProductieManager.Rpm.Misc;
using Rpm.Misc;
using Rpm.Productie;
using Rpm.Settings;
using Rpm.Various;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using Various;

namespace Controls
{
    public partial class ProductieForm : UserControl
    {
        private Bewerking _bewerking;

        public ProductieForm()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            xnotepanel.Visible = true;
            xnotepanel.Height = 32;
        }

        public ProductieFormulier Formulier { get; set; }

        public Bewerking SelectedBewerking
        {
            get => _bewerking;
            set
            {
                _bewerking = value;
                if (Disposing || IsDisposed) return;
                if (value != null)
                {
                    xbewerking.SelectedItem = xbewerking.Items.Cast<string>()
                        .FirstOrDefault(t => string.Equals(t, value.Naam, StringComparison.CurrentCultureIgnoreCase));
                    UpdateFields();
                }
            }
        }

        public void SetParent(ProductieFormulier form)
        {
            if (Disposing || IsDisposed) return;
            if (form != null)
            {
                Formulier = form;
                if (InvokeRequired)
                    Invoke(new Action(UpdateFields));
                else
                    UpdateFields();
            }
        }

        private void ResizeStatusLable()
        {
            if (Disposing || IsDisposed) return;
            var startfont = new Font(xstatuslabel.Font.FontFamily, 20);
            var txtsize = xstatuslabel.Text.MeasureString(startfont);
            while (txtsize.Width > Width - 100)
            {
                startfont = new Font(startfont.FontFamily, startfont.SizeInPoints - 0.5f);

                txtsize = xstatuslabel.Text.MeasureString(startfont);
                if (startfont.SizeInPoints <= 10) break;
            }

            xstatuslabel.Font = startfont;
            xstatuslabel.Invalidate();
        }

        private void CreateTextField(Bewerking bewerking)
        {
            if (Disposing || IsDisposed) return;
            try
            {
                var wps = bewerking.State == ProductieState.Gestart
                    ? "op " + string.Join(", ", bewerking.WerkPlekken.Where(x=> x.IsActief()).Select(x => x.Naam))
                    : "";

                xstatuslabel.Text =
                    $"{bewerking.Naam} {Enum.GetName(typeof(ProductieState), bewerking.State)?.ToUpper()} {wps}";
                ResizeStatusLable();
                productieInfoUI1.SetInfo(bewerking, bewerking.Omschrijving, Color.AliceBlue, Color.White,
                    Color.Black);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void xbewerking_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Disposing || IsDisposed) return;
            if (xbewerking.SelectedIndex < 0 || xbewerking.SelectedIndex >= Formulier.Bewerkingen.Length)
                return;
            //if (InvokeRequired)
            //    Invoke(new Action(UpdateFields));
            //else
            UpdateFields();
        }

        public Bewerking CurrentBewerking()
        {
            if (Disposing || IsDisposed) return null;
            if (xbewerking.SelectedItem == null) return null;
            return Formulier?.Bewerkingen?.FirstOrDefault(t =>
                string.Equals(t.Naam, xbewerking.SelectedItem.ToString(), StringComparison.CurrentCultureIgnoreCase));
        }

        public void OnSettingChanged(object instance, UserSettings settings, bool init)
        {
            mainMenu1.OnSettingChanged(instance, settings, init);
            BeginInvoke(new MethodInvoker(UpdateFields));
        }

        public void UpdateFields()
        {
            if (Formulier == null || Disposing || IsDisposed)
                return;
            xbewerking.SelectionChangeCommitted -= xbewerking_SelectedIndexChanged;
            try
            {
                string selectedname = null;
                if (xbewerking.SelectedItem != null)
                    selectedname = xbewerking.SelectedItem.ToString();

                var names = new List<object>();

                if (Formulier.Bewerkingen != null)
                    names.AddRange(from s in Formulier.Bewerkingen
                        where s.IsAllowed() && s.State != ProductieState.Verwijderd
                        select (object) s.Naam);

                var olditems = xbewerking.Items.Cast<string>().ToArray();
                var thesame = olditems.Length == names.Count && olditems.All(x =>
                    names.Any(s => string.Equals(x, s.ToString(), StringComparison.CurrentCultureIgnoreCase)));
                if (!thesame)
                {
                    xbewerking.Items.Clear();
                    xbewerking.Items.AddRange(names.ToArray());
                }


                if (selectedname != null)
                {
                    xbewerking.SelectedItem = selectedname;
                }
                else if (xbewerking.Items.Count > 0)
                {
                    xbewerking.SelectedItem = xbewerking.Items[0];
                    if (xbewerking.SelectedItem != null)
                        selectedname = xbewerking.SelectedItem as string;
                }

                var b = CurrentBewerking();
                if (b == null && selectedname != null && Formulier.Bewerkingen != null)
                    b = Formulier.Bewerkingen.FirstOrDefault(x =>
                        string.Equals(x.Naam, selectedname, StringComparison.CurrentCultureIgnoreCase));
                if (b != null)
                {
                    _bewerking = b;
                    if (!string.IsNullOrEmpty(b.Note?.Notitie))
                    {
                        xnoteButton.Text =
                            $@"LET OP! Er is een notitie geplaatst op {b.Note.DatumToegevoegd} door '{b.Note.Naam}'";
                        xnotepanel.Visible = true;
                        xnoteTextbox.Text = b.Note.Notitie.Trim();
                    }
                    else
                    {
                        xnoteButton.Text = @"LET OP! Er is een notitie geplaatst!";
                        xnoteTextbox.Text = "";
                        xnotepanel.Visible = false;
                    }

                    var xmeldgereed = mainMenu1.GetMenuButton("xmeldgereed");
                    if (xmeldgereed != null)
                        switch (b.State)
                        {
                            case ProductieState.Verwijderd:
                                mainMenu1.Enable("xmeldgereed", false);
                                xmeldgereed.Text = "Meld Gereed";
                                break;
                            case ProductieState.Gereed:
                                mainMenu1.Enable("xmeldgereed", false);
                                xmeldgereed.Text = "Al Gereed Gemeld";
                                break;
                            default:
                                mainMenu1.Enable("xmeldgereed", true);
                                xmeldgereed.Text = "Meld Gereed";
                                break;
                        }

                    var xdeelgereed = mainMenu1.GetMenuButton("xdeelmeldingen");
                    if (xdeelgereed != null)
                        switch (b.State)
                        {
                            case ProductieState.Verwijderd:
                                mainMenu1.Enable("xdeelmeldingen", false);
                                xdeelgereed.Text = "Meld Deel Gereed";
                                break;
                            case ProductieState.Gereed:
                                mainMenu1.Enable("xdeelmeldingen", false);
                                xdeelgereed.Text = "Al Gereed Gemeld";
                                break;
                            default:
                                mainMenu1.Enable("xdeelmeldingen", true);
                                xdeelgereed.Text = "Meld Deel Gereed";
                                break;
                        }

                    mainMenu1.Enable("xundo", b.State == ProductieState.Gereed || b.State == ProductieState.Verwijderd);
                    var xrooster = mainMenu1.GetButton("xrooster");
                    if (xrooster != null)
                        xrooster.Image = b.WerkPlekken.Any(x =>
                            x.Tijden._rooster != null && x.Tijden.WerkRooster.IsCustom())
                            ? Resources.schedule_32_32.CombineImage(Resources.exclamation_warning_15590, 1.75)
                            : Resources.schedule_32_32;
                    var xstoring = mainMenu1.GetButton("xonderbreking");
                    
                    if (xstoring != null)
                    {
                        var img = b.GetAlleStoringen(true).Length == 0
                            ? Resources.onderhoud32_321
                            : Resources.onderhoud32_321.CombineImage(Resources.exclamation_warning_15590, 1.75);
                        //img = storingen.Length == 0
                        //    ? img
                        //    : img.WriteText(storingen.Length.ToString(), StringAlignment.Near,
                        //        new Font("Segoe UI", 16, FontStyle.Bold), Brushes.Red);
                        xstoring.Image = img;
                    }

                    //mainMenu1.Enable("xwerktijden", !b.IsBemand);
                    mainMenu1.Enable("xopenpdf", b.Parent != null && b.Parent.ContainsProductiePdf());
                    //update de start knop
                    switch (b.State)
                    {
                        case ProductieState.Gestopt:
                            if (xstartb != null)
                            {
                                xstartb.Enabled = true;
                                xstartb.Text = "Start";
                                xstartb.Image = Resources.play_button_icon_icons_com_60615;
                            }

                            
                            xstatuslabel.ForeColor = Color.DarkRed;
                            xstatusimage.Image = Resources.stop_red256_24890;
                            break;
                        case ProductieState.Gestart:
                            if (xstartb != null)
                            {
                                xstartb.Enabled = true;
                                xstartb.Text = "Stop";
                                xstartb.Image = Resources.stop_red256_24890;
                            }
                            xstatuslabel.ForeColor = Color.DarkGreen;
                            xstatusimage.Image = Resources.play_button_icon_icons_com_60615;
                            break;
                        case ProductieState.Gereed:
                            if (xstartb != null)
                            {
                                xstartb.Text = "Gereed";
                                xstartb.Enabled = false;
                                xstartb.Image = Resources.check_1582;
                            }
                            xstatuslabel.ForeColor = Color.DodgerBlue;
                            xstatusimage.Image = Resources.check_1582;
                            break;
                        case ProductieState.Verwijderd:
                            break;
                    }

                    //update progressbar
                    var xpcolor = Color.DarkRed;
                    switch (b.State)
                    {
                        case ProductieState.Gestopt:
                            xpcolor = Color.DarkRed;
                            break;
                        case ProductieState.Gestart:
                            xpcolor = Color.Green;
                            break;
                        case ProductieState.Gereed:
                            xpcolor = Color.RoyalBlue;
                            break;
                        case ProductieState.Verwijderd:
                            xpcolor = Color.Red;
                            break;
                    }
                    xprogressbar.ForeColor = xpcolor;
                    xOnderbreek.Enabled = b.State is ProductieState.Gestart or ProductieState.Gestopt;
                    //update de onderbrekingknop
                    var xstoringen = b.GetStoringen(true);
                    if (xstoringen.Length > 0)
                    {
                        xprogressbar.Style = ProgressBarStyle.Marquee;
                        xOnderbreek.Text = "Hervatten";
                        xOnderbreek.Image = Resources.playcircle_32x32;
                    }
                    else
                    {
                        xprogressbar.Style = ProgressBarStyle.Blocks;
                        xOnderbreek.Text = "Onderbreek";
                        xOnderbreek.Image = Resources.Stop_Hand__32x32;
                    }

                    int xpercent = (int) b.GereedPercentage();
                    xprogressbar.ProgressColor = Functions.GetProgressColor(xpercent);
                    xprogressbar.Value = (xpercent > xprogressbar.Maximum? xprogressbar.Maximum : xpercent);
                    xprogressbar.Text = $"{b.TotaalGemaakt}/{b.Aantal}\n{xpercent}%";
                    var tg = b.TijdAanGewerkt();
                    xprogressbar.Invalidate();
                    //xstatus.Text = (string.IsNullOrEmpty(Formulier.Notitie) ? Formulier.Omschrijving : Formulier.Notitie).Replace("\n", " ");
                    var pers = b.AantalActievePersonen;
                    var xpersoneelb = mainMenu1.GetMenuButton("xindeling");
                    if (xpersoneelb != null)
                        xpersoneelb.Text = $"Beheer Indeling [{pers}]";
                    xindelingb.Text = $"Indeling [{pers}]";
                    Text =
                        $"{Formulier.ArtikelNr} [{Formulier.ProductieNr}][{Enum.GetName(typeof(ProductieState), b.State)?.ToUpper()}]";
                    if (pers == 0 && b.State == ProductieState.Gestart)
                        b.StopProductie(true,true).Wait();
                    CreateTextField(b);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            xbewerking.SelectionChangeCommitted += xbewerking_SelectedIndexChanged;
        }

        private async void StartBewerking(Control sender)
        {
            if (Disposing || IsDisposed) return;
            if (xbewerking.SelectedItem == null || Formulier.Bewerkingen == null)
                return;
            try
            {
                var b = CurrentBewerking();
                if (b != null)
                {
                    if (b.State == ProductieState.Gestopt)
                        //if (b.AantalActievePersonen == 0)
                        //{
                        //    var x = new Indeling(Formulier, b);
                        //    x.StartPosition = FormStartPosition.CenterParent;
                        //    if (x.ShowDialog() == DialogResult.OK) await b.StartProductie(true, false);
                        //}
                        //else
                        //{
                        //    await b.StartProductie(true, true);
                        //}
                        ProductieListControl.StartBewerkingen(this,new[] {b});
                    else if (b.State == ProductieState.Gestart) await b.StopProductie(true,true);
                }
            }
            catch (Exception ex)
            {
                XMessageBox.Show(this, $"Kan productie niet starten!\n\n{ex.Message}", "Fout", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void xstopb_Click(object sender, EventArgs e)
        {
            CloseButtonPressed();
        }

        private void WijzigProductie()
        {
            if (Formulier != null)
            {
                var ap = new WijzigProductie(Formulier);
                ap.ShowDialog(this);
            }
        }

        private void WijzigIndeling()
        {
            if (Disposing || IsDisposed) return;
            var b = CurrentBewerking();
            if (b == null)
                return;
            var x = new Indeling(Formulier, b);
            x.StartPosition = FormStartPosition.CenterParent;
            x.ShowDialog();
        }

        private void MeldGereed()
        {
            if (Disposing || IsDisposed) return;
            var p = Formulier;
            if (p != null && p.State != ProductieState.Verwijderd && p.State != ProductieState.Gereed)
            {
                var b = CurrentBewerking();
                if (b != null)
                {
                    var x = new GereedMelder
                    {
                        Aantal = p.AantalGemaakt,
                        Naam = b.Naam,
                        StartPosition = FormStartPosition.CenterParent,
                        Size = new Size(Width, 350),
                        Paraaf = p.Paraaf
                    };
                    x.ShowDialog(b);
                }
            }
        }

        private void SetAantalGemaakt()
        {
            if (Disposing || IsDisposed) return;
            if (Formulier == null ||
                Formulier.State == ProductieState.Verwijderd)
                return;
            var b = CurrentBewerking();
            if (b != null)
            {
                productieInfoUI1.ShowAantal = !productieInfoUI1.ShowAantal;
                //var aantal = new AantalGemaaktUI();
                //aantal.Width = Width + 10;
                //aantal.ShowDialog(Formulier, b);
            }
        }

        private void ShowAanbevolenPersoneel()
        {
            if (Disposing || IsDisposed) return;
            if (Formulier == null ||
                Formulier.State == ProductieState.Verwijderd)
                return;
            var b = CurrentBewerking();
            if (b != null)
                try
                {
                    new AanbevolenPersonenForm(b).ShowDialog();
                }
                catch (Exception e)
                {
                    XMessageBox.Show(this, e.Message, "Geen Aanbevelingen");
                }
        }

        private void ShowSelectedBewStoringen(WerkPlek selected = null)
        {
            var b = CurrentBewerking();
            if (b != null)
            {
                var allst = new AlleStoringen();
                allst.InitStoringen(b.Root, selected);
                allst.ShowDialog();
            }
        }

        private async void ShowDeelMeldingen()
        {
            var b = CurrentBewerking();
            if (b == null) return;
            var wc = new DeelsGereedMeldingenForm(b);
            if (wc.ShowDialog() == DialogResult.OK)
            {
                b = wc.Bewerking;
                await b.UpdateBewerking(null, $"[{b.Path}] Deels Gereedmeldingen Aangepast");
            }
        }


        private void ShowProductiePdf()
        {
            var b = CurrentBewerking();
            b?.Parent?.OpenProductiePdf();
        }

        public event EventHandler OnCloseButtonPressed;

        public void CloseButtonPressed()
        {
            OnCloseButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        private void xstartb_Click(object sender, EventArgs e)
        {
            StartBewerking(this);
        }

        private void xindelingb_Click(object sender, EventArgs e)
        {
            WijzigIndeling();
        }

        private void mainMenu1_OnMenuClick(object sender, EventArgs e)
        {
            if (sender is Button {Tag: MenuButton button})
                switch (button.Name.ToLower())
                {
                    case "xwijzigproductie":
                        WijzigProductie();
                        break;
                    case "xmeldgereed":
                        MeldGereed();
                        break;
                    case "xdeelmeldingen":
                        ShowDeelMeldingen();
                        break;
                    case "xindeling":
                        WijzigIndeling();
                        break;
                    case "xaantalgemaakt":
                        SetAantalGemaakt();
                        break;
                    case "xaanbevolenpersonen":
                        ShowAanbevolenPersoneel();
                        break;
                    case "xrooster":
                        CurrentBewerking()?.DoBewerkingEigenRooster(this);
                        break;
                    case "xonderbreking":
                        ShowSelectedBewStoringen();
                        break;
                    case "xwerktijden":
                        CurrentBewerking()?.ShowWerktIjden(this);
                        break;
                    case "xopenpdf":
                        ShowProductiePdf();
                        break;
                    case "xundo":
                        UndoBewerking();
                        break;
                    case "xdelete":
                        RemoveBewerking();
                        break;
                }
        }

        private void RemoveBewerking()
        {
            var xselected = CurrentBewerking();
            if (xselected == null) return;
            var res = XMessageBox.Show(this, $"Wil je de geselecteerde producties helemaal verwijderen?\n\n" +
                                   "Click op 'Ja' als je helemaal van de database wilt verwijderen.\n" +
                                   "Click op 'Nee' als je alleen op een verwijderde status wilt te zetten.", "",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res != DialogResult.Cancel)
            {
                xselected.RemoveBewerking(res == DialogResult.No, res == DialogResult.Yes);
            }
        }

        private void UndoBewerking()
        {
            var xselected = CurrentBewerking();
            if (xselected == null) return;
            xselected.Undo();
        }

        private void ShowWerkTekening()
        {
            var bew = CurrentBewerking();
            if (string.IsNullOrEmpty(bew?.ArtikelNr)) return;
            Tools.ShowSelectedTekening(bew.ArtikelNr, TekeningClosed);
        }

        private void TekeningClosed(object sender, EventArgs e)
        {
            var form = Application.OpenForms["MainForm"];
            form?.BringToFront();
            form?.Focus();
            this.Parent?.Select();
            this.Parent?.BringToFront();
            this.Parent?.Focus();
        }

        private void xstatuslabel_SizeChanged(object sender, EventArgs e)
        {
            ResizeStatusLable();
        }

        private async void aantalTeMakenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var dc = new AantalChanger();
            if (dc.ShowDialog(bew.Parent.Aantal, $"Wijzig aantal voor {bew.Naam} van {bew.Omschrijving}.") ==
                DialogResult.OK)
            {
                var change = $"[{bew.ProductieNr}|{bew.ArtikelNr}] Aantal gewijzigd!\n" +
                             $"Van: {bew.Aantal}\n" +
                             $"Naar: {dc.Aantal}";
                bew.Aantal = dc.Aantal;
                await bew.UpdateBewerking(null, change);
            }
        }

        private void leverdatumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowLeverDatumForm();
        }

        private void notitieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowNotitieForm();
        }

        private void xprodafkeurtoolstrip_Click(object sender, EventArgs e)
        {
            ShowAfkeurForm();
        }

        private void xnoteButton_Click(object sender, EventArgs e)
        {
            xnotepanel.Height = xnotepanel.Height > 100 ? 32 : 125;
        }

        private void xverpakking_Click(object sender, EventArgs e)
        {
            ShowVerpakkingForm();
        }

        private void materialenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMaterialenForm();
        }

        public async void ShowLeverDatumForm()
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var dc = new DatumChanger();
            if (dc.ShowDialog(bew.LeverDatum, $"Wijzig leverdatum voor {bew.Naam}.") == DialogResult.OK)
            {
                var change = $"[{bew.ProductieNr}|{bew.ArtikelNr}] Leverdatum gewijzigd!\n" +
                             $"Van: {bew.LeverDatum:dd MMMM yyyy HH:mm} uur\n" +
                             $"Naar: {dc.SelectedValue:dd MMMM yyyy HH:mm} uur";
                bew.LeverDatum = dc.SelectedValue;
                await bew.UpdateBewerking(null, change);
            }
        }

        public async void ShowNotitieForm()
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var xtxtform = new NotitieForms(bew.Note, bew)
            {
                Title = $"Notitie voor [{bew.ProductieNr}, {bew.ArtikelNr}] {bew.Naam}"
            };
            if (xtxtform.ShowDialog() == DialogResult.OK)
            {
                bew.Note = xtxtform.Notitie;
                await bew.UpdateBewerking(null, $"[{bew.ProductieNr}, {bew.ArtikelNr}] {bew.Naam} Notitie Gewijzigd");
            }
        }

        public void ShowAfkeurForm()
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var xafk = new AfkeurForm(bew.Parent);
            xafk.ShowDialog();
        }

        public void ShowMaterialenForm()
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var x = new MateriaalForm();
            x.ShowDialog(bew.Parent);
        }

        public void ShowVerpakkingForm()
        {
            var bew = CurrentBewerking();
            if (bew?.Parent == null) return;
            var x = new VerpakkingInstructieForm(bew);
            x.ShowDialog();
        }

        private void xtoonwerktekening_Click(object sender, EventArgs e)
        {
            ShowWerkTekening();
        }

        private void xOnderbreek_Click(object sender, EventArgs e)
        {
            CurrentBewerking()?.DoOnderbreking(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var bw = CurrentBewerking();
            if (bw == null) return;
            new AantalMonitorForm(bw).ShowDialog();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var bw = CurrentBewerking();
           // productieFormulierPrintenToolStripMenuItem.Enabled = bw?.Parent != null && bw.Parent.ContainsProductiePdf();
        }

        private void productieFormulierPrintenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var prod = CurrentBewerking()?.Parent;
            if (prod != null)
            {
                var xprod = prod.GetProductieFormulierPDF();
                if (!string.IsNullOrEmpty(xprod) && File.Exists(xprod))
                {
                    System.Diagnostics.Process.Start(xprod);
                }
            }
        }
    }
}