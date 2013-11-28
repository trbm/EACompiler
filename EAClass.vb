
' '' ''____________________________________________________________________
' '' ''
' '' '' © Copyright 2008, PYCCA, Incorporated  All rights reserved.
' '' ''____________________________________________________________________
' '' ''
' '' ''     $RCSfile: EAClass.vb,v $
' '' ''
' '' ''    $Revision: 1.1 $
' '' ''
' '' ''        $Date: 2008/09/24 21:09:08 $
' '' ''
' '' ''      $Author: thomasb $
' '' ''
' '' ''      $Source: /home/cvsroot/Fred/DVT\040Test/DVT\040TESTLA™/•\040Phase\040II\040•/EAModelCompiler/EAClass.vb,v $
' '' ''
' '' ''        $Name:  $
' '' ''____________________________________________________________________

'' ''Imports system.Text.RegularExpressions

'' ''Public Class EAClass
'' ''    Private _oSourceOutput As RichTextBox
'' ''    Private _oClassElement As EA.Element
'' ''    Private _oDomain As Domain
'' ''    Private _bFinalStateReported As Boolean = False

'' ''    Const COMMENT_START_COLUMN = 40

'' ''    Public Sub New(ByVal oClassElement As EA.Element, ByRef oDomain As Domain, ByVal oSourceOutput As RichTextBox)
'' ''        MyBase.New()

'' ''        Try
'' ''            _oDomain = oDomain
'' ''            _oSourceOutput = oSourceOutput
'' ''            _oClassElement = oClassElement

'' ''            With _oSourceOutput
'' ''                .AppendText(vbCrLf)
'' ''                .AppendText("    # ____________________________________________________________________" & vbCrLf)
'' ''                .AppendText(vbCrLf)
'' ''                .AppendText("    class " & oClassElement.Name & vbCrLf)

'' ''                addAttributes()
'' ''                addRelationships()
'' ''                addSubtypes()
'' ''                addStateMachine()
'' ''                addConstructor()
'' ''                ' addDestructor()
'' ''                .AppendText("    end  # class " & vbCrLf)
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addStateMachine()
'' ''        Dim oState As EA.Element
'' ''        Dim oConnector As EA.Connector
'' ''        Dim oClientConnectorEnd As EA.ConnectorEnd
'' ''        Dim oSupplierConnectorEnd As EA.ConnectorEnd
'' ''        Dim sArgumentList As String
'' ''        Dim bStateMachineHeaderAdded As Boolean = False

'' ''        Try
'' ''            If _oDomain.States.Count > 0 Then
'' ''                With _oSourceOutput
'' ''                    For Each oState In _oDomain.States
'' ''                        sArgumentList = ""
'' ''                        If oState.ParentID = _oClassElement.ElementID Then
'' ''                            If Not bStateMachineHeaderAdded Then
'' ''                                bStateMachineHeaderAdded = True
'' ''                                .AppendText(vbCrLf)
'' ''                                .AppendText("        machine" & vbCrLf)
'' ''                            End If

'' ''                            For Each oConnector In oState.Connectors
'' ''                                oClientConnectorEnd = oConnector.ClientEnd
'' ''                                oSupplierConnectorEnd = oConnector.SupplierEnd
'' ''                                addNormalTransition(oConnector, oState, sArgumentList)
'' ''                            Next
'' ''                            addState(oState, sArgumentList)
'' ''                        End If
'' ''                    Next
'' ''                    If bStateMachineHeaderAdded Then
'' ''                        .AppendText("        end # machine" & vbCrLf)
'' ''                    End If
'' ''                End With
'' ''            End If
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addNormalTransition(ByVal oConnector As EA.Connector, ByVal oState As EA.Element, ByRef sArgumentList As String)
'' ''        Dim oClientState As EA.Element
'' ''        Dim sEvent As String
'' ''        Dim sTokens() As String
'' ''        Dim oSupplierState As EA.Element = Nothing
'' ''        Dim sToStateName As String

'' ''        Try
'' ''            With _oSourceOutput
'' ''                If _oDomain.States.Contains(oConnector.SupplierID.ToString) Then                ' if "to" state is a normal state
'' ''                    oSupplierState = _oDomain.States(oConnector.SupplierID.ToString)

