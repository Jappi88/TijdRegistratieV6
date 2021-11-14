﻿using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Forms;
using Rpm.Misc;
using Rpm.Productie;

namespace Forms
{
    public partial class AantalGemaaktUI : MetroForm
    {
        public AantalGemaaktUI()
        {
            InitializeComponent();
        }

        public int Aantal { get; set; }
        public Bewerking Bewerking { get; set; }
        public ProductieFormulier Formulier { get; set; }

        public DialogResult ShowDialog(ProductieFormulier parent, Bewerking bew, WerkPlek plek = null)
        {
            Bewerking = bew;
            Formulier = parent;
            LoadWerkplekken(bew);
            if (xwerkplekken.Items.Count > 0)
            {
                if (plek != null)
                    xwerkplekken.SelectedItem = plek.Naam;
                if (xwerkplekken.SelectedItem == null)
                    xwerkplekken.SelectedIndex = xwerkplekken.Items.Count -1;
            }
            else xaantalgemaakt.SetValue(bew.AantalGemaakt);
            xaantal.Text = bew.AantalTeMaken.ToString();
            xaantalgemaaktlabel.Text = bew.AantalGemaakt.ToString();
            Text = $"[{bew.ArtikelNr}][{bew.ProductieNr}] {bew.Omschrijving}";
            LoadBewerkingText(Bewerking);
            return ShowDialog();
        }

        public DialogResult ShowDialog(ProductieFormulier form)
        {
            Formulier = form;
            ProductieLoadWerkplekken(form);
            if (xwerkplekken.Items.Count > 0)
                xwerkplekken.SelectedIndex = xwerkplekken.Items.Count - 1;
            else xaantalgemaakt.SetValue(form.AantalGemaakt);
            xaantal.Text = form.AantalTeMaken.ToString();
            Text = $"[{form.ArtikelNr}][{form.ProductieNr}] {form.Omschrijving}";
            LoadProductieText(Formulier);
            return ShowDialog();
        }

        private void SetPacketAantal(VerpakkingInstructie instructie, int aantal, int totaalaantal)
        {
            if (instructie == null || instructie.VerpakkenPer == 0)
            {
                xvaluepanel.Height = 40;
            }
            else
            {
                xvaluepanel.Height = 100;
                xpacketvalue.ValueChanged -= Xpacketvalue_ValueChanged;
                int xpackets = aantal <= 0? 0 : (int) Math.Ceiling((double)aantal / instructie.VerpakkenPer);
                xpacketvalue.SetValue(xpackets);
                xpacketlabel.Text = xpacketvalue.Value.ToString(CultureInfo.InvariantCulture);
                xtotalpacketlabel.Text = (totaalaantal <= 0 ? 0 : (int)Math.Ceiling((double)totaalaantal / instructie.VerpakkenPer)).ToString();
                xpacketvalue.ValueChanged += Xpacketvalue_ValueChanged;
                var x0 = "Verpakken Per {0}";
                var x1 = string.IsNullOrEmpty(instructie.VerpakkingType)
                    ? x0
                    : $"{x0} in {instructie.VerpakkingType}";
                xPacketGroup.Text = string.Format(x1, instructie.VerpakkenPer);
            }
        }

        private void Xpacketvalue_ValueChanged(object sender, EventArgs e)
        {
            var xverp = Bewerking?.VerpakkingsInstructies ?? Formulier?.VerpakkingsInstructies;
            if (xverp == null || xverp.VerpakkenPer == 0) return;
            var aantal = xaantalgemaakt.Value;
            int xpackets = aantal <= 0 ? 0 : (int)Math.Ceiling((double)aantal / xverp.VerpakkenPer);
            if (xpacketvalue.Value != xpackets)
            {
                xaantalgemaakt.Value = xpacketvalue.Value * xverp.VerpakkenPer;
                Next(false);
            }
        }

        private void AddPacket()
        {
            var xverp = Bewerking?.VerpakkingsInstructies ?? Formulier?.VerpakkingsInstructies;
            if (xverp == null || xverp.VerpakkenPer == 0) return;

            xaantalgemaakt.SetValue(xaantalgemaakt.Value + xverp.VerpakkenPer);
            Next(false);
        }

        private void RemovePacket()
        {
            var xverp = Bewerking?.VerpakkingsInstructies ?? Formulier?.VerpakkingsInstructies;
            if (xverp == null || xverp.VerpakkenPer == 0) return;

            xaantalgemaakt.SetValue(xaantalgemaakt.Value - xverp.VerpakkenPer);
            Next(false);
        }

