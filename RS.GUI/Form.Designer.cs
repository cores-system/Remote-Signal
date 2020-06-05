using System;
using System.Drawing;

namespace RS.GUI
{
    partial class Form
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.tb_log = new System.Windows.Forms.TextBox();
            this.btn_autorun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tb_log
            // 
            this.tb_log.Enabled = false;
            this.tb_log.Font = new Font(this.tb_log.Font.FontFamily, 12);
            this.tb_log.Location = new System.Drawing.Point(12, 45);
            this.tb_log.Multiline = true;
            this.tb_log.Name = "tb_log";
            this.tb_log.Size = new System.Drawing.Size(697, 320);
            this.tb_log.TabIndex = 2;
            // 
            // btn_autorun
            // 
            this.btn_autorun.Location = new System.Drawing.Point(12, 12);
            this.btn_autorun.Name = "btn_autorun";
            this.btn_autorun.Size = new System.Drawing.Size(200, 23);
            this.btn_autorun.TabIndex = 3;
            this.btn_autorun.Text = "Добавить в автозапуск";
            this.btn_autorun.UseVisualStyleBackColor = true;
            this.btn_autorun.Click += new System.EventHandler(this.AutorunSet);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(721, 377);
            this.Controls.Add(this.btn_autorun);
            this.Controls.Add(this.tb_log);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remote Signal";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_log;
        private System.Windows.Forms.Button btn_autorun;
    }
}

