﻿namespace Forms
{
    partial class LogIn
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogIn));
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.xwachtwoord1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.xgebruikersname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.xautologin = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(189, 207);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(121, 36);
            this.button2.TabIndex = 19;
            this.button2.Text = "Annuleer";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(62, 207);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 36);
            this.button1.TabIndex = 18;
            this.button1.Text = "Log In";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // xwachtwoord1
            // 
            this.xwachtwoord1.BackColor = System.Drawing.Color.White;
            this.xwachtwoord1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xwachtwoord1.ForeColor = System.Drawing.Color.Black;
            this.xwachtwoord1.Location = new System.Drawing.Point(27, 140);
            this.xwachtwoord1.Name = "xwachtwoord1";
            this.xwachtwoord1.PasswordChar = '*';
            this.xwachtwoord1.Size = new System.Drawing.Size(283, 29);
            this.xwachtwoord1.TabIndex = 13;
            this.xwachtwoord1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.xwachtwoord1_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(23, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 21);
            this.label2.TabIndex = 12;
            this.label2.Text = "Wachtwoord";
            // 
            // xgebruikersname
            // 
            this.xgebruikersname.BackColor = System.Drawing.Color.White;
            this.xgebruikersname.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xgebruikersname.ForeColor = System.Drawing.Color.Black;
            this.xgebruikersname.Location = new System.Drawing.Point(27, 84);
            this.xgebruikersname.Name = "xgebruikersname";
            this.xgebruikersname.Size = new System.Drawing.Size(283, 29);
            this.xgebruikersname.TabIndex = 11;
            this.xgebruikersname.Text = "Vul in je gebruikersnaam...";
            this.xgebruikersname.Enter += new System.EventHandler(this.xgebruikersname_MouseEnter);
            this.xgebruikersname.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.xwachtwoord1_KeyPress);
            this.xgebruikersname.Leave += new System.EventHandler(this.xgebruikersname_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(23, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 21);
            this.label1.TabIndex = 10;
            this.label1.Text = "Gebruikers Name";
            // 
            // xautologin
            // 
            this.xautologin.AutoSize = true;
            this.xautologin.Location = new System.Drawing.Point(27, 176);
            this.xautologin.Name = "xautologin";
            this.xautologin.Size = new System.Drawing.Size(105, 25);
            this.xautologin.TabIndex = 20;
            this.xautologin.Text = "Auto Login";
            this.xautologin.UseVisualStyleBackColor = true;
            // 
            // LogIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = MetroFramework.Forms.MetroFormBorderStyle.FixedSingle;
            this.ClientSize = new System.Drawing.Size(325, 270);
            this.Controls.Add(this.xautologin);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.xwachtwoord1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.xgebruikersname);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(325, 270);
            this.Name = "LogIn";
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Log In";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox xwachtwoord1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox xgebruikersname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox xautologin;
    }
}