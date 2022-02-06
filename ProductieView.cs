using AutoUpdaterDotNET;
using Forms.Aantal;
using Forms.ArtikelRecords;
using Forms.Excel;
using MetroFramework;
using MetroFramework.Forms;
using ProductieManager.Forms;
using ProductieManager.Properties;
using ProductieManager.Rpm.Misc;
using ProductieManager.Rpm.Various;
using Rpm.Controls;
using Rpm.DailyUpdate;
using Rpm.Mailing;
using Rpm.Misc;
using Rpm.Productie;
using Rpm.Settings;
using Rpm.SqlLite;
using Rpm.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Controls;
using Controls.TileView;
using Various;

namespace Forms
{
    public partial class ProductieView : Form
    {

        #region Mainform
        private string _bootDir;
        private PathWatcher _dbWatcher;
        /// <summary>
        /// De titel van de hoofdscherm
        /// </summary>
        public string MainAppTitle
        {
            get => this.Text;
            set => this.Text = value;
        }

        /// <summary>
        /// Update de titel en status text
        /// </summary>
        public void UpdateTitle()
        {
            string xuser = Manager.LogedInGebruiker == null ? "Niet Ingelogd" : $"Ingelogd als: {Manager.LogedInGebruiker.Username}";
            MainAppTitle = $"ProductieManager Versie {ProductVersion} {xuser}";
            xstatuslabel.Text = $@"Database: {Manager.AppRootPath}";
        }
        /// <summary>
        /// Of de hoofdscherm bezig is met laden
        /// </summary>
        public static bool IsLoading { get; private set; }
        private SplashScreen _splash;
        /// <summary>
        /// De timer die ingesteld staat om te kijken voor nieuwe updates
        /// </summary>
        public Timer Updatechecker;

        /// <summary>
        /// Maak een nieuwe hoofdscherm aan
        /// </summary>
        public ProductieView()
        {
            //InitBootDir(bootdir);
            InitializeComponent();
            IsLoading = true;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            Shown += Mainform_Shown;
            _specialRoosterWatcher = new Timer();
            _specialRoosterWatcher.Interval = 60000; //1 minuut;
            _specialRoosterWatcher.Tick += (x, y) => CheckForSpecialRooster(false);
            DailyMessage = new Daily
            {
                ImageList =
                {
                    ImageSize = new Size(128, 128)
                }
            };
            DailyMessage.DailyCreated += DailyMessage_DailyCreated;
            //StickyWindow.RegisterExternalReferenceForm(this);
            LoadManager();
        }

        private void LoadManager()
        {
            AutoUpdater.AppCastURL = "https://www.dropbox.com/s/3bnp0so9o1mj8at/UpdateInfo.xml?dl=1";
            AutoUpdater.AppTitle = "Productie Manager";
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            this.WindowState = FormWindowState.Minimized;
            Hide();
            Manager.OnSettingsChanged += ProductieView1_OnSettingsChanged;
            Manager.OnManagerLoaded += _manager_OnManagerLoaded;
            Manager.OnRemoteMessage += _manager_OnRemoteMessage;

            //Shown += Mainform_Shown;
            //Task.Run(new Action(productieView1.LoadForm));
            _splash = new SplashScreen(3000) { WindowState = FormWindowState.Normal };
            _splash.Shown += _splash_Shown;
            _splash.Closed += _splash_Closed;
            _splash.Show();
            _dbWatcher = new PathWatcher();
            _dbWatcher.PathLocationFound += _DbWatcher_PathLocationFound;
            _dbWatcher.PathLocationLost += _DbWatcher_PathLocationLost;
        }

        private void _splash_Closed(object sender, EventArgs e)
        {
            IsLoading = false;
            _splash?.Dispose();
            _splash = null;
            StartPosition = FormStartPosition.CenterParent;
            Application.DoEvents();
            this.InitLastInfo();
            this.Show();
            Select();
            BringToFront();
            UpdateTitle();
            ShowStartPagina();
            AutoUpdater.Start();
            Application.DoEvents();
            ShowStartupForms();
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            Exit(false);
        }

        /// <summary>
        /// Sluit de ProductieManager 
        /// </summary>
        /// <param name="restart">Of je de productiemanager wilt restarten</param>
        public static async void Exit(bool restart)
        {
            var proc = Process.GetCurrentProcess();
            if (proc.MainModule != null && restart)
                Process.Start(proc.MainModule.FileName);
            Manager.DefaultSettings?.SaveAsDefault();
            if (Manager.Opties != null)
            {
                bool cancel = false;
                var opties = Manager.Opties;
                Manager.SettingsChanging(null, ref opties, ref cancel);
                if (cancel) return;
                Manager.Opties = opties;
                await Manager.Opties.Save();
            }
            proc.Kill();
        }
        /// <summary>
        /// Een event die aangeeft of de hoofd database niet meer bestaat
        /// </summary>
        /// <param name="sender">De gene die dat heeft geconstateerd</param>
        /// <param name="e">Event informatie</param>
        private void _DbWatcher_PathLocationLost(object sender, EventArgs e)
        {
            this.Invoke(new Action(DoDbLocationLost));
        }

