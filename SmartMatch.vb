Imports System.Text.RegularExpressions

Public Class SmartMatch
    Private _oMatches As MatchCollection
    Private _sInput As String
    Private _sPattern As String
    Private _sSmartMatchName As String

    Public Sub New(ByVal sInput As String, ByVal sPattern As String, Optional ByVal sSmartMatchName As String = "")
        On Error Resume Next
        _oMatches = Regex.Matches(sInput, sPattern)
        _sInput = sInput
        _sPattern = sPattern
        _sSmartMatchName = sSmartMatchName
        If _sSmartMatchName.Length > 0 Then
            Show()
        End If
    End Sub

    Public ReadOnly Property MatchGroupCount(ByVal iMatchIndex As Integer) As Integer
        Get
            Dim iMatchGroupCount As Integer = 0
            On Error Resume Next
            iMatchGroupCount = _oMatches(iMatchIndex).Groups.Count
            Return iMatchGroupCount
        End Get
    End Property

    Public ReadOnly Property MatchGroups(ByVal iMatchIndex As Integer) As GroupCollection
        Get
            On Error Resume Next
            Return _oMatches(iMatchIndex).Groups
        End Get
    End Property

    Public ReadOnly Property MatchGroup(ByVal iMatchIndex As Integer, ByVal iGroupIndex As Integer) As String
        Get
            Dim sGroup As String = ""
            On Error Resume Next
            sGroup = _oMatches(iMatchIndex).Groups(iGroupIndex).ToString
            Return sGroup
        End Get
    End Property

    Public ReadOnly Property Matches() As MatchCollection
        Get
            Return _oMatches
        End Get
    End Property

    Public ReadOnly Property Success() As Boolean
        Get
            Return (_oMatches.Count > 0)
        End Get
    End Property

    Public Sub Show()
        Dim oMatch As Match
        Dim oGroup As Group
        Dim oCapture As Capture
        Dim iMatchCount As Integer
        Dim iGroupCount As Integer
        Dim iCaptureCount As Integer

        iMatchCount = 0
        If _oMatches IsNot Nothing Then
            If _oMatches.Count > 0 Then
                Debug.WriteLine("_________________________________________________")
                Debug.WriteLine(vbCrLf & _sSmartMatchName & " pattern: '" & _sPattern & "'")
                For Each oMatch In _oMatches
                    If iMatchCount > 0 Then

                    End If
                    Debug.WriteLine("_____________________________")
                    iGroupCount = 0
                    Debug.WriteLine("  • match(" & iMatchCount & "): " & oMatch.ToString)
                    For Each oGroup In oMatch.Groups
                        If iGroupCount > 0 Then
                            iCaptureCount = 0
                            Debug.WriteLine("    • group(" & iGroupCount & "): " & oGroup.ToString)
                            If oGroup.Captures.Count > 1 Then
                                For Each oCapture In oGroup.Captures
                                    Debug.WriteLine("      • capture(" & iCaptureCount & "): " & oCapture.ToString)
                                    iCaptureCount += 1
                                Next
                            End If
                        End If
                        iGroupCount += 1
                    Next
                    iMatchCount += 1
                Next
            End If
        Else
            Debug.WriteLine("no matches")
        End If
    End Sub
End Class