        private void LoadProductieText(ProductieFormulier form)
        {
            productieInfoUI1.SetInfo(form, "Wijzig Aantal Gemaakt", Color.AliceBlue, Color.White, Color.Black);
        }

        private void LoadBewerkingText(Bewerking bew)
        {
            productieInfoUI1.SetInfo(bew, "Wijzig Aantal Gemaakt", Color.AliceBlue, Color.White, Color.Black);
        }

        private void LoadWerkplekken(Bewerking bewerking)
        {
            xwerkplekken.Items.Clear();
            if (bewerking != null && bewerking.WerkPlekken.Count > 0)
            {
                foreach (var w in bewerking.WerkPlekken)
                    if (w.Werk != null && bewerking.Equals(w.Werk))
                        xwerkplekken.Items.Add(w.Naam);
            }
            else
            {
                if (bewerking != null) xwerkplekken.Items.Add(bewerking.Naam);
            }

            if (xwerkplekken.Items.Count > 1)
                xwerkplekken.Items.Add("Alle Werkplekken");
        }

        private void ProductieLoadWerkplekken(ProductieFormulier formulier)
        {
            xwerkplekken.Items.Clear();
            if (formulier?.Bewerkingen != null)
            {
                var bws = formulier.Bewerkingen.Where(x => x.IsAllowed()).ToArray();
                if (bws.Length > 0)
                    xwerkplekken.Items.AddRange(bws.Select(x => (object) x.Naam).ToArray());
                else xwerkplekken.Items.Add($"'{formulier.ArtikelNr} | {formulier.ProductieNr}'");
            }
        }

        private string ProductieText(ProductieFormulier form)
        {
            var value = $"Controlleer '{form.Omschrijving}' op kwaliteit en aantal.\n\n";
            var tijd = form.TijdAanGewerkt();
            var peruur = tijd > 0 && form.TotaalGemaakt > 0 ? Math.Round(form.TotaalGemaakt / tijd, 0) : 0;
            value += $"Aantal Gemaakt: {form.AantalGemaakt} van de {form.AantalTeMaken}\n" +
                     $"Actueel Per Uur: {peruur} i.p.v. {form.PerUur}";//"\n\n" + DefaultFieldText();
            return value;
        }

        private string ProductiewWerkPlekken(ProductieFormulier form)
        {
            var value = "";
            if (form is {Bewerkingen: {Length: > 0}})
                foreach (var bew in form.Bewerkingen)
                {
                    value += $"[{bew.Naam}]\n";
                    value += BewerkingPersoneelText(bew);
                    value += "\n";
                }

            return value;
        }

        private string BewerkingText(Bewerking bew)
        {
            var value = $"Controlleer {bew.Omschrijving} ({bew.Naam}) op kwaliteit en aantal.\n\n";
            var tijd = bew.TijdAanGewerkt();
            var peruur = tijd > 0 && bew.TotaalGemaakt > 0 ? Math.Round(bew.TotaalGemaakt / tijd, 0) : 0;
            value += $"Aantal Gemaakt: {bew.AantalGemaakt} van de {bew.AantalTeMaken}\n" +
                     $"Actueel Per Uur: {peruur} i.p.v. {bew.PerUur}";//"\n\n" + DefaultFieldText();
            return value;
        }

        private string BewerkingPersoneelText(Bewerking bew)
        {
            var werkplek = "";
            if (bew.WerkPlekken is {Count: > 0})
            {
                string.Join(", ",
                    bew.WerkPlekken.SelectMany(x =>
                        x.Personen.Select(x => x.PersoneelNaam + (x.Actief ? "" : "[Niet Actief]"))));
                foreach (var werk in bew.WerkPlekken)
                {
                    var personen = string.Join(", ",
                        werk.Personen.Select(x => x.PersoneelNaam + (x.Actief ? "" : "[Niet Actief]")));

                    var xvalue = werk.Personen.Count == 1 ? "heeft" : "hebben";
                    if (personen.Length > 0)
                        werkplek +=
                            $"{personen} {xvalue} op  '{werk.Naam}' {werk.AantalGemaakt} gemaakt in {werk.TijdAanGewerkt()} uur\n";
                }

                werkplek = werkplek.TrimStart(',', ' ');
            }

            return werkplek;
        }

        private string DefaultFieldText()
        {
            return "LET OP!!\n" +
                   "* Contolleer het product goed volgens de tekening.\n" +
                   "* Maak steekproeven op producten die al zijn verpakt.\n" +
                   "* Controlleer of de juiste stikkers en verpakking wordt gebruikt.\n" +
                   "* Controlleer of de aantallen kloppen.\n\n" +
                   "Vul het aantal in die je hebt gecontrolleerd, en druk op 'OK'\n";
        }

