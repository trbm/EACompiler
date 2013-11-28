

Public Class sjmErrorHandler
    Private Shared m_bSuppressDialog As Boolean = False
    Private m_sSupplementalInformation As String = ""
    Private Const VERSION_ID As String = "v5.1"

    Public Shared Property SuppressDialogBox As Boolean
        Set(ByVal Value As Boolean)
            m_bSuppressDialog = Value
        End Set
        Get
            Return m_bSuppressDialog
        End Get
    End Property

    Public Property SupplementalInformation() As String
        Set(ByVal Value As String)
            m_sSupplementalInformation = Value
        End Set
        Get
            Return m_sSupplementalInformation
        End Get
    End Property

    Public Function Announce(ByVal oException As Exception) As MsgBoxResult
        Dim result As MsgBoxResult = MsgBoxResult.No

        Dim sSupplementalLine As String = ""
        Dim sInnerExceptionMessage As String

        If m_sSupplementalInformation.Length > 0 Then
            sSupplementalLine = "Supplemental: " & vbTab & m_sSupplementalInformation & vbCrLf & vbCrLf
        End If

        If oException.InnerException Is Nothing Then
            sInnerExceptionMessage = "(none)"
        Else
            sInnerExceptionMessage = oException.InnerException.Message
        End If

        Dim sDisplayMessage As String = oException.Message & vbCrLf & vbCrLf & _
                vbTab & "Source: " & vbTab & vbTab & oException.Source & vbCrLf & vbCrLf & _
                vbTab & "Inner: " & vbTab & vbTab & sInnerExceptionMessage & vbCrLf & vbCrLf & _
                vbTab & "TargetSite: " & vbTab & oException.TargetSite.Name & vbCrLf & vbCrLf & _
                sSupplementalLine

        If SuppressDialogBox Then
            Console.WriteLine("____________________________________________________________________")
            Console.WriteLine(" ")
            Console.WriteLine("ErrorHandler exception (ignored):")
            Console.WriteLine(" ")
            Console.WriteLine(sDisplayMessage)
            Console.WriteLine("____________________________________________________________________")
        Else
            sDisplayMessage += "Do you want to break to the debugger?" & vbCrLf & vbCrLf & _
                vbTab & "YES" & vbTab & "to break to the debugger                                                        " & vbCrLf & _
                vbTab & "NO" & vbTab & "to just continue (ignoring the error)"

            result = MsgBox(sDisplayMessage, MsgBoxStyle.Critical + MsgBoxStyle.YesNo, "ErrorHandler Component (" & VERSION_ID & ")")
        End If

        m_sSupplementalInformation = ""
        Return result
    End Function

    Public Sub AnnounceMessage(ByVal sMessage As String)
        Dim sSupplementalLine As String = ""

        If m_sSupplementalInformation.Length > 0 Then
            sSupplementalLine = vbTab & m_sSupplementalInformation & vbCrLf & vbCrLf
        End If

        Dim sDisplayMessage As String = "Problem:" & vbTab & sMessage & vbCrLf & vbCrLf & sSupplementalLine
        If SuppressDialogBox Then
            Console.WriteLine("____________________________________________________________________")
            Console.WriteLine(" ")
            Console.WriteLine("ErrorHandler exception (ignored):")
            Console.WriteLine(" ")
            Console.WriteLine(sDisplayMessage)
            Console.WriteLine("____________________________________________________________________")
        Else
            MsgBox(sDisplayMessage, MsgBoxStyle.Critical, "ErrorHandler Component (" & VERSION_ID & ")")
        End If

        m_sSupplementalInformation = ""
    End Sub

    Public Sub New(ByVal oException As System.Exception)
        If MsgBoxResult.Yes = Me.Announce(oException) Then
            Stop
        End If
    End Sub

    Public Sub New(ByVal oException As System.Exception, ByVal sSupplementalInformation As String)
        m_sSupplementalInformation = sSupplementalInformation
        If MsgBoxResult.Yes = Me.Announce(oException) Then
            Debug.Assert(False)
        End If
    End Sub

    Public Sub New()
        m_sSupplementalInformation = ""
    End Sub
End Class