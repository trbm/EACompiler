
'____________________________________________________________________
'
' © Copyright 2008, PYCCA, Incorporated  All rights reserved.
'____________________________________________________________________
'
'     $RCSfile: EAInterface.vb,v $
'
'    $Revision: 1.1 $
'
'        $Date: 2008/09/24 21:09:08 $
'
'      $Author: thomasb $
'
'      $Source: EAInterface.vb,v $
'
'        $Name:  $
'____________________________________________________________________


Public Class EAInterface
    'Private _oRepository As EA.Repository
    ''Private _oTabControl As TabControl
    'Private _oOutputLanguage As IOutputLanguage

    ''Public Sub New(ByRef oTabControl As TabControl, _
    ''               ByVal oOutputLanguage As IOutputLanguage, _
    ''               ByRef lblModelFilename As Label, _
    ''               ByRef lblOutputFilename As Label, _
    ''               ByRef ofrmMain As frmMain, _
    ''               Optional ByVal oRepository As EA.Repository = Nothing)
    'Public Sub New(ByVal oOutputLanguage As IOutputLanguage, _
    '                ByVal oTextbox As RichTextBox, _
    '                ByVal oRepository As EA.Repository)

    '    MyBase.New()

    '    Try
    '        '_oTabControl = oTabControl
    '        '_oOutputLanguage = oOutputLanguage
    '        '_oRepository = oRepository

    '        'If _oRepository Is Nothing Then
    '        '    Dim oApplication As New EA.App
    '        '    If oApplication.Project.LoadProject(lblModelFilename.Text) Then
    '        '        _oRepository = oApplication.Repository
    '        '    Else
    '        '        Throw New ApplicationException("Failed to open project: " & lblModelFilename.Text)
    '        '    End If
    '        'Else
    '        'End If
    '        _oOutputLanguage.CreateDomains(_oRepository, oTextbox)
    '    Catch ex As Exception
    '        Dim oErrorHandler As New sjmErrorHandler(ex)
    '    End Try
    'End Sub

    'Protected Overrides Sub Finalize()
    '    MyBase.Finalize()
    'End Sub
End Class