        private void xopslaan_Click(object sender, EventArgs e)
        {
            var xok = true;
            if (Bewerking != null && xaantalgemaakt.Value == Bewerking.AantalGemaakt)
            {
                xok = false;
                XMessageBox.Show(
                    "Aantal dat je invult is hetzelfde als de oude aantal... en is daarom niet nodig.\nAnnuleer of vul in een aantal dat groter is dan de oude.",
                    "Onzin", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (Bewerking != null && Bewerking.AantalGemaakt > xaantalgemaakt.Value)
            {
                xok = XMessageBox.Show("Aantal dat je invult is kleiner dan de oude aantal...\n" +
                                       "Kan zijn dat je je vergist had en daarom wil corrigeren, \nWeetje zeker dat je door wilt gaan?",
                    "Aantal Gemaakt", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }
        }

        private void xannuleer_Click(object sender, EventArgs e)
        {
            Next(false);
            DialogResult = DialogResult.OK;
        }

        private void AantalUI_Shown(object sender, EventArgs e)
        {
            Manager.OnFormulierChanged += Manager_OnFormulierChanged;
            xaantalgemaakt.Select();
            xaantalgemaakt.Focus();
            xaantalgemaakt.Select(0, xaantalgemaakt.Value.ToString().Length);
        }

        private void Manager_OnFormulierChanged(object sender, ProductieFormulier changedform)
        {
            var prodnr = Bewerking?.ProductieNr ?? Formulier?.ProductieNr;
            if (changedform == null || !string.Equals(changedform.ProductieNr, prodnr)) return;
            if (Bewerking != null)
            {
                var xbew = changedform.Bewerkingen.FirstOrDefault(x => x.Equals(Bewerking));
                if (xbew != null)
                    Bewerking = xbew;
            }
            else
            {
                Formulier = changedform;
            }
        }

        private void Aantal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter) Next(false);
        }

        private void xwerkplekken_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (xwerkplekken.SelectedItem != null)
            {
                var selected = xwerkplekken.SelectedItem.ToString();
                var alles = selected.ToLower() == "alle werkplekken";
                if (Bewerking != null)
                {
                    if (!alles && Bewerking.WerkPlekken is {Count: > 0})
                    {
                        var wp = Bewerking.WerkPlekken.FirstOrDefault(t => string.Equals(t.Naam, selected, StringComparison.CurrentCultureIgnoreCase));
                        if (wp != null)
                        {
                            xaantalgemaakt.SetValue(wp.AantalGemaakt);
                            SetPacketAantal(Bewerking.VerpakkingsInstructies, wp.AantalGemaakt, Bewerking.Aantal);
                        }
                    }
                    else
                    {
                        xaantalgemaakt.SetValue(Bewerking.AantalGemaakt);
                        SetPacketAantal(Bewerking.VerpakkingsInstructies, Bewerking.AantalGemaakt, Bewerking.Aantal);
                    }

                    xaantalgemaaktlabel.Text = Bewerking.AantalGemaakt.ToString();
                }
                else if (Formulier != null)
                {
                    if (!alles && Formulier.Bewerkingen is {Length: > 0})
                    {
                        var b = Bewerking ?? Formulier.Bewerkingen.FirstOrDefault(t =>
                            string.Equals(t.Naam, selected, StringComparison.CurrentCultureIgnoreCase));

                        if (b != null)
                        {
                            xaantalgemaakt.SetValue(b.AantalGemaakt);
                            xaantalgemaaktlabel.Text = b.AantalGemaakt.ToString();
                            SetPacketAantal(b.VerpakkingsInstructies, b.AantalGemaakt, b.Aantal);
                        }
                    }
                    else
                    {
                        xaantalgemaakt.SetValue(Formulier.AantalGemaakt);
                        xaantalgemaaktlabel.Text = Formulier.AantalGemaakt.ToString();
                        SetPacketAantal(Formulier.VerpakkingsInstructies, Formulier.AantalGemaakt, Formulier.Aantal);
                    }
                   
                }

                xaantalgemaakt.Select(0, xaantalgemaakt.Value.ToString().Length);
                xaantalgemaakt.Focus();
            }
        }

