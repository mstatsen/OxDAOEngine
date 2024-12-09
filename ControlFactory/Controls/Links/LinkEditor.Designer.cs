using OxLibrary;

namespace OxDAOEngine.ControlFactory.Controls
{
    partial class LinkEditor<TField, TDAO>
    {
        
        /// Required designer variable.
        
        private System.ComponentModel.IContainer components = null;

        
        /// Clean up any resources being used.
        
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

        
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormPanel
            // 
            this.FormPanel.Size = new OxSize(390, 230);
            // 
            // LinkEditor
            // 
            this.ClientSize = new(390, 230);
            this.MaximumSize = new OxSize(390, 230);
            this.MinimumSize = new OxSize(390, 230);
            this.Name = "LinkEditor";
            this.Text = "Link";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
