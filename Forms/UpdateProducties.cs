﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProductieManager.Properties;
using Rpm.Misc;
using Rpm.SqlLite;
using Rpm.Productie;
using Rpm.Settings;
using Rpm.Various;

namespace Forms
{
    public partial class UpdateProducties : MetroFramework.Forms.MetroForm
    {
        public UpdateProducties(DatabaseUpdateEntry updateentry = null)
        {
            InitializeComponent();
            UpdateEntry = updateentry;
            UpdateMethod = UpdateForms;
            Cancelation = new CancellationTokenSource();
        }

        public Func<Task> UpdateMethod { get; set; }

        public CancellationTokenSource Cancelation;
        public DatabaseUpdateEntry UpdateEntry { get; private set; }
        public bool IsBussy { get; private set; }

        public bool ShowStop { get; set; } = true;

        public bool StartWhenShown { get; set; }

        public bool CloseWhenFinished { get; set; }
        public bool IsFinished { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void xstartb_Click(object sender, EventArgs e)
        {
            Start();
        }

        private async void Start()
        {
            if (IsBussy)
            {
                Stop();
            }
            else
            {
                if (Manager.Database == null || Manager.Database.IsDisposed)
                {
                    XMessageBox.Show(
                        "Database is niet beschikbaar!\nZorg ervoor dat je een geldige database hebt, en restart het programma.",
                        "Ongeldige Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    //var count = await Manager.Database.Count(DbType.GereedProducties);
                    //count += await Manager.Database.Count(DbType.Producties);
                    //if (count > 0)
                    //{
                    IsBussy = true;
                    OnStarted();
                    await StartMethod(UpdateMethod);
                    OnFinished();
                    //}
                    //else if (!CloseWhenFinished)
                    //    XMessageBox.Show("Er is zijn geen producties beschikbaar", "Productie Updates");
                    if (CloseWhenFinished)
                        this.Close();

                }
            }
        }

        public Task StartMethod(Func<Task> Update)
        {
            return Update?.Invoke();
        }

        private void Stop()
        {
            if (!IsBussy)
                return;
            IsBussy = false;
            Cancelation.Cancel();
        }

        private Task UpdateForms()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (IsDisposed) return;
                    var count = 0;
                    var oldlog = Manager.Database.LoggerEnabled;
                    var oldnot = Manager.Database.NotificationEnabled;
                    Manager.Database.LoggerEnabled = false;
                    Manager.Database.NotificationEnabled = false;

                    if (UpdateEntry != null)
                    {
                        await UpdateFromDatabaseEntry(UpdateEntry);
                    }
                    else
                    {
                        Manager.Database.RaiseEventWhenChanged = false;
                        Manager.Database.RaiseEventWhenDeleted = false;
                        DoProgress("Updating Medewerkers...", count, 100);
                        await UpdateUserActivity(false);
                        if (!IsBussy || IsDisposed || Disposing)
                            return;
                        DoProgress("Producties laden...", count, 100);
                        var files = await Manager.GetAllProductiePaths(true,true);
                        var forms = await Manager.Database.GetAllProducties(true, false);
                        foreach (var file in files)
                        {
                            if (!IsBussy || IsDisposed || Disposing)
                                break;
                            ProductieFormulier form = await MultipleFileDb.FromPath<ProductieFormulier>(file);
                            if (form == null) continue;
                            await form.UpdateForm(true, true, forms, null, true, false, true, true);
                            count++;
                            DoProgress($"Updating productie '{form.ProductieNr}'", count, files.Count);
                        }

                        if (IsBussy)
                        {
                            var opties = Manager.DefaultSettings??UserSettings.GetDefaultSettings();
                            IsFinished = true;
                            opties.UpdateDatabaseVersion = LocalDatabase.DbVersion;
                            opties.SaveAsDefault();
                            string x1 = count == 1 ? "update" : "updates";
                            DoProgress($"{count} {x1} Uitgevoerd!", 100);
                        }
                    }
                    Manager.Database.RaiseEventWhenChanged = true;
                    Manager.Database.RaiseEventWhenDeleted = true;
                    Manager.Database.LoggerEnabled = oldlog;
                    Manager.Database.NotificationEnabled = oldnot;
                }
                catch
                {
                }
            });
        }

        public Task UpdateUserActivity(bool gestart)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var users = await Manager.Database.GetAllPersoneel();
                    int done = 0;
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (!IsBussy || IsDisposed || Disposing)
                            break;
                        var user = users[i];
                        DoProgress($"Updating {user.PersoneelNaam}...", i, users.Count);
                        bool userchanged = false;
                        if (user.PersoneelNaam.EndsWith(" "))
                        {
                            user.PersoneelNaam = user.PersoneelNaam.Trim();
                            userchanged = true;
                        }

                        List<Klus> toremove = new List<Klus>();

                        var klusjes = gestart
                            ? user.Klusjes.Where(x => x.Status == ProductieState.Gestart).ToList()
                            : user.Klusjes.ToList(); //.Where(x=> x.IsActief).ToList(); //.Where(x => x.Status == ProductieState.Gestart).ToArray();

                        if (klusjes.Count > 0)
                        {

                            foreach (var klus in klusjes)
                            {
                                var pair = klus.GetWerk();
                                var prod = pair?.Formulier;
                                var bew = pair?.Bewerking;
                                if (prod == null || bew == null)
                                {
                                    user.Klusjes.Remove(klus);
                                    userchanged = true;
                                    continue;
                                }

                                var saved = false;
                                var plek =
                                    bew.WerkPlekken.FirstOrDefault(x =>
                                        string.Equals(x.Naam, klus.WerkPlek,
                                            StringComparison.CurrentCultureIgnoreCase));
                                if (plek == null)
                                {
                                    toremove.Add(klus);
                                    continue;
                                }

                                var xolduser = plek.Personen.FirstOrDefault(x =>
                                    string.Equals(x.PersoneelNaam, user.PersoneelNaam,
                                        StringComparison.CurrentCultureIgnoreCase));
                                var msg = "";
                                if (bew.State != klus.Status)
                                {
                                    switch (bew.State)
                                    {
                                        case ProductieState.Gestopt:
                                            klus.Stop();
                                            saved = true;
                                            break;
                                        case ProductieState.Gestart:
                                            if (klus.IsActief)
                                            {
                                                klus.Start();
                                                saved = true;
                                            }

                                            break;
                                        case ProductieState.Gereed:
                                            klus.MeldGereed();
                                            saved = true;
                                            break;
                                        case ProductieState.Verwijderd:
                                            klus.Stop();
                                            klus.Status = ProductieState.Verwijderd;
                                            saved = true;
                                            break;
                                    }

                                    msg =
                                        $"{klus.Path} van {user.PersoneelNaam}  is verandert naar {Enum.GetName(typeof(ProductieState), klus.Status)}.";

                                }

                                if (klus.Tijden.IsActief && klus.Status != ProductieState.Gestart)
                                {
                                    klus.Stop();
                                    saved = true;
                                }

                                if (saved)
                                {
                                    userchanged = true;
                                    if (xolduser != null && xolduser.ReplaceKlus(klus))
                                        await bew.UpdateBewerking(null, $"{klus.Naam} klus geupdate");
                                }
                            }
                        }

                        if (userchanged || toremove.Count > 0)
                        {
                            foreach (var k in toremove)
                                user.Klusjes.Remove(k);
                            await Manager.Database.UpSert(user, $"{user.PersoneelNaam} update");
                        }

                        done++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        public Task<int> UpdateFromDatabaseEntry(DatabaseUpdateEntry entry)
        {
            return Task.Run(async () =>
            {
                int count = 0;
                try
                {
                    count = await Manager.Database.UpdateDbFromDb(entry, Cancelation, ProgressChanged);
                    DoProgress("Update Geslaagd!", 100);
                    IsFinished = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return count;
            });
        }

        public void ProgressChanged(object sender, ProgressArg arg)
        {
            arg.IsCanceled = !IsBussy;
            DoProgress(arg.Message, arg.Pogress);
        }

        private void OnFinished()
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                if (CloseWhenFinished)
                {
                    this.Invoke(new Action(() => DialogResult = DialogResult.OK));
                    //this.Invoke(new Action(this.Close));
                }
                else
                {
                    xstartb.Invoke(new Action(() => xstartb.Image = Resources.play_button_icon_icons_com_60615));
                    xstartb.Invoke(new Action(xstartb.Invalidate));
                    circularProgressBar1.Invoke(new Action(() => circularProgressBar1.Value = 65));
                    circularProgressBar1.Invoke(new Action(() => circularProgressBar1.SubscriptText = ""));
                    circularProgressBar1.Invoke(new Action(circularProgressBar1.Invalidate));
                }
            }
            else
            {
                if (CloseWhenFinished)
                {
                    DialogResult = DialogResult.OK;
                    //this.Close();
                }
                else
                {

                    xstartb.Image = Resources.play_button_icon_icons_com_60615;
                    xstartb.Invalidate();
                    circularProgressBar1.Value = 68;
                    circularProgressBar1.SubscriptText = "";
                    circularProgressBar1.Invalidate();
                }
            }

            IsBussy = false;
        }

        private void OnStarted()
        {
            if (InvokeRequired)
            {
                xstartb.Invoke(new Action(() => xstartb.Image = Resources.stop_red256_24890));
                xstartb.Invoke(new Action(xstartb.Invalidate));
            }
            else
            {
                xstartb.Image = Resources.stop_red256_24890;
                xstartb.Invalidate();
            }

            IsBussy = true;
        }

        private void DoProgress(string message, double min, double max)
        {
            var percentage = max > 0  && min > 0? (int) ((min / max) * 100) : 0;
            DoProgress(message, percentage);
        }

        private void DoProgress(string message, int percentage)
        {
           
            if (InvokeRequired)
            {
                circularProgressBar1.Invoke(
                    new Action(() => circularProgressBar1.Text = $"{message}"));
                circularProgressBar1.Invoke(new Action(() => circularProgressBar1.Value = percentage));
                circularProgressBar1.Invoke(new Action(() => circularProgressBar1.SubscriptText = percentage + "%"));
                circularProgressBar1.Invoke(new Action(circularProgressBar1.Invalidate));
            }
            else
            {
                circularProgressBar1.Text = $"{message}";
                circularProgressBar1.Value = percentage;
                circularProgressBar1.SubscriptText = percentage + "%";
                circularProgressBar1.Invalidate();
            }
        }

        private void UpdateProducties_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }

        private void UpdateProducties_Shown(object sender, EventArgs e)
        {
            xstartb.Visible = ShowStop;
            //var openform = Application.OpenForms["SplashScreen"];
            //openform?.Close();
            if (StartWhenShown)
                Start();
        }
    }
}