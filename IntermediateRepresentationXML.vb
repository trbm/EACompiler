
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Control
Imports System.Xml
Imports System.IO
Imports System.Xml.Linq

Public Class IntermediateRepresentationXML
    Implements IOutputLanguage

    Private Enum EA_TYPE               ' these are EA types (see the EA help topic "Type" which have been inferred by trial and error
        FINAL_STATE = 4
        '    EXIT_STATE = 14
        IGNORE_STATE = 14
        INITIAL_STATE = 3
        ENTRY_STATE = 13
        TERMINATE_STATE = 12
        SYNCH_STATE = 6
    End Enum

    Private Const CONSTANTS_COLUMN = 90
    Private Const COLUMN_WIDTH As Integer = 55
    Private Shared ModelEnumerations As SortedDictionary(Of String, EA.Element)
    Private Shared _Repository As EA.Repository
    Private Shared _UniquePackages As Collection
    Private Shared _OutputTabName As String = "EA Compiler"
    Private Shared _sExecutionDirectory As String
    Private Shared _sModelDirectory As String
    Private Shared _sLanguage As String = "C"
    Private Shared _sOutputFileExtension As String = ".c"

    Public Shared oHashcodes As New Collection
    Public Shared lHashcodesSum As Long
    Public Shared oXMLBuilder As XMLBuilder
    Public Shared bSuppressCompletionSound As Boolean = False

    Public Shared Sub ShowOutputLine(sOutputLine As String)
        _Repository.WriteOutput(_OutputTabName, "  " + sOutputLine, 0)
    End Sub

    Public Sub CreateDomains(ByVal oRepository As EA.Repository, ByVal BuildDocumentation As Boolean, ByVal bIncludeDebug As Boolean) Implements IOutputLanguage.CreateDomains
        Try
            _sModelDirectory = Path.GetDirectoryName(oRepository.ConnectionString)
            Dim sXMLFilename As String = Path.Combine(_sModelDirectory, Path.GetFileNameWithoutExtension(oRepository.ConnectionString) & ".xml")
            Dim sOutputHeaderSourceFilename As String = Path.Combine(_sModelDirectory, Path.GetFileNameWithoutExtension(oRepository.ConnectionString) & ".h")
            Dim sOutputTLVFilename As String = Path.Combine(_sModelDirectory + "\\..", Path.GetFileNameWithoutExtension(oRepository.ConnectionString) & ".TLV.xml")

            _Repository = oRepository

            _sExecutionDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.Location)

            _UniquePackages = New Collection

            oRepository.CreateOutputTab(_OutputTabName)
            oRepository.EnsureOutputVisible(_OutputTabName)

            ModelEnumerations = New SortedDictionary(Of String, EA.Element)

            oXMLBuilder = New XMLBuilder("Model", "ModelRepository", True)
            oXMLBuilder.SetAttribute("sourceFileDirectory", _sModelDirectory, oXMLBuilder.RootElement)
            oXMLBuilder.SetAttribute("modelFile", Path.GetFileName(oRepository.ConnectionString), oXMLBuilder.RootElement)
            oXMLBuilder.SetAttribute("intermediateFile", Path.GetFileName(sXMLFilename), oXMLBuilder.RootElement)
            oXMLBuilder.SetAttribute("EACompilerVersion", Globals.VERSION, oXMLBuilder.RootElement)
            oXMLBuilder.SetAttribute("generated", DateTime.Now.ToShortDateString & ", " & DateTime.Now.ToShortTimeString, oXMLBuilder.RootElement)
            oXMLBuilder.SetElement("ModeledDomains", oXMLBuilder.RootElement)

            Dim oPackagesList As New Collection
            For Each oPackage As EA.Package In oRepository.Models.GetAt(0).Packages
                recursePackage(oPackage, oPackagesList)
            Next
            Dim sOutputSourceFilename As String = Path.Combine(_sModelDirectory, Path.GetFileNameWithoutExtension(oRepository.ConnectionString) & _sOutputFileExtension)

            If oPackagesList.Count > 0 Then
                '    Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
                '    oErrorHandler.AnnounceMessage("No packages found to compile")
                'Else
                ShowOutputLine("B e g i n   C o m p i l a t i o n")

                For Each oFoundPackage As EA.Package In oPackagesList
                    createDomain(oRepository, oFoundPackage, bIncludeDebug)
                Next
            End If

            If BuildDocumentation Then
                generateModelDocumentation(_Repository, oRepository.Models.GetAt(0).Packages(0))
            End If

            addHashcodeSum()
            oXMLBuilder.Save(sXMLFilename)

            Dim oAssembly As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Dim sFullInstallPathDLL As String = Path.GetFullPath(oAssembly.Location)
            Dim sFullInstallPath As String = Path.GetDirectoryName(sFullInstallPathDLL)
            Dim sFullXSLPath As String = sFullInstallPath + "\XSL\"

            Dim sXSLfilename As String = _sLanguage & ".xsl"

            Dim sStrippedXSLfilename As String = Path.GetFileName(sXSLfilename)
            Dim sFullSourceStylesheetFilename As String = Path.Combine(sFullXSLPath, sStrippedXSLfilename)
            Dim sFullHeaderFileStylesheetFilename As String = Path.Combine(sFullXSLPath, "H_" + sStrippedXSLfilename)
            Dim sFullTLVStylesheetFilename As String = Path.Combine(sFullXSLPath, "TagLinkedValues.xsl")


            ShowOutputLine("  XML file: " + sXMLFilename)

            If (File.Exists(sFullSourceStylesheetFilename)) Then
                ShowOutputLine("  XSL file: " + sFullSourceStylesheetFilename)
                TransformXML(sXMLFilename, sFullSourceStylesheetFilename, sOutputSourceFilename)
            End If

            If (File.Exists(sFullHeaderFileStylesheetFilename)) Then
                TransformXML(sXMLFilename, sFullHeaderFileStylesheetFilename, sOutputHeaderSourceFilename)
                ShowOutputLine("  XSL file: " + sFullHeaderFileStylesheetFilename)
                ShowOutputLine("  XSL file: " + sFullTLVStylesheetFilename)
                TransformXML(sXMLFilename, sFullTLVStylesheetFilename, sOutputTLVFilename)
            End If

            ShowOutputLine("  Output:   " + sOutputSourceFilename)
            ShowOutputLine("E n d   C o m p l i a t i o n")

            If Not bSuppressCompletionSound Then
                PlaySound("TortoiseSVN_Notification.wav", 0, SND_FILENAME)
            End If

            ShowOutputLine(" ")
        Catch ex As Exception
            Dim oErrorHandler As New sjmErrorHandler(ex)
        End Try
    End Sub

    Private Sub addHashcodeSum()
        oXMLBuilder.SetAttribute("hashcodeSum", IntermediateRepresentationXML.lHashcodesSum.ToString(), oXMLBuilder.RootElement)
    End Sub

    Private Sub createDomain(ByVal oRepository As EA.Repository, ByVal oPackage As EA.Package, ByVal bIncludeDebug As Boolean)
        If oPackage.Diagrams.Count > 0 Then
            If IsUnique(oPackage.Name, _UniquePackages) Then
                Dim oDomain As Domain = New Domain(oPackage, oRepository)      ' constructor does the work

                For Each oChildPackage As EA.Package In oPackage.Packages
                    createDomain(oRepository, oChildPackage, bIncludeDebug)
                Next
            End If
        End If
    End Sub

    Private Sub generateModelDocumentation(ByVal oRepository As EA.Repository, ByVal oPackage As EA.Package)
        Try
            Dim sDocumentationFilenameRoot = Path.Combine(_sModelDirectory, oPackage.Name)
            Dim sDocumentationFullFilename = sDocumentationFilenameRoot & ".pdf"
            Dim sTemplateName As String = "ARRAY POWER -- BASIC TEMPLATE"
            Dim oProject As EA.Project = oRepository.GetProjectInterface

            If (File.Exists(sDocumentationFullFilename)) Then
                File.Delete(sDocumentationFullFilename)
            End If

            ShowOutputLine("Generating '" & sDocumentationFullFilename & "' using template '" & sTemplateName & "'")
            oProject.RunReport(oPackage.PackageGUID, sTemplateName, sDocumentationFilenameRoot)
        Catch ex As Exception
            ShowOutputLine("Exception thrown during documentation generation for package '" & oPackage.Name & "': " & ex.Message)
        End Try
    End Sub

    Private Sub recursePackage(ByVal oNextPackage As EA.Package, ByVal oPackages As Collection)
        If PackageIncludesStereotype(oNextPackage, "C") Then        ' if this domain should be compiled to C
            _sLanguage = "C"
            _sOutputFileExtension = ".C"
            oPackages.Add(oNextPackage)
        End If

        If PackageIncludesStereotype(oNextPackage, "C#") Then       ' if this domain should be compiled to C-sharp
            _sLanguage = "CSharp"
            _sOutputFileExtension = ".cs"
            oPackages.Add(oNextPackage)
        End If

        For Each oPackage As EA.Package In oNextPackage.Packages
            recursePackage(oPackage, oPackages)
        Next
    End Sub

    Private Sub addMessageToDomain(sParentDomainName As String, xMessage As XmlElement)
        With oXMLBuilder
            Dim xModeledDomain As XmlElement = .RootElement.SelectSingleNode("//ModeledDomain [@name = '" & sParentDomainName & "']")
            If xModeledDomain IsNot Nothing Then
                Dim xDomainFunctions As XmlElement = xModeledDomain.SelectSingleNode("DomainFunctions")
                If xDomainFunctions Is Nothing Then
                    xDomainFunctions = .SetElement("DomainFunctions", xModeledDomain)
                End If
                Dim xDomainFunction As XmlElement =
                    .SetElement("DomainFunction", xDomainFunctions,
                                 "name", xMessage.GetAttribute("name"),
                                 "meaning", xMessage.GetAttribute("meaning"))

                For Each xParameter As XmlElement In xMessage.SelectNodes("Parameter")
                    .SetElement("Parameter", xDomainFunction,
                                "name", xParameter.GetAttribute("name"),
                                "dataType", xParameter.GetAttribute("dataType"),
                                "description", xParameter.GetAttribute("description"))
                Next

                For Each xImplementationBlock As XmlElement In xMessage.SelectNodes("ImplementationCode")  ' actually, there will only be at most one implementation blockImplementation
                    Dim sImplementationBlock As String = xImplementationBlock.InnerText
                    .SetText(sImplementationBlock, .SetElement("ImplementationCode", xDomainFunction))
                Next
            End If
        End With
    End Sub

    Protected Class Domain
        Private _oEADiagram As EA.Diagram
        Private _sName As String = ""
        Private _sDiagramNotes As String = ""
        Private _sDiagramVersion As String = ""
        Private _oPackage As EA.Package
        Private _IsRealized As Boolean
        Private _Triggers As Collection
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
        Private _StateNames As Collection
        Private _UniqueStateNames As Collection
        Private _StateIgnoreStatus As Collection
        Private _oAssociationNames As New Collection
        Private _UniqueEventNames As New Collection

        Public ReadOnly Property EAClass(ByVal iID As Integer) As EA.Element
            Get
                Dim oClass As EA.Element = Nothing

                If _ClassById.Contains(iID.ToString) Then
                    oClass = _ClassById.Item(iID.ToString)
                Else
                    Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
                    oErrorHandler.AnnounceMessage("Unknown class id: " & iID)
                End If
                Return oClass
            End Get
        End Property

        Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository)
            Try
                _sName = Canonical.CanonicalName(oPackage.Name)
                _oEADiagram = oPackage.Diagrams.GetAt(0)
                _sDiagramNotes = _oEADiagram.Notes
                _sDiagramVersion = _oEADiagram.Version
                _oPackage = oPackage

                ShowOutputLine("    Compiling Domain: " & _sName & "  (Language: " & _sLanguage & ")")

                IntermediateRepresentationXML.oHashcodes = New Collection
                lHashcodesSum = 0

                _ObjectInstances = New Collection
                _TestElements = New Collection
                _Triggers = New Collection
                _DataTypes = New Collection
                _States = New Collection
                _UniqueStateNames = New Collection
                _ElementById = New Collection
                _StateMachines = New Collection
                _InitialStates = New Collection
                _FinalStates = New Collection
                _IgnoreIndicatorStates = New Collection
                _ClassById = New Collection
                _ModelDataTypes = New Collection
                _Interfaces = New Collection

                _IsRealized = PackageIncludesStereotype(_oPackage, "realized")
                If Not _IsRealized Then
                    ShowOutputLine("             " + oPackage.Name)
                    For Each oElement As EA.Element In _oPackage.Elements
                        catalogElement(oElement)
                    Next
                    populateMetaModel(oPackage)

                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try

        End Sub

        Private Function extractGUID(oDiagram As EA.Diagram) As String
            Dim sDiagramGUID As String = oDiagram.DiagramGUID     ' get the GUID with braces and dashes
            sDiagramGUID = Regex.Replace(sDiagramGUID, "[{}]*", "")   ' strip off the braces
            sDiagramGUID = Regex.Replace(sDiagramGUID, "[-]+", "_")   ' replace all the dashes with underscores
            Return sDiagramGUID
        End Function

        Private Sub populateMetaModel(ByVal oPackage As EA.Package)
            Try
                With oXMLBuilder
                    Dim xDomains As XmlElement = oXMLBuilder.RootElement.SelectSingleNode("ModeledDomains")
                    Dim xDomain = .SetElement("ModeledDomain", xDomains, "name", Canonical.CanonicalName(oPackage.Name), "description", oPackage.Notes)
                    Dim xEnumerations As XmlElement = .SetElement("Enumerations", xDomain)
                    Dim xInterfaces As XmlElement = .SetElement("Interfaces", xDomain)
                    Dim oDiagram As EA.Diagram = oPackage.Diagrams.GetAt(0)
                    Dim sDiagramGUID As String = extractGUID(oDiagram)
                    Dim xDiagram = .SetElement("Diagram", xDomain, "diagramGUID", sDiagramGUID)
                    Dim xClasses = .SetElement("Classes", xDomain)

                    If PackageIncludesStereotype(oPackage, "realized") Then
                        ' do nothing with a realized domain
                    Else
                        If 0 = createClassNodes(oPackage, xClasses) Then   ' no classes found, delete the domain from the XML output
                            Dim xNodeDomains As XmlNode = xDomains
                            xNodeDomains.RemoveChild(xDomain)
                        Else
                            createEnumerationNodes(oPackage, xEnumerations)
                            createInterfaceNodes(oPackage, xInterfaces)
                            insertPopulationXML(oPackage)
                            insertBridgingXML(oPackage)
                            identifyInitialStates()
                        End If
                    End If
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub insertBridgingXML(oPackage As EA.Package)                ' if there is bridge information in a bridging file, grab it and insert it into the model file
            Try
                Dim sDomainName As String = Canonical.CanonicalName(oPackage.Name)
                Dim oBridgingDocument As New XmlDocument
                Dim sBridgingFilename = Path.Combine(_sModelDirectory, "Bridging.xml")
                If File.Exists(sBridgingFilename) Then
                    oBridgingDocument.Load(sBridgingFilename)

                    Dim xBridging As XmlElement = oBridgingDocument.SelectSingleNode("Bridging")
                    If xBridging IsNot Nothing Then               ' looks like this is a Bridging file

                        Dim xBridgingDomain As XmlElement = xBridging.SelectSingleNode("//Domain[@name='" & sDomainName & "']")
                        If xBridgingDomain IsNot Nothing Then       ' this bridging file has some bridging messages for our domain

                            Dim xAcceptedMessages As XmlElement = xBridgingDomain.SelectSingleNode("AcceptedMessages")
                            If xAcceptedMessages IsNot Nothing Then       ' this bridging file has some accepted messages specified

                                Dim oBridgingDocumentFragment As XmlDocumentFragment = oXMLBuilder.XMLDocument.CreateDocumentFragment
                                oBridgingDocumentFragment.InnerXml = xAcceptedMessages.OuterXml
                                Dim xDomain = oXMLBuilder.RootElement.SelectSingleNode("//ModeledDomain[@name='" + sDomainName + "']")
                                Dim xDomainBridging = oXMLBuilder.SetElement("Bridging", xDomain)
                                xDomainBridging.AppendChild(oBridgingDocumentFragment)
                                Dim xIncludeFiles As XmlElement = xBridgingDomain.SelectSingleNode("IncludeFiles")
                                If xIncludeFiles IsNot Nothing Then
                                    Dim oBridgingDocumentFragment1 As XmlDocumentFragment = oXMLBuilder.XMLDocument.CreateDocumentFragment
                                    oBridgingDocumentFragment1.InnerXml = xIncludeFiles.OuterXml
                                    xDomainBridging.AppendChild(oBridgingDocumentFragment1)
                                End If
                            End If
                        End If

                    End If
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub insertPopulationXML(oPackage As EA.Package)                ' if there is a population file, grab it and insert it into the model file
            Dim oErrorHandler As New sjmErrorHandler
            Try
                Dim oPopulationDocument As New XmlDocument
                Dim sPopulationFilename = Path.Combine(_sModelDirectory, oPackage.Name + "_Population.xml")
                If File.Exists(sPopulationFilename) Then
                    oErrorHandler.SupplementalInformation = "\r\nLoading file: \n\r" + sPopulationFilename
                    oPopulationDocument.Load(sPopulationFilename)
                    Dim xPopulation As XmlElement = oPopulationDocument.SelectSingleNode("Population")
                    If xPopulation IsNot Nothing Then               ' looks like this is a population file
                        Dim oPopulationDocumentFragment As XmlDocumentFragment = oXMLBuilder.XMLDocument.CreateDocumentFragment
                        oPopulationDocumentFragment.InnerXml = xPopulation.InnerXml
                        oXMLBuilder.XMLDocument.DocumentElement.AppendChild(oPopulationDocumentFragment)
                    End If
                Else
                    ShowOutputLine("WARNING -- no population file found:   " + sPopulationFilename)
                End If
            Catch ex As Exception
                oErrorHandler.Announce(ex)
            End Try
        End Sub

        Private Sub createInterfaceNodes(ByVal oPackage As EA.Package, ByVal xEnumerations As XmlElement)
            Try
                With oXMLBuilder
                    For Each oInterfaceClass As EA.Element In _Interfaces
                        Dim xInterface As XmlElement = .SetElement("Interface", xEnumerations, "name", Canonical.CanonicalName(oInterfaceClass.Name))
                        Dim xMethods As XmlElement = .SetElement("Methods", xInterface)
                        For Each oMethod As EA.Method In oInterfaceClass.Methods
                            Dim xMethod As XmlElement =
                                .SetElement("Method", xMethods,
                                "name", oMethod.Name,
                                "description", oMethod.Notes,
                                "stereotypes", oMethod.StereotypeEx,
                                "returnType", oMethod.ReturnType)
                            .SetCDATA(oMethod.Behavior, xMethod)
                            For Each oParameter As EA.Parameter In oMethod.Parameters
                                Dim xParameter As XmlElement =
                                    .SetElement("Parameter", xMethod,
                                                 "name", oParameter.Name,
                                                 "dataType", oParameter.Type,
                                                 "description", oParameter.Notes,
                                                 "kind", oParameter.Kind)
                            Next
                        Next
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub createEnumerationNodes(ByVal oPackage As EA.Package, ByVal xEnumerations As XmlElement)
            Try
                With oXMLBuilder
                    For Each oEnumeration As EA.Element In ModelEnumerations.Values
                        Dim xEnumeration = .SetElement("Enumeration", xEnumerations, "name", oEnumeration.Name)
                        For Each oEnumerator As EA.Attribute In oEnumeration.Attributes
                            .SetElement("Enumerator", xEnumeration,
                                        "name", Canonical.CanonicalName(oEnumerator.Name),
                                        "description", Regex.Replace(StripRichTextFormat(oEnumerator.Notes), """", "`"),
                                        "initialValue", oEnumerator.Default)
                        Next
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Function createClassNodes(ByVal oPackage As EA.Package, ByVal xClasses As XmlElement) As Integer
            Dim iClassCount As Integer = 0

            Try
                With oXMLBuilder
                    For Each oElement As EA.Element In _ClassById
                        Try

                            If (oElement.MetaType = "Class") Then
                                If oElement.PackageID = oPackage.PackageID Then
                                    ShowOutputLine("                 " + oElement.Name)
                                    iClassCount += 1
                                    Dim lClassIDHashcode As Long = (oPackage.Name + oElement.Name).GetHashCode()

                                    Dim sDiagramGUID As String = ""
                                    If oElement.Diagrams.Count > 0 Then
                                        Dim oDiagram As EA.Diagram = oElement.Diagrams.GetAt(0)
                                        sDiagramGUID = extractGUID(oDiagram)
                                    End If

                                    Dim xClass = .SetElement("Class", xClasses,
                                                                                           "name", CanonicalClassName(oElement.Name),
                                                                                           "elementId", oElement.ElementID,
                                                                                           "classID", lClassIDHashcode.ToString(),
                                                                                           "diagramGUID", sDiagramGUID)
                                    oXMLBuilder.SetAttribute("description", Regex.Replace(StripRichTextFormat(oElement.Notes), """", "`"), xClass)
                                    Try
                                        setMinimumAllocation(xClass, oElement)
                                        createSupertypes(xClass, oElement)
                                        createAttributes(xClass, oElement)
                                        createStateMachine(xClass, oElement)
                                        addAssociations(xClass, oElement)
                                        addClassOperations(xClass, oElement)
                                        addClassStereotypes(xClass, oElement)
                                        associateAttributeOperations(xClass, oElement)
                                    Catch ex As Exception
                                        Dim oErrorHandlers As New sjmErrorHandler(ex)
                                    End Try
                                End If
                            End If
                        Catch ex As Exception
                            Debug.WriteLine("y")

                        End Try
                    Next
                    resolveSupertypeNames()
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
            Return iClassCount
        End Function

        Private Sub resolveSupertypeNames()
            With oXMLBuilder
                For Each xSupertype As XmlElement In .RootElement.SelectNodes("//Supertype")
                    Dim xSuperTypeReferent As XmlElement = .RootElement.SelectSingleNode("//Class[@elementId='" & xSupertype.GetAttribute("elementId") & "']")
                    If Not xSuperTypeReferent Is Nothing Then
                        .SetAttribute("name", xSuperTypeReferent.GetAttribute("name"), xSupertype)
                    End If
                Next
            End With
        End Sub

        Private Sub setMinimumAllocation(ByVal xClass As XmlElement, ByVal oElement As EA.Element)
            Dim oSmartMatch As New SmartMatch(oElement.StereotypeEx, "([0-9]+)")
            If oSmartMatch.Matches.Count > 0 Then
                Dim iAllocation As Integer = Integer.Parse(oSmartMatch.MatchGroup(0, 0))
                If (iAllocation = 1) Then
                    iAllocation += 1
                End If
                oXMLBuilder.SetAttribute("minimumAllocation", oSmartMatch.MatchGroup(0, 0), xClass)
            Else
                oXMLBuilder.SetAttribute("minimumAllocation", "5", xClass)
            End If
        End Sub

        Private Sub createSupertypes(ByVal xClass As XmlElement, ByVal oElement As EA.Element)
            With oXMLBuilder
                Dim oTokens As String() = Split(oElement.GetRelationSet(EA.EnumRelationSetType.rsParents), ",")
                If oTokens(0).Length > 0 Then
                    .SetElement("Supertype", xClass, "elementId", oTokens(0))
                End If
            End With
        End Sub

        Private Sub createAttributes(ByVal xClass As XmlElement, ByVal oElement As EA.Element)

            With oXMLBuilder
                Dim xAttributes = .SetElement("Attributes", xClass)
                For Each oAttribute As EA.Attribute In oElement.Attributes
                    Dim sVisibility As String = getAttributeVisibilityKeyword(oAttribute)
                    Dim sCanonicalType As String = CanonicalType(oAttribute.Type)
                    Dim sCanonicalAttributeName As String = Canonical.CanonicalName(oAttribute.Name)
                    Dim sDescription As String = Regex.Replace(StripRichTextFormat(oAttribute.Notes), """", "`")

                    Dim sConst As String = oAttribute.IsConst.ToString.ToLower
                    Dim sInitialValue As String = oAttribute.Default
                    Dim lHashCode As Long = (sCanonicalType & _sName & sCanonicalAttributeName & sCanonicalType).GetHashCode()
                    'If (IsUnique(lHashCode.ToString(), IntermediateRepresentationXML.oHashcodes)) Then
                    IntermediateRepresentationXML.lHashcodesSum += lHashCode

                    'If (Not Canonical.IsCanonicalType(sCanonicalType)) Then
                    '    ShowOutputLine("                      Note: attribute '" + oAttribute.Name + "' of type '" + oAttribute.Type + "' cannot be addressed as a TLV")
                    '    lHashCode = 0               ' hashcode of zero means this attribute should not be a TLV
                    'End If

                    Dim sPersist As String = "NeverPersist"                             ' Note: 'NeverPersist' and 'WrittenToFlash' are the actual enumerator names in the 'ePERSISTENCE_STATE' enumeration 
                    If AttributeIncludesStereotype(oAttribute, "persist") Then
                        sPersist = "WrittenToFlash"
                    End If

                    Dim sDefault As String = "0"
                    If oAttribute.Default.Length > 0 Then
                        sDefault = oAttribute.Default
                    End If

                    .SetElement("Attribute", xAttributes,
                                "dataType", sCanonicalType,
                                "name", sCanonicalAttributeName,
                                "description", sDescription,
                                "visibility", sVisibility,
                                "isConst", sConst,
                                "TLVhashcode", lHashCode.ToString(),
                                "initialValue", sInitialValue,
                                "persistent", sPersist,
                                "default", sDefault)
                    'Else
                    'Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
                    'oErrorHandler.AnnounceMessage("Attribute hashcode is not unique (please change the name slightly):" + vbCrLf + vbCrLf + vbTab + "ModeledDomain name: " + vbTab + _sName + vbCrLf + vbTab + "Class name: " + vbTab + oElement.Name + vbCrLf + vbTab + "Attribute name: " + vbTab + oAttribute.Name)
                    'End If
                Next
            End With
        End Sub

        Private Sub identifyInitialStates()
            With oXMLBuilder
                For Each oState As EA.Element In _InitialStates
                    Dim sStateID As String = oState.ElementID
                    Dim bSuccess As Boolean = False

                    For Each oConnector As EA.Connector In oState.Connectors
                        Dim sConnectorClientID As String = oConnector.ClientID.ToString()
                        If sStateID = sConnectorClientID Then
                            Dim sConnectorSupplierID As String = oConnector.SupplierID.ToString()

                            Dim xInitialState As XmlElement = .RootElement.SelectSingleNode("//State [@elementID = '" & sConnectorSupplierID & "']")
                            oXMLBuilder.SetAttribute("isInitialState", "true", xInitialState)

                            Dim xInitialStateIndicatorTransition As XmlElement = .RootElement.SelectSingleNode("//Transition [TargetSideState/@elementID = '" & sConnectorSupplierID & "']")
                            oXMLBuilder.SetAttribute("isInitialStateIndicatorTransition", "true", xInitialStateIndicatorTransition)

                            bSuccess = True
                            Exit For
                        End If
                    Next

                    If Not bSuccess Then
                        Throw New Exception("State '" & oState.Name & "' is a virtual 'meatball' state which does not connect to a real first state")
                    End If
                Next
            End With
        End Sub

        Private Sub addClassStereotypes(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            With oXMLBuilder
                Dim sTokens As String() = Split(oClass.StereotypeEx, ",")
                If (sTokens.Length > 0) And Not (sTokens(0) = "") Then
                    Dim xStereotypes As XmlElement = .SetElement("Stereotypes", xClass)
                    For Each sToken As String In sTokens
                        .SetElement("Stereotype", xStereotypes, "name", sToken.ToUpper)
                    Next
                End If
            End With
        End Sub

        Private Sub addAssociations(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            Dim sSupplierCardinality As String
            Dim sClientCardinality As String
            Dim sCardinalityComment As String = ""
            Dim sOtherClassName As String
            Dim sClientRole As String
            Dim sSupplierRole As String

            With oXMLBuilder
                Try
                    Dim xRelationships = .SetElement("Relationships", xClass)
                    For Each oConnector As EA.Connector In oClass.Connectors
                        If oConnector.Type = "Association" Then
                            If oClass.Name = EAClass(oConnector.SupplierID).Name Then
                                sOtherClassName = EAClass(oConnector.ClientID).Name
                                sSupplierCardinality = oConnector.ClientEnd.Cardinality
                                sClientCardinality = oConnector.SupplierEnd.Cardinality
                                sSupplierRole = oConnector.ClientEnd.Role
                                sClientRole = oConnector.SupplierEnd.Role
                            Else
                                sOtherClassName = EAClass(oConnector.SupplierID).Name
                                sSupplierCardinality = oConnector.SupplierEnd.Cardinality
                                sClientCardinality = oConnector.ClientEnd.Cardinality
                                sSupplierRole = oConnector.SupplierEnd.Role
                                sClientRole = oConnector.ClientEnd.Role
                            End If

                            oConnector.Name = Canonical.CanonicalName(oConnector.Name)

                            If oConnector.Name.Length = 0 Then
                                Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
                                oErrorHandler.AnnounceMessage("The relationship between '" & sOtherClassName & "' and '" & oClass.Name & "' needs a name (e.g., R1)")
                            End If

                            Dim xRelationship As XmlElement = .SetElement("Relationship", xRelationships, "name", oConnector.Name)
                            Dim xThatSide As XmlElement
                            Dim xThisSide As XmlElement
                            Select Case sSupplierCardinality
                                Case "1", "0..1"
                                    xThatSide = .SetElement("ThatSide", xRelationship, "isMany", "false", "role", sSupplierRole)

                                Case "0..*", "1..*"
                                    xThatSide = .SetElement("ThatSide", xRelationship, "isMany", "true", "role", sSupplierRole)

                                Case Else
                                    If sSupplierCardinality.Length > 0 Then
                                        Throw New ApplicationException("Unknown cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
                                    Else
                                        Throw New ApplicationException("No cardinality on relationship (see class '" & oClass.Name & "'): " & sSupplierCardinality)
                                    End If
                            End Select

                            Select Case sClientCardinality
                                Case "1", "0..1"
                                    xThisSide = .SetElement("ThisSide", xRelationship, "isMany", "false", "role", sSupplierRole)

                                Case "0..*", "1..*"
                                    xThisSide = .SetElement("ThisSide", xRelationship, "isMany", "true", "role", sSupplierRole)

                                Case Else
                                    If sClientCardinality.Length > 0 Then
                                        Throw New ApplicationException("Unknown cardinality on relationship (see class '" & oClass.Name & "'): " & sClientCardinality)
                                    Else
                                        Throw New ApplicationException("No cardinality on relationship (see class '" & oClass.Name & "'): " & sClientCardinality)
                                    End If
                            End Select
                            If sOtherClassName = oClass.Name Then           ' tag the special case of a reflexive relationship
                                xRelationship.SetAttribute("isReflexive", "true")
                            End If
                            .SetAttribute("className", sOtherClassName, xThatSide)
                            .SetAttribute("className", oClass.Name, xThisSide)
                        End If
                    Next
                Catch ex As Exception
                    Dim oErrorHandler As New sjmErrorHandler(ex)
                End Try
            End With
        End Sub

        Private Function nextStateNumber() As Integer
            Static iStateNumber As Integer = 100
            iStateNumber += 1
            Return iStateNumber
        End Function

        Private Sub createStateMachine(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            Dim oErrorHandler As New sjmErrorHandler()

            Try
                With oXMLBuilder
                    Dim xStates = .SetElement("States", xClass)
                    Dim xTransitions = .SetElement("Transitions", xClass)
                    Dim xEvents = .SetElement("Events", xClass)
                    Dim sMeatballStateBoolean As String
                    Dim sPigtailStateBoolean As String
                    Dim sIgnoreStateBoolean As String
                    Dim oTransitionEventNames As New Collection
                    Dim oPigtailStates As Collection

                    For Each oStateMachineElement As EA.Element In _StateMachines
                        If oClass.ElementID = oStateMachineElement.ParentID Then
                            Dim iStateMachineID = oStateMachineElement.ElementID

                            Dim _StateIDtoName = New Collection
                            For Each oState As EA.Element In _States
                                _StateIDtoName.Add(oState.Name, oState.ElementID.ToString())
                            Next

                            If oStateMachineElement.Diagrams.Count > 0 Then
                                Dim oStateDiagram As EA.Diagram = oStateMachineElement.Diagrams.GetAt(0)
                                Dim sStateDiagramGUID As String = extractGUID(oStateDiagram)
                                .SetAttribute("stateDiagramGUID", sStateDiagramGUID, xClass)
                            End If

                            _StateNames = New Collection

                            _StateIgnoreStatus = New Collection
                            For Each oState As EA.Element In _States
                                If oState.ParentID = iStateMachineID Then
                                    If oState.Subtype = EA_TYPE.IGNORE_STATE Then         ' if this state is an 'ignore' state
                                        _StateIgnoreStatus.Add("true", oState.ElementID.ToString)
                                    Else
                                        _StateIgnoreStatus.Add("false", oState.ElementID.ToString)
                                    End If
                                End If
                            Next

                            oPigtailStates = New Collection         ' every state has a separate set of pigtails
                            For Each oState As EA.Element In _States
                                If oState.Subtype = EA_TYPE.ENTRY_STATE Then                    ' if this state is a pigtail (entry from any state) state
                                    IsUnique(oState, oPigtailStates, oState.ElementID.ToString)         ' add this state ID to the pigtail bucket (if it isn't already there)
                                End If
                            Next

                            For Each oState As EA.Element In _States
                                If oState.ParentID = iStateMachineID Then
                                    If oState.Subtype = EA_TYPE.INITIAL_STATE Then      ' if this state is the "meatball" initial state
                                        sMeatballStateBoolean = "true"
                                        Debug.WriteLine("meatball: " & oState.Name & ", " & oState.ElementGUID)
                                    Else
                                        sMeatballStateBoolean = "false"
                                    End If

                                    If oState.Subtype = EA_TYPE.IGNORE_STATE Then         ' if this state is an 'ignore' state
                                        sIgnoreStateBoolean = "true"
                                    Else
                                        sIgnoreStateBoolean = "false"
                                    End If

                                    If oState.Subtype = EA_TYPE.ENTRY_STATE Then                    ' if this state is a pigtail (entry from any state) state
                                        sPigtailStateBoolean = "true"
                                    Else
                                        sPigtailStateBoolean = "false"
                                    End If

                                    Dim sStateNumber As String = Regex.Match(oState.Name, "[^_]+").ToString()            ' grab everything up to the first underscore 
                                    Dim iElementId = oState.ElementID
                                    Dim xState As XmlElement = .SetElement("State", xStates,
                                                                           "stateNumber", sStateNumber,
                                                                           "name", oState.Name,
                                                                           "elementID", iElementId,
                                                                           "isMeatballState", sMeatballStateBoolean,
                                                                           "isIgnoreState", sIgnoreStateBoolean,
                                                                           "isPigtailState", sPigtailStateBoolean)
                                    .SetText(StripRichTextFormat(oState.Notes), xState)

                                    For Each oConnector As EA.Connector In oState.Connectors
                                        Dim sConnectorSupplierID As String = oConnector.SupplierID.ToString()
                                        Dim xExistingConnector = xTransitions.SelectSingleNode("Transition [@elementID = " + oConnector.ConnectorID.ToString() + "]")
                                        If xExistingConnector Is Nothing Then
                                            Dim sIsPigtailTransition As String = "false"
                                            If oPigtailStates.Contains(oConnector.ClientID.ToString) Then
                                                sIsPigtailTransition = "true"
                                            End If

                                            Try
                                                Dim sTokens As String() = Split(oConnector.TransitionEvent, "(")
                                                Dim sEventElementID As String = sTokens(0)

                                                Dim xTransition = .SetElement("Transition", xTransitions,
                                                                              "elementID", oConnector.ConnectorID.ToString(),
                                                                              "eventElementID", sEventElementID,
                                                                              "isPigtailTransition", sIsPigtailTransition)

                                                Dim xSourceSideState = .SetElement("SourceSideState", xTransition,
                                                                                   "elementID", oConnector.ClientID.ToString,
                                                                                   "name", _StateIDtoName(oConnector.ClientID.ToString))

                                                Dim xTargetSideState = .SetElement("TargetSideState", xTransition,
                                                                                   "elementID", oConnector.SupplierID.ToString,
                                                                                   "name", _StateIDtoName(oConnector.SupplierID.ToString),
                                                                                   "isIgnoreState", _StateIgnoreStatus(oConnector.SupplierID.ToString))
                                                addEvent(oConnector, xEvents)
                                            Catch ex As Exception
                                                ShowOutputLine("Exception occurred (but we ignore it as not important?): " & ex.Message)
                                            Finally
                                            End Try
                                        End If
                                    Next
                                End If
                            Next
                        End If
                    Next
                End With
            Catch ex As Exception
                oErrorHandler.Announce(ex)
            End Try
        End Sub

        Function verifyLegalConnector(sConnectorSupplierId As String)
            Dim bLegal As Boolean = _StateNames.Contains(sConnectorSupplierId)      ' some kinds of connectors (e.g., note links) should be ignored because they are not attached to states
            Return bLegal
        End Function

        Private Sub addEvent(ByVal oConnector As EA.Connector, ByVal xEvents As XmlElement)
            Dim xEvent As XmlElement
            Try
                With oXMLBuilder
                    If Not _Triggers.Contains(canonicalEventName(oConnector.TransitionEvent)) Then
                        If oConnector.TransitionEvent.Length > 0 Then
                            MessageBox.Show("Event '" + oConnector.TransitionEvent + "' not found" + vbCrLf + "(does it only appear in a 'specification' field?)", "Unkown Event", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        End If
                    Else
                        Dim sEvent = Regex.Replace(oConnector.TransitionEvent, "\s*\(", "(")
                        If (sEvent.Length > 0) And (IsUnique(sEvent, _UniqueEventNames)) Then
                            Dim oMatches As MatchCollection = Regex.Matches(sEvent, "([^\(]+)\(([^()]+)\)")
                            If oMatches.Count = 0 Then
                                xEvent = .SetElement("Event", xEvents,
                                                     "name", canonicalEventName(sEvent),
                                                     "transitionEventString", oConnector.TransitionEvent,
                                                     "eventElementID", sEvent)
                            Else
                                Dim oGroups As GroupCollection = oMatches(0).Groups
                                Dim sEventName As String = Regex.Replace(Trim(oGroups(1).ToString()), "\s+", " ")
                                xEvent = .SetElement("Event", xEvents,
                                                     "name", canonicalEventName(sEventName),
                                                     "transitionEventString", oConnector.TransitionEvent,
                                                     "eventElementID", sEventName)
                                If oGroups.Count > 2 Then
                                    Dim sArgumentList = Regex.Replace(Trim(oGroups(2).ToString()), "[ ,]+", " ")
                                    Dim sArgumentTokens() = Regex.Split(sArgumentList, " ")
                                    If (sArgumentTokens.Length And 1) = 0 Then   ' there had better be one or more pairs (type plus parameterName)
                                        Dim i As Integer
                                        For i = 0 To sArgumentTokens.Length - 1 Step 2
                                            .SetElement("Parameter", xEvent, "dataType", sArgumentTokens(i), "name", sArgumentTokens(i + 1))
                                        Next
                                    Else
                                        Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
                                        oErrorHandler.AnnounceMessage("Every event parameter must have a type: " + vbCrLf + "   " + oConnector.TransitionEvent)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Function getAttributeVisibilityKeyword(ByVal oAttribute As EA.Attribute) As String
            'Dim sVisibilityKeyword As String

            'Select Case oAttribute.Visibility
            '    Case "Protected"
            '        sVisibilityKeyword = "Protected"

            '    Case "Private"
            '        sVisibilityKeyword = "Private"

            '    Case "Public"
            '        sVisibilityKeyword = "Public"

            '    Case Else
            '        Throw New ApplicationException("Unhandled visiblity case: " & oAttribute.Visibility)
            'End Select
            Return oAttribute.Visibility
        End Function

        Public Shared Function CanonicalType(ByVal sType As String) As String
            Dim sReturnTypeString As String = sType

            If sType.Length > 0 Then
                Select Case sType.ToLower
                    Case "boolean", "bool"
                        sReturnTypeString = "bool"

                    Case "void"
                        sReturnTypeString = "void"

                    Case "unsigned long"
                        sReturnTypeString = "ulong"

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

            Return Canonical.CanonicalName(sReturnTypeString)
        End Function

        Private Sub catalogElement(ByVal oElement As EA.Element)
            Const ALLOW_DUPLICATE_NAME As Boolean = True

            Dim oErrorHandler As New sjmErrorHandler()
            oErrorHandler.SupplementalInformation = "Element name '" & oElement.Name & "' is not unique in this model"

            Try
                Application.DoEvents()

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
                        catalogState(oElement, ALLOW_DUPLICATE_NAME)

                    Case "Pseudostate"
                        Select Case oElement.Subtype
                            Case EA_TYPE.INITIAL_STATE
                                _InitialStates.Add(oElement, oElement.ElementID)
                                catalogState(oElement, ALLOW_DUPLICATE_NAME)

                            Case EA_TYPE.IGNORE_STATE
                                _IgnoreIndicatorStates.Add(oElement, oElement.ElementID)
                                catalogState(oElement, ALLOW_DUPLICATE_NAME)

                            Case EA_TYPE.ENTRY_STATE
                                catalogState(oElement, ALLOW_DUPLICATE_NAME)

                            Case Else
                                Debug.WriteLine(oElement.Name & " is an unhandled pseudostate subtype " & oElement.Subtype)
                        End Select

                    Case "Trigger"
                        _Triggers.Add(oElement, oElement.Name)

                    Case "StateNode"
                        catalogState(oElement, Not ALLOW_DUPLICATE_NAME)

                    Case "Enumeration"
                        ModelEnumerations.Add(oElement.Name, oElement)

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
                        oElement.Name = Canonical.CanonicalName(oElement.Name)
                        catalogState(oElement, Not ALLOW_DUPLICATE_NAME)

                    Case "Note", "Text"
                        ' do nothing with these, just allow them without complaint

                    Case Else
                        Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
                End Select

                For Each oSubElement As EA.Element In oElement.Elements
                    catalogElement(oSubElement)                      ' recurse down the element tree
                Next
            Catch ex As Exception
                oErrorHandler.Announce(ex)
            End Try
        End Sub

        Private Sub catalogState(oStateElement As EA.Element, bAllowDuplicate As Boolean)
            Try
                If _UniqueStateNames.Contains(oStateElement.Name) Then
                    If (Not bAllowDuplicate) Then
                        Dim oErrorHandler As New sjmErrorHandler()
                        oErrorHandler.AnnounceMessage("'" & oStateElement.Name & "' is not a unique state name in this model")
                    End If
                Else
                    _UniqueStateNames.Add(oStateElement.Name, oStateElement.Name)
                End If

                If Not _States.Contains(oStateElement.ElementID) Then
                    _States.Add(oStateElement, oStateElement.ElementID)
                End If
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub associateAttributeOperations(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            Try
                With oXMLBuilder
                    For Each oMethod As EA.Method In oClass.Methods
                        Dim xSameNameAttribute As XmlElement = xClass.SelectSingleNode("//Attribute [@name = '" + oMethod.Name + "']")
                        If xSameNameAttribute IsNot Nothing Then            ' there is a same-named attribute in this class
                            xSameNameAttribute.SetAttribute("hasUpdateOperation", "true")

                            Dim xSameNameOperation As XmlElement = xClass.SelectSingleNode("//Operation [@name = '" + oMethod.Name + "']")
                            If xSameNameOperation IsNot Nothing Then
                                xSameNameOperation.SetAttribute("updatesAttributeName", oMethod.Name)
                            End If
                        End If
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub

        Private Sub addClassOperations(ByVal xClass As XmlElement, ByVal oClass As EA.Element)
            Dim oMethod As EA.Method
            Dim oParameter As EA.Parameter
            Dim sLeadingComma As String = ""
            Dim sBehavior As String = ""

            Try
                With oXMLBuilder

                    Dim xMethods As XmlElement = .SetElement("Operations", xClass)
                    For Each oMethod In oClass.Methods
                        Dim sStereotypeEvent As String = "false"
                        If MethodIncludesStereotype(oMethod, "event") Then
                            sStereotypeEvent = "true"
                        End If

                        If oMethod.ReturnType.Trim().Length = 0 Then
                            oMethod.ReturnType = "void"
                        Else
                            oMethod.ReturnType = CanonicalType(oMethod.ReturnType)
                        End If

                        oMethod.Name = Canonical.CanonicalName(oMethod.Name)

                        Dim xMethod As XmlElement = .SetElement("Operation", xMethods, "name", oMethod.Name, "returnType", oMethod.ReturnType, "isEvent", sStereotypeEvent)
                        sBehavior = StripRichTextFormat(oMethod.Behavior)
                        .SetCDATA(sBehavior, xMethod)

                        For Each oParameter In oMethod.Parameters
                            oParameter.Name = Canonical.CanonicalName(oParameter.Name)
                            oParameter.Type = CanonicalType(oParameter.Type)
                            .SetElement("Parameter", xMethod,
                                        "name", oParameter.Name,
                                        "dataType", oParameter.Type)
                        Next
                    Next
                End With
            Catch ex As Exception
                Dim oErrorHandler As New sjmErrorHandler(ex)
            End Try
        End Sub


    End Class
End Class

