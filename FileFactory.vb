Imports System.IO


' ________________________________________________________________________________________________________________
'
' Filename: FileFactory.vb
'
' $Revision: 1.1 $
' $Date: 2008/09/24 21:09:08 $
' $Author: thomasb $
' $History:   $
' ________________________________________________________________________________________________________________

Public Class FileFactory

    Private _oFileInfo As FileInfo
    Private _oReadStream As FileStream
    Private _oStreamReader As StreamReader
    Private _oWriteStream As FileStream
    Private _oStreamWriter As StreamWriter
    Private _colSubsection As Collection
    Private _colLines As New Collection
    Private _sFilename As String
    Private _sBackupFilename As String
    Private _sIndent As String
    Private _sCommentLeader As String
    Private _bSubsectionRecording As Boolean

    Const COMMENT_START_COLUMN = 75
    Const TAB_STOP_INTERVAL = 25
    Const INDENTATION As String = "    "

    Private _oErrorHandler As New sjmErrorHandler

    Public ReadOnly Property FileExists() As Boolean
        Get
            return File.Exists(_sFilename)  
        End Get
    End Property

    Public ReadOnly Property SubsectionRecording() As Boolean
        Get
            Return _bSubsectionRecording
        End Get
    End Property

    Public Sub New(ByVal sFilename As String, ByVal bCreateBackupFile As Boolean)
        Try
            _sFilename = sFilename

            _oErrorHandler.SupplementalInformation = "Failed to create FileInfo for file: " & sFilename
            _oFileInfo = New FileInfo(sFilename)
            If File.Exists(sFilename) Then
                If bCreateBackupFile Then
                    createBackupFile(sFilename)
                End If
                _oFileInfo.Attributes = (_oFileInfo.Attributes Or System.IO.FileAttributes.ReadOnly) Xor System.IO.FileAttributes.ReadOnly      ' make file writable
            End If
        Catch ex As Exception
            _oErrorHandler.Announce(ex)
        End Try
        _oErrorHandler.SupplementalInformation = ""
    End Sub

    Public Sub BeginSubsectionRecording()
        _bSubsectionRecording = True
        _colSubsection = New Collection
    End Sub

    Public Function EndSubsectionRecording() As Collection
        _bSubsectionRecording = False
        Return _colSubsection
    End Function

    Public Sub Close(Optional ByVal bMakeReadonly As Boolean = False)
        Dim sLine As String

        Try
            If Not _colLines Is Nothing Then
                If (_colLines.Count > 0) Then
                    _oErrorHandler.SupplementalInformation = "Failed to create WriteStream for file: " & _sFilename
                    _oWriteStream = _oFileInfo.Create

                    _oErrorHandler.SupplementalInformation = "Failed to create StreamWriter for file: " & _sFilename
                    _oStreamWriter = New StreamWriter(_oWriteStream)

                    For Each sLine In _colLines
                        _oStreamWriter.WriteLine(sLine)
                    Next

                    _oErrorHandler.SupplementalInformation = "Failed to close file: " & _sFilename
                    _oStreamWriter.Close()
                    _oWriteStream.Close()

                    If bMakeReadonly Then
                        _oFileInfo.Attributes = _oFileInfo.Attributes Or System.IO.FileAttributes.ReadOnly
                    End If
                    _oFileInfo = Nothing
                End If
            End If
            _colLines = Nothing
        Catch ex As Exception
            _oErrorHandler.Announce(ex)
        End Try
    End Sub

    Public Sub Indent(Optional ByVal iCount As Integer = 1)
        Dim iIndex As Integer

        If iCount < 1 Then
            iCount = 1
        End If

        For iIndex = 1 To iCount
            _sIndent += INDENTATION
        Next
    End Sub

    Public Sub NoIndent()
        _sIndent = ""
    End Sub

    Public Sub Outdent(Optional ByVal iCount As Integer = 1)
        Dim iIndex As Integer

        If iCount < 1 Then
            iCount = 1
        End If

        For iIndex = 1 To iCount
            If _sIndent.Length >= INDENTATION.Length Then
                _sIndent = _sIndent.Substring(0, _sIndent.Length - INDENTATION.Length)
            End If
        Next
    End Sub

    Public Sub Add(Optional ByVal sLine As String = "", Optional ByVal sCommentString As String = "")
        sLine = _sIndent & sLine
        If sCommentString.Length > 0 Then
            sLine = sLine.PadRight(COMMENT_START_COLUMN) & _sCommentLeader & sCommentString
        End If
        _colLines.Add(sLine)
        If _bSubsectionRecording Then
            _colSubsection.add(sLine)
        End If
    End Sub

    Public Shadows ReadOnly Property ToString() As String
        Get
            Dim sToString As String = ""

            If Not _oStreamWriter Is Nothing Then
                _oStreamWriter.Close()              ' close the file before we try to open it for reading
            End If

            _oErrorHandler.SupplementalInformation = "Failed to create FileInfo for file: " & _sFilename
            _oFileInfo = New FileInfo(_sFilename)
            If File.Exists(_sFilename) Then
                _oErrorHandler.SupplementalInformation = "Failed to create ReadStream for file: " & _sFilename
                _oReadStream = _oFileInfo.OpenRead

                _oErrorHandler.SupplementalInformation = "Failed to create StreamReader for file: " & _sFilename
                _oStreamReader = New StreamReader(_oReadStream)

                sToString = _oStreamReader.ReadToEnd
            End If

            Return sToString
            _oErrorHandler.SupplementalInformation = ""
        End Get
    End Property

    Private Sub createBackupFile(ByVal sFilename As String)
        Dim oCreationTime As Date
        Dim sBackupFilename As String = sFilename & "_backup_" & Now.ToFileTime
        Try
            oCreationTime = File.GetCreationTime(sFilename)
            _oErrorHandler.SupplementalInformation = "Failed to create backup for file: " & sFilename & "  (" & sBackupFilename & ")"
            File.Copy(sFilename, sBackupFilename)
            File.SetCreationTime(sBackupFilename, oCreationTime)
        Catch ex As Exception
            _oErrorHandler.Announce(ex)
        End Try
        _oErrorHandler.SupplementalInformation = ""
    End Sub

    Public ReadOnly Property Filename() As String
        Get
            Return _sFilename
        End Get
    End Property

    Public Property CommentLeader() As String
        Get
            Return _sCommentLeader
        End Get
        Set(ByVal value As String)
            _sCommentLeader = value
        End Set
    End Property

    Protected Overrides Sub Finalize()
        Close()
        MyBase.Finalize()
    End Sub
End Class
