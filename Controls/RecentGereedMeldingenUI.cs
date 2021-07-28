﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rpm.Misc;
using Rpm.Productie;
using Rpm.Various;

namespace Controls
{
    public partial class RecentGereedMeldingenUI : UserControl
    {
        public bool IsLoaded { get; private set; }
        public bool IsSyncing { get; private set; }
        //TimeSpan _TijdTerug = TimeSpan.FromDays(1);
        //public TimeSpan TijdTerug
        //{
        //    get => _TijdTerug;
        //    set
        //    {
        //        _TijdTerug = value;
        //        UpdateTime();
        //    } 
        //}


        public int SyncInterval { get; set; } = 10000;//3 min
        private bool _Enablesync;
        public bool EnableSync
        {
            get => _Enablesync;
            set
            {
                _Enablesync = value;
                if(value)
                    SyncBewerkingen();
            }
        }

        public ObjectListView ProductieLijst => productieListControl1.ProductieLijst;

        public int ItemCount => productieListControl1.ProductieLijst.Items.Count;
        private TijdEntry Bereik { get; set; }
        public RecentGereedMeldingenUI()
        {
            InitializeComponent();
            productieListControl1.ValidHandler = IsAllowed;
            productieListControl1.ItemCountChanged += ProductieListControl1_ItemCountChanged;
            productieListControl1.RemoveCustomItemIfNotValid = true;
        }

        private void ProductieListControl1_ItemCountChanged(object sender, EventArgs e)
        {
            OnItemCountChanged();
        }

        private void UpdateTijdPeriode(bool change)
        {
            if (InvokeRequired)
                this.Invoke(new MethodInvoker(()=> xUpdateTijdPeriode(change)));
            else xUpdateTijdPeriode(change);
        }

        private void xUpdateTijdPeriode(bool change)
        {
            if (Manager.Opties != null)
            {
                if (Manager.Opties.LastGereedStart.IsDefault())
                {
                    Manager.Opties.LastGereedStart = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                    change = true;
                }
                if(change)
                    xvanafgereed.SetValue(Manager.Opties.LastGereedStart);
                if (Manager.Opties.LastGereedStop.IsDefault())
                {
                    xtotgereed.SetValue(DateTime.Now);
                    xtotgereed.Checked = false;
                }
                else if(change)
                {
                    xtotgereed.SetValue(Manager.Opties.LastGereedStop);
                    xtotgereed.Checked = true;
                }
                Bereik = new TijdEntry()
                { Start = Manager.Opties.LastGereedStart, Stop = xtotgereed.Value };
            }
        }

        public void LoadBewerkingen()
        {
            if (IsLoaded) return;
            productieListControl1.DetachEvents();
            UpdateTijdPeriode(true);
            Manager.OnSettingsChanged -= Manager_OnSettingsChanged;
            Manager.OnSettingsChanged += Manager_OnSettingsChanged;
            InitRecenteGereedmeldingen();
            productieListControl1.InitEvents();           
            
        }

