using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Controls
{
    public delegate TileInfoEntry TileChangeEventhandler(Tile tile);
    [Serializable]
    public class TileInfoEntry
    {
        public int TileCount { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string GroupName { get; set; }
        public Size Size { get; set; } = new Size(256, 128);
        public int TileIndex { get; set; }
        public Color TileColor { get; set; } = Color.DeepSkyBlue;
        public Color ForeColor { get; set; } = Color.Black;
        public Font TextFont { get; set; } = new Font(new FontFamily("segoe ui"), 16, FontStyle.Regular);
        public Image TileImage { get; set; }

        public TileInfoEntry()
        {
            Name = "Tile";
        }

        public override bool Equals(object obj)
        {
            if (obj is TileInfoEntry entry)
                return string.Equals(Name, entry.Name, StringComparison.CurrentCultureIgnoreCase);
            return false;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode()??0;
        }
    }
}
