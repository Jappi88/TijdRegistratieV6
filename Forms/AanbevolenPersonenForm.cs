﻿using Rpm.Productie;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Forms
{
    public partial class AanbevolenPersonenForm : Forms.MetroBase.MetroBaseForm
    {
        public AanbevolenPersonenForm()
        {
            InitializeComponent();
        }

        private ProductieFormulier _Form = null;
        private Bewerking _Bewerking = null;

        public AanbevolenPersonenForm(Bewerking bew):this()
        {
            _Bewerking = bew;
        }

        public AanbevolenPersonenForm(ProductieFormulier form):this()
        {
            _Form = form;
        }

        public async void InitBewerking(Bewerking bew)
        {
            if (bew == null) return;
            try
            {

                SetWaitUI();
                int aantalpers = 0;
                int aantalwp = 0;
                var pershtml = await bew.GetAanbevolenPersoneelHtml(true, aantalpers);
                var wphtml = await bew.GetAanbevolenWerkplekHtml(true, aantalwp);
                aantalpers = pershtml.Value;
                aantalwp = wphtml.Value;
                int aantal = aantalpers + aantalwp;
                if (aantal == 0)
                    ThrowNoAanbevelingen();
                xpersHtmlPanel.Text = pershtml.Key;
                xwerkplekkenHtmlPanel.Text = wphtml.Key;
                var x1 = aantalpers == 1 ? "persoon" : "personen";
                var x2 = aantalwp == 1 ? "werkplek" : "werkplekken";
                metroTabPage1.Text = $"{aantalwp} Aanbevolen {x2}";
                metroTabPage2.Text = $"{aantalpers} Aanbevolen {x1}";
                UpdateStatus(bew, aantal);
            }
            catch (Exception ex)
            {
                XMessageBox.Show(this, ex.Message, "Geen Aanbevelingen");
                this.Close();
            }
            _iswaiting = false;
            this.Invalidate();
        }

        public async void InitFormulier(ProductieFormulier form)
        {
            if (form?.Bewerkingen == null) return;
            try
            {
                SetWaitUI();
                var pershtml = await form.GetAanbevolenPersoneelHtml();
                var wphtml = await form.GetAanbevolenWerkplekkenHtml();
                int aantal = pershtml.Value + wphtml.Value;
                if (aantal == 0)
                    ThrowNoAanbevelingen();
                xpersHtmlPanel.Text = pershtml.Key;
                xwerkplekkenHtmlPanel.Text = wphtml.Key;
                var x1 = pershtml.Value == 1 ? "persoon" : "personen";
                var x2 = wphtml.Value == 1 ? "werkplek" : "werkplekken";
                metroTabPage1.Text = $"{wphtml.Value} Aanbevolen {x2}";
                metroTabPage2.Text = $"{pershtml.Value} Aanbevolen {x1}";
                UpdateStatus(form, aantal);
            }
            catch (Exception ex)
            {
                XMessageBox.Show(this, ex.Message, "Geen Aanbevelingen");
                this.Close();
            }
            _iswaiting = false;
            this.Invalidate();
        }

        private void UpdateStatus(IProductieBase prod,int count)
        {
            if (prod == null)
                this.Text = "";
            else
            {
                var x1 = count == 0 ? "Geen aanbevelingen" : "Aanbevelingen";
                this.Text = $"{x1} voor {prod.ProductieNr}-{prod.ArtikelNr} {prod.Omschrijving}";
            }

            this.Invalidate();
        }

        private bool _iswaiting = false;
        private void SetWaitUI()
        {
            if (_iswaiting) return;
            _iswaiting = true;
            Task.Run(async () =>
            {

                try
                {
                    bool valid = false;
                    this.Invoke(new MethodInvoker(() => valid = !this.IsDisposed));
                    if (!valid) return;
                    this.Invoke(new MethodInvoker(() => { xloadinglabel.Visible = true; }));
                    var cur = 0;
                    var xwv = "Aanbevelingen Zoeken.";
                    //var xcurvalue = xwv;
                    var tries = 0;
                    while (_iswaiting && tries < 200)
                    {
                        if (!this.Visible) continue;
                        if (cur > 5) cur = 0;
                        var curvalue = xwv.PadRight(xwv.Length + cur, '.');
                        //xcurvalue = curvalue;
                        this.BeginInvoke(new MethodInvoker(() =>
                        {
                            xloadinglabel.Text = curvalue;
                            xloadinglabel.Invalidate();
                        }));
                        Application.DoEvents();

                        await Task.Delay(350);
                        Application.DoEvents();
                        tries++;
                        cur++;
                        if (this.IsDisposed || this.Disposing) break;
                        if (!this.Visible) continue;
                        this.Invoke(new MethodInvoker(() => valid = !this.IsDisposed));
                        if (!valid) break;
                    }

                    if (!this.IsDisposed && !this.Disposing && this.Visible)
                        this.Invoke(new MethodInvoker(() => { xloadinglabel.Visible = false; }));
                }
                catch (Exception e)
                {
                }

            });
        }

        private void ThrowNoAanbevelingen()
        {
            throw new Exception("Er zijn geen aanbevelingen");
        }

        private void AanbevolenPersonenForm_Shown(object sender, EventArgs e)
        {
            if(_Bewerking != null)
                InitBewerking(_Bewerking);
            else if (_Form != null)
                InitFormulier(_Form);
        }
    }
}
