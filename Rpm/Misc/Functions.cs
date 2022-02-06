using Forms;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Polenter.Serialization;
using Rpm.Mailing;
using Rpm.Productie;
using Rpm.Settings;
using Rpm.SqlLite;
using Rpm.Various;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rpm.Misc
{
    public static class Functions
    {
        #region Enums

        public enum DateTimeConvertType
        {
            FullDate,
            OnlyDate,
            OnlyTime
        }

        #endregion

        #region Productie Formulieren

        public static bool IsAllowed(this ProductieFormulier form, string filter, ViewState[] states, bool incform)
        {
            if (states == null)
                return false;
            if (form.Bewerkingen == null || form.Bewerkingen.Length == 0)
                return false;
            if (string.IsNullOrEmpty(form.ProductieNr))
                return false;
            var xreturn = form.Bewerkingen.Any(x => x.IsAllowed(filter) && states.Any(x.IsValidState));
            if (incform)
                xreturn &= form.IsValidState(states);
            return xreturn;
        }

        public static bool IsAllowed(object value, string filter)
        {
            if (value is ProductieFormulier prod)
                return prod.IsAllowed(filter);
            if (value is Bewerking bew)
                return bew.IsAllowed(filter);
            return false;
        }

        public static bool IsAllowed(this ProductieFormulier form, string filter)
        {
            if (form == null || string.IsNullOrEmpty(form.ProductieNr))
                return false;
            if (!string.IsNullOrEmpty(filter) && !form.ContainsFilter(filter)) return false;
            //if (Manager.Opties?.ToegelatenProductieCrit != null && Manager.Opties.ToegelatenProductieCrit.Count > 0)
            //{
            //    foreach (var value in Manager.Opties.ToegelatenProductieCrit)
            //    {
            //        var xvalues = value.Split(':');
            //        var xcrits = xvalues[0].Split(';');
            //        bool valid = true;
            //        foreach (var c in xcrits)
            //            valid &= form.ContainsFilter(c);
            //        bool allow = false;
            //        if (xvalues.Length > 1)
            //            allow = xvalues[1].ToLower() == "toelaten";
            //        if (valid)
            //            return allow;
            //    }
            //}

            //if (Manager.Opties == null || Manager.Opties.ToonAlles)
            //    return true;
            return form.Bewerkingen.Length == 0 || form.Bewerkingen.Any(x => x.IsAllowed(filter));
        }

        public static Dictionary<string, List<object>> ToSections(this List<Bewerking> bewerkingen, bool iswerkplek,
            TijdEntry periode)
        {
            var xreturn = new Dictionary<string, List<object>>();
            try
            {
                foreach (var bew in bewerkingen)
                {

                    if (!bew.IsAllowed(null)) continue;
                    if (iswerkplek)
                    {
                        if (bew.WerkPlekken == null || bew.WerkPlekken.Count == 0) continue;
                        foreach (var wp in bew.WerkPlekken)
                        {
                            if (wp.TijdAanGewerkt(periode.Start, periode.Stop,true) <= 0) continue;
                            if (!xreturn.ContainsKey(wp.Naam))
                                xreturn.Add(wp.Naam, new List<object> {wp});
                            else xreturn[wp.Naam].Add(wp);
                        }
                    }
                    else
                    {
                        if (bew.TijdAanGewerkt(periode.Start, periode.Stop) <= 0) continue;
                        if (!xreturn.ContainsKey(bew.Naam))
                            xreturn.Add(bew.Naam, new List<object> {bew});
                        else xreturn[bew.Naam].Add(bew);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return xreturn;
        }

        public static void ExcludeFromUpdate(this ProductieFormulier productie)
        {
           // Manager.ProductieProvider?.AddToExclude(productie);
        }

        public static void RemoveExcludeFromUpdate(this ProductieFormulier productie)
        {
            //Manager.ProductieProvider?.RemoveFromExclude(productie);
        }

        public static Dictionary<string, Dictionary<string, double>> CreateChartData(
            this List<Bewerking> bewerkingen, bool iswerkplek, int startweek, int startjaar, string field,
            bool includethisweek, bool shownow)
        {
            var xreturn = new Dictionary<string, Dictionary<string, double>>();
            try
            {
                var weekrange = GetWeekRange(startweek, startjaar,null);
                var data = bewerkingen.ToSections(iswerkplek, new TijdEntry(weekrange.StartDate, weekrange.EndDate, null));
                var weekranges = GetWeekRanges(weekrange.StartDate, weekrange.EndDate, includethisweek, shownow);
                var rows =(weekranges.Count > 1
                    ? weekranges.Select(x => x.Name)
                    : new[] {"Maandag", "Dinsdag", "Woensdag", "Donderdag", "Vrijdag","Zaterdag","Zondag"}).ToArray();
                for (var xrowindex = 0; xrowindex < rows.Length; xrowindex++)
                {
                    //init values
                    WeekRange range = null;
                    var xname = rows[xrowindex];
                    if (weekranges.Count > 1)
                    {
                        range = weekranges[xrowindex];
                    }
                    else
                    {
                        //pak alle waarden doormiddel van een dag bereik
                        range = weekranges[0];
                        var xstart = range.StartDate.AddDays(xrowindex);
                        var xstop = new DateTime(xstart.Year, xstart.Month, xstart.Day).Add(Manager.Opties.GetWerkRooster()
                            .EindWerkdag);
                        range = new WeekRange {StartDate = xstart, EndDate = xstop};
                    }

                    // if (range?.StartDate >= DateTime.Now) break;
                    xreturn.Add(rows[xrowindex], new Dictionary<string, double>());
                    //var tabledata = new Dictionary<string, double>();
                    double val = 0;
                    foreach (var prod in data)
                    {
                        var value = GetSumValueRange(prod.Value, new TijdEntry(range.StartDate, range.EndDate, null), field,
                            iswerkplek);
                        if (value <= 0) continue;
                        // value = Math.Round(value, 0, MidpointRounding.AwayFromZero);
                        //tabledata.Add(prod.Key, value);
                        val += value;
                        if (xreturn.ContainsKey(rows[xrowindex]) && !string.IsNullOrEmpty(prod.Key))
                        {
                            if (xreturn[rows[xrowindex]].ContainsKey(prod.Key))
                            {
                                xreturn[rows[xrowindex]][prod.Key] +=  value;
                            }
                            else xreturn[rows[xrowindex]].Add(prod.Key, value);
                        }
                        
                    }

                    if (val <= 0)
                        xreturn.Remove(rows[xrowindex]);
                }
            }
            catch
            {
            }

            return xreturn;
        }

        public static double GetSumValueRange(this List<object> producties, TijdEntry bereik, string type,
            bool iswerkplek)
        {
            if (producties.Count == 0) return 0;
            switch (type.ToLower())
            {
                case "tijd gewerkt":
                    if (iswerkplek)
                        return Math.Round(producties.Sum(x => ((WerkPlek) x).TijdAanGewerkt(bereik.Start, bereik.Stop,true)),
                            2);

                    return Math.Round(producties.Sum(x => ((Bewerking) x).TijdAanGewerkt(bereik.Start, bereik.Stop)),
                        2);
                case "aantal gemaakt":
                    double done = 0;
                    if (iswerkplek)
                        foreach (var xprod in producties)
                        {
                            var prod = (WerkPlek) xprod;
                            if (prod == null || prod.TotaalGemaakt == 0) continue;
                            var tg = prod.TijdAanGewerkt(bereik.Start, bereik.Stop,true);
                            var totaltg = prod.TijdAanGewerkt();
                            if (tg <= 0) continue;
                            var percentage = tg / totaltg * 100;
                            var aantal = prod.TotaalGemaakt / 100 * percentage;
                            done += aantal;
                        }
                    else
                        foreach (var xprod in producties)
                        {
                            var prod = (Bewerking) xprod;
                            if (prod == null || prod.TotaalGemaakt == 0) continue;
                            var tg = prod.TijdAanGewerkt(bereik.Start, bereik.Stop);
                            var totaltg = prod.TijdAanGewerkt();
                            if (tg <= 0) continue;
                            var percentage = tg / totaltg * 100;
                            var aantal = prod.TotaalGemaakt / 100 * percentage;
                            done += aantal;
                        }

                    return Math.Round(done, 0);
                case "per uur":
                    var gewerkt = GetSumValueRange(producties, bereik, "tijd gewerkt", iswerkplek);
                    var gemaakt = GetSumValueRange(producties, bereik, "aantal gemaakt", iswerkplek);
                    if (gewerkt <= 0 || gemaakt <= 0) return 0;
                    return Math.Round(gemaakt / gewerkt, 0);
                case "storingen":
                    if (iswerkplek)
                        return Math.Round(
                            producties.Sum(x =>
                                ((WerkPlek) x).Storingen.Sum(t => t.GetTotaleTijd(bereik.Start, bereik.Stop))), 2);
                    return Math.Round(
                        producties.Sum(x =>
                            ((Bewerking) x).GetAlleStoringen(false).Sum(t => t.GetTotaleTijd(bereik.Start, bereik.Stop))),
                        2);
            }

            return 0;
        }

        public static bool IsValidState(this IProductieBase form, ViewState vstate)
        {
            if (form == null)
                return false;
            if (form.State == ProductieState.Gereed && (vstate == ViewState.Gereed || vstate == ViewState.Alles))
                return true;
            switch (vstate)
            {
                case ViewState.Gestopt:
                    return form.State == ProductieState.Gestopt;

                case ViewState.Gestart:
                    return form.State == ProductieState.Gestart;

                case ViewState.Gereed:
                    return form.State == ProductieState.Gereed;

                case ViewState.Verwijderd:
                    return form.State == ProductieState.Verwijderd;

                case ViewState.Telaat:
                    return form.TeLaat &&
                           (form.State == ProductieState.Gestopt || form.State == ProductieState.Gestart);

                case ViewState.Nieuw:
                    return form.IsNieuw;

                case ViewState.Alles:
                    return true;

                case ViewState.None:
                    return false;
            }

            return true;
        }

        public static bool IsValidState(this IProductieBase form, ViewState[] vstate)
        {
            if (form == null)
                return false;
            foreach (var state in vstate)
                if (form.IsValidState(state))
                    return true;
            return false;
        }

        //public static ProductieFormulier ReadProductie(this Stream input, Manager parent)
        //{
        //    int length = input.ReadInt32();
        //    byte[] data = new byte[length];
        //    input.Read(data, 0, length);
        //    return ProductieFormulier.DeSerializeFromData(data, parent);
        //}

        public static bool ContainsFilter(this IProductieBase werk, string filter)
        {
            if (werk == null)
                return false;
            filter = filter?.Replace(" ", "");
            if (!string.IsNullOrEmpty(filter))
            {
                if (werk.Naam != null && werk.Naam.Replace(" ", "").ToLower().Contains(filter.ToLower()))
                    return true;
                if (werk.Omschrijving != null && werk.Omschrijving.Replace(" ", "").ToLower().Contains(filter.ToLower()))
                    return true;
                if (werk.ProductieNr != null && werk.ProductieNr.Replace(" ", "").ToLower().Contains(filter.ToLower()))
                    return true;
                if (werk.ArtikelNr != null && werk.ArtikelNr.Replace(" ", "").ToLower().Contains(filter.ToLower()))
                    return true;
                return false;
            }

            return true;
        }

        #endregion Productie Formulieren

        #region Bewerkingen

        public static bool IsAllowed(this Bewerking bew)
        {
            return IsAllowed(bew, null);
        }

        public static bool IsAllowed(this Bewerking bew, string filter, ViewState state)
        {
            if (bew == null) return false;
            return IsAllowed(bew, filter) && bew.IsValidState(state);
        }

        public static bool IsAllowed(this Bewerking bew, string filter)
        {
            if (Manager.Opties == null || bew == null || string.IsNullOrEmpty(bew.ProductieNr))
                return false;
            var xreturn = false;
            if (!string.IsNullOrEmpty(filter) && !bew.ContainsFilter(filter)) return false;
            foreach (var value in Manager.Opties.ToegelatenProductieCrit)
            {
                var xvalues = value.Split(':');
                var xcrits = xvalues[0].Split(';');
                var valid = true;
                foreach (var c in xcrits)
                    valid &= bew.ContainsFilter(c);
                bool allow = false;
                if (xvalues.Length > 1)
                    allow = xvalues[1].ToLower() == "toelaten";
                if (valid)
                    return allow;
            }
            //if (valid) return xreturn;
            if (Manager.Opties.ToonAlles)
                return true;

            if (Manager.Opties.ToonVolgensBewerkingen &&
                (Manager.Opties.Bewerkingen == null || Manager.Opties.Bewerkingen.Length == 0))
                return false;
            if (Manager.Opties.ToonVolgensAfdelingen &&
                (Manager.Opties.Afdelingen == null || Manager.Opties.Afdelingen.Length == 0))
                return false;
            if (Manager.Opties.ToonAllesVanBeide &&
                (Manager.Opties.Afdelingen == null || Manager.Opties.Afdelingen.Length == 0) &&
                (Manager.Opties.Bewerkingen == null || Manager.Opties.Bewerkingen.Length == 0))
                return false;

            if (Manager.Opties.ToonAllesVanBeide || Manager.Opties.ToonVolgensBewerkingen)
                xreturn |= Manager.Opties.Bewerkingen.Any(x => bew.Naam.ToLower().Split('[')[0] == x.ToLower());

            if (!xreturn && (Manager.Opties.ToonAllesVanBeide || Manager.Opties.ToonVolgensAfdelingen))
            {
                var xs = Manager.BewerkingenLijst.GetEntry(bew.Naam.Split('[')[0]);
                if (xs != null && xs.WerkPlekken.Count > 0 && Manager.Opties.Afdelingen != null)
                {
                    var any = Manager.Opties.Afdelingen.Any(x =>
                        xs.WerkPlekken.Any(s => string.Equals(s, x, StringComparison.CurrentCultureIgnoreCase)));

                    xreturn |= any;
                }
            }

            return xreturn;
        }

        //public static bool IsAllowed(this Bewerking bewerking, ViewState[] states)
        //{
        //    if (bewerking == null || states == null || states.Length == 0) return false;
        //    return bewerking.IsAllowed() && states.Any(x => x == ViewState.Alles || IsValidState(bewerking, x));
        //}

        //public static bool IsValidState(this Bewerking bew, ViewState vstate)
        //{
        //    if (bew == null)
        //        return false;
        //    if (vstate == ViewState.Alles) return true;
        //    if (bew.State == ProductieState.Gereed && vstate != ViewState.Gereed)
        //        return false;
        //    switch (vstate)
        //    {
        //        case ViewState.Gestopt:
        //            return bew.State == ProductieState.Gestopt;

        //        case ViewState.Gestart:
        //            return bew.State == ProductieState.Gestart;

        //        case ViewState.Gereed:
        //            return bew.State == ProductieState.Gereed;

        //        case ViewState.Verwijderd:
        //            return bew.State == ProductieState.Verwijderd;

        //        case ViewState.Telaat:
        //            return bew.TeLaat && (bew.State == ProductieState.Gestopt || bew.State == ProductieState.Gestart);

        //        case ViewState.Nieuw:
        //            return bew.IsNieuw;

        //        case ViewState.Alles:
        //            return true;

        //        case ViewState.None:
        //            return false;
        //    }

        //    return true;
        //}

        public static string[] GetBewerkingen(this string value, Dictionary<string[], string[]> bewerkingen)
        {
            if (bewerkingen == null || value == null)
                return new string[] { };
            var values = value.Split(' ');
            var xreturn = new List<string>();
            var bew = "";
            foreach (var x in values)
            {
                bew += x;
                var xbew = bewerkingen.FirstOrDefault(t =>
                    t.Value.FirstOrDefault(s => string.Equals(s, bew, StringComparison.CurrentCultureIgnoreCase)) !=
                    null);
                if (xbew.Value != null && xbew.Key != null)
                {
                    xreturn.Add(bew);
                    bew = "";
                }
                else
                {
                    bew += " ";
                }
            }

            return xreturn.ToArray();
        }

        public static string[] BewerkingenByKey(this Dictionary<string[], string[]> bewerkingen, string keyvalue)
        {
            var xreturn = new List<string>();
            foreach (var b in bewerkingen)
            {
                var result = b.Key.FirstOrDefault(t => t.ToLower() == keyvalue.ToLower());
                if (result != null) xreturn.AddRange(b.Value);
            }

            return xreturn.ToArray();
        }

        public static string[] KeysByBewerking(this Dictionary<string[], string[]> bewerkingen, string keyvalue)
        {
            var xreturn = new List<string>();
            foreach (var b in bewerkingen)
            {
                var result = b.Value.FirstOrDefault(t => t.ToLower() == keyvalue.ToLower());
                if (result != null) xreturn.AddRange(b.Key);
            }

            return xreturn.ToArray();
        }

        public static string[] KeyvaluesByBewerking(this Dictionary<string[], string[]> bewerkingen, string bewerking)
        {
            var xreturn = new List<string>();
            foreach (var b in bewerkingen)
            {
                var result = b.Value.FirstOrDefault(t =>
                    string.Equals(t, bewerking, StringComparison.CurrentCultureIgnoreCase));
                if (result != null) xreturn.AddRange(b.Key);
            }

            return xreturn.ToArray();
        }

        public static List<BewerkingEntry> LoadBewerkingLijst(string txt)
        {
            var xreturn = new List<BewerkingEntry>();
            try
            {
                if (!File.Exists(txt))
                {
                    var value = ReadResourceData(txt);
                    if (value != null)
                        File.WriteAllBytes(txt, value);
                    else return xreturn;
                }

                using var sr = new StreamReader(txt);
                {
                    var isread = false;
                    var line = "";
                    while (sr.Peek() > -1)
                    {
                        line = isread ? line : sr.ReadLine();
                        isread = false;
                        if (line == null || line.Length < 4)
                            continue;
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            //de afdelingen waar de bewerkingen worden uitgevoerd.
                            var keys = line.Trim('[', ']').Split(','); //afdelingen
                            var values = new List<string>(); //bewerkingen
                            while (sr.Peek() > -1)
                            {
                                line = sr.ReadLine();
                                if (line == null || line.Length < 4)
                                    continue;
                                isread = true;
                                if (line.StartsWith("[") && line.EndsWith("]"))
                                    break;
                                var split = line.Split(':');
                                var isbemand = split.Length <= 1 || split[1] != "0";
                                xreturn.Add(new BewerkingEntry(split[0], isbemand, keys.ToList()));
                            }
                        }
                    }

                    sr.Close();
                }
            }
            catch
            {
            }

            return xreturn;
        }

        public static bool SaveStoringen(List<string> storingen, string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && storingen is {Count : > 0})
                {
                    File.WriteAllBytes(path, storingen.Serialize());
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static string[] LoadSoortStoringen(string txt)
        {
            var xreturn = new List<string>();
            try
            {
                try
                {
                    xreturn = File.ReadAllBytes(txt).DeSerialize<List<string>>();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    xreturn = null;
                }

                if (xreturn == null)
                {
                    var value = ReadResourceString(txt);
                    if (value != null)
                    {
                        xreturn = value.Replace("\r\n", "").Split(',').ToList();
                        File.WriteAllBytes(txt, xreturn.Serialize());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return xreturn?.ToArray() ?? Array.Empty<string>();
        }

        public static async void DoBewerkingEigenRooster(this Bewerking bew, IWin32Window owner)
        {
            try
            {
                var b = bew;
                if (b != null)
                {
                    WerkPlek wp = null;
                    if (bew.WerkPlekken is {Count: > 1})
                    {
                        var wpchooser = new WerkPlekChooser(bew.WerkPlekken,null)
                        {
                            Title = "Kies een werkplek om een rooster van te wijzigen"
                        };
                        if (wpchooser.ShowDialog() == DialogResult.Cancel) return;
                        wp = wpchooser.Selected;
                    }
                    else wp = bew.WerkPlekken?.FirstOrDefault();
                    if (wp == null)
                    {
                        XMessageBox.Show(owner, $"{b.Naam} heeft nog geen werkplek.\n\nMaak eerst een werkplek aan voordat je de rooster kan aanpassen.", "Geen Werkplek", MessageBoxIcon.Exclamation);
                        return;
                    }
                    var roosterform = new RoosterForm(wp.Tijden._rooster,
                        "Kies een rooster voor al je werkzaamheden");
                    roosterform.ViewPeriode = false;
                    if (roosterform.ShowDialog() == DialogResult.OK)
                    {
                        bool flag = wp.Personen.Any(x =>
                            x.WerkRooster == null || !x.WerkRooster.SameTijden(roosterform.WerkRooster));
                        if (flag)
                        {
                            var result = XMessageBox.Show(
                                owner, $"Er zijn personeel leden op {wp.Naam} die niet dezelfde rooster hebben als wat je hebt gekozen...\n" +
                                      $"De personeel bepaald natuurlijk hoe en wanneer er op {wp.Naam} wordt gewerkt.\n\n" +
                                      $"Wil je dit rooster door geven aan alle personeel leden op {wp.Naam}?",
                                "Personeel Werkrooster Wijzigen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                            if (result == DialogResult.Cancel) return;
                            if (result == DialogResult.Yes)
                            {
                                foreach (var per in wp.Personen)
                                    per.WerkRooster = roosterform.WerkRooster;
                            }
                        }
                        //wp.Tijden._rooster = roosterform.WerkRooster;
                        wp.UpdateWerkRooster(roosterform.WerkRooster,true, true,true, true, true,true, true);
                        var xchange = wp.Tijden._rooster != null && wp.Tijden._rooster.IsCustom()
                            ? "eigen rooster"
                            : "standaard rooster";
                        await b.UpdateBewerking(null,
                            $"{Manager.Opties.Username} heeft voor een {xchange} gekozen voor {wp.Path}", true);
                        
                    }
                }
            }
            catch (Exception e)
            {
                XMessageBox.Show(owner, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        public static async void ShowWerktIjden(this Bewerking bew, IWin32Window owner)
        {
            var b = bew;
            if (b == null) return;
            WerkPlek wp = null;
            if (b.WerkPlekken.Count == 1)
            {
                wp = b.WerkPlekken[0];

            }
            else if (b.WerkPlekken.Count > 1)
            {
                var wpchooser = new WerkPlekChooser(b.WerkPlekken,null);
                wpchooser.Title = $"Kies voor '{b.Naam}' een werkplek";
                if (wpchooser.ShowDialog() == DialogResult.Cancel) return;
                wp = wpchooser.Selected;
            }

            if (wp == null)
            {
                XMessageBox.Show(owner, $"{b.Naam} heeft geen werkplekken om daar tijden van te wijzigen!",
                    "Geen Werkplekken", MessageBoxIcon.Warning);
                return;
            }

            var wc = new WerktijdChanger(wp) {SaveChanges = true};
            if (wc.ShowDialog() == DialogResult.OK)
                await b.UpdateBewerking(null, $"[{b.Path}] Werktijd Aangepast");
        }

        public static void DoOnderbreking(this Bewerking bw, IWin32Window owner)
        {
            try
            {
                if (bw == null) return;
                var wps = bw.WerkPlekken.Where(x => x.IsActief()).ToList();
                WerkPlek wp = null;
                if (wps.Count == 1)
                {
                    wp = wps[0];

                }
                else if (wps.Count > 1)
                {
                    var wpchooser = new WerkPlekChooser(wps, null);
                    wpchooser.Title = $"Kies een werkplek om te onderbreken of te hervatten";
                    if (wpchooser.ShowDialog() == DialogResult.Cancel) return;
                    wp = wpchooser.Selected;
                }

                if (wp == null)
                {
                    XMessageBox.Show(owner, $"{bw.Naam} heeft geen indeling om te onderbreken.",
                        "Geen Indeling", MessageBoxIcon.Warning);
                    return;
                }

                var xsts = wp.Storingen.Where(x => !x.IsVerholpen).ToList();
                if (xsts.Count > 1)
                {
                    var allst = new AlleStoringen();
                    allst.InitStoringen(bw.Root, wp);
                    allst.ShowDialog();
                    return;
                }
                var xst = xsts.FirstOrDefault();
                var xnew = new NewStoringForm(wp, xst, xst != null);
                if (xnew.ShowDialog() == DialogResult.OK)
                {
                    if (xst != null)
                    {
                        var index = wp.Storingen.IndexOf(xst);
                        if (index > -1)
                        {
                            wp.Storingen[index] = xnew.Onderbreking;
                        }
                        else wp.Storingen.Add(xnew.Onderbreking);
                    }
                    else wp.Storingen.Add(xnew.Onderbreking);
                    xsts = wp.Storingen.Where(x => !x.IsVerholpen).ToList();
                    var x1 = xsts.Count > 0 ? "onderbroken" : "hervat";

                    if (bw.Combies.Count > 0 && xnew.Onderbreking != null)
                    {
                        foreach (var comb in bw.Combies)
                        {
                            var werk = Werk.FromPath(comb.Path);
                            if (werk?.Bewerking == null) continue;
                            var xwp = werk.Bewerking.WerkPlekken?.FirstOrDefault(x =>
                                string.Equals(x.Naam, wp.Naam, StringComparison.CurrentCultureIgnoreCase));
                            if (xwp == null) continue;
                            xst = xnew.Onderbreking.CreateCopy();
                            xst.Path = xwp.Path;
                            xwp.Storingen ??= new List<Storing>();
                            var xindex = xwp.Storingen.IndexOf(xst);
                            if (xindex > -1)
                                xwp.Storingen[xindex] = xst;
                            else xwp.Storingen.Add(xst);
                            _ = xwp.Werk.UpdateBewerking(null, $"'{xwp.Werk.Naam}' op '{xwp.Naam}' is {x1}");
                        }
                    }

                 
                    _= bw.UpdateBewerking(null, $"'{bw.Naam}' op '{wp.Naam}' is {x1}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Bewerkingen

        #region Personeel
        public static Klus GetKlus(this List<Klus> klusjes, string path)
        {
            try
            {
                return klusjes?.FirstOrDefault(x =>
                    string.Equals(path, x.Path, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool StopKlus(this List<Klus> klusjes, string path)
        {
            try
            {
                var klus = klusjes.GetKlus(path);

                if (klus != null) return klus.Stop();

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool StartKlus(this List<Klus> klusjes, string path)
        {
            try
            {
                var klus = klusjes.GetKlus(path);

                if (klus != null) return klus.Start();

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public static void CheckUserAvailibility(this Personeel[] personen, Bewerking bew, Personeel[] dbpersonen)
        //{
        //    try
        //    {
        //        if (personen != null && personen.Length > 0)
        //            foreach (var per in personen)
        //            {
        //                var xpers = dbpersonen.FirstOrDefault(x =>
        //                    x.PersoneelNaam.ToLower() == per.PersoneelNaam.ToLower());
        //                if (per.Actief && xpers != null && xpers.WerktAan != null && bew != null &&
        //                    !bew.Equals(xpers.WerktAan))
        //                    throw new Exception($"{xpers.PersoneelNaam} werkt al aan {xpers.WerktAan}!");

        //                if (bew != null && xpers != null && bew.Equals(xpers.WerktAan))
        //                {
        //                    if (per.Actief)
        //                    {
        //                        xpers.PerUur = per.PerUur;
        //                        xpers.Gestart = per.Gestart;
        //                        xpers.Gestopt = per.Gestopt;
        //                        if (bew.State == ProductieState.Gestart && per.Actief)
        //                        {
        //                            xpers.Werkplek = per.Werkplek;
        //                            xpers.WerktAan =bew.Path;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        xpers.WerktAan = null;
        //                    }
        //                }
        //            }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public static bool CheckUserAvailibility(this Personeel[] personen, Bewerking bew)
        //{
        //    try
        //    {
        //        if (personen != null && personen.Length > 0 && bew != null)
        //        {
        //            foreach (var per in personen.Where(x => x.Actief))
        //            {
        //                var xpers = Manager.Database.GetPersoneel(per.PersoneelNaam);
        //                if (xpers != null)
        //                    per.VrijeDagen = xpers.VrijeDagen;
        //                if (xpers != null && xpers.WerktAan != null && !bew.Equals(xpers.WerktAan))
        //                    throw new Exception($"{per.PersoneelNaam} werkt al aan {xpers.WerktAan}!");
        //            }

        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public static async Task<bool> ZetOpInactief(this Personeel personeel)
        {
            try
            {
                if (personeel?.Klusjes == null)
                    return false;
                var xdb = await Manager.Database.GetPersoneel(personeel.PersoneelNaam);
                foreach (var klus in personeel.Klusjes)
                {
                    if (klus.Status != ProductieState.Gestart)
                        continue;
                    klus.Stop();
                    klus.IsActief = false;
                    xdb.ReplaceKlus(klus);
                    await Manager.Database.UpSert(xdb, $"{personeel.PersoneelNaam} op inactief gezet.");
                    var pair = klus.GetWerk();
                    var prod = pair.Formulier;
                    var bew = pair.Bewerking;
                    if (bew != null)
                    {
                        var saved = false;
                        foreach (var wp in bew.WerkPlekken)
                        {
                            var xpers = wp.Personen.FirstOrDefault(x =>
                                string.Equals(x.PersoneelNaam, personeel.PersoneelNaam, StringComparison.CurrentCultureIgnoreCase));
                            if (xpers is {Actief: true})
                            {
                                xpers.ReplaceKlus(klus);
                                saved = true;
                            }
                        }

                        if (saved)
                            await bew.UpdateBewerking(null, $"{personeel.PersoneelNaam} inactief gezet op {bew.Path}.");
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Personeel

        #region IO

        public static IPAddress GetSystemIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static string Sha1(this string input)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }
        }

        public static byte[] DeCompress(this Stream input)
        {
            byte[] xreturn = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    var zip =
                        new BufferedStream(new GZipStream(input,
                            CompressionMode.Decompress));
                    zip.CopyTo(ms);

                    xreturn = ms.ToArray();
                }
            }
            catch (Exception)
            {
            }

            return xreturn;
        }

        public static byte[] DeCompress(this byte[] input)
        {
            if (input == null)
                return null;
            byte[] data = null;
            using var ms = new MemoryStream(input);
            data = DeCompress(ms);

            return data;
        }

        public static byte[] Encrypt(this byte[] input, string pass)
        {
            try
            {
                if (input == null || input.Length < 8) return null;
                var pdb =
                    new Rfc2898DeriveBytes(pass,
                        new byte[] {0x43, 0x87, 0x23, 0x72, 0x36, 0x45, 0x88, 0x61});
                MemoryStream ms = new MemoryStream();
                Aes aes = new AesManaged();
                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms,
                    aes.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public static byte[] Decrypt(this byte[] input, string pass)
        {
            try
            {
                if (input == null || input.Length < 8) return null;
                var pdb =
                    new Rfc2898DeriveBytes(pass,
                        new byte[] {0x43, 0x87, 0x23, 0x72, 0x36, 0x45, 0x88, 0x61}); // Change this
                MemoryStream ms = new MemoryStream();
                Aes aes = new AesManaged();
                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms,
                    aes.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public static byte[] Compress(this byte[] data)
        {
            if (data == null)
                return null;
            byte[] xreturn = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var zip = new GZipStream(ms, CompressionLevel.Fastest))
                    {
                        zip.Write(data, 0, data.Length);
                    }

                    xreturn = ms.ToArray();
                }
            }
            catch
            {
            }

            return xreturn;
        }

        public static byte[] StringToByteArray(string entry)
        {
            return Encoding.UTF8.GetBytes(entry);
        }

        public static string ByteArrayToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static int ReadInt32(this Stream input)
        {
            var buffer = new byte[4];
            input.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static void WriteInt32(this Stream input, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            input.Write(buffer, 0, buffer.Length);
        }

        public static string GetTextFromImage(this Bitmap image)
        {
            var xreturn = "";
            //using (TesseractEngine x = new TesseractEngine(Mainform.BootDir, "nld", EngineMode.TesseractOnly))
            //{
            //    using var page = x.Process(image, PageSegMode.SingleColumn);
            //    xreturn = page.GetText().Trim();
            //}
            return xreturn;
        }

        public static string ReadResourceString(string name)
        {
            // Determine path
            try
            {
                string xreturn = null;
                var data = ReadResourceData(name);
                if (data != null)
                    xreturn = Encoding.UTF8.GetString(data);
                return xreturn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static byte[] ReadResourceData(string name)
        {
            // Determine path
            try
            {
                byte[] xreturn = null;
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = name;
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"

                resourcePath = assembly.GetManifestResourceNames()
                    .Single(str => str.ToLower().EndsWith(name.ToLower()));

                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                using (var reader = new BinaryReader(stream))
                {
                    xreturn = reader.ReadBytes((int) stream.Length);
                    reader.Close();
                }

                return xreturn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T xCopyTo<T>(this T source, T destination)
        {
            if (source == null || destination == null)
                return default;
            //var types = source.GetType().GetProperties().Where(t => t.CanWrite).ToArray();
            //foreach (var type in types)
            //    type.SetValue(destination, type.GetValue(source));
            //return destination;

            try
            {
                foreach (var type in source.GetType().GetProperties())
                    if (type.CanWrite)
                        type.SetValue(destination, type.GetValue(source));
                return destination;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static T CreateCopy<T>(this T source) where T : new()
        {
            try
            {
                using var ms = new MemoryStream();

                var sr = new SharpSerializer(true);
                sr.Serialize(source, ms);
                ms.Position = 0;
                return (T) sr.Deserialize(ms);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static T CopyTo<T>(this T source, ref T destination) where T : new()
        {
            return destination = CreateCopy(source);
        }

        public static T[] CopyTo<T>(this T[] source, ref T[] destination) where T : new()
        {
            destination = new T[source.Length];
            for (var i = 0; i < destination.Length; i++)
                CopyTo(source[i], ref destination[i]);
            return destination;
        }

        public static T DeSerialize<T>(this byte[] data)
        {
            try
            {
                if (data == null || data.Length < 8) return default;
                var ser = new SharpSerializer(true);
                using var ms = new MemoryStream(data);
                var xreturn = (T) ser.Deserialize(ms);

                return xreturn;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static T DeSerialize<T>(this Stream input)
        {
            try
            {
                var ser = new SharpSerializer(true);
                var xreturn = (T) ser.Deserialize(input);

                return xreturn;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static T DeSerialize<T>(this string filepath)
        {
            try
            {
                //T xreturn = default;
                if (!File.Exists(filepath)) return default;
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        var data = File.ReadAllBytes(filepath);
                        if (data.Length > 8)
                            return data.DeSerialize<T>();
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }

                return default;
                //using (var ms = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //{
                //    var ser = new SharpSerializer(true);
                //    xreturn = (T)ser.Deserialize(ms);
                //    ms.Close();
                //}
                //return xreturn;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
        }

        public static byte[] Serialize<T>(this T instance)
        {
            try
            {
                if (instance == null) return null;
                var ser = new SharpSerializer(true);
                using var ms = new MemoryStream();
                ser.Serialize(instance, ms);
                var xreturn = ms.ToArray();
                return xreturn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Serialize<T>(this T instance, string destination)
        {
            try
            {
                if (instance == null) return false;
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        File.WriteAllBytes(destination, instance.Serialize());
                        return true;
                    }
                    catch (Exception e)
                    {
                        
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion IO

        #region Matching

        public static int GetWordIndex(this string blok, string value, int startindex)
        {
            if (string.IsNullOrEmpty(blok) || string.IsNullOrEmpty(value))
                return -1;

            var index = blok.IndexOf(value[0], startindex);
            var cur = 0;
            while (index > -1 && index < blok.Length - value.Length)
            {
                cur++;
                var x = blok.Substring(index, value.Length);
                if (x.Match(value, value.Length - 2))
                    return index;
                index = blok.IndexOf(value[0], index + 1);
            }

            return -1;
        }

        public static bool Match(this string a, string b, int matchcount)
        {
            var count = 0;
            var cur = 0;
            for (var i = 0; i < a.Length; i++)
            {
                if (i > b.Length)
                    break;
                if (a[i] == b[cur])
                {
                    count++;
                    cur++;
                }
            }

            if (count >= matchcount)
                return true;
            return false;
        }

        public static bool xPublicInstancePropertiesEqual<T>(this T self, T to, Type[] ignore = null) where T : class
        {
            var path = string.Format("{0}.{1}", self, to);
            var dif = new List<string>();
            return CheckForEquality(path, self, to, dif, ignore);
        }

        public static bool xPublicInstancePropertiesEqual<T>(this T self, T to, List<string> differences,
            Type[] ignore = null) where T : class
        {
            var path = string.Format("{0}.{1}", self, to);
            return CheckForEquality(path, self, to, differences, ignore);
            //if (self == null && to == null)
            //    return true;
            //if (self == null || to == null)
            //    return false;
            //var prop1 = self.GetType().GetProperties().Where(t => t.PropertyType != typeof(byte[])).ToArray();
            //var prop2 = to.GetType().GetProperties().Where(t => t.PropertyType != typeof(byte[])).ToArray();
            //if (prop1.Length != prop2.Length)
            //    return false;
            //foreach (var prop in prop1)
            //    if (prop2.Any(t =>
            //        t.Name == prop.Name && !xCompareObjectTo(t.GetValue(self), prop.GetValue(to))))
            //        return false;
            //return true;
        }

        public static bool CheckForEquality(string path, object a, object b, List<string> differences,
            Type[] ignore = null)
        {
            try
            {
                if (a == null && b == null)
                    return true;
                if (a == null || b == null)
                    return false;
                if (a.Equals(b))
                    return true;
                if (ignore != null && ignore.Any(x => x == a.GetType()))
                    return true;
                var comparableA = a as IComparable;
                if (comparableA != null && comparableA.CompareTo(b) != 0)
                {
                    differences.Add(path);
                    return false;
                }

                if (ReferenceEquals(b, null))
                {
                    differences.Add(path);
                    return false; // This is mandatory: nothing else to compare
                }

                if (b.Equals(a))
                    return true;

                var type = a.GetType();
                if (type != b.GetType())
                {
                    differences.Add(path);
                    return false; // This is mandatory: we can't go on comparing different types
                }

                var listA = a as ICollection;
                if (listA != null)
                {
                    var listB = b as ICollection;

                    if (listA.Count == listB.Count)
                    {
                        var aEnumerator = listA.GetEnumerator();
                        var bEnumerator = listB.GetEnumerator();

                        var i = 0;
                        while (aEnumerator.MoveNext() && bEnumerator.MoveNext())
                            if (!CheckForEquality($"{path}[{i++}]",
                                aEnumerator.Current, bEnumerator.Current, differences, ignore))
                                return false;
                    }
                    else
                    {
                        differences.Add(path);
                        return false;
                    }

                    return true;
                }

                var properties = type.GetProperties().Where(x => x.GetMethod != null);
                foreach (var property in properties)
                    if (!CheckForEquality($"{path}.{property.Name}",
                        property.GetValue(a), property.GetValue(b), differences, ignore))
                        return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool xCompareObjectTo(this object a, object b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null || a.GetType() != b.GetType())
                return false;
            if (a is byte[] || b is byte[])
                return true;
            if (a is List<DatabaseUpdateEntry> crit1)
            {
                var crit2 = b as List<DatabaseUpdateEntry>;
                if (crit1.Count != crit2.Count)
                    return false;
                for (var i = 0; i < crit1.Count; i++)
                    if (!crit1[i].Equals(crit2[i]))
                        return false;
                return true;
            }

            if (a is DateTime[])
            {
                var xa = a as DateTime[];
                var xb = b as DateTime[];
                if (xa.Length != xb.Length)
                    return false;
                for (var i = 0; i < xa.Length; i++)
                    if (xa[i] != xb[i])
                        return false;
                return true;
            }

            if (a is List<UitgaandAdres> a4)
            {
                var a2 = b as List<UitgaandAdres>;
                if (a4.Count != a2.Count)
                    return false;
                for (var i = 0; i < a4.Count; i++)
                {
                    if (a4[i].Adres.ToLower() != a2[i].Adres.ToLower())
                        return false;

                    if (a4[i].States != null && a2[i].States == null)
                        return false;
                    if (a2[i].States != null && a4[i].States == null)
                        return false;
                    if (a4[i].States.Length != a2[i].States.Length)
                        return false;
                    if (a4[i].States.Any(t => a2[i].States.All(x => x != t))) return false;
                }

                return true;
            }

            if (a is List<InkomendAdres> a3)
            {
                var a2 = b as List<InkomendAdres>;
                if (a3.Count != a2.Count)
                    return false;
                for (var i = 0; i < a3.Count; i++)
                {
                    if (a3[i].Adres.ToLower() != a2[i].Adres.ToLower())
                        return false;

                    if (a3[i].Actions != null && a2[i].Actions == null)
                        return false;
                    if (a2[i].Actions != null && a3[i].Actions == null)
                        return false;
                    if (a3[i].Actions.Length != a2[i].Actions.Length)
                        return false;
                    for (var j = 0; j < a3[i].Actions.Length; j++)
                        if (a2[i].Actions.All(x => x != a3[i].Actions[j]))
                            return false;
                }

                return true;
            }

            if (a is ViewState[])
            {
                var a1 = a as ViewState[];
                var a2 = b as ViewState[];
                if (a1.Length != a2.Length)
                    return false;
                for (var i = 0; i < a1.Length; i++)
                    if (a1[i] != a2[i])
                        return false;
                return true;
            }

            if (a is string[])
            {
                var xa = a as string[];
                var xb = b as string[];
                if (xa.Length != xb.Length)
                    return false;
                for (var i = 0; i < xa.Length; i++)
                    if (xa[i].ToLower() != xb[i].ToLower())
                        return false;
                return true;
            }

            if (a is UserChange) return true;

            var xs = a.Equals(b);
            if (!xs)
                Console.WriteLine(@"");
            return xs;
        }

        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[]
                {
                    typeof(string),
                    typeof(string[]),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool EmailIsValid(this string emailAddress)
        {
            if (emailAddress == null)
                return false;
            var ValidEmailRegex = CreateValidEmailRegex();
            return ValidEmailRegex.IsMatch(emailAddress);
        }

        public static bool IsImage(this byte[] data)
        {
            var jpg = new List<string> {"FF", "D8"};
            var bmp = new List<string> {"42", "4D"};
            List<string> gif = new List<string> { "47", "49", "46" };
            List<string> png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
            var imgTypes = new List<List<string>> {jpg, bmp, gif, png};

            var bytesIterated = new List<string>();

            for (var i = 0; i < 8; i++)
            {
                var bit = data[i].ToString("X2");
                bytesIterated.Add(bit);

                var isImage = imgTypes.Any(img => !img.Except(bytesIterated).Any());
                if (isImage)
                    return true;
            }

            return false;
        }

        public static bool IsImageFile(this string filepath)
        {
            try
            {
                bool isvalid = false;
                using FileStream fs = new FileStream(filepath, FileMode.Open,FileAccess.Read, FileShare.ReadWrite);
                var data = new byte[8];
                if (fs.Length >= data.Length)
                {
                    if (fs.Read(data, 0, data.Length) == data.Length)
                        isvalid = data.IsImage();

                }

                return isvalid;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool IsDefault<T>(this T value) where T : struct
        {
            var isDefault = value.Equals(default(T));

            return isDefault;
        }

        #endregion Matching

        #region Conversions

        public static string WrapText(this string text, int width)
        {
            var x = text;
            if (text != null && text.Length > width)
            {
                var count = 0;
                var value = text.Replace("\n", " ");
                foreach (var v in value)
                    if (count >= width && v == ' ')
                    {
                        x += "\n";
                        count = 0;
                    }
                    else
                    {
                        x += v;
                        count++;
                    }
            }

            return x;
        }

        private static Regex CreateValidEmailRegex()
        {
            var validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                    + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                    + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }

        public static string DateToString(this DateTime date, DateTimeConvertType type)
        {
            switch (type)
            {
                case DateTimeConvertType.FullDate:
                    return date.ToString("dddd d MMMM yyyy HH:mm");

                case DateTimeConvertType.OnlyDate:
                    return date.ToString("dddd d MMMM yyyy");

                case DateTimeConvertType.OnlyTime:
                    return date.TimeOfDay.ToString("HH:mm");
            }

            return date.ToString();
        }

        public static DateTime ToDateTime(this string value)
        {
            var date = new DateTime();
            DateTime.TryParse(value, out date);
            return date;
        }

        public static DateTime ChangeTime(this DateTime date, TimeSpan time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0,
                0);
        }

        public static TimeSpan ToTime(this string value)
        {
            var date = new DateTime();
            DateTime.TryParse(value, out date);
            return date.TimeOfDay;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="maxuren"></param>
        /// <param name="telaatformat">te laat string formaat waarvan de eerste oject de tijd is en de tweede de soort tijd (min,sec of uren)</param>
        /// <param name="tevroegformat"></param>
        /// <returns></returns>
        public static string ToString(this DateTime time, double maxuren, string telaatformat, string tevroegformat, bool alleenwerktijd)
        {
            if (time.IsDefault()) return "Geen Tijd";
            bool islater = time > DateTime.Now;
            if (islater)
            {
                var tg = alleenwerktijd?Werktijd.WerkTijdNodigTotLeverdatum(time, DateTime.Now) : (time - DateTime.Now);
                double urenlater = Math.Round(tg.TotalHours,2);
                double minlater = Math.Round(tg.TotalMinutes, 2);
                double seclater = Math.Round(tg.TotalSeconds, 0);
                if (seclater == 0) return "nu";
                if (seclater < 60)
                    return string.Format(telaatformat, new object[] { seclater, "seconden" });
                if (minlater < 60)
                    return string.Format(telaatformat, new object[] { minlater, "minuten" });
                if (urenlater <= maxuren)
                    return string.Format(telaatformat, new object[] { urenlater, "uur" });
            }
            else
            {
                var tg = alleenwerktijd ? Werktijd.TijdGewerkt(time, DateTime.Now, null, null) : (DateTime.Now - time);
                double ureneerder = Math.Round(tg.TotalHours, 2);
                double mineerder = Math.Round(tg.TotalMinutes, 2);
                double seceerder = Math.Round(tg.TotalSeconds, 0);
                if (seceerder == 0) return "nu";
                if (seceerder < 60)
                    return string.Format(tevroegformat, new object[] { seceerder, "seconden" });
                if (mineerder < 60)
                    return string.Format(tevroegformat, new object[] { mineerder, "minuten" });
                if (ureneerder <= maxuren)
                    return string.Format(tevroegformat, new object[] { ureneerder, "uur" });
            }

            return time.ToString();
        }

        public static object ToObjectValue(this string value, Type type)
        {
            object xreturn = null;
            if (typeof(int) == type)
            {
                int val;
                if (int.TryParse(value, out val))
                    xreturn = val;
            }
            else if (typeof(string) == type)
            {
                xreturn = value;
            }
            else if (typeof(double) == type)
            {
                double val;
                if (double.TryParse(value, out val))
                    xreturn = val;
            }
            else if (typeof(DateTime) == type)
            {
                DateTime val;
                if (DateTime.TryParse(value, out val))
                    xreturn = val;
            }
            else if (typeof(bool) == type)
            {
                bool val;
                if (bool.TryParse(value, out val))
                    xreturn = val;
            }

            return xreturn;
        }

        public static T ConvertObject<T>(object input)
        {
            return (T) Convert.ChangeType(input, typeof(T));
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo) member).EventHandlerType;

                case MemberTypes.Field:
                    return ((FieldInfo) member).FieldType;

                case MemberTypes.Method:
                    return ((MethodInfo) member).ReturnType;

                case MemberTypes.Property:
                    return ((PropertyInfo) member).PropertyType;

                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public static void SetValue(this DateTimePicker dtp, DateTime value)
        {
            dtp.Value = value < dtp.MinDate || value > dtp.MaxDate ? DateTime.Now : value;
        }

        public static void SetValue(this NumericUpDown nric, decimal value)
        {
            nric.Value = value < nric.Minimum || value > nric.Maximum ? nric.Minimum : value;
        }

        public static string ToHexString(this byte[] data, int length = -1)
        {
            string s = string.Empty;
            if (data == null) return s;
            int count = length <= 0 || length > data.Length ? data.Length : length;
            for (var i = 0; i < count; i++)
            {
                byte b = data[i];
                var n = (int) b;
                var n1 = n & 15;
                var n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char) (n2 - 10 + (int) 'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char) (n1 - 10 + (int) 'A')).ToString();
                else
                    s += n1.ToString();
                //if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }

            return s;
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input[0].ToString().ToUpper() + input.Substring(1)
            };

        #endregion Conversions

        #region Misc

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        /// <summary>
        /// Scrolls the vertical scroll bar of a multi-line text box to the bottom.
        /// </summary>
        /// <param name="tb">The text box to scroll</param>
        public static void ScrollToBottom(this Control tb)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                SendMessage(tb.Handle, WM_VSCROLL, new IntPtr(SB_BOTTOM), IntPtr.Zero);
        }

        public static bool CanRead(this string filepath)
        {
            try
            {
                bool xreturn = false;
                using FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                xreturn = fs.CanRead;
                fs.Close();

                return xreturn;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool CopyDirectoryTo(this string sourcedirectory, string destination, IWin32Window owner)
        {
            try
            {
                FileSystem.CopyDirectory(sourcedirectory, destination,
                    UIOption.AllDialogs, UICancelOption.ThrowException);
                return true;
            }
            catch (Exception e)
            {
                XMessageBox.Show(owner, e.Message, "Fout", MessageBoxIcon.Error);
                return false;
            }
        }

        public static string GetLastOfPathName(this string path, int depth)
        {
            string fpath = path;
            string xreturn = Path.GetFileName(fpath);
            for (int i = 0; i < depth; i++)
            {
                //string fname = Path.GetFileName(fpath);
                fpath = Path.GetDirectoryName(fpath);
                string xp = Path.GetFileName(fpath);
                bool br = false;
                if (string.IsNullOrEmpty(xp))
                {
                    xp = fpath?.TrimEnd(new char[]{'\\'});
                    br = true;
                }
                xreturn =  xp + "\\" + xreturn;
                if (br) break;
            }

            return xreturn;
        }

        public static string GetDefaultBrowserPath()
        {
            string defaultBrowserPath = null;
            RegistryKey regkey;

            // Check if we are on Vista or Higher
            OperatingSystem OS = Environment.OSVersion;
            if ((OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6))
            {
                regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", false);
                if (regkey != null)
                {
                    defaultBrowserPath = regkey.GetValue("Progid").ToString();
                }
                else
                {
                    regkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\IE.HTTP\\shell\\open\\command", false);
                    if (regkey != null) defaultBrowserPath = regkey.GetValue("").ToString();
                }
            }
            else
            {
                regkey = Registry.ClassesRoot.OpenSubKey("http\\shell\\open\\command", false);
                if (regkey != null) defaultBrowserPath = regkey.GetValue("").ToString();
            }

            return defaultBrowserPath;
        }

        public static string GetAvailibleFilepath(string directory, string filename)
        {
            if (string.IsNullOrEmpty(directory)) return filename;
            var fp = directory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var ext = Path.GetExtension(filename);

            var newfilename = filename.Replace(ext,"");
            var count = Directory.GetFiles(fp).Where(x =>
                    string.Equals((Path.GetFileName(x).Replace(ext, "").Split('[')[0] + ext),
                        ($"{newfilename}".Split('[')[0] + ext), StringComparison.CurrentCultureIgnoreCase))
                .ToArray().Length;
            if (count > 0)
                newfilename += $"[{count}]";
            newfilename += ext;
            return directory + "\\" + newfilename;
        }

        public static void PrintPDFWithAcrobat(string pdffile)
        {
            PrinterSettings printer = new PrinterSettings
            {
                PrintFileName = pdffile
            };

            PrintDocument PrintDoc = new PrintDocument();
            printer.PrintFileName = pdffile;

            PrintDoc.DocumentName = Path.GetFileName(pdffile);
            var xpr = new PrintDialog();
            xpr.Document = PrintDoc;
            xpr.PrintToFile = false;
            if (xpr.ShowDialog() == DialogResult.OK)
            {
                xpr.Document.Print();
            }
        }

        public static bool CleanupFilePath(this string filepath, string directorypath, string filename, bool move, bool rename)
        {
            try
            {
                if (Manager.Opties is {VerwijderVerwerkteBestanden: true} && File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
                else
                {
                    var fp = directorypath;
                    if (!Directory.Exists(directorypath))
                        Directory.CreateDirectory(directorypath);

                    var ext = Path.GetExtension(filepath);

                    var newfilename = $"{fp}\\{filename}";
                    if (rename)
                    {
                        var count = Directory.GetFiles(fp).Where(x =>
                                string.Equals((Path.GetFileName(x).Replace(ext, "").Split('[')[0] + ext),
                                    ($"{filename}".Split('[')[0] + ext), StringComparison.CurrentCultureIgnoreCase))
                            .ToArray().Length;
                        if (count > 0)
                            newfilename = newfilename + $"[{count}]";
                        newfilename += ext;
                    }

                    if (!File.Exists(newfilename))
                    {
                        if (move)
                            File.Move(filepath, newfilename);
                        else
                            File.Copy(filepath, newfilename, true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static int CheckForSameInstance()
        {
            var processes =
                Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location));
            return processes.Length;
        }

        public static int GetWeekNr(this DateTime datum)
        {
            var cul = CultureInfo.CurrentCulture;
            var weekNum = cul.Calendar.GetWeekOfYear(
                datum,
                CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
           
            //if (datum.DayOfWeek == DayOfWeek.Monday)
                weekNum--;
            return weekNum;
        }

        public static List<WeekRange> GetWeekRanges(DateTime vanaf, DateTime tot, bool includethisweek, bool shownow)
        {
            if (shownow) return GetNowTijden();
            return WeekRange.WeekDays(vanaf, tot, includethisweek);
        }

        public static List<WeekRange> GetNowTijden()
        {
            var xreturn = new List<WeekRange>();
            var rooster = Manager.Opties?.GetWerkRooster();
            if (rooster == null) return xreturn;
            DateTime xnow = Werktijd.EerstVolgendeWerkdag(DateTime.Now.Date, ref rooster,rooster,null);
            TimeSpan startday = rooster.StartWerkdag;
            TimeSpan endday = rooster.EindWerkdag;
            while (startday <= endday)
            {
                var xstart = new DateTime(xnow.Year, xnow.Month, xnow.Day, startday.Hours, startday.Minutes, 0);
                var xstop = new DateTime(xnow.Year, xnow.Month, xnow.Day, startday.Hours + 1, startday.Minutes, 0);
                var weekrange = new WeekRange()
                {
                    Name = $"{startday} uur",
                    StartDate =xstart,
                    EndDate = xstop,
                    Range = xstart.ToString() + '-' + xstop.ToString(),
                    Month = xstart.Month,
                    Year = xstart.Year
                };
                xreturn.Add(weekrange);
                startday = startday.Add(TimeSpan.FromHours(1));
            }

            return xreturn;
        }

        public static List<WeekRange> GetWeekRanges(int vanafweek, int vanafjaar, bool includethisweek, bool shownow)
        {
            if (shownow) return GetNowTijden();
            if (vanafjaar <= 2000 || vanafweek < 1) return new List<WeekRange>();
            var startdate = DateOfWeek(vanafjaar, DayOfWeek.Monday, vanafweek);
            var xnow = DateTime.Now;
            var enddate = DateOfWeek(xnow.Year, DayOfWeek.Sunday, xnow.GetWeekNr());
            var rooster = Manager.Opties?.GetWerkRooster();
            if (rooster != null)
            {
                startdate = startdate.Add(Manager.Opties.GetWerkRooster().StartWerkdag);
                enddate = enddate.Add(Manager.Opties.GetWerkRooster().EindWerkdag);
            }

            return WeekRange.WeekDays(startdate, enddate, includethisweek);
        }

        public static WeekRange GetWeekRange(int startweek, int startjaar, Rooster rooster)
        {
            var startdate = DateOfWeek(startjaar, DayOfWeek.Monday, startweek);
            var xnow = DateTime.Now;
            var enddate = DateOfWeek(xnow.Year, DayOfWeek.Friday, xnow.GetWeekNr());
            rooster ??= Manager.Opties?.GetWerkRooster();
            if (rooster != null)
            {
                startdate = startdate.Add(rooster.StartWerkdag);
                enddate = enddate.Add(rooster.EindWerkdag);
            }

            return new WeekRange
            {
                EndDate = enddate,
                Month = enddate.Month,
                Range = $"Vanaf {startdate.ToShortDateString()} tot {enddate.ToShortDateString()}",
                StartDate = startdate,
                Week = startweek,
                Year = startjaar
            };
        }

        public static DateTime DateOfWeek(int year, DayOfWeek day, int week)
        {
            var startOfYear = new DateTime(year, 1, 1,0,0,0);

            // The +7 and %7 stuff is to avoid negative numbers etc.
            var daysToFirstCorrectDay = ((int) day - (int) startOfYear.DayOfWeek + 7) % 7;
            if (day == DayOfWeek.Monday) week--;
            return startOfYear.AddDays(7 * (week) + daysToFirstCorrectDay);
        }

        public static DateTime DateOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            // System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            //DayOfWeek fdow = ci.DateTimeFormat.FirstDayOfWeek;
            int daynr =(startOfWeek == DayOfWeek.Sunday? 7 : (int)startOfWeek);
            daynr -= (int)dt.DayOfWeek;
            var xdt = dt.AddDays(daynr);
            return xdt;
        }

        public static UserChange UpdateChange(this UserChange change, string message, DbType type)
        {
            if (change == null)
                change = new UserChange(message, type);
            else
                change.Change = message;
            change.DbIds[type] = DateTime.Now;
            change.PcId = Manager.SystemId;
            change.TimeChanged = DateTime.Now;
            change.User = Manager.Opties == null ? Manager.SystemId : Manager.Opties.Username;
            return change;
        }

        public static bool IsAvailiblePort(this int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveUdpListeners();

            foreach (var tcpi in tcpConnInfoArray)
                if (tcpi.Port == port)
                    return false;

            return true;
        }

        public static Size MeasureString(this string value, Font font)
        {
            return TextRenderer.MeasureText(value, font);
        }

        public static Size MeasureString(this string value, Font font, Size maxsize)
        {
            return TextRenderer.MeasureText(value, font, maxsize);
        }

        public static void Shutdown()
        {
            var mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            var mboShutdownParams =
                mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (var o in mcWin32.GetInstances())
            {
                var manObj = (ManagementObject) o;
                manObj.InvokeMethod("Win32Shutdown",
                    mboShutdownParams, null);
            }
        }

        public static void SetAutoStartOnBoot(bool autostart)
        {
            var path = Assembly.GetEntryAssembly()
                ?.Location; // for getting the location of exe file ( it can change when you change the location of exe)
            var fileName =
                Assembly.GetExecutingAssembly().GetName()
                    .Name; // for getting the name of exe file( it can change when you change the name of exe)
            StartExeWhenPcStartup(fileName, path, autostart); // start the exe autometically when computer is stared.
        }

        public static void StartExeWhenPcStartup(string filename, string filepath, bool startup)
        {
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startup) key.SetValue(filename, filepath);
            else key.DeleteValue(filename, false);
        }
        
        public static Dictionary<string, List<WerkPlek>> CreateStoringCollection(this WerkPlek[] werkplekken)
        {
            var xreturn = new Dictionary<string, List<WerkPlek>>();
            try
            {
                if (werkplekken is {Length: > 0})
                    foreach (var wp in werkplekken)
                    {
                        var value = xreturn[wp.Naam];
                        if (value == null)
                            value = new List<WerkPlek>();
                        if (wp.Storingen is {Count: > 0})
                            value.Add(wp);
                    }
            }
            catch
            {
            }

            return xreturn;
        }

        public static Dictionary<DateTime, DateTime> CreateStoringDictionary(this WerkPlek[] werkplekken)
        {
            if (werkplekken == null || werkplekken.Length == 0)
                return default;
            var xreturn = new Dictionary<DateTime, DateTime>();
            foreach (var wp in werkplekken)
            {
                if (wp.Storingen == null || wp.Storingen.Count <= 0) continue;
                foreach (var st in wp.Storingen)
                {
                    var gestopt = st.IsVerholpen ? st.Gestopt : DateTime.Now;
                    xreturn[st.Gestart] = gestopt;
                    //xreturn.Add(st.Gestart, gestopt);
                }
            }

            return xreturn;
        }

        public static KeyValuePair<string, List<WerkPlek>> UpdateStoringCollection(
            this Dictionary<string, List<WerkPlek>> collection, WerkPlek plek, bool withoutstoringen, TijdEntry bereik)
        {
            try
            {
                if (collection != null && plek != null)
                {
                    var added = true;
                    var wps = new List<WerkPlek>();
                    var storingen = bereik != null
                        ? plek.Storingen.Where(x => x.Gestart >= bereik.Start && x.Gestart <= bereik.Stop).ToList()
                        : plek.Storingen;
                    plek.Storingen = storingen;
                    if (!collection.ContainsKey(plek.Naam))
                    {
                        if (storingen is {Count: > 0} || withoutstoringen)
                        {
                            collection.Add(plek.Naam, new List<WerkPlek>());
                            wps = collection[plek.Naam];
                        }
                        else
                        {
                            added = false;
                        }
                    }
                    else
                    {
                        wps = collection[plek.Naam];
                    }

                    if (added)
                    {
                        var item = wps.FirstOrDefault(x => x.Equals(plek));
                        if (item != null)
                        {
                            if (storingen == null || (storingen.Count == 0 &&  !withoutstoringen))
                                wps.Remove(item);
                            else
                                item.Storingen = storingen;
                        }
                        else if (storingen != null && (storingen.Count > 0 || withoutstoringen))
                        {
                            wps.Add(plek);
                        }

                        var xreturn = collection.FirstOrDefault(x => x.Key == plek.Naam);
                        if (!xreturn.IsDefault() && xreturn.Value == null || xreturn.Value.Count == 0)
                            collection.Remove(xreturn.Key);
                        else return xreturn;
                    }
                }
            }
            catch (Exception)
            {
            }

            return new KeyValuePair<string, List<WerkPlek>>();
        }

        public static DbType GetInstanceType<T>()
        {
            switch (typeof(T).Name)
            {
                case nameof(ProductieFormulier):
                    return DbType.Producties;
                case nameof(Personeel):
                    return DbType.Medewerkers;
                case nameof(UserAccount):
                    return DbType.Accounts;
                case nameof(UserSettings):
                    return DbType.Opties;
                case nameof(DbVersion):
                    return DbType.Versions;
                case nameof(UserChange):
                    return DbType.Changes;
                case nameof(LogEntry):
                    return DbType.Logs;
                default:
                    return DbType.None;
            }
        }

        public static object GetPropValue(this object src, string propName)
        {
            try
            {
                var xitem = src?.GetType().GetProperty(propName);
                return xitem?.GetValue(src, null);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Type GetPropType(this object src, string propName)
        {
            return src?.GetType().GetProperty(propName)?.GetType();
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propName)
        {
            return type?.GetProperty(propName);
        }

        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null || type.IsEnum) return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSupportedType(this Type type)
        {
            if (type == null || !type.IsPublic) return false;
            if (type.IsEnum) return true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.DateTime:
                case TypeCode.Boolean:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCollectionType(this Type type)
        {
            return type.GetInterfaces().Any(s => s.Namespace == "System.Collections.Generic" && (s.Name == "ICollection" || s.Name.StartsWith("ICollection`")));
        }

        public static string GetPropertyDescription(this Type type, string name)
        {
            try
            {
                var atri = type.GetProperty(name)?.GetCustomAttributes(false);
                if (atri == null) return null;
                if (atri.Length > 0)
                {
                    if (atri[0] is DisplayAttribute attrib)
                        return attrib.Description;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string GetStringFromUrl(string url)
        {
            var result = string.Empty;
            try
            {
                using var webClient = new System.Net.WebClient();
                result = webClient.DownloadString(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        public static Dictionary<string, string> GetVersionPreviews(string url)
        {
            var result = new Dictionary<string, string>();
            try
            {
                var xvalue = GetStringFromUrl(url);
                using var sr = new StringReader(xvalue);
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        var xsplit = line.TrimStart(new char[] {'['}).TrimEnd(new char[] {']'}).Split(';');
                        if(xsplit.Length < 2) continue;
                        if (result.ContainsKey(xsplit[0])) continue;
                        result.Add(xsplit[0].Trim(), xsplit[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result.OrderBy(x => x.Key).ToDictionary(i => i.Key, i => i.Value);
        }

        public static int FindIndex(this string text, string value)
        {
            var index = text.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            if (index > -1) return index;
            while (true)
            {
                index = text.IndexOf(value[0], index);
                if (index < 0) break;
                var xval = text.Substring(index, value.Length);
                if (xval.ToLower().StartsWith(value.ToLower())) return index;
                index++;
            }

            return -1;
        }

        public static Color GetProgressColor(double percentage)
        {
            switch (percentage)
            {
                case < 5:
                    return Color.DarkRed;
                case < 15:
                    return Color.Red;
                case < 25:
                    return Color.DarkOrange;
                case < 35:
                    return Color.Orange;
                case < 45:
                    return Color.Yellow;
                case < 75:
                    return Color.LightGreen;
                case < 101:
                    return Color.Green;
                case < 125:
                    return Color.RoyalBlue;
                case < 150:
                    return Color.HotPink;
            }

            return Color.Purple;
        }

        public static double GetPercentageDifference(this double a, double b)
        {
            return (b - a) == 0 ? 0 : Math.Round((((b - a) / (a == 0 ? b : a)) * 100), 2);
        }

        #endregion Misc
    }
}