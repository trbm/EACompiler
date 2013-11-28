Imports System.Text.RegularExpressions

Public Class Canonical
    Private Shared _oCanonicalTypes As List(Of String)


    Public Shared Function CanonicalName(ByVal sName As String) As String
        Dim sReturnNameString As String = Trim(sName)
        Dim arrString As Char() = sReturnNameString.ToCharArray
        If sName.Length > 0 Then
            If IsNumeric(arrString(0)) Then                                                 ' if the leading character is numeric
                sReturnNameString = "_" & sReturnNameString                                 ' add a prefix underscore
            End If
            sReturnNameString = Regex.Replace(sReturnNameString, "[\- \\/\(\)\=]", "_")        ' otherwise, just replace these characters with underscore 
            ' note that we are allowing the period character to allow for namespace specs
        End If
        Return sReturnNameString
    End Function

    Public Shared Function IsCanonicalType(sTypeName As String) As Boolean
        Dim bIsCanonicalType As Boolean = False

        If _oCanonicalTypes Is Nothing Then
            _oCanonicalTypes = New List(Of String)
            _oCanonicalTypes.Add("SECONDS")
            _oCanonicalTypes.Add("MILLISECONDS")
            _oCanonicalTypes.Add("BYTE_STRING")
            _oCanonicalTypes.Add("SHORT_STRING")
            _oCanonicalTypes.Add("VOLTAGE_DIFF_ADC_COUNTS")
            _oCanonicalTypes.Add("OTHER_ADC_COUNTS_U16")
            _oCanonicalTypes.Add("CLICKS_U16")
            _oCanonicalTypes.Add("VOLTAGE_ADC_COUNTS_U16")
            _oCanonicalTypes.Add("CURRENT_ADC_COUNTS_U16")
            _oCanonicalTypes.Add("POWER_ADC_COUNTS_U16")
            _oCanonicalTypes.Add("TEMPERATURE_ADC_COUNTS_U16")
            _oCanonicalTypes.Add("OTHER_ADC_COUNTS_U32")
            _oCanonicalTypes.Add("CLICKS_U32")
            _oCanonicalTypes.Add("VOLTAGE_ADC_COUNTS_U32")
            _oCanonicalTypes.Add("CURRENT_ADC_COUNTS_U32")
            _oCanonicalTypes.Add("POWER_ADC_COUNTS_U32")
            _oCanonicalTypes.Add("TEMPERATURE_ADC_COUNTS_U32")
            _oCanonicalTypes.Add("CENTIWATTS")
            _oCanonicalTypes.Add("CENTIVOLTS")
            _oCanonicalTypes.Add("CENTIAMPS")
            _oCanonicalTypes.Add("MILLIVOLTS")
            _oCanonicalTypes.Add("CENTIHERTZ")
            _oCanonicalTypes.Add("bool")
            _oCanonicalTypes.Add("float")
            _oCanonicalTypes.Add("byte")
            _oCanonicalTypes.Add("uchar")
            _oCanonicalTypes.Add("uint")
            _oCanonicalTypes.Add("uint16")
            _oCanonicalTypes.Add("int")
            _oCanonicalTypes.Add("ulong")
            _oCanonicalTypes.Add("long")
            _oCanonicalTypes.Add("char")
            _oCanonicalTypes.Add("unsigned long")
            _oCanonicalTypes.Add("unsigned int")
            _oCanonicalTypes.Add("unsigned char")
            _oCanonicalTypes.Add("unsigned byte")
        End If
        bIsCanonicalType = _oCanonicalTypes.Contains(sTypeName)
        Return bIsCanonicalType
    End Function

End Class
