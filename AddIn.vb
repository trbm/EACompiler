Imports System.Text.RegularExpressions

Public Class AddIn

    Public Shared SUBMENU_ITEM_COMPILE_MODEL_CS As String = "Compile to target: CS"
    Public Shared SUBMENU_ITEM_COMPILE_MODEL_CS_XML As String = "Compile to target: CS (XML)"
    Public Shared SUBMENU_ITEM_COMPILE_MODEL_PERL As String = "Compile to target: Perl"
    Public Shared SUBMENU_ITEM_COMPILE_MODEL_PYCCA As String = "Compile to target: PYCCA"
    Public Shared SUBMENU_ITEM_COMPILE_MODEL_PYCCA_XML As String = "Compile to target: PYCCA (XML)"
    Public Shared SUBMENU_ITEM_COMPILE_REQUIREMENTS As String = "Compile Requirements/Testing"

    Private Shared _bRunningNow As Boolean = False
    Public Shared VersionString As String = ""

    '_________________________________________________________________________________
    '
    Public Sub EA_GetMenuState(ByVal Repository As EA.Repository, ByVal MenuLocation As String, ByVal MenuName As String, ByVal ItemName As String, ByRef IsEnabled As Boolean, ByRef IsChecked As Boolean)

        'Try
        IsEnabled = Repository.ConnectionString.Length > 0
        'Catch ex As Exception
        '    IsEnabled = False
        'Finally
        '    ' do nothing, just allow us to continue
        'End Try

    End Sub

    '_________________________________________________________________________________
    '
    Public Function EA_Connect(ByVal Repository As EA.Repository) As String
        Globals.VERSION = getAssemblyVersion()

        VersionString = "EA Model Compiler (" & VERSION & ") - Visual Studio, C#"
        Return ""
    End Function

    '_________________________________________________________________________________
    '
    Public Function EA_GetMenuItems(ByVal Repository As EA.Repository, ByVal Location As String, ByVal MenuName As String) As Object
        Select Case Location
            Case "MainMenu"
                Select Case MenuName
                    Case ""
                        Dim sMenuItems() As String = {VersionString}
                        Return sMenuItems

                    Case VersionString
                        Dim sMenuItems() As String = {SUBMENU_ITEM_COMPILE_MODEL_PYCCA_XML, SUBMENU_ITEM_COMPILE_MODEL_CS_XML, SUBMENU_ITEM_COMPILE_REQUIREMENTS}
                        Return sMenuItems

                End Select
        End Select
        Return Nothing
    End Function

    '_________________________________________________________________________________
    '
    Public Sub EA_OnOutputItemClicked(ByVal Repository As EA.Repository, ByVal TabName As String, ByVal LineText As String, ByVal ID As Long)
    End Sub

    '_________________________________________________________________________________
    '
    Public Sub CompileOneModel(ByVal Repository As EA.Repository, ByVal BuildDocumentation As Boolean, ByVal Location As String, ByVal MenuName As String, ByVal ItemName As String)
        CompileModel(New IntermediateRepresentationXML, BuildDocumentation, Repository, "PYCCA.xsl", ".c")
    End Sub

    '_________________________________________________________________________________
    '
    Public Sub EA_MenuClick(ByVal Repository As EA.Repository, ByVal Location As String, ByVal MenuName As String, ByVal ItemName As String)
        CompileOneModel(Repository, False, Location, MenuName, ItemName)
    End Sub

    Private Sub CompileModel(ByRef oOutputLanguage As IOutputLanguage, ByVal BuildDocumentation As Boolean, ByRef oRepository As EA.Repository, ByVal sXSLfilename As String, ByVal sOutputFileExtension As String, Optional ByVal bIncludeDebug As Boolean = False)
        Try
            If Not _bRunningNow Then
                _bRunningNow = True
                oOutputLanguage.CreateDomains(oRepository, BuildDocumentation, bIncludeDebug)
                _bRunningNow = False
            Else
                Beep()
            End If
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Function getAssemblyVersion() As String
        Dim bNonZeroValue As Boolean = False
        Dim oAssembly As System.Reflection.Assembly = System.Reflection.Assembly.GetCallingAssembly()
        Dim oTokens() As String = Regex.Split(oAssembly.FullName, ",")
        Dim sRawVersionString As String = oTokens(1).Substring(oTokens(1).LastIndexOf("=") + 1)
        Return sRawVersionString
    End Function

End Class
