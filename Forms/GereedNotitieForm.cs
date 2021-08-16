﻿using BrightIdeasSoftware;
using ProductieManager.Properties;
using Rpm.Misc;
using Rpm.Productie;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Forms
{
    public partial class GereedNotitieForm : MetroFramework.Forms.MetroForm
    {
        private IProductieBase _Productie;
        public GereedNotitieForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(IProductieBase productie)
        {
            _Productie = productie;
            this.Text = $"Productie Aflsuiten: {productie.ProductieNr} | {productie.ArtikelNr}";
            this.Invalidate();
            CreateMessage(productie);
            return base.ShowDialog();
        }

        public string Reden { get; private set; }

        private void CreateMessage(IProductieBase productie)
        {
            if (productie == null) return;
            int xaantal = productie.Aantal - productie.TotaalGemaakt;
            xfieldmessage.Text = string.Format(xfieldmessage.Text, xaantal, $"'{productie.Naam}'");
        }

        private void xmateriaalopcheck_CheckedChanged(object sender, System.EventArgs e)
        {
            if (_Productie == null) return;
            if (xmateriaalopcheck.Checked)
            {
                var mats = _Productie.GetMaterialen();
                if (mats.Count > 1)
                {
                    this.MaximumSize = new System.Drawing.Size(this.MaximumSize.Width, 500);
                    xmaterialen.Visible = true;
                    this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, 350);
                    this.Size = this.MinimumSize;
                }
                else
                {
                    this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, 250);
                    this.MaximumSize = this.MinimumSize;
                    this.Size = this.MinimumSize;
                    xmaterialen.Visible = false;
                }
                xmaterialen.Items.Clear();
               foreach(var mat in mats)
                {
                    var lv = new ListViewItem(mat.ArtikelNr);
                    lv.SubItems.Add(mat.Omschrijving);
                    lv.Tag = mat;
                    lv.ImageIndex = 0;
                    xmaterialen.Items.Add(lv);
                }
                if (xmaterialen.Items.Count == 1)
                    xmaterialen.Items[0].Checked = true;
                xmaterialen.Select();
            }
            else
            {
                xmaterialen.Visible = false;
                this.MaximumSize = new System.Drawing.Size(this.MaximumSize.Width, 250);
                this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, 250);
                this.Height = 250;
            }
        }

        private void xvollepalletcheck_CheckedChanged(object sender, System.EventArgs e)
        {
            if (xvollepalletcheck.Checked)
            {
                this.MaximumSize = new System.Drawing.Size(this.MaximumSize.Width, 250);
                this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, 250);
                this.Height = 250;
                xredentextbox.Visible = false;
                xmaterialen.Visible = false;
            }
        }

        private void xanderscheck_CheckedChanged(object sender, System.EventArgs e)
        {
            if (xanderscheck.Checked)
            {
                this.MaximumSize = new System.Drawing.Size(this.MaximumSize.Width, 500);
                this.MinimumSize = new System.Drawing.Size(this.MinimumSize.Width, 350);
                this.Height = 350;
                xredentextbox.Visible = true;
                xmaterialen.Visible = false;
                xredentextbox.Select();
            }
            else
            {
                xredentextbox.Visible = false;
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (xmateriaalopcheck.Checked)
                {
                    if (_Productie == null)
                        throw new Exception("Ongeldige productie!");
                    List<string> xselected = new List<string>();

                    foreach (var item in xmaterialen.Items)
                    {
                        if (item is ListViewItem olv)
                            if (olv.Checked)
                            {
                                var mat = olv.Tag as Materiaal;
                                if (mat == null) continue;
                                xselected.Add($"[({mat.ArtikelNr}){mat.Omschrijving}]");
                            }
                    }
                    if (xmaterialen.Items.Count > 0 && xselected.Count == 0)
                    {
                        XMessageBox.Show("Selecteer de materialen die op zijn a.u.b", "Selecteer Materialen", MessageBoxIcon.Warning);
                        return;
                    }
                    Reden = $"Materialen zijn op: {string.Join(", ", xselected)}";
                }
                else if(xvollepalletcheck.Checked)
                {
                    Reden = "Geëindigd op volle pallet/bak";
                }
                else if(xanderscheck.Checked)
                {
                    if(string.IsNullOrEmpty(xredentextbox.Text.Trim()) || xredentextbox.Text.Length < 8)
                    {
                        XMessageBox.Show("Vul in een geldige reden waarom je de productie eerder wilt afsluiten.", "Vul in een geldige reden", MessageBoxIcon.Warning);
                        return;
                    }
                    Reden = xredentextbox.Text.Trim();
                }
                else
                {
                    XMessageBox.Show("Kies een reden waarom je de productie eerder wilt afsluiten.", "Kies een reden", MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                XMessageBox.Show(ex.Message, "Fout", MessageBoxIcon.Error);
            }

        }

        private void xmaterialen_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

        }
    }
}
