

Imports System.Windows.Forms.Control

Public Interface IOutputLanguage
    Sub CreateDomains(ByVal _oRepository As EA.Repository, ByVal bIncludeDebug As Boolean, ByVal BuildDocumentation As Boolean)
End Interface
