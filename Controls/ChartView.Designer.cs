﻿
namespace Controls
{
    partial class ChartView
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
            this.xdatachart = new LiveCharts.WinForms.CartesianChart();
            this.panel1 = new System.Windows.Forms.Panel();
            this.xstatuslabel = new System.Windows.Forms.Label();
            this.xoptionpanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.xbewerkingradio = new System.Windows.Forms.RadioButton();
            this.xwerkplekradio = new System.Windows.Forms.RadioButton();
            this.xweergaveperiodegroup = new System.Windows.Forms.GroupBox();
            this.xalleennucheckbox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.xstartjaar = new System.Windows.Forms.NumericUpDown();
            this.xstartweek = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.xstoringenradio = new System.Windows.Forms.RadioButton();
            this.xaantalperuurradio = new System.Windows.Forms.RadioButton();
            this.xaantalgemaaktradio = new System.Windows.Forms.RadioButton();
            this.xtijdgewerktradio = new System.Windows.Forms.RadioButton();
            this.xserieslist = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel3 = new System.Windows.Forms.Panel();
            this.xstatus = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.xoptionpanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.xweergaveperiodegroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xstartjaar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xstartweek)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xserieslist)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // xdatachart
            // 
            this.xdatachart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xdatachart.Location = new System.Drawing.Point(208, 167);
            this.xdatachart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.xdatachart.Name = "xdatachart";
            this.xdatachart.Size = new System.Drawing.Size(665, 391);
            this.xdatachart.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.xstatuslabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(873, 36);
            this.panel1.TabIndex = 1;
            // 
            // xstatuslabel
            // 
            this.xstatuslabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xstatuslabel.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstatuslabel.Location = new System.Drawing.Point(0, 0);
            this.xstatuslabel.Name = "xstatuslabel";
            this.xstatuslabel.Size = new System.Drawing.Size(873, 36);
            this.xstatuslabel.TabIndex = 0;
            this.xstatuslabel.Text = "Tijd gewerkt van de afgelopen x weken";
            this.xstatuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // xoptionpanel
            // 
            this.xoptionpanel.AutoScroll = true;
            this.xoptionpanel.Controls.Add(this.groupBox1);
            this.xoptionpanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.xoptionpanel.Location = new System.Drawing.Point(0, 36);
            this.xoptionpanel.Name = "xoptionpanel";
            this.xoptionpanel.Size = new System.Drawing.Size(873, 131);
            this.xoptionpanel.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.xweergaveperiodegroup);
            this.groupBox1.Controls.Add(this.xstoringenradio);
            this.groupBox1.Controls.Add(this.xaantalperuurradio);
            this.groupBox1.Controls.Add(this.xaantalgemaaktradio);
            this.groupBox1.Controls.Add(this.xtijdgewerktradio);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(873, 129);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Weergave";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.xbewerkingradio);
            this.groupBox3.Controls.Add(this.xwerkplekradio);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(162, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(140, 129);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Soort Weergave";
            // 
            // xbewerkingradio
            // 
            this.xbewerkingradio.AutoSize = true;
            this.xbewerkingradio.Checked = true;
            this.xbewerkingradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xbewerkingradio.Location = new System.Drawing.Point(6, 26);
            this.xbewerkingradio.Name = "xbewerkingradio";
            this.xbewerkingradio.Size = new System.Drawing.Size(99, 21);
            this.xbewerkingradio.TabIndex = 1;
            this.xbewerkingradio.TabStop = true;
            this.xbewerkingradio.Text = "Bewerkingen";
            this.xbewerkingradio.UseVisualStyleBackColor = true;
            this.xbewerkingradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xwerkplekradio
            // 
            this.xwerkplekradio.AutoSize = true;
            this.xwerkplekradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xwerkplekradio.Location = new System.Drawing.Point(6, 53);
            this.xwerkplekradio.Name = "xwerkplekradio";
            this.xwerkplekradio.Size = new System.Drawing.Size(99, 21);
            this.xwerkplekradio.TabIndex = 0;
            this.xwerkplekradio.Text = "Werkplekken";
            this.xwerkplekradio.UseVisualStyleBackColor = true;
            this.xwerkplekradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xweergaveperiodegroup
            // 
            this.xweergaveperiodegroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xweergaveperiodegroup.Controls.Add(this.xalleennucheckbox);
            this.xweergaveperiodegroup.Controls.Add(this.label2);
            this.xweergaveperiodegroup.Controls.Add(this.xstartjaar);
            this.xweergaveperiodegroup.Controls.Add(this.xstartweek);
            this.xweergaveperiodegroup.Controls.Add(this.label1);
            this.xweergaveperiodegroup.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xweergaveperiodegroup.Location = new System.Drawing.Point(301, 0);
            this.xweergaveperiodegroup.Name = "xweergaveperiodegroup";
            this.xweergaveperiodegroup.Size = new System.Drawing.Size(572, 129);
            this.xweergaveperiodegroup.TabIndex = 2;
            this.xweergaveperiodegroup.TabStop = false;
            this.xweergaveperiodegroup.Text = "Weergave Periode";
            // 
            // xalleennucheckbox
            // 
            this.xalleennucheckbox.AutoSize = true;
            this.xalleennucheckbox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xalleennucheckbox.Location = new System.Drawing.Point(6, 81);
            this.xalleennucheckbox.Name = "xalleennucheckbox";
            this.xalleennucheckbox.Size = new System.Drawing.Size(123, 21);
            this.xalleennucheckbox.TabIndex = 4;
            this.xalleennucheckbox.Text = "Alleen Vandaag";
            this.xalleennucheckbox.UseVisualStyleBackColor = true;
            this.xalleennucheckbox.CheckedChanged += new System.EventHandler(this.xalleennucheckbox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(73, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "/";
            // 
            // xstartjaar
            // 
            this.xstartjaar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstartjaar.Location = new System.Drawing.Point(94, 50);
            this.xstartjaar.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.xstartjaar.Minimum = new decimal(new int[] {
            2021,
            0,
            0,
            0});
            this.xstartjaar.Name = "xstartjaar";
            this.xstartjaar.Size = new System.Drawing.Size(62, 25);
            this.xstartjaar.TabIndex = 2;
            this.xstartjaar.Value = new decimal(new int[] {
            2021,
            0,
            0,
            0});
            this.xstartjaar.ValueChanged += new System.EventHandler(this.xstartweek_ValueChanged);
            // 
            // xstartweek
            // 
            this.xstartweek.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstartweek.Location = new System.Drawing.Point(6, 50);
            this.xstartweek.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
            this.xstartweek.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.xstartweek.Name = "xstartweek";
            this.xstartweek.Size = new System.Drawing.Size(62, 25);
            this.xstartweek.TabIndex = 1;
            this.xstartweek.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.xstartweek.ValueChanged += new System.EventHandler(this.xstartweek_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vanaf Week/Jaar";
            // 
            // xstoringenradio
            // 
            this.xstoringenradio.AutoSize = true;
            this.xstoringenradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstoringenradio.Location = new System.Drawing.Point(6, 96);
            this.xstoringenradio.Name = "xstoringenradio";
            this.xstoringenradio.Size = new System.Drawing.Size(133, 21);
            this.xstoringenradio.TabIndex = 3;
            this.xstoringenradio.Text = "Tijd Aan Storingen";
            this.xstoringenradio.UseVisualStyleBackColor = true;
            this.xstoringenradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xaantalperuurradio
            // 
            this.xaantalperuurradio.AutoSize = true;
            this.xaantalperuurradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xaantalperuurradio.Location = new System.Drawing.Point(6, 73);
            this.xaantalperuurradio.Name = "xaantalperuurradio";
            this.xaantalperuurradio.Size = new System.Drawing.Size(110, 21);
            this.xaantalperuurradio.TabIndex = 2;
            this.xaantalperuurradio.Text = "Aantal Per Uur";
            this.xaantalperuurradio.UseVisualStyleBackColor = true;
            this.xaantalperuurradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xaantalgemaaktradio
            // 
            this.xaantalgemaaktradio.AutoSize = true;
            this.xaantalgemaaktradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xaantalgemaaktradio.Location = new System.Drawing.Point(6, 50);
            this.xaantalgemaaktradio.Name = "xaantalgemaaktradio";
            this.xaantalgemaaktradio.Size = new System.Drawing.Size(117, 21);
            this.xaantalgemaaktradio.TabIndex = 1;
            this.xaantalgemaaktradio.Text = "Aantal Gemaakt";
            this.xaantalgemaaktradio.UseVisualStyleBackColor = true;
            this.xaantalgemaaktradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xtijdgewerktradio
            // 
            this.xtijdgewerktradio.AutoSize = true;
            this.xtijdgewerktradio.Checked = true;
            this.xtijdgewerktradio.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xtijdgewerktradio.Location = new System.Drawing.Point(6, 26);
            this.xtijdgewerktradio.Name = "xtijdgewerktradio";
            this.xtijdgewerktradio.Size = new System.Drawing.Size(98, 21);
            this.xtijdgewerktradio.TabIndex = 0;
            this.xtijdgewerktradio.TabStop = true;
            this.xtijdgewerktradio.Text = "Tijd Gewerkt";
            this.xtijdgewerktradio.UseVisualStyleBackColor = true;
            this.xtijdgewerktradio.CheckedChanged += new System.EventHandler(this.radiocheckchanged_CheckedChanged);
            // 
            // xserieslist
            // 
            this.xserieslist.AllColumns.Add(this.olvColumn1);
            this.xserieslist.CellEditUseWholeCell = false;
            this.xserieslist.CheckBoxes = true;
            this.xserieslist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.xserieslist.Cursor = System.Windows.Forms.Cursors.Default;
            this.xserieslist.Dock = System.Windows.Forms.DockStyle.Left;
            this.xserieslist.FullRowSelect = true;
            this.xserieslist.HideSelection = false;
            this.xserieslist.Location = new System.Drawing.Point(0, 167);
            this.xserieslist.Name = "xserieslist";
            this.xserieslist.ShowGroups = false;
            this.xserieslist.ShowItemToolTips = true;
            this.xserieslist.Size = new System.Drawing.Size(208, 391);
            this.xserieslist.TabIndex = 5;
            this.xserieslist.UseCompatibleStateImageBehavior = false;
            this.xserieslist.UseExplorerTheme = true;
            this.xserieslist.UseHotItem = true;
            this.xserieslist.UseTranslucentHotItem = true;
            this.xserieslist.UseTranslucentSelection = true;
            this.xserieslist.View = System.Windows.Forms.View.Details;
            this.xserieslist.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.xserieslist_ItemChecked);
            this.xserieslist.SelectedIndexChanged += new System.EventHandler(this.xserieslist_SelectedIndexChanged);
            this.xserieslist.DoubleClick += new System.EventHandler(this.xserieslist_DoubleClick);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Title";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "Series";
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.Controls.Add(this.xstatus);
            this.panel3.Controls.Add(this.xdatachart);
            this.panel3.Controls.Add(this.xserieslist);
            this.panel3.Controls.Add(this.xoptionpanel);
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(873, 558);
            this.panel3.TabIndex = 4;
            // 
            // xstatus
            // 
            this.xstatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xstatus.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xstatus.Location = new System.Drawing.Point(3, 170);
            this.xstatus.Name = "xstatus";
            this.xstatus.Size = new System.Drawing.Size(870, 388);
            this.xstatus.TabIndex = 6;
            this.xstatus.Text = "Producties Laden...";
            this.xstatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.AutoScroll = true;
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(873, 558);
            this.panel4.TabIndex = 5;
            // 
            // ChartView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel4);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ChartView";
            this.Size = new System.Drawing.Size(873, 558);
            this.panel1.ResumeLayout(false);
            this.xoptionpanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.xweergaveperiodegroup.ResumeLayout(false);
            this.xweergaveperiodegroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xstartjaar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xstartweek)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xserieslist)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private LiveCharts.WinForms.CartesianChart xdatachart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label xstatuslabel;
        private System.Windows.Forms.Panel xoptionpanel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton xbewerkingradio;
        private System.Windows.Forms.RadioButton xwerkplekradio;
        private System.Windows.Forms.GroupBox xweergaveperiodegroup;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton xstoringenradio;
        private System.Windows.Forms.RadioButton xaantalperuurradio;
        private System.Windows.Forms.RadioButton xaantalgemaaktradio;
        private System.Windows.Forms.RadioButton xtijdgewerktradio;
        private BrightIdeasSoftware.ObjectListView xserieslist;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown xstartjaar;
        private System.Windows.Forms.NumericUpDown xstartweek;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox xalleennucheckbox;
        private System.Windows.Forms.Label xstatus;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
