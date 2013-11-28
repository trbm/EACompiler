Public Class frmStatusBox

    Private Const DECREMENT_VALUE As Double = 0.008


    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ProgressBar1.Visible = True
        '   Me.BackColor = Color.DarkSeaGreen
        lblClass.Text = ""
        Me.Show()
    End Sub

    Public Property Filename() As String
        Get
            Return lblFilename.Text
        End Get
        Set(ByVal value As String)
            lblFilename.Text = value
            lblFilename.Visible = True
        End Set
    End Property

    Public Property VersionStamp() As String
        Get
            Return lblVersion.Text
        End Get
        Set(ByVal value As String)
            lblVersion.Text = value
        End Set
    End Property

    Public Property ProgressValueMaximum() As Integer
        Get
            Return ProgressBar1.Maximum
        End Get
        Set(ByVal value As Integer)
            If value > ProgressBar1.Value Then
                ProgressBar1.Maximum = value
            End If
        End Set
    End Property

    Public Property ProgressValue() As Integer
        Get
            Return ProgressBar1.Value
        End Get
        Set(ByVal value As Integer)
            If value > ProgressBar1.Maximum Then
                ProgressBar1.Maximum = value
            End If
            ProgressBar1.Value = value
        End Set
    End Property

    Public Sub ShowClassName(ByVal sClassName As String)
        lblClass.Text = sClassName
        Application.DoEvents()
    End Sub

    Public Sub FadeAway()
        If RichTextBox1.TextLength = 0 Then
            lblClass.Text = ""
            Me.BackColor = Color.LimeGreen
            lblFilename.Text = "Compilation Complete"
            ProgressBar1.Visible = False
            tmrDwell.Enabled = True
            tmrFade.Enabled = False
            Application.DoEvents()
            PlaySound("TortoiseSVN_Notification.wav", 0, SND_FILENAME)
        End If
    End Sub

    Private Sub tmrDwell_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrDwell.Tick
        tmrDwell.Enabled = False
        tmrFade.Enabled = True
    End Sub

    Private Sub tmrFade_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrFade.Tick
        Me.Opacity -= DECREMENT_VALUE
        If Me.Opacity <= 0 Then
            tmrFade.Enabled = False
            processCompleteCleanup()
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Static oNow As DateTime = Now
        lblTime.Text = (Now - oNow).Seconds.ToString
    End Sub

    Public Sub ReportError(ByVal sError As String)
        If sError.Length > 0 Then
            RichTextBox1.Clear()
            RichTextBox1.AppendText(sError)
            Me.Height = 366
            Me.BackColor = Color.Gainsboro
            cmdOK.Visible = True
            Me.TopMost = True
            lblClass.Text = "PYCCA Error Encountered"
            ProgressBar1.Visible = False
            lblTime.Visible = False
            lblVersion.Visible = False
        End If
    End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        processCompleteCleanup()
    End Sub

    Private Sub processCompleteCleanup()
        Me.Close()
    End Sub
End Class