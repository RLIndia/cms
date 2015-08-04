'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Namespace WebApplication
	Public Module EarthManager
    Function QueryStringParameters(Optional ByVal CityName As String = Nothing, Optional ByVal Country As String = Nothing, Optional ByVal Latitude As Single = 0.0!, Optional ByVal Longitude As Single = 0.0!) As String()
      Dim Result() As String = Nothing
      If CityName IsNot Nothing Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.City)
        AddElement(Result, CityName)
      End If
      If Country IsNot Nothing Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.Country)
        AddElement(Result, Country)
      End If
      'Dim Culture As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
      If Latitude <> 0.0! Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.Latitude)
        'AddElement(Result, Latitude.ToString(Culture)) 'http://forums.asp.net/p/1879671/5289354.aspx/1?p=True&t=634953181716283221
        AddElement(Result, Latitude.ToString("0.####", Globalization.CultureInfo.InvariantCulture))
      End If
      If Longitude <> 0.0! Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.Longitude)
        'AddElement(Result, Longitude.ToString(Culture)) 'http://forums.asp.net/p/1879671/5289354.aspx/1?p=True&t=634953181716283221
        AddElement(Result, Longitude.ToString("0.####", Globalization.CultureInfo.InvariantCulture))
      End If
      Return Result
    End Function

	End Module
End Namespace