﻿using OxLibrary;
using OxLibrary.Forms;

namespace OxDAOEngine.Editor
{
    partial class UniqueKeyViewer<TField, TDAO> : OxDialog
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
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Size = new OxSize(574, 398);
            // 
            // ListItemEditor
            // 
            this.Name = "ListItemEditor";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
