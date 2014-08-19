
' one
' oneB

' really three
' really three A


'____________________________________________________________________
'
' � Copyright 2008, brennan/marquez  All rights reserved.
'____________________________________________________________________
'
'     $RCSfile: frmMain.vb,v $
'
'    $Revision: 1.2 $
'
'        $Date: 2008/09/26 20:33:27 $
'
'      $Author: thomasb $
'
'      $Source: frmMain.vb,v $
'
'        $Name:  $
'____________________________________________________________________


Imports System.io
Imports EACompilerII

Public Class frmMain
    Const PATH_KEY = "EATool_PATH_KEY"
    Const FILENAME_KEY = "EATool_FILENAME_KEY"
    Const TARGET_LANGUAGE_KEY = "EATool_TARGET_LANGUAGE_KEY"
    Const AUTO_RUN_KEY = "EATool_AUTO_RUN_KEY"
    Const AUTO_CLOSE_KEY = "EATool_AUTO_CLOSE_KEY"
    Const AUTO_RUN_TEXT = "AutoRun"
    Const AUTO_CLOSE_TEXT = "AutoClose"
    Const PERL_TEXT = "Perl"
    Const VB_TEXT = "VB"
    Const PYCCA_TEXT = "PYCCA"
    Const AUTO_RUN_TIME = "AUTO_RUN_TIME"

    Private _oEAInterface As EAInterface
    Private _bInitializationComplete As Boolean = False

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        establishStandardText()
        setTargetLanguage()
        setAutoCheckboxes()

        Me.Text = Me.Text & "  (v" & VERSION & ")"
        lblModelFilename.Text = GetRegistryKey(FILENAME_KEY, "")

        If File.Exists(lblModelFilename.Text) Then
            cmdTranslate.Enabled = True
        End If

        If IsNumeric(txtAutoRunSeconds.Text) Then
            Timer1.Interval = 1000 * txtAutoRunSeconds.Text
        End If

        _bInitializationComplete = True
    End Sub

    Private Sub setAutoCheckboxes()
        Dim sAutoRunSetting As String = GetRegistryKey(AUTO_RUN_KEY, "True")
        Dim sAutoCloseSetting As String = GetRegistryKey(AUTO_CLOSE_KEY, "True")
        Dim sAutoRunTime As String = GetRegistryKey(AUTO_RUN_TIME, "3")

        chkAutoRun.Checked = (sAutoRunSetting = "True")
        chkAutoClose.Checked = (sAutoCloseSetting = "True")
        txtAutoRunSeconds.Text = sAutoRunTime
    End Sub

    Private Sub setTargetLanguage()
        Dim sTargetLanguageName As String = GetRegistryKey(TARGET_LANGUAGE_KEY, VB_TEXT)

        Select Case sTargetLanguageName
            Case PERL_TEXT
                radPerl.Checked = True

            Case VB_TEXT
                radVB.Checked = True

            Case PYCCA_TEXT
                radPYCCA.Checked = True

            Case Else
                radVB.Checked = True
        End Select
    End Sub

    Private Sub establishStandardText()
        radPerl.Text = PERL_TEXT
        radVB.Text = VB_TEXT
        radPYCCA.Text = PYCCA_TEXT
        chkAutoClose.Text = AUTO_CLOSE_TEXT
        chkAutoRun.Text = AUTO_RUN_TEXT
    End Sub

    Private Sub clearTextboxesRecurse(ByVal oParentControl As Control)
        For Each oControl As Control In oParentControl.Controls
            If oControl.GetType.ToString.IndexOf("RichTextBox") > 0 Then
                CType(oControl, RichTextBox).Clear()
            End If
            clearTextboxesRecurse(oControl)
        Next
    End Sub

    Public Sub Translate(Optional ByVal oOutputLanguage As IOutputLanguage = Nothing, Optional ByVal oRepository As EA.Repository = Nothing)
        If oOutputLanguage Is Nothing Then
            If radPYCCA.Checked Then
                oOutputLanguage = New OutputLanguagePYCCA
                lblOutputFilename.Text = "building for PYCCA target..."
            End If

            If radPerl.Checked Then
                oOutputLanguage = New OutputLanguagePERL
                lblOutputFilename.Text = "building for Perl target..."
            End If

            If radVB.Checked Then
                oOutputLanguage = New OutputLanguageVB
                lblOutputFilename.Text = "building for VB target..."
            End If
        End If

        Try
            gStatusBox = New frmStatusBox
            clearTextboxesRecurse(Me)
            Application.DoEvents()
            oOutputLanguage.CreateDomains(oRepository)
            '_oEAInterface = New EAInterface(TabControl1, oOutputLanguage, lblModelFilename, lblOutputFilename, Me, oRepository)
            'lblOutputFilename.Text = oOutputLanguage.CreateOutputFile(lblModelFilename.Text, _
            '                                                          RichTextBox8, _
            '                                                          Me.Controls)
            gStatusBox.FadeAway()
            PlaySound("TortoiseSVN_Notification.wav", 0, SND_FILENAME)
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Sub cmdTranslate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdTranslate.Click
        Translate()
    End Sub

    Private Sub cmdModelFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdModelFile.Click
        chkAutoRun.Checked = False
        chkAutoClose.Checked = False
        With OpenFileDialog1
            .Title = "Select a model file"
            .Filter = "Enterprise Architect model files (*.eap)|*.eap|All files (*.*)|*.*"
            .InitialDirectory = GetRegistryKey(PATH_KEY, "c:\")
            .FileName = GetRegistryKey(FILENAME_KEY, "")
            .ShowDialog()
            If .FileName.Length > 0 Then
                SetRegistryKey(PATH_KEY, .FileName)
                SetRegistryKey(FILENAME_KEY, .FileName)
                lblModelFilename.Text = .FileName
                cmdTranslate.Enabled = True
            End If
        End With
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        If chkAutoRun.Checked Then
            cmdTranslate_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub radVB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles radVB.CheckedChanged
        Dim sTargetName As String
        Dim oRadioButton As New RadioButton

        If _bInitializationComplete Then
            For Each oControl As Control In Panel1.Controls
                If oControl.GetType.ToString = oRadioButton.GetType.ToString Then       ' found a radio button
                    oRadioButton = oControl
                    If oRadioButton.Checked Then
                        sTargetName = oRadioButton.Text
                        SetRegistryKey(TARGET_LANGUAGE_KEY, sTargetName)
                        Exit For
                    End If
                End If
            Next
        End If
    End Sub

    Private Sub chkAutoRun_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoRun.CheckedChanged
        If _bInitializationComplete Then
            SetRegistryKey(AUTO_RUN_KEY, chkAutoRun.Checked)
        End If
    End Sub

    Private Sub chkAutoClose_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoClose.CheckedChanged
        If _bInitializationComplete Then
            SetRegistryKey(AUTO_CLOSE_KEY, chkAutoClose.Checked)
        End If
    End Sub

    Private Sub txtAutoRunSeconds_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAutoRunSeconds.TextChanged
        If _bInitializationComplete Then
            SetRegistryKey(AUTO_RUN_TIME, txtAutoRunSeconds.Text)
        End If
    End Sub

    Private Sub cmdDone_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDone.Click
        Me.Hide()
    End Sub
End Class
