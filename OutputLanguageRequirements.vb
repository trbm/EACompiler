Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO
Imports System.Text

Public Class OutputLanguageRequirements
    Implements IOutputLanguage

    Private oSourceOutput As RichTextBox
    Private oSRSOutputFile As OutputFile
    Private oMainStringBuilder As New StringBuilder
    Private oBoilerPlateInputFile As InputFile
    Private Const MAJOR_SECTION_NUMBER As String = "4"

    Public Sub CreateDomains(ByVal oRepository As EA.Repository, ByVal bIncludeDebug As Boolean, ByVal sXSLfilename As String, ByVal sOutputFileExtension As String) Implements IOutputLanguage.CreateDomains
        Dim iPackageCount As Integer = 0

        Try
            gStatusBox = New frmStatusBox
            gStatusBox.VersionStamp = "EA Model Compiler (v" & VERSION & ")"

            Dim sSRSFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), "SoftwareRequirementsSpecification.rtf")

            'Dim sSRSBoilerplate As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), "TD-00XXX PROJECT BOILERPLATE SRS.rtf")
            'If File.Exists(sSRSBoilerplate) Then
            OutputFile.ClearFilesCreated()
            oSRSOutputFile = New OutputFile(sSRSFilename, True)

            'oBoilerPlateInputFile = New InputFile(sSRSBoilerplate)

            'Dim oFileStringBuilder As New StringBuilder(oBoilerPlateInputFile.ToString)

            For Each oModel As EA.Package In oRepository.Models
                analyzePackage(oModel, oRepository)
            Next

            Dim sRequirementsText As String = oMainStringBuilder.ToString

            'oFileStringBuilder = oFileStringBuilder.Replace("[REQUIREMENTS]", sRequirementsText)        ' add all the requirements entries

            'oFileStringBuilder = oFileStringBuilder.Replace(" TBD", " {\highlight7 TBD}")               ' add highlighting for all 'TBD' entries 

            'oSRSOutputFile.Add(oFileStringBuilder.ToString)
            'oSRSOutputFile.Close()

            'Else
            'MsgBox("Could not find template file: " + vbCrLf + sSRSBoilerplate)
            'End If
            gStatusBox.FadeAway()
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Sub addPostamble()
        With oSRSOutputFile
            .Add("}")
        End With
    End Sub

    Private Sub addPreamble()
        With oSRSOutputFile
            .Add("{\rtf1\ansi\deff0				 ")
            .Add("								 ")
            .Add("  {\fonttbl					 ")
            .Add("  {\f0 Century Gothic;}		 ")
            .Add("  }							 ")
            .Add("   							 ")
            .Add("{\footer\par\pard\li200\ri200\qj\fs16\i This file was created automatically from a requirements model by EACompiler (v" + VERSION + "). Do not edit it directly.        Page \chpgn}")
        End With
    End Sub

    Private Sub analyzePackage(ByVal oPackage As EA.Package, ByVal oRepository As EA.Repository)
        For Each oChildPackage As EA.Package In oPackage.Packages
            oSourceOutput = New RichTextBox
            Dim oNewDomain As Domain = New Domain(oChildPackage, oRepository, oSourceOutput, oMainStringBuilder)
            analyzePackage(oChildPackage, oRepository)          ' recurse down the package chain
        Next
    End Sub

    Protected Class Domain
        Private _ClassById As Collection
        Private _Triggers As Collection
        Private _Enumerations As Collection
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
        Private _ModelEnumerations As Collection
        Private _Requirements As Collection
        Private _TestCases As Collection

        Private _oTestFixtureElement As EA.Element
        Private _oSourceOutput As RichTextBox
        Private _oRepository As EA.Repository
        Private _sPackageId As String
        Private _oProject As EA.Project
        Private _oPackage As EA.Package
        Private _IsRealized As Boolean
        Private _Name As String

        Private _oDomainStringBuilder As StringBuilder

        Public ReadOnly Property ClassByID() As Collection
            Get
                Return _ClassById
            End Get
        End Property

        Public ReadOnly Property TestCases() As Collection
            Get
                Return _TestCases
            End Get
        End Property

        Public ReadOnly Property Requirements() As Collection
            Get
                Return _Requirements
            End Get
        End Property

        Public ReadOnly Property Name() As String
            Get
                Return _Name
            End Get
        End Property

        Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository, ByRef oSourceOutput As RichTextBox, ByVal oStringBuilder As StringBuilder)
            Try
                giNextStateID = oPackage.Name.GetHashCode
                giNextEventID = giNextStateID

                _oDomainStringBuilder = oStringBuilder
                _oSourceOutput = oSourceOutput
                _oRepository = oRepository
                _oPackage = oPackage
                _sPackageId = oPackage.PackageID
                _Name = CanonicalClassName(oPackage.Name)

                _oTestFixtureElement = Nothing

                OutputFile.ClearFilesCreated()

                _Boundarys = New Collection
                _Notes = New Collection
                _ObjectInstances = New Collection
                _TestElements = New Collection
                _ClassById = New Collection
                _Triggers = New Collection
                _Enumerations = New Collection
                _States = New Collection
                _ElementById = New Collection
                _StateMachines = New Collection
                _InitialStates = New Collection
                _FinalStates = New Collection
                _IgnoreIndicatorStates = New Collection
                _ParentChildren = New Collection
                _ModelEnumerations = New Collection
                _Requirements = New Collection
                _TestCases = New Collection

                catalogElements()
                generateSource()

                generateRequirementString(oRepository)

                If _oSourceOutput.Text.Length > 0 Then
                    Dim sOutputFilename As String = Path.Combine(Path.GetDirectoryName(oRepository.ConnectionString), Canonical.CanonicalName(oPackage.Name) & ".cs")
                    createOutputFile(sOutputFilename, _oSourceOutput.Text, Name)
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createOutputFile(ByVal sOutputFilename As String, ByRef sFileText As String, ByVal sDomainName As String)
            If sFileText.Length > 0 Then
                OutputFile.ClearFilesCreated()
                Dim oOutputFile As OutputFile = New OutputFile(sOutputFilename, True)
                With oOutputFile
                    .Add("using TestDirector;")
                    .Add("using TestDirector.TestDirectorSchema;")
                    .Add("using NUnit.Framework;")
                    .Add()
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
                    .Add("//           Copyright © 2012,  ArrayPower, Inc.   All rights reserved.")
                    .Add("// ________________________________________________________________________________")
                    .Add("")
                    .Add("")
                    .Add("public class " + Name)
                    .Add("{")
                    .Add(sFileText)
                    .Add("}")
                End With
                oOutputFile.Close()
            End If
        End Sub

        Private Sub catalogElement(ByVal oElement As EA.Element)
            Application.DoEvents()

            If (Not _ElementById.Contains(oElement.ElementID)) Then
                _ElementById.Add(oElement, oElement.ElementID)              ' just as a debugging convenience, to look up any element from its id only
            End If

            If oElement.Name.Length = 0 Then
                oElement.Name = "NoName_" & oElement.ElementID
            End If

            Select Case oElement.MetaType
                Case "UseCase", "TestCase"
                    _TestCases.Add(oElement, oElement.ElementID)

                Case "Requirement"
                    _Requirements.Add(oElement, _Name + oElement.ElementID.ToString)

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
                    _ModelEnumerations.Add(oElement, oElement.ElementID)

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
            Try
                For Each oElement As EA.Element In _oPackage.Elements
                    catalogElement(oElement)
                Next
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createOneTest(ByVal oTest As EA.Test)
            Dim sDescription As String = vbCrLf

            If oTest.Notes.Length > 0 Then
                sDescription = "      // " + SafeDescription(oTest.Notes)
            End If

            oTest.Name = CanonicalClassName(oTest.Name)
            With _oSourceOutput
                .AppendText(vbCrLf)
                .AppendText("        [Test]" + vbCrLf)
                .AppendText("        [Category(""manual"")]" + vbCrLf)
                .AppendText("        public void " + oTest.Name + "()" + sDescription + vbCrLf)
                .AppendText("        {" + vbCrLf)
                .AppendText("            TestStep.BeginTest(""" + oTest.Name + """);" + vbCrLf)

                oTest.Input = StripRichTextFormat(oTest.Input)
                If oTest.Input.Length > 0 Then
                    Dim sLines() As String = Split(oTest.Input, vbCrLf)
                    For Each sLine As String In sLines
                        Dim sTrimLine As String = Trim(sLine)
                        If sTrimLine.Length > 0 Then
                            .AppendText("            " + sTrimLine + vbCrLf)
                        End If
                    Next
                End If
                .AppendText("        }" + vbCrLf)
            End With
        End Sub

        Private Sub createTestCase(ByVal oTestCase As EA.Element)
            oTestCase.Name = CanonicalClassName(oTestCase.Name)

            With _oSourceOutput
                .AppendText(vbCrLf)
                .AppendText("    [TestFixture()]" + vbCrLf)
                .AppendText("    public class " + oTestCase.Name + vbCrLf)
                .AppendText("    {" + vbCrLf)
                .AppendText(vbCrLf)
                .AppendText("        [TestFixtureSetUp()]" + vbCrLf)
                .AppendText("        public void TestFixtureSetup()" + vbCrLf)
                .AppendText("        {" + vbCrLf)
                .AppendText("            NUnit_TestFixture oNUnit_TestFixture = new NUnit_TestFixture { TestFixtureName = """ + oTestCase.Name + """, Version = ""0.0"" };" + vbCrLf)
                .AppendText("            InstancePopulation.CreateInitialInstancePopulation();" + vbCrLf)
                .AppendText("            TestStep.SetupTestFixture(oNUnit_TestFixture, InstancePopulation.oRequirementsDocument);" + vbCrLf)
                .AppendText(vbCrLf)
                For Each oRealizedRequirement As Object In oTestCase.Realizes
                    Dim sRequirementID As String = Regex.Match(oRealizedRequirement.Name, "[^ ]+").Value
                    .AppendText("            TestStep.RelatedRequirement(""" + sRequirementID + """);" + vbCrLf)
                    Debug.WriteLine(oRealizedRequirement.Name + "   " + sRequirementID)
                Next
                .AppendText("        }" + vbCrLf)
                .AppendText(vbCrLf)
                .AppendText("        [TestFixtureTearDown()]" + vbCrLf)
                .AppendText("        public void TestFixtureTearDown()" + vbCrLf)
                .AppendText("        {" + vbCrLf)
                .AppendText("            Reports oReports = new Reports();" + vbCrLf)
                .AppendText("            oReports.CreateReport();" + vbCrLf)
                .AppendText("        }" + vbCrLf)
                For Each oTest As EA.Test In oTestCase.Tests
                    createOneTest(oTest)
                Next
                .AppendText("    }" + vbCrLf)
            End With
        End Sub

        Private Sub generateSource()
            Dim iRequirementCounter As Integer = 0
            Dim sRequirementID As String = ""
            Dim sDescription As String = ""
            Dim oSmartMatch As SmartMatch
            Dim oRequirementIDs As New Collection
            Dim bHeaderAdded As Boolean = False

            Try
                With _oSourceOutput
                    gStatusBox.lblFilename.Text = Name
                    gStatusBox.ProgressValueMaximum = _Requirements.Count
                    For Each oRequirement As Object In _Requirements
                        Application.DoEvents()
                        If oRequirement.Name.Length > 0 Then
                            If Not bHeaderAdded Then
                                bHeaderAdded = True
                                .AppendText("    public " + Name + " (RequirementsDocument oRequirementsDocument)" + vbCrLf)
                                .AppendText("    {" + vbCrLf)
                                .AppendText(vbCrLf)
                            End If

                            oSmartMatch = New SmartMatch(oRequirement.Name.Trim, "([A-Z]+)([_0-9]+) (.+)")
                            If oSmartMatch.Matches.Count > 0 Then
                                sRequirementID = oSmartMatch.Matches(0).Groups(1).ToString + oSmartMatch.Matches(0).Groups(2).ToString
                                sDescription = SafeDescription(oSmartMatch.Matches(0).Groups(3).ToString.Trim)

                                gStatusBox.ProgressValue = iRequirementCounter
                                gStatusBox.lblClass.Text = sRequirementID

                                If Not oRequirementIDs.Contains(sRequirementID) Then
                                    oRequirementIDs.Add(sRequirementID, sRequirementID)
                                    .AppendText("        oRequirement_" + sRequirementID.PadRight(7) + " = new UncategorizedRequirement { RequirementID = """ + (sRequirementID + """,").PadRight(8) + " Description = """ + sDescription + """ };" + vbCrLf)
                                End If
                            End If
                        End If
                        iRequirementCounter += 1
                    Next

                    If iRequirementCounter > 0 Then
                        .AppendText("    }" + vbCrLf)
                        .AppendText(vbCrLf)
                        .AppendText("    // declarations" + vbCrLf)
                        For Each sRequirementID In oRequirementIDs
                            .AppendText("    static internal UncategorizedRequirement oRequirement_" + sRequirementID + ";" + vbCrLf)
                        Next
                    End If

                    For Each oTestCase As EA.Element In _TestCases
                        createTestCase(oTestCase)
                    Next

                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addSectionName(ByVal sName As String)
            Static iSectionNumber As Integer = 1
            Dim sSectionNumber As String = iSectionNumber.ToString + ". "

            With _oDomainStringBuilder
                .Append("{\par\pard\li100\ri200\qj\b\fs26 " + MAJOR_SECTION_NUMBER + "." + sSectionNumber + sName + "}")
            End With
            iSectionNumber += 1
        End Sub

        Private Sub generateRequirementString(ByRef oRepository As EA.Repository)
            Dim iRequirementCounter As Integer = 0
            Dim oUniqueSectionNames As New Collection

            Try
                With _oDomainStringBuilder
                    gStatusBox.lblFilename.Text = Name
                    gStatusBox.ProgressValueMaximum = _Requirements.Count


                    For Each oRequirementCandidate As EA.Element In _Requirements
                        For Each oConnectorCandidate As EA.Connector In oRequirementCandidate.Connectors
                            If oConnectorCandidate.MetaType = "Aggregation" And oConnectorCandidate.SupplierID = oRequirementCandidate.ElementID Then
                                If IsUnique(oRequirementCandidate.Name, oUniqueSectionNames) Then
                                    addSectionName(oRequirementCandidate.Name)
                                    For Each oConnector As EA.Connector In oRequirementCandidate.Connectors
                                        If _Requirements.Contains(_Name + oConnector.ClientID.ToString) Then
                                            addRequirement(_Requirements(_Name + oConnector.ClientID.ToString))
                                        End If
                                    Next
                                End If
                            End If
                        Next
                        iRequirementCounter += 1
                    Next
                End With

            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addRequirement(ByVal oRequirement As EA.Element)
            Dim oSmartMatch As SmartMatch
            Dim sStatus As String
            Dim sRequirementPrefix As String = ""
            Dim sRequirementNumeric As String = ""
            Dim sDescription As String = ""
            Dim oRequirementIDs As New Collection

            With _oDomainStringBuilder
                Application.DoEvents()
                If oRequirement.Name.Length > 0 Then
                    'If Not bHeaderAdded Then
                    '    bHeaderAdded = True
                    'End If

                    sStatus = extractStatus(oRequirement)

                    oSmartMatch = New SmartMatch(oRequirement.Name.Trim, "([A-Z]+)([_0-9]+) (.+)")
                    If oSmartMatch.Matches.Count > 0 Then
                        If (oSmartMatch.MatchGroupCount(0) >= 4) Then
                            sRequirementPrefix = oSmartMatch.MatchGroups(0)(1).ToString()
                            sRequirementNumeric = oSmartMatch.MatchGroups(0)(2).ToString()
                            sDescription = oSmartMatch.MatchGroups(0)(3).ToString()

                            If (sRequirementNumeric.Length >= 2) And (sRequirementPrefix.Length >= 2) Then
                                .Append("")
                                .Append("{\keep\keepn\widctlpar\par\pard\qj\fs20_____________________________________________________________________________\par")
                                .Append("{\b " + sRequirementPrefix + sRequirementNumeric + "}\par" + sStatus)
                                .Append("\li200\ri300\qj\fs20" + sDescription)
                                If oRequirement.Notes.Length > 20 Then
                                    .Append("\i   NOTES: " + oRequirement.Notes)
                                End If
                                .Append("}")
                            End If
                        End If
                    End If
                End If

            End With

        End Sub

        Private Function extractStatus(ByVal oRequirement) As String
            Dim sStatus As String = ""

            'Select Case oRequirement.Status
            '    Case "Implemented"
            '        sStatus = "Status: {\b REMOVED}"

            '    Case "Mandatory"
            '        sStatus = ""

            '    Case "Proposed"
            '        sStatus = "Status: {\i needs DESCRIPTIONiew}"
            'End Select

            Return sStatus
        End Function



    End Class


End Class

