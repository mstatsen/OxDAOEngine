﻿namespace OxDAOEngine.Settings
{
    partial class SettingsForm
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
            SuspendLayout();
            // 
            // SettingsForm
            // 
            ClientSize = new Size(349, 0);
            Location = new Point(0, 0);
            MaximumSize = new Size(349, 0);
            MinimumSize = new Size(349, 0);
            Name = "SettingsForm";
            Text = "Settings";
            Shown += SettingsForm_Shown;
            ResumeLayout(false);
        }

        #endregion
    }
}
