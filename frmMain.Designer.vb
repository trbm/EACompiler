<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.cmdTranslate = New System.Windows.Forms.Button
        Me.lblModelFilename = New System.Windows.Forms.Label
        Me.cmdModelFile = New System.Windows.Forms.Button
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog
        Me.TabControl1 = New System.Windows.Forms.TabControl
        Me.TabPage1 = New System.Windows.Forms.TabPage
        Me.RichTextBox1 = New System.Windows.Forms.RichTextBox
        Me.TabPage2 = New System.Windows.Forms.TabPage
        Me.RichTextBox2 = New System.Windows.Forms.RichTextBox
        Me.TabPage3 = New System.Windows.Forms.TabPage
        Me.RichTextBox3 = New System.Windows.Forms.RichTextBox
        Me.TabPage4 = New System.Windows.Forms.TabPage
        Me.RichTextBox4 = New System.Windows.Forms.RichTextBox
        Me.TabPage5 = New System.Windows.Forms.TabPage
        Me.RichTextBox5 = New System.Windows.Forms.RichTextBox
        Me.TabPage6 = New System.Windows.Forms.TabPage
        Me.RichTextBox6 = New System.Windows.Forms.RichTextBox
        Me.TabPage7 = New System.Windows.Forms.TabPage
        Me.RichTextBox7 = New System.Windows.Forms.RichTextBox
        Me.TabPage8 = New System.Windows.Forms.TabPage
        Me.RichTextBox8 = New System.Windows.Forms.RichTextBox
        Me.lblOutputFilename = New System.Windows.Forms.Label
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.radVB = New System.Windows.Forms.RadioButton
        Me.radPerl = New System.Windows.Forms.RadioButton
        Me.radPYCCA = New System.Windows.Forms.RadioButton
        Me.chkAutoClose = New System.Windows.Forms.CheckBox
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.chkAutoRun = New System.Windows.Forms.CheckBox
        Me.txtAutoRunSeconds = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.cmdDone = New System.Windows.Forms.Button
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.TabPage4.SuspendLayout()
        Me.TabPage5.SuspendLayout()
        Me.TabPage6.SuspendLayout()
        Me.TabPage7.SuspendLayout()
        Me.TabPage8.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdTranslate
        '
        Me.cmdTranslate.Enabled = False
        Me.cmdTranslate.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdTranslate.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdTranslate.Location = New System.Drawing.Point(14, 44)
        Me.cmdTranslate.Name = "cmdTranslate"
        Me.cmdTranslate.Size = New System.Drawing.Size(159, 25)
        Me.cmdTranslate.TabIndex = 3
        Me.cmdTranslate.Text = "Translate"
        Me.cmdTranslate.UseVisualStyleBackColor = False
        '
        'lblModelFilename
        '
        Me.lblModelFilename.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblModelFilename.AutoEllipsis = True
        Me.lblModelFilename.BackColor = System.Drawing.Color.Transparent
        Me.lblModelFilename.Font = New System.Drawing.Font("Eras Medium ITC", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblModelFilename.Location = New System.Drawing.Point(195, 15)
        Me.lblModelFilename.Name = "lblModelFilename"
        Me.lblModelFilename.Size = New System.Drawing.Size(657, 21)
        Me.lblModelFilename.TabIndex = 6
        Me.lblModelFilename.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'cmdModelFile
        '
        Me.cmdModelFile.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdModelFile.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdModelFile.Location = New System.Drawing.Point(14, 13)
        Me.cmdModelFile.Name = "cmdModelFile"
        Me.cmdModelFile.Size = New System.Drawing.Size(159, 25)
        Me.cmdModelFile.TabIndex = 5
        Me.cmdModelFile.Text = "Model..."
        Me.cmdModelFile.UseVisualStyleBackColor = False
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Controls.Add(Me.TabPage4)
        Me.TabControl1.Controls.Add(Me.TabPage5)
        Me.TabControl1.Controls.Add(Me.TabPage6)
        Me.TabControl1.Controls.Add(Me.TabPage7)
        Me.TabControl1.Controls.Add(Me.TabPage8)
        Me.TabControl1.Font = New System.Drawing.Font("Lucida Console", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TabControl1.Location = New System.Drawing.Point(14, 76)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(956, 225)
        Me.TabControl1.TabIndex = 7
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.RichTextBox1)
        Me.TabPage1.Location = New System.Drawing.Point(4, 24)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(948, 197)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'RichTextBox1
        '
        Me.RichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox1.Font = New System.Drawing.Font("Lucida Console", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RichTextBox1.Location = New System.Drawing.Point(3, 3)
        Me.RichTextBox1.Name = "RichTextBox1"
        Me.RichTextBox1.Size = New System.Drawing.Size(942, 191)
        Me.RichTextBox1.TabIndex = 0
        Me.RichTextBox1.Text = ""
        Me.RichTextBox1.WordWrap = False
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.RichTextBox2)
        Me.TabPage2.Location = New System.Drawing.Point(4, 24)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(948, 197)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'RichTextBox2
        '
        Me.RichTextBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox2.Location = New System.Drawing.Point(3, 3)
        Me.RichTextBox2.Name = "RichTextBox2"
        Me.RichTextBox2.Size = New System.Drawing.Size(942, 191)
        Me.RichTextBox2.TabIndex = 1
        Me.RichTextBox2.Text = ""
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.RichTextBox3)
        Me.TabPage3.Location = New System.Drawing.Point(4, 24)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Size = New System.Drawing.Size(948, 197)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'RichTextBox3
        '
        Me.RichTextBox3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox3.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox3.Name = "RichTextBox3"
        Me.RichTextBox3.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox3.TabIndex = 1
        Me.RichTextBox3.Text = ""
        '
        'TabPage4
        '
        Me.TabPage4.Controls.Add(Me.RichTextBox4)
        Me.TabPage4.Location = New System.Drawing.Point(4, 24)
        Me.TabPage4.Name = "TabPage4"
        Me.TabPage4.Size = New System.Drawing.Size(948, 197)
        Me.TabPage4.TabIndex = 3
        Me.TabPage4.UseVisualStyleBackColor = True
        '
        'RichTextBox4
        '
        Me.RichTextBox4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox4.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox4.Name = "RichTextBox4"
        Me.RichTextBox4.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox4.TabIndex = 1
        Me.RichTextBox4.Text = ""
        '
        'TabPage5
        '
        Me.TabPage5.Controls.Add(Me.RichTextBox5)
        Me.TabPage5.Location = New System.Drawing.Point(4, 24)
        Me.TabPage5.Name = "TabPage5"
        Me.TabPage5.Size = New System.Drawing.Size(948, 197)
        Me.TabPage5.TabIndex = 4
        Me.TabPage5.UseVisualStyleBackColor = True
        '
        'RichTextBox5
        '
        Me.RichTextBox5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox5.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox5.Name = "RichTextBox5"
        Me.RichTextBox5.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox5.TabIndex = 1
        Me.RichTextBox5.Text = ""
        '
        'TabPage6
        '
        Me.TabPage6.Controls.Add(Me.RichTextBox6)
        Me.TabPage6.Location = New System.Drawing.Point(4, 24)
        Me.TabPage6.Name = "TabPage6"
        Me.TabPage6.Size = New System.Drawing.Size(948, 197)
        Me.TabPage6.TabIndex = 5
        Me.TabPage6.UseVisualStyleBackColor = True
        '
        'RichTextBox6
        '
        Me.RichTextBox6.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox6.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox6.Name = "RichTextBox6"
        Me.RichTextBox6.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox6.TabIndex = 1
        Me.RichTextBox6.Text = ""
        '
        'TabPage7
        '
        Me.TabPage7.Controls.Add(Me.RichTextBox7)
        Me.TabPage7.Location = New System.Drawing.Point(4, 24)
        Me.TabPage7.Name = "TabPage7"
        Me.TabPage7.Size = New System.Drawing.Size(948, 197)
        Me.TabPage7.TabIndex = 6
        Me.TabPage7.UseVisualStyleBackColor = True
        '
        'RichTextBox7
        '
        Me.RichTextBox7.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox7.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox7.Name = "RichTextBox7"
        Me.RichTextBox7.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox7.TabIndex = 1
        Me.RichTextBox7.Text = ""
        '
        'TabPage8
        '
        Me.TabPage8.Controls.Add(Me.RichTextBox8)
        Me.TabPage8.Location = New System.Drawing.Point(4, 24)
        Me.TabPage8.Name = "TabPage8"
        Me.TabPage8.Size = New System.Drawing.Size(948, 197)
        Me.TabPage8.TabIndex = 7
        Me.TabPage8.UseVisualStyleBackColor = True
        '
        'RichTextBox8
        '
        Me.RichTextBox8.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RichTextBox8.Location = New System.Drawing.Point(0, 0)
        Me.RichTextBox8.Name = "RichTextBox8"
        Me.RichTextBox8.Size = New System.Drawing.Size(948, 197)
        Me.RichTextBox8.TabIndex = 1
        Me.RichTextBox8.Text = ""
        '
        'lblOutputFilename
        '
        Me.lblOutputFilename.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblOutputFilename.AutoEllipsis = True
        Me.lblOutputFilename.BackColor = System.Drawing.Color.Transparent
        Me.lblOutputFilename.Font = New System.Drawing.Font("Eras Medium ITC", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOutputFilename.Location = New System.Drawing.Point(195, 47)
        Me.lblOutputFilename.Name = "lblOutputFilename"
        Me.lblOutputFilename.Size = New System.Drawing.Size(657, 21)
        Me.lblOutputFilename.TabIndex = 8
        Me.lblOutputFilename.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Panel1.Controls.Add(Me.radVB)
        Me.Panel1.Controls.Add(Me.radPerl)
        Me.Panel1.Controls.Add(Me.radPYCCA)
        Me.Panel1.Location = New System.Drawing.Point(854, -1)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(118, 71)
        Me.Panel1.TabIndex = 9
        '
        'radVB
        '
        Me.radVB.AutoSize = True
        Me.radVB.Checked = True
        Me.radVB.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radVB.Location = New System.Drawing.Point(14, 47)
        Me.radVB.Name = "radVB"
        Me.radVB.Size = New System.Drawing.Size(42, 19)
        Me.radVB.TabIndex = 2
        Me.radVB.TabStop = True
        Me.radVB.Text = "VB"
        Me.radVB.UseVisualStyleBackColor = True
        '
        'radPerl
        '
        Me.radPerl.AutoSize = True
        Me.radPerl.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radPerl.Location = New System.Drawing.Point(14, 25)
        Me.radPerl.Name = "radPerl"
        Me.radPerl.Size = New System.Drawing.Size(47, 19)
        Me.radPerl.TabIndex = 1
        Me.radPerl.Text = "Perl"
        Me.radPerl.UseVisualStyleBackColor = True
        '
        'radPYCCA
        '
        Me.radPYCCA.AutoSize = True
        Me.radPYCCA.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.radPYCCA.Location = New System.Drawing.Point(14, 3)
        Me.radPYCCA.Name = "radPYCCA"
        Me.radPYCCA.Size = New System.Drawing.Size(64, 19)
        Me.radPYCCA.TabIndex = 0
        Me.radPYCCA.Text = "PYCCA"
        Me.radPYCCA.UseVisualStyleBackColor = True
        '
        'chkAutoClose
        '
        Me.chkAutoClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkAutoClose.AutoSize = True
        Me.chkAutoClose.Enabled = False
        Me.chkAutoClose.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAutoClose.Location = New System.Drawing.Point(19, 313)
        Me.chkAutoClose.Name = "chkAutoClose"
        Me.chkAutoClose.Size = New System.Drawing.Size(89, 19)
        Me.chkAutoClose.TabIndex = 10
        Me.chkAutoClose.Text = "Auto Close"
        Me.chkAutoClose.UseVisualStyleBackColor = True
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 3000
        '
        'chkAutoRun
        '
        Me.chkAutoRun.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkAutoRun.AutoSize = True
        Me.chkAutoRun.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAutoRun.Location = New System.Drawing.Point(129, 313)
        Me.chkAutoRun.Name = "chkAutoRun"
        Me.chkAutoRun.Size = New System.Drawing.Size(81, 19)
        Me.chkAutoRun.TabIndex = 11
        Me.chkAutoRun.Text = "Auto Run"
        Me.chkAutoRun.UseVisualStyleBackColor = True
        '
        'txtAutoRunSeconds
        '
        Me.txtAutoRunSeconds.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtAutoRunSeconds.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtAutoRunSeconds.Location = New System.Drawing.Point(209, 309)
        Me.txtAutoRunSeconds.Name = "txtAutoRunSeconds"
        Me.txtAutoRunSeconds.Size = New System.Drawing.Size(41, 22)
        Me.txtAutoRunSeconds.TabIndex = 12
        Me.txtAutoRunSeconds.Text = "10"
        Me.txtAutoRunSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(255, 314)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 15)
        Me.Label1.TabIndex = 13
        Me.Label1.Text = "seconds"
        '
        'cmdDone
        '
        Me.cmdDone.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdDone.Font = New System.Drawing.Font("Eras Medium ITC", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdDone.Location = New System.Drawing.Point(811, 306)
        Me.cmdDone.Name = "cmdDone"
        Me.cmdDone.Size = New System.Drawing.Size(159, 25)
        Me.cmdDone.TabIndex = 14
        Me.cmdDone.Text = "Done"
        Me.cmdDone.UseVisualStyleBackColor = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 14.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(984, 335)
        Me.Controls.Add(Me.cmdDone)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtAutoRunSeconds)
        Me.Controls.Add(Me.chkAutoRun)
        Me.Controls.Add(Me.chkAutoClose)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.lblOutputFilename)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.lblModelFilename)
        Me.Controls.Add(Me.cmdModelFile)
        Me.Controls.Add(Me.cmdTranslate)
        Me.Font = New System.Drawing.Font("Eras Medium ITC", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmMain"
        Me.Text = "EA Model Compiler"
        Me.TransparencyKey = System.Drawing.Color.Red
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage4.ResumeLayout(False)
        Me.TabPage5.ResumeLayout(False)
        Me.TabPage6.ResumeLayout(False)
        Me.TabPage7.ResumeLayout(False)
        Me.TabPage8.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmdTranslate As System.Windows.Forms.Button
    Friend WithEvents lblModelFilename As System.Windows.Forms.Label
    Friend WithEvents cmdModelFile As System.Windows.Forms.Button
    Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage6 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage7 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage8 As System.Windows.Forms.TabPage
    Friend WithEvents RichTextBox1 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox2 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox3 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox4 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox5 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox6 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox7 As System.Windows.Forms.RichTextBox
    Friend WithEvents RichTextBox8 As System.Windows.Forms.RichTextBox
    Friend WithEvents lblOutputFilename As System.Windows.Forms.Label
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents radPerl As System.Windows.Forms.RadioButton
    Friend WithEvents radPYCCA As System.Windows.Forms.RadioButton
    Friend WithEvents radVB As System.Windows.Forms.RadioButton
    Friend WithEvents chkAutoClose As System.Windows.Forms.CheckBox
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents chkAutoRun As System.Windows.Forms.CheckBox
    Friend WithEvents txtAutoRunSeconds As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdDone As System.Windows.Forms.Button

End Class
