namespace KCUnivDB
{
    partial class StudentDashboard
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StudentDashboard));
            this.btnMinimize = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Label();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2GradientPanel1 = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.btnHome = new Guna.UI2.WinForms.Guna2Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.guna2Elipse2 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.guna2GradientPanel2 = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblDateDisplay = new System.Windows.Forms.Label();
            this.guna2Elipse3 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.lblStudentName = new System.Windows.Forms.Label();
            this.lblLogout = new System.Windows.Forms.Label();
            this.btnPersonalInformation = new Guna.UI2.WinForms.Guna2Button();
            this.btnEnrollment = new Guna.UI2.WinForms.Guna2Button();
            this.guna2GradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.guna2GradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // btnMinimize
            // 
            this.btnMinimize.AutoSize = true;
            this.btnMinimize.Font = new System.Drawing.Font("Arial Rounded MT Bold", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMinimize.ForeColor = System.Drawing.Color.Maroon;
            this.btnMinimize.Location = new System.Drawing.Point(1025, 1);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(33, 29);
            this.btnMinimize.TabIndex = 44;
            this.btnMinimize.Text = "🗕";
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.Font = new System.Drawing.Font("Arial Rounded MT Bold", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.Maroon;
            this.btnClose.Location = new System.Drawing.Point(1062, 9);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(28, 29);
            this.btnClose.TabIndex = 43;
            this.btnClose.Text = "×";
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.BorderRadius = 10;
            this.guna2Elipse1.TargetControl = this;
            // 
            // guna2GradientPanel1
            // 
            this.guna2GradientPanel1.BackColor = System.Drawing.Color.Firebrick;
            this.guna2GradientPanel1.Controls.Add(this.btnEnrollment);
            this.guna2GradientPanel1.Controls.Add(this.btnPersonalInformation);
            this.guna2GradientPanel1.Controls.Add(this.btnHome);
            this.guna2GradientPanel1.Controls.Add(this.pictureBox1);
            this.guna2GradientPanel1.Location = new System.Drawing.Point(12, 12);
            this.guna2GradientPanel1.Name = "guna2GradientPanel1";
            this.guna2GradientPanel1.Size = new System.Drawing.Size(232, 705);
            this.guna2GradientPanel1.TabIndex = 45;
            // 
            // btnHome
            // 
            this.btnHome.Animated = true;
            this.btnHome.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnHome.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnHome.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnHome.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnHome.FillColor = System.Drawing.Color.Firebrick;
            this.btnHome.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHome.ForeColor = System.Drawing.Color.White;
            this.btnHome.Image = ((System.Drawing.Image)(resources.GetObject("btnHome.Image")));
            this.btnHome.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnHome.ImageOffset = new System.Drawing.Point(10, 0);
            this.btnHome.Location = new System.Drawing.Point(0, 204);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(232, 58);
            this.btnHome.TabIndex = 48;
            this.btnHome.Text = "Home";
            this.btnHome.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.btnHome.TextOffset = new System.Drawing.Point(-20, 0);
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(16, -40);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(193, 260);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 47;
            this.pictureBox1.TabStop = false;
            // 
            // guna2Elipse2
            // 
            this.guna2Elipse2.BorderRadius = 10;
            this.guna2Elipse2.TargetControl = this.guna2GradientPanel1;
            // 
            // guna2GradientPanel2
            // 
            this.guna2GradientPanel2.BackColor = System.Drawing.Color.Firebrick;
            this.guna2GradientPanel2.Controls.Add(this.pictureBox2);
            this.guna2GradientPanel2.Controls.Add(this.label2);
            this.guna2GradientPanel2.Controls.Add(this.lblWelcome);
            this.guna2GradientPanel2.Controls.Add(this.lblDateDisplay);
            this.guna2GradientPanel2.Location = new System.Drawing.Point(284, 92);
            this.guna2GradientPanel2.Name = "guna2GradientPanel2";
            this.guna2GradientPanel2.Size = new System.Drawing.Size(806, 229);
            this.guna2GradientPanel2.TabIndex = 46;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(432, -23);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(316, 293);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 70;
            this.pictureBox2.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Century Gothic", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(42, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(259, 17);
            this.label2.TabIndex = 69;
            this.label2.Text = "Always stay updated in your student portal";
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.BackColor = System.Drawing.Color.Transparent;
            this.lblWelcome.Font = new System.Drawing.Font("Century Gothic", 13.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWelcome.ForeColor = System.Drawing.Color.White;
            this.lblWelcome.Location = new System.Drawing.Point(39, 124);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(259, 26);
            this.lblWelcome.TabIndex = 68;
            this.lblWelcome.Text = "Welcome Back, Name!";
            // 
            // lblDateDisplay
            // 
            this.lblDateDisplay.AutoSize = true;
            this.lblDateDisplay.BackColor = System.Drawing.Color.Transparent;
            this.lblDateDisplay.Font = new System.Drawing.Font("Century Gothic", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateDisplay.ForeColor = System.Drawing.Color.White;
            this.lblDateDisplay.Location = new System.Drawing.Point(42, 50);
            this.lblDateDisplay.Name = "lblDateDisplay";
            this.lblDateDisplay.Size = new System.Drawing.Size(90, 17);
            this.lblDateDisplay.TabIndex = 67;
            this.lblDateDisplay.Text = "mm//dd//yyyy";
            // 
            // guna2Elipse3
            // 
            this.guna2Elipse3.BorderRadius = 10;
            this.guna2Elipse3.TargetControl = this.guna2GradientPanel2;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(284, 38);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(40, 38);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 48;
            this.pictureBox3.TabStop = false;
            // 
            // lblStudentName
            // 
            this.lblStudentName.AutoSize = true;
            this.lblStudentName.BackColor = System.Drawing.Color.Transparent;
            this.lblStudentName.Font = new System.Drawing.Font("Century Gothic", 7.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStudentName.ForeColor = System.Drawing.Color.Firebrick;
            this.lblStudentName.Location = new System.Drawing.Point(326, 48);
            this.lblStudentName.Name = "lblStudentName";
            this.lblStudentName.Size = new System.Drawing.Size(126, 17);
            this.lblStudentName.TabIndex = 70;
            this.lblStudentName.Text = "[Student Full Name]";
            // 
            // lblLogout
            // 
            this.lblLogout.AutoSize = true;
            this.lblLogout.BackColor = System.Drawing.Color.Transparent;
            this.lblLogout.Font = new System.Drawing.Font("Century Gothic", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLogout.ForeColor = System.Drawing.Color.Firebrick;
            this.lblLogout.Location = new System.Drawing.Point(1029, 49);
            this.lblLogout.Name = "lblLogout";
            this.lblLogout.Size = new System.Drawing.Size(52, 16);
            this.lblLogout.TabIndex = 71;
            this.lblLogout.Text = "Log out ";
            this.lblLogout.Click += new System.EventHandler(this.lblLogout_Click);
            // 
            // btnPersonalInformation
            // 
            this.btnPersonalInformation.Animated = true;
            this.btnPersonalInformation.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnPersonalInformation.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnPersonalInformation.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnPersonalInformation.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnPersonalInformation.FillColor = System.Drawing.Color.Firebrick;
            this.btnPersonalInformation.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPersonalInformation.ForeColor = System.Drawing.Color.White;
            this.btnPersonalInformation.Image = ((System.Drawing.Image)(resources.GetObject("btnPersonalInformation.Image")));
            this.btnPersonalInformation.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnPersonalInformation.ImageOffset = new System.Drawing.Point(10, 0);
            this.btnPersonalInformation.Location = new System.Drawing.Point(0, 251);
            this.btnPersonalInformation.Name = "btnPersonalInformation";
            this.btnPersonalInformation.Size = new System.Drawing.Size(232, 58);
            this.btnPersonalInformation.TabIndex = 49;
            this.btnPersonalInformation.Text = "Personal Information";
            this.btnPersonalInformation.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.btnPersonalInformation.TextOffset = new System.Drawing.Point(-20, 0);
            this.btnPersonalInformation.Click += new System.EventHandler(this.btnPersonalInformation_Click);
            // 
            // btnEnrollment
            // 
            this.btnEnrollment.Animated = true;
            this.btnEnrollment.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnEnrollment.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnEnrollment.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnEnrollment.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnEnrollment.FillColor = System.Drawing.Color.Firebrick;
            this.btnEnrollment.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnEnrollment.ForeColor = System.Drawing.Color.White;
            this.btnEnrollment.Image = ((System.Drawing.Image)(resources.GetObject("btnEnrollment.Image")));
            this.btnEnrollment.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnEnrollment.ImageOffset = new System.Drawing.Point(10, 0);
            this.btnEnrollment.Location = new System.Drawing.Point(0, 323);
            this.btnEnrollment.Name = "btnEnrollment";
            this.btnEnrollment.Size = new System.Drawing.Size(232, 58);
            this.btnEnrollment.TabIndex = 50;
            this.btnEnrollment.Text = "Enrollment";
            this.btnEnrollment.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.btnEnrollment.TextOffset = new System.Drawing.Point(-20, 0);
            // 
            // StudentDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1102, 729);
            this.Controls.Add(this.lblLogout);
            this.Controls.Add(this.lblStudentName);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.guna2GradientPanel2);
            this.Controls.Add(this.guna2GradientPanel1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnMinimize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "StudentDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StudentDashboard";
            this.guna2GradientPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.guna2GradientPanel2.ResumeLayout(false);
            this.guna2GradientPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label btnMinimize;
        private System.Windows.Forms.Label btnClose;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private Guna.UI2.WinForms.Guna2GradientPanel guna2GradientPanel2;
        private Guna.UI2.WinForms.Guna2GradientPanel guna2GradientPanel1;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse2;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblDateDisplay;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblStudentName;
        private System.Windows.Forms.PictureBox pictureBox2;
        private Guna.UI2.WinForms.Guna2Button btnHome;
        private System.Windows.Forms.Label lblLogout;
        private Guna.UI2.WinForms.Guna2Button btnPersonalInformation;
        private Guna.UI2.WinForms.Guna2Button btnEnrollment;
    }
}