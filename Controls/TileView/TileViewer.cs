using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MetroFramework;

namespace Controls
{
    public partial class TileViewer : UserControl
    {
        private readonly System.Timers.Timer _timer;
        private TileInfoEntry[] _Tiles = new TileInfoEntry[] { };
        public TileInfoEntry[] Tiles
        {
            get => _Tiles;
            set
            {
                _Tiles = value;
                UpdateTiles();
            }
        }

        public string Header
        {
            get => xHeaderLabel.Text;
            set
            {
                xHeaderLabel.Text = value;
                Invalidate();
            } 
        }

        public Label HeaderLabel
        {
            get => xHeaderLabel;
            set=> xHeaderLabel = value;
        }

        public int TileInfoRefresInterval { get; set; } = 10000;
        public bool EnableTimer
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public TileViewer()
        {
            InitializeComponent();
            _timer = new System.Timers.Timer();
            _timer.Interval = TileInfoRefresInterval;
            _timer.Elapsed += _timer_Elapsed;
        }

        public void StartTimer()
        {
            _timer?.Start();
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StopTimer();
            try
            {
                for (int i = 0; i < _Tiles.Length; i++)
                {
                    var xtileinfo = _Tiles[i];
                    Tile xtile = null;
                    if (this.InvokeRequired)
                        this.Invoke(new MethodInvoker(() => xtile = GetTile(xtileinfo.Name)));
                    else
                        xtile = GetTile(xtileinfo.Name);
                    if (xtile != null)
                    {
                        var xupdate = OnTileRequestInfo(xtile);
                        if (xupdate != null)
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(() => UpdateTile(xupdate, xtile)));
                                this.Invoke(new MethodInvoker(xTileContainer.Invalidate));
                            }
                            else
                            {
                                UpdateTile(xupdate, xtile);
                                xTileContainer.Invalidate();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            StartTimer();
        }

        private void UpdateTiles()
        {
            try
            {
                var xtoremove = GetAllTiles().Where(x => x.Tag is TileInfoEntry ent && !_Tiles.Any(t => t.Equals(ent)))
                    .ToList();
                if (xtoremove.Count > 0)
                {
                    xTileContainer.SuspendLayout();
                    xtoremove.ForEach(x => xTileContainer.Controls.Remove(x));
                    xTileContainer.ResumeLayout();
                }

                for (int i = 0; i < _Tiles.Length; i++)
                {
                    var xtileinfo = _Tiles[i];
                    xtileinfo.TileIndex = i;
                    var xtile = GetTile(xtileinfo.Name);
                    bool isnew = xtile == null;
                    xtileinfo.GroupName = xHeaderLabel.Text;
                    xtile = UpdateTile(xtileinfo, xtile);
                    if (isnew)
                    {
                        xTileContainer.Controls.Add(xtile);
                        xtile.Click += (x, y) => OnTileClicked(x);
                    }

                    xTileContainer.Controls.SetChildIndex(xtile, i);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Tile UpdateTile(TileInfoEntry entry, Tile tile = null)
        {
            try
            {
                var xtile = tile ?? new Tile();
                xtile.Name = entry.Name;
                xtile.Text = entry.Text;
                xtile.Tag = entry;
                xtile.Image = entry.TileImage;
                xtile.TileCount = entry.TileCount;
                xtile.TileTextFontWeight = MetroTileTextWeight.Regular;
                xtile.TileTextFontSize = MetroTileTextSize.Tall;
                xtile.Font = entry.TextFont;
                xtile.ForeColor = entry.ForeColor;
                xtile.UseCustomBackColor = true;
                xtile.UseCustomForeColor = true;
                xtile.BackColor = entry.TileColor;
                xtile.Size = entry.Size;
                return xtile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public Tile GetTile(string name)
        {
            try
            {
                var xcontrols = xTileContainer.Controls.Cast<Control>().ToList();
                for (int i = 0; i < xcontrols.Count; i++)
                {
                    var xcon = xcontrols[i];
                    if (string.Equals(xcon.Name, name, StringComparison.CurrentCultureIgnoreCase) && xcon is Tile tile)
                        return tile;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public List<Tile> GetAllTiles()
        {
            var xret = new List<Tile>();
            try
            {
                var xcontrols = xTileContainer.Controls.Cast<Control>().ToList();
                for (int i = 0; i < xcontrols.Count; i++)
                {
                    var xcon = xcontrols[i];
                    if (xcon is Tile tile)
                        xret.Add(tile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return xret;
        }

        public event EventHandler TileClicked;
        public event TileChangeEventhandler TileRequestInfo;

        protected virtual void OnTileClicked(object sender)
        {
            TileClicked?.Invoke(sender, EventArgs.Empty);
        }

        protected virtual TileInfoEntry OnTileRequestInfo(Tile tile)
        {
            return TileRequestInfo?.Invoke(tile);
        }
    }
}
