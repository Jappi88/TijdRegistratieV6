namespace Controls
{
    partial class TileViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.xHeaderLabel = new System.Windows.Forms.Label();
            this.xTileContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // xHeaderLabel
            // 
            this.xHeaderLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.xHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xHeaderLabel.Location = new System.Drawing.Point(0, 0);
            this.xHeaderLabel.Name = "xHeaderLabel";
            this.xHeaderLabel.Size = new System.Drawing.Size(1061, 32);
            this.xHeaderLabel.TabIndex = 0;
            this.xHeaderLabel.Text = "Header";
            this.xHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // xTileContainer
            // 
            this.xTileContainer.AutoScroll = true;
            this.xTileContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xTileContainer.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.xTileContainer.Location = new System.Drawing.Point(0, 32);
            this.xTileContainer.Name = "xTileContainer";
            this.xTileContainer.Size = new System.Drawing.Size(1061, 596);
            this.xTileContainer.TabIndex = 1;
            // 
            // TileViewer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.xTileContainer);
            this.Controls.Add(this.xHeaderLabel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "TileViewer";
            this.Size = new System.Drawing.Size(1061, 628);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label xHeaderLabel;
        private System.Windows.Forms.FlowLayoutPanel xTileContainer;
    }
}