        private void Manager_OnSettingsChanged(object instance, Rpm.Settings.UserSettings settings, bool reinit)
        {
            if (settings != null)
            {
                try
                {
                    this.BeginInvoke(new MethodInvoker(() =>
                    {
                        bool changed = Bereik.Start != settings.LastGereedStart || (!settings.LastGereedStop.IsDefault() && settings.LastGereedStop != Bereik.Stop);
                        UpdateTijdPeriode(true);
                        //var dt = DateTime.Now;
                        //xhourvalue.SetValue(new DateTime(dt.Year, dt.Month, dt.Day, settings.LastRecentGereedTime.Hours,
                        //    settings.LastRecentGereedTime.Minutes, 0));
                        if (changed)
                            InitRecenteGereedmeldingen();
                    }));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void StopView()
        {
            productieListControl1.DetachEvents();
            productieListControl1.InitProductie(new List<Bewerking>(),true);
            IsSyncing = false;
            IsLoaded = false;
        }

        private void SyncBewerkingen()
        {
            if (IsSyncing || !EnableSync || IsDisposed || !IsLoaded) return;
            IsSyncing = true;
            Task.Run(() =>
            {
                //this.BeginInvoke(new MethodInvoker(async () =>
                //{
                while (IsSyncing && EnableSync && !IsDisposed && IsLoaded)
                {
                    try
                    {
                        Task.Delay(SyncInterval).Wait();
                        if (_iswaiting) continue;
                        InitRecenteGereedmeldingen();
                        //var prods = await Manager.Database.GetAllProducties(true, true, null, IsAllowed);
                        //if (prods != null && prods.Any())
                        //{
                        //    for (int i = 0; i < prods.Count; i++)
                        //    {
                        //        productieListControl1.UpdateBewerking(prods[i], null, null, null);
                        //        if (!IsSyncing || !EnableSync || IsDisposed || !IsLoaded) break;
                        //    }
                        //}
                        //var xbws = productieListControl1.ProductieLijst.Objects?.Cast<Bewerking>().ToList();
                        //if (xbws != null && xbws.Count > 0)
                        //{
                        //    var states = productieListControl1.GetCurrentViewStates();
                        //    for (int i = 0; i < xbws.Count; i++)
                        //    {
                        //        productieListControl1.UpdateBewerking(xbws[i]?.Parent, xbws, states, null);
                        //        if (!IsSyncing || !EnableSync || IsDisposed || !IsLoaded) break;
                        //    }
                        //}

                       
                        if (!IsSyncing || !EnableSync || IsDisposed || !IsLoaded) break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                //}));
            });
        }

        private void UpdateTime()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(xUpdateTime));
            else xUpdateTime();
        }

        private void xUpdateTime()
        {
            var dt = DateTime.Now;
            if (xtotgereed.Checked)
                dt = xtotgereed.Value;
            Bereik = new TijdEntry(xvanafgereed.Value, dt, null);
            if(Manager.Opties != null)
            {
                Manager.Opties.LastGereedStart = xvanafgereed.Value;
                if (xtotgereed.Checked)
                    Manager.Opties.LastGereedStop = xtotgereed.Value;
                else Manager.Opties.LastGereedStop = new DateTime();
            }
        }

        private Task InitRecenteGereedmeldingen()
        {
            return Task.Run(async () =>
            {
                if (_iswaiting) return;
                try
                {
                    DoWait();
                    var bewerkingen = await Manager.Database.GetBewerkingen(ViewState.Gereed, true, null, IsAllowed);
                    productieListControl1.InitProductie(bewerkingen, true);
                    IsLoaded = true;                   
                }
                catch (Exception e)
                {

                }
                if (EnableSync)
                    SyncBewerkingen();
                StopWait();
            });
        }

        private bool _iswaiting;
        private async void DoWait()
        {
            if (_iswaiting) return;
            _iswaiting = true;
            var value = "Producties laden";
            int cur = 0;
            while (_iswaiting && !IsDisposed)
            {
                int xcur = cur++;
                xstatus.Invoke(new MethodInvoker(() =>
                {
                    xstatus.Text = value.PadRight(value.Length + xcur, '.');
                    xstatus.Invalidate();
                }));
                await Task.Delay(350);
                if (cur > 5)
                    cur = 0;
            }

            _iswaiting = false;
        }

        private void StopWait()
        {
            _iswaiting = false;
            UpdateStatus();
        }

        private bool IsAllowed(object value, string filter)
        {
            UpdateTijdPeriode(false);
            if (value is ProductieFormulier form)
            {
                if (form.Bewerkingen == null || form.Bewerkingen.Length == 0) return false;
                return form.Bewerkingen.Any(x => IsAllowed(x, filter));
            }
            if (value is Bewerking bew)
            {
                if (string.IsNullOrEmpty(bew.ProductieNr) ||
                    bew.State != ProductieState.Gereed || !bew.IsAllowed(filter)) return false;
                
                if ((bew.DatumGereed < Bereik.Start || bew.DatumGereed > Bereik.Stop)) return false;
                return true;
            }

            return false;
        }

        public event EventHandler ItemCountChanged;

        private void UpdateStatus()
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                var bws = productieListControl1.Bewerkingen;
                int count = bws?.Count ?? 0;
                var x1 = count == 1 ? "Gereedmelding" : "Gereedmeldingen";
                var tijd = Bereik.Stop - Bereik.Start;
                var uur = tijd.Hours;
                var dagen = (int)tijd.TotalDays;
                var weken = dagen >= 7 ? (int)(dagen / 7) : 0;
                dagen = dagen - (weken * 7);
                var weekt = weken == 1 ? "week" : "weken";
                var dagt = dagen == 1 ? "dag" : "dagen";
                var xvals = new List<string>();
                if (weken > 0)
                    xvals.Add($"{weken} {weekt}");
                if (dagen > 0)
                    xvals.Add($"{dagen} {dagt}");
                if(uur > 0 || xvals.Count == 0)
                        xvals.Add($"{uur} uur");
                var xmsg = $"{count} {x1} van de afgelopen ";
                for (int i = 0; i < xvals.Count; i++)
                {
                   
                    if (i > 0)
                    {
                        if (i == xvals.Count - 1)
                            xmsg += $" en {xvals[i]}";
                        else xmsg += $", {xvals[i]}";
                    }
                    else
                        xmsg += xvals[i];

                }
                xstatus.Text = xmsg + ".";
            }));
        }

        protected virtual void OnItemCountChanged()
        {
            UpdateStatus();
            ItemCountChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void xupdatetijdb_Click(object sender, EventArgs e)
        {
            // var tijd = xhourvalue.Value.TimeOfDay;
            UpdateTime();
            UpdateTijdPeriode(true);
            await InitRecenteGereedmeldingen();
        }
    }
}
