
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO
Imports System.Xml.Linq

Public Class OutputLanguageCS_XML
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
    Private Shared ModelEnumerations As SortedDictionary(Of String, EA.Element)

    Public Shared oXMLBuilder As XMLBuilder 


    Public Sub CreateDomains(ByVal oRepository As EA.Repository, ByVal bIncludeDebug As Boolean) Implements IOutputLanguage.CreateDomains
        Try
            gStatusBox = New frmStatusBox
            gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

            ModelEnumerations = New SortedDictionary(Of String, EA.Element)
            '_ModelDataTypes = New Collection
            '_oRelationshipNames = New Collection
            '_oRelatesConnectorNamesC = New Collection
            '_oRelatesConnectorNamesH = New Collection
            'ModelEventNames = New Collection

            oXMLBuilder = New XMLBuilder("Model", "ModelRepository", True)
            oXMLBuilder.SetElement("Domains", oXMLBuilder.RootElement)
            oXMLBuilder.SetElement("Enumerations", oXMLBuilder.RootElement)

            Dim oPackagesList As New Collection
            For Each oPackage As EA.Package In oRepository.Models.GetAt(0).Packages
                recursePackage(oPackage, oPackagesList)
            Next

            If oPackagesList.Count = 0 Then
                MsgBox("No packages found with stereotype 'cs' so no compilation was done")
            Else
                For Each oFoundPackage As EA.Package In oPackagesList
                    createDomain(oRepository, oFoundPackage, bIncludeDebug)
                Next
            End If

            oXMLBuilder.Save("c:\x.xml")
            gStatusBox.FadeAway()
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Sub createDomain(ByVal oRepository As EA.Repository, ByVal oPackage As EA.Package, ByVal bIncludeDebug As Boolean)
        gStatusBox.Filename = oPackage.Name
        Dim oDomain As Domain = New Domain(oPackage, oRepository)      ' constructor does the work

        For Each oChildPackage As EA.Package In oPackage.Packages
            createDomain(oRepository, oChildPackage, bIncludeDebug)
        Next
    End Sub

    Private Sub recursePackage(ByVal oNextPackage As EA.Package, ByVal oPackages As Collection)
        For Each oPackage As EA.Package In oNextPackage.Packages
            If Not PackageIncludesStereotype(oPackage, "realized") Then
                oPackages.Add(oPackage)
            End If
            recursePackage(oPackage, oPackages)
        Next
    End Sub

    Protected Class Domain
        Private _oEADiagram As EA.Diagram
        Private _sName As String = ""
        Private _sDiagramNotes As String = ""
        Private _sDiagramVersion As String = ""
        Private _oPackage As EA.Package
        Private _IsRealized As Boolean
        Private _Triggers As Collection
        'Private _Enumerations As Collection
        Private _DataTypes As Collection
        Private _States As Collection
        Private _Notes As Collection
        Private _Boundarys As Collection
        Private _StateMachines As Collection
        Private _ObjectInstances As Collection
        Private _Interfaces As Collection
        Private _ElementById As Collection
        Private _InitialStates As Collection
        Private _FinalStates As Collection
        Private _IgnoreIndicatorStates As Collection
        Private _TestElements As Collection
        Private _ModelDataTypes As Collection
        Private _ClassById As Collection

        Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository)
            Try
                _sName = CanonicalName(oPackage.Name)
                _oEADiagram = oPackage.Diagrams.GetAt(0)
                _sDiagramNotes = _oEADiagram.Notes
                _sDiagramVersion = _oEADiagram.Version
                _oPackage = oPackage
                _IsRealized = PackageIncludesStereotype(_oPackage, "realized")

                _ObjectInstances = New Collection
                _TestElements = New Collection
                _Interfaces = New Collection
                _Triggers = New Collection
                '_Enumerations = New Collection
                _DataTypes = New Collection
                _States = New Collection
                _ElementById = New Collection
                _StateMachines = New Collection
                _InitialStates = New Collection
                _FinalStates = New Collection
                _IgnoreIndicatorStates = New Collection
                _DataTypes = New Collection
                _ClassById = New Collection
                _ModelDataTypes = New Collection

                For Each oElement As EA.Element In _oPackage.Elements
                    catalogElement(oElement)
                Next

                populateMetaModel(oPackage)
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try

        End Sub

        'Private Sub createEnumerationsFile(ByVal oRepository As EA.Repository)
        '    Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), "Enumerations")
        '    Dim oDataTypesFileH As OutputFile = New OutputFile(sOutputFilename + ".h", True)
        '    Dim oDataTypesFileC As OutputFile = New OutputFile(sOutputFilename + ".c", True)
        '    Dim oDataTypesFileCS As OutputFile = New OutputFile(sOutputFilename + ".cs", True)

        '    With oDataTypesFileCS
        '        .Add()
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//              File: " & sOutputFilename & ".cs")
        '        .Add("//")
        '        .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
        '        .Add("//")
        '        .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
        '        .Add("//")
        '        .Add("//________________________________________________________________________________")
        '        .Add("")
        '        .Add("using System.Collections;")
        '        .Add("")
        '        .Add("public class Enumerations")
        '        .Add("{")
        '        addEnumeratorsCS(oDataTypesFileCS)
        '        .Add("}")
        '        .Add("")
        '        .Add("public enum eEVENT")
        '        .Add("{")
        '        'For Each sEventName As String In ModelEventNames
        '        '    Dim sTokens As String() = sEventName.Split(",")
        '        '    .Add("    " & sTokens(0) & ",")
        '        'Next
        '        .Add("}")
        '        .Close()
        '    End With

        '    With oDataTypesFileC
        '        .Add()
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//              File: " & sOutputFilename & ".c")
        '        .Add("//")
        '        .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
        '        .Add("//")
        '        .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
        '        .Add("//")
        '        .Add("//________________________________________________________________________________")
        '        .Add("")
        '        .Add("")
        '        .Add("#include ""Enumerations.h""")
        '        .Add("")
        '        addEnumerators(oDataTypesFileC)
        '        .Close()
        '    End With

        '    With oDataTypesFileH
        '        .Add()
        '        .Add("#ifndef DATATYPES_H_")
        '        .Add("#define DATATYPES_H_")
        '        .Add()
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
        '        .Add("//________________________________________________________________________________")
        '        .Add("//")
        '        .Add("//              File: " & sOutputFilename & ".h")
        '        .Add("//")
        '        .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
        '        .Add("//")
        '        .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
        '        .Add("//")
        '        .Add("//________________________________________________________________________________")
        '        .Add("")
        '        .Add("")
        '        .Add("")
        '        .Add("")

        '        'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
        '        '    If oEnumeration.Attributes.Count > 0 Then
        '        '        oEnumeration.Name = CanonicalName(oEnumeration.Name)           ' massage all the enumeration names to be safe for the C compiler
        '        '    End If
        '        'Next

        '        'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
        '        '    If oEnumeration.Attributes.Count > 0 Then
        '        '        .Add("#define " + oEnumeration.Name + "_COUNT " + oEnumeration.Attributes.Count.ToString)
        '        '    End If
        '        'Next

        '        'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
        '        '    If oEnumeration.Attributes.Count > 0 Then
        '        '        .Add("")
        '        '        .Add("    typedef enum " + oEnumeration.Name)
        '        '        .Add("    {")
        '        '        Dim iEnumeratorCount As Integer = 0

        '        '        _SortedEnumeratorNames = New List(Of String)
        '        '        For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
        '        '            oEnumerator.Name = CanonicalName(oEnumerator.Name)
        '        '            Dim sComment As String = CleanNoteString(oEnumerator)
        '        '            If sComment.Length > 0 Then
        '        '                sComment = "// " & sComment
        '        '            End If
        '        '            _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
        '        '        Next
        '        '        _SortedEnumeratorNames.Sort()

        '        '        For Each sEnumeratorName As String In _SortedEnumeratorNames
        '        '            .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
        '        '            iEnumeratorCount += 1
        '        '        Next

        '        '        .Add("    } " + CanonicalName(oEnumeration.Name) + ";")
        '        '    Else
        '        '        If oEnumeration.Notes.Length > 0 Then
        '        '            MsgBox("Enumerations should be specified by creating 'attributes' for each element--it looks like you only put text in the Notes box: " & vbCrLf & vbCrLf & oEnumeration.Notes)
        '        '        End If
        '        '    End If
        '        'Next
        '        '.Add("")
        '        'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
        '        '    If oEnumeration.Attributes.Count > 0 Then
        '        '        .Add("    extern char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID);")
        '        '    End If
        '        'Next
        '        '.Add("")
        '        '.Add("")


        '        For Each oDataType As EA.Element In _ModelDataTypes
        '            If oDataType.Notes.Length > 0 Then
        '                .Add("")
        '                .Add("    typedef struct " + CanonicalName(oDataType.Name) + "_s")
        '                .Add("    {")
        '                .Add(oDataType.Notes + "    } " + CanonicalName(oDataType.Name) + ";")
        '            End If
        '        Next

        '        .Add("#endif")
        '        .Close()
        '    End With
        'End Sub

        'Private Sub addEnumeratorsCS(ByVal oEnumerationsFileC As OutputFile)
        '    Dim oEnumeration As EA.Element

        '    With oEnumerationsFileC
        '        For Each oEnumeration In OutputLanguageCS_XML.ModelEnumerations.Values
        '            If oEnumeration.Attributes.Count > 0 Then
        '                .Add("")
        '                .Add("    public enum " + CanonicalName(oEnumeration.Name))
        '                .Add("    {")
        '                Dim iEnumeratorCount As Integer = 0
        '                _SortedEnumeratorNames = New List(Of String)
        '                Dim NoteStrings As List(Of String) = New List(Of String)
        '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
        '                    oEnumerator.Name = CanonicalName(oEnumerator.Name)
        '                    Dim sComment As String = CleanNoteString(oEnumerator)
        '                    If sComment.Length > 0 Then
        '                        sComment = "// " & sComment
        '                    End If
        '                    _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
        '                Next
        '                _SortedEnumeratorNames.Sort()

        '                For Each sEnumeratorName As String In _SortedEnumeratorNames
        '                    .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
        '                    iEnumeratorCount += 1
        '                Next
        '                .Add("    }")
        '            End If
        '        Next

        '        .Add("")
        '        .Add("")
        '        For Each oEnumeration In _ModelEnumerations.Values
        '            If oEnumeration.Attributes.Count > 0 Then
        '                .Add("    static private Hashtable " + oEnumeration.Name + "_Descriptions;                                                  ")
        '                .Add("    static public string Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID)                                           ")
        '                .Add("    {                                                                                                                 ")
        '                .Add("        string sDescription = ""illegal array index received = "" + ID.ToString() + "" for array '" + oEnumeration.Name + "'"";     ")
        '                .Add("        if (" + oEnumeration.Name + "_Descriptions == null)                                                           ")
        '                .Add("        {                                                                                                             ")
        '                .Add("            " + oEnumeration.Name + "_Descriptions = new Hashtable();                                                 ")
        '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
        '                    .Add("            " + oEnumeration.Name + "_Descriptions.Add((long)" + oEnumeration.Name + "." + oEnumeration.Name + "_" + CanonicalName(oEnumerator.Name) + ", """ + CleanNoteString(oEnumerator) + """); ")
        '                Next
        '                .Add("        }                                                                                                             ")
        '                .Add("                                                                                                                      ")
        '                .Add("        if(" + oEnumeration.Name + "_Descriptions.Contains((long)ID))                                                 ")
        '                .Add("        {                                                                                                             ")
        '                .Add("           sDescription = (string)" + oEnumeration.Name + "_Descriptions[(long)ID];                                   ")
        '                .Add("        }                                                                                                             ")
        '                .Add("                                                                                                                      ")
        '                .Add("                                                                                                                      ")
        '                .Add("        return sDescription;                                                                                          ")
        '                .Add("    }                                                                                                                 ")
        '                .Add(vbCrLf)
        '            End If
        '        Next
        '        .Add("")
        '        .Add("")
        '    End With
        'End Sub

        'Private Sub addEnumerators(ByVal oDataTypesFileC As OutputFile)
        '    Dim oEnumeration As EA.Element

        '    With oDataTypesFileC
        '        For Each oEnumeration In _ModelEnumerations.Values
        '            If oEnumeration.Attributes.Count > 0 Then
        '                .Add("    static char* " + oEnumeration.Name + "_Descriptions[" + oEnumeration.Attributes.Count.ToString + "] = ")
        '                .Add("    {")
        '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
        '                    .Add("        """ + CleanNoteString(oEnumerator) + " "",")
        '                Next
        '                .Add("    };")
        '                .Add(vbCrLf)
        '            End If
        '        Next
        '        .Add("")
        '        .Add("")
        '        For Each oEnumeration In _ModelEnumerations.Values
        '            If oEnumeration.Attributes.Count > 0 Then
        '                'Dim sFirstEnumeratorName As String = ""

        '                'For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
        '                '    sFirstEnumeratorName = oEnumerator.Name
        '                '    Exit For
        '                'Next

        '                .Add("    char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID)")
        '                .Add("    {")
        '                .Add("        return " + oEnumeration.Name + "_Descriptions[ID];")
        '                .Add("    };")
        '                .Add(vbCrLf)
        '            End If
        '        Next
        '        .Add("")
        '        .Add("")
        '    End With
        'End Sub

        'Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef sFileText As String, ByVal oDomain As Domain)
        '    Dim sDomainName As String = oDomain._sName

        '    If sFileText.Length > 0 Then
        '        OutputFile.ClearFilesCreated()
        '        Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
        '        With oOutputFile
        '            .Add("// ________________________________________________________________________________")
        '            .Add("// ")
        '            .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
        '            .Add("// ________________________________________________________________________________")
        '            .Add("// ")
        '            .Add("//               File: " & sOutputFilename)
        '            .Add("// ")
        '            .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
        '            .Add("// ")
        '            .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
        '            .Add("// ")
        '            .Add("// ________________________________________________________________________________")
        '            .Add("// ")
        '            .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
        '            .Add("// ________________________________________________________________________________")
        '            .Add("")
        '            .Add("")
        '            .Add("using System;")
        '            .Add("using System.IO;")
        '            .Add("using System.Diagnostics;")
        '            .Add("using System.Windows.Forms;")
        '            .Add("using System.Collections.Generic;")
        '            .Add("using System.IO.Ports;")
        '            .Add("using System.Text.RegularExpressions;")
        '            .Add("using System.Text;")
        '            .Add("")
        '            .Add("namespace " & oDomain._sName)
        '            .Add("{")
        '            addInstanceCollections(oOutputFile, oDomain)
        '            .Add(sFileText)
        '            addEventClasses(oOutputFile)
        '            .Add("}")
        '        End With
        '        oOutputFile.Close()
        '    End If
        'End Sub

        Private Sub populateMetaModel(ByVal oPackage As EA.Package)
            Try
                Dim xDomains = OutputLanguageCS_XML.oXMLBuilder.RootElement.SelectSingleNode("Domains")
                Dim xEnumerations = OutputLanguageCS_XML.oXMLBuilder.RootElement.SelectSingleNode("Enumerations")
                With OutputLanguageCS_XML.oXMLBuilder
                    Dim xDomain = .SetElement("Domain", xDomains, "name", oPackage.Name)
                    Dim xClasses = .SetElement("Classes", xDomain)

                    If PackageIncludesStereotype(oPackage, "realized") Then
                        ' do nothing with a realized domain
                    Else
                        createClassNodes(oPackage, xClasses)
                        createEnumerationNodes(oPackage, xEnumerations)
                    End If
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createEnumerationNodes(ByVal oPackage As EA.Package, ByVal xEnumerations As XmlElement)
            Try
                With OutputLanguageCS_XML.oXMLBuilder
                    For Each oEnumeration As EA.Element In OutputLanguageCS_XML.ModelEnumerations.Values
                        Dim xEnumeration = .SetElement("Enumeration", xEnumerations, "name", oEnumeration.Name)
                        For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                            .SetElement("Enumerator", xEnumeration,
                                        "name", CanonicalName(oEnumerator.Name),
                                        "description", oEnumerator.Notes)
                        Next
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createClassNodes(ByVal oPackage As EA.Package, ByVal xClasses As XmlElement)
            Try
                With OutputLanguageCS_XML.oXMLBuilder
                    For Each oElement As EA.Element In _ClassById
                        If (oElement.MetaType = "Class") Then
                            If oElement.PackageID = oPackage.PackageID Then
                                Dim xClass = .SetElement("Class", xClasses, "name", CanonicalClassName(oElement.Name), "elementID", oElement.ElementID)
                                Dim xAttributes = .SetElement("Attributes", xClass)
                                For Each oAttribute As EA.Attribute In oElement.Attributes
                                    Dim sVisibility As String = getAttributeVisibilitykeywork(oAttribute)
                                    Dim sCanonicalType = CanonicalType(oAttribute.Type)
                                    Dim sCanonicalAttributeName = CanonicalName(oAttribute.Name)
                                    Dim sDescription = oAttribute.Notes
                                    .SetElement("Attribute", xAttributes,
                                                "dataType", sCanonicalType,
                                                "name", sCanonicalAttributeName,
                                                "description", sDescription,
                                                "visibility", sVisibility)
                                Next
                                createStateMachine(xClass, oElement)
                            End If
                        End If
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createStateMachine(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            Try
                With OutputLanguageCS_XML.oXMLBuilder
                    Dim xStates = .SetElement("States", xClass)
                    Dim xTransitions = .SetElement("Transitions", xClass)
                    Dim xEvents = .SetElement("Events", xClass)

                    For Each oStateMachineElement As EA.Element In _StateMachines
                        If oClass.ElementID = oStateMachineElement.ParentID Then
                            Dim iStateMachineID = oStateMachineElement.ElementID

                            For Each oState As EA.Element In _States
                                If oState.ParentID = iStateMachineID Then
                                    Dim iElementId = oState.ElementID
                                    Dim bIsQuiet = ElementIncludesStereotype(oState, "quiet")
                                    .SetElement("State", xStates,
                                                "isQuiet", bIsQuiet.ToString().ToLower,
                                                "name", oState.Name,
                                                "elementID", iElementId)

                                    For Each oConnector As EA.Connector In oState.Connectors
                                        Dim xExistingConnector = xTransitions.SelectSingleNode("Transition [@elementID = " + oConnector.ConnectorID.ToString() + "]")
                                        If xExistingConnector Is Nothing Then
                                            Dim xTransition = .SetElement("Transition", xTransitions,
                                                                          "elementID", oConnector.ConnectorID.ToString(),
                                                                          "eventElementID", oConnector.TransitionEvent)
                                            Dim xSourceSideState = .SetElement("SourceSideState", xTransition,
                                                                               "elementID", oConnector.ClientID.ToString)
                                            Dim xTargetSideState = .SetElement("TargetSideState", xTransition,
                                                                               "elementID", oConnector.SupplierID.ToString)
                                            addEvent(oConnector, xEvents)
                                        End If
                                    Next
                                End If
                            Next
                        End If
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addEvent(ByVal oConnector As EA.Connector, ByVal xEvents As XmlElement)
            Dim xEvent As XmlElement
            Try
                With OutputLanguageCS_XML.oXMLBuilder
                    If Not _Triggers.Contains(canonicalEventName(oConnector.TransitionEvent)) Then
                        If oConnector.TransitionEvent.Length > 0 Then
                            MessageBox.Show("Event '" + oConnector.TransitionEvent + "' not found" + vbCrLf + "(does it only appear in a 'specifcation' field?)", "Unkown Event", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        End If
                    Else
                        Debug.Assert(Not oConnector.TransitionEvent.Contains("Go"))
                        Dim sEvent = Regex.Replace(oConnector.TransitionEvent, "\s*\(", "(")
                        If sEvent.Length > 0 Then
                            Dim oMatches As MatchCollection = Regex.Matches(sEvent, "([^\(]+)\(([^()]+)\)")
                            If oMatches.Count = 0 Then
                                xEvent = .SetElement("Event", xEvents, "name", canonicalEventName(sEvent),
                                                     "transitionEventString", oConnector.TransitionEvent)
                            Else
                                Dim oGroups As GroupCollection = oMatches(0).Groups
                                Dim sEventName As String = Regex.Replace(Trim(oGroups(1).ToString()), "\s+", " ")
                                xEvent = .SetElement("Event", xEvents, "name", canonicalEventName(sEventName),
                                                     "transitionEventString", oConnector.TransitionEvent)
                                If oGroups.Count > 2 Then
                                    Dim sArgumentList = Regex.Replace(Trim(oGroups(2).ToString()), "[ ,]+", " ")
                                    Dim sArgumentTokens() = Regex.Split(sArgumentList, " ")
                                    Dim i As Integer
                                    For i = 0 To sArgumentTokens.Length - 1 Step 2
                                        .SetElement("Parameter", xEvent, "dataType", sArgumentTokens(i), "name", sArgumentTokens(i + 1))
                                    Next
                                End If
                            End If
                        End If
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

        Public Shared Function CanonicalType(ByVal sType As String) As String
            sType = sType.Trim
            Dim sReturnTypeString As String = sType

            If sType.Length > 0 Then
                Select Case sType.ToLower
                    Case "boolean", "bool"
                        sReturnTypeString = "bool"

                    Case "void"
                        sReturnTypeString = "void"

                    Case "unsigned long"
                        sReturnTypeString = "long"

                    Case "byte", "unsigned char"
                        sReturnTypeString = "byte"

                    Case "int"
                        sReturnTypeString = "int"

                    Case "char"
                        sReturnTypeString = "string"

                    Case "float", "double"
                        sReturnTypeString = "float"

                    Case "string", "char*"
                        sReturnTypeString = "string"

                End Select
            End If

            Return CanonicalName(sReturnTypeString)
        End Function

        Private Sub catalogElement(ByVal oElement As EA.Element)
            Try
                Application.DoEvents()

                Dim oErrorHandler As New sjmErrorHandler()
                oErrorHandler.SupplementalInformation = oElement.Name

                oElement.Name = CanonicalClassName(oElement.Name)           ' establish safe names right off the bat (rather than sprinkling everywhere)
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
                        '_Enumerations.Add(oElement, oElement.ElementID)
                        OutputLanguageCS_XML.ModelEnumerations.Add(oElement.Name, oElement)

                    Case "DataType"
                        _DataTypes.Add(oElement, oElement.ElementID)
                        _ModelDataTypes.Add(oElement, oElement.ElementID)

                    Case "Class"
                        oElement.Name = CanonicalClassName(oElement.Name)
                        _ClassById.Add(oElement, oElement.ElementID)

                    Case "Interface"
                        oElement.Name = CanonicalClassName(oElement.Name)
                        _Interfaces.Add(oElement, oElement.ElementID)

                    Case "State"
                        oElement.Name = CanonicalName(oElement.Name)
                        _States.Add(oElement, oElement.ElementID)

                    Case "Note", "Text"
                        ' do nothing with these, just allow them without complaint

                    Case Else
                        Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
                End Select

                For Each oSubElement As EA.Element In oElement.Elements
                    catalogElement(oSubElement)                      ' recurse down the element tree
                Next
            Catch ex As Exception
            End Try
        End Sub


    End Class






















    ''Private Const DEFAULT_INSTANCE_ALLOCATION As Integer = 14
    ''Private Const INSTANCE_POINTER_ARRAY_COUNT As Integer = DEFAULT_INSTANCE_ALLOCATION                 ' specifies the number of slots allocated to the array of pointers used to formalize x:M relationships

    'Private Shared _ModelEnumerations As SortedDictionary(Of String, EA.Element)             ' a collection of *all* enumerations found in the model (using a hashtable to be sure name-order is always the same)
    'Private Shared _SortedEnumeratorNames As List(Of String)
    'Private Shared _ModelDataTypes As Collection                    ' a collection of *all* data types found in the model
    'Private Shared _oRelationshipNames As Collection
    'Private Shared _iPackageCount As Integer = 0
    'Private Shared _oRelatesConnectorNamesC As Collection
    'Private Shared _oRelatesConnectorNamesH As Collection

    'Public Shared ModelEventNames As Collection
    'Public Shared DomainEventNames As Collection





























    'Public Sub xCreateDomains(ByVal oRepository As EA.Repository, ByVal bIncludeDebug As Boolean) 'Implements IOutputLanguage.CreateDomains
    '    Try
    '        gStatusBox = New frmStatusBox
    '        gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

    '        _ModelEnumerations = New SortedDictionary(Of String, EA.Element)
    '        _ModelDataTypes = New Collection
    '        _oRelationshipNames = New Collection
    '        _oRelatesConnectorNamesC = New Collection
    '        _oRelatesConnectorNamesH = New Collection
    '        ModelEventNames = New Collection

    '        Dim oPackagesList As New Collection

    '        For Each oPackage As EA.Package In oRepository.Models.GetAt(0).Packages
    '            recursePackage(oPackage, oPackagesList)
    '        Next

    '        If _iPackageCount = 0 Then
    '            MsgBox("No packages found with stereotype 'cs' so no compilation was done")
    '        Else
    '            For Each oFoundPackage As EA.Package In oPackagesList
    '                createDomain(oRepository, oFoundPackage, bIncludeDebug)
    '            Next
    '        End If

    '        createEnumerationsFile(oRepository)

    '        gStatusBox.FadeAway()
    '    Catch ex As Exception
    '        Dim oErrorHandler As New sjmErrorHandler(ex)
    '    End Try
    'End Sub



    'Public Shared Function CanonicalType(ByVal sType As String) As String
    '    sType = sType.Trim
    '    Dim sReturnTypeString As String = sType

    '    If sType.Length > 0 Then
    '        Select Case sType.ToLower
    '            Case "boolean", "bool"
    '                sReturnTypeString = "bool"

    '            Case "void"
    '                sReturnTypeString = "void"

    '            Case "unsigned long"
    '                sReturnTypeString = "long"

    '            Case "byte", "unsigned char"
    '                sReturnTypeString = "byte"

    '            Case "int"
    '                sReturnTypeString = "int"

    '            Case "char"
    '                sReturnTypeString = "string"

    '            Case "float", "double"
    '                sReturnTypeString = "float"

    '            Case "string", "char*"
    '                sReturnTypeString = "string"

    '        End Select
    '    End If



    'Public Shared Function CanonicalType(ByVal sType As String) As String
    '    sType = sType.Trim
    '    Dim sReturnTypeString As String = sType

    '    If sType.Length > 0 Then
    '        Select Case sType.ToLower
    '            Case "boolean", "bool"
    '                sReturnTypeString = "bool"

    '            Case "void"
    '                sReturnTypeString = "void"

    '            Case "unsigned long"
    '                sReturnTypeString = "long"

    '            Case "byte", "unsigned char"
    '                sReturnTypeString = "byte"

    '            Case "int"
    '                sReturnTypeString = "int"

    '            Case "char"
    '                sReturnTypeString = "string"

    '            Case "float", "double"
    '                sReturnTypeString = "float"

    '            Case "string", "char*"
    '                sReturnTypeString = "string"

    '        End Select
    '    End If

    '    Return CanonicalName(sReturnTypeString)
    'End Function

    ''Private Sub recursePackage(ByVal oNextPackage As EA.Package, ByVal oPackages As Collection)
    ''    For Each oPackage As EA.Package In oNextPackage.Packages
    ''        If PackageIncludesStereotype(oPackage, "cs") Then
    ''            _iPackageCount += 1
    ''            oPackages.Add(oPackage)
    ''        End If
    ''        recursePackage(oPackage, oPackages)
    ''    Next
    ''End Sub

    'Private Sub createEnumerationsFile(ByVal oRepository As EA.Repository)
    '    Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), "Enumerations")
    '    Dim oDataTypesFileH As OutputFile = New OutputFile(sOutputFilename + ".h", True)
    '    Dim oDataTypesFileC As OutputFile = New OutputFile(sOutputFilename + ".c", True)
    '    Dim oDataTypesFileCS As OutputFile = New OutputFile(sOutputFilename + ".cs", True)

    '    With oDataTypesFileCS
    '        .Add()
    '        .Add("//________________________________________________________________________________")
    '        .Add("//")
    '        .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
    '        .Add("//________________________________________________________________________________")
    '        .Add("//")
    '        .Add("//              File: " & sOutputFilename & ".cs")
    '        .Add("//")
    '        .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
    '        .Add("//")
    '        .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
    '        .Add("//")
    '        .Add("//________________________________________________________________________________")
    '        .Add("")
    '        .Add("using System.Collections;")
    '        .Add("")
    '        .Add("public class Enumerations")
    '        .Add("{")
    '        addEnumeratorsCS(oDataTypesFileCS)
    '        .Add("}")
    '        .Add("")
    '        .Add("public enum eEVENT")
    '        .Add("{")
    '        For Each sEventName As String In ModelEventNames
    '            Dim sTokens As String() = sEventName.Split(",")
    '            .Add("    " & sTokens(0) & ",")
    '        Next
    '        .Add("}")
    '        .Close()
    '    End With

    '    ''With oDataTypesFileC
    '    ''    .Add()
    '    ''    .Add("//________________________________________________________________________________")
    '    ''    .Add("//")
    '    ''    .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
    '    ''    .Add("//________________________________________________________________________________")
    '    ''    .Add("//")
    '    ''    .Add("//              File: " & sOutputFilename & ".c")
    '    ''    .Add("//")
    '    ''    .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
    '    ''    .Add("//")
    '    ''    .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
    '    ''    .Add("//")
    '    ''    .Add("//________________________________________________________________________________")
    '    ''    .Add("")
    '    ''    .Add("")
    '    ''    .Add("#include ""Enumerations.h""")
    '    ''    .Add("")
    '    ''    addEnumerators(oDataTypesFileC)
    '    ''    .Close()
    '    ''End With

    '    'With oDataTypesFileH
    '    '    .Add()
    '    '    .Add("#ifndef DATATYPES_H_")
    '    '    .Add("#define DATATYPES_H_")
    '    '    .Add()
    '    '    .Add("//________________________________________________________________________________")
    '    '    .Add("//")
    '    '    .Add("//         THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
    '    '    .Add("//________________________________________________________________________________")
    '    '    .Add("//")
    '    '    .Add("//              File: " & sOutputFilename & ".h")
    '    '    .Add("//")
    '    '    .Add("//        Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
    '    '    .Add("//")
    '    '    .Add("//         Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
    '    '    .Add("//")
    '    '    .Add("//________________________________________________________________________________")
    '    '    .Add("")
    '    '    .Add("")
    '    '    .Add("")
    '    '    .Add("")

    '    'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
    '    '    If oEnumeration.Attributes.Count > 0 Then
    '    '        oEnumeration.Name = CanonicalName(oEnumeration.Name)           ' massage all the enumeration names to be safe for the C compiler
    '    '    End If
    '    'Next

    '    ''For Each oEnumeration As EA.Element In _ModelEnumerations.Values
    '    ''    If oEnumeration.Attributes.Count > 0 Then
    '    ''        .Add("#define " + oEnumeration.Name + "_COUNT " + oEnumeration.Attributes.Count.ToString)
    '    ''    End If
    '    ''Next

    '    'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
    '    '    If oEnumeration.Attributes.Count > 0 Then
    '    '        .Add("")
    '    '        .Add("    typedef enum " + oEnumeration.Name)
    '    '        .Add("    {")
    '    '        Dim iEnumeratorCount As Integer = 0

    '    '        _SortedEnumeratorNames = New List(Of String)
    '    '        For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
    '    '            oEnumerator.Name = CanonicalName(oEnumerator.Name)
    '    '            Dim sComment As String = CleanNoteString(oEnumerator)
    '    '            If sComment.Length > 0 Then
    '    '                sComment = "// " & sComment
    '    '            End If
    '    '            _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
    '    '        Next
    '    '        _SortedEnumeratorNames.Sort()

    '    '        For Each sEnumeratorName As String In _SortedEnumeratorNames
    '    '            .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
    '    '            iEnumeratorCount += 1
    '    '        Next

    '    '        .Add("    } " + CanonicalName(oEnumeration.Name) + ";")
    '    '    Else
    '    '        If oEnumeration.Notes.Length > 0 Then
    '    '            MsgBox("Enumerations should be specified by creating 'attributes' for each element--it looks like you only put text in the Notes box: " & vbCrLf & vbCrLf & oEnumeration.Notes)
    '    '        End If
    '    '    End If
    '    'Next
    '    '.Add("")
    '    'For Each oEnumeration As EA.Element In _ModelEnumerations.Values
    '    '    If oEnumeration.Attributes.Count > 0 Then
    '    '        .Add("    extern char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID);")
    '    '    End If
    '    'Next
    '    '.Add("")
    '    '.Add("")


    '    'For Each oDataType As EA.Element In _ModelDataTypes
    '    '    If oDataType.Notes.Length > 0 Then
    '    '        .Add("")
    '    '        .Add("    typedef struct " + CanonicalName(oDataType.Name) + "_s")
    '    '        .Add("    {")
    '    '        .Add(oDataType.Notes + "    } " + CanonicalName(oDataType.Name) + ";")
    '    '    End If
    '    'Next

    '    '.Add("#endif")
    '    '.Close()
    '    'End With
    'End Sub

    'Private Sub addEnumeratorsCS(ByVal oEnumerationsFileC As OutputFile)
    '    Dim oEnumeration As EA.Element

    '    With oEnumerationsFileC
    '        For Each oEnumeration In _ModelEnumerations.Values
    '            If oEnumeration.Attributes.Count > 0 Then
    '                .Add("")
    '                .Add("    public enum " + CanonicalName(oEnumeration.Name))
    '                .Add("    {")
    '                Dim iEnumeratorCount As Integer = 0
    '                _SortedEnumeratorNames = New List(Of String)
    '                Dim NoteStrings As List(Of String) = New List(Of String)
    '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
    '                    oEnumerator.Name = CanonicalName(oEnumerator.Name)
    '                    Dim sComment As String = CleanNoteString(oEnumerator)
    '                    If sComment.Length > 0 Then
    '                        sComment = "// " & sComment
    '                    End If
    '                    _SortedEnumeratorNames.Add("        " + oEnumeration.Name + "_" + (oEnumerator.Name + " = ZZZ, ").PadRight(70) + sComment)    ' leave a ZZZ marker for the ordinal value after sorting
    '                Next
    '                _SortedEnumeratorNames.Sort()

    '                For Each sEnumeratorName As String In _SortedEnumeratorNames
    '                    .Add(sEnumeratorName.Replace("ZZZ", iEnumeratorCount.ToString))     ' tuck the ordinal value into the pre-built string
    '                    iEnumeratorCount += 1
    '                Next
    '                .Add("    }")
    '            End If
    '        Next

    '        .Add("")
    '        .Add("")
    '        For Each oEnumeration In _ModelEnumerations.Values
    '            If oEnumeration.Attributes.Count > 0 Then
    '                .Add("    static private Hashtable " + oEnumeration.Name + "_Descriptions;                                                  ")
    '                .Add("    static public string Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID)                                           ")
    '                .Add("    {                                                                                                                 ")
    '                .Add("        string sDescription = ""illegal array index received = "" + ID.ToString() + "" for array '" + oEnumeration.Name + "'"";     ")
    '                .Add("        if (" + oEnumeration.Name + "_Descriptions == null)                                                           ")
    '                .Add("        {                                                                                                             ")
    '                .Add("            " + oEnumeration.Name + "_Descriptions = new Hashtable();                                                 ")
    '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
    '                    .Add("            " + oEnumeration.Name + "_Descriptions.Add((long)" + oEnumeration.Name + "." + oEnumeration.Name + "_" + CanonicalName(oEnumerator.Name) + ", """ + CleanNoteString(oEnumerator) + """); ")
    '                Next
    '                .Add("        }                                                                                                             ")
    '                .Add("                                                                                                                      ")
    '                .Add("        if(" + oEnumeration.Name + "_Descriptions.Contains((long)ID))                                                 ")
    '                .Add("        {                                                                                                             ")
    '                .Add("           sDescription = (string)" + oEnumeration.Name + "_Descriptions[(long)ID];                                   ")
    '                .Add("        }                                                                                                             ")
    '                .Add("                                                                                                                      ")
    '                .Add("                                                                                                                      ")
    '                .Add("        return sDescription;                                                                                          ")
    '                .Add("    }                                                                                                                 ")
    '                .Add(vbCrLf)
    '            End If
    '        Next
    '        .Add("")
    '        .Add("")
    '    End With
    'End Sub

    'Private Sub addEnumerators(ByVal oDataTypesFileC As OutputFile)
    '    Dim oEnumeration As EA.Element

    '    With oDataTypesFileC
    '        For Each oEnumeration In _ModelEnumerations.Values
    '            If oEnumeration.Attributes.Count > 0 Then
    '                .Add("    static char* " + oEnumeration.Name + "_Descriptions[" + oEnumeration.Attributes.Count.ToString + "] = ")
    '                .Add("    {")
    '                For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
    '                    .Add("        """ + CleanNoteString(oEnumerator) + " "",")
    '                Next
    '                .Add("    };")
    '                .Add(vbCrLf)
    '            End If
    '        Next
    '        .Add("")
    '        .Add("")
    '        For Each oEnumeration In _ModelEnumerations.Values
    '            If oEnumeration.Attributes.Count > 0 Then
    '                'Dim sFirstEnumeratorName As String = ""

    '                'For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
    '                '    sFirstEnumeratorName = oEnumerator.Name
    '                '    Exit For
    '                'Next

    '                .Add("    char* Get_" + oEnumeration.Name + "_Description(" + oEnumeration.Name + " ID)")
    '                .Add("    {")
    '                .Add("        return " + oEnumeration.Name + "_Descriptions[ID];")
    '                .Add("    };")
    '                .Add(vbCrLf)
    '            End If
    '        Next
    '        .Add("")
    '        .Add("")
    '    End With
    'End Sub

    'Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef sFileText As String, ByVal oDomain As Domain)
    '    Dim sDomainName As String = oDomain.Name

    '    If sFileText.Length > 0 Then
    '        OutputFile.ClearFilesCreated()
    '        Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
    '        With oOutputFile
    '            .Add("// ________________________________________________________________________________")
    '            .Add("// ")
    '            .Add("//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
    '            .Add("// ________________________________________________________________________________")
    '            .Add("// ")
    '            .Add("//               File: " & sOutputFilename)
    '            .Add("// ")
    '            .Add("//         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
    '            .Add("// ")
    '            .Add("//          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
    '            .Add("// ")
    '            .Add("// ________________________________________________________________________________")
    '            .Add("// ")
    '            .Add("//           Copyright © 2011,  ArrayPower Inc.   All rights reserved.")
    '            .Add("// ________________________________________________________________________________")
    '            .Add("")
    '            .Add("")
    '            .Add("using System;")
    '            .Add("using System.IO;")
    '            .Add("using System.Diagnostics;")
    '            .Add("using System.Windows.Forms;")
    '            .Add("using System.Collections.Generic;")
    '            .Add("using System.IO.Ports;")
    '            .Add("using System.Text.RegularExpressions;")
    '            .Add("using System.Text;")
    '            .Add("")
    '            .Add("namespace " & oDomain.Name)
    '            .Add("{")
    '            addInstanceCollections(oOutputFile, oDomain)
    '            .Add(sFileText)
    '            addEventClasses(oOutputFile)
    '            .Add("}")
    '        End With
    '        oOutputFile.Close()
    '    End If
    'End Sub

    'Private Sub addInstanceCollections(ByVal oOutputFile As OutputFile, ByVal oDomain As Domain)
    '    With oOutputFile
    '        .Add("    public class " & oDomain.Name)
    '        .Add("    {")
    '        For Each oEAClass As EA.Element In oDomain.ClassByID
    '            .Add("        public static List<" & oEAClass.Name & "> " & oEAClass.Name & "s { get; set; }")
    '        Next
    '        .Add("    }")
    '        .Add("")
    '    End With
    'End Sub

    'Private Sub addEventClasses(ByVal oOutputFile As OutputFile)
    '    For Each sEventString As String In DomainEventNames
    '        sEventString = Regex.Replace(sEventString, "[ ]+", " ")
    '        Dim sTokens As String() = sEventString.Split(",")
    '        Dim sArgumentString As String = ""
    '        Dim sArgumentNamesOnlyString As String = ""
    '        Dim sTypeArgPairDelimiter As String = ""
    '        Dim sArgOnlyDelimiter As String = ""
    '        Dim sEventName As String = sTokens(0)

    '        With oOutputFile
    '            .Add("public class " & sEventName & " : ZEvent")
    '            .Add("{")

    '            For index As Integer = 0 To sTokens.Length - 1
    '                sTokens(index) = sTokens(index).Trim()
    '            Next

    '            For index As Integer = 1 To sTokens.Length - 2
    '                If sTokens(index).Length > 0 Then
    '                    .Add("    public " & sTokens(index) & "  { get; set; }")
    '                    sArgumentString += sTypeArgPairDelimiter & sTokens(index)
    '                    sTypeArgPairDelimiter = ", "
    '                End If
    '            Next

    '            .Add("    private void _" & sEventName & "(" & sArgumentString & ")")
    '            .Add("    {	")
    '            .Add("        EventID = eEVENT." & sEventName & ";")
    '            .Add("        Name = """ & sEventName & """;")
    '            If sArgumentString.Length > 0 Then
    '                sArgOnlyDelimiter = ""
    '                For index As Integer = 1 To sTokens.Length - 2
    '                    Dim sTypeArgSplit As String() = sTokens(index).Split(" ")
    '                    .Add("        this." & sTypeArgSplit(1) & " = " & sTypeArgSplit(1) & ";      // save the parameter value in local instance storage")
    '                    sArgumentNamesOnlyString += sArgOnlyDelimiter & sTypeArgSplit(1)
    '                    sArgOnlyDelimiter = ", "
    '                Next
    '                sArgumentString = ", " & sArgumentString
    '            End If
    '            .Add("        EventPump.EnqueueEvent(this);")
    '            .Add("    }")

    '            .Add("")
    '            .Add("    public " & sEventName & "(ZClass oTargetInstance" & sArgumentString & ") ")
    '            .Add("        : base(oTargetInstance) ")
    '            .Add("    { ")
    '            .Add("        _" & sEventName & "(" & sArgumentNamesOnlyString & "); ")
    '            .Add("    } ")
    '            .Add("")
    '            .Add("    public " & sEventName & "(long lDelayMilliseconds, ZClass oTargetInstance" & sArgumentString & ") ")
    '            .Add("        : base(oTargetInstance) ")
    '            .Add("    { ")
    '            .Add("       this.Delay_milliseconds = lDelayMilliseconds;")
    '            .Add("       _" & sEventName & "(" & sArgumentNamesOnlyString & "); ")
    '            .Add("    } ")

    '            Dim sArgumentStringSansLeadingComma As String = ""
    '            If sArgumentNamesOnlyString.Length > 0 Then
    '                sArgumentStringSansLeadingComma = sArgumentString.Substring(1, sArgumentString.Length - 1)
    '                sArgumentNamesOnlyString = ", " & sArgumentNamesOnlyString
    '            End If
    '            .Add("")
    '            .Add("    // quick accessors")
    '            .Add("    public static void Send(ZClass oTargetInstance" & sArgumentString & ") { " & sEventName & " oDummyEvent = new " & sEventName & "(oTargetInstance" & sArgumentNamesOnlyString & "); }")
    '            .Add("    public static void Self(" & sArgumentStringSansLeadingComma & ") { " & sEventName & " oDummyEvent = new " & sEventName & "(ZClass.ActiveInstance" & sArgumentNamesOnlyString & "); }")
    '            .Add("    public static void DelayedSelf(int iTicks" & sArgumentString & ") { " & sEventName & " oDummyEvent = new " & sEventName & "(iTicks, ZClass.ActiveInstance" & sArgumentNamesOnlyString & "); }")
    '            .Add("    public static void Delayed(int iTicks, ZClass oTargetInstance" & sArgumentString & ") { " & sEventName & " oDummyEvent = new " & sEventName & "(iTicks, oTargetInstance" & sArgumentNamesOnlyString & "); }")
    '            .Add("}" & vbCrLf)
    '        End With
    '    Next

    'End Sub

    ''Private Sub createDomain(ByVal oRepository As EA.Repository, ByVal oPackage As EA.Package, ByVal bIncludeDebug As Boolean)
    ''    Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), CanonicalName(oPackage.Name) & ".cs")
    ''    Dim oSourceOutout As New RichTextBox

    ''    gStatusBox.Filename = oPackage.Name
    ''    Dim oDomain As Domain = New Domain(oPackage, oRepository, oSourceOutout)      ' constructor does the work

    ''    createOutputFile(sOutputFilename, oSourceOutout.Text, oDomain)

    ''    For Each oChildPackage As EA.Package In oPackage.Packages
    ''        createDomain(oRepository, oChildPackage, bIncludeDebug)
    ''    Next
    ''End Sub

    'Protected Class xDomain
    '    Private _ClassById As Collection
    '    Private _Triggers As Collection
    '    Private _Enumerations As Collection
    '    Private _DataTypes As Collection
    '    Private _States As Collection
    '    Private _Notes As Collection
    '    Private _Boundarys As Collection
    '    Private _StateMachines As Collection
    '    Private _ObjectInstances As Collection
    '    Private _Interfaces As Collection
    '    Private _ElementById As Collection
    '    Private _InitialStates As Collection
    '    Private _FinalStates As Collection
    '    Private _IgnoreIndicatorStates As Collection
    '    Private _TestElements As Collection
    '    Private _ParentChildren As Collection
    '    Private _oExternals As Collection

    '    Private _oTestFixtureElement As EA.Element
    '    Private _oSourceOutput As RichTextBox
    '    Private _oRepository As EA.Repository
    '    Private _sPackageId As String
    '    Private _oProject As EA.Project
    '    Private _oPackage As EA.Package
    '    Private _IsRealized As Boolean
    '    Private _Name As String
    '    Private _DiagramVersion As String
    '    Private _DiagramNotes As String
    '    Private _EAClassInstances As New List(Of EAClass)

    '    Public ReadOnly Property EAClassInstances()
    '        Get
    '            Return _EAClassInstances
    '        End Get
    '    End Property

    '    Public ReadOnly Property ClassByID() As Collection
    '        Get
    '            Return _ClassById
    '        End Get
    '    End Property

    '    Public ReadOnly Property ParentChildren() As Collection
    '        Get
    '            Return _ParentChildren
    '        End Get
    '    End Property

    '    Public ReadOnly Property Name() As String
    '        Get
    '            Return _Name
    '        End Get
    '    End Property

    '    Public ReadOnly Property DiagramVersion() As String
    '        Get
    '            Return _DiagramVersion
    '        End Get
    '    End Property

    '    Public ReadOnly Property DiagramNotes() As String
    '        Get
    '            Return _DiagramNotes
    '        End Get
    '    End Property

    '    Public ReadOnly Property TestFixtureElement() As EA.Element
    '        Get
    '            Return _oTestFixtureElement
    '        End Get
    '    End Property

    '    Public ReadOnly Property TestElements() As Collection
    '        Get
    '            Return _TestElements
    '        End Get
    '    End Property

    '    Public ReadOnly Property Notes() As Collection
    '        Get
    '            Return _Notes
    '        End Get
    '    End Property

    '    Public ReadOnly Property Repository() As EA.Repository
    '        Get
    '            Return _oRepository
    '        End Get
    '    End Property

    '    Public ReadOnly Property Boundarys() As Collection
    '        Get
    '            Return _Boundarys
    '        End Get
    '    End Property

    '    Public ReadOnly Property ObjectInstances() As Collection
    '        Get
    '            Return _ObjectInstances
    '        End Get
    '    End Property

    '    Public ReadOnly Property Externals() As Collection
    '        Get
    '            Return _oExternals
    '        End Get
    '    End Property

    '    Public ReadOnly Property StateMachines() As Collection
    '        Get
    '            Return _StateMachines
    '        End Get
    '    End Property

    '    Public ReadOnly Property ElementById() As Collection
    '        Get
    '            Return _ElementById
    '        End Get
    '    End Property

    '    Public ReadOnly Property States() As Collection
    '        Get
    '            Return _States
    '        End Get
    '    End Property

    '    Public ReadOnly Property Triggers() As Collection
    '        Get
    '            Return _Triggers
    '        End Get
    '    End Property

    '    Public ReadOnly Property EAClass(ByVal iID As Integer) As EA.Element
    '        Get
    '            Dim oClass As EA.Element = Nothing

    '            If _ClassById.Contains(iID.ToString) Then
    '                oClass = _ClassById.Item(iID.ToString)
    '            Else
    '                MsgBox("Unknown class id: " & iID, MsgBoxStyle.Critical)
    '            End If
    '            Return oClass
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsRealized() As Boolean
    '        Get
    '            Return _IsRealized
    '        End Get
    '    End Property

    '    Public ReadOnly Property Package() As EA.Package
    '        Get
    '            Return _oPackage
    '        End Get
    '    End Property

    '    Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository, ByRef oSourceOutput As RichTextBox)
    '        Try
    '            giNextStateID = oPackage.Name.GetHashCode
    '            giNextEventID = giNextStateID

    '            _oSourceOutput = oSourceOutput
    '            _oRepository = oRepository
    '            _oPackage = oPackage
    '            _sPackageId = oPackage.PackageID
    '            _Name = CanonicalName(oPackage.Name)
    '            _oTestFixtureElement = Nothing

    '            DomainEventNames = New Collection

    '            Try
    '                _DiagramNotes = ""
    '                _DiagramVersion = "??"

    '                Dim oDiagram As EA.Diagram = oPackage.Diagrams.GetAt(0)
    '                _DiagramNotes = oDiagram.Notes
    '                _DiagramVersion = oDiagram.Version
    '            Catch
    '            Finally
    '            End Try

    '            _oExternals = New Collection
    '            _Boundarys = New Collection
    '            _Notes = New Collection
    '            _ObjectInstances = New Collection
    '            _TestElements = New Collection
    '            _Interfaces = New Collection
    '            _ClassById = New Collection
    '            _Triggers = New Collection
    '            _Enumerations = New Collection
    '            _DataTypes = New Collection
    '            _States = New Collection
    '            _ElementById = New Collection
    '            _StateMachines = New Collection
    '            _InitialStates = New Collection
    '            _FinalStates = New Collection
    '            _IgnoreIndicatorStates = New Collection
    '            _ParentChildren = New Collection

    '            _IsRealized = PackageIncludesStereotype(_oPackage, "realized")

    '            If Not _IsRealized Then
    '                catalogElements()
    '                'catalogParents()
    '                generateSource()
    '                'checkRelationshipNames()
    '                'addInitialInstancePopulation()
    '                addDomainOperations()
    '                'createVersionFile()
    '                'createExternalsFile()
    '            End If
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Sub addDomainOperations()
    '        With _oSourceOutput
    '            For Each oEAClass As EAClass In _EAClassInstances
    '                oEAClass.AddClassOperations(True)
    '            Next
    '        End With
    '    End Sub

    '    Private Sub snip(ByVal sMyID As String, ByRef sAncestryString As String)
    '        Dim sOrignalAncestryString As String = sAncestryString
    '        sAncestryString = Regex.Replace(sAncestryString, "[,]*" + sMyID, "")
    '    End Sub

    '    Private Function matchIDs(ByVal sAncestryString1 As String, ByVal sAncestryString2 As String) As Boolean
    '        Dim bMatch As Boolean = False

    '        sAncestryString1 = Regex.Replace(sAncestryString1, "[,]+", " ")         ' all double commas become single spaces
    '        sAncestryString1 = Regex.Replace(sAncestryString1, "[ ]+", " ")         ' all double spaces become singles
    '        Dim sAncestry1Ids() = Split(sAncestryString1.Trim, " ")

    '        sAncestryString2 = Regex.Replace(sAncestryString2, "[,]+", " ")         ' all double commas become single spaces
    '        sAncestryString2 = Regex.Replace(sAncestryString2, "[ ]+", " ")         ' all double spaces become singles
    '        Dim sAncestry2Ids() = Split(sAncestryString2.Trim, " ")

    '        If (sAncestry1Ids.Length = sAncestry2Ids.Length) And _
    '           (sAncestryString1.Length > 0) And _
    '           (sAncestryString2.Length > 0) Then

    '            Dim oComparisonCollection As New Collection
    '            For Each sID1 As String In sAncestry1Ids                            ' first add all the ancestry 1 IDs
    '                oComparisonCollection.Add(sID1, sID1)
    '            Next

    '            bMatch = True                                                       ' assume all will match
    '            For Each sID2 As String In sAncestry2Ids                            ' next verify all the ancestry 2 IDs are represented
    '                If Not oComparisonCollection.Contains(sID2) Then
    '                    bMatch = False                                              ' any single failure is enough to bail out 
    '                    Exit For
    '                End If
    '            Next
    '        End If

    '        Return bMatch
    '    End Function

    '    Private Function removeNonFamilyIDs(ByVal oFamily As Collection, ByVal oNonFamily As Collection) As Collection
    '        Dim oFamilyMemberAncestry As New Collection             ' we start with a complete set of ancestry IDs for these family members
    '        For Each oFamilyMemberClass As EA.Element In oFamily
    '            oFamilyMemberAncestry.Add(oFamilyMemberClass.GetRelationSet(EA.EnumRelationSetType.rsParents), oFamilyMemberClass.ElementID.ToString)
    '        Next

    '        For Each oNonFamilyMemberClass As EA.Element In oNonFamily
    '            Dim sNonFamilyMemberClassID As String = oNonFamilyMemberClass.ElementID.ToString

    '            For Each oFamilyMemberClass As EA.Element In oFamily
    '                Dim sFamilyMemberID As String = oFamilyMemberClass.ElementID.ToString
    '                Dim sFamilyMemberAncestry As String = oFamilyMemberAncestry(sFamilyMemberID)

    '                oFamilyMemberAncestry.Remove(sFamilyMemberID)
    '                snip(sNonFamilyMemberClassID, sFamilyMemberAncestry)
    '                oFamilyMemberAncestry.Add(sFamilyMemberAncestry, sFamilyMemberID)
    '            Next
    '        Next

    '        Return oFamilyMemberAncestry
    '    End Function

    '    Private Sub appendChild(ByVal oParentClass As EA.Element, ByVal oChildClass As EA.Element)
    '        Dim oChildren As Collection

    '        If Not _ParentChildren.Contains(oParentClass.ElementID.ToString) Then
    '            oChildren = New Collection
    '            _ParentChildren.Add(oChildren, oParentClass.ElementID.ToString)     ' add this parent's child collection to the main collection
    '        End If
    '        oChildren = _ParentChildren(oParentClass.ElementID.ToString)

    '        If Not oChildren.Contains(oChildClass.ElementID.ToString) Then
    '            oChildren.Add(oChildClass, oChildClass.ElementID.ToString)              ' add one child to this parent's child collection
    '        End If
    '    End Sub

    '    Private Sub catalogElement(ByVal oElement As EA.Element)
    '        Application.DoEvents()

    '        oElement.Name = CanonicalClassName(oElement.Name)           ' establish safe names right off the bat (rather than sprinkling everywehre)
    '        _ElementById.Add(oElement, oElement.ElementID)              ' just as a debugging convenience, to look up any element from its id only

    '        If oElement.Name.Length = 0 Then
    '            oElement.Name = "NoName_" & oElement.ElementID
    '        End If

    '        Select Case oElement.MetaType
    '            Case "Object"
    '                _ObjectInstances.Add(oElement, oElement.ElementID)

    '            Case "StateMachine"
    '                _StateMachines.Add(oElement, oElement.ElementID)

    '            Case "FinalState"
    '                _FinalStates.Add(oElement, oElement.ElementID)
    '                _States.Add(oElement, oElement.ElementID)

    '            Case "Pseudostate"
    '                Select Case oElement.Name
    '                    Case "Initial"
    '                        _InitialStates.Add(oElement, oElement.ElementID)
    '                        _States.Add(oElement, oElement.ElementID)

    '                    Case Else
    '                        _IgnoreIndicatorStates.Add(oElement, oElement.ElementID)
    '                        _States.Add(oElement, oElement.ElementID)
    '                End Select

    '            Case "Trigger"
    '                _Triggers.Add(oElement, oElement.Name)

    '            Case "StateNode"
    '                _States.Add(oElement, oElement.ElementID)

    '            Case "Enumeration"
    '                _Enumerations.Add(oElement, oElement.ElementID)
    '                _ModelEnumerations.Add(oElement.Name, oElement)

    '            Case "DataType"
    '                _DataTypes.Add(oElement, oElement.ElementID)
    '                _ModelDataTypes.Add(oElement, oElement.ElementID)

    '            Case "Class"
    '                oElement.Name = CanonicalClassName(oElement.Name)
    '                _ClassById.Add(oElement, oElement.ElementID)

    '            Case "Interface"
    '                oElement.Name = CanonicalClassName(oElement.Name)
    '                _Interfaces.Add(oElement, oElement.ElementID)

    '            Case "State"
    '                oElement.Name = CanonicalName(oElement.Name)
    '                _States.Add(oElement, oElement.ElementID)

    '            Case "Note", "Text"
    '                ' do nothing with these, just allow them without complaint

    '            Case Else
    '                Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
    '        End Select

    '        For Each oSubElement As EA.Element In oElement.Elements
    '            catalogElement(oSubElement)                      ' recurse down the element tree
    '        Next
    '    End Sub

    '    Private Sub catalogElements()
    '        Dim oElement As EA.Element = Nothing
    '        Try
    '            For Each oElement In _oPackage.Elements
    '                catalogElement(oElement)
    '            Next
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler
    '            oErrorHandler.SupplementalInformation = "Catalog Elements (" & oElement.Name & ")"
    '            oErrorHandler.Announce(ex)
    '        End Try
    '    End Sub

    '    Private Sub generateSource()
    '        Dim oClassElement As EA.Element
    '        Dim oEAClass As EAClass
    '        Static iClassCounter As Integer = 0

    '        Try
    '            If (_ClassById.Count > 0) Or (_Enumerations.Count > 0) Or (_DataTypes.Count > 0) Then
    '                Application.DoEvents()
    '                With _oSourceOutput
    '                    If _oPackage.Notes.Length > 0 Then
    '                        .AppendText("       // " & _oPackage.Notes)
    '                    End If
    '                    .AppendText(vbCrLf)

    '                    gStatusBox.ProgressValueMaximum = _ClassById.Count
    '                    For Each oClassElement In _ClassById
    '                        gStatusBox.ProgressValue = iClassCounter
    '                        Application.DoEvents()
    '                        If _sPackageId = oClassElement.PackageID Then
    '                            If Not ElementIncludesStereotype(oClassElement, "DataType") Then  ' classes with stereotype 'DataType' are just psuedo-types for use in the model
    '                                If ElementIncludesStereotype(oClassElement, "omit") Then
    '                                    Debug.WriteLine("Omitting class (by stereotype 'omit'): " & oClassElement.Name)
    '                                Else
    '                                    oEAClass = New EAClass(oClassElement, Me, _oSourceOutput)
    '                                    _EAClassInstances.Add(oEAClass)
    '                                End If
    '                            End If
    '                        End If
    '                        iClassCounter += 1
    '                    Next
    '                End With
    '            End If
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    'End Class

    'Private Class EAClass
    '    Private _oEAElement As EA.Element
    '    Private _oSourceOutput As RichTextBox
    '    Private _oDomain As Domain
    '    Private _bFinalStateReported As Boolean = False
    '    Private _sInitialState As String = ""
    '    Private _bActiveClass As Boolean = False
    '    Private _sParentClassName As String = ""
    '    Private _bIsSupertype As Boolean
    '    Private _bIsSubtype As Boolean
    '    Private _oStateClassSequences As New Collection
    '    Private _oChildrenCollection As Collection
    '    Private _oEventNames As New List(Of String)

    '    Public Sub New(ByVal oEAElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
    '        Dim bIsTestFixture As Boolean = False
    '        Dim bIsTest As Boolean = False
    '        Dim oStateNames As New Collection

    '        Try
    '            _oDomain = oDomain
    '            _oSourceOutput = oSourceOutput
    '            _oEAElement = oEAElement

    '            gStatusBox.ShowClassName(_oEAElement.Name)

    '            If _oDomain.TestFixtureElement IsNot Nothing Then
    '                bIsTestFixture = (_oDomain.TestFixtureElement.Name = _oEAElement.Name)
    '            End If
    '            bIsTest = _oDomain.TestElements.Contains(_oEAElement.Name)

    '            With _oSourceOutput
    '                .AppendText(vbCrLf)

    '                If Not (ElementIncludesStereotype(_oEAElement, "domain") Or ElementIncludesStereotype(_oEAElement, "external")) Then
    '                    .AppendText("//_______________________________________________________________________________________________" & vbCrLf)
    '                    .AppendText("//_______________________________________________________________________________________________" & vbCrLf)
    '                    .AppendText("public class " & _oEAElement.Name & " : " & parentSupertype() & vbCrLf)
    '                    .AppendText("{" & vbCrLf)

    '                    Dim iClassStateMachineID As Integer = getClassStateMachineID(_oEAElement)
    '                    For Each oState As EA.Element In _oDomain.States
    '                        If oState.ParentID = iClassStateMachineID Then
    '                            If oState.MetaType <> "Pseudostate" Then
    '                                .AppendText("    private static " & oState.Name & " o" & oState.Name & " = new " & oState.Name & "();" & vbCrLf)
    '                                oStateNames.Add(oState.Name)
    '                            End If
    '                        End If
    '                    Next

    '                    Dim oAttributes As Collection = accumulateAttributes(_oEAElement)
    '                    Dim oConnectors As Collection = accumulateConnectors(_oEAElement)

    '                    addStateMachine(oStateNames)
    '                    addAttributes()

    '                    AddClassOperations(False)          ' add all the operations 

    '                    .AppendText("}" & vbCrLf)
    '                Else
    '                    'AddDomainExternalOperations()
    '                End If
    '            End With
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Function parentSupertype() As String
    '        Dim sParentName = "ZClass"
    '        Dim oParentClass As EA.Element
    '        Dim oTokens As String() = Split(_oEAElement.GetRelationSet(EA.EnumRelationSetType.rsParents), ",")

    '        If oTokens(0).Length > 0 Then
    '            oParentClass = _oDomain.ClassByID.Item(oTokens(0))
    '            sParentName = oParentClass.Name
    '        End If
    '        Return sParentName
    '    End Function

    '    Public Sub AddClassOperations(ByVal bDomainLevel As Boolean)
    '        Dim oMethod As EA.Method
    '        Dim oParameter As EA.Parameter
    '        Dim sLeadingComma As String = ""
    '        Dim oRequiredParameters As Collection
    '        Dim sBehavior As String = ""

    '        With _oSourceOutput
    '            For Each oMethod In _oEAElement.Methods
    '                If (Not bDomainLevel And (Not MethodIncludesStereotype(oMethod, "domain") Or (Not MethodIncludesStereotype(oMethod, "external")) Or _
    '                           (bDomainLevel And (MethodIncludesStereotype(oMethod, "domain") Or MethodIncludesStereotype(oMethod, "external"))))) Then

    '                    If oMethod.ReturnType.Trim().Length = 0 Then
    '                        oMethod.ReturnType = "void"
    '                    Else
    '                        oMethod.ReturnType = CanonicalType(oMethod.ReturnType)
    '                    End If

    '                    oMethod.Name = CanonicalName(oMethod.Name)
    '                    sLeadingComma = ""

    '                    .AppendText(vbCrLf)
    '                    If oMethod.Notes.Length > 0 Then
    '                        For Each sLine As String In Split(oMethod.Notes, vbCrLf)
    '                            If sLine.Length > 0 Then
    '                                .AppendText("    // " & sLine)
    '                            End If
    '                        Next
    '                        .AppendText(vbCrLf)
    '                    End If

    '                    oRequiredParameters = New Collection
    '                    For Each oParameter In oMethod.Parameters
    '                        Application.DoEvents()
    '                        oParameter.Name = CanonicalName(oParameter.Name)
    '                        oParameter.Type = CanonicalType(oParameter.Type)
    '                        oRequiredParameters.Add(oParameter)
    '                    Next

    '                    If MethodIncludesStereotype(oMethod, "event") Then
    '                        .AppendText("       public delegate void " + oMethod.Name + "Delegate(")
    '                        sLeadingComma = ""
    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Type + " " + oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(");" & vbCrLf)
    '                        .AppendText("       public static event " + oMethod.Name + "Delegate " + oMethod.Name + "Event;    //_________________ '" + oMethod.Name & " (delegate)" & vbCrLf)
    '                        .AppendText("       public static void Raise_" + oMethod.Name + "(")
    '                        sLeadingComma = ""
    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Type + " " + oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")           " & vbCrLf)
    '                        .AppendText("       {                                                                      " & vbCrLf)
    '                        .AppendText("           if (null != " + oMethod.Name + "Event)                                " & vbCrLf)
    '                        .AppendText("           {                                                                  " & vbCrLf)
    '                        .AppendText("               " + oMethod.Name + "Event(")
    '                        sLeadingComma = ""
    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(");                      " & vbCrLf)
    '                        .AppendText("           }                                                                  " & vbCrLf)
    '                        .AppendText("       }                                                                      " & vbCrLf)
    '                    Else
    '                        sBehavior = StripRichTextFormat(oMethod.Behavior)
    '                        .AppendText(vbCrLf)
    '                        If MethodIncludesStereotype(oMethod, "instance") Then
    '                            .AppendText("    public " & oMethod.ReturnType)
    '                        Else
    '                            .AppendText("    public static " & oMethod.ReturnType)
    '                        End If
    '                        .AppendText(" " + oMethod.Name + "(")

    '                        sLeadingComma = ""
    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Type + " " + oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next

    '                        .AppendText(")" + "                                                 //_________________ `" + oMethod.Name & "  (domain function)")
    '                        .AppendText(vbCrLf)
    '                        .AppendText("    {" & vbCrLf)
    '                        .AppendText(sBehavior)
    '                        .AppendText(vbCrLf)
    '                        .AppendText("    }" & vbCrLf)
    '                    End If
    '                End If
    '            Next
    '        End With
    '    End Sub

    '    Private Function getClassStateMachineID(ByVal oClass As EA.Element)
    '        Dim iStateMachineID As Integer = oClass.ElementID

    '        For Each oStateMachineElement As EA.Element In _oDomain.StateMachines
    '            Application.DoEvents()
    '            If oClass.ElementID = oStateMachineElement.ParentID Then
    '                iStateMachineID = oStateMachineElement.ElementID
    '                Exit For
    '            End If
    '        Next
    '        Return iStateMachineID
    '    End Function

    '    Private Sub recordStateCallingSequence(ByVal oState As EA.Element, ByVal sArgumentList As String, ByVal oClass As EA.Element)
    '        Dim sCallingSequence As String = "        state " & oState.Name & " ("

    '        If sArgumentList.Length > 0 Then
    '            Dim sArgumentString As String = " "
    '            Dim sTypeArgPairDelimiter As String = " "
    '            Dim sArguments() As String = Split(sArgumentList + ",", ",")
    '            For Each sArgument As String In sArguments
    '                If sArgument.Length > 0 Then
    '                    Dim sTypeNamePairs() = Split(sArgument.Trim, " ")
    '                    Dim sType As String = CanonicalType(sTypeNamePairs(0)).Trim
    '                    Dim sName As String = sTypeNamePairs(1).Trim
    '                    sCallingSequence &= sTypeArgPairDelimiter & vbCrLf & "            " & sType & " " & sName
    '                    sTypeArgPairDelimiter = ", "
    '                End If
    '            Next
    '        End If
    '        sCallingSequence &= ")" & vbCrLf
    '        sCallingSequence &= "            {"

    '        If Not IsUnique(sCallingSequence, _oStateClassSequences, oState.Name & oClass.Name) Then
    '            Dim sPreviousCallingSequence As String = _oStateClassSequences(oState.Name & oClass.Name)
    '            If Not sPreviousCallingSequence = sCallingSequence Then
    '                MessageBox.Show("State '" + oState.Name + "' has incoming events with different " + vbCrLf + "parameter sequences:" + vbCrLf + vbCrLf + sCallingSequence + vbCrLf + vbCrLf + "and:" + vbCrLf + vbCrLf + sPreviousCallingSequence, "Modeling Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    '            End If
    '        End If
    '    End Sub

    '    Private Sub addStateMachine(ByVal oStateNames As Collection)
    '        Dim oState As EA.Element
    '        Dim bStateMachineHeaderAdded As Boolean = False
    '        Dim oSupplierState As EA.Element = Nothing
    '        Dim oPopulateTransitions As New Collection
    '        Dim sDomainName As String = _oDomain.Name
    '        Dim oPigtails As New Collection
    '        Dim iClassStateMachineID As Integer
    '        Dim oStateNameUniqueBucket As New Collection

    '        Try
    '            iClassStateMachineID = getClassStateMachineID(_oEAElement)

    '            If _oDomain.States.Count > 0 Then
    '                For Each oState In _oDomain.States
    '                    If oState.ParentID = iClassStateMachineID Then
    '                        _bActiveClass = True
    '                        Exit For
    '                    End If
    '                Next

    '                If _bActiveClass Then               ' if this class has a state machine
    '                    With _oSourceOutput
    '                        For Each oState In _oDomain.States
    '                            If oState.ParentID = iClassStateMachineID Then
    '                                If oState.Subtype = EA_TYPE.INITIAL_STATE Then        ' if this state is the "meatball" initial state
    '                                    If oState.Connectors.Count = 1 Then
    '                                        Dim oOriginalState As EA.Element = _oDomain.States(CType(oState.Connectors.GetAt(0), EA.Connector).SupplierID.ToString)
    '                                        _sInitialState = oOriginalState.Name
    '                                    Else
    '                                        MsgBox("An initial state in the state model for class '" & _oEAElement.Name & "' has no transition out", MsgBoxStyle.Critical)
    '                                    End If
    '                                End If

    '                                If IsUnique(oState.Name, oStateNameUniqueBucket) Then
    '                                    addState(oState, _oEAElement)
    '                                End If
    '                            End If
    '                        Next
    '                        .AppendText(vbCrLf)

    '                        If _sInitialState.Length > 0 Then
    '                            .AppendText("    public " & _oEAElement.Name & "()     // class constructor" & vbCrLf)
    '                            .AppendText("    {" & vbCrLf)
    '                            .AppendText("       if(" & _oDomain.Name & "." & _oEAElement.Name & "s == null)" & vbCrLf)
    '                            .AppendText("       {" & vbCrLf)
    '                            .AppendText("           " & _oDomain.Name & "." & _oEAElement.Name & "s = new List<" & _oEAElement.Name & ">();" & vbCrLf)
    '                            .AppendText("       }" & vbCrLf)

    '                            For Each sStateName As String In oStateNames
    '                                .AppendText("        o" & sStateName & ".InitializeState(this);" & vbCrLf)
    '                            Next
    '                            .AppendText("")
    '                            .AppendText("        " & _oDomain.Name & "." & _oEAElement.Name & "s.Add(this);" & vbCrLf)
    '                            .AppendText("        this.Name = """ & _oEAElement.Name & " ("" + " & _oDomain.Name & "." & _oEAElement.Name & "s.Count.ToString() + "")"";" & vbCrLf)

    '                            .AppendText("")
    '                            .AppendText("        CurrentState = o" & _sInitialState & ";" & vbCrLf)
    '                            .AppendText("    }" & vbCrLf)
    '                        End If
    '                    End With
    '                End If
    '            End If
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Sub addNormalTransition(ByVal oConnector As EA.Connector, _
    '                                    ByVal oState As EA.Element, _
    '                                    ByVal oClass As EA.Element)
    '        Dim sArgumentList As String = ""
    '        Dim oClientState As EA.Element
    '        Dim sEvent As String
    '        Dim sTokens() As String
    '        Dim oSupplierState As EA.Element = Nothing
    '        Dim oErrorHandler As New sjmErrorHandler
    '        Dim oEVENT_ErrorHandler As New sjmErrorHandler

    '        Try
    '            With _oSourceOutput
    '                oErrorHandler.SupplementalInformation = "Class: " & oClass.Name & ", State: " & oState.Name & ", Connector: " & oConnector.Name
    '                If _oDomain.States.Contains(oConnector.ClientID.ToString) Then                      ' if "to" state is a normal state
    '                    oSupplierState = _oDomain.States(oConnector.ClientID.ToString)
    '                    If oSupplierState Is oState Then
    '                        If _oDomain.States.Contains(oConnector.SupplierID.ToString) Then
    '                            oClientState = _oDomain.States(oConnector.SupplierID.ToString)

    '                            sEvent = Regex.Replace(oConnector.TransitionEvent, "\s*\(", "(")
    '                            sTokens = Split(sEvent, ":")
    '                            If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
    '                                sEvent = sTokens(1)
    '                            End If

    '                            If sEvent.Length > 0 Then
    '                                Dim oMatches As MatchCollection = Regex.Matches(sEvent, "([^\(]+)\(([^()]+)\)")
    '                                If oMatches.Count > 0 Then
    '                                    Dim oGroups As GroupCollection = oMatches(0).Groups
    '                                    If oGroups.Count > 2 Then
    '                                        sEvent = Regex.Replace(Trim(oGroups(1).ToString()), "\s+", " ")
    '                                        sArgumentList = Regex.Replace(Trim(oGroups(2).ToString()), "\s+", " ")
    '                                    End If
    '                                End If
    '                                recordStateCallingSequence(oClientState, sArgumentList, oClass)
    '                                Dim sCanonicalEventName As String = canonicalEventName(sEvent)
    '                                Dim sCanonicalEventString As String = sCanonicalEventName & "," & sArgumentList & ", "      ''''' & oClass.Name

    '                                IsUnique(sCanonicalEventString, OutputLanguageCS.ModelEventNames)     ' add the event name to the list, if it isn't there already
    '                                IsUnique(sCanonicalEventString, OutputLanguageCS.DomainEventNames)    ' add the event name to this list too, if it isn't there already

    '                                Dim sClientStateName As String = CanonicalName(oClientState.Name)
    '                                If oClientState.Subtype = EA_TYPE.EXIT_STATE Then  ' if this state is an ignore marker
    '                                    sClientStateName = "null"
    '                                Else
    '                                    sClientStateName = oClass.Name & ".o" & sClientStateName
    '                                End If

    '                                If oSupplierState.Subtype <> EA_TYPE.ENTRY_STATE Then
    '                                    .AppendText("                RegisterTransition(eEVENT." & sCanonicalEventName & ", " & sClientStateName & ");" & vbCrLf)
    '                                End If

    '                                If Not _oEventNames.Contains(sCanonicalEventName) Then
    '                                    _oEventNames.Add(sCanonicalEventName)
    '                                End If
    '                            End If
    '                        End If
    '                    End If
    '                End If
    '            End With
    '        Catch ex As Exception
    '            oErrorHandler.Announce(ex)
    '        End Try
    '    End Sub

    '    Private Sub addNormalPigtailTransition(ByVal iClassStateMachineID As Integer)
    '        Dim oSupplierState As EA.Element = Nothing
    '        Dim oState As EA.Element
    '        Dim oConnector As EA.Connector
    '        Dim sEvent As String
    '        Dim sTokens() As String

    '        Try
    '            For Each oState In _oDomain.States
    '                Application.DoEvents()
    '                If oState.ParentID = iClassStateMachineID Then
    '                    If oState.Subtype = EA_TYPE.ENTRY_STATE Then       ' if this state is a pigtail-originating state
    '                        For Each oConnector In oState.Connectors
    '                            sEvent = canonicalEventName(oConnector)
    '                            sTokens = Split(sEvent, ":")
    '                            If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
    '                                sEvent = sTokens(1)
    '                            End If

    '                            If sEvent.Length > 0 Then
    '                                If sEvent.IndexOf(")") > 0 Then
    '                                    sTokens = Split(sEvent.Substring(0, sEvent.IndexOf(")")), "(")            ' peel off any parameter payload
    '                                    If sTokens.Length > 1 Then
    '                                        sEvent = sTokens(0)
    '                                    End If
    '                                End If
    '                                With _oSourceOutput
    '                                    oSupplierState = _oDomain.States(oConnector.SupplierID.ToString)
    '                                    IsUnique(sEvent, OutputLanguageCS.ModelEventNames)     ' add the event name to the list if it isn't there already
    '                                    IsUnique(sEvent, OutputLanguageCS.DomainEventNames)    ' add the event name to the list if it isn't there already
    '                                    .AppendText("                RegisterTransition(eEVENT." & sEvent & ", o" & oSupplierState.Name & ");      // pigtail event" & vbCrLf)
    '                                End With
    '                            End If
    '                        Next
    '                    End If
    '                End If
    '            Next
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Sub addState(ByVal oState As EA.Element, ByVal oClass As EA.Element)
    '        Dim sLines() As String
    '        Dim sLine As String
    '        Dim bQuiet As Boolean = False
    '        Dim bOmit As Boolean = False
    '        Dim sArgumentString As String = ""
    '        Dim sQuietCountVariableName = oClass.Name & "_" & oState.Name & "_quietCount"

    '        Try
    '            bQuiet = ElementIncludesStereotype(oState, "quiet")
    '            bOmit = ElementIncludesStereotype(oState, "omit")

    '            If (Not bOmit) And (oState.Name.Length > 0) Then
    '                With _oSourceOutput
    '                    Select Case oState.Subtype
    '                        Case EA_TYPE.SYNCH_STATE, EA_TYPE.INITIAL_STATE, EA_TYPE.EXIT_STATE, EA_TYPE.ENTRY_STATE
    '                            ' do nothing with these pseudo states, just allow them without error

    '                        Case Else
    '                            sLines = Split(oState.Notes, vbCrLf)
    '                            .AppendText(vbCrLf)
    '                            .AppendText("    public class " & oState.Name & " : ZState" & vbCrLf)
    '                            .AppendText("    {																										  " & vbCrLf)
    '                            .AppendText("        public void InitializeState(" & oClass.Name & " oParentClass)" & vbCrLf)
    '                            .AppendText("        {" & vbCrLf)
    '                            .AppendText("            ParentClass = (" & oClass.Name & ")oParentClass;           // a convenient pointer to my parent class" & vbCrLf)
    '                            .AppendText("            if (oExitTransitions == null)  // if the state transition table has not yet been created" & vbCrLf)
    '                            .AppendText("            {" & vbCrLf)
    '                            .AppendText("                Name = """ & oState.Name & """;" & vbCrLf)
    '                            If bQuiet Then
    '                                .AppendText("                iQuietCount = 0;    // announce just once, quiet mode" & vbCrLf & vbCrLf)
    '                            Else
    '                                .AppendText("                iQuietCount = -1;" & vbCrLf & vbCrLf)
    '                            End If
    '                            .AppendText("                oExitTransitions = new Dictionary<eEVENT, ZState>();" & vbCrLf)

    '                            For Each oConnector As EA.Connector In oState.Connectors
    '                                addNormalTransition(oConnector, oState, _oEAElement)
    '                            Next

    '                            Dim iClassStateMachineID As Integer = getClassStateMachineID(_oEAElement)
    '                            addNormalPigtailTransition(iClassStateMachineID)
    '                            .AppendText("            }" & vbCrLf)
    '                            .AppendText("        }" & vbCrLf)
    '                            .AppendText(vbCrLf)

    '                            .AppendText("        public override void StateAction(ZEvent oEvent) //__________________________________________________`" & oState.Name & vbCrLf)
    '                            .AppendText("        {																									  " & vbCrLf)
    '                            .AppendText("            base.StateAction(oEvent); " & vbCrLf)
    '                            .AppendText("            " & _oEAElement.Name & " self = (" & _oEAElement.Name & ")ParentClass;           // a convenient pointer to my parent class" & vbCrLf)
    '                            .AppendText(vbCrLf)

    '                            'Debug.Assert(Not oState.Name.Contains("S103_Response_Processing"))
    '                            'If _oStateClassSequences.Contains(oState.Name & oClass.Name) Then
    '                            'End If

    '                            Dim bActionLinesFound As Boolean = False
    '                            For Each sLine In sLines
    '                                Dim sCleanLine As String = StripRichTextFormat(sLine)
    '                                .AppendText(vbLf & sCleanLine)
    '                                bActionLinesFound = bActionLinesFound Or (sCleanLine.Length > 0)
    '                            Next

    '                            If Not bActionLinesFound Then
    '                                .AppendText("            throw new NotImplementedException(""       #### State action empty: " & oState.Name & " ####"");" & vbCrLf)
    '                            End If

    '                            .AppendText(vbCrLf)
    '                            .AppendText("        }" & vbCrLf)
    '                            .AppendText("    }" & vbCrLf)
    '                    End Select
    '                End With
    '            End If

    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Function canonicalDefaultValue(ByVal sDefaultValue As String, ByVal sType As String) As String
    '        Dim sReturnDefaultString As String = sDefaultValue

    '        If sReturnDefaultString.Length = 0 Then
    '            sReturnDefaultString = "0"
    '        End If

    '        Select Case sType.ToLower
    '            Case "int", "float", "double", "boolean", "long", "byte", "unsigned char"
    '                ' do nothing, no adjustment needed

    '            Case "char", "string"
    '                sReturnDefaultString = """" & sReturnDefaultString & """"

    '            Case "xevent"
    '                sReturnDefaultString = "Nothing"
    '        End Select

    '        Return sReturnDefaultString
    '    End Function

    '    Private Function accumulateConnectors(ByVal oClass As EA.Element, Optional ByVal hUnique As Hashtable = Nothing, Optional ByRef oConnectors As Collection = Nothing)
    '        If oConnectors Is Nothing Then
    '            oConnectors = New Collection
    '            hUnique = New Hashtable
    '        End If

    '        If Not hUnique.Contains(oClass) Then
    '            hUnique.Add(oClass, oClass)
    '            For Each oConnector As EA.Connector In oClass.Connectors
    '                oConnector.Notes = oClass.Name     ' borrowing this instrinsic property to carry the name of the class participating in the relationship
    '                oConnectors.Add(oConnector)
    '            Next

    '            Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
    '            If (sTokens(0).Length > 0) Then
    '                For Each sId As String In sTokens
    '                    Dim oParentClass As EA.Element = _oDomain.EAClass(sId)
    '                    accumulateConnectors(oParentClass, hUnique, oConnectors)
    '                Next
    '            End If
    '        End If

    '        Return oConnectors
    '    End Function

    '    Private Function accumulateAttributes(ByVal oClass As EA.Element, Optional ByVal hUnique As Hashtable = Nothing, Optional ByRef oAttributes As Collection = Nothing)
    '        If oAttributes Is Nothing Then
    '            oAttributes = New Collection
    '            hUnique = New Hashtable
    '        End If

    '        If Not hUnique.Contains(oClass) Then
    '            hUnique.Add(oClass, oClass)
    '            For Each oAttribute As EA.Attribute In oClass.Attributes
    '                oAttributes.Add(oAttribute.Name)
    '            Next

    '            Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
    '            If (sTokens(0).Length > 0) Then
    '                For Each sId As String In sTokens
    '                    Dim oParentClass As EA.Element = _oDomain.EAClass(sId)
    '                    accumulateAttributes(oParentClass, hUnique, oAttributes)
    '                Next
    '            End If
    '        End If

    '        Return oAttributes
    '    End Function

    '    Private Function assembleConstructorAttributes(ByVal oConstructorAttributes As Collection, ByVal oClass As EA.Element) As Boolean
    '        Static bRequired As Boolean = False

    '        For Each oAttribute As EA.Attribute In oClass.Attributes
    '            If AttributeIncludesStereotype(oAttribute, "constructor") Then
    '                If AttributeIncludesStereotype(oAttribute, "required") Then
    '                    bRequired = True
    '                End If
    '                IsUnique(oAttribute, oConstructorAttributes, oAttribute.AttributeGUID)
    '            End If
    '        Next

    '        Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
    '        sTokens = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
    '        If (sTokens(0).Length > 0) Then
    '            For Each sParentClassId As String In sTokens
    '                Dim oParentClass As EA.Element = _oDomain.EAClass(sParentClassId)
    '                assembleConstructorAttributes(oConstructorAttributes, oParentClass)
    '            Next
    '        End If

    '        Return bRequired
    '    End Function

    '    Private Sub addAttributes()
    '        Dim sCommentText As String
    '        Dim sNotesString As String = ""
    '        Dim oAttribute As EA.Attribute
    '        Dim sCanonicalAttributeName As String
    '        Dim sCanonicalType As String
    '        Dim sOtherClassName As String = ""
    '        Dim sCardinalityComment As String = ""
    '        Dim sCollectionName As String = Replace(_oEAElement.Name & "s", "]s", "s]")

    '        Try
    '            With _oSourceOutput
    '                For Each oAttribute In _oEAElement.Attributes
    '                    oAttribute.Type = CanonicalType(oAttribute.Type)
    '                    oAttribute.Name = CanonicalName(oAttribute.Name)
    '                Next

    '                For Each oAttribute In _oEAElement.Attributes
    '                    If oAttribute.Notes.Length > 0 Then
    '                        sCommentText = "      ' " & oAttribute.Notes
    '                    Else
    '                        sCommentText = ""
    '                    End If
    '                Next

    '                Dim oConstructorAttributes As New Collection
    '                Dim bRequired As Boolean = assembleConstructorAttributes(oConstructorAttributes, _oEAElement)

    '                For Each oAttribute In _oEAElement.Attributes
    '                    Application.DoEvents()

    '                    Dim sVisibility As String = getAttributeVisibilitykeywork(oAttribute)

    '                    sCanonicalType = CanonicalType(oAttribute.Type)
    '                    sCanonicalAttributeName = CanonicalName(oAttribute.Name)

    '                    .AppendText(vbCrLf)
    '                    If oAttribute.Notes.Length > 0 Then
    '                        For Each sLine As String In Split(oAttribute.Notes, vbCrLf)
    '                            .AppendText("    // " & sLine & vbCrLf)
    '                        Next
    '                    End If
    '                    .AppendText("    public " & sCanonicalType & " " & sCanonicalAttributeName & " { get; set; }" & vbCrLf)
    '                Next

    '                For Each oConnector As EA.Connector In _oEAElement.Connectors
    '                    addAssociation(oConnector, _oEAElement)
    '                Next
    '            End With
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Sub addAssociation(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
    '        Dim sSupplierCardinality As String
    '        Dim sClientCardinality As String
    '        Dim sCardinalityComment As String = ""
    '        Dim sOtherClassName As String

    '        Try
    '            With _oSourceOutput
    '                If oConnector.Type = "Association" Then
    '                    If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
    '                        sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
    '                        sSupplierCardinality = oConnector.ClientEnd.Cardinality
    '                        sClientCardinality = oConnector.SupplierEnd.Cardinality
    '                    Else
    '                        sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
    '                        sSupplierCardinality = oConnector.SupplierEnd.Cardinality
    '                        sClientCardinality = oConnector.ClientEnd.Cardinality
    '                    End If

    '                    oConnector.Name = CanonicalName(oConnector.Name)
    '                    If oConnector.Name.Length = 0 Then
    '                        Throw New ApplicationException("The relationship between '" & sOtherClassName & "' and '" & oClass.Name & "' needs a name (e.g., R1)")
    '                    End If

    '                    Select Case sSupplierCardinality
    '                        Case "1", "0..1"
    '                            .AppendText("    public " & sOtherClassName & " " & oConnector.Name & "_" & sOtherClassName & ";     // referential attribute" & vbCrLf)

    '                        Case "0..*", "1..*"
    '                            .AppendText("    public List<" & sOtherClassName & "> " & oConnector.Name & "_" & sOtherClassName & "_List = new List<" & sOtherClassName & ">();     // referential attribute" & vbCrLf)

    '                        Case Else
    '                            If sSupplierCardinality.Length > 0 Then
    '                                Throw New ApplicationException("Unknown cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
    '                            Else
    '                                Throw New ApplicationException("No cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
    '                            End If
    '                    End Select
    '                End If
    '            End With
    '        Catch ex As Exception
    '            Dim oErrorHandler As New sjmErrorHandler(ex)
    '        End Try
    '    End Sub

    '    Private Function getAttributeVisibilitykeywork(ByVal oAttribute As EA.Attribute) As String
    '        Dim sVisibilityKeyword As String

    '        Select Case oAttribute.Visibility
    '            Case "Protected"
    '                sVisibilityKeyword = "Protected"

    '            Case "Private"
    '                sVisibilityKeyword = "Private"

    '            Case "Public"
    '                sVisibilityKeyword = "Public"

    '            Case Else
    '                Throw New ApplicationException("Unhandled visiblity case: " & oAttribute.Visibility)
    '        End Select
    '        Return sVisibilityKeyword
    '    End Function

    'End Class
End Class

