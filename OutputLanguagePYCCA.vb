
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO

Public Class OutputLanguagePYCCA
    Implements IOutputLanguage

    Private Enum EA_TYPE               ' these are EA types (see the EA help topic "Type" which have been inferred by trial and error
        FINAL_STATE = 4
        EXIT_STATE = 14
        INITIAL_STATE = 3
        ENTRY_STATE = 13
        TERMINATE_STATE = 12
        SYNCH_STATE = 6
    End Enum

    Private Const CONSTANTS_COLUMN = 90
    Private Const COLUMN_WIDTH As Integer = 55
    Private Const DEFAULT_INSTANCE_ALLOCATION As Integer = 14
    Private Const INSTANCE_POINTER_ARRAY_COUNT As Integer = DEFAULT_INSTANCE_ALLOCATION                 ' specifies the number of slots allocated to the array of pointers used to formalize x:M relationships

    Private Shared _ModelEnumerations As SortedDictionary(Of String, EA.Element)             ' a collection of *all* enumerations found in the model (using a hashtable to be sure name-order is always the same)
    Private Shared _SortedEnumeratorNames As List(Of String)
    Private Shared _ModelDataTypes As Collection                    ' a collection of *all* data types found in the model
    Private Shared _oRelationshipNames As Collection
    Private Shared _iPackageCount As Integer = 0
    Private Shared _oRelatesConnectorNamesC As Collection
    Private Shared _oRelatesConnectorNamesH As Collection

    Public Sub CreateDomains(ByVal oRepository As EA.Repository, ByVal bIncludeDebug As Boolean, ByVal sXSLfilename As String, ByVal sOutputFileExtension As String) Implements IOutputLanguage.CreateDomains
        Try
            gStatusBox = New frmStatusBox
            gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

            _ModelEnumerations = New SortedDictionary(Of String, EA.Element)
            _ModelDataTypes = New Collection
            _oRelationshipNames = New Collection
            _oRelatesConnectorNamesC = New Collection
            _oRelatesConnectorNamesH = New Collection

            Dim oPackagesList As New Collection

            For Each oPackage As EA.Package In oRepository.Models.GetAt(0).Packages
                recursePackage(oPackage, oPackagesList)
            Next

            If _iPackageCount = 0 Then
                MsgBox("No packages found with stereotype 'pycca' so no compilation was done")
            Else
                For Each oFoundPackage As EA.Package In oPackagesList
                    createDomain(oRepository, oFoundPackage, bIncludeDebug)
                Next
            End If

            createDataTypesFile(oRepository)

            gStatusBox.FadeAway()
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Public Shared Function CanonicalType(ByVal sType As String) As String
        sType = sType.Trim
        Dim sReturnTypeString As String = sType

        If sType.Length > 0 Then
            Select Case sType.ToLower
                Case "boolean", "bool"
                    sReturnTypeString = "bool"

                Case "void"
                    sReturnTypeString = ""

                Case "unsigned long"
                    sReturnTypeString = "long"

                Case "byte", "unsigned char"
                    sReturnTypeString = "byte"

                Case "int"
                    sReturnTypeString = "int"

                Case "char"
                    sReturnTypeString = "string"

                Case "float", "double"
                    sReturnTypeString = "double"

                Case "string"
                    sReturnTypeString = "char*"

            End Select
        End If

        Return Canonical.CanonicalName(sReturnTypeString)
    End Function

    Private Sub recursePackage(ByVal oNextPackage As EA.Package, ByVal oPackages As Collection)
        For Each oPackage As EA.Package In oNextPackage.Packages
            If PackageIncludesStereotype(oPackage, "pycca") Then
                _iPackageCount += 1
                oPackages.Add(oPackage)
            End If
            recursePackage(oPackage, oPackages)
        Next
    End Sub

    Private Function runPyccaProccess(ByVal oRepository As EA.Repository, ByVal sDomainName As String) As Boolean
        Dim sDirectoryPath As String = Path.GetDirectoryName(oRepository.ConnectionString)
        Dim sRandomizer As String = Now.Ticks.ToString()
        Dim CMD_FILENAME As String = "c:\temp_" + sRandomizer + "_" + sDomainName + ".cmd"
        Dim ERROR_FILENAME As String = sDirectoryPath + "\" + sRandomizer + "_" + sDomainName + "_pycca.output.cmd"
        Dim bSuccess As Boolean = True
        Dim oErrorHandler As New sjmErrorHandler
        Dim sInstrumentationString As String = "  -save " '"    -instrument *   "
        Dim oFile = New OutputFile(CMD_FILENAME)
        Dim sErrorString As String = ""

        Try
            For Each sFilename As String In Directory.GetFiles(sDirectoryPath, "*.cmd")             ' cleanup any left over working files, just to keep things tidy
                Try
                    File.Delete(sFilename)
                Catch ' ignore any exceptions when we try to delete files
                Finally
                End Try
            Next

            With oFile
                .Add("cd """ + sDirectoryPath + """")                        ' change directory to the model directory    
                .Add(sDirectoryPath.Substring(0, 2))                ' switch to the appropriate drive (e.g., C: or V:)
                .Add("pycca.exe -noline " + sInstrumentationString + sDomainName + ".pycca" + " > """ + ERROR_FILENAME + """")
            End With
            oFile.Close()

            Try
                oErrorHandler.SupplementalInformation = "Exception thrown while executing '" + CMD_FILENAME + "'"
                Shell(CMD_FILENAME, AppWinStyle.Hide, True)             ' execute the temporary command file
            Catch ex As Exception
                oErrorHandler.Announce(ex)
            End Try

            Dim oErrorFile As InputFile = New InputFile(ERROR_FILENAME)
            sErrorString = oErrorFile.ToString
            oErrorFile = Nothing
            gStatusBox.ReportError(sErrorString)

            Return (sErrorString.Length = 0)
        Catch ex As Exception
            oErrorHandler.Announce(ex)
        End Try
    End Function

    Private Sub createDataTypesFile(ByVal oRepository As EA.Repository)
        Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), "Enumerations")
        Dim oDataTypesFileH As OutputFile = New OutputFile(sOutputFilename + ".h", True)
        Dim oDataTypesFileC As OutputFile = New OutputFile(sOutputFilename + ".c", True)
        Dim oDataTypesFileCS As OutputFile = New OutputFile(sOutputFilename + ".cs", True)

        With oDataTypesFileCS
            .Add()
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//              File: " & sOutputFilename & ".cs")
            .Add("//")
            .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("//")
            .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("//")
            .Add("//________________________________________________________________________________")
            .Add("")
            .Add("using System.Collections;")
            .Add("")
            .Add("class Enumerations")
            .Add("{")
            addEnumeratorsCS(oDataTypesFileCS)
            .Add("}")
            .Close()
        End With

        With oDataTypesFileC
            .Add()
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//              File: " & sOutputFilename & ".c")
            .Add("//")
            .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("//")
            .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("//")
            .Add("//________________________________________________________________________________")
            .Add("")
            .Add("")
            .Add("#include ""Enumerations.h""")
            .Add("")
            addEnumerators(oDataTypesFileC)
            .Close()
        End With

        With oDataTypesFileH
            .Add()
            .Add("#ifndef DATATYPES_H_")
            .Add("#define DATATYPES_H_")
            .Add()
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("//________________________________________________________________________________")
            .Add("//")
            .Add("//              File: " & sOutputFilename & ".h")
            .Add("//")
            .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("//")
            .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("//")
            .Add("//________________________________________________________________________________")
            .Add("")
            .Add("")
            .Add("#define INSTANCE_POINTER_ARRAY_COUNT " + INSTANCE_POINTER_ARRAY_COUNT.ToString() + "       // the number of slots allocated in the multiple instance pointer arrays")
            .Add("")
            .Add("")

            For Each oEnumeration As EA.Element In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    oEnumeration.Name = Canonical.CanonicalName(oEnumeration.Name)           ' massage all the enumeration names to be safe for the C compiler
                End If
            Next

            For Each oEnumeration As EA.Element In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("#define " + oEnumeration.Name + "_COUNT " + oEnumeration.Attributes.Count.ToString)
                End If
            Next

            For Each oEnumeration As EA.Element In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("")
                    .Add("    typedef enum " + oEnumeration.Name)
                    .Add("    {")
                    Dim iEnumeratorCount As Integer = 0

                    _SortedEnumeratorNames = New List(Of String)
                    For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                        oEnumerator.Name = Canonical.CanonicalName(oEnumerator.Name)
                        Dim sComment As String = CleanNoteString(oEnumerator)
                        If sComment.Length > 0 Then
                            sComment = "// " & sComment
                        End If
                        _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
                    Next
                    _SortedEnumeratorNames.Sort()

                    For Each sEnumeratorName As String In _SortedEnumeratorNames
                        .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
                        iEnumeratorCount += 1
                    Next

                    .Add("    } " + Canonical.CanonicalName(oEnumeration.Name) + ";")
                Else
                    If oEnumeration.Notes.Length > 0 Then
                        MsgBox("Enumerations should be specified by creating 'attributes' for each element--it looks like you only put text in the Notes box: " & vbCrLf & vbCrLf & oEnumeration.Notes)
                    End If
                End If
            Next
            .Add("")
            For Each oEnumeration As EA.Element In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("    extern char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID);")
                End If
            Next
            .Add("")
            .Add("")


            For Each oDataType As EA.Element In _ModelDataTypes
                If oDataType.Notes.Length > 0 Then
                    .Add("")
                    .Add("    typedef struct " + Canonical.CanonicalName(oDataType.Name) + "_s")
                    .Add("    {")
                    .Add(oDataType.Notes + "    } " + Canonical.CanonicalName(oDataType.Name) + ";")
                End If
            Next

            .Add("#endif")
            .Close()
        End With
    End Sub

    Private Sub addEnumeratorsCS(ByVal oDataTypesFileC As OutputFile)
        Dim oEnumeration As EA.Element

        With oDataTypesFileC
            For Each oEnumeration In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("")
                    .Add("    public enum " + Canonical.CanonicalName(oEnumeration.Name))
                    .Add("    {")
                    Dim iEnumeratorCount As Integer = 0
                    _SortedEnumeratorNames = New List(Of String)
                    Dim NoteStrings As List(Of String) = New List(Of String)
                    For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                        oEnumerator.Name = Canonical.CanonicalName(oEnumerator.Name)
                        Dim sComment As String = CleanNoteString(oEnumerator)
                        If sComment.Length > 0 Then
                            sComment = "// " & sComment
                        End If
                        _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
                    Next
                    _SortedEnumeratorNames.Sort()

                    For Each sEnumeratorName As String In _SortedEnumeratorNames
                        .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
                        iEnumeratorCount += 1
                    Next
                    .Add("    }")
                End If
            Next

            .Add("")
            .Add("")
            For Each oEnumeration In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("    static private Hashtable " + oEnumeration.Name + "_Descriptions;                                                  ")
                    .Add("    static public string Get_" + oEnumeration.Name + "_Description(long ID)                                           ")
                    .Add("    {                                                                                                                 ")
                    .Add("        string sDescription = ""illegal array index received = "" + ID.ToString() + "" for array '" + oEnumeration.Name + "'"";     ")
                    .Add("        if (" + oEnumeration.Name + "_Descriptions == null)                                                           ")
                    .Add("        {                                                                                                             ")
                    .Add("            " + oEnumeration.Name + "_Descriptions = new Hashtable();                                                 ")
                    For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                        .Add("            " + oEnumeration.Name + "_Descriptions.Add((long)" + oEnumeration.Name + "." + oEnumeration.Name + "_" + Canonical.CanonicalName(oEnumerator.Name) + ", """ + CleanNoteString(oEnumerator) + """); ")
                    Next
                    .Add("        }                                                                                                             ")
                    .Add("                                                                                                                      ")
                    .Add("        if(" + oEnumeration.Name + "_Descriptions.Contains(ID))                                                       ")
                    .Add("        {                                                                                                             ")
                    .Add("           sDescription = (string)" + oEnumeration.Name + "_Descriptions[ID];                                         ")
                    .Add("        }                                                                                                             ")
                    .Add("                                                                                                                      ")
                    .Add("                                                                                                                      ")
                    .Add("        return sDescription;                                                                                          ")
                    .Add("    }                                                                                                                 ")
                    .Add(vbCrLf)
                End If
            Next
            .Add("")
            .Add("")
        End With
    End Sub

    Private Sub addEnumerators(ByVal oDataTypesFileC As OutputFile)
        Dim oEnumeration As EA.Element

        With oDataTypesFileC
            For Each oEnumeration In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    .Add("    static char* " + oEnumeration.Name + "_Descriptions[" + oEnumeration.Attributes.Count.ToString + "] = ")
                    .Add("    {")
                    For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                        .Add("        """ + CleanNoteString(oEnumerator) + " "",")
                    Next
                    .Add("    };")
                    .Add(vbCrLf)
                End If
            Next
            .Add("")
            .Add("")
            For Each oEnumeration In _ModelEnumerations.Values
                If oEnumeration.Attributes.Count > 0 Then
                    'Dim sFirstEnumeratorName As String = ""

                    'For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                    '    sFirstEnumeratorName = oEnumerator.Name
                    '    Exit For
                    'Next

                    .Add("    char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID)")
                    .Add("    {")
                    .Add("        return " + oEnumeration.Name + "_Descriptions[ID];")
                    .Add("    };")
                    .Add(vbCrLf)
                End If
            Next
            .Add("")
            .Add("")
        End With
    End Sub

    Private Sub createAccessorFile(ByVal oDomain As Domain, ByVal sAccessorFilenameRoot As String)
        Dim oAccessorOutputFileC As OutputFile = New OutputFile(sAccessorFilenameRoot + ".c")
        Dim oAccessorOutputFileH As OutputFile = New OutputFile(sAccessorFilenameRoot + ".h")
        Dim oSymbolsOutputFileC As OutputFile = New OutputFile(sAccessorFilenameRoot + "_Symbols.c")

        With oAccessorOutputFileC
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("// ")
            .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("// ")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
            .Add("// ________________________________________________________________________________")
            .Add("")
            .Add("")
            .Add("#include """ + oDomain.Name + "_Accessors.h""")
            .Add("#include <assert.h>")
            .Add("")
            .Add("extern void* _SELF;")
            .Add("")
        End With

        With oAccessorOutputFileH
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("// ")
            .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("// ")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
            .Add("// ________________________________________________________________________________")
            .Add("")
            .Add("")
            .Add("#ifndef " & Path.GetFileName(sAccessorFilenameRoot))
            .Add("#define " & Path.GetFileName(sAccessorFilenameRoot))
            .Add("")
            .Add("#include ""PYCCA.h""")
            .Add("#include ""mechs.h""")
            .Add("#include <string.h>")
            .Add("")

            .Add("// class structure forward references")
            For Each oEAClass As EAClass In oDomain.EAClassInstances
                .Add("    struct " + oEAClass.Name + ";")
            Next
            .Add("")
            .Add("")
        End With

        With oSymbolsOutputFileC
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
            .Add("// ")
            .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
            .Add("// ")
            .Add("// ________________________________________________________________________________")
            .Add("// ")
            .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
            .Add("// ________________________________________________________________________________")
            .Add("")
            .Add("#include ""Platform.h""")
            .Add("#include """ & oDomain.Name & "_Accessors.h""")
            .Add("")
            .Add("")
            For Each oEAClass As EAClass In oDomain.EAClassInstances
                oEAClass.AddAccessors(oAccessorOutputFileC, oAccessorOutputFileH)
            Next
            .Add("")
            For Each oEAClass As EAClass In oDomain.EAClassInstances
                oEAClass.AddStateEnumeration(oSymbolsOutputFileC)
                oEAClass.AddEventEnumeration(oSymbolsOutputFileC)
            Next
            .Add("")
            .Add("    bool " & oDomain.Name & "_ClassInfoFromInstance(MechInstance oMechInstance, INSTANCE_INFO* oInstanceInfo )")
            .Add("    {")
            For Each oEAClass As EAClass In oDomain.EAClassInstances
                If Not (ElementIncludesStereotype(oEAClass.EAElement, "domain") Or ElementIncludesStereotype(oEAClass.EAElement, "external")) Then
                    .Add((("       if( IsInstanceOf_" & oEAClass.Name & "(oMechInstance) )").PadRight(60) + " { oInstanceInfo->ClassName = """ & oEAClass.Name & """; oInstanceInfo->StateNames = " & oEAClass.Name & "_States; oInstanceInfo->EventNames = " & oEAClass.Name & "_Events; }"))
                End If
            Next
            .Add("")
            .Add("        return (strlen(oInstanceInfo->ClassName) > 0);")
            .Add("     }")
        End With


        oAccessorOutputFileH.Add("#endif")

        oAccessorOutputFileC.Close()
        oAccessorOutputFileC = Nothing
        oAccessorOutputFileH.Close()
        oAccessorOutputFileH = Nothing
        oSymbolsOutputFileC.Close()
        oSymbolsOutputFileC = Nothing
    End Sub

    Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef sFileText As String, ByVal sDomainName As String)

        If sFileText.Length > 0 Then
            OutputFile.ClearFilesCreated()
            Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
            With oOutputFile
                .Add("# ________________________________________________________________________________")
                .Add("# ")
                .Add("#          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
                .Add("# ________________________________________________________________________________")
                .Add("# ")
                .Add("#               File: " & sOutputFilename)
                .Add("# ")
                .Add("#         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
                .Add("# ")
                .Add("#          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
                .Add("# ")
                .Add("# ________________________________________________________________________________")
                .Add("# ")
                .Add("#           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
                .Add("# ________________________________________________________________________________")
                .Add("")
                .Add("")
                .Add("domain " + sDomainName)
                .Add()
                .Add("   implementation prolog")
                .Add("   {")
                .Add("   ")
                .Add("#include """ + sDomainName + "_IncludeFilesList.h""")
                .Add("   ")
                .Add("   }")

                .Add("   implementation epilog")
                .Add("   {")
                .Add("   ")
                .Add("#include """ + sDomainName + "_InitialInstancePopulation.c""")
                .Add("#include """ + sDomainName + "_Accessors.c""")
                .Add("   ")
                .Add("   }")

                .Add(sFileText)
                .Add("end  # domain")
            End With
            oOutputFile.Close()
        End If
    End Sub

    Private Sub createDomain(ByVal oRepository As EA.Repository, ByVal oPackage As EA.Package, ByVal bIncludeDebug As Boolean)
        Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), Canonical.CanonicalName(oPackage.Name) & ".pycca")
        Dim sAccessorFilenameRoot As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), Canonical.CanonicalName(oPackage.Name) & "_Accessors")
        Dim oSourceOutout As New RichTextBox

        gStatusBox.Filename = oPackage.Name
        Dim oDomain As Domain = New Domain(oPackage, oRepository, oSourceOutout)      ' constructor does the work

        createOutputFile(sOutputFilename, oSourceOutout.Text, oDomain.Name)
        runPyccaProccess(oRepository, oDomain.Name)

        For Each oChildPackage As EA.Package In oPackage.Packages
            createDomain(oRepository, oChildPackage, bIncludeDebug)
        Next

        createAccessorFile(oDomain, sAccessorFilenameRoot)
    End Sub

    Protected Class Domain
        Private _ClassById As Collection
        Private _Triggers As Collection
        Private _Enumerations As Collection
        Private _DataTypes As Collection
        Private _States As Collection
        Private _Notes As Collection
        Private _Boundarys As Collection
        Private _StateMachines As Collection
        Private _ObjectInstances As Collection
        Private _ElementById As Collection
        Private _InitialStates As Collection
        Private _FinalStates As Collection
        Private _IgnoreIndicatorStates As Collection
        Private _TestElements As Collection
        Private _ParentChildren As Collection
        Private _oExternals As Collection

        Private _oTestFixtureElement As EA.Element
        Private _oSourceOutput As RichTextBox
        Private _oRepository As EA.Repository
        Private _sPackageId As String
        Private _oProject As EA.Project
        Private _oPackage As EA.Package
        Private _IsRealized As Boolean
        Private _Name As String
        Private _DiagramVersion As String
        Private _DiagramNotes As String
        Private _EAClassInstances As New List(Of EAClass)

        Public ReadOnly Property EAClassInstances()
            Get
                Return _EAClassInstances
            End Get
        End Property

        Public ReadOnly Property ClassByID() As Collection
            Get
                Return _ClassById
            End Get
        End Property

        Public ReadOnly Property ParentChildren() As Collection
            Get
                Return _ParentChildren
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                Return _Name
            End Get
        End Property

        Public ReadOnly Property DiagramVersion() As String
            Get
                Return _DiagramVersion
            End Get
        End Property

        Public ReadOnly Property DiagramNotes() As String
            Get
                Return _DiagramNotes
            End Get
        End Property

        Public ReadOnly Property TestFixtureElement() As EA.Element
            Get
                Return _oTestFixtureElement
            End Get
        End Property

        Public ReadOnly Property TestElements() As Collection
            Get
                Return _TestElements
            End Get
        End Property

        Public ReadOnly Property Notes() As Collection
            Get
                Return _Notes
            End Get
        End Property

        Public ReadOnly Property Repository() As EA.Repository
            Get
                Return _oRepository
            End Get
        End Property

        Public ReadOnly Property Boundarys() As Collection
            Get
                Return _Boundarys
            End Get
        End Property

        Public ReadOnly Property ObjectInstances() As Collection
            Get
                Return _ObjectInstances
            End Get
        End Property

        Public ReadOnly Property Externals() As Collection
            Get
                Return _oExternals
            End Get
        End Property

        Public ReadOnly Property StateMachines() As Collection
            Get
                Return _StateMachines
            End Get
        End Property

        Public ReadOnly Property ElementById() As Collection
            Get
                Return _ElementById
            End Get
        End Property

        Public ReadOnly Property States() As Collection
            Get
                Return _States
            End Get
        End Property

        Public ReadOnly Property Triggers() As Collection
            Get
                Return _Triggers
            End Get
        End Property

        Public ReadOnly Property EAClass(ByVal iID As Integer) As EA.Element
            Get
                Dim oClass As EA.Element = Nothing

                If _ClassById.Contains(iID.ToString) Then
                    oClass = _ClassById.Item(iID.ToString)
                Else
                    MsgBox("Unknown class id: " & iID, MsgBoxStyle.Critical)
                End If
                Return oClass
            End Get
        End Property

        Public ReadOnly Property IsRealized() As Boolean
            Get
                Return _IsRealized
            End Get
        End Property

        Public ReadOnly Property Package() As EA.Package
            Get
                Return _oPackage
            End Get
        End Property

        Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository, ByRef oSourceOutput As RichTextBox)
            Try
                giNextStateID = oPackage.Name.GetHashCode
                giNextEventID = giNextStateID

                _oSourceOutput = oSourceOutput
                _oRepository = oRepository
                _oPackage = oPackage
                _sPackageId = oPackage.PackageID
                _Name = Canonical.CanonicalName(oPackage.Name)
                _oTestFixtureElement = Nothing

                Try
                    _DiagramNotes = ""
                    _DiagramVersion = "??"

                    Dim oDiagram As EA.Diagram = oPackage.Diagrams.GetAt(0)
                    _DiagramNotes = oDiagram.Notes
                    _DiagramVersion = oDiagram.Version
                Catch
                Finally
                End Try

                _oExternals = New Collection
                _Boundarys = New Collection
                _Notes = New Collection
                _ObjectInstances = New Collection
                _TestElements = New Collection
                _ClassById = New Collection
                _Triggers = New Collection
                _Enumerations = New Collection
                _DataTypes = New Collection
                _States = New Collection
                _ElementById = New Collection
                _StateMachines = New Collection
                _InitialStates = New Collection
                _FinalStates = New Collection
                _IgnoreIndicatorStates = New Collection
                _ParentChildren = New Collection

                _IsRealized = PackageIncludesStereotype(_oPackage, "realized")

                If Not _IsRealized Then
                    catalogElements()
                    catalogParents()
                    generateSource()
                    checkRelationshipNames()
                    addInitialInstancePopulation()
                    addDomainOperations()
                    createVersionFile()
                    createExternalsFile()
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createExternalsFile()
            Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(_oRepository.ConnectionString), Canonical.CanonicalName(_Name) & "_Externals.h")
            Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)

            With oOutputFile
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//               File: " & sOutputFilename)
                .Add("// ")
                .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
                .Add("// ")
                .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
                .Add("// ")
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
                .Add("// ________________________________________________________________________________")
                .Add("")
                .Add("")
                For Each sExternalOperation As String In _oExternals
                    .Add(("#define eop_" & Canonical.CanonicalName(_Name) & "_" & sExternalOperation).PadRight(50) & " " & sExternalOperation)
                Next
            End With
            oOutputFile.Close()
        End Sub

        Private Sub addDomainOperations()
            With _oSourceOutput
                For Each oEAClass As EAClass In _EAClassInstances
                    oEAClass.AddClassOperations(True)
                Next
            End With
        End Sub

        Private Sub addInitialInstancePopulation()
            With _oSourceOutput
                For Each oClassElement As EA.Element In _ObjectInstances
                    Application.DoEvents()
                    If _sPackageId = oClassElement.PackageID Then
                        If oClassElement.ParentID = 0 Then
                            MsgBox("Please make instance '" & oClassElement.Name & "' a child of it's class type" & vbCrLf & "(e.g., myDog should be a child of Dog)")
                        Else
                            Dim oParentClass As EA.Element = _ClassById(oClassElement.ParentID.ToString)
                            .AppendText(vbCrLf)
                            .AppendText("    instance " & oParentClass.Name & " @ " & oClassElement.Name & vbCrLf)
                            .AppendText(StripRichTextFormat(oClassElement.Notes) & vbCrLf)
                            .AppendText("    end  # instance" & vbCrLf)
                        End If
                    End If
                Next

            End With
        End Sub

        Private Sub checkRelationshipNames()
            Dim oIsUnique As New Collection

            For Each oClass As EA.Element In _ClassById
                For Each oConnector As EA.Connector In oClass.Connectors
                    Dim sSupplierID As String = oConnector.SupplierID.ToString
                    If oConnector.Name.Length > 0 Then
                        If oClass.ElementID = sSupplierID Then
                            If Not IsUnique(oConnector.Name, oIsUnique) Then
                                MsgBox("Class '" + oClass.Name + "' in domain '" + Me.Name + "' has a connector '" + oConnector.Name + "' with a duplicate name")
                            End If
                        End If
                    End If
                Next
            Next
        End Sub

        Private Sub snip(ByVal sMyID As String, ByRef sAncestryString As String)
            Dim sOrignalAncestryString As String = sAncestryString
            sAncestryString = Regex.Replace(sAncestryString, "[,]*" + sMyID, "")
        End Sub

        Private Function matchIDs(ByVal sAncestryString1 As String, ByVal sAncestryString2 As String) As Boolean
            Dim bMatch As Boolean = False

            sAncestryString1 = Regex.Replace(sAncestryString1, "[,]+", " ")         ' all double commas become single spaces
            sAncestryString1 = Regex.Replace(sAncestryString1, "[ ]+", " ")         ' all double spaces become singles
            Dim sAncestry1Ids() = Split(sAncestryString1.Trim, " ")

            sAncestryString2 = Regex.Replace(sAncestryString2, "[,]+", " ")         ' all double commas become single spaces
            sAncestryString2 = Regex.Replace(sAncestryString2, "[ ]+", " ")         ' all double spaces become singles
            Dim sAncestry2Ids() = Split(sAncestryString2.Trim, " ")

            If (sAncestry1Ids.Length = sAncestry2Ids.Length) And _
               (sAncestryString1.Length > 0) And _
               (sAncestryString2.Length > 0) Then

                Dim oComparisonCollection As New Collection
                For Each sID1 As String In sAncestry1Ids                            ' first add all the ancestry 1 IDs
                    oComparisonCollection.Add(sID1, sID1)
                Next

                bMatch = True                                                       ' assume all will match
                For Each sID2 As String In sAncestry2Ids                            ' next verify all the ancestry 2 IDs are represented
                    If Not oComparisonCollection.Contains(sID2) Then
                        bMatch = False                                              ' any single failure is enough to bail out 
                        Exit For
                    End If
                Next
            End If

            Return bMatch
        End Function

        Private Function removeNonFamilyIDs(ByVal oFamily As Collection, ByVal oNonFamily As Collection) As Collection
            Dim oFamilyMemberAncestry As New Collection             ' we start with a complete set of ancestry IDs for these family members
            For Each oFamilyMemberClass As EA.Element In oFamily
                oFamilyMemberAncestry.Add(oFamilyMemberClass.GetRelationSet(EA.EnumRelationSetType.rsParents), oFamilyMemberClass.ElementID.ToString)
            Next

            For Each oNonFamilyMemberClass As EA.Element In oNonFamily
                Dim sNonFamilyMemberClassID As String = oNonFamilyMemberClass.ElementID.ToString

                For Each oFamilyMemberClass As EA.Element In oFamily
                    Dim sFamilyMemberID As String = oFamilyMemberClass.ElementID.ToString
                    Dim sFamilyMemberAncestry As String = oFamilyMemberAncestry(sFamilyMemberID)

                    oFamilyMemberAncestry.Remove(sFamilyMemberID)
                    snip(sNonFamilyMemberClassID, sFamilyMemberAncestry)
                    oFamilyMemberAncestry.Add(sFamilyMemberAncestry, sFamilyMemberID)
                Next
            Next

            Return oFamilyMemberAncestry
        End Function

        Private Sub appendChild(ByVal oParentClass As EA.Element, ByVal oChildClass As EA.Element)
            Dim oChildren As Collection

            If Not _ParentChildren.Contains(oParentClass.ElementID.ToString) Then
                oChildren = New Collection
                _ParentChildren.Add(oChildren, oParentClass.ElementID.ToString)     ' add this parent's child collection to the main collection
            End If
            oChildren = _ParentChildren(oParentClass.ElementID.ToString)

            If Not oChildren.Contains(oChildClass.ElementID.ToString) Then
                oChildren.Add(oChildClass, oChildClass.ElementID.ToString)              ' add one child to this parent's child collection
            End If
        End Sub

        Private Sub catalogParents()
            Dim oFamily As Collection
            Dim oNonFamily As Collection

            For Each oClass As EA.Element In ClassByID
                oFamily = New Collection
                oNonFamily = New Collection
                If oClass.GetRelationSet(EA.EnumRelationSetType.rsParents) = "" Then        ' this is a non-subtype class
                    For Each oCandidateChildClass As EA.Element In ClassByID
                        Dim sCandidateChildClassAncestry As String = oCandidateChildClass.GetRelationSet(EA.EnumRelationSetType.rsParents)
                        If sCandidateChildClassAncestry.Contains(oClass.ElementID.ToString) Or (oCandidateChildClass Is oClass) Then
                            oFamily.Add(oCandidateChildClass, oCandidateChildClass.ElementID.ToString)
                        Else
                            oNonFamily.Add(oCandidateChildClass, oCandidateChildClass.ElementID.ToString)
                        End If
                    Next

                    Dim oFamilyMemberAncestry As Collection = removeNonFamilyIDs(oFamily, oNonFamily)

                    For Each oCandidateParent As EA.Element In oFamily
                        Dim sCandidateParentIDWithSelf As String = oFamilyMemberAncestry(oCandidateParent.ElementID.ToString) + "," + oCandidateParent.ElementID.ToString
                        For Each oCandidateChildClass As EA.Element In oFamily
                            Dim sCandidateChildClassAncestry As String = oFamilyMemberAncestry(oCandidateChildClass.ElementID.ToString)
                            If (matchIDs(sCandidateChildClassAncestry, sCandidateParentIDWithSelf)) Then
                                appendChild(oCandidateParent, oCandidateChildClass)
                            End If
                        Next
                    Next
                End If
            Next
        End Sub

        Private Sub catalogElement(ByVal oElement As EA.Element)
            Application.DoEvents()

            oElement.Name = CanonicalClassName(oElement.Name)           ' establish safe names right off the bat (rather than sprinkling everywehre)
            _ElementById.Add(oElement, oElement.ElementID)              ' just as a debugging convenience, to look up any element from its id only

            If oElement.Name.Length = 0 Then
                oElement.Name = "NoName_" & oElement.ElementID
            End If

            Select Case oElement.MetaType
                Case "Object"
                    _ObjectInstances.Add(oElement, oElement.ElementID)

                Case "StateMachine"
                    _StateMachines.Add(oElement, oElement.ElementID)

                Case "FinalState"
                    _FinalStates.Add(oElement, oElement.ElementID)
                    _States.Add(oElement, oElement.ElementID)

                Case "Pseudostate"
                    Select Case oElement.Name
                        Case "Initial"
                            _InitialStates.Add(oElement, oElement.ElementID)
                            _States.Add(oElement, oElement.ElementID)

                        Case Else
                            _IgnoreIndicatorStates.Add(oElement, oElement.ElementID)
                            _States.Add(oElement, oElement.ElementID)
                    End Select

                Case "Trigger"
                    _Triggers.Add(oElement, oElement.Name)

                Case "StateNode"
                    _States.Add(oElement, oElement.ElementID)

                Case "Enumeration"
                    _Enumerations.Add(oElement, oElement.ElementID)
                    _ModelEnumerations.Add(oElement.Name, oElement)

                Case "DataType"
                    _DataTypes.Add(oElement, oElement.ElementID)
                    _ModelDataTypes.Add(oElement, oElement.ElementID)

                Case "Class"
                    oElement.Name = CanonicalClassName(oElement.Name)
                    _ClassById.Add(oElement, oElement.ElementID)

                Case "State"
                    oElement.Name = Canonical.CanonicalName(oElement.Name)
                    _States.Add(oElement, oElement.ElementID)

                Case "Note", "Text"
                    ' do nothing with these, just allow them without complaint

                Case Else
                    Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
            End Select

            For Each oSubElement As EA.Element In oElement.Elements
                catalogElement(oSubElement)                      ' recurse down the element tree
            Next
        End Sub

        Private Sub catalogElements()
            Dim oElement As EA.Element = Nothing
            Try
                For Each oElement In _oPackage.Elements
                    catalogElement(oElement)
                Next
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler
                oErrorHandler.SupplementalInformation = "Catalog Elements (" & oElement.Name & ")"
                oErrorHandler.Announce(ex)
            End Try
        End Sub

        Private Sub sortByStereoType(ByVal oElement As EA.Element)
            If oElement.Stereotype.ToUpper.Contains("TESTFIXTURE") Then
                If _oTestFixtureElement Is Nothing Then
                    _oTestFixtureElement = oElement
                Else
                    Throw New ApplicationException("Only a single test fixture is allowed per domain, but both '" & _oTestFixtureElement.Name & "' and '" & oElement.Name & "' were found")
                End If
            Else
                If ElementIncludesStereotype(oElement, "test") Then
                    _TestElements.Add(oElement, oElement.Name)
                End If
            End If
        End Sub

        'Private Sub recordStateElement(ByVal oElement As EA.Element)
        '    Dim oErrorHandler As New sjmErrorHandler

        '    oErrorHandler.SupplementalInformation = "_States (State): " & oElement.Name                         ' in case an exception is thrown
        '    _States.Add(oElement, oElement.ElementID)
        'End Sub

        Private Sub createVersionFile()
            Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(_oRepository.ConnectionString), Canonical.CanonicalName(_Name) & "Version.h")
            Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
            Dim sTokens() As String = _DiagramVersion.Replace("v", "").Split(".")
            Dim sMajor As String = ""
            Dim sMinor As String = ""
            Dim sSub As String = ""

            If sTokens.Length > 0 Then
                sMajor = sTokens(0)
            End If

            If sTokens.Length > 1 Then
                sMinor = sTokens(1)
            End If

            If sTokens.Length > 2 Then
                sSub = sTokens(2)
            End If

            With oOutputFile
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//               File: " & sOutputFilename)
                .Add("// ")
                .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
                .Add("// ")
                .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
                .Add("// ")
                .Add("// ________________________________________________________________________________")
                .Add("// ")
                .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
                .Add("// ________________________________________________________________________________")
                .Add("")
                .Add("")
                .Add("int " & _Name & "_VERSION_MAJOR = " & sMajor & ";")
                .Add("int " & _Name & "_VERSION_MINOR = " & sMinor & ";")
                .Add("int " & _Name & "_VERSION_SUB = " & sSub & ";")
                .Add("")
            End With
            oOutputFile.Close()
        End Sub

        Private Sub generateSource()
            Dim oClassElement As EA.Element
            Dim oEAClass As EAClass
            Static iClassCounter As Integer = 0

            Try
                If (_ClassById.Count > 0) Or (_Enumerations.Count > 0) Or (_DataTypes.Count > 0) Then
                    Application.DoEvents()
                    With _oSourceOutput
                        If _oPackage.Notes.Length > 0 Then
                            .AppendText("       # " & _oPackage.Notes)
                        End If
                        .AppendText(vbCrLf)

                        gStatusBox.ProgressValueMaximum = _ClassById.Count
                        For Each oClassElement In _ClassById
                            gStatusBox.ProgressValue = iClassCounter
                            Application.DoEvents()
                            If _sPackageId = oClassElement.PackageID Then
                                If Not ElementIncludesStereotype(oClassElement, "DataType") Then  ' classes with stereotype 'DataType' are just psuedo-types for use in the model
                                    If ElementIncludesStereotype(oClassElement, "omit") Then
                                        Debug.WriteLine("Omitting class (by stereotype 'omit'): " & oClassElement.Name)
                                    Else
                                        oEAClass = New EAClass(oClassElement, Me, _oSourceOutput)
                                        _EAClassInstances.Add(oEAClass)
                                    End If
                                End If
                            End If
                            iClassCounter += 1
                        Next
                    End With
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

    End Class

    Private Class EAClass
        Private _oEAElement As EA.Element
        Private _oSourceOutput As RichTextBox
        Private _oDomain As Domain
        Private _bFinalStateReported As Boolean = False
        Private _sInitialState As String = ""
        Private _bActiveClass As Boolean = False
        Private _sParentClassName As String = ""
        Private _bIsSupertype As Boolean
        Private _bIsSubtype As Boolean
        Private _oStateClassSequences As New Collection
        Private _oChildrenCollection As Collection
        Private _oEventNames As New List(Of String)

        Public Sub New(ByVal oEAElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
            Dim bIsTestFixture As Boolean = False
            Dim bIsTest As Boolean = False
            Dim i As Integer
            Dim iSlots As Integer = DEFAULT_INSTANCE_ALLOCATION
            Dim sSlotsComment As String = "default value"

            Try
                _oDomain = oDomain
                _oSourceOutput = oSourceOutput
                _oEAElement = oEAElement

                gStatusBox.ShowClassName(_oEAElement.Name)

                If _oDomain.TestFixtureElement IsNot Nothing Then
                    bIsTestFixture = (_oDomain.TestFixtureElement.Name = _oEAElement.Name)
                End If
                bIsTest = _oDomain.TestElements.Contains(_oEAElement.Name)

                With _oSourceOutput
                    .AppendText(vbCrLf)

                    If Not (ElementIncludesStereotype(_oEAElement, "domain") Or ElementIncludesStereotype(_oEAElement, "external")) Then
                        .AppendText("    #_________________________________________________________________________" & vbCrLf)
                        .AppendText("    class " & _oEAElement.Name & "         ## element id: " & _oEAElement.ElementID & vbCrLf)
                        .AppendText(vbCrLf)
                        .AppendText("        population dynamic" + vbCrLf)

                        For i = 0 To 100
                            If ElementIncludesStereotype(_oEAElement, i.ToString) Then
                                iSlots = i
                                sSlotsComment = "specified via stereotype in the model"
                                Exit For
                            End If
                        Next
                        .AppendText("        slots " & iSlots.ToString() & "          # " & sSlotsComment & vbCrLf)
                        .AppendText(vbCrLf)

                        Dim oAttributes As Collection = accumulateAttributes(_oEAElement)
                        Dim oConnectors As Collection = accumulateConnectors(_oEAElement)

                        addStateMachine()
                        addAttributes()
                        addSubtypes()
                        addPolymorphicEvents()

                        AddClassOperations(False)          ' add all the operations 

                        .AppendText("    end  # class" & vbCrLf)
                    Else
                        AddDomainExternalOperations()
                    End If
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Public ReadOnly Property IsSupertype() As Boolean
            Get
                Return _bIsSupertype
            End Get
        End Property

        Public ReadOnly Property EAElement() As EA.Element
            Get
                Return _oEAElement
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                Return _oEAElement.Name
            End Get
        End Property

        Public Sub AddClassOperations(ByVal bDomainLevel As Boolean)
            Dim oMethod As EA.Method
            Dim oParameter As EA.Parameter
            Dim sLeadingComma As String = ""
            Dim oRequiredParameters As Collection
            Dim sBehavior As String = ""

            With _oSourceOutput
                For Each oMethod In _oEAElement.Methods
                    If Not MethodIncludesStereotype(oMethod, "polymorphic") Then        ' "operations" tagged as "polymorphic" are actually polymorphic events -- was the best place I could find to add poly event specfication
                        If (Not bDomainLevel And (Not MethodIncludesStereotype(oMethod, "domain") Or (Not MethodIncludesStereotype(oMethod, "external")) Or _
                                   (bDomainLevel And (MethodIncludesStereotype(oMethod, "domain") Or MethodIncludesStereotype(oMethod, "external"))))) Then

                            Application.DoEvents()
                            If Not MethodIncludesStereotype(oMethod, "event") Then
                                sBehavior = StripRichTextFormat(oMethod.Behavior)

                                oMethod.ReturnType = CanonicalType(oMethod.ReturnType)
                                oMethod.Name = Canonical.CanonicalName(oMethod.Name)
                                sLeadingComma = ""

                                .AppendText(vbCrLf)
                                If oMethod.Notes.Length > 0 Then
                                    For Each sLine As String In Split(oMethod.Notes, vbCrLf)
                                        If sLine.Length > 0 Then
                                            .AppendText("    # " & sLine)
                                        End If
                                    Next
                                End If

                                .AppendText(vbCrLf)
                                If MethodIncludesStereotype(oMethod, "instance") Then
                                    .AppendText("    instance operation" & vbCrLf)
                                Else
                                    .AppendText("    class operation" & vbCrLf)
                                End If
                            End If
                            .AppendText("    " + oMethod.Name + "(")

                            oRequiredParameters = New Collection
                            For Each oParameter In oMethod.Parameters
                                Application.DoEvents()
                                oParameter.Name = Canonical.CanonicalName(oParameter.Name)
                                oParameter.Type = CanonicalType(oParameter.Type)
                                oRequiredParameters.Add(oParameter)
                            Next

                            For Each oParameter In oRequiredParameters
                                .AppendText(sLeadingComma & oParameter.Type + " " + oParameter.Name)
                                sLeadingComma = ", "
                            Next
                            .AppendText(")")
                            If oMethod.ReturnType.Length > 0 Then
                                .AppendText("     : (" & oMethod.ReturnType & ")")
                            End If
                            .AppendText(vbCrLf)
                            .AppendText("    {" & vbCrLf)
                            .AppendText(sBehavior)
                            .AppendText(vbCrLf)
                            .AppendText("    }" & vbCrLf)
                        End If
                    End If
                Next
            End With
        End Sub

        Public Sub AddDomainExternalOperations()
            Dim oMethod As EA.Method
            Dim oParameter As EA.Parameter
            Dim sLeadingComma As String = ""
            Dim oRequiredParameters As Collection
            Dim sBehavior As String = ""

            With _oSourceOutput
                For Each oMethod In _oEAElement.Methods
                    Application.DoEvents()
                    sBehavior = StripRichTextFormat(oMethod.Behavior)

                    oMethod.ReturnType = CanonicalType(oMethod.ReturnType)
                    oMethod.Name = Canonical.CanonicalName(oMethod.Name)
                    sLeadingComma = ""

                    .AppendText(vbCrLf)
                    If oMethod.Notes.Length > 0 Then
                        For Each sLine As String In Split(oMethod.Notes, vbCrLf)
                            If sLine.Length > 0 Then
                                .AppendText("    # " & sLine)
                            End If
                        Next
                    End If

                    .AppendText(vbCrLf)
                    .AppendText(vbCrLf)

                    If MethodIncludesStereotype(oMethod, "domain") Then
                        .AppendText("    domain operation" & vbCrLf)
                    Else
                        If MethodIncludesStereotype(oMethod, "external") Then
                            .AppendText("    external operation" & vbCrLf)
                            _oDomain.Externals.Add(oMethod.Name)
                        End If
                    End If

                    .AppendText("sync_" & oMethod.Name & "(SyncParamRef params)")
                    oRequiredParameters = New Collection
                    For Each oParameter In oMethod.Parameters
                        Application.DoEvents()
                        oParameter.Name = Canonical.CanonicalName(oParameter.Name)
                        oParameter.Type = CanonicalType(oParameter.Type)
                        oRequiredParameters.Add(oParameter)
                    Next

                    .AppendText("        {")
                    Dim i As Integer = 0
                    For Each oParameter In oRequiredParameters
                        .AppendText("    " & oParameter.Type & " " & oParameter.Name & " = (" & oParameter.Type & ")params->ilparm[" & i & "];           // get parameter " & i & " from the synch param block" & vbCrLf)
                        i += 1
                    Next
                    .AppendText(vbCrLf)
                    .AppendText(sBehavior)
                    .AppendText("        }" & vbCrLf)

                    If MethodIncludesStereotype(oMethod, "domain") Then
                        .AppendText("    domain operation" & vbCrLf)

                        .AppendText("    " + oMethod.Name + "(")
                        sLeadingComma = ""
                        For Each oParameter In oRequiredParameters
                            .AppendText(sLeadingComma & oParameter.Type + " " + oParameter.Name)
                            sLeadingComma = ", "
                        Next
                        .AppendText(")")
                        .AppendText("    {")

                        .AppendText("    SyncParamRef p;" & vbCrLf)
                        .AppendText("    p = mechSyncRequest(" & _oDomain.Name & "_sync_" & oMethod.Name & ");   // queue up the sync function")
                        i = 0
                        For Each oParameter In oRequiredParameters
                            .AppendText(vbCrLf & "    p->ilparm[" & i & "] = (long)" & oParameter.Name & ";")
                            i += 1
                        Next
                        .AppendText(vbCrLf & "        }")
                    End If
                Next
            End With
        End Sub

        Public Sub AddStateEnumeration(ByVal oAccessorOutputFileH As OutputFile)
            Dim iClassStateMachineID As Integer = getClassStateMachineID(_oEAElement)
            Dim oState As EA.Element
            Dim oStateNames As New Collection
            Dim iStateNamesCount As Integer

            If _oDomain.States.Count > 0 Then
                For Each oState In _oDomain.States
                    If oState.ParentID = iClassStateMachineID Then
                        oStateNames.Add(oState.Name)
                    End If
                Next
            End If

            With oAccessorOutputFileH
                iStateNamesCount = oStateNames.Count + 1
                .Add("")
                .Add(("    char* " & Name & "_States[" & iStateNamesCount.ToString & "] =").PadRight(50) + " // count is +1 to gracefully handle the 'no states' case")
                .Add("    {")
                Dim iIndex As Integer = 0
                For Each sStateName As String In oStateNames
                    .Add(("        """ & sStateName & """,").PadRight(50) + " // " + iIndex.ToString())
                    iIndex += 1
                Next
                .Add("        """"")
                .Add("    };")
            End With
        End Sub

        Public Sub AddEventEnumeration(ByVal oAccessorOutputFileH As OutputFile)
            Dim iEventNamesCount As Integer = _oEventNames.Count + 1

            With oAccessorOutputFileH
                .Add(("    char* " & Name & "_Events[" & iEventNamesCount.ToString & "] =").PadRight(50) + " // count is +1 to gracefully handle the 'no events' case")
                .Add("    {")
                _oEventNames.Sort()
                Dim iIndex As Integer = 0
                For Each sEventName As String In _oEventNames
                    .Add(("        """ & sEventName & """,").PadRight(50) + " // " + iIndex.ToString())
                    iIndex += 1
                Next
                .Add("        """"")
                .Add("    };")
                .Add("    //...............................................................................")
            End With
        End Sub

        Private Sub addRelatesC(ByVal oAccessorOutputFileC As OutputFile, ByVal oConnector As EA.Connector)
            Dim sOtherClassName As String
            Dim sSupplierCardinality As String
            Dim sClientCardinality As String

            With oAccessorOutputFileC
                oConnector.Name = Canonical.CanonicalName(oConnector.Name)
                'Debug.Assert(Not oConnector.Name = "R251")
                If IsUnique(oConnector.Name, _oRelatesConnectorNamesC) Then          ' if we haven't handled this connector's Relate...
                    If oConnector.Type = "Association" Then
                        If _oEAElement.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
                            sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
                            sClientCardinality = oConnector.SupplierEnd.Cardinality
                        Else
                            sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
                            sClientCardinality = oConnector.ClientEnd.Cardinality
                        End If

                        If sOtherClassName = _oEAElement.Name Then
                            .Add("    // reflexive relationships (like '" & oConnector.Name & "') are not supported by accessors")
                        Else
                            Select Case sSupplierCardinality
                                Case "1", "0..1"
                                    Select Case sClientCardinality
                                        Case "1", "0..1"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ",  struct " & sOtherClassName & "* o" & sOtherClassName & ") { o" & _oEAElement.Name & "->" & oConnector.Name & " = o" & sOtherClassName & "; o" & sOtherClassName & "->" & oConnector.Name & " = o" & _oEAElement.Name & "; }  // 1a")
                                        Case "0..*", "1..*"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ",  struct " & sOtherClassName & "* o" & sOtherClassName & ") { o" & _oEAElement.Name & "->" & oConnector.Name & " = o" & sOtherClassName & "; ClassRefSetVar( " & _oEAElement.Name & ", o" & _oEAElement.Name & "s );  PYCCA_relateToMany( o" & _oEAElement.Name & "s, o" & sOtherClassName & " , " & oConnector.Name & ", o" & _oEAElement.Name & " ); }  //1 ")
                                    End Select

                                Case "0..*", "1..*"
                                    Select Case sSupplierCardinality
                                        Case "1", "0..1"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & sOtherClassName & "* o" & sOtherClassName & ",  struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ") { o" & sOtherClassName & "->" & oConnector.Name & " = o" & _oEAElement.Name & "; o" & _oEAElement.Name & "->" & oConnector.Name & " = o" & sOtherClassName & "; }  // 2a")
                                        Case "0..*", "1..*"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & sOtherClassName & "* o" & sOtherClassName & ",  struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ") { o" & sOtherClassName & "->" & oConnector.Name & " = o" & _oEAElement.Name & "; ClassRefSetVar( " & sOtherClassName & ", o" & sOtherClassName & "s );  PYCCA_relateToMany( o" & sOtherClassName & "s, o" & _oEAElement.Name & " , " & oConnector.Name & ", o" & sOtherClassName & " ); }    //2 ")
                                    End Select

                                Case Else
                                    ' do nothing, this error is announced elsewhere during processing
                            End Select
                        End If
                    End If
                End If
            End With
        End Sub

        Private Sub addRelatesH(ByVal oAccessorOutputFileH As OutputFile, ByVal oConnector As EA.Connector)
            Dim sOtherClassName As String
            Dim sSupplierCardinality As String
            Dim sClientCardinality As String

            With oAccessorOutputFileH
                oConnector.Name = Canonical.CanonicalName(oConnector.Name)

                If IsUnique(oConnector.Name, _oRelatesConnectorNamesH) Then          ' if we haven't handled this connector's Relate...
                    If oConnector.Type = "Association" Then
                        If _oEAElement.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
                            sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
                            sClientCardinality = oConnector.SupplierEnd.Cardinality
                        Else
                            sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
                            sClientCardinality = oConnector.ClientEnd.Cardinality
                        End If

                        If sOtherClassName = _oEAElement.Name Then
                            .Add("    // reflexive relationships (like '" & oConnector.Name & "') are not supported by accessors")
                        Else
                            Select Case sSupplierCardinality
                                Case "1", "0..1"
                                    Select Case sClientCardinality
                                        Case "1", "0..1"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ",  struct " & sOtherClassName & "* o" & sOtherClassName & ");")
                                        Case "0..*", "1..*"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ",  struct " & sOtherClassName & "* o" & sOtherClassName & ");")
                                    End Select

                                Case "0..*", "1..*"
                                    Select Case sSupplierCardinality
                                        Case "1", "0..1"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & sOtherClassName & "* o" & sOtherClassName & ",  struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ");")
                                        Case "0..*", "1..*"
                                            .Add("    void Relate_" & oConnector.Name & "( struct " & sOtherClassName & "* o" & sOtherClassName & ",  struct " & _oEAElement.Name & "* o" & _oEAElement.Name & ");")
                                    End Select

                                Case Else
                                    ' do nothing, this error is announced elsewhere during processing
                            End Select
                        End If
                    End If
                End If
            End With
        End Sub

        Public Sub AddAccessors(ByVal oAccessorOutputFileC As OutputFile, ByVal oAccessorOutputFileH As OutputFile)
            Dim sPrefix As String = Name + "_"              ' this prefix will include the class name preceding the event name
            sPrefix = ""                                    ' but that class name is probably redundant, so let's omit it

            If Not (ElementIncludesStereotype(_oEAElement, "domain") Or ElementIncludesStereotype(_oEAElement, "external")) Then
                With oAccessorOutputFileC
                    .Add("    //___________________________________________________ " + Name)
                    .Add("    bool IsInstanceOf_" + Name + "(MechInstance oInstance)                         { return ((void*)BeginStorage(" + Name + ") <= (void*)oInstance) && ((void*)oInstance < (void*)EndStorage(" + Name + ")); }")
                    .Add("    void " + Name + "_SelectOneInstance(struct " + Name + "** oFoundInstance )     { PYCCA_selectOneInstWhere( *oFoundInstance, " + Name + ", ""true""); }")

                    .Add("")
                    For Each oConnector As EA.Connector In _oEAElement.Connectors
                        addRelatesC(oAccessorOutputFileC, oConnector)
                        addRelatesH(oAccessorOutputFileH, oConnector)
                    Next

                    For Each sEventName As String In _oEventNames
                        .Add("")
                        .Add("    void " + sPrefix + sEventName + "(MechInstance sourceInstance, struct " + Name + "* targetInstance)	{ mechEventGenerate(EventNumber(" + Name + ",  " + sEventName + "), (MechInstance)targetInstance, sourceInstance); }")
                        .Add("    void " + sPrefix + sEventName + "_Delayed(MechInstance sourceInstance, struct " + Name + "* targetInstance, int delayTicks)    { mechEventGenerateDelayed(EventNumber(" + Name + ",  " + sEventName + "), (MechInstance)targetInstance, sourceInstance, delayTicks); }")
                        .Add("    void " + sPrefix + sEventName + "_Self(struct " + Name + "* self)                                     { mechEventGenerate(EventNumber(" + Name + ",  " + sEventName + "), (MechInstance)self, (MechInstance)self); }")
                        .Add("    void " + sPrefix + sEventName + "_Self_Delayed(struct " + Name + "* self, int delayTicks)             { mechEventGenerateDelayed(EventNumber(" + Name + ",  " + sEventName + "), (MechInstance)self, (MechInstance)self, delayTicks); }")
                        .Add("    void " + sPrefix + sEventName + "_ToSelf()                                                            { assert(_SELF != NULL); " + sPrefix + sEventName + "_Self( _SELF ); }")
                        .Add("    void " + sPrefix + sEventName + "_DelayedToSelf(int delayTicks)                                       { assert(_SELF != NULL); " + sPrefix + sEventName + "_Self_Delayed( _SELF, delayTicks); }")
                    Next
                    .Add("    																										            ")
                End With

                With oAccessorOutputFileH
                    .Add("    //___________________________________________________ " + Name)
                    .Add("    void " + Name + "_SelectOneInstance(struct " + Name + "** oFoundInstance );")
                    .Add("    bool IsInstanceOf_" + Name + "(MechInstance oInstance);")
                    For Each sEventName As String In _oEventNames
                        .Add("")
                        .Add("    void " + sPrefix + sEventName + "(MechInstance sourceInstance, struct " + Name + "* targetInstance);		    ")
                        .Add("    void " + sPrefix + sEventName + "_Delayed(MechInstance sourceInstance, struct " + Name + "* targetInstance, int delayTicks);")
                        .Add("    void " + sPrefix + sEventName + "_Self(struct " + Name + " *const self);")
                        .Add("    void " + sPrefix + sEventName + "_Self_Delayed(struct " + Name + "* self, int delayTicks);")
                        .Add("    void " + sPrefix + sEventName + "_ToSelf(void);")
                        .Add("    void " + sPrefix + sEventName + "_DelayedToSelf(int delayTicks);")
                    Next
                    .Add("    																										            ")
                End With
            End If
        End Sub

        Private Function getClassStateMachineID(ByVal oClass As EA.Element)
            Dim iStateMachineID As Integer = oClass.ElementID

            For Each oStateMachineElement As EA.Element In _oDomain.StateMachines
                Application.DoEvents()
                If oClass.ElementID = oStateMachineElement.ParentID Then
                    iStateMachineID = oStateMachineElement.ElementID
                    Exit For
                End If
            Next
            Return iStateMachineID
        End Function

        Private Sub recordStateCallingSequence(ByVal oState As EA.Element, ByVal sArgumentList As String, ByVal oClass As EA.Element)
            Dim sCallingSequence As String = "        state " & oState.Name & " ("

            If sArgumentList.Length > 0 Then
                Dim sArgumentString As String = " "
                Dim sDelimiter As String = " "
                Dim sArguments() As String = Split(sArgumentList + ",", ",")
                For Each sArgument As String In sArguments
                    If sArgument.Length > 0 Then
                        Dim sTypeNamePairs() = Split(sArgument.Trim, " ")
                        Dim sType As String = CanonicalType(sTypeNamePairs(0)).Trim
                        Dim sName As String = sTypeNamePairs(1).Trim
                        sCallingSequence &= sDelimiter & vbCrLf & "            " & sType & " " & sName
                        sDelimiter = ", "
                    End If
                Next
            End If
            sCallingSequence &= ")" & vbCrLf
            sCallingSequence &= "            {"
            IsUnique(sCallingSequence, _oStateClassSequences, oState.Name & oClass.Name)
        End Sub

        Private Sub addStateMachine()
            Dim oState As EA.Element
            Dim oConnector As EA.Connector
            Dim bStateMachineHeaderAdded As Boolean = False
            Dim oSupplierState As EA.Element = Nothing
            Dim oPopulateTransitions As New Collection
            Dim sDomainName As String = _oDomain.Name
            Dim oPigtails As New Collection
            Dim iClassStateMachineID As Integer
            Dim oStateNameUniqueBucket As New Collection

            Try
                iClassStateMachineID = getClassStateMachineID(_oEAElement)

                If _oDomain.States.Count > 0 Then
                    For Each oState In _oDomain.States
                        If oState.ParentID = iClassStateMachineID Then
                            _bActiveClass = True
                            Exit For
                        End If
                    Next

                    If _bActiveClass Then               ' if this class has a state machine
                        With _oSourceOutput
                            .AppendText("        machine" & vbCrLf)
                            .AppendText("            default transition CH                  # always use 'cannot happen' as the default" & vbCrLf & vbCrLf)

                            addNormalPigtailTransition(oPigtails, iClassStateMachineID)
                            If oPigtails.Count > 0 Then
                                For Each sPigtail As String In oPigtails
                                    Application.DoEvents()
                                    For Each oState In _oDomain.States
                                        Application.DoEvents()
                                        If oState.ParentID = iClassStateMachineID Then
                                            If oState.MetaType <> "Pseudostate" Then
                                                .AppendText(("	        transition  " & oState.Name & " - ").PadRight(100) & sPigtail & vbCrLf)
                                            End If
                                        End If
                                    Next
                                Next
                                .AppendText(vbCrLf)
                            End If

                            For Each oState In _oDomain.States
                                Application.DoEvents()
                                If oState.ParentID = iClassStateMachineID Then
                                    oPopulateTransitions = New Collection
                                    For Each oConnector In oState.Connectors
                                        Application.DoEvents()
                                        addNormalTransition(oConnector, oState, _oEAElement)
                                    Next
                                End If
                            Next

                            For Each oState In _oDomain.States
                                Application.DoEvents()
                                If oState.ParentID = iClassStateMachineID Then
                                    If oState.Subtype = EA_TYPE.INITIAL_STATE Then        ' if this state is the "meatball" initial state
                                        If oState.Connectors.Count = 1 Then
                                            Dim oOriginalState As EA.Element = _oDomain.States(CType(oState.Connectors.GetAt(0), EA.Connector).SupplierID.ToString)
                                            _sInitialState = oOriginalState.Name
                                        Else
                                            MsgBox("An initial state in the state model for class '" & _oEAElement.Name & "' has no transition out", MsgBoxStyle.Critical)
                                        End If
                                    End If

                                    If IsUnique(oState.Name, oStateNameUniqueBucket) Then
                                        addState(oState, _oEAElement)
                                    End If
                                End If
                            Next
                            .AppendText("        end   # machine" & vbCrLf)
                            .AppendText(vbCrLf)
                        End With
                    End If
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addNormalTransition(ByVal oConnector As EA.Connector, _
                                        ByVal oState As EA.Element, _
                                        ByVal oClass As EA.Element)
            Dim sArgumentList As String = ""
            Dim oClientState As EA.Element
            Dim sEvent As String
            Dim sTokens() As String
            Dim oSupplierState As EA.Element = Nothing
            Dim oErrorHandler As New sjmErrorHandler
            Dim oEVENT_ErrorHandler As New sjmErrorHandler

            Try
                With _oSourceOutput
                    oErrorHandler.SupplementalInformation = "Class: " & oClass.Name & ", State: " & oState.Name & ", Connector: " & oConnector.Name
                    If _oDomain.States.Contains(oConnector.ClientID.ToString) Then                      ' if "to" state is a normal state
                        oSupplierState = _oDomain.States(oConnector.ClientID.ToString)
                        If oSupplierState Is oState Then
                            If _oDomain.States.Contains(oConnector.SupplierID.ToString) Then
                                oClientState = _oDomain.States(oConnector.SupplierID.ToString)

                                sEvent = Regex.Replace(oConnector.TransitionEvent, "\s*\(", "(")
                                sTokens = Split(sEvent, ":")
                                If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
                                    sEvent = sTokens(1)
                                End If

                                If sEvent.Length > 0 Then
                                    Dim oMatches As MatchCollection = Regex.Matches(sEvent, "([^\(]+)\(([^()]+)\)")
                                    If oMatches.Count > 0 Then
                                        Dim oGroups As GroupCollection = oMatches(0).Groups
                                        If oGroups.Count > 2 Then
                                            sEvent = Regex.Replace(Trim(oGroups(1).ToString()), "\s+", " ")
                                            sArgumentList = Regex.Replace(Trim(oGroups(2).ToString()), "\s+", " ")
                                        End If
                                    End If
                                    recordStateCallingSequence(oClientState, sArgumentList, oClass)
                                    Dim sCanonicalEventName As String = canonicalEventName(sEvent)

                                    Dim sClientStateName As String = Canonical.CanonicalName(oClientState.Name)
                                    If oClientState.Subtype = EA_TYPE.EXIT_STATE Then  ' if this state is an ignore marker
                                        sClientStateName = "IG"
                                    End If

                                    If oSupplierState.Subtype <> EA_TYPE.ENTRY_STATE Then
                                        .AppendText("            transition " & Canonical.CanonicalName(oSupplierState.Name) & " - " & sCanonicalEventName & " -> " & sClientStateName & vbCrLf)
                                    End If

                                    If Not _oEventNames.Contains(sCanonicalEventName) Then
                                        _oEventNames.Add(sCanonicalEventName)
                                    End If

                                Else
                                    If (oSupplierState.Subtype = EA_TYPE.INITIAL_STATE) Then
                                        .AppendText("            initial state " & oClientState.Name & vbCrLf)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End With
            Catch ex As Exception
                oErrorHandler.Announce(ex)
            End Try
        End Sub

        Private Sub addNormalPigtailTransition(ByRef Pigtails As Collection, ByVal iClassStateMachineID As Integer)
            Dim oSupplierState As EA.Element = Nothing
            Dim oState As EA.Element
            Dim oConnector As EA.Connector
            Dim sEvent As String
            Dim sTokens() As String

            Try
                For Each oState In _oDomain.States
                    Application.DoEvents()
                    If oState.ParentID = iClassStateMachineID Then
                        If oState.Subtype = EA_TYPE.ENTRY_STATE Then       ' if this state is a pigtail-originating state
                            For Each oConnector In oState.Connectors
                                sEvent = canonicalEventName(oConnector)
                                sTokens = Split(sEvent, ":")
                                If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
                                    sEvent = sTokens(1)
                                End If

                                If sEvent.Length > 0 Then
                                    If sEvent.IndexOf(")") > 0 Then
                                        sTokens = Split(sEvent.Substring(0, sEvent.IndexOf(")")), "(")            ' peel off any parameter payload
                                        If sTokens.Length > 1 Then
                                            sEvent = sTokens(0)
                                        End If
                                    End If
                                    With Pigtails
                                        oSupplierState = _oDomain.States(oConnector.SupplierID.ToString)
                                        .Add(sEvent & " -> " & oSupplierState.Name)
                                    End With
                                End If
                            Next
                        End If
                    End If
                Next
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addState(ByVal oState As EA.Element, ByVal oClass As EA.Element)
            Dim sLines() As String
            Dim sLine As String
            Dim bQuiet As Boolean = False
            Dim bOmit As Boolean = False
            Dim sArgumentString As String = ""

            Try
                bQuiet = ElementIncludesStereotype(oState, "quiet")
                bOmit = ElementIncludesStereotype(oState, "omit")

                If (Not bOmit) And (oState.Name.Length > 0) Then
                    With _oSourceOutput
                        Select Case oState.Subtype
                            Case EA_TYPE.SYNCH_STATE, EA_TYPE.INITIAL_STATE, EA_TYPE.EXIT_STATE, EA_TYPE.ENTRY_STATE
                                ' do nothing with these pseudo states, just allow them without error

                            Case Else
                                sLines = Split(oState.Notes, vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("            # ________________________________________________________________ State   " & oClass.Name & ": " & oState.Name & " (v" & oState.Version & ")" & vbCrLf)

                                If oState.Subtype = EA_TYPE.FINAL_STATE Then
                                    .AppendText("            final state " & oState.Name & vbCrLf)
                                    .AppendText("            state " & oState.Name & "()  {}" & vbCrLf)
                                Else
                                    If _oStateClassSequences.Contains(oState.Name & oClass.Name) Then
                                        .AppendText("    " & _oStateClassSequences(oState.Name & oClass.Name))

                                        If bQuiet Then
                                            .AppendText("    static int quietCount = 0;    // announce just once, quiet mode" & vbCrLf)
                                        Else
                                            .AppendText("    static int quietCount = -1;" & vbCrLf)
                                        End If
                                        .AppendText("    BumpQuietCount( &quietCount );" & vbCrLf)
                                        .AppendText("    _SELF = self; " & vbCrLf)
                                        .AppendText("    ///////////////////////////////////// `" & oState.Name & " //////////////////////////////////////" & vbCrLf)

                                        For Each sLine In sLines
                                            .AppendText(vbLf & StripRichTextFormat(sLine))
                                        Next
                                        .AppendText(vbCrLf)
                                        .AppendText("}" & vbCrLf)
                                    Else
                                        .AppendText("           state " & oState.Name & "()  {}" & vbCrLf)
                                    End If
                                End If
                        End Select
                    End With
                Else
                    'debug.WriteLine("Omitting state (by stereotype 'omit'): " & oState.Name)
                End If

            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub cardinalityComment(ByVal oConnector As EA.Connector, ByRef sCardinalityComment As String, ByRef sOtherClassName As String, ByVal oClass As EA.Element)
            Dim sSupplierCardinality As String

            If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
                sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
                sSupplierCardinality = oConnector.ClientEnd.Cardinality
            Else
                sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
                sSupplierCardinality = oConnector.SupplierEnd.Cardinality
            End If

            If sOtherClassName = "" Or sSupplierCardinality = "" Then
                Throw New ApplicationException("One or both ends of relationship '" & oConnector.Name & "' has no multiplicity supplied")
            End If

            sSupplierCardinality = Canonical.CanonicalName(sSupplierCardinality)

            Select Case sSupplierCardinality
                Case "1", "0..1"
                    sCardinalityComment = "   ' " & buildRelationshipPhrase(oConnector, oClass)

                Case ""
                    sCardinalityComment = "   ' CARDINALITY ASSUMED: " & buildRelationshipPhrase(oConnector, oClass)

                Case "0..*", "1..*"
                    sCardinalityComment = "   ' " & buildRelationshipPhrase(oConnector, oClass)

                Case Else
                    sCardinalityComment = "   ' <unknown cardinality> " & sSupplierCardinality
            End Select
        End Sub

        Private Function canonicalDefaultValue(ByVal sDefaultValue As String, ByVal sType As String) As String
            Dim sReturnDefaultString As String = sDefaultValue

            If sReturnDefaultString.Length = 0 Then
                sReturnDefaultString = "0"
            End If

            Select Case sType.ToLower
                Case "int", "float", "double", "boolean", "long", "byte", "unsigned char"
                    ' do nothing, no adjustment needed

                Case "char", "string"
                    sReturnDefaultString = """" & sReturnDefaultString & """"

                Case "xevent"
                    sReturnDefaultString = "Nothing"
            End Select

            Return sReturnDefaultString
        End Function

        Private Function accumulateConnectors(ByVal oClass As EA.Element, Optional ByVal hUnique As Hashtable = Nothing, Optional ByRef oConnectors As Collection = Nothing)
            If oConnectors Is Nothing Then
                oConnectors = New Collection
                hUnique = New Hashtable
            End If

            If Not hUnique.Contains(oClass) Then
                hUnique.Add(oClass, oClass)
                For Each oConnector As EA.Connector In oClass.Connectors
                    oConnector.Notes = oClass.Name     ' borrowing this instrinsic property to carry the name of the class participating in the relationship
                    oConnectors.Add(oConnector)
                Next

                Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
                If (sTokens(0).Length > 0) Then
                    For Each sId As String In sTokens
                        Dim oParentClass As EA.Element = _oDomain.EAClass(sId)
                        accumulateConnectors(oParentClass, hUnique, oConnectors)
                    Next
                End If
            End If

            Return oConnectors
        End Function

        Private Function accumulateAttributes(ByVal oClass As EA.Element, Optional ByVal hUnique As Hashtable = Nothing, Optional ByRef oAttributes As Collection = Nothing)
            If oAttributes Is Nothing Then
                oAttributes = New Collection
                hUnique = New Hashtable
            End If

            If Not hUnique.Contains(oClass) Then
                hUnique.Add(oClass, oClass)
                For Each oAttribute As EA.Attribute In oClass.Attributes
                    oAttributes.Add(oAttribute.Name)
                Next

                Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
                If (sTokens(0).Length > 0) Then
                    For Each sId As String In sTokens
                        Dim oParentClass As EA.Element = _oDomain.EAClass(sId)
                        accumulateAttributes(oParentClass, hUnique, oAttributes)
                    Next
                End If
            End If

            Return oAttributes
        End Function

        Private Function assembleConstructorAttributes(ByVal oConstructorAttributes As Collection, ByVal oClass As EA.Element) As Boolean
            Static bRequired As Boolean = False

            For Each oAttribute As EA.Attribute In oClass.Attributes
                If AttributeIncludesStereotype(oAttribute, "constructor") Then
                    If AttributeIncludesStereotype(oAttribute, "required") Then
                        bRequired = True
                    End If
                    IsUnique(oAttribute, oConstructorAttributes, oAttribute.AttributeGUID)
                End If
            Next

            Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
            sTokens = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
            If (sTokens(0).Length > 0) Then
                For Each sParentClassId As String In sTokens
                    Dim oParentClass As EA.Element = _oDomain.EAClass(sParentClassId)
                    assembleConstructorAttributes(oConstructorAttributes, oParentClass)
                Next
            End If

            Return bRequired
        End Function

        Private Sub addPolymorphicEvents()
            Dim oPolymorphicEvents As New Collection

            For Each oMethod As EA.Method In _oEAElement.Methods
                If MethodIncludesStereotype(oMethod, "polymorphic") Then        ' "operations" tagged as "polymorphic" are actually polymorphic events -- was the best place I could find to add poly event specfication
                    oPolymorphicEvents.Add(oMethod.Name)
                End If
            Next

            If oPolymorphicEvents.Count > 0 Then
                With _oSourceOutput
                    .AppendText("")
                    .AppendText("        polymorphic event" + vbCrLf)
                    For Each sEventName As String In oPolymorphicEvents
                        .AppendText("            " + sEventName + vbCrLf)
                    Next
                    .AppendText("        end   # polymorphic event" & vbCrLf)
                    .AppendText(vbCrLf)
                End With
            End If
        End Sub

        Private Sub addSubtypes()
            Try
                If _oDomain.ParentChildren.Contains(_oEAElement.ElementID.ToString) Then
                    With _oSourceOutput
                        .AppendText("        subtype SUBTYPE reference" + vbCrLf)
                        _oChildrenCollection = _oDomain.ParentChildren(_oEAElement.ElementID.ToString)
                        For Each oChildClass As EA.Element In _oChildrenCollection
                            .AppendText("            " + oChildClass.Name + vbCrLf)
                        Next
                        .AppendText("        end  # subtype" + vbCrLf)
                        .AppendText(vbCrLf)
                    End With
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addAttributes()
            Dim sCommentText As String
            Dim sNotesString As String = ""
            Dim oAttribute As EA.Attribute
            Dim sCanonicalAttributeName As String
            Dim sCanonicalType As String
            Dim sOtherClassName As String = ""
            Dim sCardinalityComment As String = ""
            Dim sCollectionName As String = Replace(_oEAElement.Name & "s", "]s", "s]")

            Try
                With _oSourceOutput
                    For Each oAttribute In _oEAElement.Attributes
                        oAttribute.Type = CanonicalType(oAttribute.Type)
                        oAttribute.Name = Canonical.CanonicalName(oAttribute.Name)
                    Next

                    For Each oAttribute In _oEAElement.Attributes
                        If oAttribute.Notes.Length > 0 Then
                            sCommentText = "      ' " & oAttribute.Notes
                        Else
                            sCommentText = ""
                        End If
                    Next

                    Dim oConstructorAttributes As New Collection
                    Dim bRequired As Boolean = assembleConstructorAttributes(oConstructorAttributes, _oEAElement)

                    For Each oAttribute In _oEAElement.Attributes
                        Application.DoEvents()

                        Dim sVisibility As String = getAttributeVisibilitykeywork(oAttribute)

                        sCanonicalType = CanonicalType(oAttribute.Type)
                        sCanonicalAttributeName = Canonical.CanonicalName(oAttribute.Name)

                        If oAttribute.Notes.Length > 0 Then
                            For Each sLine As String In Split(oAttribute.Notes, vbCrLf)
                                .AppendText("        ## " & sLine & vbCrLf)
                            Next
                        End If

                        Dim sInitialValue As String = ""
                        If oAttribute.Default.Length > 0 Then
                            sInitialValue = " default {" & canonicalDefaultValue(oAttribute.Default, sCanonicalType) & "}"
                        End If
                        .AppendText("        attribute (" & sCanonicalType & " " & sCanonicalAttributeName & ")" & sInitialValue & vbCrLf)
                    Next

                    For Each oConnector As EA.Connector In _oEAElement.Connectors
                        addAssociation(oConnector, _oEAElement)
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addAssociation(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
            Dim sSupplierCardinality As String
            Dim sClientCardinality As String
            Dim sCardinalityComment As String = ""
            Dim sOtherClassName As String

            Try
                With _oSourceOutput
                    If oConnector.Type = "Association" Then
                        If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
                            sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
                            sClientCardinality = oConnector.SupplierEnd.Cardinality
                        Else
                            sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
                            sClientCardinality = oConnector.ClientEnd.Cardinality
                        End If

                        oConnector.Name = Canonical.CanonicalName(oConnector.Name)

                        Select Case sSupplierCardinality
                            Case "1", "0..1"
                                .AppendText("        reference " & oConnector.Name & " -> " & sOtherClassName & vbCrLf)

                            Case "0..*", "1..*"
                                .AppendText("        reference " & oConnector.Name & " -" & INSTANCE_POINTER_ARRAY_COUNT & ">> " & sOtherClassName & vbCrLf)

                            Case Else
                                If sSupplierCardinality.Length > 0 Then
                                    Throw New ApplicationException("Unknown cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
                                Else
                                    Throw New ApplicationException("No cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
                                End If
                        End Select
                    End If
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Function getAttributeVisibilitykeywork(ByVal oAttribute As EA.Attribute) As String
            Dim sVisibilityKeyword As String

            Select Case oAttribute.Visibility
                Case "Protected"
                    sVisibilityKeyword = "Protected"

                Case "Private"
                    sVisibilityKeyword = "Private"

                Case "Public"
                    sVisibilityKeyword = "Public"

                Case Else
                    Throw New ApplicationException("Unhandled visiblity case: " & oAttribute.Visibility)
            End Select
            Return sVisibilityKeyword
        End Function

        Private Sub printAttributeDescription(ByVal sAttributeName As String)
            _oSourceOutput.AppendText(("   print(""\n  " & sAttributeName & ": ").PadRight(COLUMN_WIDTH) & (" ""); print($self->{" & sAttributeName & "})").PadRight(COLUMN_WIDTH) & " if $self->{" & sAttributeName & "};" & vbCrLf)
        End Sub

        Private Function buildRelationshipPhrase(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element) As String
            Dim iClientClassId As Integer
            Dim iSupplierClassId As Integer
            Dim sPhrase As String = ""
            Dim oClientClass As EA.Element
            Dim oSupplierClass As EA.Element
            Dim sClientRole As String
            Dim sSupplierRole As String
            Dim sSupplierCardinality As String
            Dim sSupplierClassName As String
            Dim sClientClassName As String

            Try
                With oConnector
                    iClientClassId = .ClientID
                    iSupplierClassId = .SupplierID
                    sClientRole = .ClientEnd.Role
                    sSupplierRole = .SupplierEnd.Role
                    sSupplierCardinality = .SupplierEnd.Cardinality

                    With _oDomain
                        oClientClass = .EAClass(iClientClassId)
                        sClientClassName = oClientClass.Name
                        oSupplierClass = .EAClass(iSupplierClassId)
                        sSupplierClassName = oSupplierClass.Name
                    End With

                    If sClientClassName = oClass.Name Then
                        ' do nothing, perspective is already proper
                    Else
                        If sSupplierClassName = oClass.Name Then
                            iClientClassId = .SupplierID
                            iSupplierClassId = .ClientID
                            sClientRole = .SupplierEnd.Role
                            sSupplierRole = .ClientEnd.Role
                            sSupplierCardinality = .ClientEnd.Cardinality

                            With _oDomain
                                oClientClass = .EAClass(iClientClassId)
                                sClientClassName = oClientClass.Name
                                oSupplierClass = .EAClass(iSupplierClassId)
                                sSupplierClassName = oSupplierClass.Name
                            End With
                        Else
                            Throw New ApplicationException("PerspectiveClassName '" & oClass.Name & "'does not match either participant in relationship")
                        End If
                    End If
                End With

                Select Case oConnector.Type
                    Case "Generalization"
                        sPhrase = oClientClass.Name & " is a " & oSupplierClass.Name
                    Case "Association"
                        sPhrase = ""
                        Select Case sSupplierCardinality
                            Case "1"
                                sPhrase += oClientClass.Name & " """ & sClientRole & """ exactly one " & oSupplierClass.Name

                            Case "0..1"
                                sPhrase += oClientClass.Name & " """ & sClientRole & """ zero or one " & oSupplierClass.Name

                            Case "0..*"
                                sPhrase += oClientClass.Name & " """ & sClientRole & """ zero or more " & oSupplierClass.Name & "s"

                            Case "1..*"
                                sPhrase += oClientClass.Name & " """ & sClientRole & """ one or more " & oSupplierClass.Name & "s"

                            Case Else
                                sPhrase += "<unknown cardinality on '" & oClientClass.Name & "' side of relationship '" & oConnector.Name & "'"
                        End Select
                    Case Else
                        sPhrase = "<unknown connector type: " & oConnector.Type
                End Select
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try

            Return sPhrase
        End Function

    End Class
End Class

