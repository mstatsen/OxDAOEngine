using OxLibrary;

namespace OxDAOEngine.Settings
{
    partial class ExportSettingsForm<TField, TDAO>
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
            // HTMLSettingsForm
            // 
            this.ClientSize = new OxSize(OxWh.W487, OxWh.W447);
            this.Name = "HTMLSettingsForm";
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.ExportSettingsForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
