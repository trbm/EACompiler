Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.IO


Public Class XMLBuilder
    Private Const VERSION = "2.2"
    Private _xDocument As XmlDocument
    Private _sDocumentFilename As String
    Private _xRoot As XmlElement
    Private _sRootName As String
    Private _Names = New List(Of String)
    Private _Values As New List(Of String)

    Public Sub New(ByVal sRootName As String, ByVal sDocumentFilename As String, Optional ByVal bIncludeCopyright As Boolean = False, Optional sXSLfilename As String = "")
        _xDocument = New XmlDocument
        _sDocumentFilename = sDocumentFilename
        _sRootName = safeName(sRootName)

        _xDocument.AppendChild(_xDocument.CreateXmlDeclaration("1.0", "utf-8", ""))

        Dim sComment As String = vbCrLf + "   Intermediate model representation, do not modify this file by hand" + vbCrLf + vbCrLf + "   Generated: " & Now.ToLongDateString & ", " & Now.ToLongTimeString
        If bIncludeCopyright Then
            sComment += vbCrLf + "   Copyright © 2009-2012,  brennan-marquez, LLC   All rights reserved." + vbCrLf
        End If

        _xDocument.AppendChild(_xDocument.CreateComment(sComment))
        If sXSLfilename.Length > 0 Then
            _xDocument.AppendChild(_xDocument.CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" & sXSLfilename & "'"))
        End If
        _xRoot = _xDocument.CreateElement(_sRootName)
        _xDocument.AppendChild(_xRoot)
    End Sub

    Public Sub SetIncludeReference(xParentNode As XmlElement, sFilename As String)
        System.Diagnostics.Debug.Assert(False, "this include mechanism works--the xi:include element does get included--but include is only supported in XSL 2.0 which Microsoft does not support")
        Dim xIncludeElement As XmlElement = SetElement("xi:include", xParentNode, "href", sFilename)
        Dim xFallbackElement As XmlElement = SetElement("xi:fallback", xIncludeElement)
        SetText("<!-- no such file found -->", xFallbackElement)
    End Sub

    Public Sub Close()
        _xDocument.Save(_sDocumentFilename)
    End Sub

    Public ReadOnly Property XMLDocument() As XmlDocument
        Get
            Return _xDocument
        End Get
    End Property

    Public ReadOnly Property RootElement() As XmlElement
        Get
            If _xRoot Is Nothing Then
                _xRoot = _xDocument.CreateElement(_sRootName)
            End If
            Return _xRoot
        End Get
    End Property

    Public Sub SetText(ByVal sTextString As String, ByVal xElement As XmlElement)
        Dim xCDataSection As XmlCDataSection = xElement.OwnerDocument.CreateCDataSection(sTextString)
        xElement.AppendChild(xCDataSection)
    End Sub

    Public Sub SetAttribute(ByVal sAttributeName As String, ByVal sAttributeValue As String, ByVal xElement As XmlElement)
        Dim xAttribute As XmlAttribute = xElement.OwnerDocument.CreateAttribute(sAttributeName)
        xAttribute.Value = sAttributeValue
        xElement.Attributes.Append(xAttribute)
    End Sub

    Public Sub zSetAttribute(ByVal sAttributeName As String, ByVal sAttributeValue As String, ByVal sXPath As String)
        Dim xElement As XmlElement = GetElement(sXPath)
        SetAttribute(sAttributeName, filterSafeXML(sAttributeValue), xElement)
    End Sub

    Public Function ContainsElement(ByVal sXPath As String) As Boolean
        Dim xElement As XmlElement
        xElement = _xRoot.SelectSingleNode(sXPath)
        Return (xElement IsNot Nothing)
    End Function

    Private Function filterSafeXML(sString As String) As String
        Dim sResultString As String = sString
        sResultString = Regex.Replace(sResultString, "<", "&lt;")
        sResultString = Regex.Replace(sResultString, ">", "&gt;")
        sResultString = Regex.Replace(sResultString, "&", "&amp;")
        sResultString = Regex.Replace(sResultString, """", "&quot;")
        Return sResultString
    End Function

    ''' <summary>
    ''' Given an XPath expression, try to find the specified element and return it if found. Otherwise,
    ''' create a new element (presumably so that the next time the XPath search is performed this
    ''' new element will be found. This function makes it easy to create an element the first time
    ''' then find that same element with each subsequent call without having to add all the checking
    ''' for null logic every time.
    ''' </summary>
    ''' <param name="sXPath">The search expression to execute to find an existing element.</param>
    ''' <param name="sElementName">In case the XPath search turns up nothing, use this name to create
    ''' a new instance of the element</param>
    ''' <param name="xParentElement">The parent element of the newly created element (if a new element needs
    ''' to be created)</param>
    ''' <returns>The found element identified by the XPath expression, if it exists, or a newly created
    ''' element (that may or may not satisfy the XPath expression next time).</returns>
    ''' <remarks></remarks>
    ''' 

    Public Function GetElement(ByVal sXPath As String, ByVal sElementName As String, ByVal xParentElement As XmlElement) As XmlElement
        Dim xElement As XmlElement
        xElement = GetElement(sXPath)
        If xElement Is Nothing Then
            xElement = SetElement(sElementName, xParentElement)
        End If
        Return xElement
    End Function

    Public Function GetElement(ByVal sXPath As String) As XmlElement
        Dim xElement As XmlElement

        Try
            xElement = _xRoot.SelectSingleNode(sXPath)
            Return xElement
        Catch ex As Exception
            Throw New ApplicationException("XPath string '" & sXPath & "' is illegally formed: " & ex.Message)
        End Try
    End Function

    Public Function GetElements(ByVal sXPath As String) As XmlNodeList
        Dim xElementList As XmlNodeList

        Try
            xElementList = _xRoot.SelectNodes(sXPath)
            Return xElementList
        Catch ex As Exception
            Throw New ApplicationException("XPath string '" & sXPath & "' is illegally formed: " & ex.Message)
        End Try
    End Function

    Public Sub setNameValue(ByVal xElement As XmlElement, ByVal sAttributeName As String, ByVal sAttributeValue As String)
        Dim xAttribute As XmlAttribute

        If (sAttributeName IsNot Nothing) And (sAttributeValue IsNot Nothing) Then
            If (sAttributeName.Length > 0) And (sAttributeValue.Length > 0) Then
                _Names.Add(sAttributeName)
                _Values.Add(sAttributeValue)
                xAttribute = _xDocument.CreateAttribute(sAttributeName)
                xAttribute.Value = sAttributeValue
                xElement.Attributes.Append(xAttribute)
            End If
        End If
    End Sub

    Public Function SetElement(ByVal sElementName As String, ByVal xParentElement As XmlElement, _
                          Optional ByVal sAttributeName0 As String = "", Optional ByVal sAttributeValue0 As String = "", _
                          Optional ByVal sAttributeName1 As String = "", Optional ByVal sAttributeValue1 As String = "", _
                          Optional ByVal sAttributeName2 As String = "", Optional ByVal sAttributeValue2 As String = "", _
                          Optional ByVal sAttributeName3 As String = "", Optional ByVal sAttributeValue3 As String = "", _
                          Optional ByVal sAttributeName4 As String = "", Optional ByVal sAttributeValue4 As String = "", _
                          Optional ByVal sAttributeName5 As String = "", Optional ByVal sAttributeValue5 As String = "", _
                          Optional ByVal sAttributeName6 As String = "", Optional ByVal sAttributeValue6 As String = "", _
                          Optional ByVal sAttributeName7 As String = "", Optional ByVal sAttributeValue7 As String = "", _
                          Optional ByVal sAttributeName8 As String = "", Optional ByVal sAttributeValue8 As String = "", _
                          Optional ByVal sAttributeName9 As String = "", Optional ByVal sAttributeValue9 As String = "") As XmlElement

        Dim xElement As XmlElement = Nothing

        Try
            _Names = New List(Of String)()
            _Values = New List(Of String)()

            If xParentElement Is Nothing Then
                Throw New ApplicationException
            End If

            Dim sTokens As String() = Split(sElementName, ":")
            If sTokens.Length > 1 Then
                xElement = _xDocument.CreateElement(sTokens(0), sTokens(1), "http://www.w3.org/2001/XInclude")
            Else
                xElement = _xDocument.CreateElement(sTokens(0))
            End If
            xParentElement.AppendChild(xElement)

            setNameValue(xElement, sAttributeName0, sAttributeValue0)
            setNameValue(xElement, sAttributeName1, sAttributeValue1)
            setNameValue(xElement, sAttributeName2, sAttributeValue2)
            setNameValue(xElement, sAttributeName3, sAttributeValue3)
            setNameValue(xElement, sAttributeName4, sAttributeValue4)
            setNameValue(xElement, sAttributeName5, sAttributeValue5)
            setNameValue(xElement, sAttributeName6, sAttributeValue6)
            setNameValue(xElement, sAttributeName7, sAttributeValue7)
            setNameValue(xElement, sAttributeName8, sAttributeValue8)
            setNameValue(xElement, sAttributeName9, sAttributeValue9)
        Catch ex As Exception
            Dim oErrorHandler As sjmErrorHandler = New sjmErrorHandler()
            oErrorHandler.AnnounceMessage("Exception: " & ex.Message)
        End Try

        Return xElement
    End Function

    Public Sub SetCDATA(ByVal sInnerText As String, ByVal xParentElement As XmlElement)
        Dim xCDATA As XmlCDataSection = _xDocument.CreateCDataSection(sInnerText)
        xParentElement.AppendChild(xCDATA)
    End Sub

    Public Sub Save(ByVal sFilename As String)

        Dim oOutputFile As OutputFile = New OutputFile(sFilename)
        oOutputFile.Add(_xDocument.OuterXml)
        oOutputFile.Close()
    End Sub

    Private Function safeName(ByVal sName As String) As String
        Dim sSafeName As String

        sSafeName = Regex.Replace(sName, "[^A-Za-z0-9_]", "_")
        Return sSafeName
    End Function
End Class
