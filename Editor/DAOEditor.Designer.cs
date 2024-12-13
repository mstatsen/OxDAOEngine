using OxLibrary;
using OxLibrary.Forms;

namespace OxDAOEngine.Editor
{
    partial class DAOEditor<TField, TDAO, TFieldGroup>
        : OxDialog
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
            // AbstractEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.SetAutoSize(true);
            this.ClientSize = new(1225, 636);
            this.Location = new OxPoint(0, 0);
            this.MaximumSize = new OxSize(1225, 636);
            this.MinimumSize = new OxSize(1225, 636);
            this.Name = "AbstractEditor";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Game";
            this.ResumeLayout(false);

        }

        #endregion
    }
}