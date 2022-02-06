using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ProductieManager.Forms.MetroDock
{
    public class DockInstance : MetroForm, IDockContent, IContextMenuStripHost
    {
        private readonly DockContentHandler m_dockHandler;
        private string m_tabText;
        private static readonly object DockChangedEvent = new object();

        public DockInstance()
        {

            this.m_dockHandler =
                new DockContentHandler((Form) this, new GetPersistStringCallback(this.GetPersistString));
            InitializeComponent();
            this.m_dockHandler.DockStateChanged += new EventHandler(this.DockHandler_DockStateChanged);
            var fontInheritanceFix = PatchController.EnableFontInheritanceFix;
            MaximizeBox = false;
            bool flag = true;
            if (fontInheritanceFix.GetValueOrDefault() == true & fontInheritanceFix.HasValue)
                return;
            this.ParentChanged += new EventHandler(this.DockContent_ParentChanged);
        }

        private void DockContent_ParentChanged(object Sender, EventArgs e)
        {
            if (this.Parent == null)
                return;
            this.Font = this.Parent.Font;
        }

        [Browsable(false)] public DockContentHandler DockHandler => this.m_dockHandler;

        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get => this.DockHandler.AllowEndUserDocking;
            set => this.DockHandler.AllowEndUserDocking = value;
        }

        [DefaultValue(DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop |
                      DockAreas.DockBottom | DockAreas.Document)]
        public DockAreas DockAreas
        {
            get => this.DockHandler.DockAreas;
            set => this.DockHandler.DockAreas = value;
        }

        [DefaultValue(0.25)]
        public double AutoHidePortion
        {
            get => this.DockHandler.AutoHidePortion;
            set => this.DockHandler.AutoHidePortion = value;
        }

        [DefaultValue(null)]
        public string TabText
        {
            get => this.m_tabText;
            set => this.DockHandler.TabText = this.m_tabText = value;
        }

        private bool ShouldSerializeTabText() => this.m_tabText != null;

        [DefaultValue(true)]
        public bool CloseButton
        {
            get => this.DockHandler.CloseButton;
            set => this.DockHandler.CloseButton = value;
        }

        [DefaultValue(true)]
        public bool CloseButtonVisible
        {
            get => this.DockHandler.CloseButtonVisible;
            set => this.DockHandler.CloseButtonVisible = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPanel DockPanel
        {
            get => this.DockHandler.DockPanel;
            set => this.DockHandler.DockPanel = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockState DockState
        {
            get => this.DockHandler.DockState;
            set => this.DockHandler.DockState = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPane Pane
        {
            get => this.DockHandler.Pane;
            set => this.DockHandler.Pane = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHidden
        {
            get => this.DockHandler.IsHidden;
            set => this.DockHandler.IsHidden = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockState VisibleState
        {
            get => this.DockHandler.VisibleState;
            set => this.DockHandler.VisibleState = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsFloat
        {
            get => this.DockHandler.IsFloat;
            set => this.DockHandler.IsFloat = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPane PanelPane
        {
            get => this.DockHandler.PanelPane;
            set => this.DockHandler.PanelPane = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPane FloatPane
        {
            get => this.DockHandler.FloatPane;
            set => this.DockHandler.FloatPane = value;
        }

        protected virtual string GetPersistString() => this.GetType().ToString();

        [DefaultValue(false)]
        public bool HideOnClose
        {
            get => this.DockHandler.HideOnClose;
            set => this.DockHandler.HideOnClose = value;
        }

        [DefaultValue(DockState.Unknown)]
        public DockState ShowHint
        {
            get => this.DockHandler.ShowHint;
            set => this.DockHandler.ShowHint = value;
        }

        [Browsable(false)] public bool IsActivated => this.DockHandler.IsActivated;

        public bool IsDockStateValid(DockState dockState) => this.DockHandler.IsDockStateValid(dockState);

        [DefaultValue(null)]
        public ContextMenu TabPageContextMenu
        {
            get => this.DockHandler.TabPageContextMenu;
            set => this.DockHandler.TabPageContextMenu = value;
        }

        [DefaultValue(null)]
        public ContextMenuStrip TabPageContextMenuStrip
        {
            get => this.DockHandler.TabPageContextMenuStrip;
            set => this.DockHandler.TabPageContextMenuStrip = value;
        }

        void IContextMenuStripHost.ApplyTheme()
        {
            //this.DockHandler.ApplyTheme();
            if (this.DockPanel == null)
                return;
            if (this.MainMenuStrip != null)
                this.DockPanel.Theme.ApplyTo((ToolStrip) this.MainMenuStrip);
            if (this.ContextMenuStrip == null)
                return;
            this.DockPanel.Theme.ApplyTo((ToolStrip) this.ContextMenuStrip);
        }

        [Localizable(true)]
        [Category("Appearance")]
        [DefaultValue(null)]
        public string ToolTipText
        {
            get => this.DockHandler.ToolTipText;
            set => this.DockHandler.ToolTipText = value;
        }

        public new void Activate() => this.DockHandler.Activate();

        public new void Hide() => this.DockHandler.Hide();

        public new void Show() => this.DockHandler.Show();

        public void Show(DockPanel dockPanel) => this.DockHandler.Show(dockPanel);

        public void Show(DockPanel dockPanel, DockState dockState) => this.DockHandler.Show(dockPanel, dockState);

        public void Show(DockPanel dockPanel, Rectangle floatWindowBounds) => this.DockHandler.Show(dockPanel, floatWindowBounds);

        public void Show(DockPane pane, IDockContent beforeContent) => this.DockHandler.Show(pane, beforeContent);

        public void Show(DockPane previousPane, DockAlignment alignment, double proportion) => this.DockHandler.Show(previousPane, alignment, proportion);

        public void FloatAt(Rectangle floatWindowBounds) => this.DockHandler.FloatAt(floatWindowBounds);

        public void DockTo(DockPane paneTo, DockStyle dockStyle, int contentIndex) => this.DockHandler.DockTo(paneTo, dockStyle, contentIndex);

        public void DockTo(DockPanel panel, DockStyle dockStyle) => this.DockHandler.DockTo(panel, dockStyle);

        void IDockContent.OnActivated(EventArgs e) => this.OnActivated(e);

        void IDockContent.OnDeactivate(EventArgs e) => this.OnDeactivate(e);

        private void DockHandler_DockStateChanged(object sender, EventArgs e) => this.OnDockStateChanged(e);

        public event EventHandler DockStateChanged
        {
            add => this.Events.AddHandler(DockChangedEvent, (Delegate)value);
            remove => this.Events.RemoveHandler(DockChangedEvent, (Delegate)value);
        }

        protected virtual void OnDockStateChanged(EventArgs e)
        {
            EventHandler eventHandler = (EventHandler)this.Events[DockChangedEvent];
            if (eventHandler == null)
                return;
            eventHandler((object)this, e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.DockPanel is { SupportDeeplyNestedContent: true } && this.IsHandleCreated)
                this.BeginInvoke(new Action(() => base.OnSizeChanged(e)));
            else
                base.OnSizeChanged(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DockInstance
            // 
            this.ClientSize = new System.Drawing.Size(583, 431);
            this.ControlBox = false;
            this.DisplayHeader = false;
            this.Movable = false;
            this.Name = "DockInstance";
            this.Padding = new System.Windows.Forms.Padding(10, 30, 10, 10);
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Empty;
            this.ResumeLayout(false);

        }

        private void xsluiten_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