        private async void DoDbLocationLost()
        {
            try
            {
                _dbWatcher?.Stop();
                bool found = false;
                for (int i = 0; i < 5; i++)
                {
                    if (Directory.Exists(Manager.DefaultSettings.MainDB.RootPath))
                    {
                        found = true;
                        break;
                    }

                    await Task.Delay(400);
                }

                if (found) return;
                Dictionary<string, DialogResult> xbtns = new Dictionary<string, DialogResult>
                {
                    {"Herstart", DialogResult.OK},
                    {"Ga Offline", DialogResult.Cancel},
                    {"Afsluiten", DialogResult.No},
                    {"Kies DB", DialogResult.Yes}
                };
                var xrslt = XMessageBox.Show(this, $"Oorspronkelijke database kan niet geladen worden!\n\n" + " * Kies 'Herstart' als je de ProductieManager opnieuw wilt opstarten.\n" + " * Kies 'Offline' als je gewoon offline wilt werken.\n" + " * Kies anders voor een andere database of sluit de ProductieManager af.", "Database niet gevonden!", MessageBoxIcon.Warning, null, xbtns);
                if (xrslt == DialogResult.OK)
                {
                    Application.Restart();
                    return;
                }

                if (xrslt == DialogResult.No)
                {
                    this.Close();
                    return;
                }

                if (xrslt == DialogResult.Yes)
                {
                    DbPathChooser ps = new DbPathChooser();
                    if (ps.ShowDialog() == DialogResult.OK)
                    {
                        Manager.DefaultSettings.MainDB.RootPath = ps.SelectedPath;
                        Exit(true);
                        return;
                    }
                }

                LoadManager(Manager.DefaultSettings.TempMainDB.RootPath, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            _dbWatcher?.Start();
        }

        /// <summary>
        /// Of de hoofd database weer is gevonden
        /// </summary>
        /// <param name="sender">De gene die dat heeft geconstateerd</param>
        /// <param name="e">De event informatoie</param>
        private void _DbWatcher_PathLocationFound(object sender, EventArgs e)
        {
            try
            {
                Exit(true);
            }
            catch (Exception ex)
            {
                _dbWatcher.PathLost = true;
                Console.WriteLine(ex);
            }
        }

        private void InitBootDir(string path = null)
        {

            _bootDir = Application.StartupPath;
            Manager.AppRootPath = path ?? _bootDir;

            if (Manager.DefaultSettings != null)
            {
                var dbent = Manager.DefaultSettings.MainDB;
                var tempdbent = Manager.DefaultSettings.TempMainDB;
                dbent ??= new DatabaseUpdateEntry()
                {
                    Naam = "Main Boot Dir",
                    RootPath = Manager.DefaultSettings.MainDbPath ?? Application.StartupPath,
                    UpdateDatabases =
                        new List<DbType>()
                        {
                            DbType.Accounts,
                            DbType.Producties,
                            DbType.GereedProducties,
                            DbType.Medewerkers,
                            DbType.Opties
                        }
                };
                tempdbent ??= new DatabaseUpdateEntry()
                {
                    Naam = "Temp Boot Dir",
                    RootPath = Application.StartupPath,
                    UpdateDatabases =
                        new List<DbType>()
                        {
                            DbType.Accounts,
                            DbType.Producties,
                            DbType.GereedProducties,
                            DbType.Medewerkers,
                            DbType.Opties
                        }
                };
                string rootpath = null;
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        if (Directory.Exists(dbent.RootPath))
                        {
                            rootpath = dbent.RootPath;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    Application.DoEvents();
                }

                if (rootpath != null)
                {
                    _bootDir = rootpath;
                }
                else
                {
                    tempdbent.RootPath = _bootDir;
                    if (tempdbent.LastUpdated < dbent.LastUpdated || tempdbent.LastUpdated.IsDefault())
                    {
                        tempdbent.LastUpdated = DateTime.Now;
                    }

                }

                Manager.DefaultSettings.MainDB = dbent;
                Manager.DefaultSettings.TempMainDB = tempdbent;
                Manager.DefaultSettings.SaveAsDefault();
            }
        }

        private void _manager_OnRemoteMessage(RemoteMessage message, Manager instance)
        {
            ShowMessagePopup(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void ShowMessagePopup(RemoteMessage msg)
        {
            if (msg == null) return;
            if (Manager.Opties is { ToonLogNotificatie: false }) return;
            try
            {
                //if (InvokeRequired)
                //    BeginInvoke(new Action(msg.Notify));
                //else
                msg.Notify();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void _manager_OnManagerLoaded()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(DoLoaded));
            else DoLoaded();

        }

        private void DoLoaded()
        {
            if (_splash != null)
            {
                if (_splash.CanClose)
                    _splash.Close();
                else _splash.CanClose = true;

            }
            this.BeginInvoke(new Action(() => { _specialRoosterWatcher?.Start(); }));
            _dbWatcher.WatchPath(Manager.DefaultSettings.MainDB.UpdatePath, false, true);
            UpdateTitle();
        }

        private void _splash_Shown(object sender, EventArgs e)
        {
            IsLoading = true;
            Task.Factory.StartNew(new Action(Action));
            //BeginInvoke(new Action(Action));
        }

        private void Action()
        {
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    InitBootDir();
                    xversie.Text = $@"Versie {ProductVersion}";
                    LoadManager(_bootDir, true);
                    ShowUnreadMessage = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }));

        }

        private void Mainform_Shown(object sender, EventArgs e)
        {
            FormShown();
            Invalidate();
            Updatechecker = new Timer { Interval = 60000 };
            Updatechecker.Tick += _updatechecker_Tick;
            Updatechecker.Start();
        }

        private void _updatechecker_Tick(object sender, EventArgs e)
        {
            AutoUpdater.Start();
        }

        /// <summary>
        /// Een event voor als de hoofdscherm zichtbaar is
        /// </summary>
        public event EventHandler OnFormShown;

        private void CleanupUnusedDlls()
        {
            try
            {
                var asms = AppDomain.CurrentDomain.GetAssemblies();
                var names = new List<AssemblyName>();
                foreach (var asm in asms)
                {
                    var asmlibs = asm.GetReferencedAssemblies().Where(x =>
                        !x.FullName.ToLower().StartsWith("system") &&
                        !x.FullName.ToLower().StartsWith("mscorlib")).ToArray();
                    foreach (var asmlib in asmlibs)
                        if (!names.Any(x => x.Equals(asmlib)))
                            names.Add(asmlib);
                }

                var dlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll",
                    SearchOption.TopDirectoryOnly);
                foreach (var dll in dlls)
                {
                    var dllname = Path.GetFileNameWithoutExtension(dll);
                    var dllasm = names.FirstOrDefault(x =>
                        string.Equals(dllname, x.Name, StringComparison.CurrentCultureIgnoreCase));

                    if (dllasm == null)
                        File.Delete(dll);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProductieView1_OnSettingsChanged(object instance, UserSettings user, bool reinit)
        {
            try
            {
                BeginInvoke(new Action(UpdateTitle));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void mynotifyicon_Click()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void openenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void sluitenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region "Mainform"

        private void Mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                this.SetLastInfo();
                try
                {
                    var forms = Application.OpenForms;
                    for (int i = 0; i < forms.Count; i++)
                    {
                        var open = forms[i];
                        if (string.IsNullOrEmpty(open.Name) || string.Equals(this.Name, open.Name, StringComparison.CurrentCultureIgnoreCase)) continue;
                        open.Close();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

                SaveLayouts();
                Manager.DefaultSettings?.SaveAsDefault();
                Manager.OnSettingsChanged -= ProductieView1_OnSettingsChanged;
                Manager.OnManagerLoaded -= _manager_OnManagerLoaded;
                Manager.OnRemoteMessage -= _manager_OnRemoteMessage;
                Manager.ProductieProvider?.StopSync();
                Manager.ProductieProvider?.DisableOfflineDb();
                if (Manager.LogedInGebruiker != null)
                    Manager.LogOut(this, false);
                else Manager.SaveSettings(Manager.Opties, false, false, true);
                DetachEvents();
                //  _updatechecker?.Stop();
                // _updatechecker = null;
                //Manager.Database?.Dispose();
                //ProductieView._manager?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Mainform_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && Manager.Opties is { MinimizeToTray: true })
            {
                this.Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.Text = $@"ProductieManager Versie {Application.ProductVersion}";
                notifyIcon1.ShowBalloonTip(5000);
            }
        }

        #endregion "Mainform"

        #region IMain Interface
        /// <summary>
        /// Toon de hoofdscherm
        /// </summary>
        public void ShowForm()
        {
            Show();
        }

        /// <summary>
        /// Geef aan dat de hoofdscherm zichtbaar is
        /// </summary>
        protected void FormShown()
        {
            OnFormShown?.Invoke(this, EventArgs.Empty);
        }

        #endregion IMain Interface

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
        #endregion
        private void DailyMessage_DailyCreated(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(ShowDaily));
            else ShowDaily();
        }

        private void ShowDaily()
        {
            try
            {
                if (DailyMessage.CreatedDailies.Count > 0)
                {
                    var xenum = Application.OpenForms.GetEnumerator();
                    var xremove = new List<Form_Alert>();
                    while (xenum.MoveNext())
                    {
                        if (xenum.Current is Form_Alert alert)
                        {
                            xremove.Add(alert);
                        }
                    }
                    xremove.ForEach(x =>
                    {
                        x.Close();
                        x.Dispose();
                    });
                    var xf = new DailyMessageForm(DailyMessage);
                    xf.Height = (80 + (DailyMessage.CreatedDailies.Count * 250));
                    if (xf.Height > 850)
                        xf.Height = 850;
                    xf.Width = DailyMessage.CreatedDailies.Count > 1 ? 1100 : 850;
                    xf.HtmlText = string.Join("\r\n", DailyMessage.CreatedDailies);
                    xf.ShowDialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public event EventHandler OnFormLoaded;

        protected void FormLoaded()
        {
            OnFormLoaded?.Invoke(this, EventArgs.Empty);
        }

        #region Variables

        private readonly Timer _specialRoosterWatcher;
        public static Manager _manager;

        public int ProductieRefreshInterval { get; set; } = 10000; // 10 sec

        public bool ProductieSyncEnabled { get; set; }
        //private PersoneelsForm _persform;
        //private LogForm _logform;
        //private AlleStoringen _storingen;
        //private AlleVaardigheden _vaardigeheden;
        //private RangeCalculatorForm _rangeform;

        private static readonly List<StartProductie> _formuis = new();
        private static readonly List<ProductieLijstForm> _Productelijsten = new();
        private static Producties _producties;
        private static ProductieLijsten _productielijstdock;
        private static LogForm _logform;
        private static RangeCalculatorForm _calcform;
        private static ChatForm _chatform;
        private static OpmerkingenForm _opmerkingform;
        private static ArtikelsForm _ArtikelsForm;
        private static PersoneelsForm _PersoneelForm;
        private static ProductieOverzichtForm _ProductieOverzicht;
        private static WerkplaatsIndeling _WerkplaatsIndeling;
        private static MetroForm _berekenverbruik;

        private readonly Daily DailyMessage;
        public bool ShowUnreadMessage { get; set; }

        // [NonSerialized] private Opties _opties;

        //private string[] _groups = null;

        #endregion Variables

        #region Manager

        private async void InitManager(string path, bool autologin, bool disposeold)
        {
            try
            {
                if (_manager == null || disposeold)
                {
                    _manager?.Dispose();
                    //if (_manager == null)
                    _manager = new Manager(false);
                }

                DetachEvents();

                _manager.InitManager();
                //xproductieListControl1.InitProductie(false, true, true, true, false, false);
                
                takenManager1.InitManager();
               // werkPlekkenUI1.InitUI(_manager);
                InitEvents();
                await _manager.Load(path, autologin, true, true);
                //xbewerkingListControl.InitProductie(true, true, true, true, false, false);
                if (Manager.Opmerkingen != null)
                    Manager.Opmerkingen.OnOpmerkingenChanged += Opmerkingen_OnOpmerkingenChanged;
                // _manager.StartMonitor();
                FormLoaded();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void LoadManager(string path, bool disposeOld, bool autologin = true)
        {
            InitManager(path, autologin, disposeOld);
        }

        public void ShowStartupForms()
        {
            try
            {
                if (this.Disposing || this.IsDisposed) return;
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        //CheckForSyncDatabase();
                        //CheckForUpdateDatabase();
                        CheckForPreview(false, true);
                        CheckForSpecialRooster(true);
                        LoadStartedProducties();
                        //LoadProductieLogs();
                        //RunProductieRefresh();
                        UpdateKlachtButton();
                        UpdateVerpakkingenButton();
                        Manager.ArtikelRecords?.CheckForOpmerkingen(true);
                        InitDBCorupptedMonitor();
                        DailyMessage.CreateDaily();
                        UpdateUnreadMessages(null);
                        UpdateUnreadOpmerkingen();
                        //UpdateAllLists();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }));


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private TileMainView _mainpage;

        private void ShowStartPagina()
        {
            try
            {
                if (_mainpage == null)
                {
                    _mainpage = new TileMainView();
                    _mainpage.Text = "StartPagina";
                    _mainpage.FormClosed += (x, y) =>
                    {
                        _mainpage?.Dispose();
                        _mainpage = null;
                    };
                }

                _mainpage.Show(dockPanel1);
                if (_mainpage.WindowState == FormWindowState.Minimized)
                    _mainpage.WindowState = FormWindowState.Normal;
                mainMenu1.BringToFront();
                takenManager1.BringToFront();
                dockPanel1.BringToFront();
                _mainpage.BringToFront();
                _mainpage.Focus();
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void InitEvents()
        {
            Manager.OnSettingsChanged += _manager_OnSettingsChanged;
            //Manager.OnProductiesLoaded += Manager_OnProductiesChanged;
            Manager.OnLoginChanged += _manager_OnLoginChanged;
            Manager.OnSettingsChanging += _manager_OnSettingsChanging;
            Manager.OnFormulierActie += Manager_OnFormulierActie;
            //Manager.DbUpdater.DbEntryUpdated += DbUpdater_DbEntryUpdated;
            //Manager.OnDbBeginUpdate += Manager_OnDbBeginUpdate;
            //Manager.OnDbEndUpdate += Manager_OnDbEndUpdate;
            Manager.OnManagerLoaded += _manager_OnManagerLoaded;
            Manager.FilterChanged += Manager_FilterChanged;
            ProductieChat.MessageRecieved += ProductieChat_MessageRecieved;
            ProductieChat.GebruikerUpdate += ProductieChat_GebruikerUpdate;
            MultipleFileDb.CorruptedFilesChanged += MultipleFileDb_CorruptedFilesChanged;

            Manager.KlachtChanged += Klachten_KlachtChanged;
            Manager.KlachtDeleted += Klachten_KlachtChanged;

            Manager.VerpakkingChanged += Manager_VerpakkingChanged; 
            Manager.VerpakkingDeleted += Manager_VerpakkingDeleted;

            Manager.RequestRespondDialog += Manager_RequestRespondDialog;

            _manager.OnShutdown += _manager_OnShutdown;
            //xproductieListControl1.InitEvents();
            //xbewerkingListControl.InitEvents();
            //xproductieListControl1.ItemCountChanged += XproductieListControl1_ItemCountChanged;
            //xbewerkingListControl.ItemCountChanged += XproductieListControl1_ItemCountChanged;
           // recentGereedMeldingenUI1.ItemCountChanged += XproductieListControl1_ItemCountChanged;
            //xproductieListControl1.SelectedItemChanged += XproductieListControl1_SelectedItemChanged;
            //werkPlekkenUI1.InitEvents();
           // werkPlekkenUI1.OnRequestOpenWerk += WerkPlekkenUI1_OnRequestOpenWerk;
           // werkPlekkenUI1.OnPlekkenChanged += WerkPlekkenUI1_OnPlekkenChanged;
        }

        private DialogResult Manager_RequestRespondDialog(object sender, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, string[] chooseitems = null, Dictionary<string, DialogResult> custombuttons = null, Image customImage = null, MetroColorStyle style = MetroColorStyle.Default)
        {
            var xret = DialogResult.Cancel;
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                    xret = DoOpmerking(sender, message, title, buttons, icon, chooseitems, custombuttons, customImage, style)));
            else xret = DoOpmerking(sender, message, title, buttons, icon, chooseitems, custombuttons, customImage, style);
            return xret;
        }

        private DialogResult DoOpmerking(object sender, string message, string title, MessageBoxButtons buttons,
            MessageBoxIcon icon, string[] chooseitems = null, Dictionary<string, DialogResult> custombuttons = null,
            Image customImage = null, MetroColorStyle style = MetroColorStyle.Default)
        {
            var xmsg = new XMessageBox();
            xmsg.StartPosition = FormStartPosition.CenterParent;
            return xmsg.ShowDialog(this,message, title, buttons, icon, chooseitems, custombuttons, customImage, style);
        }

        public void DetachEvents()
        {
            Manager.OnSettingsChanged -= _manager_OnSettingsChanged;
            Manager.OnFormulierActie -= Manager_OnFormulierActie;
            // Manager.OnProductiesLoaded -= Manager_OnProductiesChanged;
            //Manager.DbUpdater.DbEntryUpdated -= DbUpdater_DbEntryUpdated;
            Manager.OnLoginChanged -= _manager_OnLoginChanged;
            Manager.OnSettingsChanging -= _manager_OnSettingsChanging;
            Manager.FilterChanged -= Manager_FilterChanged;
            if (Manager.Opmerkingen != null)
                Manager.Opmerkingen.OnOpmerkingenChanged -= Opmerkingen_OnOpmerkingenChanged;
            Manager.RequestRespondDialog -= Manager_RequestRespondDialog;
            ProductieChat.MessageRecieved -= ProductieChat_MessageRecieved;
            ProductieChat.GebruikerUpdate -= ProductieChat_GebruikerUpdate;
            MultipleFileDb.CorruptedFilesChanged -= MultipleFileDb_CorruptedFilesChanged;

            //Manager.OnDbBeginUpdate -= Manager_OnDbBeginUpdate;
            //Manager.OnDbEndUpdate -= Manager_OnDbEndUpdate;
            Manager.OnManagerLoaded -= _manager_OnManagerLoaded;

            Manager.KlachtChanged -= Klachten_KlachtChanged;
            Manager.KlachtDeleted -= Klachten_KlachtChanged;

            Manager.VerpakkingChanged -= Manager_VerpakkingChanged;
            Manager.VerpakkingDeleted -= Manager_VerpakkingDeleted;
            if (_manager != null)
                _manager.OnShutdown -= _manager_OnShutdown;
            //xproductieListControl1.DetachEvents();
            //xbewerkingListControl.DetachEvents();
            ////productieListControl1.ItemCountChanged -= XproductieListControl1_ItemCountChanged;
            //xbewerkingListControl.ItemCountChanged -= XproductieListControl1_ItemCountChanged;
            //recentGereedMeldingenUI1.ItemCountChanged -= XproductieListControl1_ItemCountChanged;
            //werkPlekkenUI1.DetachEvents();
            //werkPlekkenUI1.OnRequestOpenWerk -= WerkPlekkenUI1_OnRequestOpenWerk;
            //werkPlekkenUI1.OnPlekkenChanged -= WerkPlekkenUI1_OnPlekkenChanged;
        }

        public void SaveLayouts()
        {
            //xbewerkingListControl.SaveColumns(false);
            //recentGereedMeldingenUI1.productieListControl1.SaveColumns(false);
        }

        private void Manager_VerpakkingDeleted(object sender, EventArgs e)
        {
           UpdateVerpakkingenButton();
        }

        private void Manager_VerpakkingChanged(object sender, EventArgs e)
        {
            UpdateVerpakkingenButton();
        }

        private void Klachten_KlachtChanged(object sender, EventArgs e)
        {
            UpdateKlachtButton();
        }

        private void UpdateKlachtButton()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(xUpdateKlachtButton));
            else xUpdateKlachtButton();
        }

        private void UpdateVerpakkingenButton()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(xUpdateVerpakkingenButton));
            else xUpdateVerpakkingenButton();
        }

        private void xUpdateKlachtButton()
        {
            try
            {
                var xkl = Manager.Klachten?.GetUnreadKlachten();
                if (xkl is {Count: > 0})
                {
                    string cnt = xkl.Count > 99 ? "99+" : xkl.Count.ToString();
                    int fs = cnt.Length < 2 ? 16 : cnt.Length < 3 ? 14 : cnt.Length < 4 ? 12 : 10;
                    var ximg = GraphicsExtensions.DrawUserCircle(new Size(32, 32), Brushes.White,
                        cnt,
                        new Font("Ariel", fs, FontStyle.Bold), Color.DarkRed);
                    xklachten.Image = Resources.Leave_80_icon_icons_com_57305.CombineImage(ximg, 1.75);
                    var xopen = Application.OpenForms["KlachtenForm"];
                    if (xopen != null) return;
                    var x0 = xkl.Count == 1 ? "is" : "zijn";
                    var x1 = xkl.Count == 1 ? "klacht" : "klachten";
                    if (XMessageBox.Show(this,$"Er {x0} {xkl.Count} nieuwe {x1}!\n\n" +
                                         $"Nu bekijken?", $"Nieuwe {x1}", MessageBoxButtons.YesNo, Resources.Leave_80_icon_icons_com_57305_128x128, MetroColorStyle.Red) == DialogResult.Yes)
                    {
                        xklachten_Click(this, EventArgs.Empty);
                    }
                }
                else
                {
                    xkl = Manager.Klachten?.GetAlleKlachten();
                    if (xkl is {Count: > 0})
                    {
                        var cnt = xkl.Count > 99 ? "99+" : xkl.Count.ToString();
                        int fs = cnt.Length < 2? 16: cnt.Length < 3? 14 : cnt.Length < 4? 12 : 10;
                        var ximg = GraphicsExtensions.DrawUserText(new Size(32, 32), Brushes.LightCoral,
                            cnt,
                            new Font("Ariel", fs, FontStyle.Bold));
                        xklachten.Image = Resources.Leave_80_icon_icons_com_57305.CombineImage(ximg, 1.75);
                    }
                    else
                        xklachten.Image = Resources.Leave_80_icon_icons_com_57305;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void xUpdateVerpakkingenButton()
        {
            try
            {
                var xkl = Manager.Verpakkingen?.Database?.Count()??0;
                if (xkl > 0)
                {
                    string cnt = xkl > 99 ? "99+" : xkl.ToString();
                    int fs = cnt.Length < 2 ? 16 : cnt.Length < 3 ? 14 : cnt.Length < 4 ? 12 : 10;
                    var ximg = GraphicsExtensions.DrawUserCircle(new Size(32, 32), Brushes.White,
                        cnt,
                        new Font("Ariel", fs, FontStyle.Bold), Color.DarkRed);
                    xverpakkingen.Image = Resources.Box_1_35524.CombineImage(ximg, 1.75);
                }
                else
                {
                    xverpakkingen.Image = Resources.Box_1_35524;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void MultipleFileDb_CorruptedFilesChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new MethodInvoker(UpdateCorruptedButtonImage));
                else UpdateCorruptedButtonImage();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void UpdateCorruptedButtonImage()
        {
            try
            {
                var xcorrupted = MultipleFileDb.CorruptedFilePaths;
                if (xcorrupted.Count > 0)
                {
                    var ximg = GraphicsExtensions.DrawUserCircle(new Size(32, 32), Brushes.White,
                        xcorrupted.Count.ToString(),
                        new Font("Ariel", 16, FontStyle.Bold), Color.DarkRed);
                    xcorruptedfilesbutton.Image = Resources.error_notification_32x32.CombineImage(ximg, 1.75);
                }
                else
                {
                    xcorruptedfilesbutton.Image = Resources.error_notification_32x32;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Opmerkingen_OnOpmerkingenChanged(object sender, EventArgs e)
        {
          UpdateUnreadOpmerkingen();
        }

        private void Manager_FilterChanged(object sender, EventArgs e)
        {
            try
            {
                //BeginInvoke(new MethodInvoker(() => werkPlekkenUI1.LoadPlekken(true)));
                //BeginInvoke(new MethodInvoker(recentGereedMeldingenUI1.LoadBewerkingen));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void XproductieListControl1_ItemCountChanged(object sender, EventArgs e)
        {
            UpdateTabCounts();
        }

        private void ProductieChat_GebruikerUpdate(UserChat user)
        {
            UpdateUnreadMessages(user);
        }

        private void ProductieChat_MessageRecieved(ProductieChatEntry message)
        {
            UpdateUnreadMessages(message?.Afzender);
        }

      

        private DialogResult _manager_OnShutdown(Manager instance, ref TimeSpan verlengtijd)
        {
            var af = new AfsluitPromp();
            var res = af.ShowDialog();
            if (res == DialogResult.OK) verlengtijd = af.VerlengTijd;
            return res;
        }

        private void _manager_OnSettingsChanging(object instance, ref UserSettings settings, ref bool cancel)
        {
            if (IsDisposed || Disposing) return;
            try
            {
                var xsettings = settings;
                if (InvokeRequired)
                    Invoke(new Action(() =>
                    {
                        try
                        {
                            //xsettings.ViewDataProductieState = xproductieListControl1.ProductieLijst.SaveState();
                            //xsettings.ViewDataBewerkingenState = xbewerkingListControl.ProductieLijst.SaveState();
                            //xsettings.ViewDataWerkplekState = werkPlekkenUI1.xwerkpleklist.SaveState();
                            //xsettings.ViewDataRecentProductieState =
                            //    recentGereedMeldingenUI1.ProductieLijst.SaveState();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }));
                else
                    try
                    {
                        //xsettings.ViewDataProductieState = xproductieListControl1.ProductieLijst.SaveState();
                        //xsettings.ViewDataBewerkingenState = xbewerkingListControl.ProductieLijst.SaveState();
                        //xsettings.ViewDataWerkplekState = werkPlekkenUI1.xwerkpleklist.SaveState();
                        //xsettings.ViewDataRecentProductieState = recentGereedMeldingenUI1.ProductieLijst.SaveState();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                settings = xsettings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void _manager_OnSettingsChanged(object instance, UserSettings settings, bool init)
        {
            try
            {
                if (IsDisposed || Disposing) return;
                mainMenu1.OnSettingChanged(instance, settings, init);
                if (!init) return;
                BeginInvoke(new MethodInvoker(() =>
                {
                    if (IsDisposed || Disposing) return;
                    try
                    {
                        var name = Manager.Opties == null ? "Default" : Manager.Opties.Username;
                        Text = @$"ProductieManager [{name}]";


                        //if (Manager.Opties?._viewproddata != null)
                        //{
                        //    xproductieListControl1.ProductieLijst.RestoreState(Manager.Opties?.ViewDataProductieState);
                        //    xproductieListControl1.ProductieLijst.Columns.Remove(
                        //        xproductieListControl1.ProductieLijst.Columns["Naam"]);
                        //}

                        //if (Manager.Opties?._viewbewdata != null)
                        //    xbewerkingListControl.ProductieLijst.RestoreState(Manager.Opties.ViewDataBewerkingenState);
                        //if (Manager.Opties?._viewwerkplekdata != null)
                        //    werkPlekkenUI1.xwerkpleklist.RestoreState(Manager.Opties.ViewDataWerkplekState);
                        //if (Manager.Opties?._viewrecentproddata != null)
                        //    recentGereedMeldingenUI1.ProductieLijst.RestoreState(Manager.Opties
                        //        .ViewDataRecentProductieState);
                        var xrooster = mainMenu1.GetButton("xroostermenubutton");
                        if (xrooster != null)
                            xrooster.Image = Manager.Opties?.TijdelijkeRooster == null
                                ? Resources.schedule_32_32
                                : Resources.schedule_32_32.CombineImage(Resources.exclamation_warning_15590, 1.75);

                        //LoadFilter();
                        _manager.SetSettings(settings);
                        //werkPlekkenUI1.LoadPlekken(true);
                        CheckForSpecialRooster(false);
                        //recentGereedMeldingenUI1.LoadBewerkingen();
                        this.Invalidate();
                        //Manager.Taken?.StartBeheer();
                        //if (Manager.IsLoaded)
                        //    CheckForSpecialRooster(true);
                        if (_specialRoosterWatcher != null && !_specialRoosterWatcher.Enabled)
                            _specialRoosterWatcher.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public event LogInChangedHandler OnLoginChanged;

        private void _manager_OnLoginChanged(UserAccount user, object instance)
        {
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed || Disposing) return;
                    xloginb.Image = user != null ? Resources.Logout_37127__1_ : Resources.Login_37128__1_;
                    xcorruptedfilesbutton.Visible = user is {AccesLevel: AccesType.Manager};
                    xMissingTekening.Visible = user is { AccesLevel: AccesType.Manager };
                    OnLoginChanged?.Invoke(user, instance);
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitDBCorupptedMonitor()
        {
            try
            {
                if (Manager.Database?.ProductieFormulieren?.MultiFiles != null)
                    Manager.Database.ProductieFormulieren.MultiFiles.MonitorCorrupted = true;
                if (Manager.Database?.GereedFormulieren?.MultiFiles != null)
                    Manager.Database.GereedFormulieren.MultiFiles.MonitorCorrupted = true;
                if (Manager.Database?.UserAccounts?.MultiFiles != null)
                    Manager.Database.UserAccounts.MultiFiles.MonitorCorrupted = true;
                if (Manager.Database?.AllSettings?.MultiFiles != null)
                    Manager.Database.AllSettings.MultiFiles.MonitorCorrupted = true;
                if (Manager.Database?.PersoneelLijst?.MultiFiles != null)
                    Manager.Database.PersoneelLijst.MultiFiles.MonitorCorrupted = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DoActie(object[] values, MainAktie type)
        {
            var flag = values is {Length: > 0};
            var xforms = Application.OpenForms.Cast<Form>().Where(x => x is Form_Alert).ToList();
            xforms.ForEach(x=> x.Close());
            switch (type)
            {
                case MainAktie.OpenProductie:
                    if (flag)
                    {
                        var form =
                            (ProductieFormulier) values.FirstOrDefault(x => x is ProductieFormulier);
                        var bew = (Bewerking) values.FirstOrDefault(x => x is Bewerking);
                        if (form != null)
                            ShowProductieForm(this,form, true, bew);
                    }

                    break;
                case MainAktie.OpenIndeling:
                    if (flag)
                    {
                        var form =
                            (ProductieFormulier) values.FirstOrDefault(x => x is ProductieFormulier);
                        ShowWerkplekken(form);
                    }

                    break;
                case MainAktie.OpenProductieWijziging:
                    if (flag)
                    {
                        var form =
                            (ProductieFormulier) values.FirstOrDefault(x => x is ProductieFormulier);
                        ShowProductieSettings(form);
                    }

                    break;
                case MainAktie.OpenInstellingen:
                    ShowOptieWidow();
                    break;
                case MainAktie.OpenRangeSearcher:
                    ShowCalculatieWindow();
                    break;
                case MainAktie.OpenPersoneel:
                    ShowPersoneelWindow();
                    break;
                case MainAktie.OpenStoringen:
                    if (flag)
                    {
                        var bew = (Bewerking) values.FirstOrDefault(x => x is Bewerking);
                        if (bew != null)
                            ShowBewStoringen(bew);
                    }

                    break;
                case MainAktie.OpenAlleStoringen:
                    ShowOnderbrekeningenWidow();
                    break;
                case MainAktie.OpenVaardigheden:
                    if (flag)
                    {
                        var per = (Personeel) values.FirstOrDefault(x => x is Personeel);
                        if (per != null)
                            ShowPersoonVaardigheden(per);
                    }

                    break;
                case MainAktie.OpenAlleVaardigheden:
                    ShowAlleVaardighedenWidow();
                    break;
                case MainAktie.OpenAantalGemaaktProducties:
                    if (values.FirstOrDefault() is List<Bewerking> bws && values.LastOrDefault() is int mins)
                        DoAantalGemaakt(this,bws, mins);
                    break;
                case MainAktie.StartBewerking:
                    if (values.FirstOrDefault() is Bewerking bew2)
                        ProductieListControl.StartBewerkingen(this, new Bewerking[] {bew2});
                    break;
                case MainAktie.StopBewerking:
                    if (values.FirstOrDefault() is Bewerking bew3)
                        _= bew3.StopProductie(true,true);
                    break;
            }
        }

        private void Manager_OnFormulierActie(object[] values, MainAktie type)
        {
            if (IsDisposed || Disposing) return;
            try
            {
                if (InvokeRequired)
                    Invoke(new MethodInvoker(() =>
                    {
                        DoActie(values, type);
                    }));
                else DoActie(values, type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Manager

        #region Main Menu Methods

        private void DoCreateExcel()
        {
            var f = new CreateExcelForm();
            f.ShowDialog();
        }

        private async void DoQuickProductie()
        {
            if (Manager.BewerkingenLijst == null || Manager.Database?.ProductieFormulieren == null)
            {
                XMessageBox.Show(this,"Kan geen productie aanmaken, omdat de Database niet is geladen.", "Database niet geladen!", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            var xnewform = new NiewProductieForm();
            var result = xnewform.ShowDialog();
            if (result == DialogResult.Cancel) return;

            var created = xnewform.CreatedFormulier;
            if (created != null)
            {
                //await created.UpdateForm(true, false);
                if (result == DialogResult.Yes && created.Bewerkingen.Length > 0)
                    ProductieListControl.StartBewerkingen(this, created.Bewerkingen);
                else
                    await created.UpdateForm(true, false);
                SelectProductieItem(created);
            }
        }

        private async void DoOnderbreking()
        {
            var message =
                "Wat zou je willen doen?\n" +
                "Wil je een storing/onderbreking bekijken, toevoegen of wijzigen?";
            var bttns = new Dictionary<string, DialogResult>();
            bttns.Add("Annuleren", DialogResult.Cancel);
            bttns.Add("Wijzigen", DialogResult.No);
            bttns.Add("Toevoegen", DialogResult.Yes);
            bttns.Add("Bekijken", DialogResult.Ignore);


            var res = XMessageBox.Show(this, message, "Onderbreking", MessageBoxButtons.OK, MessageBoxIcon.Question, null,
                bttns);
            if (res == DialogResult.Cancel) return;
            var prods = await Manager.GetProducties(new[] {ViewState.Gestart, ViewState.Gestopt}, true, false,null);
            var plekken = new List<WerkPlek>();
            switch (res)
            {
                case DialogResult.Ignore:
                case DialogResult.Yes:

                    prods.ForEach(x =>
                    {
                        var xplekken = x.GetAlleWerkplekken();
                        if (res == DialogResult.Ignore)
                            xplekken = xplekken.Where(w => w.Storingen.Count > 0).ToList();
                        if (xplekken.Count > 0)
                            plekken.AddRange(xplekken);
                    });
                    if (plekken.Count == 0)
                    {
                        var xvalue = res == DialogResult.Ignore
                            ? "onderbrekeningen van te bekijken"
                            : "een onderbreking aan toe te voegen";
                        XMessageBox.Show(this, $"Er zijn geen  aangemaakte werkplekken om {xvalue}.", "Geen Werkplekken",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    break;
                case DialogResult.No:
                    prods.ForEach(x =>
                    {
                        var xplekken = x.GetAlleWerkplekken();
                        if (xplekken.Count > 0)
                            foreach (var plek in xplekken)
                                if (plek.Storingen.Any(x => !x.IsVerholpen))
                                    plekken.Add(plek);
                    });
                    if (plekken.Count == 0)
                        XMessageBox.Show(this, $"Er zijn geen openstaande onderbrekeningen om te wijzigen.", "Onderbreking",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }

            if (plekken.Count > 0)
            {
                var selector = new WerkPlekChooser(plekken, null);
                if (selector.ShowDialog() == DialogResult.OK)
                {
                    var plek = selector.Selected;
                    if (plek == null) return;
                    NewStoringForm storingform = null;
                    switch (res)
                    {
                        case DialogResult.Yes:
                            storingform = new NewStoringForm(plek);
                            break;
                        case DialogResult.No:
                            var storingen = plek.Storingen.Where(x => !x.IsVerholpen).ToList();
                            if (storingen.Count == 0) return;
                            if (storingen.Count > 1)
                            {
                                var msg = $"Er zijn {storingen.Count} openstaande onderbrekeningen beschikbaar.\n" +
                                          "Kies een openstaande onderbreking om te wijzigen.";

                                var msgbox = new XMessageBox();
                                if (msgbox.ShowDialog(this, msg, "Kies Onderbreking", MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Information,
                                    storingen.Select(x => x.ToString()).ToArray()) == DialogResult.OK)
                                {
                                    var selected = msgbox.SelectedValue;
                                    if (selected != null)
                                    {
                                        var storing = storingen.FirstOrDefault(x => x.Equals(selected));
                                        if (storing != null) storingform = new NewStoringForm(plek, storing);
                                    }
                                }
                            }
                            else
                            {
                                storingform = new NewStoringForm(plek, storingen[0]);
                            }

                            break;
                    }

                    if (storingform != null)
                    {
                        if (storingform.ShowDialog() == DialogResult.OK)
                        {
                            plek.UpdateStoring(storingform.Onderbreking);
                            if (plek.Werk != null)
                            {
                                await plek.Werk?.UpdateBewerking(null,
                                    $"Onderbreking aangepast op {storingform.Onderbreking.Path}");
                                RemoteProductie.SendStoringMail(storingform.Onderbreking, plek.Werk);
                            }

                            var msg = $"Onderbreking {plek.Path}\n" +
                                      $"{storingform.Onderbreking.StoringType} is ";

                            switch (res)
                            {
                                case DialogResult.Yes:
                                    msg += "toegevoegd.";
                                    break;
                                case DialogResult.No:
                                    msg += "gewijzigd.";
                                    break;
                            }

                            XMessageBox.Show(this, msg, "Onderbreking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else if (res == DialogResult.Ignore)
                    {
                        var sts = new StoringForm(plek);
                        sts.ShowDialog();
                    }
                }
            }
        }

        private void DoOpenProductiePdf()
        {
            //switch (metroTabControl.SelectedIndex)
            //{
            //    case 0: 
            //        if (xbewerkingListControl.SelectedItem is Bewerking bew) bew.Parent?.OpenProductiePdf();
            //        break;
            //    case 2: //werkplekken
            //        var plek = werkPlekkenUI1.SelectedWerkplek;
            //        plek?.Werk?.Parent?.OpenProductiePdf();
            //        break;
            //}
        }

        private void DoPdfEnabled()
        {
            //mainMenu1.Enable("xbekijkproductiepdf", false);
            //switch (metroTabControl.SelectedIndex)
            //{
            //    case 0: //producties
            //        if (xbewerkingListControl.SelectedItem is Bewerking bew)
            //            mainMenu1.Enable("xbekijkproductiepdf",
            //                bew.Parent != null && bew.Parent.ContainsProductiePdf());
            //        break;
            //    case 2: //werkplekken
            //        var plek = werkPlekkenUI1.SelectedWerkplek;
            //        if (plek != null)
            //            mainMenu1.Enable("xbekijkproductiepdf",
            //                plek.Werk?.Parent != null && plek.Werk.Parent.ContainsProductiePdf());
            //        break;
            //}
        }

        private void SelectProductieItem(object item)
        {
            //if (item is ProductieFormulier form)
            //{
            //    //xproductieListControl1.SelectedItem = form;
            //    metroTabControl.SelectedIndex = 0;
            //}
            //else if (item is Bewerking bew)
            //{
            //    xbewerkingListControl.SelectedItem = bew;
            //    metroTabControl.SelectedIndex = 0;
            //}
            //else if (item is WerkPlek plek)
            //{
            //    werkPlekkenUI1.SelectedWerkplek = plek;
            //    metroTabControl.SelectedIndex = 1;
            //}
        }

        private async void DoEigenRooster()
        {
            try
            {
                if (Manager.Opties == null)
                    throw new Exception(
                        "Opties zijn niet geladen en kan daarvoor geen rooster aanpassen.\n\nRaadpleeg Ihab a.u.b.");
                //if (Manager.Opties.TijdelijkeRooster != null)
                //{
                //    var xvalue = "\nMet een bereik:\n";
                //    if (Manager.Opties.TijdelijkeRooster.GebruiktVanaf)
                //        xvalue += $"Vanaf {Manager.Opties.TijdelijkeRooster.Vanaf} ";
                //    if (Manager.Opties.TijdelijkeRooster.GebruiktTot)
                //        xvalue += $"Tot {Manager.Opties.TijdelijkeRooster.Tot}\n";
                //    if (Manager.Opties.TijdelijkeRooster.GebruiktVanaf || Manager.Opties.TijdelijkeRooster.GebruiktTot)
                //        xrstr += xvalue;
                //}

                //var message = "Wat voor rooster zou je willen gebruiken voor alle producties?\n\n" +
                //              $"Momenteel gebruik je {xrstr}.";
                //var dialog = XMessageBox.Show(message, "Eigen Rooster",
                //    MessageBoxButtons.OK, MessageBoxIcon.Question, null, bttns);
               // if (dialog == DialogResult.Cancel) return;
                var xold = Manager.Opties?.GetWerkRooster() ?? Rooster.StandaartRooster();
                var roosterform = new RoosterForm(Manager.Opties.TijdelijkeRooster,
                           "Kies een rooster voor al je werkzaamheden");
                roosterform.ViewPeriode = false;
                if (roosterform.ShowDialog() == DialogResult.Cancel)
                    return;
                Manager.Opties.TijdelijkeRooster = roosterform.WerkRooster;
                var thesame = xold.SameTijden(Manager.Opties?.GetWerkRooster());
                if (!thesame)
                {
                    var bws = await Manager.GetBewerkingen(new ViewState[] {ViewState.Gestart}, true);

                    bws = bws.Where(x => string.Equals(Manager.Opties.Username, x.GestartDoor,
                        StringComparison.CurrentCultureIgnoreCase)).ToList();

                    if (bws.Count > 0)
                    {
                        var bwselector = new BewerkingSelectorForm(bws,true,true);
                        bwselector.Title = "Selecteer Werkplekken waarvan de rooster aangepast moet worden";
                        if (bwselector.ShowDialog() == DialogResult.OK)
                            await Manager.UpdateGestarteProductieRoosters(bwselector.SelectedWerkplekken, roosterform.WerkRooster);
                    }

                }
                var xrooster = mainMenu1.GetButton("xroostermenubutton");
                var iscustom = Manager.Opties.TijdelijkeRooster != null && Manager.Opties.TijdelijkeRooster.IsCustom();
                if (xrooster != null)
                    xrooster.Image = iscustom
                        ? Resources.schedule_32_32.CombineImage(Resources.exclamation_warning_15590, 1.75)
                        : Resources.schedule_32_32;
                var xchange = iscustom ? "aangepaste rooster" : "standaard rooster";
                await Manager.Opties.Save($"{Manager.Opties.Username} heeft een {xchange}.");
            }
            catch (Exception e)
            {
                XMessageBox.Show(this, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private async void DoOpenStartedProducties()
        {
            try
            {
                var startedprods =
                    (await Manager.Database.GetAllProducties(false, true, null))
                    .Where(x => x.State == ProductieState.Gestart)
                    .ToArray();
                if (startedprods.Length == 0)
                {
                    XMessageBox.Show(this, $"Er zijn geen gestarte producties om te openen.", "Geen Producties",
                        MessageBoxIcon.Exclamation);
                }
                else
                {
                    var xvalue0 = startedprods.Length == 1 ? "is" : "zijn";
                    var xvalue1 = startedprods.Length == 1 ? "productie" : "producties";
                    if (XMessageBox.Show(
                            this, $"Er {xvalue0} {startedprods.Length} gestarte {xvalue1}.\n\nWil je ze allemaal openen?",
                        "Open Producties", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        for (var i = 0; i < startedprods.Length; i++)
                        {
                            var prod = startedprods[i];
                            var bws = prod.Bewerkingen?.Where(x => x.IsAllowed()).ToArray();
                            if (bws?.Length > 0)
                            {
                                var bw = bws.FirstOrDefault(x => x.State == ProductieState.Gestart);
                                if (bw != null)
                                    ShowProductieForm(this,prod, true, bw);
                            }
                        }
                }
            }
            catch (Exception e)
            {
                XMessageBox.Show(this, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private void DoUpdateFormsFromDirectory()
        {
            if (Manager.LogedInGebruiker == null ||
                Manager.LogedInGebruiker.AccesLevel <= AccesType.ProductieBasis) return;
            var ofd = new FolderBrowserDialog()
            {
                Description = "Kies een folder met productieFormulieren pdf's"

            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var xupdater = new UpdateProducties();
                xupdater.CloseWhenFinished = false;
                xupdater.ShowStop = true;
                xupdater.StartWhenShown = true;
                xupdater.UpdateMethod = ()=> Manager.UpdateFormulierenFromDirectory(ofd.SelectedPath, false,xupdater.ProgressChanged);
                xupdater.ShowDialog();
            }
        }

        private void DoLoadDbInstance()
        {
            try
            {
                var msgbox = new XMessageBox();
                var msg = "Kies een database om te laden";
                var bttns = new Dictionary<string, DialogResult>();
                bttns.Add("Annuleren", DialogResult.Cancel);
                bttns.Add("Database Laden", DialogResult.OK);
                bttns.Add("Kies Folder", DialogResult.Yes);
                //var dbs = Manager.DefaultSettings?.DbUpdateEntries ??
                          //UserSettings.GetDefaultSettings()?.DbUpdateEntries ?? new List<DatabaseUpdateEntry>();
                          var dbnames = new List<string>();
                var xroot = Path.GetDirectoryName(Manager.AppRootPath);
                if (Directory.Exists(xroot))
                {
                    var dirs = Directory.GetDirectories(xroot);
                    foreach (var dir in dirs)
                    {
                        if (Directory.Exists(Path.Combine(dir, "RPM_Data")))
                        {
                            dbnames.Add(dir);
                        }
                    }
                }

                dbnames.Add( "Standaard Database");
                var result = msgbox.ShowDialog(this, msg, "Database Laden", MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Information, dbnames.ToArray(), bttns);
                if (result == DialogResult.Cancel) return;
                string path = null;
                if (result == DialogResult.OK)
                {
                    if (msgbox.SelectedValue != null && msgbox.SelectedValue.ToLower() == "standaard database")
                    {
                        var stng = Manager.DefaultSettings ?? UserSettings.GetDefaultSettings();
                        if (Directory.Exists(stng.MainDB.RootPath))
                            path = stng.MainDB.RootPath;
                        else path = Application.StartupPath;
                    }
                    else
                    {
                        var ent = dbnames.FirstOrDefault(x =>
                            string.Equals(x, msgbox.SelectedValue, StringComparison.CurrentCultureIgnoreCase));
                        path = ent;
                    }
                }

                if (path == null)
                {
                    var fb = new FolderBrowserDialog {Description = "Kies een database locatie"};
                    if (fb.ShowDialog() == DialogResult.OK)
                        path = fb.SelectedPath;
                }

                if (path != null)
                {
                    if (!Directory.Exists(path))
                    {
                        XMessageBox.Show(this, $"'{path}' bestaat niet, of is niet toegankelijk!", "Fout",
                            MessageBoxIcon.Error);
                        return;
                    }

                    //if (name != null)
                    //{
                    //    var opties = await Manager.Database.GetSetting(name);
                    //    if (opties != null)
                    //        Manager.Opties = opties;
                    //}
                    this.BeginInvoke(new Action(()=> LoadManager(path,true)));
                    //await _manager.Load(path, true, true, true);
                }
            }
            catch (Exception e)
            {
                XMessageBox.Show(this, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private void mainMenu1_OnMenuClick(object sender, EventArgs e)
        {
            if (sender is Button {Tag: MenuButton menubutton})
                try
                {
                    switch (menubutton.Name.ToLower())
                    {
                        case "xniewproductie":
                            if (Manager.Database?.ProductieFormulieren == null)
                            {
                                XMessageBox.Show(this, $"Kan geen productie aanmaken, omdat de Database niet is geladen.", "Database niet geladen!", MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                                return;
                            }
                            var AddProduction = new WijzigProductie();
                            if (AddProduction.ShowDialog() == DialogResult.OK)
                                BeginInvoke(new MethodInvoker(async () =>
                                {
                                    var form = AddProduction.Formulier;
                                    var msg = await Manager.AddProductie(form,false);
                                    Manager.RemoteMessage(msg);
                                }));

                            break;
                        case "xopenproductie":
                            if (Manager.Database?.ProductieFormulieren == null)
                            {
                                XMessageBox.Show(this, $"Kan geen productie toevoegen, omdat de Database niet is geladen.", "Database niet geladen!", MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                                return;
                            }
                            var ofd = new OpenFileDialog
                            {
                                Title = @"Open Productie Formulier(en)",
                                Filter = @"Pdf|*.pdf|Image|*.JPG;*.Img|Text|*.txt|Alles|*.*",
                                Multiselect = true
                            };
                            if (ofd.ShowDialog() == DialogResult.OK)
                                BeginInvoke(new MethodInvoker(async () =>
                                {
                                    var files = ofd.FileNames;
                                    await _manager.AddProductie(files,true, false, true);
                                }));
                            break;
                        case "xquickproductie":
                            DoQuickProductie();
                            break;
                        case "xsearchtekening":
                            ZoekWerkTekening();
                            break;
                        case "xchangeaantal":
                            DoAantalGemaakt(this);
                            break;
                        case "xwerkplaatsindeling":
                            ShowWerkplaatsIndelingWindow();
                            break;
                        case "xverbruik":
                            ShowBerekenVerbruikWindow();
                            break;
                        case "xcreateexcel":
                            //maak een nieuwe excel aan
                            DoCreateExcel();
                            break;
                        case "xupdatedb":
                            var updater = new DbUpdater();
                            updater.ShowDialog();
                            break;
                        case "xlaaddb":
                            DoLoadDbInstance();
                            break;
                        case "xupdateforms":
                            DoUpdateFormsFromDirectory();
                            break;
                        case "xstats":
                            var chartform = new ViewChartForm();
                            chartform.ShowDialog();
                            break;
                        case "xstoringmenubutton": //storing verwerken
                            DoOnderbreking();
                            break;
                        case "xbekijkproductiepdf":
                            DoOpenProductiePdf();
                            break;
                        case "xroostermenubutton":
                            SetSpecialeRooster();
                            break;
                        case "xspecialeroosterbutton":
                          BeheerSpecialeRoosters();
                            break;
                        case "xopenproducties":
                            DoOpenStartedProducties();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    XMessageBox.Show(this, ex.Message, "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        #endregion Main Menu Methods

        #region Control Events

        private void xUpdate_Click(object sender, EventArgs e)
        {
            AutoUpdater.Start(false);
        }

        private void xsettingsb_Click(object sender, EventArgs e)
        {
            ShowOptieWidow();
        }

        private void xloginb_Click(object sender, EventArgs e)
        {
            if (Manager.LogedInGebruiker != null)
            {
                Manager.LogOut(this,true);
            }
            else
            {
                var xlogin = new LogIn();
                xlogin.StartPosition = FormStartPosition.CenterParent;
                xlogin.ShowDialog();
            }
        }

        private void xupdateallform_Click(object sender, EventArgs e)
        {
            var prod = new UpdateProducties();
            prod.ShowDialog();
        }

        private void xmateriaalverbruikb_Click(object sender, EventArgs e)
        {
            var xmats = new MateriaalVerbruikForm();
            xmats.ShowDialog();
        }

        private void xsendemail_Click(object sender, EventArgs e)
        {
            var emailform = new SendEmailForm();
            emailform.ShowDialog();
        }

        private void xallenotities_Click(object sender, EventArgs e)
        {
            var noteform = new AlleNotitiesForm();
            noteform.ShowDialog();
        }

        private void xchatformbutton_Click(object sender, EventArgs e)
        {
            ShowChatWindow();
        }

        private void xpersoneelb_Click(object sender, EventArgs e)
        {
            ShowPersoneelWindow();
        }

        private void xallstoringenb_Click(object sender, EventArgs e)
        {
            ShowOnderbrekeningenWidow();
        }

        private void xtoonlogsb_Click(object sender, EventArgs e)
        {
            ShowProductieLogWindow();
        }

        private void xallevaardighedenb_Click(object sender, EventArgs e)
        {
            ShowAlleVaardighedenWidow();
        }

        private void xprodinfob_Click(object sender, EventArgs e)
        {
            ShowCalculatieWindow();
        }

        private void xspeciaalroosterbutton_Click(object sender, EventArgs e)
        {
            SetSpecialeRooster();
        }

        private void xdbbewerkingen_Click(object sender, EventArgs e)
        {
            ShowBewerkingDb();
        }


        private void WerkPlekkenUI1_OnPlekkenChanged(object sender, EventArgs e)
        {
           // if (sender is ObjectListView olv)
                //tabPage3.Text = olv.Items.Count == 0
                    //? "Geen Actieve Werkplekken"
                    //: $"Actieve Werkplekken [{olv.Items.Count}]";
        }

        private void WerkPlekkenUI1_OnRequestOpenWerk(object sender, EventArgs e)
        {
            if (!(sender is Bewerking b)) return;
            if (b.Parent != null) ShowProductieForm(this,b.Parent, true, b);
        }

        private void xaboutb_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        //private readonly Timer _prodsearch = new() {Interval = 250};
        //private readonly Timer _bwssearch = new() {Interval = 250};

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.valksolarsystems.com/nl");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.valksystemen.nl/");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.valksystemen.nl/");
        }

        #endregion Control Events

        #region Methods

        public Task RunProductieRefresh()
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (ProductieSyncEnabled && !IsDisposed && !Disposing)
                    {
                        await Task.Delay(ProductieRefreshInterval);
                        if (!ProductieSyncEnabled || IsDisposed || Disposing) break;
                        BeginInvoke(new MethodInvoker(xUpdateList));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        private void xUpdateList()
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CheckForSpecialRooster(bool prompchange)
        {
            if (Manager.Opties == null || Manager.LogedInGebruiker == null ||
                Manager.LogedInGebruiker.AccesLevel == AccesType.AlleenKijken)
            {
                xspeciaalroosterlabel.Visible = false;
                return;
            }
           
            var xtime = DateTime.Now;
            //eerst kijken of het weekend is.
            var culture = new CultureInfo("nl-NL");
            var day = culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);
            if (xtime.DayOfWeek == DayOfWeek.Saturday || xtime.DayOfWeek == DayOfWeek.Sunday)
            {
                //het is weekend, dus we zullen moeten kijken of er wel gewerkt wordt.
                var xbttntxt =
                    $"Het is {day} {xtime.TimeOfDay:hh\\:mm} uur. Vandaag is geen officiële werkdag. Click hier voor de speciale rooster";
                xspeciaalroosterbutton.Text = xbttntxt;
                var rooster = Manager.Opties.SpecialeRoosters.FirstOrDefault(x => x.Vanaf.Date == xtime.Date);
                if (rooster == null && prompchange)
                {
                    var splash = Application.OpenForms["SplashScreen"];
                    splash?.Close();
                    var xmsg =
                        $"Het is vandaag {day}, en geen officiële werkdag.\n" +
                        "Je kan een speciale rooster toevoegen als er vandaag toch wordt gewerkt.\n\n" +
                        "Wil je een rooster nu toevoegen?\nSpeciale roosters kan je achteraf ook aanpassen in de instellingen";
                    if (XMessageBox.Show(this, xmsg, "Speciaal Rooster", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        SetSpecialeRooster();
                }

                xspeciaalroosterlabel.Visible = true;
            }
            else
            {
                var rooster = Manager.Opties.WerkRooster;
                if (xtime.TimeOfDay < rooster.StartWerkdag || xtime.TimeOfDay > rooster.EindWerkdag)
                {
                    var xbttntxt =
                        $"Het is nu {day} {xtime.TimeOfDay:hh\\:mm} uur, dat is buiten om de gewone werkrooster van {rooster.StartWerkdag:hh\\:mm} tot {rooster.EindWerkdag:hh\\:mm} uur. Click hier voor de aangepaste rooster";
                    xspeciaalroosterbutton.Text = xbttntxt;
                    xspeciaalroosterlabel.Visible = true;
                }
                else
                {
                    xspeciaalroosterlabel.Visible = false;
                }
            }

            if (_specialRoosterWatcher != null)
            {
                xtime = DateTime.Now;
                _specialRoosterWatcher.Interval = (60 - xtime.Second) * 1000 - xtime.Millisecond;
            }
        }

        private void SetSpecialeRooster()
        {
            if (Manager.Opties == null) return;
            var xtime = DateTime.Now;
            //eerst kijken of het weekend is.
            var culture = new CultureInfo("nl-NL");
            var day = culture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek);
            if (xtime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                var rooster = Manager.Opties?.SpecialeRoosters?.FirstOrDefault(x => x.Vanaf.Date == DateTime.Now.Date);
                var xreturn = rooster != null;
                if (rooster == null)
                {
                    rooster = Rooster.StandaartRooster();
                    rooster.Vanaf = DateTime.Now;
                    rooster.GebruiktPauze = false;
                    rooster.StartWerkdag = new TimeSpan(7, 0, 0);
                    rooster.EindWerkdag = new TimeSpan(12, 0, 0);
                }

                var roosterform = new RoosterForm(rooster, "Vul in de speciale werkdag tijden");
                roosterform.ViewPeriode = false;
                if (roosterform.ShowDialog() == DialogResult.OK)
                {
                    if (Manager.Opties.SpecialeRoosters == null)
                        Manager.Opties.SpecialeRoosters = new List<Rooster>();
                    var newrooster = roosterform.WerkRooster;
                    var dt = DateTime.Now;
                    var tijd = roosterform.WerkRooster.StartWerkdag;
                    newrooster.Vanaf = new DateTime(dt.Year, dt.Month, dt.Day, tijd.Hours, tijd.Minutes, 0);
                    Manager.Opties.SpecialeRoosters.Remove(rooster);
                    Manager.Opties.SpecialeRoosters.Add(newrooster);
                    Manager.Opties.SpecialeRoosters = Manager.Opties.SpecialeRoosters.OrderBy(x => x.Vanaf).ToList();

                    var bws = Manager.GetBewerkingen(new ViewState[] { ViewState.Gestart }, true).Result;
                    bws = bws.Where(x => string.Equals(Manager.Opties.Username, x.GestartDoor,
                        StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (bws.Count > 0)
                    {
                        var bwselector = new BewerkingSelectorForm(bws,true,true);
                        bwselector.Title = "Selecteer Werkplekken waarvan de rooster aangepast moet worden";
                        if (bwselector.ShowDialog() == DialogResult.OK)
                            Manager.UpdateGestarteProductieRoosters(bwselector.SelectedWerkplekken, null);
                    }

                    Manager.Opties.Save("Speciale roosters aangepast.");
                }
            }
            else
            {
                //var rooster = Manager.Opties.WerkRooster;
                //if (xtime.TimeOfDay < rooster.StartWerkdag || xtime.TimeOfDay > rooster.EindWerkdag)
                //{

                //}
                DoEigenRooster();
            }
        }

        private async void BeheerSpecialeRoosters()
        {
            var sproosters = new SpeciaalWerkRoostersForm(Manager.Opties.SpecialeRoosters);
            if (sproosters.ShowDialog() == DialogResult.OK)
            {
               
                var acces1 = Manager.LogedInGebruiker is { AccesLevel: >= AccesType.ProductieBasis };
                if (acces1)
                {
                    Manager.Opties.SpecialeRoosters = sproosters.Roosters;
                }
                if (acces1 && sproosters.Roosters.Count > 0)
                {
                    var bws = await Manager.GetBewerkingen(new ViewState[] { ViewState.Gestart }, true);
                    bws = bws.Where(x => string.Equals(Manager.Opties.Username, x.GestartDoor,
                        StringComparison.CurrentCultureIgnoreCase)).ToList();
                    if (bws.Count > 0)
                    {
                        var bwselector = new BewerkingSelectorForm(bws,true,true);
                        bwselector.Title = "Selecteer Werkplekken waarvan de rooster aangepast moet worden";
                        if (bwselector.ShowDialog() == DialogResult.OK)
                            await Manager.UpdateGestarteProductieRoosters(bwselector.SelectedWerkplekken, null);
                    }
                }

                if (acces1)
                    _ = Manager.Opties.Save();
            }
        }

        private void CheckForSyncDatabase()
        {
            var opties = Manager.DefaultSettings ?? UserSettings.GetDefaultSettings();
            if (opties?.TempMainDB != null && opties.MainDB != null &&
                opties.TempMainDB.LastUpdated > opties.MainDB.LastUpdated && Directory.Exists(opties.MainDB.UpdatePath))
            {
                var splash = (SplashScreen) Application.OpenForms["SplashScreen"];
                if (splash != null)
                {
                    while (splash.Visible && !splash.CanClose)
                        Application.DoEvents();
                    splash.Close();
                }

                opties.TempMainDB.LastUpdated = opties.MainDB.LastUpdated;
                var prod = new UpdateProducties(opties.TempMainDB)
                    {CloseWhenFinished = true, ShowStop = false, StartWhenShown = true};
                prod.ShowDialog();
                if (prod.IsFinished)
                {
                    opties.MainDB.LastUpdated = DateTime.Now;
                    opties.SaveAsDefault();
                }
            }
        }

        //public static void CheckForUpdateDatabase()
        //{
        //    var opties = Manager.DefaultSettings ?? UserSettings.GetDefaultSettings();
        //    if (opties != null && new Version(LocalDatabase.DbVersion) > new Version(opties.UpdateDatabaseVersion))
        //    {
        //        var splash = Application.OpenForms["SplashScreen"];
        //        splash?.Close();

        //        var prod = new UpdateProducties {CloseWhenFinished = true, ShowStop = false, StartWhenShown = true};
        //        if (prod.ShowDialog() == DialogResult.OK)
        //        {
        //            opties.UpdateDatabaseVersion = LocalDatabase.DbVersion;
        //            opties.SaveAsDefault();
        //        }
        //    }
        //}

        private async void LoadStartedProducties()
        {
            if (_manager == null || Manager.Opties == null || !Manager.Opties.ToonAlleGestartProducties)
                return;
            if (Manager.LogedInGebruiker is {AccesLevel: >= AccesType.ProductieBasis})
            {
                var prs = await Manager.GetAllProductieIDs(false, true);
                foreach (var v in prs)
                {
                    var prod = await Manager.Database.GetProductie(v);
                    if (prod?.Bewerkingen == null || prod.Bewerkingen.Length == 0) continue;
                    var xs = prod.Bewerkingen.FirstOrDefault(x => x.State == ProductieState.Gestart);
                    if (xs == null) continue;
                    ShowProductieForm(this,prod, true, xs);
                }
            }
        }

        private void LoadProductieLogs()
        {
            if (_manager == null || Manager.Opties == null || !Manager.Opties.ToonProductieLogs)
                return;
            ShowProductieLogWindow();
        }

        private void UpdateTabCounts()
        {
            //BeginInvoke(new MethodInvoker(() =>
            //{
            //    //tabPage1.Text = $"Producties [{xproductieListControl1.ProductieLijst.Items.Count}]";
            //    //tabPage2.Text = $"Bewerkingen [{xbewerkingListControl.ProductieLijst.Items.Count}]";
            //   // tabPage4.Text = $"Recente Gereedmeldingen [{recentGereedMeldingenUI1.ItemCount}]";
            //}));
        }

        private XMessageBox _unreadMessages;

        public void UpdateUnreadMessages(UserChat user)
        {
            if (this.InvokeRequired)
                BeginInvoke(new Action(xUpdateUnreadMessages));
            else xUpdateUnreadMessages();
        }

        private void xUpdateUnreadMessages()
        {
            try
            {
                if (ProductieChat.Chat == null) return;
                var unread = ProductieChat.Chat.GetAllUnreadMessages();
                if (unread.Count > 0)
                {
                    var ximg = GraphicsExtensions.DrawUserCircle(new Size(32, 32), Brushes.White,
                        unread.Count.ToString(),
                        new Font("Ariel", 16, FontStyle.Bold), Color.DarkRed);
                    xchatformbutton.Image = Resources.conversation_chat_32x321.CombineImage(ximg, 1.75);
                }
                else
                {
                    xchatformbutton.Image = Resources.conversation_chat_32x321;
                }

                if (_chatform != null)
                {
                    if (_chatform.WindowState == FormWindowState.Minimized)
                        _chatform.WindowState = FormWindowState.Normal;
                    //if (user != null)
                    //    _chatform.SelectedUser(user);
                    _chatform.Show();
                    _chatform.BringToFront();
                    _chatform.Focus();
                }
                else if (unread.Count > 0 && ShowUnreadMessage)
                {
                    _unreadMessages?.Dispose();
                    var names = new List<string>();
                    foreach (var msg in unread.Where(msg => msg.Afzender != null && !names.Any(x =>
                        string.Equals(x, msg.Afzender.UserName, StringComparison.CurrentCultureIgnoreCase))))
                        names.Add(msg.Afzender.UserName);
                    bool iedereen = unread.Any(x =>
                        string.Equals(x.Ontvanger, "iedereen", StringComparison.CurrentCultureIgnoreCase));
                    if (names.Count == 0) return;
                    {
                        Application.OpenForms["SplashScreen"]?.Close();
                        var xv = names.Count == 1 ? "bericht" : "berichten";
                        var bttns = new Dictionary<string, DialogResult>();
                        bttns.Add("OK", DialogResult.OK);
                        bttns.Add("Toon Bericht", DialogResult.Yes);
                        _unreadMessages = new XMessageBox();
                        var result = _unreadMessages.ShowDialog(
                            this, $"Je hebt {unread.Count} ongelezen {xv} van {string.Join(", ", names)}",
                            $"{unread.Count} ongelezen berichten", MessageBoxButtons.OK, MessageBoxIcon.None, null,
                            bttns);
                        _unreadMessages?.Dispose();
                        _unreadMessages = null;
                        if (result == DialogResult.Yes)
                            ShowChatWindow(iedereen ? "Iedereen" : names[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void UpdateUnreadOpmerkingen()
        {
            if (this.InvokeRequired)
                BeginInvoke(new Action(xUpdateUnreadOpmerkingen));
            else xUpdateUnreadOpmerkingen();
        }

        public void xUpdateUnreadOpmerkingen()
        {
            try
            {
                if (Manager.Opmerkingen?.OpmerkingenLijst == null) return;
                var unread = Manager.Opmerkingen.GetUnreadNotes();
                if (unread.Count > 0)
                {
                    var ximg = GraphicsExtensions.DrawUserCircle(new Size(32, 32), Brushes.White,
                        unread.Count.ToString(),
                        new Font("Ariel", 16, FontStyle.Bold), Color.DarkRed);
                    xopmerkingentoolstripbutton.Image = Resources.notes_office_page_papers_32x32.CombineImage(ximg, 1.75);
                }
                else
                {
                    xopmerkingentoolstripbutton.Image = Resources.notes_office_page_papers_32x32;
                }

                if (_opmerkingform != null)
                {
                    if (_opmerkingform.WindowState == FormWindowState.Minimized)
                        _opmerkingform.WindowState = FormWindowState.Normal;
                    //if (user != null)
                    //    _chatform.SelectedUser(user);
                    _opmerkingform.Show();
                    _opmerkingform.BringToFront();
                    _opmerkingform.Focus();
                }
                else if (unread.Count > 0 && ShowUnreadMessage)
                {
                    Application.OpenForms["SplashScreen"]?.Close();
                    var xv = unread.Count == 1 ? "opmerking" : "opmerkingen";
                    var bttns = new Dictionary<string, DialogResult>();
                    bttns.Add("OK", DialogResult.OK);
                    bttns.Add("Toon Opmerkingen", DialogResult.Yes);
                    var xmsgBox = new XMessageBox();
                    var result = xmsgBox.ShowDialog(
                        this, $"Je hebt {unread.Count} ongelezen {xv}",
                        $"{unread.Count} ongelezen {xv}", MessageBoxButtons.OK, MessageBoxIcon.None, null,
                        bttns);
                    xmsgBox.Dispose();
                    if (result == DialogResult.Yes)
                        ShowOpmerkingWindow();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Methods

        #region MenuButton Methods

        public static StartProductie ShowProductieForm(IWin32Window owner, ProductieFormulier pform, bool showform,
            Bewerking bewerking = null)
        {
            if (pform == null || Manager.LogedInGebruiker == null ||
                Manager.LogedInGebruiker.AccesLevel < AccesType.ProductieBasis)
                return null;
            try
            {
                var bws = pform.Bewerkingen;
                    //.Where(x =>
                   // x.IsAllowed() && x.State != ProductieState.Verwijderd).ToList();
               // if (bws.Count == 0)
                   // throw new Exception(
                 //       $"Kan '{pform.Omschrijving}' niet openen omdat er geen geldige bewerkingen zijn!\n\n" +
                   //     "Bewerkingen zijn verwijderd of gefiltered.");
                var productie = _formuis.FirstOrDefault(t => t.Name == pform.ProductieNr.Trim().Replace(" ", ""));
                if (productie is {IsDisposed: false} && _producties != null && showform)
                {
                    productie.Show(_producties.DockPanel);
                }
                else
                {
                    if (productie != null)
                        _formuis.Remove(productie);
                    //productie.Dispose();

                    productie = new StartProductie(pform, bewerking)
                    {
                        Name = pform.ProductieNr.Trim().Replace(" ", ""),
                        TabText = $"[{pform.ProductieNr}, {pform.ArtikelNr}]"
                    };
                    productie.FormClosing += AddProduction_FormClosing;
                    productie.SelectedBewerking = bewerking;
                    _formuis.Add(productie);

                    if (_producties == null || _producties.IsDisposed)
                    {
                        _producties = new Producties
                        {
                            Tag = productie,
                            StartPosition = FormStartPosition.CenterScreen
                        };
                        _producties.FormClosed += (x, y) => _producties = null;
                    }

                    if (showform)
                        productie.Show(_producties.DockPanel);
                }

                if (!_producties.Visible && showform)
                    _producties.Show();
                if (_producties.WindowState == FormWindowState.Minimized)
                    _producties.WindowState = FormWindowState.Normal;

                _producties.Focus();
                return productie;
            }
            catch (Exception e)
            {
                XMessageBox.Show(owner, e.Message, "Fout", MessageBoxIcon.Error);
                return null;
            }
        }

        public static ProductieLijstForm ShowProductieLijstForm(IWin32Window owner)
        {
            try
            {
                var prodform = new ProductieLijstForm(_Productelijsten.Count);

                prodform.FormClosing += AddProduction_FormClosing;
                _Productelijsten.Add(prodform);
                if (_productielijstdock == null || _productielijstdock.IsDisposed)
                {
                    _productielijstdock = new ProductieLijsten
                    {
                        Tag = prodform,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    _productielijstdock.FormClosed += (x, y) => _productielijstdock = null;
                }

                prodform.Show(_productielijstdock.DockPanel);

                if (!_productielijstdock.Visible)
                    _productielijstdock.Show();
                if (_productielijstdock.WindowState == FormWindowState.Minimized)
                    _productielijstdock.WindowState = FormWindowState.Normal;

                _productielijstdock.Focus();
                return prodform;
            }
            catch (Exception e)
            {
                XMessageBox.Show(owner, e.Message, "Fout", MessageBoxIcon.Error);
                return null;
            }
        }

        public static void AddProduction_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is StartProductie prod)
                _formuis.Remove(prod);
            else if (sender is ProductieLijstForm form)
                _Productelijsten.Remove(form);
        }

        public static void ShowProductieLogWindow()
        {
            if (_logform == null)
            {
                _logform = new LogForm();
                _logform.FormClosed += (x, y) =>
                {
                    _logform?.Dispose();
                    _logform = null;
                };
            }

            _logform.Show();
            if (_logform.WindowState == FormWindowState.Minimized)
                _logform.WindowState = FormWindowState.Normal;
            _logform.BringToFront();
            _logform.Focus();
        }

        public static void ShowCalculatieWindow()
        {
            //var _calcform = new RangeCalculatorForm();
            //_calcform.ShowDialog();
            if (_calcform == null)
            {
                _calcform = new RangeCalculatorForm();
                _calcform.FormClosed += (x, y) =>
                {
                    _calcform?.Dispose();
                    _calcform = null;
                };
            }
            _calcform.Show();
            if (_calcform.WindowState == FormWindowState.Minimized)
                _calcform.WindowState = FormWindowState.Normal;
            _calcform.BringToFront();
            _calcform.Focus();
        }

        public static void ShowChatWindow(string username = null)
        {
            if (_chatform == null)
            {
                _chatform = new ChatForm();
                _chatform.FormClosed += (x, y) =>
                {
                    _chatform?.Dispose();
                    _chatform = null;
                };
            }

            _chatform.Show(username);
            if (_chatform.WindowState == FormWindowState.Minimized)
                _chatform.WindowState = FormWindowState.Normal;
            _chatform.BringToFront();
            _chatform.Focus();
        }

        public static void ShowOpmerkingWindow()
        {
            if (_opmerkingform == null)
            {
                _opmerkingform = new OpmerkingenForm();
                _opmerkingform.LoadOpmerkingen();
                _opmerkingform.FormClosed += (x, y) =>
                {
                    _opmerkingform?.Dispose();
                    _opmerkingform = null;
                };
            }

            _opmerkingform.Show();
            if (_opmerkingform.WindowState == FormWindowState.Minimized)
                _opmerkingform.WindowState = FormWindowState.Normal;
            _opmerkingform.BringToFront();
            _opmerkingform.Focus();
        }

        public static void ShowArtikelenWindow(string artnr = null)
        {
            if (_ArtikelsForm == null)
            {
                _ArtikelsForm = new ArtikelsForm();
                _ArtikelsForm.FormClosed += (x, y) =>
                {
                    _ArtikelsForm?.Dispose();
                    _ArtikelsForm = null;
                };
            }

            _ArtikelsForm.Show();
            if (_ArtikelsForm.WindowState == FormWindowState.Minimized)
                _ArtikelsForm.WindowState = FormWindowState.Normal;
            _ArtikelsForm.BringToFront();
            _ArtikelsForm.Focus();
            if (artnr != null)
                _ArtikelsForm.SelectedArtikelNr = artnr;
        }

        public static void ShowOnderbrekeningenWidow()
        {
            var x = new AlleStoringen();
            x.InitStoringen();
            x.ShowDialog();
        }

        public static void ShowAlleVaardighedenWidow()
        {
            new AlleVaardigheden().ShowDialog();
        }

        public static void ShowPersoneelWindow()
        {
            if (_PersoneelForm == null)
            {
                _PersoneelForm = new PersoneelsForm(false);
                _PersoneelForm.FormClosed += (x, y) =>
                {
                    _PersoneelForm?.Dispose();
                    _PersoneelForm = null;
                };
            }

            _PersoneelForm.Show();
            if (_PersoneelForm.WindowState == FormWindowState.Minimized)
                _PersoneelForm.WindowState = FormWindowState.Normal;
            _PersoneelForm.BringToFront();
            _PersoneelForm.Focus();
        }

        public static void ShowWerkplaatsIndelingWindow()
        {
            if (_WerkplaatsIndeling == null)
            {
                _WerkplaatsIndeling = new WerkplaatsIndeling();
                _WerkplaatsIndeling.FormClosed += (x, y) =>
                {
                    _WerkplaatsIndeling?.Dispose();
                    _WerkplaatsIndeling = null;
                };
            }

            _WerkplaatsIndeling.Show();
            if (_WerkplaatsIndeling.WindowState == FormWindowState.Minimized)
                _WerkplaatsIndeling.WindowState = FormWindowState.Normal;
            _WerkplaatsIndeling.BringToFront();
            _WerkplaatsIndeling.Focus();
        }

        public static void ShowBerekenVerbruikWindow()
        {
            if (_berekenverbruik == null)
            {
                _berekenverbruik = new MetroForm();
                _berekenverbruik.Style = MetroColorStyle.Orange;
                _berekenverbruik.StartPosition = FormStartPosition.CenterScreen;
                _berekenverbruik.ShadowType = MetroFormShadowType.AeroShadow;
                _berekenverbruik.MinimumSize = new Size(750, 550);
                _berekenverbruik.Text = "Bereken Verbruik";
                var xver = new ProductieVerbruikUI();
                xver.Dock = DockStyle.Fill;
                xver.ShowMateriaalSelector = false;
                xver.ShowOpslaan = true;
                xver.ShowPerUur = true;
                xver.ShowSluiten = true;
                xver.ShowOpdrukkerArtikelNr = true;
                xver.UpdateFields(true);
                xver.MaxUitgangsLengte = 12450;
                xver.RestStuk = 50;
                _berekenverbruik.Controls.Add(xver);
                _berekenverbruik.FormClosed += (x, y) =>
                {
                    _berekenverbruik?.Dispose();
                    _berekenverbruik = null;
                };
            }

            _berekenverbruik.Show();
            if (_berekenverbruik.WindowState == FormWindowState.Minimized)
                _berekenverbruik.WindowState = FormWindowState.Normal;
            _berekenverbruik.BringToFront();
            _berekenverbruik.Focus();
        }

        public static void ShowPersoonVaardigheden(Personeel persoon)
        {
            if (persoon == null)
                return;
            new VaardighedenForm(persoon).ShowDialog();
        }

        public static void ShowBewStoringen(Bewerking bew)
        {
            var form = bew?.GetParent();
            if (form == null) return;
            var allst = new AlleStoringen();
            allst.InitStoringen(form);
            allst.ShowDialog();
        }

        public static void ShowWerkplekken(ProductieFormulier form)
        {
            if (form == null)
                return;
            var ind = new Indeling(form);
            ind.ShowDialog();
        }

        public void ZoekWerkTekening()
        {
            try
            {
                var tb = new TextFieldEditor();
                tb.Style = MetroColorStyle.Yellow;
                tb.FieldImage = Resources.libreoffice_draw_icon_128x128.CombineImage(Resources.search_locate_find_6278,
                    ContentAlignment.BottomRight, 2.5);
                tb.MultiLine = false;
                tb.Title = "Zoek WerkTekening";
                if (tb.ShowDialog() == DialogResult.OK)
                {
                    Tools.ShowSelectedTekening(tb.SelectedText, TekeningClosed);
                }
            }
            catch (Exception e)
            {
                XMessageBox.Show(this, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private static AantalGemaaktProducties _gemaaktform;
        public static async void DoAantalGemaakt(IWin32Window owner, List<Bewerking> bewerkingen = null, int lastchangedminutes = -1)
        {
            try
            {
                if (Manager.Database == null || Manager.Database.IsDisposed)
                    return;
                if (_gemaaktform != null) return;
                var bws = bewerkingen??await Manager.Database.GetBewerkingen(ViewState.Gestart, true,null, null);
                _gemaaktform = new AantalGemaaktProducties(bws, lastchangedminutes);
                _gemaaktform.FormClosed += (o,e) =>
                {
                    _gemaaktform?.Dispose();
                    _gemaaktform = null;
                };
                _gemaaktform.ShowDialog();

            }
            catch (Exception e)
            {
                XMessageBox.Show(owner, e.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private void TekeningClosed(object sender, EventArgs e)
        {
            this.Parent?.BringToFront();
            this.Parent?.Focus();
        }

        public static void ShowProductieSettings(ProductieFormulier form)
        {
            if (form == null)
                return;
            var x = new WijzigProductie(form);
            x.ShowDialog();
        }

        public static void ShowOptieWidow()
        {
            var opties = new Opties();
            opties.ShowDialog();
            //if (_opties != null)
            //{
            //    if (!_opties.Visible)
            //        _opties.Show();
            //    if (_opties.WindowState == FormWindowState.Minimized)
            //        _opties.WindowState = FormWindowState.Normal;
            //}
            //else
            //{
            //    _opties = new Opties();
            //    _opties.FormClosed += (x, y) => { _opties = null; };
            //    _opties.Show();
            //}

            //_opties.BringToFront();
            //_opties.Select();
        }

        public static void ShowBewerkingDb()
        {
            var db = new DbBewerkingChanger();
            db.ShowDialog();
        }

        public static void ShowProductieOverzicht()
        {
            if (_ProductieOverzicht == null)
            {
                _ProductieOverzicht = new ProductieOverzichtForm();
                _ProductieOverzicht.FormClosed += (x, y) =>
                {
                    _ProductieOverzicht?.Dispose();
                    _ProductieOverzicht = null;
                };
            }

            _ProductieOverzicht.Show();
            if (_ProductieOverzicht.WindowState == FormWindowState.Minimized)
                _ProductieOverzicht.WindowState = FormWindowState.Normal;
            _ProductieOverzicht.BringToFront();
            _ProductieOverzicht.Focus();
        }

        public static void CheckForPreview(bool showall, bool onlyifnew)
        {
            try
            {
                var xprevs =
                    Functions.GetVersionPreviews(Manager.LastPreviewsUrl);
                var xvers = Assembly.GetExecutingAssembly().GetName().Version;
                if (xprevs.Count > 0)
                {
                    UpdatePreviewForm xshowform = null;
                    if (!showall)
                    {
                        
                        if (!xprevs.ContainsKey(xvers.ToString())) return;
                        bool isnew = new Version(Manager.DefaultSettings.LastPreviewVersion) < xvers;
                        if (onlyifnew && (Manager.DefaultSettings.PreviewShown && !isnew))
                            return;
                        xshowform = new UpdatePreviewForm(xprevs[xvers.ToString()], onlyifnew,false);
                        xshowform.Title = $"NIEUW In {xvers}!";
                    }
                    else
                    {
                        var urls = xprevs.Select(x => x.Value).ToArray();
                        xshowform = new UpdatePreviewForm(urls);
                        xshowform.Title = $"Alle Aanpassingen In Versie: {xprevs.LastOrDefault().Key?? xvers.ToString()}";
                    }

                    xshowform.ShowDialog();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion MenuButton Methods

        #region Menu Button Events
        private void xproductieoverzichtb_Click(object sender, EventArgs e)
        {
            ShowProductieOverzicht();
        }

        private async void xsearchprodnr_Click(object sender, EventArgs e)
        {
            try
            {
                if (Manager.Database == null)
                    throw new Exception("Database is niet geladen!");
                if (Manager.Database == null)
                    throw new Exception("Database is niet geladen!");
                var tb = new TextFieldEditor();
                tb.FieldImage = Resources.search_page_document_128x128;
                tb.MultiLine = false;
                tb.Title = "Zoek productienr";
                tb.EnableSecondaryField = true;
                tb.SecondaryDescription = "Vul in een Artikelnr, bewerking naam of een stukje omschrijving";
                if (tb.ShowDialog() == DialogResult.OK)
                {
                    if (tb.UseSecondary)
                    {
                        var calcform = new RangeCalculatorForm();
                        var rf = new RangeCalculatorForm.RangeFilter();
                        rf.Enabled = true;
                        rf.Criteria = tb.SecondaryText.Trim();
                        calcform.Show(rf);
                    }
                    else
                    {
                        var xsel = tb.SelectedText.Trim();
                        var prod = await Manager.Database.GetProductie(xsel);
                        if (prod == null)
                            throw new Exception($"Er is geen productie gevonden met '{xsel}'.");
                        var bw = prod.Bewerkingen?.FirstOrDefault(x => x.IsAllowed());
                        Manager.FormulierActie(new object[] {prod, bw}, MainAktie.OpenProductie);
                    }
                }
            }
            catch (Exception ex)
            {
                XMessageBox.Show(this, ex.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var e = new KeyEventArgs(keyData);

            if (e.Control && e.KeyCode == Keys.F)

            {
                xsearchprodnr_Click(null, EventArgs.Empty);
                return true; // handled
            }

            if (e.Control && e.KeyCode == Keys.T)
            {
                mainMenu1.PressButton("xSearchTekening");
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void xopennewlijst_Click(object sender, EventArgs e)
        {
            ShowProductieLijstForm(this);
        }

        private void xtoonartikels_Click(object sender, EventArgs e)
        {
            ShowArtikelenWindow();
            //new ArtikelsForm().ShowDialog();
            //if (Manager.Opties != null)
            //    xbewerkingListControl.ProductieLijst.RestoreState(Manager.Opties.ViewDataBewerkingenState);
        }

        private void xopmerkingentoolstripbutton_Click(object sender, EventArgs e)
        {
            ShowOpmerkingWindow();
        }

        private void xShowPreview_Click(object sender, EventArgs e)
        {
            try
            {
                CheckForPreview(true, false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                XMessageBox.Show(this, exception.Message, "Fout", MessageBoxIcon.Error);
            }
        }

        private void xHelpButton_Click(object sender, EventArgs e)
        {
            var xhelp = new UpdatePreviewForm(Manager.HelpDropUrl, false, true);
            xhelp.Title = "ProductieManager HelpDesk";
            if (xhelp.IsValid)
            {
                xhelp.ShowDialog();
            }
            else
            {
                XMessageBox.Show(this, $"HelpDesk is tijdelijk niet beschikbaar", "Niet Beschikbaar",
                    MessageBoxIcon.Exclamation);
            }
        }

        private void xcorruptedfilesbutton_Click(object sender, EventArgs e)
        {
            var xlist = MultipleFileDb.CorruptedFilePaths;
            var xweergave = new LijstWeergaveForm();
            xweergave.AllowAddItem = false;
            xweergave.AllowOpslaan = false;
            xweergave.ViewedData = xlist.ToArray();

            MultipleFileDb.CorruptedFilesChanged += xweergave.OnItemsChanged;

            xweergave.ItemRemoved += (x, y) =>
            {

                if (x is string[] items)
                {
                    int deleted = 0;
                    foreach (var item in items)
                    {
                        var itemname = Path.GetFileNameWithoutExtension(item);
                        var xdir = Path.GetDirectoryName(item);
                        var xdirname = Path.GetFileName(xdir);
                        var xdel = Manager.Database.RemoveFromCollection(xdirname, new string[] {itemname}).Result;
                        if(xdel > 0)
                            MultipleFileDb.CorruptedFilePaths.Remove(item);
                        deleted += xdel;
                    }

                    if (deleted > 0)
                    {
                        MultipleFileDb.OnCorruptedFilesChanged();
                    }
                }
            };
            xweergave.ShowDialog();
            MultipleFileDb.CorruptedFilesChanged -= xweergave.OnItemsChanged;
            xweergave.Dispose();
        }

        private bool _xtekeningbusy;
        private void xMissingTekening_Click(object sender, EventArgs e)
        {
            if (_xtekeningbusy) return;
            _xtekeningbusy = true;
            var xloading = new LoadingForm();
            xloading.StartPosition = FormStartPosition.CenterParent;
            var progarg = xloading.Arg;
            progarg.Message = "Producties Laden...";
            progarg.OnChanged(this);
            var wb = new WebBrowserForm();
            wb.ShowErrorMessage = false;
            wb.Arg = progarg;
            wb.FilesFormatToOpen = new string[] { "{0}_fbr.pdf" };
            wb.ShowNotFoundList = true;
            wb.StopNavigatingAfterError = false;
            Task.Factory.StartNew(async () =>
            {
               
                var prods = await Manager.Database.GetBewerkingenInArtnrSections(true, false);
                progarg.Max = prods.Count;
                progarg.OnChanged(this);
                if (progarg.IsCanceled) return;
                var arts = prods.Select(x => x.Key).ToArray();
                //Process.Start(xtek); 
                
                wb.FilesToOpen.AddRange(arts);
                wb.BeginInvoke(new MethodInvoker(() => wb.Navigate()));
                //this.BeginInvoke(new MethodInvoker(()=> wb.ShowDialog()));
                //this.Invoke(new MethodInvoker(() => { xloading.BringToFront();
                //    xloading.Focus();
                //}));
            });
           
            wb.Show();
            xloading.ShowDialog();
            wb.Close();
            this.BringToFront();
            this.Focus();
            _xtekeningbusy = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var xdraw = new DrawArrow(this, Direction.Top, true, this.PointToScreen(xToolButtons.Location));
            xdraw.Draw();
            var xdraw1 = new DrawArrow(this, Direction.Left, true,this.PointToScreen(mainMenu1.Location));
            xdraw1.Draw();
            //var xdraw = new DrawArrow(this, Direction.Top, true, this.PointToScreen(xToolButtons.Location));
            //xdraw.Draw();
            //var xdraw = new DrawArrow(this, Direction.Top, true, this.PointToScreen(xToolButtons.Location));
            //xdraw.Draw();
        }

        private Point FindLocation(Control ctrl, bool height)
        {
            if (ctrl.Parent is Form)
                return new Point(ctrl.Location.X + (height ? 0 : ctrl.Width),
                    ctrl.Location.Y + (height ? ctrl.Height : 0));

            Point p = FindLocation(ctrl.Parent, height);
            p.X += ctrl.Location.X;
            p.Y += ctrl.Location.Y;
            return p;
        }

        private void xklachten_Click(object sender, EventArgs e)
        {
            var kl = new KlachtenForm();
            kl.ShowDialog();
        }

        private void xverpakkingen_Click(object sender, EventArgs e)
        {
            var vr = new VerpakkingenForm();
            vr.ShowDialog();
        }

        private void xMaakWeekOverzichtToolstrip_Click(object sender, EventArgs e)
        {
            var xf = new CreateWeekExcelForm();
            xf.ShowDialog();
        }
        #endregion Menu Button Events

        #region Taken Lijst

        private void takenManager1_OnTaakClicked(Taak taak)
        {
            if (taak?.Formulier != null)
            {
                if (taak.Plek != null)
                {
                    //werkPlekkenUI1.xwerkpleklist.SelectedObject = taak.Plek;
                    //if (werkPlekkenUI1.xwerkpleklist.SelectedItem != null)
                    //{
                    //    metroTabControl.SelectedIndex = 1;
                    //    werkPlekkenUI1.xwerkpleklist.SelectedItem.EnsureVisible();
                    //    return;
                    //}
                }

                if (taak.Bewerking != null)
                {
                    //xbewerkingListControl.SelectedItem = taak.Bewerking;
                    //if (xbewerkingListControl.SelectedItem != null) metroTabControl.SelectedIndex = 0;
                }
                //else
                //{
                    //xproductieListControl1.SelectedItem = taak.Formulier;
                    //if (xproductieListControl1.SelectedItem != null) metroTabControl.SelectedIndex = 0;
                //}
            }
        }

        private async void takenManager1_OnTaakUitvoeren(Taak taak)
        {
            var save = false;
            switch (taak.Type)
            {
                case AktieType.ControleCheck:
                    if (taak.Bewerking != null)
                    {
                        var xui = new AantalGemaaktUI();
                        if (xui.ShowDialog(taak.Formulier, taak.Bewerking, taak.Plek) == DialogResult.OK)
                            //taak.Update();
                            save = false;
                    }

                    break;

                case AktieType.Beginnen:
                    if (taak.Formulier != null)
                    {
                        var p = taak.Formulier;
                        if (p.State != ProductieState.Verwijderd && p.State != ProductieState.Gereed)
                            ShowProductieForm(this,p, true, taak.Bewerking);
                    }

                    break;

                case AktieType.GereedMelden:
                    if (taak.Formulier != null)
                    {
                        var p = taak.Formulier;
                        if (p.State != ProductieState.Verwijderd && p.State != ProductieState.Gereed)
                            ProductieListControl.MeldGereed(this, p);
                        //taak.Update();
                    }

                    break;

                case AktieType.BewerkingGereed:
                    if (taak.Bewerking != null)
                    {
                        var b = taak.Bewerking;
                        if (b.State != ProductieState.Verwijderd && b.State != ProductieState.Gereed)
                            ProductieListControl.MeldBewerkingGereed(this, b);
                        //taak.Update();
                    }

                    break;

                case AktieType.KlaarZetten:
                    if (taak.Formulier != null)
                    {
                        var p = taak.Formulier;
                        var matform = new MateriaalForm();
                        if (matform.ShowDialog(p) == DialogResult.OK)
                        {
                            taak.Formulier = matform.Formulier;
                            save = true;
                        }
                    }

                    break;

                case AktieType.Stoppen:
                    break;

                case AktieType.PersoneelChange:
                    if (taak.Bewerking != null)
                    {
                        var ind = new Indeling(taak.Formulier, taak.Bewerking);
                        ind.StartPosition = FormStartPosition.CenterParent;
                        if (ind.ShowDialog() == DialogResult.OK)
                        {
                            save = false;
                        }//taak.Update();
                           
                    }

                    break;

                case AktieType.Telaat:
                    if (taak.Bewerking != null)
                    {
                        var dt = new DatumChanger();
                        if (dt.ShowDialog(taak.Bewerking.LeverDatum,
                                $"Wijzig leverdatum voor ({taak.Bewerking.Path}) {taak.Bewerking.Omschrijving}.") ==
                            DialogResult.OK)
                        {
                            taak.Formulier = Manager.Database.GetProductie(taak.Bewerking.ProductieNr).Result;
                            taak.Bewerking = taak.Formulier.Bewerkingen?.FirstOrDefault(x =>
                                string.Equals(x.Naam, taak.Bewerking.Naam, StringComparison.CurrentCultureIgnoreCase));

                            if (taak.Bewerking != null)
                            {
                                taak.Bewerking.LeverDatum = dt.SelectedValue;
                                save = true;
                            }
                        }

                        dt.Dispose();
                    }
                    else if (taak.Formulier != null)
                    {
                        var dt = new DatumChanger();
                        if (dt.ShowDialog(taak.Formulier.LeverDatum,
                            $"Wijzig leverdatum voor {taak.Formulier.Omschrijving}.") == DialogResult.OK)
                        {
                            taak.Formulier = Manager.Database.GetProductie(taak.Formulier.ProductieNr).Result;
                            if (taak.Formulier.Bewerkingen.Length > 0)
                                taak.Formulier.Bewerkingen[taak.Formulier.Bewerkingen.Length - 1].LeverDatum =
                                    dt.SelectedValue;
                            else taak.Formulier.LeverDatum = dt.SelectedValue;
                            save = true;
                        }

                        dt.Dispose();
                    }

                    break;

                case AktieType.PersoneelVrij:
                    if (taak.Bewerking != null)
                    {
                        var ind = new Indeling(taak.Formulier, taak.Bewerking);
                        ind.StartPosition = FormStartPosition.CenterParent;
                        ind.ShowDialog();
                        // if (ind.ShowDialog() == DialogResult.OK)
                        //taak.Update();
                    }

                    break;
                case AktieType.Onderbreking:
                    if (taak.Plek != null)
                    {
                        var st = new StoringForm(taak.Plek);
                        st.ShowDialog();
                    }

                    break;
                case AktieType.None:
                    break;
            }

            if (save && taak.Formulier != null)
                await taak.Formulier.UpdateForm(true, false, null, $"[{taak.GetPath()}] Taak Uitgevoerd");
            else if (save)
                taak.Bewerking?.UpdateBewerking(null, $"[{taak.GetPath()}] Taak Uitgevoerd");
        }


        #endregion Taken Lijst

        private void xArtikelRecordsToolstripButton_Click(object sender, EventArgs e)
        {
            var artikels = new ArtikelRecordsForm();
            artikels.ShowDialog();
        }

        private void xShowDaily_Click(object sender, EventArgs e)
        {
            DailyMessage.CreateDaily(true);
        }

        private void xSporenButton_Click(object sender, EventArgs e)
        {
            new SporenForm().ShowDialog();
        }
    }
}