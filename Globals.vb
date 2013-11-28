
Imports System.Xml
Imports System.Text.RegularExpressions
Imports System.Text


Module Globals
    Public VERSION As String = "<set via assembly now>"

    Public Const COPYRIGHT_STRING As String = "Copyright © 2011, 2012       ArrayPower   All rights reserved."

    Public Declare Auto Function PlaySound Lib "winmm.dll" (ByVal lpszSoundName As String, ByVal hModule As Integer, ByVal dwFlags As Integer) As Integer

    Public Const SND_FILENAME As Integer = &H20000
    Public Const SND_RESOURCE As Integer = &H40004

    Public Const VB_BASED As Boolean = True

    Public Const ATTRIBUTE_PREFIX As String = "" ' "ATTR_"

    Public Const TYPE_COLUMN_WIDTH As Integer = 25

    Public Const INTERFACE_CLASS As String = "interface"
    Public Const VISIBILITY_MARKER As String = "makeVisible"

    Public Const REG_REGISTRY_PATH = "Software\\Brennan/marqueZ"
    Public Const REG_ROOT_REGISTRY_KEY = "DeveloperRoot"

    Public Const ATTRIBUTES_SUFFIX As String = "_Attributes"
    Public Const MANY_TYPE_STRING As String = "RELATIONSHIP_END_MANY"
    Public Const ONE_TYPE_STRING As String = "RELATIONSHIP_END_ONE"

    Public giNextStateID As Integer
    Public giNextEventID As Integer
    Public gStatusBox As frmStatusBox

    Public Function CleanNoteString(ByVal oNoteAttribute As EA.Attribute) As String
        Dim sCleanString As String = oNoteAttribute.Notes


        sCleanString = sCleanString.Replace(vbCr, "")
        sCleanString = sCleanString.Replace(vbLf, "")
        Return sCleanString
    End Function

    Public Function PrefixInterfaceLetter(ByVal sName As String, ByVal sPrefix As String) As String
        Dim sReturnName As String = Regex.Replace(sName, "[\[\]]", "")
        Return sPrefix & sReturnName
    End Function

    Public Function CanonicalClassName(ByVal sName As String) As String
        Dim oCanonicalNameList As Collection = Nothing
        Dim oReservedNameList As Collection = Nothing
        Dim sReturnNameString As String = Canonical.CanonicalName(sName)

        If oCanonicalNameList Is Nothing Then
            oCanonicalNameList = New Collection
            With oCanonicalNameList
                .Add(0, "LOOP")
                .Add(0, "WHILE")
                .Add(0, "END")
                .Add(0, "FOR")
                .Add(0, "EACH")
                .Add(0, "NEXT")
                .Add(0, "THEN")
                .Add(0, "ELSE")
                .Add(0, "DIM")
                .Add(0, "PRIVATE")
                .Add(0, "PUBLIC")
                .Add(0, "CONST")
                .Add(0, "STATIC")
                .Add(0, "COLLECTION")
                .Add(0, "HASHTABLE")
                .Add(0, "STRING")
                .Add(0, "LONG")
                .Add(0, "INTEGER")
                .Add(0, "BYVAL")
                .Add(0, "BYREF")
                .Add(0, "SUB")
                .Add(0, "FUNCTION")
            End With

            oReservedNameList = New Collection
            With oReservedNameList
                .Add(0, "TESTDIRECTOR")
                .Add(0, "NUNIT")
                .Add(0, "REQUIREMENT")
            End With
        End If

        If oCanonicalNameList.Contains(sReturnNameString.ToUpper) Then
            sReturnNameString = "[" & sReturnNameString & "]"           ' surrounding the name with brackets will make the compiler happy
        End If

        If oReservedNameList.Contains(sReturnNameString.ToUpper) Then
            sReturnNameString = "_" & sReturnNameString                 ' prefix underscore will avoid conflicts with reserved name
        End If

        sReturnNameString = sReturnNameString.Replace("""", "'")
        sReturnNameString = sReturnNameString.Replace(" ", "_")
        sReturnNameString = sReturnNameString.Replace("-", "_")

        Return sReturnNameString
    End Function

    Public Function SafeDescription(ByVal sDescription As String) As String
        Dim oBuilder As New StringBuilder(sDescription)
        oBuilder = oBuilder.Replace("""", "'")

        Return oBuilder.ToString
    End Function

    Public Function StripRichTextFormat(ByVal sCodeLines As String) As String
        Dim sScrubbedLine As String = sCodeLines

        sScrubbedLine = Regex.Replace(sScrubbedLine, "&gt;", ">")       ' translate the XML special character back
        sScrubbedLine = Regex.Replace(sScrubbedLine, "&lt;", "<")       ' translate the XML special character back
        sScrubbedLine = Regex.Replace(sScrubbedLine, "&amp;", "&")      ' translate the XML special character back
        sScrubbedLine = Regex.Replace(sScrubbedLine, "&quot;", """")    ' translate the XML special character back
        sScrubbedLine = Regex.Replace(sScrubbedLine, "&apos;", "'")     ' translate the XML special character back
        sScrubbedLine = Regex.Replace(sScrubbedLine, "\xA0", "  ")      ' 0xA0 chars suppressed

        sScrubbedLine = Regex.Replace(sScrubbedLine, "</*[a-z0-9 =""#]+>", "")  ' strip RTF tags

        Return sScrubbedLine
    End Function

    Public Function IsUnique(ByVal oUniqueObjectCandidate As Object, ByVal colUniqueBucket As Collection, Optional ByVal sKey As String = "") As Boolean
        Dim bResult As Boolean

        If sKey.Length = 0 Then
            sKey = oUniqueObjectCandidate.GetHashCode.ToString
        End If

        If colUniqueBucket Is Nothing Then
            colUniqueBucket = New Collection
        End If

        If colUniqueBucket.Contains(sKey) Then
            bResult = False
        Else
            bResult = True
            colUniqueBucket.Add(oUniqueObjectCandidate, sKey)
        End If

        Return bResult
    End Function

    Public Function canonicalEventName(ByVal oConnector As EA.Connector) As String
        Return Canonical.CanonicalName(oConnector.TransitionEvent)
    End Function

    Public Function canonicalEventName(ByVal sCanonicalName As String) As String
        sCanonicalName = Regex.Replace(sCanonicalName, "\s*\(", "(")
        sCanonicalName = Canonical.CanonicalName(sCanonicalName)
        Dim sTokens() As String = Split(sCanonicalName, ":")
        If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
            sCanonicalName = sTokens(1)
        End If

        Return Canonical.CanonicalName(sCanonicalName)
    End Function

    Public Sub SetRegistryKey(ByVal sKeyName As String, ByVal sKeyValue As String)
        Dim oRegKey As Microsoft.Win32.RegistryKey

        oRegKey = Microsoft.Win32.Registry.LocalMachine
        oRegKey = oRegKey.CreateSubKey(REG_REGISTRY_PATH)
        oRegKey.SetValue(sKeyName, sKeyValue)
        oRegKey.Close()
    End Sub

    Public Function GetRegistryKey(ByVal sKeyName As String, Optional ByVal sDefault As String = "") As String
        Dim oRegKey As Microsoft.Win32.RegistryKey
        Dim sKeyValue As String = ""

        oRegKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REG_REGISTRY_PATH)
        If Not (oRegKey) Is Nothing Then
            sKeyValue = oRegKey.GetValue(sKeyName)
        End If

        If sKeyValue Is Nothing Then
            sKeyValue = sDefault
        End If

        Return sKeyValue
    End Function

    Public Function PackageIncludesStereotype(ByVal oPackage As EA.Package, ByVal sStereotype As String) As Boolean
        Return oPackage.StereotypeEx.ToUpper.Contains(sStereotype.ToUpper())
    End Function

    Public Function ElementIncludesStereotype(ByVal oElement As EA.Element, ByVal sStereotype As String) As Boolean
        Dim sTokens() As String
        Dim bReturnResult As Boolean = False

        sTokens = Split(sStereotype, ",")
        For Each sToken As String In sTokens
            If oElement.StereotypeEx.ToString.ToUpper = sStereotype.ToUpper Then
                bReturnResult = True
                Exit For
            End If
        Next
        Return bReturnResult
    End Function

    Public Function MethodIncludesStereotype(ByVal oElement As EA.Method, ByVal sStereotype As String) As Boolean
        Return (oElement.StereotypeEx.ToString.ToUpper.Contains(sStereotype.ToUpper))
    End Function

    Public Function AttributeIncludesStereotype(ByVal oAttribute As EA.Attribute, ByVal sStereotype As String) As Boolean
        Return (oAttribute.StereotypeEx.ToString.ToUpper.Contains(sStereotype.ToUpper))
    End Function

    Public Sub TransformXML(sXMLFilename As String, sXSLfilename As String, sOutputFilename As String)
        Dim oXSL As New Xml.Xsl.XslCompiledTransform
        Dim sIntermediateFilename As String = sOutputFilename + "_"
        oXSL.Load(sXSLfilename)
        oXSL.Transform(sXMLFilename, sIntermediateFilename)
        oXSL = Nothing

        Dim oInputFile As New InputFile(sIntermediateFilename)
        Dim oOutputFile As New OutputFile(sOutputFilename)

        Dim sCleanString As String = oInputFile.ToString
        sCleanString = Regex.Replace(sCleanString, "\s*[\r\n]+", vbCrLf)
        sCleanString = Regex.Replace(sCleanString, "/\*\*/", "")

        oOutputFile.Add(sCleanString)
        oInputFile.Close()
        oInputFile = Nothing
        oOutputFile.Close()
        oOutputFile = Nothing

        Dim oFileInfo As New IO.FileInfo(sIntermediateFilename)
        oFileInfo.Delete()
    End Sub

    Public Function RecursiveFileFind(sStartDirectory As String, sFilename As String) As String
        Dim sFullySpecifiedFilename As String = ""
        Dim oCandidateFile As IO.FileInfo = New IO.FileInfo(IO.Path.Combine(sStartDirectory, sFilename))

        If oCandidateFile.Exists Then
            sFullySpecifiedFilename = oCandidateFile.FullName
        Else
            For Each sDirectoryName As String In IO.Directory.GetDirectories(sStartDirectory)
                If Not sDirectoryName.Contains("$") Then            ' if directory is NOT a system directory (like the recycle bin)
                    oCandidateFile = New IO.FileInfo(IO.Path.Combine(sDirectoryName, sFilename))
                    Debug.WriteLine(oCandidateFile.FullName)
                    If oCandidateFile.Exists Then
                        sFullySpecifiedFilename = oCandidateFile.FullName
                        sFilename = ""
                        Exit For
                    Else
                        If sFullySpecifiedFilename.Length = 0 Then
                            sFullySpecifiedFilename = RecursiveFileFind(sDirectoryName, sFilename)
                        Else
                            Exit For
                        End If
                    End If
                End If
            Next
        End If

        Return sFullySpecifiedFilename
    End Function
End Module
