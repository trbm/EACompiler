
'____________________________________________________________________
'
' © Copyright 2008, brennan|marquez  All rights reserved.
'____________________________________________________________________
'
'     $RCSfile: InputFile.vb,v $
'
'    $Revision: 1.1 $
'
'        $Date: 2008/09/24 21:09:15 $
'
'      $Author: thomasb $
'
'        $Name:  $
'____________________________________________________________________

Imports System.IO
Imports System.Text.RegularExpressions

Public Class InputFile
    Private _oFileInfo As FileInfo
    Private _oReadStream As FileStream
    Private _oStreamReader As StreamReader
    Private _sFullFilename As String
    Private _sToString As String

    Public Sub New(ByVal sFullFilename As String, Optional ByVal bOverwriteExisting As Boolean = True)
        MyBase.New()

        _sFullFilename = sFullFilename
        _oFileInfo = New FileInfo(_sFullFilename)
        If File.Exists(_sFullFilename) Then
            _oReadStream = _oFileInfo.OpenRead
            _oStreamReader = New StreamReader(_oReadStream)
            _sToString = _oStreamReader.ReadToEnd
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim sReturn As String = ""
        If _sToString IsNot Nothing Then
            sReturn = _sToString.ToString()
        End If
        Return sReturn
    End Function

    Public Sub Close()
        If _oStreamReader IsNot Nothing Then
            _oStreamReader.Close()
        End If
        _oStreamReader = Nothing
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        Me.Close()
    End Sub

    Public ReadOnly Property FileInfo() As FileInfo
        Get
            Return _oFileInfo
        End Get
    End Property
End Class

