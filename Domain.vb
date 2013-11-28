
'' ''____________________________________________________________________
'' ''
'' '' © Copyright 2008, PYCCA, Incorporated  All rights reserved.
'' ''____________________________________________________________________
'' ''
'' ''     $RCSfile: Domain.vb,v $
'' ''
'' ''    $Revision: 1.1 $
'' ''
'' ''        $Date: 2008/09/24 21:09:08 $
'' ''
'' ''      $Author: thomasb $
'' ''
'' ''      $Source: Domain.vb,v $
'' ''
'' ''        $Name:  $
'' ''____________________________________________________________________

' ''Public Class Domain
' ''    Private _oSourceOutput As RichTextBox
' ''    Private _oRepository As EA.Repository
' ''    Private _sPackageId As String
' ''    Private _oProject As EA.Project
' ''    Private _oPackage As EA.Package
' ''    Private _ClassById As Collection
' ''    Private _Triggers As Collection
' ''    Private _States As Collection
' ''    Private _InitialStates As Collection
' ''    Private _FinalStates As Collection
' ''    Private _IgnoreIndicatorStates As Collection
' ''    Private _IsRealized As Boolean

' ''    Public ReadOnly Property IgnoreIndicatorStates() As Collection
' ''        Get
' ''            Return _IgnoreIndicatorStates
' ''        End Get
' ''    End Property

' ''    Public ReadOnly Property InitialStates() As Collection
' ''        Get
' ''            Return _InitialStates
' ''        End Get
' ''    End Property

' ''    Public ReadOnly Property FinalStates() As Collection
' ''        Get
' ''            Return _FinalStates
' ''        End Get
' ''    End Property

' ''    Public ReadOnly Property States() As Collection
' ''        Get
' ''            Return _States
' ''        End Get
' ''    End Property

' ''    Public ReadOnly Property EAClass(ByVal iID As Integer) As EA.Element
' ''        Get
' ''            Dim oClass As EA.Element = Nothing

' ''            If _ClassById.Contains(iID.ToString) Then
' ''                oClass = _ClassById.Item(iID.ToString)
' ''            Else
' ''                MsgBox("Unknown class id: " & iID, MsgBoxStyle.Critical)
' ''            End If
' ''            Return oClass
' ''        End Get
' ''    End Property

' ''    Public ReadOnly Property IsRealized() As Boolean
' ''        Get
' ''            Return _IsRealized
' ''        End Get
' ''    End Property

' ''    Public Sub New(ByRef oPackage As EA.Package, ByRef oRepository As EA.Repository, ByRef oSourceOutput As RichTextBox)
' ''        Try
' ''            _oSourceOutput = oSourceOutput
' ''            _oRepository = oRepository
' ''            _oPackage = oPackage
' ''            _sPackageId = _oPackage.PackageID

' ''            _ClassById = New Collection
' ''            _Triggers = New Collection
' ''            _States = New Collection
' ''            _InitialStates = New Collection
' ''            _FinalStates = New Collection
' ''            _IgnoreIndicatorStates = New Collection

' ''            _IsRealized = (_oPackage.StereotypeEx.IndexOf("realized") > -1)

' ''            If Not _IsRealized Then
' ''                catalogElements()
' ''                generateSource()
' ''            End If
' ''        Catch ex As Exception
' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
' ''        End Try
' ''    End Sub

' ''    Private Sub catalogElements()
' ''        Dim oElement As EA.Element

' ''        Try
' ''            For Each oElement In _oPackage.Elements
' ''                Select Case oElement.MetaType
' ''                    Case "FinalState"
' ''                        _FinalStates.Add(oElement, oElement.ElementID)
' ''                        _States.Add(oElement, oElement.ElementID)  

' ''                    Case "Pseudostate"
' ''                        Select Case oElement.Name
' ''                            Case "Initial"
' ''                                _InitialStates.Add(oElement, oElement.ElementID)

' ''                            Case Else
' ''                                _IgnoreIndicatorStates.Add(oElement, oElement.ElementID)
' ''                                _States.Add(oElement, oElement.ElementID)
' ''                        End Select

' ''                    Case "Trigger"
' ''                        Debug.WriteLine("Trigger: " & oElement.Name & " has parent: " & oElement.ParentID)
' ''                        _Triggers.Add(oElement, oElement.Name)

' ''                    Case "StateNode"
' ''                        _States.Add(oElement, oElement.ElementID)

' ''                    Case "Class"
' ''                        _ClassById.Add(oElement, oElement.ElementID)

' ''                    Case "State"
' ''                        _States.Add(oElement, oElement.ElementID)

' ''                    Case Else
' ''                        Debug.WriteLine(oElement.Name & " is an unhandled metatype " & oElement.MetaType)
' ''                End Select
' ''            Next
' ''        Catch ex As Exception
' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
' ''        End Try
' ''    End Sub

' ''    Private Sub generateSource()
' ''        Dim oClassElement As EA.Element
' ''        Dim oEAClass As EAClass

' ''        Try
' ''            With _oSourceOutput
' ''                .AppendText("")
' ''                .AppendText("domain " & _oPackage.Name)
' ''                If _oPackage.Notes.Length > 0 Then
' ''                    .AppendText("   // " & _oPackage.Notes)
' ''                End If
' ''                .AppendText(vbCrLf)
' ''                For Each oClassElement In _ClassById
' ''                    If _sPackageId = oClassElement.PackageID Then
' ''                        oEAClass = New EAClass(oClassElement, Me, _oSourceOutput)
' ''                    End If
' ''                Next
' ''                addProlog()
' ''                addEpilog()
' ''                .AppendText("end")
' ''            End With
' ''        Catch ex As Exception
' ''            Dim oErrorHandler As New sjmErrorHandler(ex)
' ''        End Try
' ''    End Sub

' ''    Private Sub addProlog()
' ''        With _oSourceOutput
' ''            .AppendText(vbCrLf)
' ''            .AppendText("    implementation prolog" & vbCrLf)
' ''            .AppendText("    {" & vbCrLf)
' ''            .AppendText("    #include <stdio.h>" & vbCrLf)
' ''            .AppendText("    }" & vbCrLf)
' ''        End With
' ''    End Sub

' ''    Private Sub addEpilog()
' ''        With _oSourceOutput
' ''            .AppendText(vbCrLf)
' ''            .AppendText("    implementation epilog " & vbCrLf)
' ''            .AppendText("    {" & vbCrLf)
' ''            .AppendText("        Int()" & vbCrLf)
' ''            .AppendText("        main()" & vbCrLf)
' ''            .AppendText("            {" & vbCrLf)
' ''            .AppendText("                return 0 ;" & vbCrLf)
' ''            .AppendText("            }" & vbCrLf)
' ''            .AppendText("    }" & vbCrLf)
' ''        End With
' ''    End Sub

' ''End Class