'' ''                    If oSupplierState Is oState Then
'' ''                        If _oDomain.States.Contains(oConnector.ClientID.ToString) Then          ' if "from" state is a normal state
'' ''                            oClientState = _oDomain.States(oConnector.ClientID.ToString)
'' ''                            sEvent = oConnector.TransitionEvent
'' ''                            sTokens = Split(sEvent, ":")
'' ''                            If sTokens.Length > 1 Then              ' peel off any "ev1:" style prefix
'' ''                                sEvent = sTokens(1)
'' ''                            End If

'' ''                            If sEvent.Length > 0 Then
'' ''                                If sEvent.IndexOf(")") > 0 Then
'' ''                                    sTokens = Split(sEvent.Substring(0, sEvent.IndexOf(")")), "(")            ' peel off any parameter payload
'' ''                                    If sTokens.Length > 1 Then
'' ''                                        sEvent = sTokens(0)
'' ''                                        sArgumentList = sTokens(1)
'' ''                                    End If
'' ''                                End If
'' ''                                If _oDomain.IgnoreIndicatorStates.Contains(oState.ElementID) Then
'' ''                                    sToStateName = "IG"
'' ''                                Else
'' ''                                    sToStateName = oState.Name
'' ''                                End If
'' ''                                .AppendText("            transition " & oClientState.Name & " - " & sEvent & " -> " & sToStateName & vbCrLf)
'' ''                            End If
'' ''                        Else
'' ''                            .AppendText("            initial state " & oState.Name & vbCrLf)
'' ''                        End If
'' ''                    End If
'' ''                End If
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Function canonicalTypeNames(ByVal sArgumentList As String) As String
'' ''        Dim sCanonicalArgumentList As String

'' ''        sCanonicalArgumentList = Trim(sArgumentList)
'' ''        sCanonicalArgumentList = Regex.Replace(sCanonicalArgumentList, "boolean ", "bool ", RegexOptions.IgnoreCase)
'' ''        sCanonicalArgumentList = Regex.Replace(sCanonicalArgumentList, "string ", "char* ", RegexOptions.IgnoreCase)
'' ''        Return sCanonicalArgumentList
'' ''    End Function

'' ''    Private Sub addState(ByVal oState As EA.Element, ByVal sArgumentList As String)
'' ''        Dim sLines() As String
'' ''        Dim sLine As String
'' ''        Dim sCanonicalArgumentList As String

'' ''        Try
'' ''            With _oSourceOutput
'' ''                If Not _oDomain.IgnoreIndicatorStates.Contains(oState.ElementID) Then
'' ''                    sCanonicalArgumentList = canonicalTypeNames(sArgumentList)
'' ''                    sLines = Split(oState.Notes, vbCrLf)
'' ''                    .AppendText("            state " & oState.Name & "(")
'' ''                    .AppendText(sCanonicalArgumentList)
'' ''                    .AppendText(")" & vbCrLf)
'' ''                    .AppendText("                {" & vbCrLf)
'' ''                    For Each sLine In sLines
'' ''                        .AppendText("                    " & sLine & vbCrLf)
'' ''                    Next
'' ''                    .AppendText("                }" & vbCrLf & vbCrLf)
'' ''                End If
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addSubtypes()
'' ''        Dim sTokens() As String
'' ''        Dim sSubtypeId As String
'' ''        Dim sSubtypeName As String
'' ''        Static iGeneralizationRelationshipNumber As Integer = 0

'' ''        Try
'' ''            With _oSourceOutput
'' ''                sTokens = Split(_oClassElement.GetRelationSet(EA.EnumRelationSetType.rsGeneralizeEnd), ",")
'' ''                If sTokens.Length > 1 Then
'' ''                    .AppendText(vbCrLf & "        subtype R" & iGeneralizationRelationshipNumber.ToString & " reference" & vbCrLf)
'' ''                    For Each sSubtypeId In sTokens
'' ''                        sSubtypeName = _oDomain.EAClass(sSubtypeId).Name
'' ''                        iGeneralizationRelationshipNumber += 1
'' ''                        .AppendText(("            " & sSubtypeName).PadRight(COMMENT_START_COLUMN) & "# id: " & sSubtypeId & vbCrLf)
'' ''                    Next
'' ''                    .AppendText("        end" & vbCrLf)
'' ''                End If
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try

