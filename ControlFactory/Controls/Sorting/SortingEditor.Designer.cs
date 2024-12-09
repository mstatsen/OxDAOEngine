using OxLibrary;

namespace OxDAOEngine.ControlFactory.Controls
{
    partial class SortingEditor<TField, TDAO>
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
            this.ClientSize = new(390, 230);
            this.MaximumSize = new OxSize(OxWh.W390, OxWh.W230);
            this.MinimumSize = new OxSize(OxWh.W390, OxWh.W230);
            this.Name = "SortingEditor";
            this.Text = "Sorting";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
