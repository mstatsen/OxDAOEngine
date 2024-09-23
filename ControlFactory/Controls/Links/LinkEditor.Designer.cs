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
            // MainPanel
            // 
            this.MainPanel.Size = new System.Drawing.Size(390, 230);
            this.MainPanel.Text = "Link";
            // 
            // LinkEditor
            // 
            this.ClientSize = new System.Drawing.Size(390, 230);
            this.MaximumSize = new System.Drawing.Size(390, 230);
            this.MinimumSize = new System.Drawing.Size(390, 230);
            this.Name = "LinkEditor";
            this.Text = "Link";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
