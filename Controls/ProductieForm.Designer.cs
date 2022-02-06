namespace Controls
{
    partial class ProductieForm
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
            this.components = new System.ComponentModel.Container();
            Various.MenuButton menuButton1 = new Various.MenuButton();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductieForm));
            Various.MenuButton menuButton2 = new Various.MenuButton();
            Various.MenuButton menuButton3 = new Various.MenuButton();
            Various.MenuButton menuButton4 = new Various.MenuButton();
            Various.MenuButton menuButton5 = new Various.MenuButton();
            Various.MenuButton menuButton6 = new Various.MenuButton();
            Various.MenuButton menuButton7 = new Various.MenuButton();
            Various.MenuButton menuButton8 = new Various.MenuButton();
            Various.MenuButton menuButton9 = new Various.MenuButton();
            Various.MenuButton menuButton10 = new Various.MenuButton();
            Various.MenuButton menuButton11 = new Various.MenuButton();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aantalTeMakenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leverdatumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.materialenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xprodafkeurtoolstrip = new System.Windows.Forms.ToolStripMenuItem();
            this.notitieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.xbewerking = new MetroFramework.Controls.MetroComboBox();
            this.xprogressbar = new CircularProgressBar.CircularProgressBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.xstatuslabel = new System.Windows.Forms.Label();
            this.xstopb = new System.Windows.Forms.Button();
            this.xindelingb = new System.Windows.Forms.Button();
            this.xstartb = new System.Windows.Forms.Button();
            this.xverpakking = new System.Windows.Forms.Button();
            this.xtoonwerktekening = new System.Windows.Forms.Button();
            this.xOnderbreek = new System.Windows.Forms.Button();
            this.xpanelcontainer = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.xnotepanel = new System.Windows.Forms.Panel();
            this.xnoteTextbox = new System.Windows.Forms.TextBox();
            this.xnoteButton = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.Panel();
            this.xstatusimage = new System.Windows.Forms.PictureBox();
            this.productieInfoUI1 = new Controls.ProductieInfoUI();
            this.mainMenu1 = new Controls.MainMenu();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.xpanelcontainer.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.xnotepanel.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xstatusimage)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.leverdatumToolStripMenuItem,
            this.toolStripSeparator1,
            this.aantalTeMakenToolStripMenuItem,
            this.xprodafkeurtoolstrip,
            this.materialenToolStripMenuItem,
            this.notitieToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(189, 200);
            // 
            // aantalTeMakenToolStripMenuItem
            // 
            this.aantalTeMakenToolStripMenuItem.Image = global::ProductieManager.Properties.Resources.hashtag_icon_152828_32_32;
            this.aantalTeMakenToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.aantalTeMakenToolStripMenuItem.Name = "aantalTeMakenToolStripMenuItem";
            this.aantalTeMakenToolStripMenuItem.Size = new System.Drawing.Size(188, 38);
            this.aantalTeMakenToolStripMenuItem.Text = "Aantal Te Maken";
            this.aantalTeMakenToolStripMenuItem.Click += new System.EventHandler(this.aantalTeMakenToolStripMenuItem_Click);
            // 
            // leverdatumToolStripMenuItem
            // 
            this.leverdatumToolStripMenuItem.Image = global::ProductieManager.Properties.Resources.systemtime_778_32_32;
            this.leverdatumToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.leverdatumToolStripMenuItem.Name = "leverdatumToolStripMenuItem";
            this.leverdatumToolStripMenuItem.Size = new System.Drawing.Size(188, 38);
            this.leverdatumToolStripMenuItem.Text = "Leverdatum";
            this.leverdatumToolStripMenuItem.Click += new System.EventHandler(this.leverdatumToolStripMenuItem_Click);
            // 
            // materialenToolStripMenuItem
            // 
            this.materialenToolStripMenuItem.Image = global::ProductieManager.Properties.Resources.pngegg__1_;
            this.materialenToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.materialenToolStripMenuItem.Name = "materialenToolStripMenuItem";
            this.materialenToolStripMenuItem.Size = new System.Drawing.Size(188, 38);
            this.materialenToolStripMenuItem.Text = "Materialen";
            this.materialenToolStripMenuItem.ToolTipText = "Beheer materialen";
            this.materialenToolStripMenuItem.Click += new System.EventHandler(this.materialenToolStripMenuItem_Click);
            // 
            // xprodafkeurtoolstrip
            // 
            this.xprodafkeurtoolstrip.Image = global::ProductieManager.Properties.Resources.bin_icon_icons_com_32x32;
            this.xprodafkeurtoolstrip.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.xprodafkeurtoolstrip.Name = "xprodafkeurtoolstrip";
            this.xprodafkeurtoolstrip.Size = new System.Drawing.Size(188, 38);
            this.xprodafkeurtoolstrip.Text = "Afkeur";
            this.xprodafkeurtoolstrip.ToolTipText = "Vul in product afkeur";
            this.xprodafkeurtoolstrip.Click += new System.EventHandler(this.xprodafkeurtoolstrip_Click);
            // 
            // notitieToolStripMenuItem
            // 
            this.notitieToolStripMenuItem.Image = global::ProductieManager.Properties.Resources.Note_34576_32x32;
            this.notitieToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.notitieToolStripMenuItem.Name = "notitieToolStripMenuItem";
            this.notitieToolStripMenuItem.Size = new System.Drawing.Size(188, 38);
            this.notitieToolStripMenuItem.Text = "Notitie";
            this.notitieToolStripMenuItem.Click += new System.EventHandler(this.notitieToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(185, 6);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.xbewerking);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.ForeColor = System.Drawing.Color.Black;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(175, 53);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Kies Bewerking";
            // 
            // xbewerking
            // 
            this.xbewerking.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xbewerking.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xbewerking.ItemHeight = 23;
            this.xbewerking.Location = new System.Drawing.Point(4, 23);
            this.xbewerking.Margin = new System.Windows.Forms.Padding(4);
            this.xbewerking.Name = "xbewerking";
            this.xbewerking.Size = new System.Drawing.Size(167, 29);
            this.xbewerking.TabIndex = 0;
            this.toolTip1.SetToolTip(this.xbewerking, "Kies Bewerking");
            this.xbewerking.UseSelectable = true;
            // 
            // xprogressbar
            // 
            this.xprogressbar.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            this.xprogressbar.AnimationSpeed = 500;
            this.xprogressbar.BackColor = System.Drawing.Color.Transparent;
            this.xprogressbar.ContextMenuStrip = this.contextMenuStrip1;
            this.xprogressbar.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xprogressbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.xprogressbar.InnerColor = System.Drawing.Color.White;
            this.xprogressbar.InnerMargin = 5;
            this.xprogressbar.InnerWidth = 5;
            this.xprogressbar.Location = new System.Drawing.Point(3, 51);
            this.xprogressbar.Margin = new System.Windows.Forms.Padding(4);
            this.xprogressbar.MarqueeAnimationSpeed = 2000;
            this.xprogressbar.Name = "xprogressbar";
            this.xprogressbar.OuterColor = System.Drawing.Color.Gray;
            this.xprogressbar.OuterMargin = -30;
            this.xprogressbar.OuterWidth = 35;
            this.xprogressbar.ProgressColor = System.Drawing.Color.ForestGreen;
            this.xprogressbar.ProgressWidth = 12;
            this.xprogressbar.SecondaryFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xprogressbar.Size = new System.Drawing.Size(170, 167);
            this.xprogressbar.StartAngle = 270;
            this.xprogressbar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.xprogressbar.SubscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(166)))), ((int)(((byte)(166)))));
            this.xprogressbar.SubscriptMargin = new System.Windows.Forms.Padding(0);
            this.xprogressbar.SubscriptText = "";
            this.xprogressbar.SuperscriptColor = System.Drawing.Color.Black;
            this.xprogressbar.SuperscriptMargin = new System.Windows.Forms.Padding(-35, -5, 0, 0);
            this.xprogressbar.SuperscriptText = "";
            this.xprogressbar.TabIndex = 1;
            this.xprogressbar.Text = "68%";
            this.xprogressbar.TextMargin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.toolTip1.SetToolTip(this.xprogressbar, "Productie Voortgang");
            this.xprogressbar.Value = 68;
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Productie";
            // 
            // xstatuslabel
            // 
            this.xstatuslabel.ContextMenuStrip = this.contextMenuStrip1;
            this.xstatuslabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xstatuslabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xstatuslabel.Font = new System.Drawing.Font("Book Antiqua", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstatuslabel.ForeColor = System.Drawing.Color.DarkGreen;
            this.xstatuslabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xstatuslabel.Location = new System.Drawing.Point(40, 0);
            this.xstatuslabel.Name = "xstatuslabel";
            this.xstatuslabel.Padding = new System.Windows.Forms.Padding(5);
            this.xstatuslabel.Size = new System.Drawing.Size(778, 45);
            this.xstatuslabel.TabIndex = 0;
            this.xstatuslabel.Text = "Status";
            this.xstatuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.xstatuslabel, "Productie Status");
            this.xstatuslabel.SizeChanged += new System.EventHandler(this.xstatuslabel_SizeChanged);
            // 
            // xstopb
            // 
            this.xstopb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xstopb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xstopb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstopb.ForeColor = System.Drawing.Color.Black;
            this.xstopb.Image = global::ProductieManager.Properties.Resources.delete_1577;
            this.xstopb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xstopb.Location = new System.Drawing.Point(63, 431);
            this.xstopb.Margin = new System.Windows.Forms.Padding(4);
            this.xstopb.Name = "xstopb";
            this.xstopb.Size = new System.Drawing.Size(108, 35);
            this.xstopb.TabIndex = 1;
            this.xstopb.Text = "Sluiten";
            this.xstopb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xstopb, "Sluit productie pagina");
            this.xstopb.UseVisualStyleBackColor = true;
            this.xstopb.Click += new System.EventHandler(this.xstopb_Click);
            // 
            // xindelingb
            // 
            this.xindelingb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xindelingb.ContextMenuStrip = this.contextMenuStrip1;
            this.xindelingb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xindelingb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xindelingb.ForeColor = System.Drawing.Color.Black;
            this.xindelingb.Image = global::ProductieManager.Properties.Resources.iconfinder_technologymachineelectronic32_32;
            this.xindelingb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xindelingb.Location = new System.Drawing.Point(5, 226);
            this.xindelingb.Margin = new System.Windows.Forms.Padding(4);
            this.xindelingb.Name = "xindelingb";
            this.xindelingb.Size = new System.Drawing.Size(167, 35);
            this.xindelingb.TabIndex = 3;
            this.xindelingb.Text = "Indeling";
            this.xindelingb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xindelingb, "Beheer de indeling");
            this.xindelingb.UseVisualStyleBackColor = true;
            this.xindelingb.Click += new System.EventHandler(this.xindelingb_Click);
            // 
            // xstartb
            // 
            this.xstartb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xstartb.ContextMenuStrip = this.contextMenuStrip1;
            this.xstartb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xstartb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstartb.ForeColor = System.Drawing.Color.Black;
            this.xstartb.Image = global::ProductieManager.Properties.Resources.play_button_icon_icons_com_60615;
            this.xstartb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xstartb.Location = new System.Drawing.Point(4, 265);
            this.xstartb.Margin = new System.Windows.Forms.Padding(4);
            this.xstartb.Name = "xstartb";
            this.xstartb.Size = new System.Drawing.Size(167, 35);
            this.xstartb.TabIndex = 2;
            this.xstartb.Text = "Start";
            this.xstartb.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xstartb, "Start of stop de bewerking");
            this.xstartb.UseVisualStyleBackColor = true;
            this.xstartb.Click += new System.EventHandler(this.xstartb_Click);
            // 
            // xverpakking
            // 
            this.xverpakking.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xverpakking.ContextMenuStrip = this.contextMenuStrip1;
            this.xverpakking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xverpakking.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xverpakking.ForeColor = System.Drawing.Color.Black;
            this.xverpakking.Image = global::ProductieManager.Properties.Resources.package_box_10801;
            this.xverpakking.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xverpakking.Location = new System.Drawing.Point(4, 343);
            this.xverpakking.Margin = new System.Windows.Forms.Padding(4);
            this.xverpakking.Name = "xverpakking";
            this.xverpakking.Size = new System.Drawing.Size(167, 35);
            this.xverpakking.TabIndex = 4;
            this.xverpakking.Text = "Verpakking";
            this.xverpakking.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xverpakking, "Bekijk of wijzig de verpakkingsinstructies");
            this.xverpakking.UseVisualStyleBackColor = true;
            this.xverpakking.Click += new System.EventHandler(this.xverpakking_Click);
            // 
            // xtoonwerktekening
            // 
            this.xtoonwerktekening.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xtoonwerktekening.ContextMenuStrip = this.contextMenuStrip1;
            this.xtoonwerktekening.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xtoonwerktekening.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xtoonwerktekening.ForeColor = System.Drawing.Color.Black;
            this.xtoonwerktekening.Image = global::ProductieManager.Properties.Resources.libreoffice_draw_icon_181050;
            this.xtoonwerktekening.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xtoonwerktekening.Location = new System.Drawing.Point(4, 382);
            this.xtoonwerktekening.Margin = new System.Windows.Forms.Padding(4);
            this.xtoonwerktekening.Name = "xtoonwerktekening";
            this.xtoonwerktekening.Size = new System.Drawing.Size(167, 35);
            this.xtoonwerktekening.TabIndex = 5;
            this.xtoonwerktekening.Text = "Tekening";
            this.xtoonwerktekening.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xtoonwerktekening, "Toon werktekening");
            this.xtoonwerktekening.UseVisualStyleBackColor = true;
            this.xtoonwerktekening.Click += new System.EventHandler(this.xtoonwerktekening_Click);
            // 
            // xOnderbreek
            // 
            this.xOnderbreek.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xOnderbreek.ContextMenuStrip = this.contextMenuStrip1;
            this.xOnderbreek.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xOnderbreek.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xOnderbreek.ForeColor = System.Drawing.Color.Black;
            this.xOnderbreek.Image = global::ProductieManager.Properties.Resources.Stop_Hand__32x32;
            this.xOnderbreek.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xOnderbreek.Location = new System.Drawing.Point(4, 304);
            this.xOnderbreek.Margin = new System.Windows.Forms.Padding(4);
            this.xOnderbreek.Name = "xOnderbreek";
            this.xOnderbreek.Size = new System.Drawing.Size(167, 35);
            this.xOnderbreek.TabIndex = 6;
            this.xOnderbreek.Text = "Onderbreek";
            this.xOnderbreek.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.xOnderbreek, "Onderbreek de productie");
            this.xOnderbreek.UseVisualStyleBackColor = true;
            this.xOnderbreek.Click += new System.EventHandler(this.xOnderbreek_Click);
            // 
            // xpanelcontainer
            // 
            this.xpanelcontainer.AutoScroll = true;
            this.xpanelcontainer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.xpanelcontainer.Controls.Add(this.productieInfoUI1);
            this.xpanelcontainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xpanelcontainer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xpanelcontainer.Location = new System.Drawing.Point(40, 45);
            this.xpanelcontainer.Name = "xpanelcontainer";
            this.xpanelcontainer.Size = new System.Drawing.Size(643, 345);
            this.xpanelcontainer.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.ContextMenuStrip = this.contextMenuStrip1;
            this.panel2.Controls.Add(this.xOnderbreek);
            this.panel2.Controls.Add(this.xtoonwerktekening);
            this.panel2.Controls.Add(this.xverpakking);
            this.panel2.Controls.Add(this.xstopb);
            this.panel2.Controls.Add(this.xindelingb);
            this.panel2.Controls.Add(this.xstartb);
            this.panel2.Controls.Add(this.groupBox3);
            this.panel2.Controls.Add(this.xprogressbar);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(683, 45);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(175, 470);
            this.panel2.TabIndex = 12;
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.Controls.Add(this.xpanelcontainer);
            this.panel3.Controls.Add(this.xnotepanel);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Controls.Add(this.panel5);
            this.panel3.Controls.Add(this.mainMenu1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(858, 515);
            this.panel3.TabIndex = 6;
            // 
            // xnotepanel
            // 
            this.xnotepanel.BackColor = System.Drawing.Color.Maroon;
            this.xnotepanel.Controls.Add(this.xnoteTextbox);
            this.xnotepanel.Controls.Add(this.xnoteButton);
            this.xnotepanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.xnotepanel.Location = new System.Drawing.Point(40, 390);
            this.xnotepanel.Name = "xnotepanel";
            this.xnotepanel.Size = new System.Drawing.Size(643, 125);
            this.xnotepanel.TabIndex = 13;
            // 
            // xnoteTextbox
            // 
            this.xnoteTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xnoteTextbox.BackColor = System.Drawing.Color.White;
            this.xnoteTextbox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xnoteTextbox.Location = new System.Drawing.Point(2, 34);
            this.xnoteTextbox.Multiline = true;
            this.xnoteTextbox.Name = "xnoteTextbox";
            this.xnoteTextbox.ReadOnly = true;
            this.xnoteTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.xnoteTextbox.Size = new System.Drawing.Size(639, 90);
            this.xnoteTextbox.TabIndex = 1;
            this.xnoteTextbox.Text = "sdfsdfsdfsdfs rkjhf r rj oqhjeoir e";
            this.xnoteTextbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // xnoteButton
            // 
            this.xnoteButton.BackColor = System.Drawing.Color.Maroon;
            this.xnoteButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.xnoteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.xnoteButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xnoteButton.ForeColor = System.Drawing.SystemColors.Control;
            this.xnoteButton.Image = global::ProductieManager.Properties.Resources.exclamation_warning_24x24;
            this.xnoteButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.xnoteButton.Location = new System.Drawing.Point(0, 0);
            this.xnoteButton.Name = "xnoteButton";
            this.xnoteButton.Size = new System.Drawing.Size(643, 32);
            this.xnoteButton.TabIndex = 0;
            this.xnoteButton.Text = "LET OP! Er is een notitie geplaatst!";
            this.xnoteButton.UseVisualStyleBackColor = false;
            this.xnoteButton.Click += new System.EventHandler(this.xnoteButton_Click);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.xstatuslabel);
            this.panel5.Controls.Add(this.xstatusimage);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(40, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(818, 45);
            this.panel5.TabIndex = 5;
            // 
            // xstatusimage
            // 
            this.xstatusimage.ContextMenuStrip = this.contextMenuStrip1;
            this.xstatusimage.Dock = System.Windows.Forms.DockStyle.Left;
            this.xstatusimage.Location = new System.Drawing.Point(0, 0);
            this.xstatusimage.Name = "xstatusimage";
            this.xstatusimage.Size = new System.Drawing.Size(40, 45);
            this.xstatusimage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.xstatusimage.TabIndex = 1;
            this.xstatusimage.TabStop = false;
            // 
            // productieInfoUI1
            // 
            this.productieInfoUI1.AllowVerpakkingEdit = false;
            this.productieInfoUI1.AutoScroll = true;
            this.productieInfoUI1.BackColor = System.Drawing.Color.White;
            this.productieInfoUI1.ContextMenuStrip = this.contextMenuStrip1;
            this.productieInfoUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.productieInfoUI1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.productieInfoUI1.Location = new System.Drawing.Point(0, 0);
            this.productieInfoUI1.Margin = new System.Windows.Forms.Padding(4);
            this.productieInfoUI1.Name = "productieInfoUI1";
            this.productieInfoUI1.Padding = new System.Windows.Forms.Padding(5);
            this.productieInfoUI1.ShowAantal = true;
            this.productieInfoUI1.Size = new System.Drawing.Size(643, 345);
            this.productieInfoUI1.TabIndex = 0;
            // 
            // mainMenu1
            // 
            this.mainMenu1.ContextMenuStrip = this.contextMenuStrip1;
            this.mainMenu1.Dock = System.Windows.Forms.DockStyle.Left;
            this.mainMenu1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainMenu1.IsExpanded = false;
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            menuButton1.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton1.CombineImage = null;
            menuButton1.CombineScale = 1.5D;
            menuButton1.ContextMenu = this.contextMenuStrip1;
            menuButton1.Enabled = true;
            menuButton1.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton1.Image")));
            menuButton1.ImageSize = new System.Drawing.Size(32, 32);
            menuButton1.Index = 0;
            menuButton1.Name = "xwijzigproductie";
            menuButton1.Text = "Wijzig Productie";
            menuButton1.Tooltip = "Wijzig productie";
            menuButton2.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton2.CombineImage = null;
            menuButton2.CombineScale = 1.5D;
            menuButton2.ContextMenu = null;
            menuButton2.Enabled = true;
            menuButton2.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton2.Image")));
            menuButton2.ImageSize = new System.Drawing.Size(32, 32);
            menuButton2.Index = 1;
            menuButton2.Name = "xmeldgereed";
            menuButton2.Text = "Meld Gereed";
            menuButton2.Tooltip = "Meld bewerking gereed";
            menuButton3.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton3.CombineImage = null;
            menuButton3.CombineScale = 1.5D;
            menuButton3.ContextMenu = null;
            menuButton3.Enabled = true;
            menuButton3.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton3.Image")));
            menuButton3.ImageSize = new System.Drawing.Size(32, 32);
            menuButton3.Index = 2;
            menuButton3.Name = "xdeelmeldingen";
            menuButton3.Text = "Deels Gereedmeldingen";
            menuButton3.Tooltip = "Deels gereedmeldingen";
            menuButton4.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton4.CombineImage = null;
            menuButton4.CombineScale = 1.5D;
            menuButton4.ContextMenu = null;
            menuButton4.Enabled = true;
            menuButton4.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton4.Image")));
            menuButton4.ImageSize = new System.Drawing.Size(32, 32);
            menuButton4.Index = 3;
            menuButton4.Name = "xaantalgemaakt";
            menuButton4.Text = "Aantal Gemaakt";
            menuButton4.Tooltip = "Wijzig aantal gemaakt";
            menuButton5.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton5.CombineImage = null;
            menuButton5.CombineScale = 1.5D;
            menuButton5.ContextMenu = null;
            menuButton5.Enabled = true;
            menuButton5.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton5.Image")));
            menuButton5.ImageSize = new System.Drawing.Size(32, 32);
            menuButton5.Index = 4;
            menuButton5.Name = "xaanbevolenpersonen";
            menuButton5.Text = "Aanbevolen Personen";
            menuButton5.Tooltip = "Toon aanbevolen personeel";
            menuButton6.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton6.CombineImage = null;
            menuButton6.CombineScale = 1.5D;
            menuButton6.ContextMenu = null;
            menuButton6.Enabled = true;
            menuButton6.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton6.Image")));
            menuButton6.ImageSize = new System.Drawing.Size(32, 32);
            menuButton6.Index = 5;
            menuButton6.Name = "xrooster";
            menuButton6.Text = "Werk Rooster";
            menuButton6.Tooltip = "Wijzig  werkrooster";
            menuButton7.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton7.CombineImage = null;
            menuButton7.CombineScale = 1.5D;
            menuButton7.ContextMenu = null;
            menuButton7.Enabled = true;
            menuButton7.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton7.Image")));
            menuButton7.ImageSize = new System.Drawing.Size(32, 32);
            menuButton7.Index = 6;
            menuButton7.Name = "xonderbreking";
            menuButton7.Text = "Onderbrekeningen";
            menuButton7.Tooltip = "Wijzig/voeg onderbrekeningen";
            menuButton8.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton8.CombineImage = null;
            menuButton8.CombineScale = 1.5D;
            menuButton8.ContextMenu = null;
            menuButton8.Enabled = true;
            menuButton8.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton8.Image")));
            menuButton8.ImageSize = new System.Drawing.Size(32, 32);
            menuButton8.Index = 7;
            menuButton8.Name = "xwerktijden";
            menuButton8.Text = "Gewerkte Tijden";
            menuButton8.Tooltip = "Toon of wijzig gewerkte tijden";
            menuButton9.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton9.CombineImage = null;
            menuButton9.CombineScale = 1.5D;
            menuButton9.ContextMenu = null;
            menuButton9.Enabled = true;
            menuButton9.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton9.Image")));
            menuButton9.ImageSize = new System.Drawing.Size(32, 32);
            menuButton9.Index = 8;
            menuButton9.Name = "xopenpdf";
            menuButton9.Text = "Productie Pdf";
            menuButton9.Tooltip = "Open productieformulier als pdf";
            menuButton10.AccesLevel = Rpm.Various.AccesType.AlleenKijken;
            menuButton10.CombineImage = null;
            menuButton10.CombineScale = 1.5D;
            menuButton10.ContextMenu = null;
            menuButton10.Enabled = true;
            menuButton10.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton10.Image")));
            menuButton10.ImageSize = new System.Drawing.Size(32, 32);
            menuButton10.Index = 9;
            menuButton10.Name = "xundo";
            menuButton10.Text = "Terug Zetten";
            menuButton10.Tooltip = "Zet een verwijderd of een gereed productie terug naar de normale status";
            menuButton11.AccesLevel = Rpm.Various.AccesType.ProductieBasis;
            menuButton11.CombineImage = null;
            menuButton11.CombineScale = 1.5D;
            menuButton11.ContextMenu = null;
            menuButton11.Enabled = true;
            menuButton11.Image = ((System.Drawing.Bitmap)(resources.GetObject("menuButton11.Image")));
            menuButton11.ImageSize = new System.Drawing.Size(32, 32);
            menuButton11.Index = 10;
            menuButton11.Name = "xdelete";
            menuButton11.Text = "Verwijderen";
            menuButton11.Tooltip = "Productie Verwijderen";
            this.mainMenu1.MenuButtons = new Various.MenuButton[] {
        menuButton1,
        menuButton2,
        menuButton3,
        menuButton4,
        menuButton5,
        menuButton6,
        menuButton7,
        menuButton8,
        menuButton9,
        menuButton10,
        menuButton11};
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(40, 515);
            this.mainMenu1.TabIndex = 4;
            this.mainMenu1.OnMenuClick += new System.EventHandler(this.mainMenu1_OnMenuClick);
            // 
            // ProductieForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel3);
            this.DoubleBuffered = true;
            this.Name = "ProductieForm";
            this.Size = new System.Drawing.Size(858, 515);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.xpanelcontainer.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.xnotepanel.ResumeLayout(false);
            this.xnotepanel.PerformLayout();
            this.panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xstatusimage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private MetroFramework.Controls.MetroComboBox xbewerking;
        private CircularProgressBar.CircularProgressBar xprogressbar;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel xpanelcontainer;
        private System.Windows.Forms.Label xstatuslabel;
        private MainMenu mainMenu1;
        private System.Windows.Forms.Button xstopb;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button xindelingb;
        private System.Windows.Forms.Button xstartb;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.PictureBox xstatusimage;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aantalTeMakenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leverdatumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem notitieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xprodafkeurtoolstrip;
        private System.Windows.Forms.Panel xnotepanel;
        private System.Windows.Forms.Button xnoteButton;
        private System.Windows.Forms.TextBox xnoteTextbox;
        private System.Windows.Forms.Button xverpakking;
        private System.Windows.Forms.ToolStripMenuItem materialenToolStripMenuItem;
        private System.Windows.Forms.Button xtoonwerktekening;
        private System.Windows.Forms.Button xOnderbreek;
        private ProductieInfoUI productieInfoUI1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