'' ''    End Sub

'' ''    Private Sub addRelationships()
'' ''        Dim oConnector As EA.Connector
'' ''        Dim sOtherClassName As String
'' ''        Dim sSupplierCardinality As String

'' ''        Try
'' ''            With _oSourceOutput
'' ''                For Each oConnector In _oClassElement.Connectors
'' ''                    If oConnector.Type = "Association" Then
'' ''                        If _oClassElement.Name = _oDomain.EAClass(oConnector.SupplierID).Name Then
'' ''                            sOtherClassName = _oDomain.EAClass(oConnector.ClientID).Name
'' ''                            sSupplierCardinality = oConnector.ClientEnd.Cardinality
'' ''                        Else
'' ''                            sOtherClassName = _oDomain.EAClass(oConnector.SupplierID).Name
'' ''                            sSupplierCardinality = oConnector.SupplierEnd.Cardinality
'' ''                        End If

'' ''                        Select Case sSupplierCardinality
'' ''                            Case "1", "0..1"
'' ''                                .AppendText(("        reference " & oConnector.Name & " -> " & sOtherClassName & " ").PadRight(COMMENT_START_COLUMN) & "# " & buildRelationshipPhrase(oConnector) & vbCrLf)

'' ''                            Case ""
'' ''                                .AppendText(("        reference " & oConnector.Name & " -> " & sOtherClassName & " ").PadRight(COMMENT_START_COLUMN) & "# CARDINALITY ASSUMED: " & buildRelationshipPhrase(oConnector) & vbCrLf)

'' ''                            Case "0..*", "1..*"
'' ''                                .AppendText(("        reference " & oConnector.Name & " ->> " & sOtherClassName & " ").PadRight(COMMENT_START_COLUMN) & "# " & buildRelationshipPhrase(oConnector) & vbCrLf)

'' ''                            Case Else
'' ''                                .AppendText("        <unknown cardinality> " & sSupplierCardinality & vbCrLf)
'' ''                        End Select
'' ''                    End If
'' ''                Next
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addAttributes()
'' ''        Dim oAttribute As EA.Attribute
'' ''        Dim sCommentText As String
'' ''        Dim sDefaultValue As String
'' ''        Dim sCanonicalAttributeString As String

'' ''        Try
'' ''            With _oSourceOutput
'' ''                For Each oAttribute In _oClassElement.Attributes
'' ''                    If oAttribute.Notes.Length > 0 Then
'' ''                        sCommentText = "      # " & oAttribute.Notes
'' ''                    Else
'' ''                        sCommentText = ""
'' ''                    End If

'' ''                    If oAttribute.Default.Length > 0 Then
'' ''                        sDefaultValue = "  default {" & oAttribute.Default & "}"
'' ''                    Else
'' ''                        sDefaultValue = ""
'' ''                    End If
'' ''                    sCanonicalAttributeString = canonicalTypeNames(oAttribute.Type & " " & oAttribute.Name)
'' ''                    .AppendText("        attribute  (" & sCanonicalAttributeString & ")" & sDefaultValue & sCommentText & vbCrLf)
'' ''                Next
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addDestructor()
'' ''        Dim oAttribute As EA.Attribute

'' ''        Try
'' ''            With _oSourceOutput
'' ''                .AppendText("" & vbCrLf)
'' ''                .AppendText("        destructor" & vbCrLf)
'' ''                .AppendText("            {" & vbCrLf)
'' ''                For Each oAttribute In _oClassElement.Attributes
'' ''                    .AppendText("                self->" & oAttribute.Name & " = 0;" & vbCrLf)
'' ''                Next
'' ''                .AppendText("            }" & vbCrLf)
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Sub addConstructor()
'' ''        Dim oAttribute As EA.Attribute

'' ''        Try
'' ''            With _oSourceOutput
'' ''                .AppendText("" & vbCrLf)
'' ''                .AppendText("        constructor" & vbCrLf)
'' ''                .AppendText("            {" & vbCrLf)
'' ''                For Each oAttribute In _oClassElement.Attributes
'' ''                    If oAttribute.Default.Length = 0 Then
'' ''                        Select Case oAttribute.Type.ToUpper
'' ''                            Case "CHAR", "STRING"
'' ''                                .AppendText("                self->" & oAttribute.Name & " = ""uninitialized string"";" & vbCrLf)

