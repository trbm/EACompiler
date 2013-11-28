

Imports System.IO
Imports System.Text.RegularExpressions

Public Class OutputFile
    Private _oWriteStream As FileStream
    Private _oFileInfo As FileInfo
    Private _oStreamWriter As StreamWriter
    Private _oLines As Collection
    Private _sFullFilename As String
    Private _bOverwriteExisting As Boolean
    Private Shared _oFilesCreated As Collection

    Public Sub New(ByVal sFullFilename As String, Optional ByVal bOverwriteExisting As Boolean = True)
        MyBase.New()

        _sFullFilename = sFullFilename
        _bOverwriteExisting = bOverwriteExisting
        If _oFilesCreated Is Nothing Then
            _oFilesCreated = New Collection
        End If
        initializeFileIO()
    End Sub

    Public Sub Add(Optional ByVal sLineText As String = "", Optional ByVal SuppressEOL As Boolean = False)
        Dim sMassagedLine As String = sLineText

        _oLines.Add(sMassagedLine)
    End Sub

    Public Sub AddLine(Optional ByVal sLineText As String = "", Optional ByVal SuppressEOL As Boolean = False)
        Dim sMassagedLine As String = sLineText & vbCrLf
        _oLines.Add(sMassagedLine)
    End Sub

    Public Sub Close()
        Dim sLine As String

        On Error Resume Next   ' just ignore any errors as the file closes
        If _oStreamWriter IsNot Nothing Then
            For Each sLine In _oLines
                _oStreamWriter.WriteLine(sLine)
            Next
            _oStreamWriter.Close()
            _oStreamWriter = Nothing
        End If

        If _oWriteStream IsNot Nothing Then
            _oWriteStream.Close()
            _oWriteStream = Nothing
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim sString As String = ""
        Dim sLine As String

        For Each sLine In _oLines
            sString &= sLine
        Next

        Return sString
    End Function

    Private Sub initializeFileIO()
        Try
            OutputFile.ClearFilesCreated()

            _oLines = New Collection
            _oFileInfo = New FileInfo(_sFullFilename)
            With _oFileInfo
                If .Exists Then
                    If Not _bOverwriteExisting Then
                        Throw New ApplicationException("File '" & _sFullFilename & "' already exists and may not be overwritten")
                    End If
                    _oFileInfo.Attributes = (_oFileInfo.Attributes Or System.IO.FileAttributes.ReadOnly) Xor System.IO.FileAttributes.ReadOnly      ' make file writable
                    _oFileInfo.Delete()
                End If
                _oWriteStream = _oFileInfo.Create
                _oStreamWriter = New StreamWriter(_oWriteStream)
                If Not _oFilesCreated.Contains(_oFileInfo.Name) Then
                    _oFilesCreated.Add(_oFileInfo.Name, _oFileInfo.FullName)
                End If
            End With
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        Me.Close()
    End Sub

    Public Function GetFiles(ByVal sfilenameSpec As String) As Collection
        Dim sFilenames As New Collection

        Dim oDirectoryFiles() As FileInfo = _oFileInfo.Directory.GetFiles(sfilenameSpec, IO.SearchOption.TopDirectoryOnly)

        For Each oFileInfo As FileInfo In oDirectoryFiles
            sFilenames.Add(oFileInfo.Name, oFileInfo.FullName)
        Next
        Return sFilenames
    End Function

    Public Shared Sub ClearFilesCreated()
        _oFilesCreated = New Collection
    End Sub

    Public ReadOnly Property FileInfo() As FileInfo
        Get
            Return _oFileInfo
        End Get
    End Property

    Public Shared ReadOnly Property FilesCreated() As Collection
        Get
            Return _oFilesCreated
        End Get
    End Property
End Class

