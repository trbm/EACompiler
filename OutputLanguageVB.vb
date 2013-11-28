Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO

Public Class OutputLanguageVB
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

    Private _oInterfaces As New Collection
    Private _lblOutputFilename As Label
    Private Shared _iPackageCount As Integer = 0

    Public Shared Function GetStateTypeName(ByVal oState As EA.Element) As String
        Dim sName As String = "Normal"
        Static oStateTypeToName As Collection

        If oStateTypeToName Is Nothing Then
            oStateTypeToName = New Collection
            With oStateTypeToName
                .Add("Final", CStr(EA_TYPE.FINAL_STATE))
                .Add("Exit", CStr(EA_TYPE.EXIT_STATE))
                .Add("Initial", CStr(EA_TYPE.INITIAL_STATE))
                .Add("Entry", CStr(EA_TYPE.ENTRY_STATE))
                .Add("Terminate", CStr(EA_TYPE.TERMINATE_STATE))
                .Add("Synch", CStr(EA_TYPE.SYNCH_STATE))
            End With
        End If

        If oStateTypeToName.Contains(oState.Subtype.ToString) Then
            sName = oStateTypeToName(oState.Subtype.ToString)
        End If

        Return sName
    End Function

    Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef oTextbox As RichTextBox)

        If oTextbox.Text.Length > 0 Then
            OutputFile.ClearFilesCreated()
            Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
            With oOutputFile
                .Add("' ________________________________________________________________________________")
                .Add("' ")
                .Add("'          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY")
                .Add("' ________________________________________________________________________________")
                .Add("' ")
                .Add("'               File: " & sOutputFilename)
                .Add("' ")
                .Add("'         Created by: " & Application.ProductName & " (EA Model Compiler v" & VERSION & ")")
                .Add("' ")
                .Add("'          Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString)
                .Add("' ")
                .Add("' ________________________________________________________________________________")
                .Add("' ")
                .Add("'           Copyright © 2011,  brennan-marquez, LLC   All rights reserved.")
                .Add("' ________________________________________________________________________________")
                .Add("")
                .Add("")
                .Add("Imports Microsoft.VisualBasic")
                .Add("Imports System.Xml")
                .Add("Imports System.net.Sockets")
                .Add("Imports System.net")
                .Add("Imports ArchitecturalSupport")
                .Add("")
                .Add(oTextbox.Text)
            End With

            oOutputFile.Close()
        End If
    End Sub

    Private Sub recursePackage(ByVal oNextPackage As EA.Package, ByVal oPackages As Collection)
        Debug.WriteLine(oNextPackage.Name)
        For Each oPackage As EA.Package In oNextPackage.Packages
            _iPackageCount += 1
            oPackages.Add(oPackage)
            recursePackage(oPackage, oPackages)
        Next
    End Sub

    Private Sub createDomain(ByVal oRepository, ByVal oPackage)
        Dim oTextbox As New RichTextBox
        Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), oPackage.Name & ".vb")

        gStatusBox.Filename = oPackage.Name

        Dim oDomain As Domain = New Domain(oPackage, oRepository, oTextbox)      ' constructor does the work
        createOutputFile(sOutputFilename, oTextbox)
    End Sub

    Public Sub CreateDomains(ByVal oRepository As EA.Repository, ByVal bIncludeDebug As Boolean, ByVal sXSLfilename As String, ByVal sOutputFileExtension As String) Implements IOutputLanguage.CreateDomains
        Try
            gStatusBox = New frmStatusBox
            gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

            Dim oPackagesList As New Collection

            For Each oPackage As EA.Package In oRepository.Models.GetAt(0).Packages
                recursePackage(oPackage, oPackagesList)
            Next

            If _iPackageCount = 0 Then
                MsgBox("No packages found with stereotype 'pycca' so no compilation was done")
            Else
                For Each oFoundPackage As EA.Package In oPackagesList
                    createDomain(oRepository, oFoundPackage)
                Next
            End If

            gStatusBox.FadeAway()
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Protected Class Domain
        Private _oTestFixtureElement As EA.Element
        Private _TestElements As Collection
        Private _oSourceOutput As RichTextBox
        Private _oRepository As EA.Repository
        Private _sPackageId As String
        Private _oProject As EA.Project
        Private _oPackage As EA.Package
        Private _ClassById As Collection
        Private _Triggers As Collection
        Private _States As Collection
        Private _Notes As Collection
        Private _Boundarys As Collection
        Private _StateMachines As Collection
        Private _IsRealized As Boolean
        Private _Name As String
        Private _ElementById As Collection
        Private _EAClassInstances As New List(Of EAClass)

        Public ReadOnly Property Name() As String
            Get
                Return _Name
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

        Public ReadOnly Property Boundarys() As Collection
            Get
                Return _Boundarys
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

                _Boundarys = New Collection
                _Notes = New Collection
                _TestElements = New Collection
                _ClassById = New Collection
                _Triggers = New Collection
                _States = New Collection
                _ElementById = New Collection
                _StateMachines = New Collection

                _IsRealized = PackageIncludesStereotype(_oPackage, "realized")

                If Not _IsRealized Then
                    catalogElements()

                    addVisibilityMarkers()

                    With _oSourceOutput
                        .AppendText(vbCrLf)
                        .AppendText(vbCrLf)
                        .AppendText("Namespace " & _Name & vbCrLf)
                        .AppendText("")

                        '  addInterfaceClasses()

                        generateSource()

                        addStateConstants()

                        addEventConstants()

                        .AppendText("End Namespace" & vbCrLf)

                    End With
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addStateConstants()
            Dim oState As EA.Element
            Dim iNextStateId As Integer
            Dim oUniqueBucket As Collection

            Application.DoEvents()
            With _oSourceOutput
                .AppendText(vbCrLf)

                iNextStateId = giNextStateID            ' set to our first state id
                .AppendText("    public Module " & _Name & "_States" & vbCrLf)

                oUniqueBucket = New Collection
                For Each oState In _States
                    If IsUnique(oState.Name, oUniqueBucket) Then
                        .AppendText(("        Public Const STATE_" & Canonical.CanonicalName(oState.Name) & " As Long = ").PadRight(CONSTANTS_COLUMN) & iNextStateId & vbCrLf)
                        iNextStateId += 1                   ' inc the local ID number as well (these will track through this loop)
                    End If
                Next
                .AppendText(vbCrLf)

                iNextStateId = giNextStateID            ' reset back to our first state id (to get IDs aligned)
                .AppendText("     Public sub RegisterStateNames()" & vbCrLf)
                .AppendText("         Static bRegistrationComplete As Boolean = False" & vbCrLf)
                .AppendText(vbCrLf)
                .AppendText("         If Not bRegistrationComplete Then" & vbCrLf)
                .AppendText("             bRegistrationComplete = True" & vbCrLf)

                oUniqueBucket = New Collection
                For Each oState In _States
                    If IsUnique(oState.Name, oUniqueBucket) Then
                        .AppendText(("                RegisterStateNameString( """ & Canonical.CanonicalName(oState.Name) & """, """ & iNextStateId & """) ") & vbCrLf)
                        iNextStateId += 1
                    End If
                Next
                .AppendText("            End If" & vbCrLf)
                .AppendText("        End Sub" & vbCrLf)
                .AppendText("    End Module" & vbCrLf)
                .AppendText(vbCrLf)

                giNextStateID = iNextStateId            ' update the global ID holder
            End With
        End Sub

        Private Sub addEventConstants()
            Dim sEvent As String
            Dim iNextEventID As Integer
            Dim oEvents As New Collection
            Dim oState As EA.Element
            Dim oConnector As EA.Connector
            Dim sTokens() As String

            Application.DoEvents()
            With _oSourceOutput
                iNextEventID = giNextEventID                        ' set ID starting point
                For Each oState In _States
                    For Each oConnector In oState.Connectors
                        Application.DoEvents()
                        sEvent = canonicalEventName(oConnector)
                        sTokens = Split(sEvent, ":")                ' discard any "ev1:" type prefixes
                        sEvent = sTokens(0)
                        sTokens = Split(sEvent, "(")                ' discard any parameters supplied
                        sEvent = Canonical.CanonicalName(sTokens(0))
                        If sEvent.Length > 0 Then
                            If Not oEvents.Contains(sEvent) Then
                                oEvents.Add(sEvent, sEvent)
                            End If
                        End If
                    Next
                Next
                .AppendText(vbCrLf)

                iNextEventID = giNextEventID                        ' reset back to starting point
                .AppendText("    Public Module " & _Name & "_Events" & vbCrLf)
                For Each sEvent In oEvents
                    .AppendText(("       Public Const EVENT_" & sEvent & " As Long = ").PadRight(CONSTANTS_COLUMN) & iNextEventID & vbCrLf)
                    iNextEventID += 1
                Next
                .AppendText(vbCrLf)

                iNextEventID = giNextEventID                        ' reset back to starting point
                .AppendText("        Public sub RegisterEventNames()" & vbCrLf)
                .AppendText("            Static bRegistrationComplete As Boolean = False" & vbCrLf)
                .AppendText(vbCrLf)
                .AppendText("            If Not bRegistrationComplete Then" & vbCrLf)
                .AppendText("                bRegistrationComplete = True" & vbCrLf)
                For Each sEvent In oEvents
                    .AppendText(("                RegisterEventNameString( """ & sEvent & """, """ & iNextEventID & """) ") & vbCrLf)
                    iNextEventID += 1
                Next
                .AppendText("            End If" & vbCrLf)
                .AppendText("        End Sub" & vbCrLf)
                .AppendText("    End Module" & vbCrLf)
                .AppendText(vbCrLf)

                giNextEventID = iNextEventID            ' bump the global ID holder to account for the IDs we used
            End With
        End Sub

        Private Sub addVisibilityMarkers()
            For Each oClass As EA.Element In _oPackage.Elements
                If ElementIncludesStereotype(oClass, INTERFACE_CLASS) Then
                    EAClass(oClass.ElementID).Tag = VISIBILITY_MARKER
                    Dim sTokens() As String = Split(oClass.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
                    If (sTokens(0).Length > 0) Then
                        For Each sId As String In sTokens
                            Dim oParentClass As EA.Element = EAClass(sId)
                            If oParentClass.Tag = VISIBILITY_MARKER Then
                                Exit For
                            Else
                                oParentClass.Tag = VISIBILITY_MARKER
                            End If
                        Next
                    End If
                End If
            Next
        End Sub

        Private Sub catalogElements()
            Dim oElement As EA.Element
            Dim oErrorHandler As New sjmErrorHandler

            Try
                Application.DoEvents()

                For Each oElement In _oPackage.Elements
                    Application.DoEvents()
                    oElement.Name = CanonicalClassName(oElement.Name)           ' establish safe names right off the bat (rather than sprinkling everywehre)
                    _ElementById.Add(oElement, oElement.ElementID)              ' just as a debugging convenience, to look up any element from its id only

                    If oElement.Name.Length = 0 Then
                        oElement.Name = "NoName_" & oElement.ElementID
                    End If

                    Select Case oElement.MetaType
                        Case "StateMachine"
                            _StateMachines.Add(oElement, oElement.ElementID)

                        Case "FinalState"
                            _States.Add(oElement, oElement.ElementID)

                        Case "Pseudostate"
                            _States.Add(oElement, oElement.ElementID)

                        Case "Trigger"
                            oErrorHandler.SupplementalInformation = "_Triggers: " & oElement.Name               ' in case an exception is thrown
                            If Not _Triggers.Contains(oElement.Name) Then   ' event names may be reused between state machines
                                _Triggers.Add(oElement, oElement.Name)
                            End If

                        Case "Class"
                            oErrorHandler.SupplementalInformation = "_ClassById: " & oElement.Name              ' in case an exception is thrown
                            oElement.Tag = ""
                            _ClassById.Add(oElement, oElement.ElementID)
                            sortByStereoType(oElement)

                        Case "State"
                            recordStateElement(oElement)

                        Case "Text"
                            ' do nothing with this type, but don't throw a warning that it is unknown

                        Case Else
                            Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
                    End Select
                Next
            Catch ex As Exception
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

        Private Sub recordStateElement(ByVal oElement As EA.Element)
            Dim oErrorHandler As New sjmErrorHandler

            oErrorHandler.SupplementalInformation = "_States (State): " & oElement.Name                         ' in case an exception is thrown
            _States.Add(oElement, oElement.ElementID)
        End Sub

        Private Sub fileFooter(ByVal oPackage As EA.Package)
            With _oSourceOutput
                .AppendText(vbCrLf)
                .AppendText("" & vbCrLf)
                .AppendText(vbCrLf)
            End With
        End Sub

        Private Sub generateSource()
            Dim oClassElement As EA.Element
            Dim oEAClass As EAClass
            Static iClassCounter As Integer = 0

            Try
                Application.DoEvents()
                With _oSourceOutput
                    If _oPackage.Notes.Length > 0 Then
                        .AppendText("   ' " & _oPackage.Notes)
                    End If
                    .AppendText(vbCrLf)

                    gStatusBox.ProgressValueMaximum = _ClassById.Count
                    For Each oClassElement In _ClassById
                        gStatusBox.ProgressValue = iClassCounter
                        Application.DoEvents()
                        If _sPackageId = oClassElement.PackageID Then
                            If ElementIncludesStereotype(oClassElement, "omit") Then
                                Debug.WriteLine("Omitting class (by stereotype 'omit'): " & oClassElement.Name)
                            Else
                                oEAClass = New EAClass(oClassElement, Me, _oSourceOutput)
                                _EAClassInstances.Add(oEAClass)
                            End If
                        End If
                        iClassCounter += 1
                    Next
                    fileFooter(_oPackage)
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        'Private Sub addXMLStreaming()
        '    With _oSourceOutput
        '        .AppendText(vbCrLf)
        '        .AppendText("    Public Module RehydrateFromXML" & vbCrLf)
        '        .AppendText("        Public Sub RehydrateFromXML(sFilename as string)" & vbCrLf)
        '        .AppendText("            Dim xXMLDocument As New XmlDocument" & vbCrLf)
        '        .AppendText("            xXMLDocument.Load(sFilename)" & vbCrLf)
        '        .AppendText(vbCrLf)
        '        .AppendText("'---------------------------------------- rehydate" & vbCrLf)
        '        For Each oClass As EAClass In _EAClassInstances
        '            If Not oClass.IsSupertype Then
        '                .AppendText("            " & oClass.Name & ".RehydrateFromXML(xXMLDocument)" & vbCrLf)
        '                .AppendText(vbCrLf)
        '            End If
        '        Next
        '        .AppendText("'---------------------------------------- relink" & vbCrLf)
        '        For Each oClass As EAClass In _EAClassInstances
        '            If Not oClass.IsSupertype Then
        '                .AppendText("            " & oClass.Name & ".RelinkFromXML(xXMLDocument)" & vbCrLf)
        '                .AppendText(vbCrLf)
        '            End If
        '        Next
        '        .AppendText("        End Sub " & vbCrLf)
        '        .AppendText("    End Module" & vbCrLf)

        '        .AppendText(vbCrLf)
        '        .AppendText("    Public Module DehydrateToXML" & vbCrLf)
        '        .AppendText("        Public Sub DehydrateToXML(sFilename as string)" & vbCrLf)
        '        .AppendText("            Dim oXMLBuilder As New XMLBuilder(""OUTPUT"", sFilename)" & vbCrLf)
        '        .AppendText(vbCrLf)
        '        For Each oClass As EAClass In _EAClassInstances
        '            If Not oClass.IsSupertype Then
        '                .AppendText("            " & oClass.Name & ".DehydrateToXML(oXMLBuilder)" & vbCrLf)
        '            End If
        '        Next
        '        .AppendText("           oXMLBuilder.Save(sFilename)" & vbCrLf)
        '        .AppendText("        End Sub " & vbCrLf)
        '        .AppendText("    End Module" & vbCrLf)
        '    End With
        'End Sub
    End Class

    Private Class EAClass
        Private _oEAElement As EA.Element
        Private _oSourceOutput As RichTextBox
        Private _oDomain As Domain
        Private _bFinalStateReported As Boolean = False
        Private _oEvents As New Collection
        Private _sInitialState As String = ""
        Private _bActiveClass As Boolean = False
        Private _sParentClassName As String = ""
        Private _bIsSupertype As Boolean
        Private _bIsSubtype As Boolean

        Const COMMENT_START_COLUMN = 40

        Public Sub New(ByVal oEAElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
            Dim bIsTestFixture As Boolean = False
            Dim bIsTest As Boolean = False
            Dim sTokens() As String
            Dim sVisibility As String = "Public"

            Try
                _oDomain = oDomain
                _oSourceOutput = oSourceOutput
                _oEAElement = oEAElement

                If _oEAElement.Tag = VISIBILITY_MARKER Then
                    sVisibility = "Public"
                End If

                gStatusBox.ShowClassName(_oEAElement.Name)

                If _oDomain.TestFixtureElement IsNot Nothing Then
                    bIsTestFixture = (_oDomain.TestFixtureElement.Name = _oEAElement.Name)
                End If
                bIsTest = _oDomain.TestElements.Contains(_oEAElement.Name)

                _oEAElement.Name = Canonical.CanonicalName(_oEAElement.Name)      ' be sure the class name is in a legal form

                sTokens = Split(_oEAElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
                _bIsSupertype = (sTokens(0).Length > 0)
                sTokens = Split(_oEAElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeStart), ",")
                If (sTokens(0).Length > 0) Then
                    _sParentClassName = Canonical.CanonicalName(_oDomain.EAClass(sTokens(0)).Name)
                End If

                _bIsSubtype = (_sParentClassName.Length > 0)        ' this class has a parent class name iff it is a subtype

                With _oSourceOutput
                    .AppendText(vbCrLf)

                    .AppendText("<Serializable> _" & vbCrLf)

                    If _bIsSupertype Then
                        .AppendText(sVisibility & " MustInherit Class " & _oEAElement.Name & vbCrLf)
                    Else
                        If bIsTestFixture Then
                            .AppendText("<NUnit.Framework.TestFixture()> _" & vbCrLf)
                        End If

                        .AppendText(sVisibility & " Class " & _oEAElement.Name & "' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% `" & _oEAElement.Name & vbCrLf)
                    End If
                    '.AppendText("' _____________________________________________________________________________________" & vbCrLf)
                    '.AppendText("' _ " & vbCrLf)
                    '.AppendText("' _" & ("                                    `" & _oEAElement.Name).PadLeft(84) & vbCrLf)
                    '.AppendText("' _____________________________________________________________________________________" & vbCrLf)

                    If _bIsSubtype Then            ' if this class is a subtype
                        .AppendText("    Inherits " & _sParentClassName & vbCrLf)
                    Else
                        .AppendText("    Inherits XClass" & vbCrLf)
                    End If

                    If bIsTestFixture Then
                        If _oDomain.TestElements.Count > 0 Then
                            .AppendText(vbCrLf)
                            .AppendText(vbCrLf)
                            .AppendText("        ' NUnit test entry points")
                            .AppendText(vbCrLf)
                            For Each oTestElement As EA.Element In _oDomain.TestElements
                                oTestElement.Name = Canonical.CanonicalName(oTestElement.Name)

                                .AppendText(vbCrLf)
                                .AppendText("        <NUnit.Framework.Test()> _" & vbCrLf)
                                addCustomAttributes(oTestElement)
                                .AppendText("        Public Sub " & oTestElement.Name & " ()" & vbCrLf)
                                .AppendText("            Assert.TestBegin()" & vbCrLf)
                                .AppendText("            XEvent.Send(EVENT_Go, New " & oTestElement.Name & "(""o" & oTestElement.Name & """), ""start " & oTestElement.Name & " running"")" & vbCrLf)
                                .AppendText("            Assert.TestEnd()" & vbCrLf)
                                .AppendText("        End Sub" & vbCrLf)
                            Next
                        End If
                    End If

                    .AppendText(vbCrLf)
                    .AppendText("    Private Shared m_o" & _oEAElement.Name & "s As New List(Of " & _oEAElement.Name & ")" & vbCrLf)
                    .AppendText(vbCrLf)
                    .AppendText("    Protected Overrides Sub DeleteSelf()" & vbCrLf)
                    .AppendText("       DeleteSelfFromGlobalHash()" & vbCrLf)
                    .AppendText("       On Error Resume Next  ' don't worry if the instance has already been removed" & vbCrLf)
                    .AppendText("       m_o" & _oEAElement.Name & "s.Remove(Me)" & vbCrLf)
                    .AppendText("    End Sub" & vbCrLf)
                    .AppendText(vbCrLf)

                    Dim oAttributes As Collection = accumulateAttributes(_oEAElement)
                    Dim oConnectors As Collection = accumulateConnectors(_oEAElement)

                    addEvents(_oEAElement)

                    addStateMachine(_oEAElement)

                    addAttributes(_oEAElement)

                    addSynchronousCalls(_oEAElement)

                    .AppendText("End Class " & vbCrLf)
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

        Public ReadOnly Property Name() As String
            Get
                Return _oEAElement.Name
            End Get
        End Property

        Private Sub addCustomAttributes(ByVal oTestElement As EA.Element)
            With _oSourceOutput
                If ElementIncludesStereotype(oTestElement, "manual") Then
                    .AppendText("        <NUnit.Framework.Category(""Manual"")> _" & vbCrLf)
                Else
                    .AppendText("        <NUnit.Framework.Category(""Automated"")> _" & vbCrLf)
                End If

                If ElementIncludesStereotype(oTestElement, "development") Then
                    .AppendText("        <NUnit.Framework.Category(""UnderDevelopment"")> _" & vbCrLf)
                End If
            End With
        End Sub

        Private Sub addEvents(ByVal oClass As EA.Element)
            Dim oMethod As EA.Method
            Dim oParameter As EA.Parameter
            Dim sLeadingComma As String = ""

            With _oSourceOutput
                For Each oMethod In oClass.Methods
                    Application.DoEvents()
                    If MethodIncludesStereotype(oMethod, "event") Then
                        Application.DoEvents()
                        sLeadingComma = ""
                        .AppendText("    Public Event " & oMethod.Name & "(")
                        For Each oParameter In oMethod.Parameters
                            oParameter.Name = Canonical.CanonicalName(oParameter.Name)
                            oParameter.Type = canonicalType(oParameter.Type)
                            .AppendText(sLeadingComma & oParameter.Name & " As " & canonicalType(oParameter.Type))
                            sLeadingComma = ", "
                        Next
                        .AppendText(")")
                        If oMethod.Notes.Length > 0 Then
                            .AppendText("      ' " & oMethod.Notes)
                        End If
                        .AppendText(vbCrLf)

                        sLeadingComma = ""
                        .AppendText("    Public Sub RaiseEvent_" & oMethod.Name & "(")
                        For Each oParameter In oMethod.Parameters
                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
                            sLeadingComma = ", "
                        Next
                        .AppendText(")" & vbCrLf)
                        sLeadingComma = ""
                        .AppendText("        RaiseEvent " & oMethod.Name & "(")
                        For Each oParameter In oMethod.Parameters
                            .AppendText(sLeadingComma & oParameter.Name)
                            sLeadingComma = ", "
                        Next
                        .AppendText(")" & vbCrLf)
                        .AppendText("    End Sub" & vbCrLf)
                        .AppendText(vbCrLf)
                    End If
                Next
                .AppendText(vbCrLf)
            End With
        End Sub

        Private Sub addSynchronousCalls(ByVal oClass As EA.Element)
            Dim oMethod As EA.Method
            Dim oParameter As EA.Parameter
            Dim sLeadingComma As String = ""
            Dim oOptionalParameters As Collection
            Dim oRequiredParameters As Collection
            Dim sBehavior As String
            Dim sStaticString As String

            With _oSourceOutput
                For Each oMethod In oClass.Methods
                    Application.DoEvents()
                    If Not MethodIncludesStereotype(oMethod, "event") Then
                        If oMethod.IsStatic Then
                            sStaticString = "Shared "
                        Else
                            sStaticString = ""
                        End If

                        If oMethod.Behavior.Length > 0 Then
                            sBehavior = StripRichTextFormat(oMethod.Behavior)
                        Else
                            Dim sMessage As String = """Method not yet implemented: " & _oDomain.Name & "." & oClass.Name & "." & oMethod.Name & """"
                            sBehavior = "            NUnit.Framework.Assert.Ignore(" & sMessage & ")" & vbCrLf & _
                            "            Console.Writeline(" & sMessage & ")" & vbCrLf
                        End If

                        oMethod.Name = Canonical.CanonicalName(oMethod.Name)
                        sLeadingComma = ""

                        .AppendText("       '________________________________________________________________ Method: " & oMethod.Name & vbCrLf)

                        If oMethod.Notes.Length > 0 Then
                            .AppendText("        ''' <summary>" & vbCrLf)
                            For Each sLine As String In Split(oMethod.Notes, vbCrLf)
                                .AppendText("        ''' " & sLine & vbCrLf)
                            Next
                            .AppendText("        ''' </summary>" & vbCrLf)
                        End If

                        oMethod.ReturnType = canonicalType(oMethod.ReturnType)

                        If MethodIncludesStereotype(oMethod, "setup") Then
                            .AppendText("        <NUnit.Framework.Setup()> _" & vbCrLf)
                            If oMethod.ReturnType.Length > 0 Then           ' if there is a return type (other than 'void')
                                Throw New ApplicationException("An NUnit setup method cannot return a value: " & oMethod.Name)
                            Else
                                .AppendText("        Public " & sStaticString & "Sub " & oMethod.Name & "(")
                            End If
                        Else
                            If oMethod.ReturnType.Length > 0 Then           ' if there is a return type (other than 'void')
                                .AppendText("        Public " & sStaticString & "Function " & oMethod.Name & "(")
                            Else
                                .AppendText("        Public " & sStaticString & "Sub " & oMethod.Name & "(")
                            End If
                        End If

                        oOptionalParameters = New Collection
                        oRequiredParameters = New Collection
                        For Each oParameter In oMethod.Parameters
                            Application.DoEvents()
                            oParameter.Name = Canonical.CanonicalName(oParameter.Name)
                            oParameter.Type = canonicalType(oParameter.Type)
                            If oParameter.Default.Length > 0 Then           ' parameters with default values are assumed to be optional
                                oParameter.Default = Regex.Replace(oParameter.Default.ToString.Trim, "null", "Nothing", RegexOptions.IgnoreCase)
                                oOptionalParameters.Add(oParameter)
                            Else
                                oRequiredParameters.Add(oParameter)
                            End If
                        Next

                        For Each oParameter In oRequiredParameters
                            .AppendText(sLeadingComma & oParameter.Name & " As " & oParameter.Type)
                            sLeadingComma = ", "
                        Next

                        For Each oParameter In oOptionalParameters
                            .AppendText(sLeadingComma & "Optional " & oParameter.Name & " As " & oParameter.Type & " = " & oParameter.Default)
                            sLeadingComma = ", "
                        Next

                        If oMethod.ReturnType.Length > 0 Then           ' if there is a return type
                            .AppendText(") As " & oMethod.ReturnType & vbCrLf)
                            .AppendText(sBehavior)
                            .AppendText("        End Function" & vbCrLf & vbCrLf)
                        Else
                            .AppendText(")" & vbCrLf)
                            .AppendText(sBehavior)
                            .AppendText("        End Sub" & vbCrLf & vbCrLf)
                        End If
                    End If
                Next
            End With
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

        Private Sub addStateMachine(ByVal oClass As EA.Element)
            Dim oState As EA.Element
            Dim oConnector As EA.Connector
            Dim sArgumentList As String = ""
            Dim bStateMachineHeaderAdded As Boolean = False
            Dim oSupplierState As EA.Element = Nothing
            Dim oPopulateTransitions As New Collection
            Dim sTransitionLine As String
            Dim sDomainName As String = _oDomain.Name
            Dim Pigtails As New Collection
            Dim sPigtail As String
            Dim iClassStateMachineID As Integer
            Dim oStateNameUniqueBucket As New Collection

            Try
                iClassStateMachineID = getClassStateMachineID(oClass)

                If _oDomain.States.Count > 0 Then
                    For Each oState In _oDomain.States
                        If oState.ParentID = iClassStateMachineID Then
                            _bActiveClass = True
                            Exit For
                        End If
                    Next

                    If _bActiveClass Then               ' if this class has a state machine
                        With _oSourceOutput
                            .AppendText("        Public Overrides Sub DispatchEvent(ByRef oEvent As XEvent, ByVal bIsSelfDirectedEvent As Boolean)" & vbCrLf)
                            .AppendText("            Dim currentStateAtEntry As Integer = _currentState" & vbCrLf)
                            .AppendText("            Dim sSelfIcon As String = """"" & vbCrLf)
                            .AppendText("            Dim bQuiet As Boolean = False" & vbCrLf)
                            .AppendText(vbCrLf)
                            .AppendText("            If bIsSelfDirectedEvent Then" & vbCrLf)
                            .AppendText("                sSelfIcon = ""[""" & vbCrLf)
                            .AppendText("            End If" & vbCrLf)
                            .AppendText(vbCrLf)
                            addNormalPigtailTransition(Pigtails, iClassStateMachineID)
                            If Pigtails.Count > 0 Then
                                .AppendText("            Select Case (oEvent.EventID)           ' pigtail transitions (from any state to the target state)" & vbCrLf)
                                For Each sPigtail In Pigtails
                                    Application.DoEvents()
                                    .AppendText(sPigtail)
                                Next
                                .AppendText("                Case Else" & vbCrLf)
                            End If
                            .AppendText("                    Select Case (_currentState)" & vbCrLf)
                            For Each oState In _oDomain.States
                                Application.DoEvents()
                                If oState.ParentID = iClassStateMachineID Then
                                    oPopulateTransitions = New Collection

                                    For Each oConnector In oState.Connectors
                                        Application.DoEvents()
                                        addNormalTransition(oConnector, oState, sArgumentList, oPopulateTransitions, oClass)
                                    Next

                                    If oPopulateTransitions.Count > 0 Then
                                        .AppendText("                        Case STATE_" & Canonical.CanonicalName(oState.Name) & vbCrLf)
                                        .AppendText("                            Select Case (oEvent.EventID)" & vbCrLf)
                                        For Each sTransitionLine In oPopulateTransitions
                                            .AppendText(sTransitionLine)
                                        Next
                                        .AppendText("                                Case Else" & vbCrLf)
                                        .AppendText("                                    CannotHappenErrorHandler(""" & Canonical.CanonicalName(oClass.Name) & """, Me, _currentState, oEvent)" & vbCrLf)
                                        .AppendText("                            End Select " & vbCrLf)
                                        .AppendText(vbCrLf)
                                    End If
                                End If
                            Next
                            .AppendText("                        Case Else" & vbCrLf)
                            .AppendText("                            TerminalStateErrorHandler(""" & oClass.Name & """, Me, _currentState, oEvent)" & vbCrLf)
                            .AppendText("                    End Select" & vbCrLf)
                            If Pigtails.Count > 0 Then
                                .AppendText("            End Select" & vbCrLf)
                            End If
                            .AppendText("            AnnounceStateTransition(bQuiet, ""ST "" & Me._sInstanceName & "" "" & ArchitecturalGlobals.StateNameString(currentStateAtEntry) & "" --["" & sSelfIcon & EventNameString(oEvent.EventID) & ""]-> "" & ArchitecturalGlobals.StateNameString(_currentState))" & vbCrLf)
                            .AppendText("        End Sub" & vbCrLf)
                            .AppendText(vbCrLf)

                            For Each oState In _oDomain.States
                                Application.DoEvents()
                                If oState.ParentID = iClassStateMachineID Then
                                    If oState.Subtype = EA_TYPE.INITIAL_STATE Then        ' if this state is the "meatball" initial state
                                        If oState.Connectors.Count = 1 Then
                                            Dim oOriginalState As EA.Element = _oDomain.States(CType(oState.Connectors.GetAt(0), EA.Connector).SupplierID.ToString)
                                            _sInitialState = Canonical.CanonicalName(oOriginalState.Name)
                                        Else
                                            MsgBox("An initial state in the state model for class '" & oClass.Name & "' has no transition out", MsgBoxStyle.Critical)
                                        End If
                                    End If

                                    If IsUnique(oState.Name, oStateNameUniqueBucket) Then
                                        addState(oState, oClass)
                                    End If
                                End If
                            Next
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
                                        ByRef sArgumentList As String, _
                                        ByVal oPopulateTransitions As Collection, _
                                        ByVal oClass As EA.Element)
            Dim oClientState As EA.Element
            Dim sEvent As String
            Dim sTokens() As String
            Dim oSupplierState As EA.Element = Nothing
            Dim sToStateName As String
            Dim sTransitionLine As String
            Dim oErrorHandler As New sjmErrorHandler
            Dim oEVENT_ErrorHandler As New sjmErrorHandler

            Try
                oErrorHandler.SupplementalInformation = "Class: " & oClass.Name & ", State: " & oState.Name & ", Connector: " & oConnector.Name
                If _oDomain.States.Contains(oConnector.ClientID.ToString) Then                      ' if "to" state is a normal state
                    oSupplierState = _oDomain.States(oConnector.ClientID.ToString)
                    If oSupplierState Is oState Then
                        If _oDomain.States.Contains(oConnector.SupplierID.ToString) Then
                            oClientState = _oDomain.States(oConnector.SupplierID.ToString)

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
                                        sArgumentList = sTokens(1)
                                    End If
                                End If
                                sToStateName = Canonical.CanonicalName(oState.Name)

                                Try
                                    oEVENT_ErrorHandler.SupplementalInformation = "EVENT_" & sEvent
                                    _oEvents.Add(sEvent)
                                    If oClientState.Subtype = EA_TYPE.SYNCH_STATE Then  ' if this state is an ignore marker
                                        sTransitionLine = "                                Case EVENT_" & sEvent & "   ' ignored" & vbCrLf & _
                                                          "                                    ' do nothing" & vbCrLf
                                    Else
                                        sTransitionLine = "                                Case EVENT_" & sEvent & vbCrLf & _
                                                          "                                    bQuiet = action_" & Canonical.CanonicalName(oClientState.Name) & "(oEvent)" & vbCrLf
                                    End If
                                    If Not oPopulateTransitions.Contains(sEvent) Then
                                        oErrorHandler.SupplementalInformation = "POP_" & sEvent
                                        oPopulateTransitions.Add(sTransitionLine, sEvent)
                                    End If
                                Catch ex As Exception
                                    oEVENT_ErrorHandler.Announce(ex)
                                End Try
                            End If
                        End If
                    End If
                End If
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
                                        .Add("                Case EVENT_" & sEvent & vbCrLf)
                                        .Add("                    action_" & Canonical.CanonicalName(oSupplierState.Name) & "(oEvent)" & vbCrLf)
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

            Try
                bQuiet = ElementIncludesStereotype(oState, "quiet")
                bOmit = ElementIncludesStereotype(oState, "omit")

                If (Not bOmit) And (oState.Name.Length > 0) Then
                    With _oSourceOutput
                        If oState.Subtype <> EA_TYPE.SYNCH_STATE Then           ' if this state is NOT an "ignore event" state
                            sLines = Split(oState.Notes, vbCrLf)
                            .AppendText("       '________________________________________________________________ Action -- " & Canonical.CanonicalName(oClass.Name) & ": " & Canonical.CanonicalName(oState.Name) & " (v" & oState.Version & ")" & vbCrLf & vbCrLf)


                            .AppendText("        Private Function action_" & Canonical.CanonicalName(oState.Name) & "(ByVal oEvent As XEvent) As Boolean" & vbCrLf)
                            .AppendText("            '.............. begin action code .............." & vbCrLf & vbCrLf)
                            For Each sLine In sLines
                                .AppendText(StripRichTextFormat(sLine) & vbLf)
                            Next
                            .AppendText(vbCrLf & "            '............... end action code ..............." & vbCrLf)

                            Select Case oState.Subtype
                                Case EA_TYPE.FINAL_STATE
                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
                                    .AppendText("            XUnit.Framework.Assert.Pass()        ' this assert never returns if NUnit is running the test" & vbCrLf)

                                Case EA_TYPE.EXIT_STATE
                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
                                    .AppendText("            XUnit.Framework.Assert.Fail(oEvent)  ' this assert never returns if NUnit is running the test" & vbCrLf)

                                Case EA_TYPE.TERMINATE_STATE
                                    .AppendText("            deleteSelf                           ' this state is terminal, this instance self-destructs" & vbCrLf)
                                    .AppendText("            XUnit.Framework.Assert.Fail(oEvent)  ' this assert never returns if NUnit is running the test" & vbCrLf)

                                Case Else
                                    .AppendText("            me._currentState = STATE_" & Canonical.CanonicalName(oState.Name) & vbCrLf)
                            End Select

                            If bQuiet Then
                                .AppendText("            Return true    ' return QUIET" & vbCrLf)
                            Else
                                '.AppendText("            ArchitecturalGlobals.DebugWriteLine(""        [STATE: " & Canonical.CanonicalName(oState.Name) & " (" & GetStateTypeName(oState) & ")]"")" & vbCrLf)
                                .AppendText("            Return false   ' return NOT QUIET" & vbCrLf)
                            End If
                            .AppendText("        End Function" & vbCrLf & vbCrLf)
                        End If
                    End With
                Else
                    Debug.WriteLine("Omitting state (by stereotype 'omit'): " & oState.Name)
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

            sOtherClassName = Canonical.CanonicalName(sOtherClassName)
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

        'Private Sub addRelink(ByVal oClass As EA.Element, ByVal oConnectors As Collection)
        '    If Not _bIsSupertype Then
        '        With _oSourceOutput
        '            .AppendText(vbCrLf)
        '            .AppendText("        Public Shared Sub RelinkFromXML(ByVal oXMLDocument As XmlDocument)					 " & vbCrLf)
        '            .AppendText("            For Each x" & oClass.Name & " As XmlElement In oXMLDocument.SelectNodes(""//" & oClass.Name & "s/*"")		 " & vbCrLf)
        '            .AppendText("                With x" & oClass.Name & "																 " & vbCrLf)

        '            .AppendText("                    Dim o" & oClass.Name & " As " & oClass.Name & " = Nothing						   " & vbCrLf)
        '            .AppendText("                    Dim s" & oClass.Name & "ID As String = x" & oClass.Name & ".GetAttribute(""ID"")  " & vbCrLf)
        '            .AppendText("                    For Each o" & oClass.Name & " In " & oClass.Name & "." & oClass.Name & "s		   " & vbCrLf)
        '            .AppendText("                        If o" & oClass.Name & ".Guid = s" & oClass.Name & "ID Then					   " & vbCrLf)
        '            .AppendText("                            Exit For																   " & vbCrLf)
        '            .AppendText("                        End If																		   " & vbCrLf)
        '            .AppendText("                    Next																			   " & vbCrLf)

        '            For Each oConnector As EA.Connector In oConnectors
        '                addRelatedRelink(oConnector, oClass)
        '            Next

        '            .AppendText("                End With																 " & vbCrLf)
        '            .AppendText("            Next																		 " & vbCrLf)
        '            .AppendText("        End Sub																		 " & vbCrLf)
        '        End With
        '    End If
        'End Sub

        'Private Sub addDehydrate(ByVal oClass As EA.Element, ByVal oAttributes As Collection, ByVal oConnectors As Collection)
        '    Dim hUnique As New Hashtable

        '    If Not _bIsSupertype Then
        '        With _oSourceOutput
        '            .AppendText("        Public Shared Sub DehydrateToXML(ByVal oXMLBuilder As XMLBuilder)" & vbCrLf)
        '            .AppendText("            With oXMLBuilder" & vbCrLf)
        '            .AppendText("                Dim x" & oClass.Name & "s As XmlElement = .SetElement(""" & oClass.Name & "s"","""")" & vbCrLf)
        '            .AppendText("                For Each o" & oClass.Name & " As " & oClass.Name & " In " & oClass.Name & "s" & vbCrLf)
        '            .AppendText("                    Dim x" & oClass.Name & " As XmlElement = .SetElement(""" & oClass.Name & """, x" & oClass.Name & "s, _" & vbCrLf)

        '            For Each sAttribute As String In oAttributes
        '                .AppendText("                                                           """ & sAttribute & """, o" & oClass.Name & "." & sAttribute & ", _" & vbCrLf)
        '            Next
        '            .AppendText("                                                           ""ID"", o" & oClass.Name & "._sGuid)" & vbCrLf)
        '            .AppendText(vbCrLf)

        '            For Each oConnector As EA.Connector In oConnectors
        '                addRelatedDehydrate(oConnector, oClass)
        '            Next

        '            .AppendText("                Next" & vbCrLf)
        '            .AppendText("            End With" & vbCrLf)
        '            .AppendText("        End Sub" & vbCrLf)
        '            .AppendText(vbCrLf)
        '        End With
        '    End If
        'End Sub

        'Private Sub addRehydrate(ByVal oClass As EA.Element, ByVal oAttributes As Collection)
        '    If Not _bIsSupertype Then
        '        With _oSourceOutput
        '            .AppendText("        Public Shared Sub RehydrateFromXML(ByVal oXMLDocument As XmlDocument)			 " & vbCrLf)
        '            .AppendText("            For Each x" & oClass.Name & " As XmlElement In oXMLDocument.SelectNodes(""//" & oClass.Name & "s/*"") " & vbCrLf)
        '            .AppendText("                With x" & oClass.Name & "														 " & vbCrLf)

        '            For Each sAttributeName As String In oAttributes
        '                .AppendText("                    Dim s" & sAttributeName & " As String = .GetAttribute(""" & sAttributeName & """)		 " & vbCrLf)
        '            Next
        '            .AppendText("                    Dim sID As String = .GetAttribute(""ID"")		 " & vbCrLf)

        '            .AppendText("                    Dim oNew" & oClass.Name & " As New " & oClass.Name & " With { _" & vbCrLf)
        '            For Each sAttributeName As String In oAttributes
        '                .AppendText("                                                           ." & sAttributeName & " = s" & sAttributeName & ", _		 " & vbCrLf)
        '            Next
        '            .AppendText("                                                           ._sGuid = sID" & "}" & vbCrLf)
        '            .AppendText("                End With														 " & vbCrLf)
        '            .AppendText("            Next																 " & vbCrLf)
        '            .AppendText("        End Sub																 " & vbCrLf)
        '        End With
        '    End If
        'End Sub

        'Private Sub addRelatedRelink(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
        '    Dim sSupplierCardinality As String
        '    Dim sClientCardinality As String
        '    Dim sCardinalityComment As String = ""
        '    Dim sOtherClassName As String

        '    With _oSourceOutput

        '        If oConnector.Type = "Association" Then
        '            If oConnector.Notes = _oDomain.EAClass(oConnector.SupplierID).Name Then
        '                sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.ClientID).Name)
        '                sSupplierCardinality = oConnector.ClientEnd.Cardinality
        '                sClientCardinality = oConnector.SupplierEnd.Cardinality
        '            Else
        '                sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.SupplierID).Name)
        '                sSupplierCardinality = oConnector.SupplierEnd.Cardinality
        '                sClientCardinality = oConnector.ClientEnd.Cardinality
        '            End If
        '            oClass.Name = Canonical.CanonicalName(oClass.Name)
        '            oConnector.Name = Canonical.CanonicalName(oConnector.Name)

        '            .AppendText(vbCrLf)
        '            .AppendText("                    For Each x" & sOtherClassName & " As XmlElement In x" & oClass.Name & ".SelectNodes("".//" & oConnector.Name & "_" & sOtherClassName & """)	 " & vbCrLf)
        '            .AppendText("                        Dim s" & sOtherClassName & "ID As String = x" & sOtherClassName & ".GetAttribute(""ID"")  " & vbCrLf)
        '            .AppendText("                        For Each o" & sOtherClassName & " As " & sOtherClassName & " In " & sOtherClassName & "." & sOtherClassName & "s		   " & vbCrLf)
        '            .AppendText("                            If o" & sOtherClassName & ".Guid = s" & sOtherClassName & "ID Then					   " & vbCrLf)
        '            .AppendText("					             o" & oClass.Name & ".relate_" & oConnector.Name & "_" & sOtherClassName & "_Force(o" & sOtherClassName & ")" & vbCrLf)
        '            .AppendText("                                Exit For																   " & vbCrLf)
        '            .AppendText("                            End If																		   " & vbCrLf)
        '            .AppendText("                        Next																			   " & vbCrLf)
        '            .AppendText("                    Next																 " & vbCrLf)
        '        End If
        '    End With
        'End Sub

        'Private Sub addRelatedDehydrate(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
        '    Dim sSupplierCardinality As String
        '    Dim sClientCardinality As String
        '    Dim sCardinalityComment As String = ""
        '    Dim sOtherClassName As String

        '    With _oSourceOutput
        '        If oConnector.Type = "Association" Then
        '            If oConnector.Notes = _oDomain.EAClass(oConnector.SupplierID).Name Then    ' borrowed this intrinsic property to carry the name of the class participating in the relationship
        '                sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.ClientID).Name)
        '                sSupplierCardinality = oConnector.ClientEnd.Cardinality
        '                sClientCardinality = oConnector.SupplierEnd.Cardinality
        '            Else
        '                sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.SupplierID).Name)
        '                sSupplierCardinality = oConnector.SupplierEnd.Cardinality
        '                sClientCardinality = oConnector.ClientEnd.Cardinality
        '            End If
        '            oClass.Name = Canonical.CanonicalName(oClass.Name)
        '            oConnector.Name = Canonical.CanonicalName(oConnector.Name)

        '            .AppendText(vbCrLf)
        '            Select Case sSupplierCardinality
        '                Case "0..1", "1"
        '                    .AppendText("                    If o" & oClass.Name & "." & oConnector.Name & "_" & sOtherClassName & " IsNot Nothing Then" & vbCrLf)
        '                    .AppendText("                        .SetElement(""" & oConnector.Name & "_" & sOtherClassName & """, x" & oClass.Name & ", ""ID"", o" & oClass.Name & "." & oConnector.Name & "_" & sOtherClassName & "._sGuid)" & vbCrLf)
        '                    .AppendText("                    End If" & vbCrLf)

        '                Case "0..*", "1..*"
        '                    .AppendText("                    Dim x" & oConnector.Name & "_" & sOtherClassName & "s As XmlElement = .SetElement(""" & oConnector.Name & "_" & sOtherClassName & "s"", x" & oClass.Name & ")" & vbCrLf)
        '                    .AppendText("                    For Each o" & oConnector.Name & "_" & sOtherClassName & " As " & sOtherClassName & " In o" & oClass.Name & "." & oConnector.Name & "_" & sOtherClassName & "s" & vbCrLf)
        '                    .AppendText("                        .SetElement(""" & oConnector.Name & "_" & sOtherClassName & """, x" & oConnector.Name & "_" & sOtherClassName & "s, ""ID"", o" & oConnector.Name & "_" & sOtherClassName & "._sGuid)	" & vbCrLf)
        '                    .AppendText("                    Next																	    " & vbCrLf)

        '                Case Else
        '                    Throw New ApplicationException("unknown supplier cardinalilty on relationship '" & oConnector.Name & "' -- " & sSupplierCardinality)
        '            End Select
        '        End If
        '    End With
        'End Sub

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

        Private Sub addAttributes(ByVal oClass As EA.Element)
            Dim sCommentText As String
            Dim sNotesString As String = ""
            Dim oAttribute As EA.Attribute
            Dim sCanonicalAttributeName As String
            Dim sCanonicalType As String
            Dim oConnector As EA.Connector
            Dim sOtherClassName As String = ""
            Dim sCardinalityComment As String = ""
            Dim sStaticString As String
            Dim sCollectionName As String = Replace(oClass.Name & "s", "]s", "s]")

            Try
                With _oSourceOutput
                    For Each oAttribute In oClass.Attributes
                        oAttribute.Type = canonicalType(oAttribute.Type)
                        oAttribute.Name = Canonical.CanonicalName(oAttribute.Name)
                    Next

                    For Each oAttribute In oClass.Attributes
                        If oAttribute.Notes.Length > 0 Then
                            sCommentText = "      ' " & oAttribute.Notes
                        Else
                            sCommentText = ""
                        End If
                    Next

                    Dim oConstructorAttributes As New Collection
                    Dim bRequired As Boolean = assembleConstructorAttributes(oConstructorAttributes, oClass)

                    If (Not bRequired) And (oConstructorAttributes.Count > 0) Then
                        .AppendText("    												   " & vbCrLf)
                        .AppendText("    Public Sub New()								   " & vbCrLf)
                        .AppendText("        MyBase.New()	                               " & vbCrLf)
                        .AppendText("    End Sub  							               " & vbCrLf)
                    End If
                    .AppendText("    												   " & vbCrLf)
                    .AppendText("        Public Sub New(")
                    Dim sComma As String = ""
                    For Each oAttribute In oConstructorAttributes
                        .AppendText(sComma & "ByVal " & oAttribute.Name & " As " & oAttribute.Type)
                        sComma = ", "
                    Next
                    .AppendText(")" & vbCrLf)
                    For Each oAttribute In oConstructorAttributes
                        .AppendText("        m_" & ATTRIBUTE_PREFIX & oAttribute.Name & " = " & oAttribute.Name & vbCrLf)
                    Next
                    .AppendText("             m_o" & _oEAElement.Name & "s.Add(Me)  " & vbCrLf)
                    .AppendText("             " & _oDomain.Name & "_States.RegisterStateNames()" & vbCrLf)
                    .AppendText("             " & _oDomain.Name & "_Events.RegisterEventNames()" & vbCrLf)
                    .AppendText("             initialStateSetup()              ' execute any custom intiailzation code needed by the state machine" & vbCrLf)
                    .AppendText("         End Sub" & vbCrLf)
                    .AppendText(vbCrLf)

                    .AppendText("         Private Sub initialStateSetup()" & vbCrLf)
                    If _sInitialState.Length > 0 Then
                        .AppendText("             action_" & _sInitialState & "(Nothing)    ' execute initial state action (sets _currentState)" & vbCrLf)
                    Else
                        .AppendText("             ' do nothing" & vbCrLf)
                    End If
                    .AppendText("         End Sub" & vbCrLf)
                    .AppendText(vbCrLf)

                    .AppendText("         Public Shared ReadOnly Property " & sCollectionName & "() As List(Of " & _oEAElement.Name & ")" & vbCrLf)
                    .AppendText("             Get" & vbCrLf)
                    .AppendText("                 Return m_o" & _oEAElement.Name & "s" & vbCrLf)
                    .AppendText("             End Get" & vbCrLf)
                    .AppendText("         End Property" & vbCrLf)
                    .AppendText(vbCrLf)

                    For Each oAttribute In oClass.Attributes
                        Application.DoEvents()

                        Dim sVisibility As String = getAttributeVisibilitykeywork(oAttribute)

                        If oAttribute.IsStatic Then
                            sStaticString = "Shared "
                        Else
                            sStaticString = ""
                        End If


                        sCanonicalType = canonicalType(oAttribute.Type)
                        sCanonicalAttributeName = Canonical.CanonicalName(oAttribute.Name)
                        .AppendText("        '________________________________________________________________ Property: " & sCanonicalAttributeName & vbCrLf & vbCrLf)


                        If oAttribute.Notes.Length > 0 Then
                            .AppendText("        ''' <summary>" & vbCrLf)
                            For Each sLine As String In Split(oAttribute.Notes, vbCrLf)
                                .AppendText("        ''' " & sLine & vbCrLf)
                            Next
                            .AppendText("        ''' </summary>" & vbCrLf)
                        End If

                        Dim sInitialValue As String = ""
                        If oAttribute.Default.Length > 0 Then
                            sInitialValue = " = " & oAttribute.Default & " "
                        End If
                        .AppendText(("        " & sVisibility & " m_" & sStaticString & sCanonicalAttributeName & " As " & sCanonicalType & sInitialValue).PadRight(CONSTANTS_COLUMN) & vbCrLf)
                        .AppendText("        Public " & sStaticString & "Property " & ATTRIBUTE_PREFIX & sCanonicalAttributeName & "() As " & sCanonicalType & vbCrLf)
                        .AppendText("            Get" & vbCrLf)
                        .AppendText("                Return m_" & sCanonicalAttributeName & vbCrLf)
                        .AppendText("            End Get" & vbCrLf)
                        .AppendText("            Set(ByVal value As " & sCanonicalType & ")" & vbCrLf)
                        .AppendText("                m_" & sCanonicalAttributeName & " = value" & vbCrLf)
                        .AppendText("            End Set" & vbCrLf)
                        .AppendText("        End Property" & vbCrLf)
                        .AppendText(vbCrLf)
                    Next

                    For Each oConnector In oClass.Connectors
                        addAssociation(oConnector, oClass)
                    Next
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

        Private Sub addAssociation(ByVal oConnector As EA.Connector, ByVal oClass As EA.Element)
            Dim sSupplierCardinality As String
            Dim sClientCardinality As String
            Dim sCardinalityComment As String = ""
            Dim sOtherClassName As String

            Try
                With _oSourceOutput
                    If oConnector.Type = "Association" Then
                        If oClass.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
                            sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.ClientID).Name)
                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
                            sClientCardinality = oConnector.SupplierEnd.Cardinality
                        Else
                            sOtherClassName = Canonical.CanonicalName(_oDomain.EAClass(oConnector.SupplierID).Name)
                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
                            sClientCardinality = oConnector.ClientEnd.Cardinality
                        End If
                        oClass.Name = Canonical.CanonicalName(oClass.Name)
                        oConnector.Name = Canonical.CanonicalName(oConnector.Name)

                        ' cardinalityComment(oConnector, sCardinalityComment, sOtherClassName, oClass)

                        .AppendText("        '________________________________________________________________ Association: " & oConnector.Name & "_" & sOtherClassName & vbCrLf & vbCrLf)
                        Select Case sSupplierCardinality
                            Case "1", "0..1"
                                .AppendText("        Public _" & oConnector.Name & "_" & sOtherClassName & " As " & sOtherClassName & "       ' cardinality: " & sSupplierCardinality & vbCrLf)
                                .AppendText(vbCrLf)
                                Select Case sSupplierCardinality
                                    Case "0..1", "1"
                                        .AppendText("        Public ReadOnly Property " & oConnector.Name & "_" & sOtherClassName & "() As " & sOtherClassName & vbCrLf)
                                        .AppendText("            Get" & vbCrLf)
                                        .AppendText("                Return _" & oConnector.Name & "_" & sOtherClassName & vbCrLf)
                                        .AppendText("            End Get" & vbCrLf)
                                        .AppendText("        End Property" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case "0..*", "1..*"
                                        .AppendText("        Public ReadOnly Property " & oConnector.Name & "_" & sOtherClassName & "s() As List(of " & sOtherClassName & vbCrLf)
                                        .AppendText("            Get" & vbCrLf)
                                        .AppendText("                Return _" & oConnector.Name & "_" & sOtherClassName & "s" & vbCrLf)
                                        .AppendText("            End Get" & vbCrLf)
                                        .AppendText("        End Property" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case Else
                                        Throw New ApplicationException("unknown supplier cardinalilty on relationship '" & oConnector.Name & "' -- " & sSupplierCardinality)
                                End Select
                                .AppendText("        Protected Function relate_" & oConnector.Name & "_" & sOtherClassName & "_Force(ByVal o" & sOtherClassName & " As " & sOtherClassName & ") As " & oClass.Name & vbCrLf)
                                .AppendText("            Return _relate_" & oConnector.Name & "_" & sOtherClassName & "(o" & sOtherClassName & ", True)" & vbCrLf)
                                .AppendText("        End Function" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Public Function Relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ") As " & oClass.Name & vbCrLf)
                                .AppendText("            Return _relate_" & oConnector.Name & "_" & sOtherClassName & "(o" & sOtherClassName & ", False)" & vbCrLf)
                                .AppendText("        End Function" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Private Function _relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ", bAllowOverwrite as boolean) As " & oClass.Name & vbCrLf)
                                .AppendText("            If o" & sOtherClassName & " Is Me Then" & vbCrLf)
                                .AppendText("                Throw New ApplicationException(""Reflexive relationships are not currently supported: " & oConnector.Name & """)" & vbCrLf)
                                .AppendText("            End If" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("            If o" & sOtherClassName & " Is Nothing Then" & vbCrLf)
                                .AppendText("                Throw New ApplicationException(""Instance '"" & Me._sInstanceName & ""' was passed a null instance to relate through " & oConnector.Name & """)" & vbCrLf)
                                .AppendText("            End If" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("            If " & oConnector.Name & "_" & sOtherClassName & " Is Nothing  Or bAllowOverwrite Then  ' 111" & vbCrLf)
                                .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & " = o" & sOtherClassName & vbCrLf)
                                .AppendText("            Else" & vbCrLf)
                                .AppendText("                Throw New ApplicationException(""Instance '"" & Me._sInstanceName & ""' is already related through " & oConnector.Name & """)" & vbCrLf)
                                .AppendText("            End If" & vbCrLf)
                                .AppendText(vbCrLf)
                                Select Case sClientCardinality
                                    Case "0..1", "1"
                                        .AppendText("            If o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " Is Nothing  Or bAllowOverwrite Then  ' 222" & vbCrLf)
                                        .AppendText("                o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Me" & vbCrLf)
                                        .AppendText("            Else" & vbCrLf)
                                        .AppendText("                Throw New ApplicationException(""Instance '"" & o" & sOtherClassName & "._sInstanceName & ""' is already related through " & oConnector.Name & """)" & vbCrLf)
                                        .AppendText("            End If" & vbCrLf)
                                        .AppendText("            Return me" & vbCrLf)
                                        .AppendText("        End Function" & vbCrLf)
                                        .AppendText(vbCrLf)
                                        '.AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "()" & vbCrLf)
                                        '.AppendText("            If _" & oConnector.Name & "_" & sOtherClassName & " Is Nothing Then " & vbCrLf)
                                        '.AppendText("                Throw New ApplicationException(""No instance is currently related through " & oConnector.Name & """) " & vbCrLf)
                                        '.AppendText("            End If" & vbCrLf)
                                        '.AppendText("            _" & oConnector.Name & "_" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Nothing" & vbCrLf)
                                        '.AppendText("            _" & oConnector.Name & "_" & sOtherClassName & " = Nothing" & vbCrLf)
                                        '.AppendText("        End Sub" & vbCrLf)
                                        '.AppendText(vbCrLf)
                                        '.AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")" & vbCrLf)
                                        '.AppendText("            Unrelate_" & oConnector.Name & "_" & sOtherClassName & "()    ' for backward compatibility: ignore the instance passed in" & vbCrLf)
                                        '.AppendText("        End Sub" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case "0..*", "1..*"
                                        .AppendText("            If bAllowOverwrite Then   ' 333" & vbCrLf)
                                        .AppendText("                On Error Resume Next            ' ignore a duplicate error" & vbCrLf)
                                        .AppendText("                o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Add(Me)" & vbCrLf)
                                        .AppendText("            Else" & vbCrLf)
                                        .AppendText("                o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & "s.Add(Me)" & vbCrLf)
                                        .AppendText("            End If" & vbCrLf)
                                        .AppendText("            Return me  ' 666" & vbCrLf)
                                        .AppendText("        End Function" & vbCrLf)
                                        .AppendText(vbCrLf)
                                        '.AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "()" & vbCrLf)
                                        '.AppendText("            _" & oConnector.Name & "_" & sOtherClassName & " = Nothing" & vbCrLf)
                                        '.AppendText("        End Sub" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case Else
                                        Throw New ApplicationException("unknown client cardinalilty on relationship '" & oConnector.Name & "' -- " & sClientCardinality)
                                End Select

                            Case "0..*", "1..*"
                                .AppendText("        Friend _" & oConnector.Name & "_" & sOtherClassName & "s As New List(of " & sOtherClassName & ")" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Public ReadOnly Property " & oConnector.Name & "_" & sOtherClassName & "s() As List(of " & sOtherClassName & ")" & vbCrLf)
                                .AppendText("            Get" & vbCrLf)
                                .AppendText("                Return _" & oConnector.Name & "_" & sOtherClassName & "s" & vbCrLf)
                                .AppendText("            End Get" & vbCrLf)
                                .AppendText("        End Property" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Protected Function relate_" & oConnector.Name & "_" & sOtherClassName & "_Force(ByVal o" & sOtherClassName & " As " & sOtherClassName & ") As " & oClass.Name & vbCrLf)
                                .AppendText("            Return _relate_" & oConnector.Name & "_" & sOtherClassName & "(o" & sOtherClassName & ", True)" & vbCrLf)
                                .AppendText("        End Function" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Public Function Relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ") As " & oClass.Name & vbCrLf)
                                .AppendText("            Return _relate_" & oConnector.Name & "_" & sOtherClassName & "(o" & sOtherClassName & ", False)" & vbCrLf)
                                .AppendText("        End Function" & vbCrLf)
                                .AppendText(vbCrLf)
                                .AppendText("        Public Function _relate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ", bAllowOverwrite as boolean) As " & oClass.Name & vbCrLf)





                                .AppendText("            If bAllowOverwrite Then " & vbCrLf)
                                .AppendText("                On Error Resume Next            ' ignore a duplicate error" & vbCrLf)
                                .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & "s.Add(o" & sOtherClassName & ")" & vbCrLf)
                                .AppendText("            Else" & vbCrLf)
                                .AppendText("                _" & oConnector.Name & "_" & sOtherClassName & "s.Add(o" & sOtherClassName & ")" & vbCrLf)
                                .AppendText("            End If" & vbCrLf)



                                Select Case sClientCardinality
                                    Case "0..1", "1"
                                        .AppendText("            o" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Me" & vbCrLf)
                                        .AppendText("            Return me" & vbCrLf)
                                        .AppendText("        End Function" & vbCrLf)
                                        .AppendText(vbCrLf)
                                        ''.AppendText("        Public Sub Unrelate_" & oConnector.Name & "_" & sOtherClassName & "(ByVal o" & sOtherClassName & " As " & sOtherClassName & ")" & vbCrLf)
                                        ''.AppendText("            If _" & oConnector.Name & "_" & sOtherClassName & " Is Nothing Then " & vbCrLf)
                                        ''.AppendText("                Throw New ApplicationException(""No instance is currently related through " & oConnector.Name & """) " & vbCrLf)
                                        ''.AppendText("            End If" & vbCrLf)
                                        ''.AppendText("            _" & oConnector.Name & "_" & sOtherClassName & "._" & oConnector.Name & "_" & oClass.Name & " = Nothing" & vbCrLf)
                                        ''.AppendText("            _" & oConnector.Name & "_" & sOtherClassName & " = Nothing" & vbCrLf)
                                        ''.AppendText("        End Sub" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case "0..*", "1..*"
                                        .AppendText("            o" & sOtherClassName & "." & oConnector.Name & "_" & oClass.Name & "s.Add(Me)  " & vbCrLf)
                                        .AppendText("            Return me" & vbCrLf)
                                        .AppendText("        End Function" & vbCrLf)
                                        .AppendText(vbCrLf)

                                    Case Else
                                        Throw New ApplicationException("unknown client cardinalilty on relationship '" & oConnector.Name & "' -- " & sClientCardinality)
                                End Select

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

        Private Function canonicalType(ByVal sType As String) As String
            sType = sType.Trim
            Dim sReturnTypeString As String = sType

            If sType.Length > 0 Then
                Select Case sType.ToLower
                    Case "boolean", "bool"
                        sReturnTypeString = "Boolean"

                    Case "void"
                        sReturnTypeString = ""

                    Case "unsigned long"
                        sReturnTypeString = "Long"

                    Case "byte", "unsigned char"
                        sReturnTypeString = "Byte"

                    Case "int"
                        sReturnTypeString = "Integer"

                    Case "char"
                        sReturnTypeString = "String"

                    Case "float", "double"
                        sReturnTypeString = "Double"

                    Case "void"
                        sReturnTypeString = "Object"
                End Select
            End If

            Return Canonical.CanonicalName(sReturnTypeString)
        End Function

    End Class
End Class