'' ''                            Case "BOOLEAN"
'' ''                                .AppendText("                self->" & oAttribute.Name & " = false;" & vbCrLf)

'' ''                            Case Else
'' ''                                .AppendText("                self->" & oAttribute.Name & " = 0;" & vbCrLf)

'' ''                        End Select
'' ''                    End If
'' ''                Next
'' ''                .AppendText("            }" & vbCrLf)
'' ''            End With
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try
'' ''    End Sub

'' ''    Private Function buildRelationshipPhrase(ByVal oConnector As EA.Connector) As String
'' ''        Dim iClientClassId As Integer
'' ''        Dim iSupplierClassId As Integer
'' ''        Dim sPhrase As String = ""
'' ''        Dim oClientClass As EA.Element
'' ''        Dim oSupplierClass As EA.Element
'' ''        Dim sClientRole As String
'' ''        Dim sSupplierRole As String
'' ''        Dim sSupplierCardinality As String
'' ''        Dim sSupplierClassName As String
'' ''        Dim sClientClassName As String

'' ''        Try
'' ''            With oConnector
'' ''                iClientClassId = .ClientID
'' ''                iSupplierClassId = .SupplierID
'' ''                sClientRole = .ClientEnd.Role
'' ''                sSupplierRole = .SupplierEnd.Role
'' ''                sSupplierCardinality = .SupplierEnd.Cardinality

'' ''                With _oDomain
'' ''                    oClientClass = .EAClass(iClientClassId)
'' ''                    sClientClassName = oClientClass.Name
'' ''                    oSupplierClass = .EAClass(iSupplierClassId)
'' ''                    sSupplierClassName = oSupplierClass.Name
'' ''                End With

'' ''                If sClientClassName = _oClassElement.Name Then
'' ''                    ' do nothing, perspective is already proper
'' ''                Else
'' ''                    If sSupplierClassName = _oClassElement.Name Then
'' ''                        iClientClassId = .SupplierID
'' ''                        iSupplierClassId = .ClientID
'' ''                        sClientRole = .SupplierEnd.Role
'' ''                        sSupplierRole = .ClientEnd.Role
'' ''                        sSupplierCardinality = .ClientEnd.Cardinality

'' ''                        With _oDomain
'' ''                            oClientClass = .EAClass(iClientClassId)
'' ''                            sClientClassName = oClientClass.Name
'' ''                            oSupplierClass = .EAClass(iSupplierClassId)
'' ''                            sSupplierClassName = oSupplierClass.Name
'' ''                        End With
'' ''                    Else
'' ''                        Throw New ApplicationException("PerspectiveClassName '" & _oClassElement.Name & "'does not match either participant in relationship")
'' ''                    End If
'' ''                End If
'' ''            End With

'' ''            Select Case oConnector.Type
'' ''                Case "Generalization"
'' ''                    sPhrase = oClientClass.Name & " is a " & oSupplierClass.Name
'' ''                Case "Association"
'' ''                    sPhrase = ""
'' ''                    Select Case sSupplierCardinality
'' ''                        Case "1"
'' ''                            sPhrase += oClientClass.Name & " '" & sClientRole & "' exactly one " & oSupplierClass.Name

'' ''                        Case "0..1"
'' ''                            sPhrase += oClientClass.Name & " '" & sClientRole & "' zero or one " & oSupplierClass.Name

'' ''                        Case "0..*"
'' ''                            sPhrase += oClientClass.Name & " '" & sClientRole & "' zero or more " & oSupplierClass.Name

'' ''                        Case "1..*"
'' ''                            sPhrase += oClientClass.Name & " '" & sClientRole & "' one or more " & oSupplierClass.Name & "s"

'' ''                        Case Else
'' ''                            sPhrase += "<unknown cardinality on '" & oClientClass.Name & "' side of relationship '" & oConnector.Name & "'"
'' ''                    End Select
'' ''                Case Else
'' ''                    sPhrase = "<unknown connector type: " & oConnector.Type
'' ''            End Select
'' ''        Catch ex As Exception
'' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
'' ''        End Try

'' ''        Return sPhrase
'' ''    End Function


'' ''End Class
