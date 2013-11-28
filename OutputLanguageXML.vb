
'____________________________________________________________________
'
' © Copyright 2009, brennan-marquez, LLC  All rights reserved.
'____________________________________________________________________
'
'     $RCSfile: OutputLanguageXML.vb,v $
'
'    $Revision: 1.5 $
'
'        $Date: 2008/10/06 20:01:44 $
'
'      $Author: thomasb $
'
'      $Source: OutputLanguageVB.vb,v $
'
'        $Name:  $
'____________________________________________________________________


Imports system.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO

Public Class OutputLanguageXML
    Implements IOutputLanguage

    Public Sub CreateDomains(ByVal _oRepository As EA.Repository, ByVal bIncludeDebug As Boolean) Implements IOutputLanguage.CreateDomains
        Try
            gStatusBox = New frmStatusBox
            gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

            Dim oRootPackage As EA.Package = _oRepository.Models.GetAt(0).Packages.GetAt(0)
            If oRootPackage.Packages.Count > 0 Then
                For Each oPackage As EA.Package In oRootPackage.Packages
                    Dim oDomain As Domain = New Domain(oPackage, _oRepository)      ' constructor does the work
                Next
                gStatusBox.FadeAway()
            End If
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Class Domain
        Private _sDomainName As String
        Private _oPackage As EA.Package
        Private _oRepository As EA.Repository
        Private _oXMLBuilder As XMLBuilder
        Private _oElementCheckoff As Collection

        Private Enum EA_TYPE               ' these are EA types which have been inferred by trial and error
            FINAL_STATE = 4
            EXIT_STATE = 14
            INITIAL_STATE = 3
            ENTRY_STATE = 13
            TERMINATE_STATE = 12
            SYNCH_STATE = 6
        End Enum

        Public Sub New(ByVal oPackage As EA.Package, ByVal oRepository As EA.Repository)
            Try
                _oPackage = oPackage
                _oRepository = oRepository

                _sDomainName = oPackage.Name
                Dim sFullFilename As String = Path.Combine(Path.GetDirectoryName(_oRepository.ConnectionString), _sDomainName & ".xml")
                _oXMLBuilder = New XMLBuilder("Domain", sFullFilename, True)
                _oXMLBuilder.SetAttribute("name", _sDomainName, ".")

                createCreationInformationElement()
                createCheckoffList()
                enumerateClasses()
                enumerateRelationships()
                enumerateStateModels()
                identifySourceSinkStates()

                _oXMLBuilder.Close()
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createCreationInformationElement()
            'With _oXMLBuilder
            '    .SetElement("Generation", ".")                Dim xDateElement As XmlElement = .SetElement("Date", "//Generation", )                Dim xDateElement As XmlElement = .SetElement("Date", "//Generation", )
            '    '' CONTINUE PRODUCTION HERE (is it easier to just use the XMLElement methods?     xDateElement.SetAttributeNode  Value = Now.ToLongDateString & ", " & Now.ToLongTimeString            End With
        End Sub

        Private Sub identifySourceSinkStates()
            Dim xState As XmlElement

            For Each xState In _oXMLBuilder.GetElements("//State")
                If xState.ChildNodes.Count = 0 Then
                    _oXMLBuilder.SetAttribute("isSink", "true", xState)
                End If
            Next

            For Each xState In _oXMLBuilder.GetElements("//State")
                Dim sStateId As String = xState.GetAttribute("id")
                Dim xSourceTransition = _oXMLBuilder.GetElement("//Transition [@targetStateId = '" & sStateId & "']")
                If xSourceTransition Is Nothing Then
                    _oXMLBuilder.SetAttribute("isSource", "true", xState)
                End If
            Next
        End Sub

        Private Sub createCheckoffList()
            _oElementCheckoff = New Collection
            For Each oElement As EA.Element In _oPackage.Elements
                Debug.WriteLine(oElement.MetaType)
                _oElementCheckoff.Add(oElement, oElement.ElementID.ToString)         ' this list is consumed as items are categorized
            Next
        End Sub

        Private Sub enumerateClasses()
            Dim sIsOmittedAttributeName As String = ""
            Dim sIsQuietAttributeName As String = ""
            Dim sIsInterfaceAttributeName As String = ""
            Dim sIsOmitted As String = ""
            Dim sIsQuiet As String = ""
            Dim sIsInterface As String = ""
            Dim sClassId As String

            Try
                For Each oElement As EA.Element In _oElementCheckoff
                    Application.DoEvents()

                    If oElement.MetaType = "Class" Then
                        eatElement(oElement)        ' remove this element from the list, it's done

                        sIsOmittedAttributeName = ""
                        sIsQuietAttributeName = ""
                        sIsInterfaceAttributeName = ""
                        sIsOmitted = ""
                        sIsQuiet = ""
                        sIsInterface = ""
                        sClassId = oElement.ElementID

                        If ElementIncludesStereotype(oElement, "omit") Then
                            sIsOmitted = "true"
                            sIsOmittedAttributeName = "isOmitted"
                        End If

                        If ElementIncludesStereotype(oElement, "interface") Then
                            sIsInterface = "true"
                            sIsInterfaceAttributeName = "isInterface"
                        End If

                        If ElementIncludesStereotype(oElement, "quiet") Then
                            sIsQuiet = "true"
                            sIsQuietAttributeName = "isQuiet"
                        End If

                        _oXMLBuilder.SetElement("Class", ".", _
                                                "name", CanonicalName(oElement.Name), _
                                                "isOmitted", sIsOmitted, _
                                                "isQuiet", sIsQuiet, _
                                                "isInterface", sIsInterface, _
                                                "id", sClassId)
                    End If
                Next

                markSubtypesSupertypes()
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub eatElement(ByVal oElement As EA.Element)
            'If _oElementCheckoff.Contains(oElement.ElementID.ToString) Then
            '    _oElementCheckoff.Remove(oElement.ElementID.ToString)
            'End If
        End Sub

        Private Function parseEventName(ByVal oConnector As Object, Optional ByRef oArgumentNames As Collection = Nothing) As String
            Dim sRawName As String = ""
            Dim sEventName As String = ""

            Try
                Select Case oConnector.ObjectType
                    Case EA.ObjectType.otConnector
                        sRawName = oConnector.TransitionEvent

                    Case EA.ObjectType.otTransition, EA.ObjectType.otElement
                        sRawName = oConnector.name

                    Case Else
                        Throw New ApplicationException("Unknown EA object type: " & oConnector.ObjectType.ToString)
                End Select

                If sRawName.Length > 0 Then
                    Dim oEventNameMatch As New SmartMatch(sRawName.Trim, "([^(]+)\(?([^)]*)")
                    oEventNameMatch.Show()

                    sEventName = oEventNameMatch.MatchGroup(0, 1)
                    Dim sArguments As String = oEventNameMatch.MatchGroup(0, 2)

                    If sArguments.Length > 0 Then
                        oArgumentNames = New Collection
                        Dim oArgumentMatch As New SmartMatch(sArguments, "([^, ]+)")
                        oArgumentMatch.Show()
                        For Each oMatch As Match In oArgumentMatch.Matches
                            Dim sArgumentName As String = oMatch.ToString
                            oArgumentNames.Add(sArgumentName)
                        Next
                    End If
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
            Return sEventName
        End Function

        Private Sub enumerateRelationships()
            Try
                For Each oElement As EA.Element In _oPackage.Elements
                    Application.DoEvents()
                    For Each oConnector As EA.Connector In oElement.Connectors
                        If oConnector.MetaType = "Association" Then
                            eatElement(oElement)        ' remove this element from the list, it's done

                            Dim sOneEndClassId As String = oConnector.SupplierID
                            Dim xOneEndClass As XmlElement = _oXMLBuilder.GetElement("//Class [@id='" & sOneEndClassId & "']")
                            Dim sOneEndCardinality As String = oConnector.SupplierEnd.Cardinality
                            Dim sOneEndRole As String = oConnector.SupplierEnd.Role

                            Dim sOtherEndClassId As String = oConnector.ClientID
                            Dim xOtherEndClass As XmlElement = _oXMLBuilder.GetElement("//Class [@id='" & sOtherEndClassId & "']")
                            Dim sOtherEndCardinality As String = oConnector.ClientEnd.Cardinality
                            Dim sOtherEndRole As String = oConnector.ClientEnd.Role

                            If oElement.ElementID = sOneEndClassId Then
                                _oXMLBuilder.SetElement("Relationship", "//Class [@name='" & oElement.Name & "']", _
                                                        "name", oConnector.Name, _
                                                        "relatedClassId", sOtherEndClassId, _
                                                        "relatedClassName", xOtherEndClass.Attributes("name").Value, _
                                                        "cardinality", sOtherEndCardinality, _
                                                        "role", sOneEndRole)
                            Else
                                _oXMLBuilder.SetElement("Relationship", "//Class [@name='" & oElement.Name & "']", _
                                                        "name", oConnector.Name, _
                                                        "relatedClassId", sOneEndClassId, _
                                                        "relatedClassName", xOneEndClass.Attributes("name").Value, _
                                                        "cardinality", sOneEndCardinality, _
                                                        "role", sOtherEndRole)
                            End If
                        End If
                    Next
                Next
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub markSubtypesSupertypes()
            Dim sTokens() As String
            Dim bIsSupertype As Boolean
            Dim bIsSubtype As Boolean

            Try
                For Each oElement As EA.Element In _oPackage.Elements
                    Application.DoEvents()
                    sTokens = Split(oElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
                    bIsSupertype = (sTokens(0).Length > 0)

                    sTokens = Split(oElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
                    bIsSubtype = (sTokens(0).Length > 0)

                    If bIsSubtype Then
                        _oXMLBuilder.SetAttribute("isSubtype", "true", "//Class [@id='" & oElement.ElementID & "']")
                    End If

                    If bIsSupertype Then
                        _oXMLBuilder.SetAttribute("isSupertype", "true", "//Class [@id='" & oElement.ElementID & "']")
                    End If
                Next
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub enumerateStateModels()
            addStateMachines()
            addStates()
            addEvents()
            addTransitions()
        End Sub

        Private Sub addTransitions()
            Dim bAlreadyExists As Boolean
            Dim xEvent As XmlElement = Nothing
            Dim sTriggerEventId As String

            Try
                With _oXMLBuilder
                    For Each oElement As EA.Element In _oPackage.Elements
                        Application.DoEvents()

                        For Each oConnector As EA.Connector In oElement.Connectors
                            If oConnector.MetaType = "Transition" Then
                                Dim sThisStateId As String = oConnector.ClientID.ToString
                                Dim sOtherStateId As String = oConnector.SupplierID.ToString
                                Dim sTriggerEventName As String = parseEventName(oConnector)

                                If sTriggerEventName.Length > 0 Then
                                    xEvent = .GetElement("//Event [@name='" & sTriggerEventName & "']")
                                    sTriggerEventId = xEvent.GetAttribute("id")
                                Else
                                    sTriggerEventId = "none"
                                End If

                                bAlreadyExists = .ContainsElement("//State [@id='" & sThisStateId & "']/Transition [@triggerEventId='" & sTriggerEventId & "']")
                                If Not bAlreadyExists Then             ' this transition has not yet been added
                                    Dim xOtherEndState As XmlElement = _oXMLBuilder.GetElement("//State [@id='" & sOtherStateId & "']")

                                    Dim xTransition As XmlElement
                                    xTransition = .SetElement("Transition", "//State [@id='" & sThisStateId & "']", _
                                                              "targetStateId", sOtherStateId, _
                                                              "targetStateName", xOtherEndState.GetAttribute("name").ToString)
                                    'If sTriggerEventName.Length > 0 Then
                                    .SetAttribute("triggerEventId", sTriggerEventId, xTransition)
                                    .SetAttribute("triggerEventName", sTriggerEventName, xTransition)
                                    'Else

                                    'End If

                                End If
                            End If
                        Next
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addEvents()
            Try
                With _oXMLBuilder
                    For Each oElement As EA.Element In _oPackage.Elements
                        Application.DoEvents()

                        If oElement.MetaType = "Trigger" Then
                            eatElement(oElement)        ' remove this element from the list, it's done

                            Dim oArguments As Collection = Nothing
                            Dim sEventName As String = ""

                            sEventName = parseEventName(oElement, oArguments)

                            Dim xEvent As XmlElement = .SetElement("Event", "//StateMachine [@id='" & oElement.ParentID & "']", _
                                                                   "id", oElement.ElementID, _
                                                                   "name", sEventName)
                            If oArguments IsNot Nothing Then
                                For Each sArgumentName As String In oArguments
                                    .SetElement("Argument", xEvent, _
                                                "name", sArgumentName)
                                Next
                            End If
                        End If
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addStateMachines()
            Try
                With _oXMLBuilder
                    For Each oElement As EA.Element In _oPackage.Elements
                        Application.DoEvents()

                        If oElement.MetaType = "StateMachine" Then
                            .SetElement("StateMachine", "//Class [@id='" & oElement.ParentID & "']", "id", oElement.ElementID)
                        End If
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addStates()
            Dim xNewPseudoState As XmlElement

            Try
                With _oXMLBuilder
                    For Each oElement As EA.Element In _oPackage.Elements
                        Application.DoEvents()
                        Select Case oElement.MetaType
                            Case "FinalState"
                                eatElement(oElement)
                                .SetElement("State", "//StateMachine [@id='" & oElement.ParentID & "']", _
                                            "id", oElement.ElementID, _
                                            "name", oElement.Name, _
                                            "isFinal", "true")

                            Case "Pseudostate"
                                eatElement(oElement)
                                xNewPseudoState = .SetElement("State", "//StateMachine [@id='" & oElement.ParentID & "']", _
                                                              "id", oElement.ElementID, _
                                                              "name", oElement.Name)
                                If oElement.Subtype = EA_TYPE.INITIAL_STATE Then    ' found the 'meatball' 
                                    xNewPseudoState.SetAttribute("isInitialPseudoState", "true")
                                End If

                            Case "State"
                                eatElement(oElement)
                                .SetElement("State", "//StateMachine [@id='" & oElement.ParentID & "']", _
                                            "id", oElement.ElementID, _
                                            "name", oElement.Name)

                        End Select
                    Next
                End With

            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        'Private Sub catalogElements()
        '    Dim oElement As EA.Element
        '    Dim oErrorHandler As New sjmErrorHandler

        '    Try
        '        Application.DoEvents()
        '        For Each oElement In _oPackage.Elements
        '            Application.DoEvents()

        '            If oElement.Name.Length = 0 Then
        '                oElement.Name = "NoName_" & oElement.ElementID
        '            End If
        '            oElement.Name = CanonicalName(oElement.Name, True)            ' establish safe names right off the bat (rather than sprinkling everywehre)

        '            Select Case oElement.MetaType
        '                Case "StateMachine"
        '                    _StateMachines.Add(oElement, oElement.ElementID)

        '                Case "FinalState"
        '                    _States.Add(oElement, oElement.ElementID)

        '                Case "Pseudostate"
        '                    _States.Add(oElement, oElement.ElementID)

        '                Case "Trigger"
        '                    oErrorHandler.SupplementalInformation = "_Triggers: " & oElement.Name                               ' in case an exception is thrown
        '                    If Not _Triggers.Contains(oElement.Name) Then   ' event names may be reused between state machines
        '                        _Triggers.Add(oElement, oElement.Name)
        '                    End If

        '                Case "Class"
        '                    oErrorHandler.SupplementalInformation = "_ClassById: " & oElement.Name                              ' in case an exception is thrown
        '                    _ClassById.Add(oElement, oElement.ElementID)
        '                    sortByStereoType(oElement)

        '                Case "State"
        '                    recordStateElement(oElement)

        '                Case "Text"
        '                    ' do nothing with this type, but don't throw a warning that it is unknown

        '                Case Else
        '                    Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
        '            End Select
        '        Next
        '    Catch ex As Exception
        '        oErrorHandler.Announce(ex)
        '    End Try
        'End Sub



    End Class










    '    Private Enum EA_TYPE               ' these are EA types (see the EA help topic "Type" which have been inferred by trial and error
    '        FINAL_STATE = 4
    '        EXIT_STATE = 14
    '        INITIAL_STATE = 3
    '        ENTRY_STATE = 13
    '        TERMINATE_STATE = 12
    '        SYNCH_STATE = 6
    '    End Enum

    '    Private Const CONSTANTS_COLUMN = 70
    '    Private Const COLUMN_WIDTH As Integer = 55

    '    Private _oInterfaces As New Collection
    '    Private _lblOutputFilename As Label

    '    Public Shared Function GetStateTypeName(ByVal oState As EA.Element) As String
    '        Dim sName As String = "Normal"
    '        Static oStateTypeToName As Collection

    '        If oStateTypeToName Is Nothing Then
    '            oStateTypeToName = New Collection
    '            With oStateTypeToName
    '                .Add("Final", CStr(EA_TYPE.FINAL_STATE))
    '                .Add("Exit", CStr(EA_TYPE.EXIT_STATE))
    '                .Add("Initial", CStr(EA_TYPE.INITIAL_STATE))
    '                .Add("Entry", CStr(EA_TYPE.ENTRY_STATE))
    '                .Add("Terminate", CStr(EA_TYPE.TERMINATE_STATE))
    '                .Add("Synch", CStr(EA_TYPE.SYNCH_STATE))
    '            End With
    '        End If

    '        If oStateTypeToName.Contains(oState.Subtype.ToString) Then
    '            sName = oStateTypeToName(oState.Subtype.ToString)
    '        End If

    '        Return sName
    '    End Function

    '    Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef oTextbox As RichTextBox)

    '        If oTextbox.Text.Length > 0 Then
    '            OutputFile.ClearFilesCreated()
    '            Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
    '            With oOutputFile
    '                .Add("' ________________________________________________________________________________")
    '                .Add("' ")
    '                .Add("'          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
    '                .Add("' ________________________________________________________________________________")
    '                .Add("' ")
    '                .Add("'               File: " & sOutputFilename)
    '                .Add("' ")
    '                .Add("'         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
    '                .Add("' ")
    '                .Add("'          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
    '                .Add("' ")
    '                .Add("' ________________________________________________________________________________")
    '                .Add("' ")
    '                .Add("'           Copyright © 2009,  brennan-marquez, LLC   All rights reserved.")
    '                .Add("' ________________________________________________________________________________")
    '                .Add("")
    '                .Add("")
    '                .Add("Imports Microsoft.VisualBasic")
    '                .Add("Imports ArchitecturalSupport")
    '                .Add("Imports ArchitecturalSupport.XUnit.Framework")
    '                .Add("")
    '                .Add(oTextbox.Text)
    '            End With

    '            oOutputFile.Close()
    '        End If
    '    End Sub


    '    Protected Class Domain
    '        Private _oTestFixtureElement As EA.Element
    '        Private _TestElements As Collection
    '        Private _oSourceOutput As RichTextBox
    '        Private _oRepository As EA.Repository
    '        Private _sPackageId As String
    '        Private _oProject As EA.Project
    '        Private _oPackage As EA.Package
    '        Private _ClassById As Collection
    '        Private _Triggers As Collection
    '        Private _States As Collection
    '        Private _Notes As Collection
    '        Private _Boundarys As Collection
    '        Private _StateMachines As Collection
    '        Private _IsRealized As Boolean
    '        Private _Name As String
    '        Private _ElementById As Collection

    '        Public ReadOnly Property Name() As String
    '            Get
    '                Return _Name
    '            End Get
    '        End Property

    '        Public ReadOnly Property TestFixtureElement() As EA.Element
    '            Get
    '                Return _oTestFixtureElement
    '            End Get
    '        End Property

    '        Public ReadOnly Property TestElements() As Collection
    '            Get
    '                Return _TestElements
    '            End Get
    '        End Property

    '        Public ReadOnly Property Notes() As Collection
    '            Get
    '                Return _Notes
    '            End Get
    '        End Property

    '        Public ReadOnly Property Boundarys() As Collection
    '            Get
    '                Return _Boundarys
    '            End Get
    '        End Property

    '        Public ReadOnly Property StateMachines() As Collection
    '            Get
    '                Return _StateMachines
    '            End Get
    '        End Property

    '        Public ReadOnly Property ElementById() As Collection
    '            Get
    '                Return _ElementById
    '            End Get
    '        End Property

    '        Public ReadOnly Property States() As Collection
    '            Get
    '                Return _States
    '            End Get
    '        End Property

    '        Public ReadOnly Property EAClass(ByVal iID As Integer) As EA.Element
    '            Get
    '                Dim oClass As EA.Element = Nothing

    '                If _ClassById.Contains(iID.ToString) Then
    '                    oClass = _ClassById.Item(iID.ToString)
    '                Else
    '                    MsgBox("Unknown class id: " & iID, MsgBoxStyle.Critical)
    '                End If
    '                Return oClass
    '            End Get
    '        End Property

    '        Public ReadOnly Property IsRealized() As Boolean
    '            Get
    '                Return _IsRealized
    '            End Get
    '        End Property

    '        Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository, ByRef oSourceOutput As RichTextBox)
    '            Try
    '                giNextStateID = oPackage.Name.GetHashCode
    '                giNextEventID = giNextStateID

    '                _oSourceOutput = oSourceOutput
    '                _oRepository = oRepository
    '                _oPackage = oPackage
    '                _sPackageId = oPackage.PackageID
    '                _Name = CanonicalName(oPackage.Name)

    '                _oTestFixtureElement = Nothing

    '                _Boundarys = New Collection
    '                _Notes = New Collection
    '                _TestElements = New Collection
    '                _ClassById = New Collection
    '                _Triggers = New Collection
    '                _States = New Collection
    '                _ElementById = New Collection
    '                _StateMachines = New Collection

    '                _IsRealized = PackageIncludesStereotype(_oPackage, "realized")

    '                If Not _IsRealized Then
    '                    With _oSourceOutput
    '                        catalogElements()

    '                        generateSource()

    '                        addStateConstants()

    '                        addEventConstants()

    '                        .AppendText("End Namespace" & vbCrLf)

    '                        addInterfaceClasses()
    '                    End With
    '                End If
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub addStateConstants()
    '            Dim oState As EA.Element
    '            Dim iNextStateId As Integer
    '            Dim oUniqueBucket As Collection

    '            Application.DoEvents()
    '            With _oSourceOutput
    '                .AppendText(vbCrLf)

    '                iNextStateId = giNextStateID            ' set to our first state id
    '                .AppendText("Public Module " & _Name & "_States" & vbCrLf)

    '                oUniqueBucket = New Collection
    '                For Each oState In _States
    '                    If IsUnique(oState.Name, oUniqueBucket) Then
    '                        .AppendText(("   Public Const STATE_" & CanonicalName(CanonicalName(oState.Name)) & " As Long = ").PadRight(CONSTANTS_COLUMN) & iNextStateId & vbCrLf)
    '                        iNextStateId += 1                   ' inc the local ID number as well (these will track through this loop)
    '                    End If
    '                Next
    '                .AppendText(vbCrLf)

    '                iNextStateId = giNextStateID            ' reset back to our first state id (to get IDs aligned)
    '                .AppendText("   Public sub RegisterStateNames()" & vbCrLf)
    '                .AppendText("      Static bRegistrationComplete As Boolean = False" & vbCrLf)
    '                .AppendText(vbCrLf)
    '                .AppendText("      If Not bRegistrationComplete Then" & vbCrLf)
    '                .AppendText("          bRegistrationComplete = True" & vbCrLf)

    '                oUniqueBucket = New Collection
    '                For Each oState In _States
    '                    If IsUnique(oState.Name, oUniqueBucket) Then
    '                        .AppendText(("            RegisterStateNameString( """ & CanonicalName(oState.Name) & """, """ & iNextStateId & """) ") & vbCrLf)
    '                        iNextStateId += 1
    '                    End If
    '                Next
    '                .AppendText("        End If" & vbCrLf)
    '                .AppendText("    End Sub" & vbCrLf)
    '                .AppendText("End Module" & vbCrLf)
    '                .AppendText(vbCrLf)

    '                giNextStateID = iNextStateId            ' update the global ID holder
    '            End With
    '        End Sub

    '        Private Sub addEventConstants()
    '            Dim sEvent As String
    '            Dim iNextEventID As Integer
    '            Dim oEvents As New Collection
    '            Dim oState As EA.Element
    '            Dim oConnector As EA.Connector
    '            Dim sTokens() As String

    '            Application.DoEvents()
    '            With _oSourceOutput
    '                iNextEventID = giNextEventID                        ' set ID starting point
    '                For Each oState In _States
    '                    For Each oConnector In oState.Connectors
    '                        Application.DoEvents()
    '                        sEvent = canonicalEventName(oConnector)
    '                        sTokens = Split(sEvent, ":")                ' discard any "ev1:" type prefixes
    '                        sEvent = sTokens(0)
    '                        sTokens = Split(sEvent, "(")                ' discard any parameters supplied
    '                        sEvent = CanonicalName(sTokens(0), True)
    '                        If sEvent.Length > 0 Then
    '                            If Not oEvents.Contains(sEvent) Then
    '                                oEvents.Add(sEvent, sEvent)
    '                            End If
    '                        End If
    '                    Next
    '                Next
    '                .AppendText(vbCrLf)

    '                iNextEventID = giNextEventID                        ' reset back to starting point
    '                .AppendText("Public Module " & _Name & "_Events" & vbCrLf)
    '                For Each sEvent In oEvents
    '                    .AppendText(("   Public Const EVENT_" & sEvent & " As Long = ").PadRight(CONSTANTS_COLUMN) & iNextEventID & vbCrLf)
    '                    iNextEventID += 1
    '                Next
    '                .AppendText(vbCrLf)

    '                iNextEventID = giNextEventID                        ' reset back to starting point
    '                .AppendText("    Public sub RegisterEventNames()" & vbCrLf)
    '                .AppendText("        Static bRegistrationComplete As Boolean = False" & vbCrLf)
    '                .AppendText(vbCrLf)
    '                .AppendText("        If Not bRegistrationComplete Then" & vbCrLf)
    '                .AppendText("            bRegistrationComplete = True" & vbCrLf)
    '                For Each sEvent In oEvents
    '                    .AppendText(("            RegisterEventNameString( """ & sEvent & """, """ & iNextEventID & """) ") & vbCrLf)
    '                    iNextEventID += 1
    '                Next
    '                .AppendText("        End If" & vbCrLf)
    '                .AppendText("    End Sub" & vbCrLf)
    '                .AppendText("End Module" & vbCrLf)
    '                .AppendText(vbCrLf)

    '                giNextEventID = iNextEventID            ' bump the global ID holder to account for the IDs we used
    '            End With
    '        End Sub

    '        Private Sub catalogElements()
    '            Dim oElement As EA.Element
    '            Dim oErrorHandler As New sjmErrorHandler

    '            Try
    '                Application.DoEvents()

    '                For Each oElement In _oPackage.Elements
    '                    Application.DoEvents()
    '                    oElement.Name = CanonicalName(oElement.Name, True)            ' establish safe names right off the bat (rather than sprinkling everywehre)
    '                    _ElementById.Add(oElement, oElement.ElementID)              ' just as a debugging convenience, to look up any element from its id only

    '                    If oElement.Name.Length = 0 Then
    '                        oElement.Name = "NoName_" & oElement.ElementID
    '                    End If

    '                    Select Case oElement.MetaType
    '                        Case "StateMachine"
    '                            _StateMachines.Add(oElement, oElement.ElementID)

    '                        Case "FinalState"
    '                            _States.Add(oElement, oElement.ElementID)

    '                        Case "Pseudostate"
    '                            _States.Add(oElement, oElement.ElementID)

    '                        Case "Trigger"
    '                            oErrorHandler.SupplementalInformation = "_Triggers: " & oElement.Name                               ' in case an exception is thrown
    '                            If Not _Triggers.Contains(oElement.Name) Then   ' event names may be reused between state machines
    '                                _Triggers.Add(oElement, oElement.Name)
    '                            End If

    '                        Case "Class"
    '                            oErrorHandler.SupplementalInformation = "_ClassById: " & oElement.Name                              ' in case an exception is thrown
    '                            _ClassById.Add(oElement, oElement.ElementID)
    '                            sortByStereoType(oElement)

    '                        Case "State"
    '                            recordStateElement(oElement)

    '                        Case "Text"
    '                            ' do nothing with this type, but don't throw a warning that it is unknown

    '                        Case Else
    '                            Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
    '                    End Select
    '                Next
    '            Catch ex As Exception
    '                oErrorHandler.Announce(ex)
    '            End Try
    '        End Sub

    '        Private Sub sortByStereoType(ByVal oElement As EA.Element)
    '            If oElement.Stereotype.ToUpper.Contains("TESTFIXTURE") Then
    '                If _oTestFixtureElement Is Nothing Then
    '                    _oTestFixtureElement = oElement
    '                Else
    '                    Throw New ApplicationException("Only a single test fixture is allowed per domain, but both '" & _oTestFixtureElement.Name & "' and '" & oElement.Name & "' were found")
    '                End If
    '            Else
    '                If ElementIncludesStereotype(oElement, "test") Then
    '                    _TestElements.Add(oElement, oElement.Name)
    '                End If
    '            End If

    '        End Sub

    '        Private Sub recordStateElement(ByVal oElement As EA.Element)
    '            Dim oErrorHandler As New sjmErrorHandler

    '            oErrorHandler.SupplementalInformation = "_States (State): " & oElement.Name                         ' in case an exception is thrown
    '            'If _StatesNames.Contains(oElement.Name) Then
    '            '    Throw New ApplicationException("State name '" & oElement.Name & "' is not unique within the model")
    '            'End If
    '            '_StatesNames.Add(oElement, oElement.Name)
    '            _States.Add(oElement, oElement.ElementID)
    '        End Sub

    '        Private Sub fileFooter(ByVal oPackage As EA.Package)
    '            With _oSourceOutput
    '                .AppendText(vbCrLf)
    '                .AppendText("" & vbCrLf)
    '                .AppendText(vbCrLf)
    '            End With
    '        End Sub

    '        Private Sub addInterfaceClasses()
    '            Dim oClassElement As EA.Element
    '            Dim oEAClass As InterfaceClass
    '            Static iClassCounter As Integer = 0

    '            Try
    '                With _oSourceOutput
    '                    For Each oClassElement In _ClassById
    '                        Application.DoEvents()
    '                        If _sPackageId = oClassElement.PackageID Then
    '                            If ElementIncludesStereotype(oClassElement, "interface") Then
    '                                oEAClass = New InterfaceClass(oClassElement, Me, _oSourceOutput)
    '                            End If
    '                        End If
    '                        iClassCounter += 1
    '                    Next
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub generateSource()
    '            Dim oClassElement As EA.Element
    '            Dim oEAClass As EAClass
    '            Static iClassCounter As Integer = 0

    '            Try
    '                Application.DoEvents()
    '                With _oSourceOutput
    '                    .AppendText(vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText("Namespace " & _Name & vbCrLf)

    '                    .AppendText("")
    '                    If _oPackage.Notes.Length > 0 Then
    '                        .AppendText("   ' " & _oPackage.Notes)
    '                    End If
    '                    .AppendText(vbCrLf)

    '                    gStatusBox.ProgressValueMaximum = _ClassById.Count
    '                    For Each oClassElement In _ClassById
    '                        gStatusBox.ProgressValue = iClassCounter
    '                        Application.DoEvents()
    '                        If _sPackageId = oClassElement.PackageID Then
    '                            If Not ElementIncludesStereotype(oClassElement, "omit") Then
    '                                oEAClass = New EAClass(oClassElement, Me, _oSourceOutput)
    '                            Else
    '                                Debug.WriteLine("Omitting class (by stereotype 'omit'): " & oClassElement.Name)
    '                            End If
    '                        End If
    '                        iClassCounter += 1
    '                    Next
    '                    fileFooter(_oPackage)
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub
    '    End Class

    '    Private Class EAClass
    '        Private _oSourceOutput As RichTextBox
    '        Private _oDomain As Domain
    '        Private _bFinalStateReported As Boolean = False
    '        Private _oEvents As New Collection
    '        Private _sInitialState As String = ""
    '        Private _bActiveClass As Boolean = False
    '        Private _sParentClassName As String = ""
    '        Private _bIsSupertype As Boolean
    '        Private _bIsSubtype As Boolean
    '        Const COMMENT_START_COLUMN = 40

    '        Public Sub New(ByVal oClassElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
    '            Dim bIsTestFixture As Boolean = False
    '            Dim bIsTest As Boolean = False
    '            Dim sTokens() As String

    '            Try
    '                _oDomain = oDomain
    '                _oSourceOutput = oSourceOutput

    '                If _oDomain.TestFixtureElement IsNot Nothing Then
    '                    bIsTestFixture = (_oDomain.TestFixtureElement.Name = oClassElement.Name)
    '                End If
    '                bIsTest = _oDomain.TestElements.Contains(oClassElement.Name)

    '                oClassElement.Name = CanonicalName(oClassElement.Name)      ' be sure the class name is in a legal form

    '                sTokens = Split(oClassElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
    '                _bIsSupertype = (sTokens(0).Length > 0)
    '                sTokens = Split(oClassElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
    '                If (sTokens(0).Length > 0) Then
    '                    _sParentClassName = CanonicalName(_oDomain.EAClass(sTokens(0)).Name)
    '                End If

    '                _bIsSubtype = (_sParentClassName.Length > 0)        ' this class has a parent class name iff it is a subtype

    '                With _oSourceOutput
    '                    .AppendText(vbCrLf)
    '                    .AppendText("' _____________________________________________________________________________________" & vbCrLf)
    '                    .AppendText("' _ " & vbCrLf)
    '                    .AppendText("' _" & ("                                    `" & oClassElement.Name).PadLeft(84) & vbCrLf)
    '                    .AppendText("' _____________________________________________________________________________________" & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    If _bIsSupertype Then
    '                        .AppendText("Friend MustInherit Class " & oClassElement.Name & vbCrLf)
    '                    Else
    '                        If bIsTestFixture Then
    '                            .AppendText("<NUnit.Framework.TestFixture()> _" & vbCrLf)
    '                        End If
    '                        .AppendText("Friend Class " & oClassElement.Name & vbCrLf)
    '                    End If

    '                    If _bIsSubtype Then            ' if this class is a subtype
    '                        .AppendText("    Inherits " & _sParentClassName & vbCrLf)
    '                    Else
    '                        .AppendText("    Inherits XClass" & vbCrLf)
    '                    End If

    '                    If bIsTestFixture Then
    '                        If _oDomain.TestElements.Count > 0 Then
    '                            .AppendText(vbCrLf)
    '                            .AppendText(vbCrLf)
    '                            .AppendText("        ' NUnit test entry points")
    '                            .AppendText(vbCrLf)
    '                            For Each oTestElement As EA.Element In _oDomain.TestElements
    '                                oTestElement.Name = CanonicalName(oTestElement.Name)

    '                                .AppendText(vbCrLf)
    '                                .AppendText("        <NUnit.Framework.Test()> _" & vbCrLf)
    '                                addCustomAttributes(oTestElement)
    '                                .AppendText("        Public Sub " & oTestElement.Name & " ()" & vbCrLf)
    '                                .AppendText("            Assert.TestBegin()" & vbCrLf)
    '                                .AppendText("            XEvent.Send(EVENT_Go, New " & oTestElement.Name & "(""o" & oTestElement.Name & """), ""start " & oTestElement.Name & " running"")" & vbCrLf)
    '                                .AppendText("            Assert.TestEnd()" & vbCrLf)
    '                                .AppendText("        End Sub" & vbCrLf)
    '                            Next
    '                        End If
    '                    End If

    '                    .AppendText(vbCrLf)
    '                    .AppendText("    Private Shared _oInstances As New Collection " & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    If _bIsSubtype Then
    '                        .AppendText("    Protected Overloads Sub deleteSelf()" & vbCrLf)
    '                    Else
    '                        .AppendText("    Protected Sub deleteSelf()" & vbCrLf)
    '                    End If
    '                    .AppendText("        On Error Resume Next  ' don't worry if the instance has already been removed" & vbCrLf)
    '                    .AppendText("        _oInstances.Remove(_sInstanceName)" & vbCrLf)
    '                    .AppendText("    End Sub" & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    addEvents(oClassElement)

    '                    addSynchronousCalls(oClassElement)

    '                    addStateMachine(oClassElement)

    '                    addAttributes(oClassElement)

    '                    .AppendText("End Class " & vbCrLf)
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub addCustomAttributes(ByVal oTestElement As EA.Element)
    '            With _oSourceOutput
    '                If ElementIncludesStereotype(oTestElement, "manual") Then
    '                    .AppendText("        <NUnit.Framework.Category(""Manual"")> _" & vbCrLf)
    '                Else
    '                    .AppendText("        <NUnit.Framework.Category(""Automated"")> _" & vbCrLf)
    '                End If

    '                If ElementIncludesStereotype(oTestElement, "development") Then
    '                    .AppendText("        <NUnit.Framework.Category(""UnderDevelopment"")> _" & vbCrLf)
    '                End If
    '            End With
    '        End Sub

    '        Private Sub addEvents(ByVal oClass As EA.Element)
    '            Dim oMethod As EA.Method
    '            Dim oParameter As EA.Parameter
    '            Dim sLeadingComma As String = ""

    '            With _oSourceOutput
    '                For Each oMethod In oClass.Methods
    '                    Application.DoEvents()
    '                    If oMethod.Abstract Then            ' borrowing the 'abstract' designator to indicate server->client events
    '                        Application.DoEvents()
    '                        sLeadingComma = ""
    '                        .AppendText("    Public Event " & oMethod.Name & "(")
    '                        For Each oParameter In oMethod.Parameters
    '                            oParameter.Name = CanonicalName(oParameter.Name, True)
    '                            oParameter.Type = canonicalType(oParameter.Type)
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & canonicalType(oParameter.Type))
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")")
    '                        If oMethod.Notes.Length > 0 Then
    '                            .AppendText("      ' " & oMethod.Notes)
    '                        End If
    '                        .AppendText(vbCrLf)

    '                        sLeadingComma = ""
    '                        .AppendText("    Public Sub RaiseEvent_" & oMethod.Name & "(")
    '                        For Each oParameter In oMethod.Parameters
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")" & vbCrLf)
    '                        sLeadingComma = ""
    '                        .AppendText("        RaiseEvent " & oMethod.Name & "(")
    '                        For Each oParameter In oMethod.Parameters
    '                            .AppendText(sLeadingComma & oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")" & vbCrLf)
    '                        .AppendText("    End Sub" & vbCrLf)
    '                        .AppendText(vbCrLf)
    '                    End If
    '                Next
    '                .AppendText(vbCrLf)
    '            End With
    '        End Sub

    '        Private Sub addSynchronousCalls(ByVal oClass As EA.Element)
    '            Dim oMethod As EA.Method
    '            Dim oParameter As EA.Parameter
    '            Dim sLeadingComma As String = ""
    '            Dim oOptionalParameters As Collection
    '            Dim oRequiredParameters As Collection
    '            Dim sBehavior As String
    '            Dim sStaticString As String

    '            With _oSourceOutput
    '                For Each oMethod In oClass.Methods
    '                    Application.DoEvents()

    '                    If oMethod.IsStatic Then
    '                        sStaticString = "Shared "
    '                    Else
    '                        sStaticString = ""
    '                    End If

    '                    If oMethod.Behavior.Length > 0 Then
    '                        sBehavior = StripRichTextFormat(oMethod.Behavior)
    '                    Else
    '                        Dim sMessage As String = """Method not yet implemented: " & _oDomain.Name & "." & oClass.Name & "." & oMethod.Name & """"
    '                        sBehavior = "            NUnit.Framework.Assert.Ignore(" & sMessage & ")" & vbCrLf & _
    '                        "            Console.Writeline(" & sMessage & ")"
    '                    End If

    '                    oMethod.Name = CanonicalName(oMethod.Name, True)
    '                    If Not oMethod.Abstract Then            ' borrowing the 'abstract' designator to indicate server->client events
    '                        sLeadingComma = ""

    '                        oMethod.ReturnType = canonicalType(oMethod.ReturnType)

    '                        If MethodIncludesStereotype(oMethod, "setup") Then
    '                            .AppendText("        <NUnit.Framework.Setup()> _" & vbCrLf)
    '                            If oMethod.ReturnType.Length > 0 Then           ' if there is a return type (other than 'void')
    '                                Throw New ApplicationException("An NUnit setup method cannot return a value: " & oMethod.Name)
    '                            Else
    '                                .AppendText("        Public " & sStaticString & "Sub " & oMethod.Name & "(")
    '                            End If
    '                        Else
    '                            If oMethod.ReturnType.Length > 0 Then           ' if there is a return type (other than 'void')
    '                                .AppendText("        Public Shared Function " & oMethod.Name & "(")
    '                            Else
    '                                .AppendText("        Public Shared Sub " & oMethod.Name & "(")
    '                            End If
    '                        End If

    '                        oOptionalParameters = New Collection
    '                        oRequiredParameters = New Collection
    '                        For Each oParameter In oMethod.Parameters
    '                            Application.DoEvents()
    '                            oParameter.Name = CanonicalName(oParameter.Name)
    '                            oParameter.Type = canonicalType(oParameter.Type)
    '                            If oParameter.Default.Length > 0 Then           ' parameters with default values are assumed to be optional
    '                                oParameter.Default = Regex.Replace(oParameter.Default.ToString.Trim, "null", "Nothing", RegexOptions.IgnoreCase)
    '                                oOptionalParameters.Add(oParameter)
    '                            Else
    '                                oRequiredParameters.Add(oParameter)
    '                            End If
    '                        Next

    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
    '                            sLeadingComma = ", "
    '                        Next

    '                        For Each oParameter In oOptionalParameters
    '                            .AppendText(sLeadingComma & "Optional " & oParameter.Name & " As " & oParameter.Type & " = " & oParameter.Default)
    '                            sLeadingComma = ", "
    '                        Next

    '                        If oMethod.ReturnType.Length > 0 Then           ' if there is a return type
    '                            .AppendText(") As " & oMethod.ReturnType & vbCrLf)
    '                            .AppendText(sBehavior & vbCrLf)
    '                            .AppendText("        End Function" & vbCrLf & vbCrLf)
    '                        Else
    '                            .AppendText(")" & vbCrLf)
    '                            .AppendText(sBehavior & vbCrLf)
    '                            .AppendText("        End Sub" & vbCrLf & vbCrLf)
    '                        End If
    '                    End If
    '                Next
    '            End With
    '        End Sub

    '        Private Function getClassStateMachineID(ByVal oClass As EA.Element)
    '            Dim iStateMachineID As Integer = oClass.ElementID

    '            For Each oStateMachineElement As EA.Element In _oDomain.StateMachines
    '                Application.DoEvents()
    '                If oClass.ElementID = oStateMachineElement.ParentID Then
    '                    iStateMachineID = oStateMachineElement.ElementID
    '                    Exit For
    '                End If
    '            Next
    '            Return iStateMachineID
    '        End Function

    '        Private Sub addStateMachine(ByVal oClass As EA.Element)
    '            Dim oState As EA.Element
    '            Dim oConnector As EA.Connector
    '            Dim sArgumentList As String = ""
    '            Dim bStateMachineHeaderAdded As Boolean = False
    '            Dim oSupplierState As EA.Element = Nothing
    '            Dim oPopulateTransitions As New Collection
    '            Dim sTransitionLine As String
    '            Dim sDomainName As String = _oDomain.Name
    '            Dim Pigtails As New Collection
    '            Dim sPigtail As String
    '            Dim iClassStateMachineID As Integer
    '            Dim oStateNameUniqueBucket As New Collection

    '            Try
    '                iClassStateMachineID = getClassStateMachineID(oClass)

    '                If _oDomain.States.Count > 0 Then
    '                    For Each oState In _oDomain.States
    '                        If oState.ParentID = iClassStateMachineID Then
    '                            _bActiveClass = True
    '                            Exit For
    '                        End If
    '                    Next

    '                    If _bActiveClass Then               ' if this class has a state machine
    '                        With _oSourceOutput
    '                            .AppendText("        Public Overrides Sub DispatchEvent(ByRef oEvent As XEvent, ByVal bIsSelfDirectedEvent As Boolean)" & vbCrLf)
    '                            .AppendText("            Dim currentStateAtEntry As Integer = _currentState" & vbCrLf)
    '                            .AppendText("            Dim sSelfIcon As String = """"" & vbCrLf)
    '                            .AppendText("            Dim bQuiet As Boolean = False" & vbCrLf)
    '                            .AppendText(vbCrLf)
    '                            .AppendText("            If bIsSelfDirectedEvent Then" & vbCrLf)
    '                            .AppendText("                sSelfIcon = ""[""" & vbCrLf)
    '                            .AppendText("            End If" & vbCrLf)
    '                            .AppendText(vbCrLf)
    '                            .AppendText("            initialStateSetup()              ' execute any custom intiailzation code needed by the state machine" & vbCrLf)
    '                            addNormalPigtailTransition(Pigtails, iClassStateMachineID)
    '                            If Pigtails.Count > 0 Then
    '                                .AppendText("            Select Case (oEvent.EventID)           ' pigtail transitions (from any state to the target state)" & vbCrLf)
    '                                For Each sPigtail In Pigtails
    '                                    Application.DoEvents()
    '                                    .AppendText(sPigtail)
    '                                Next
    '                                .AppendText("                Case Else" & vbCrLf)
    '                            End If
    '                            .AppendText("                    Select Case (_currentState)" & vbCrLf)
    '                            For Each oState In _oDomain.States
    '                                Application.DoEvents()
    '                                If oState.ParentID = iClassStateMachineID Then
    '                                    oPopulateTransitions = New Collection

    '                                    For Each oConnector In oState.Connectors
    '                                        Application.DoEvents()
    '                                        addNormalTransition(oConnector, oState, sArgumentList, oPopulateTransitions, oClass)
    '                                    Next

    '                                    If oPopulateTransitions.Count > 0 Then
    '                                        .AppendText("                        Case STATE_" & CanonicalName(oState.Name) & vbCrLf)
    '                                        .AppendText("                            Select Case (oEvent.EventID)" & vbCrLf)
    '                                        For Each sTransitionLine In oPopulateTransitions
    '                                            .AppendText(sTransitionLine)
    '                                        Next
    '                                        .AppendText("                                Case Else" & vbCrLf)
    '                                        .AppendText("                                    CannotHappenErrorHandler(""" & CanonicalName(oClass.Name) & """, Me, _currentState, oEvent)" & vbCrLf)
    '                                        .AppendText("                            End Select " & vbCrLf)
    '                                        .AppendText(vbCrLf)
    '                                    End If
    '                                End If
    '                            Next
    '                            .AppendText("                        Case Else" & vbCrLf)
    '                            .AppendText("                            TerminalStateErrorHandler(""" & oClass.Name & """, Me, _currentState, oEvent)" & vbCrLf)
    '                            .AppendText("                    End Select" & vbCrLf)
    '                            If Pigtails.Count > 0 Then
    '                                .AppendText("            End Select" & vbCrLf)
    '                            End If
    '                            .AppendText("            AnnounceStateTransition(bQuiet, ""ST "" & Me._sInstanceName & "" "" & ArchitecturalGlobals.StateNameString(currentStateAtEntry) & "" --["" & sSelfIcon & EventNameString(oEvent.EventID) & ""]-> "" & ArchitecturalGlobals.StateNameString(_currentState))" & vbCrLf)
    '                            .AppendText("        End Sub" & vbCrLf)
    '                            .AppendText(vbCrLf)

    '                            For Each oState In _oDomain.States
    '                                Application.DoEvents()
    '                                If oState.ParentID = iClassStateMachineID Then
    '                                    If oState.Subtype = EA_TYPE.INITIAL_STATE Then        ' if this state is the "meatball" initial state
    '                                        If oState.Connectors.Count = 1 Then
    '                                            Dim oOriginalState As EA.Element = _oDomain.States(CType(oState.Connectors.GetAt(0), EA.Connector).SupplierID.ToString)
    '                                            _sInitialState = CanonicalName(oOriginalState.Name)
    '                                        Else
    '                                            MsgBox("An initial state in the state model for class '" & oClass.Name & "' has no transition out", MsgBoxStyle.Critical)
    '                                        End If
    '                                    End If

    '                                    If IsUnique(oState.Name, oStateNameUniqueBucket) Then
    '                                        addState(oState, oClass)
    '                                    End If
    '                                End If
    '                            Next
    '                            .AppendText(vbCrLf)
    '                        End With
    '                    End If
    '                End If
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub addNormalTransition(ByVal oConnector As EA.Connector, _
    '                                        ByVal oState As EA.Element, _
    '                                        ByRef sArgumentList As String, _
    '                                        ByVal oPopulateTransitions As Collection, _
    '                                        ByVal oClass As EA.Element)
    '            Dim oClientState As EA.Element
    '            Dim sEvent As String
    '            Dim sTokens() As String
    '            Dim oSupplierState As EA.Element = Nothing
    '            Dim sToStateName As String
    '            Dim sTransitionLine As String
    '            Dim oErrorHandler As New sjmErrorHandler
    '            Dim oEVENT_ErrorHandler As New sjmErrorHandler

    '            Try
    '                oErrorHandler.SupplementalInformation = "Class: " & oClass.Name & ", State: " & oState.Name & ", Connector: " & oConnector.Name
    '                If _oDomain.States.Contains(oConnector.ClientID.ToString) Then                      ' if "to" state is a normal state
    '                    oSupplierState = _oDomain.States(oConnector.ClientID.ToString)
    '                    If oSupplierState Is oState Then
    '                        If _oDomain.States.Contains(oConnector.SupplierID.ToString) Then
    '                            oClientState = _oDomain.States(oConnector.SupplierID.ToString)

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
    '                                        sArgumentList = sTokens(1)
    '                                    End If
    '                                End If
    '                                sToStateName = CanonicalName(oState.Name)

    '                                Try
    '                                    oEVENT_ErrorHandler.SupplementalInformation = "EVENT_" & sEvent
    '                                    _oEvents.Add(sEvent)
    '                                    If oClientState.Subtype = EA_TYPE.SYNCH_STATE Then  ' if this state is an ignore marker
    '                                        sTransitionLine = "                                Case EVENT_" & sEvent & "   ' ignored" & vbCrLf & _
    '                                                          "                                    ' do nothing" & vbCrLf
    '                                    Else
    '                                        sTransitionLine = "                                Case EVENT_" & sEvent & vbCrLf & _
    '                                                          "                                    bQuiet = action_" & CanonicalName(oClientState.Name) & "(oEvent)" & vbCrLf
    '                                    End If
    '                                    If Not oPopulateTransitions.Contains(sEvent) Then
    '                                        oErrorHandler.SupplementalInformation = "POP_" & sEvent
    '                                        oPopulateTransitions.Add(sTransitionLine, sEvent)
    '                                    End If
    '                                Catch ex As Exception
    '                                    oEVENT_ErrorHandler.Announce(ex)
    '                                End Try
    '                            End If
    '                        End If
    '                    End If
    '                End If
    '            Catch ex As Exception
    '                oErrorHandler.Announce(ex)
    '            End Try
    '        End Sub

    '        Private Sub addNormalPigtailTransition(ByRef Pigtails As Collection, ByVal iClassStateMachineID As Integer)
    '            Dim oSupplierState As EA.Element = Nothing
    '            Dim oState As EA.Element
    '            Dim oConnector As EA.Connector
    '            Dim sEvent As String
    '            Dim sTokens() As String

    '            Try
    '                For Each oState In _oDomain.States
    '                    Application.DoEvents()
    '                    If oState.ParentID = iClassStateMachineID Then
    '                        If oState.Subtype = EA_TYPE.ENTRY_STATE Then       ' if this state is a pigtail-originating state
    '                            For Each oConnector In oState.Connectors
    '                                sEvent = canonicalEventName(oConnector)
    '                                sTokens = Split(sEvent, ":")
    '                                If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
    '                                    sEvent = sTokens(1)
    '                                End If

    '                                If sEvent.Length > 0 Then
    '                                    If sEvent.IndexOf(")") > 0 Then
    '                                        sTokens = Split(sEvent.Substring(0, sEvent.IndexOf(")")), "(")            ' peel off any parameter payload
    '                                        If sTokens.Length > 1 Then
    '                                            sEvent = sTokens(0)
    '                                        End If
    '                                    End If
    '                                    With Pigtails
    '                                        oSupplierState = _oDomain.States(oConnector.SupplierID.ToString)
    '                                        .Add("                Case EVENT_" & sEvent & vbCrLf)
    '                                        .Add("                    action_" & CanonicalName(oSupplierState.Name) & "(oEvent)" & vbCrLf)
    '                                    End With
    '                                End If
    '                            Next
    '                        End If
    '                    End If
    '                Next
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub addState(ByVal oState As EA.Element, ByVal oClass As EA.Element)
    '            Dim sLines() As String
    '            Dim sLine As String
    '            Dim bQuiet As Boolean = False
    '            Dim bOmit As Boolean = False

    '            Try
    '                bQuiet = ElementIncludesStereotype(oState, "quiet")
    '                bOmit = ElementIncludesStereotype(oState, "omit")

    '                If (Not bOmit) And (oState.Name.Length > 0) Then
    '                    With _oSourceOutput
    '                        If oState.Subtype <> EA_TYPE.SYNCH_STATE Then           ' if this state is NOT an "ignore event" state
    '                            sLines = Split(oState.Notes, vbCrLf)
    '                            .AppendText("        '________________________________________________________ " & CanonicalName(oClass.Name) & ": " & CanonicalName(oState.Name) & " (v" & oState.Version & ")" & vbCrLf & vbCrLf)
    '                            .AppendText("        Private Function action_" & CanonicalName(oState.Name) & "(ByVal oEvent As XEvent) As Boolean" & vbCrLf)
    '                            .AppendText("            '.............. begin action code .............." & vbCrLf & vbCrLf)
    '                            For Each sLine In sLines
    '                                .AppendText(StripRichTextFormat(sLine) & vbLf)
    '                            Next
    '                            .AppendText(vbCrLf & "            '............... end action code ..............." & vbCrLf)

    '                            Select Case oState.Subtype
    '                                Case EA_TYPE.FINAL_STATE
    '                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
    '                                    .AppendText("            XUnit.Framework.Assert.Pass()        ' this assert never returns if NUnit is running the test" & vbCrLf)

    '                                Case EA_TYPE.EXIT_STATE
    '                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
    '                                    .AppendText("            XUnit.Framework.Assert.Fail(oEvent)  ' this assert never returns if NUnit is running the test" & vbCrLf)

    '                                Case EA_TYPE.TERMINATE_STATE
    '                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
    '                                    .AppendText("            XUnit.Framework.Assert.Fail(oEvent)  ' this assert never returns if NUnit is running the test" & vbCrLf)

    '                                Case Else
    '                                    .AppendText("            me._currentState = STATE_" & CanonicalName(oState.Name) & vbCrLf)
    '                            End Select

    '                            If bQuiet Then
    '                                .AppendText("            Return true    ' return QUIET" & vbCrLf)
    '                            Else
    '                                '.AppendText("            ArchitecturalGlobals.DebugWriteLine(""        [STATE: " & CanonicalName(oState.Name) & " (" & GetStateTypeName(oState) & ")]"")" & vbCrLf)
    '                                .AppendText("            Return false   ' return NOT QUIET" & vbCrLf)
    '                            End If
    '                            .AppendText("        End Function" & vbCrLf & vbCrLf)
    '                        End If
    '                    End With
    '                Else
    '                    Debug.WriteLine("Omitting state (by stereotype 'omit'): " & oState.Name)
    '                End If

    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub cardinalityComment(ByVal oConnector As EA.Connector, ByRef sCardinalityComment As String, ByRef sOtherClassName As String, ByVal oClass As EA.Element)
    '            Dim sSupplierCardinality As String

    '            If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
    '                sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
    '                sSupplierCardinality = oConnector.ClientEnd.Cardinality
    '            Else
    '                sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
    '                sSupplierCardinality = oConnector.SupplierEnd.Cardinality
    '            End If

    '            If sOtherClassName = "" Or sSupplierCardinality = "" Then
    '                Throw New ApplicationException("One or both ends of relationship '" & oConnector.Name & "' has no multiplicity supplied")
    '            End If

    '            sOtherClassName = CanonicalName(sOtherClassName)
    '            sSupplierCardinality = CanonicalName(sSupplierCardinality)

    '            Select Case sSupplierCardinality
    '                Case "1", "0..1"
    '                    sCardinalityComment = "   ' " & buildRelationshipPhrase(oConnector, oClass)

    '                Case ""
    '                    sCardinalityComment = "   ' CARDINALITY ASSUMED: " & buildRelationshipPhrase(oConnector, oClass)

    '                Case "0..*", "1..*"
    '                    sCardinalityComment = "   ' " & buildRelationshipPhrase(oConnector, oClass)

    '                Case Else
    '                    sCardinalityComment = "   ' <unknown cardinality> " & sSupplierCardinality
    '            End Select
    '        End Sub

    '        Private Function canonicalDefaultValue(ByVal sDefaultValue As String, ByVal sType As String) As String
    '            Dim sReturnDefaultString As String = sDefaultValue

    '            If sReturnDefaultString.Length = 0 Then
    '                sReturnDefaultString = "0"
    '            End If

    '            Select Case sType.ToLower
    '                Case "int", "float", "double", "boolean", "long", "byte", "unsigned char"
    '                    ' do nothing, no adjustment needed

    '                Case "char"
    '                    sReturnDefaultString = """" & sReturnDefaultString & """"

    '                Case Else                       ' if not a builtin type, must be an instance pointer
    '                    sReturnDefaultString = "Nothing"
    '            End Select

    '            Return sReturnDefaultString
    '        End Function

    '        Private Sub addAttributes(ByVal oClass As EA.Element)
    '            Dim sCommentText As String
    '            Dim sNotesString As String = ""
    '            Dim oAttribute As EA.Attribute
    '            Dim sCanonicalAttributeName As String
    '            Dim sCanonicalType As String
    '            Dim sDefaultValue As String
    '            Dim oConnector As EA.Connector
    '            Dim sOtherClassName As String = ""
    '            Dim sCardinalityComment As String = ""
    '            Dim sStaticString As String

    '            Try
    '                With _oSourceOutput
    '                    For Each oAttribute In oClass.Attributes
    '                        If oAttribute.Notes.Length > 0 Then
    '                            sCommentText = "      ' " & oAttribute.Notes
    '                        Else
    '                            sCommentText = ""
    '                        End If
    '                    Next

    '                    .AppendText(vbCrLf)
    '                    .AppendText("    Public Sub New(ByVal sInstanceName As String)	   " & vbCrLf)
    '                    .AppendText("        MyBase.New(sInstanceName)					   " & vbCrLf)
    '                    .AppendText("        commonNew(_sInstanceName)				       " & vbCrLf)
    '                    .AppendText("    End Sub										   " & vbCrLf)
    '                    .AppendText("    												   " & vbCrLf)
    '                    .AppendText("    Public Sub New()								   " & vbCrLf)
    '                    .AppendText("        MyBase.New()	                   " & vbCrLf)
    '                    .AppendText("        commonNew(_sInstanceName)					   " & vbCrLf)
    '                    .AppendText("    End Sub										   " & vbCrLf)
    '                    .AppendText("    												   " & vbCrLf)
    '                    .AppendText("    Private Sub commonNew(ByVal sInstanceName As String	   ")

    '                    For Each oAttribute In oClass.Attributes
    '                        Application.DoEvents()
    '                        sCanonicalType = canonicalType(oAttribute.Type)
    '                        sCanonicalAttributeName = CanonicalName(oAttribute.Name)
    '                        sDefaultValue = canonicalDefaultValue(oAttribute.Default, oAttribute.Type)
    '                        .AppendText(", Optional " & sCanonicalAttributeName & " As " & sCanonicalType & " = " & sDefaultValue)
    '                    Next
    '                    .AppendText(")" & vbCrLf)
    '                    .AppendText("            Try" & vbCrLf)



    '                    If _sInitialState.Length > 0 Then
    '                        .AppendText("                 _currentState = STATE_" & _sInitialState & vbCrLf)
    '                    End If

    '                    For Each oAttribute In oClass.Attributes
    '                        Application.DoEvents()
    '                        sCanonicalType = canonicalType(oAttribute.Type)
    '                        sCanonicalAttributeName = CanonicalName(oAttribute.Name)
    '                        sDefaultValue = canonicalDefaultValue(oAttribute.Default, oAttribute.Type)
    '                        .AppendText("            _" & sCanonicalAttributeName & " = " & sCanonicalAttributeName & vbCrLf)
    '                    Next
    '                    .AppendText(vbCrLf)
    '                    .AppendText("             	 If not _oInstances.Contains(_sInstanceName) Then  " & vbCrLf)
    '                    .AppendText("             	     _oInstances.Add(Me, _sInstanceName)  " & vbCrLf)
    '                    .AppendText("                End If  " & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText("                If Not goClasses.Contains(""" & oClass.Name & """) Then  " & vbCrLf)
    '                    .AppendText("                    goClasses.Add(""" & oClass.Name & """, _oInstances)  " & vbCrLf)
    '                    .AppendText("                End If  " & vbCrLf)
    '                    .AppendText("            Catch ex As Exception" & vbCrLf)
    '                    .AppendText("                   Dim oErrorHandler As New sjmErrorHandler(ex)" & vbCrLf)
    '                    .AppendText("            End Try " & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText("               " & _oDomain.Name & "_States.RegisterStateNames()" & vbCrLf)
    '                    .AppendText("               " & _oDomain.Name & "_Events.RegisterEventNames()" & vbCrLf)
    '                    .AppendText("         End Sub" & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    .AppendText("         Private Sub initialStateSetup()" & vbCrLf)
    '                    .AppendText("             ' do nothing -- probably should not support initial state action code (see the code compiler for details)" & vbCrLf)
    '                    .AppendText("         End Sub" & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    .AppendText("         Public Overrides Sub DisplayAttributes()" & vbCrLf)
    '                    .AppendText("             Try" & vbCrLf)
    '                    .AppendText("                 MyBase.DisplayAttributes" & vbCrLf)
    '                    .AppendText("                 With _oDataGridView" & vbCrLf)
    '                    For Each oAttribute In oClass.Attributes
    '                        sCanonicalType = canonicalType(oAttribute.Type)
    '                        sCanonicalAttributeName = ATTRIBUTE_PREFIX & CanonicalName(oAttribute.Name)
    '                        .AppendText("                .Rows.Insert(0, Split(""" & sCanonicalAttributeName & ","" & " & sCanonicalAttributeName & ".ToString & "", " & sCanonicalType & """, "",""))" & vbCrLf)
    '                    Next

    '                    If Not _bIsSupertype Then           ' supertypes don't display their current state (the leaf type will do it)
    '                        .AppendText("                .Rows.Insert(0, Split(""[currentState],"" & StateNameString(_currentState, true) & "", <arch>"", "",""))" & vbCrLf)
    '                    End If

    '                    .AppendText("                 End With" & vbCrLf)
    '                    .AppendText("             Catch ex As Exception" & vbCrLf)
    '                    .AppendText("             Finally" & vbCrLf)
    '                    .AppendText("                 ' we don't want anything to stop running just because a display error occurred " & vbCrLf)
    '                    .AppendText("             End Try" & vbCrLf)
    '                    .AppendText("         End Sub" & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    .AppendText("         Public Shared ReadOnly Property " & oClass.Name & "s() As Collection" & vbCrLf)
    '                    .AppendText("             Get" & vbCrLf)
    '                    .AppendText("                 Return _oInstances" & vbCrLf)
    '                    .AppendText("             End Get" & vbCrLf)
    '                    .AppendText("         End Property" & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    For Each oAttribute In oClass.Attributes
    '                        Application.DoEvents()

    '                        If oAttribute.IsStatic Then
    '                            sStaticString = "Shared "
    '                        Else
    '                            sStaticString = ""
    '                        End If

    '                        sCanonicalType = canonicalType(oAttribute.Type)
    '                        sCanonicalAttributeName = CanonicalName(oAttribute.Name)
    '                        .AppendText(("        Dim " & sStaticString & "_" & sCanonicalAttributeName & " As " & sCanonicalType).PadRight(CONSTANTS_COLUMN) & " ' _______________________ " & sCanonicalAttributeName & vbCrLf)
    '                        .AppendText("         Public " & sStaticString & "Property " & ATTRIBUTE_PREFIX & sCanonicalAttributeName & "() As " & sCanonicalType & vbCrLf)
    '                        .AppendText("             Get" & vbCrLf)
    '                        .AppendText("                 Return _" & sCanonicalAttributeName & vbCrLf)
    '                        .AppendText("             End Get" & vbCrLf)
    '                        .AppendText("             Set(ByVal value As " & sCanonicalType & ")" & vbCrLf)
    '                        .AppendText("                 _" & sCanonicalAttributeName & " = value" & vbCrLf)
    '                        .AppendText("             End Set" & vbCrLf)
    '                        .AppendText("         End Property" & vbCrLf)
    '                        .AppendText(vbCrLf)
    '                    Next

    '                    For Each oConnector In oClass.Connectors
    '                        addAssociation(oConnector, oClass)
    '                    Next
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub addAssociation(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
    '            Dim sSupplierCardinality As String
    '            Dim sClientCardinality As String
    '            Dim sCardinalityComment As String = ""
    '            Dim sOtherClassName As String

    '            Try
    '                With _oSourceOutput
    '                    If oConnector.Type = "Association" Then
    '                        If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
    '                            sOtherClassName = CanonicalName(_oDomain.EAClass(oConnector.ClientID).Name)
    '                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
    '                            sClientCardinality = oConnector.SupplierEnd.Cardinality
    '                        Else
    '                            sOtherClassName = CanonicalName(_oDomain.EAClass(oConnector.SupplierID).Name)
    '                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
    '                            sClientCardinality = oConnector.ClientEnd.Cardinality
    '                        End If
    '                        oClass.Name = CanonicalName(oClass.Name)
    '                        oConnector.Name = CanonicalName(oConnector.Name)

    '                        cardinalityComment(oConnector, sCardinalityComment, sOtherClassName, oClass)

    '                        .AppendText(vbCrLf)
    '                        .AppendText(vbCrLf)
    '                        .AppendText(vbCrLf)
    '                        Select Case sSupplierCardinality
    '                            Case "1", "0..1"
    '                                .AppendText("        Public _" & oConnector.Name & "_" & sOtherClassName & " As " & sOtherClassName & vbCrLf)
    '                                Select Case sSupplierCardinality
    '                                    Case "0..1", "1"
    '                                        .AppendText("        Public ReadOnly Property " & oConnector.Name & "_" & sOtherClassName & "() As " & sOtherClassName & "          ' cardinality: " & sSupplierCardinality & vbCrLf)
    '                                        .AppendText("            Get" & vbCrLf)
    '                                        .AppendText("                Return _" & oConnector.Name & "_" & sOtherClassName & vbCrLf)
    '                                        .AppendText("            End Get" & vbCrLf)
    '                                        .AppendText("        End Property" & vbCrLf)

    '                                    Case "0..*", "1..*"
    '                                        .AppendText("        Public ReadOnly Property " & oConnector.Name & "_" & sOtherClassName & "s() As Collection          ' cardinality: " & sSupplierCardinality & vbCrLf)
    '                                        .AppendText("            Get" & vbCrLf)
    '                                        .AppendText("                Return _" & oConnector.Name & "_" & sOtherClassName & "s" & vbCrLf)
    '                                        .AppendText("            End Get" & vbCrLf)
    '                                        .AppendText("        End Property" & vbCrLf)

    '                                    Case Else
    '                                        Throw New ApplicationException("unknown client cardinalilty on relationship '" & oConnector.Name & "' -- " & sClientCardinality)
    '                                End Select
    '                                .AppendText("        Public Sub Relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")" & vbCrLf)
    '                                .AppendText("            If o" & sOtherClassName & " Is Me Then" & vbCrLf)
    '                                .AppendText("                Throw New ApplicationException(""Reflexive relationships are not currently supported: " & oConnector.Name & """)" & vbCrLf)
    '                                .AppendText("            End If" & vbCrLf)
    '                                .AppendText(vbCrLf)
    '                                .AppendText("            If o" & sOtherClassName & " Is Nothing Then" & vbCrLf)
    '                                .AppendText("                Throw New ApplicationException(""Instance '"" & Me._sInstanceName & ""' was passed a null instance to relate through " & oConnector.Name & """)" & vbCrLf)
    '                                .AppendText("            End If" & vbCrLf)
    '                                .AppendText(vbCrLf)
    '                                .AppendText("            If _" & oConnector.Name & "_" & sOtherClassName & " Is Nothing Then" & vbCrLf)
    '                                .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & " = o" & sOtherClassName & vbCrLf)
    '                                .AppendText("            Else" & vbCrLf)
    '                                .AppendText("                Throw New ApplicationException(""Instance '"" & Me._sInstanceName & ""' is already related through " & oConnector.Name & """)" & vbCrLf)
    '                                .AppendText("            End If" & vbCrLf)
    '                                .AppendText(vbCrLf)
    '                                Select Case sClientCardinality
    '                                    Case "0..1", "1"
    '                                        .AppendText("            If o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " Is Nothing Then" & vbCrLf)
    '                                        .AppendText("                o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Me" & vbCrLf)
    '                                        .AppendText("            Else" & vbCrLf)
    '                                        .AppendText("                Throw New ApplicationException(""Instance '"" & o" & sOtherClassName & "._sInstanceName & ""' is already related through " & oConnector.Name & """)" & vbCrLf)
    '                                        .AppendText("            End If" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)
    '                                        .AppendText(vbCrLf)
    '                                        .AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")" & vbCrLf)
    '                                        .AppendText("            _" & oConnector.Name & "_" & sOtherClassName & " = Nothing" & vbCrLf)
    '                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Nothing" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)

    '                                    Case "0..*", "1..*"
    '                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Add( Me, _lInstanceSerialNumber )" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)
    '                                        .AppendText(vbCrLf)
    '                                        .AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "()" & vbCrLf)
    '                                        .AppendText("            If _" & oConnector.Name & "_" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Contains( _lInstanceSerialNumber ) Then" & vbCrLf)
    '                                        .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Remove( _lInstanceSerialNumber.tostring )" & vbCrLf)
    '                                        .AppendText("            End If" & vbCrLf)
    '                                        .AppendText("            _" & oConnector.Name & "_" & sOtherClassName & " = Nothing" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)

    '                                    Case Else
    '                                        Throw New ApplicationException("unknown client cardinalilty on relationship '" & oConnector.Name & "' -- " & sClientCardinality)
    '                                End Select

    '                            Case "0..*", "1..*"
    '                                .AppendText("        Public _" & oConnector.Name & "_" & sOtherClassName & "s As New Collection          ' cardinality: " & sSupplierCardinality & vbCrLf)
    '                                .AppendText("        Public Sub Relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")          ' cardinality: " & sSupplierCardinality & vbCrLf)
    '                                .AppendText("            _" & oConnector.Name & "_" & sOtherClassName & "s.Add(o" & sOtherClassName & ", o" & sOtherClassName & "._lInstanceSerialNumber , )" & vbCrLf)
    '                                Select Case sClientCardinality
    '                                    Case "0..1", "1"
    '                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Me" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)
    '                                        .AppendText(vbCrLf)
    '                                        .AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")" & vbCrLf)
    '                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Nothing" & vbCrLf)
    '                                        .AppendText("            If _" & oConnector.Name & "_" & sOtherClassName & "s.Contains( o" & sOtherClassName & "._lInstanceSerialNumber ) Then" & vbCrLf)
    '                                        .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & "s.Remove( o" & sOtherClassName & "._lInstanceSerialNumber.tostring )" & vbCrLf)
    '                                        .AppendText("            End If" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)

    '                                    Case "0..*", "1..*"
    '                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Add(Me)" & vbCrLf)
    '                                        .AppendText("        End Sub" & vbCrLf)

    '                                    Case Else
    '                                        Throw New ApplicationException("unknown client cardinalilty on relationship '" & oConnector.Name & "' -- " & sClientCardinality)
    '                                End Select

    '                            Case Else
    '                                If sSupplierCardinality.Length > 0 Then
    '                                    Throw New ApplicationException("Unknown cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
    '                                Else
    '                                    Throw New ApplicationException("No cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
    '                                End If
    '                        End Select
    '                    End If
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Sub printAttributeDescription(ByVal sAttributeName As String)
    '            _oSourceOutput.AppendText(("   print(""\n  " & sAttributeName & ": ").PadRight(COLUMN_WIDTH) & (" ""); print($self->{" & sAttributeName & "})").PadRight(COLUMN_WIDTH) & " if $self->{" & sAttributeName & "};" & vbCrLf)
    '        End Sub

    '        Private Function buildRelationshipPhrase(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element) As String
    '            Dim iClientClassId As Integer
    '            Dim iSupplierClassId As Integer
    '            Dim sPhrase As String = ""
    '            Dim oClientClass As EA.Element
    '            Dim oSupplierClass As EA.Element
    '            Dim sClientRole As String
    '            Dim sSupplierRole As String
    '            Dim sSupplierCardinality As String
    '            Dim sSupplierClassName As String
    '            Dim sClientClassName As String

    '            Try
    '                With oConnector
    '                    iClientClassId = .ClientID
    '                    iSupplierClassId = .SupplierID
    '                    sClientRole = .ClientEnd.Role
    '                    sSupplierRole = .SupplierEnd.Role
    '                    sSupplierCardinality = .SupplierEnd.Cardinality

    '                    With _oDomain
    '                        oClientClass = .EAClass(iClientClassId)
    '                        sClientClassName = oClientClass.Name
    '                        oSupplierClass = .EAClass(iSupplierClassId)
    '                        sSupplierClassName = oSupplierClass.Name
    '                    End With

    '                    If sClientClassName = oClass.Name Then
    '                        ' do nothing, perspective is already proper
    '                    Else
    '                        If sSupplierClassName = oClass.Name Then
    '                            iClientClassId = .SupplierID
    '                            iSupplierClassId = .ClientID
    '                            sClientRole = .SupplierEnd.Role
    '                            sSupplierRole = .ClientEnd.Role
    '                            sSupplierCardinality = .ClientEnd.Cardinality

    '                            With _oDomain
    '                                oClientClass = .EAClass(iClientClassId)
    '                                sClientClassName = oClientClass.Name
    '                                oSupplierClass = .EAClass(iSupplierClassId)
    '                                sSupplierClassName = oSupplierClass.Name
    '                            End With
    '                        Else
    '                            Throw New ApplicationException("PerspectiveClassName '" & oClass.Name & "'does not match either participant in relationship")
    '                        End If
    '                    End If
    '                End With

    '                Select Case oConnector.Type
    '                    Case "Generalization"
    '                        sPhrase = oClientClass.Name & " is a " & oSupplierClass.Name
    '                    Case "Association"
    '                        sPhrase = ""
    '                        Select Case sSupplierCardinality
    '                            Case "1"
    '                                sPhrase += oClientClass.Name & " """ & sClientRole & """ exactly one " & oSupplierClass.Name

    '                            Case "0..1"
    '                                sPhrase += oClientClass.Name & " """ & sClientRole & """ zero or one " & oSupplierClass.Name

    '                            Case "0..*"
    '                                sPhrase += oClientClass.Name & " """ & sClientRole & """ zero or more " & oSupplierClass.Name & "s"

    '                            Case "1..*"
    '                                sPhrase += oClientClass.Name & " """ & sClientRole & """ one or more " & oSupplierClass.Name & "s"

    '                            Case Else
    '                                sPhrase += "<unknown cardinality on '" & oClientClass.Name & "' side of relationship '" & oConnector.Name & "'"
    '                        End Select
    '                    Case Else
    '                        sPhrase = "<unknown connector type: " & oConnector.Type
    '                End Select
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try

    '            Return sPhrase
    '        End Function
    '    End Class

    '    Private Class InterfaceClass
    '        Protected _oClassElement As EA.Element
    '        Protected _oSourceOutput As RichTextBox
    '        Protected _oDomain As Domain
    '        Protected _bFinalStateReported As Boolean = False
    '        Protected _oEvents As New Collection
    '        Protected _sInitialState As String = ""
    '        Protected _bActiveClass As Boolean = False
    '        Protected _sParentClassName As String = ""
    '        Protected _bIsSupertype As Boolean
    '        Protected _bIsSubtype As Boolean

    '        Private Sub addEvents()
    '            Dim oMethod As EA.Method
    '            Dim oParameter As EA.Parameter
    '            Dim sLeadingComma As String = ""

    '            With _oSourceOutput
    '                For Each oMethod In _oClassElement.Methods
    '                    Application.DoEvents()
    '                    If oMethod.Abstract Then            ' borrowing the 'abstract' designator to indicate client->server synchronous calls (this one is)
    '                        Application.DoEvents()

    '                        sLeadingComma = ""

    '                        .AppendText("    '''<summary>" & vbCrLf)
    '                        .AppendText("    '''" & StripRichTextFormat(oMethod.Notes).Trim() & vbCrLf)
    '                        .AppendText("    '''</summary>" & vbCrLf)
    '                        .AppendText("    Public Event " & oMethod.Name & "(")
    '                        For Each oParameter In oMethod.Parameters
    '                            oParameter.Name = CanonicalName(oParameter.Name, True)
    '                            oParameter.Type = canonicalType(oParameter.Type)
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & canonicalType(oParameter.Type))
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")")
    '                        .AppendText(vbCrLf)

    '                        sLeadingComma = ""
    '                        .AppendText("    Private Sub " & oMethod.Name & "_EventHandler(")
    '                        For Each oParameter In oMethod.Parameters
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(") Handles _o" & _oClassElement.Name & "." & oMethod.Name & vbCrLf)
    '                        sLeadingComma = ""
    '                        .AppendText("        RaiseEvent " & oMethod.Name & "(")
    '                        For Each oParameter In oMethod.Parameters
    '                            .AppendText(sLeadingComma & oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")" & vbCrLf)
    '                        .AppendText("    End Sub" & vbCrLf)
    '                        .AppendText(vbCrLf)
    '                    End If
    '                Next
    '            End With
    '        End Sub

    '        Private Sub addInterfaceMethods()
    '            Dim oMethod As EA.Method
    '            Dim oParameter As EA.Parameter
    '            Dim sLeadingComma As String = ""
    '            Dim oOptionalParameters As Collection
    '            Dim oRequiredParameters As Collection
    '            Dim sStaticString As String

    '            With _oSourceOutput
    '                For Each oMethod In _oClassElement.Methods
    '                    Application.DoEvents()

    '                    If oMethod.IsStatic Then
    '                        sStaticString = "Shared "
    '                    Else
    '                        sStaticString = ""
    '                    End If

    '                    oOptionalParameters = New Collection
    '                    oRequiredParameters = New Collection
    '                    For Each oParameter In oMethod.Parameters
    '                        Application.DoEvents()
    '                        oParameter.Name = CanonicalName(oParameter.Name)
    '                        oParameter.Type = canonicalType(oParameter.Type)
    '                        If oParameter.Default.Length > 0 Then           ' parameters with default values are assumed to be optional
    '                            oParameter.Default = Regex.Replace(oParameter.Default.ToString.Trim, "null", "Nothing", RegexOptions.IgnoreCase)
    '                            oOptionalParameters.Add(oParameter)
    '                        Else
    '                            oRequiredParameters.Add(oParameter)
    '                        End If
    '                    Next


    '                    oMethod.Name = CanonicalName(oMethod.Name, True)
    '                    If Not oMethod.Abstract Then            ' borrowing the 'abstract' designator to indicate server->client events
    '                        .AppendText("    '''<summary>" & vbCrLf)
    '                        .AppendText("    '''" & StripRichTextFormat(oMethod.Notes).Trim() & vbCrLf)
    '                        .AppendText("    '''</summary>" & vbCrLf)

    '                        For Each oArgument As EA.Parameter In oRequiredParameters
    '                            .AppendText("    '''<param  name=""" & oArgument.Name & """>" & cleanCommentLines(StripRichTextFormat(oArgument.Notes)) & "</param>" & vbCrLf)
    '                        Next

    '                        sLeadingComma = ""
    '                        oMethod.ReturnType = canonicalType(oMethod.ReturnType)
    '                        If oMethod.ReturnType.Length > 0 Then           ' if there is a return type (other than 'void')
    '                            .AppendText("    Public " & sStaticString & "Function " & oMethod.Name & "(")
    '                        Else
    '                            .AppendText("    Public " & sStaticString & "Sub " & oMethod.Name & "(")
    '                        End If

    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
    '                            sLeadingComma = ", "
    '                        Next

    '                        For Each oParameter In oOptionalParameters
    '                            .AppendText(sLeadingComma & "Optional " & oParameter.Name & " As " & oParameter.Type & " = " & oParameter.Default)
    '                            sLeadingComma = ", "
    '                        Next

    '                        If oMethod.ReturnType.Length > 0 Then           ' if there is a return type
    '                            .AppendText(") As " & oMethod.ReturnType & vbCrLf)
    '                            .AppendText("       Return " & _oDomain.Name & "." & _oClassElement.Name & "." & oMethod.Name & "(")
    '                        Else
    '                            .AppendText(")" & vbCrLf)
    '                            .AppendText("            " & _oDomain.Name & "." & _oClassElement.Name & "." & oMethod.Name & "(")
    '                        End If

    '                        sLeadingComma = ""
    '                        For Each oParameter In oRequiredParameters
    '                            .AppendText(sLeadingComma & oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        For Each oParameter In oOptionalParameters
    '                            .AppendText(sLeadingComma & oParameter.Name)
    '                            sLeadingComma = ", "
    '                        Next
    '                        .AppendText(")            ' delegate the work to the internal method" & vbCrLf)

    '                        If oMethod.ReturnType.Length > 0 Then           ' if there is a return type
    '                            .AppendText("    End Function" & vbCrLf & vbCrLf)
    '                        Else
    '                            .AppendText("    End Sub" & vbCrLf & vbCrLf)
    '                        End If
    '                    End If
    '                Next
    '            End With
    '        End Sub

    '        Public Sub New(ByVal oClassElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
    '            Dim bIsTestFixture As Boolean = False
    '            Dim bIsTest As Boolean = False

    '            Try
    '                _oSourceOutput = oSourceOutput
    '                _oDomain = oDomain
    '                _oClassElement = oClassElement

    '                With _oSourceOutput
    '                    .AppendText(vbCrLf)
    '                    .AppendText("' _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _" & vbCrLf)
    '                    .AppendText("' _ " & vbCrLf)
    '                    .AppendText("' _" & ("COM OBJECT: " & oClassElement.Name).PadLeft(84) & vbCrLf)
    '                    .AppendText("' _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _" & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText("'''<summary>" & vbCrLf)
    '                    .AppendText("'''" & cleanCommentLines(StripRichTextFormat(oClassElement.Notes.Trim)) & vbCrLf)
    '                    .AppendText("'''</summary>" & vbCrLf)
    '                    .AppendText("Public Class " & oClassElement.Name & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                    .AppendText("    Private WithEvents _o" & _oClassElement.Name & " As New " & _oDomain.Name & "." & _oClassElement.Name & vbCrLf & vbCrLf)
    '                    .AppendText(vbCrLf)

    '                    addAttributes()

    '                    addInterfaceMethods()

    '                    addEvents()

    '                    .AppendText("End Class " & vbCrLf)
    '                End With
    '            Catch ex As Exception
    '                Dim oErrorHandler As New sjmErrorHandler(ex)
    '            End Try
    '        End Sub

    '        Private Function cleanCommentLines(ByVal sNoteLines As String)
    '            Dim sCleanCommentLine As String = sNoteLines
    '            sCleanCommentLine = Regex.Replace(sCleanCommentLine, """", """""")        ' replace all double quotes (the " character) with double-double quotes
    '            sCleanCommentLine = Regex.Replace(sCleanCommentLine, vbCr, " ")           ' replace any CR with space
    '            sCleanCommentLine = Regex.Replace(sCleanCommentLine, vbTab, " ")          ' replace any TAB with space
    '            sCleanCommentLine = Regex.Replace(sCleanCommentLine, vbLf, " ")           ' replace any LF with space
    '            Return sCleanCommentLine.Trim
    '        End Function

    '        Private Sub addAttributes()
    '            Dim sStaticString As String
    '            Dim sClassName As String

    '            With _oSourceOutput
    '                For Each oAttribute As EA.Attribute In _oClassElement.Attributes
    '                    Application.DoEvents()

    '                    If oAttribute.IsStatic Then
    '                        sStaticString = "Shared "
    '                        sClassName = _oDomain.Name & "." & _oClassElement.Name
    '                    Else
    '                        sStaticString = ""
    '                        sClassName = "_o" & _oClassElement.Name
    '                    End If

    '                    Dim sCanonicalType As String = canonicalType(oAttribute.Type)
    '                    Dim sCanonicalAttributeName As String = CanonicalName(oAttribute.Name)
    '                    .AppendText("    '''<summary>" & vbCrLf)
    '                    .AppendText("    '''" & StripRichTextFormat(oAttribute.Notes).Trim() & vbCrLf)
    '                    .AppendText("    '''</summary>" & vbCrLf)
    '                    .AppendText("    Public " & sStaticString & "Property " & sCanonicalAttributeName & "() As " & sCanonicalType & vbCrLf)
    '                    .AppendText("        Get" & vbCrLf)
    '                    .AppendText("            Return " & sClassName & "." & "ATTR_" & sCanonicalAttributeName & vbCrLf)
    '                    .AppendText("        End Get" & vbCrLf)
    '                    .AppendText("        Set(ByVal value As " & sCanonicalType & ")" & vbCrLf)
    '                    .AppendText("            " & sClassName & "." & "ATTR_" & sCanonicalAttributeName & " = value" & vbCrLf)
    '                    .AppendText("        End Set" & vbCrLf)
    '                    .AppendText("    End Property" & vbCrLf)
    '                    .AppendText(vbCrLf)
    '                Next
    '            End With
    '        End Sub
    '    End Class
    'End Class
End Class