        private async void Next(bool movenext)
        {
            if (xwerkplekken.SelectedItem != null)
            {
                var selected = xwerkplekken.SelectedItem.ToString();
                var alles = selected.ToLower() == "alle werkplekken";
                var changed = false;
                if (Bewerking != null)
                {
                    var skip = false;
                    var change = $"Aantal gemaakt aangepast naar {xaantalgemaakt.Value}";
                    if (alles && Bewerking.AantalGemaakt != (int) xaantalgemaakt.Value)
                    {
                        Bewerking.AantalGemaakt = (int) xaantalgemaakt.Value;
                        SetPacketAantal(Bewerking.VerpakkingsInstructies, Bewerking.AantalGemaakt, Bewerking.Aantal);
                        changed = true;
                    }

                    if (!changed && Bewerking.WerkPlekken is {Count: > 0})
                    {
                        var wp = Bewerking.WerkPlekken.FirstOrDefault(t => string.Equals(t.Naam, selected, StringComparison.CurrentCultureIgnoreCase));
                        if (wp != null)
                            if (wp.AantalGemaakt != (int) xaantalgemaakt.Value)
                            {
                                change =
                                    $"[{wp.Path}] Aantal aangepast van {wp.AantalGemaakt} naar {xaantalgemaakt.Value}";
                                wp.AantalGemaakt = (int) xaantalgemaakt.Value;
                                wp.LaatstAantalUpdate = DateTime.Now;
                                SetPacketAantal(Bewerking.VerpakkingsInstructies, wp.AantalGemaakt, Bewerking.Aantal);
                                changed = true;
                            }
                            else
                            {
                                skip = true;
                            }
                    }


                    if (!changed && !skip && Bewerking.AantalGemaakt != xaantalgemaakt.Value)
                    {
                        change =
                            $"[{Bewerking.Path}] Aantal aangepast van {Bewerking.AantalGemaakt} naar {xaantalgemaakt.Value}";
                        Bewerking.AantalGemaakt = (int) xaantalgemaakt.Value;
                        Bewerking.LaatstAantalUpdate = DateTime.Now;
                        SetPacketAantal(Bewerking.VerpakkingsInstructies, Bewerking.AantalGemaakt, Bewerking.Aantal);
                        changed = true;
                    }

                    xaantalgemaaktlabel.Text = Bewerking.AantalGemaakt.ToString();
                    if (changed)
                        await Bewerking.UpdateBewerking(null, change);
                    LoadBewerkingText(Bewerking);
                }
                else if (Formulier != null)
                {
                    if (alles)
                    {
                        if (Formulier.AantalGemaakt != (int) xaantalgemaakt.Value)
                        {
                            var change =
                                $"[{Formulier.ProductieNr}] Aantal aangepast van {Formulier.AantalGemaakt} naar {xaantalgemaakt.Value}";
                            Formulier.AantalGemaakt = (int) xaantalgemaakt.Value;
                            Formulier.LaatstAantalUpdate = DateTime.Now;
                            xaantalgemaaktlabel.Text = Formulier.AantalGemaakt.ToString();
                            LoadProductieText(Formulier);
                            SetPacketAantal(Formulier.VerpakkingsInstructies, Formulier.AantalGemaakt, Formulier.Aantal);
                            await Formulier.UpdateForm(false, false, null, change);
                        }
                    }
                    else if (Formulier.Bewerkingen is {Length: > 0})
                    {
                        var b = Bewerking ??
                                Formulier.Bewerkingen.FirstOrDefault(t => string.Equals(t.Naam, selected, StringComparison.CurrentCultureIgnoreCase));

                        if (b != null && b.AantalGemaakt != (int) xaantalgemaakt.Value)
                        {
                            var change =
                                $"[{b.Path}] Aantal aangepast van {b.AantalGemaakt} naar {xaantalgemaakt.Value}";
                            b.AantalGemaakt = (int) xaantalgemaakt.Value;
                            b.LaatstAantalUpdate = DateTime.Now;
                            LoadProductieText(Formulier);
                            SetPacketAantal(b.VerpakkingsInstructies, b.AantalGemaakt, b.Aantal);
                            await b.UpdateBewerking(null, change);

                            xaantalgemaaktlabel.Text = b.AantalGemaakt.ToString();
                        }
                    }
                }

                if (xwerkplekken.Items.Count > 0 && movenext)
                {
                    if (xwerkplekken.SelectedIndex + 1 < xwerkplekken.Items.Count)
                        xwerkplekken.SelectedIndex++;
                    else xwerkplekken.SelectedIndex = 0;
                }

                xaantalgemaakt.Select(0, xaantalgemaakt.Value.ToString().Length);
                xaantalgemaakt.Focus();
            }
        }

        private void xnextb_Click(object sender, EventArgs e)
        {
            Next(false);
        }

        private void AantalGemaaktUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Manager.OnFormulierChanged -= Manager_OnFormulierChanged;
        }

        private void xremovePacket_Click(object sender, EventArgs e)
        {
            RemovePacket();
        }

        private void xaddPacket_Click(object sender, EventArgs e)
        {
            AddPacket();
        }
    }
}