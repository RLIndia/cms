'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Imports System.Xml.Serialization
Imports System.Reflection
Imports System.IO

Namespace WebApplication
  Public Module converter

    Function ObjectToXml(ByVal Obj As Object) As System.Xml.XmlDocument
      Dim myXmlDocument As New System.Xml.XmlDocument
      Using Stream As New System.IO.MemoryStream
        Dim xml As New XmlSerializer(Obj.GetType)
        Dim xmlns As New XmlSerializerNamespaces
        xmlns.Add(String.Empty, String.Empty)
        xml.Serialize(Stream, Obj, xmlns)
        Stream.Position = 0
        myXmlDocument.Load(Stream)
      End Using
      Return myXmlDocument
    End Function

    Function CryptUrl(ByVal Url As String) As String
      If Not String.IsNullOrEmpty(Url) Then
        Dim Position As Integer = InStr(Url, "/") - 1
        If Position < 0 Then
          Return StringToBase64(Url, True)
        Else
          Dim Domain As String = Url.Substring(0, Position)
          If CBool(Domain.Length) Then
            Dim Crypt = StringToBase64(Domain, True)
            If InStr(Base64ToString(Crypt), "?") <> 0 Then
              Crypt = StringToBase64(Domain)
            End If
            Return Crypt & Url.Substring(Position)
          End If
        End If
      End If
      Return Nothing
    End Function
    Function DecryptUrl(ByVal Url As String) As String
      Try
        Dim Position As Integer = InStr(Url, "/") - 1
        If Position < 0 Then
          Position = Len(Url)
        End If
        Dim Crypt As String = Url.Substring(0, Position)
        Dim Domain = Base64ToString(Crypt, True)
        If InStr(Domain, ".") = 0 Then
          Domain = Base64ToString(Crypt)
        End If
        Return Domain & Url.Substring(Position)
      Catch ex As Exception
      End Try
      Return Nothing
    End Function
    Function StringToBase64(ByVal Text As String, Optional ByVal AsciiEncoding As Boolean = False) As String
      'This function is a quick way to crypt a text string
      Dim Bytes As Byte() = StringToByteArray(Text, AsciiEncoding)
      Return System.Convert.ToBase64String(Bytes)
    End Function
    Function Base64ToString(ByVal Text As String, Optional ByVal AsciiEncoding As Boolean = False) As String
      'Now easy to decrypt a data
      Dim Bytes As Byte() = System.Convert.FromBase64String(Text)
      Return ByteArrayToString(Bytes, AsciiEncoding)
    End Function

    Function ControlToText(ByVal Control As Control) As String
      Dim StringWriter As New System.IO.StringWriter
      Dim HtmlTextWriter As New System.Web.UI.HtmlTextWriter(StringWriter)
      Try
        Control.RenderControl(HtmlTextWriter)
      Catch ex As Exception
        'HttpContext.Current.Request is not available
        Dim request As New Hosting.SimpleWorkerRequest("default.aspx", Nothing, New StringWriter())
        Dim FakeContext As New HttpContext(request)
        Dim Swap As HttpContext = HttpContext.Current
        HttpContext.Current = FakeContext
        Control.RenderControl(HtmlTextWriter)
        HttpContext.Current = Swap
      End Try
      Return StringWriter.ToString
    End Function

    Function StreamToString(ByVal Stream As System.IO.MemoryStream) As String
      Dim Buffer() As Byte = Stream.GetBuffer
      Return ByteArrayToString(Buffer)
    End Function
    Function StringToStream(ByVal Text As String) As System.IO.Stream
      Dim Bytes() As Byte = StringToByteArray(Text)
      Return New System.IO.MemoryStream(Bytes)
    End Function

    Function StringToByteArray(ByVal Text As String, ByVal EncodingName As String) As Byte()
      'EncodingName possible values here http://errore-404.com/?p=5&a=71&t=tabellacodiciencoding
      If Not String.IsNullOrEmpty(Text) Then
        Dim Encoding As System.Text.Encoding = System.Text.Encoding.GetEncoding(EncodingName)
        Return Encoding.GetBytes(Text)
      End If
      Return Nothing
    End Function

    Function StringToByteArray(ByVal Text As String, Optional ByVal ASCIIEncoding As Boolean = False) As Byte()
      If Not String.IsNullOrEmpty(Text) Then
        If ASCIIEncoding Then
          Return System.Text.Encoding.ASCII.GetBytes(Text)
        Else
          'The object System.Text.Encoding.Unicode have a problem in Windows x64. Replache this object with System.Text.Encoding.GetEncoding("utf-16LE") 
          Return System.Text.Encoding.GetEncoding("utf-16LE").GetBytes(Text) 'Unicode encoding
        End If
      End If
      Return Nothing
    End Function

    Function ByteArrayToString(ByVal Bytes() As Byte, ByVal EncodingName As String) As String
      'EncodingName possible values here http://errore-404.com/?p=5&a=71&t=tabellacodiciencoding
      Dim Encoding As System.Text.Encoding = System.Text.Encoding.GetEncoding(EncodingName)
      Return Encoding.GetString(Bytes)
    End Function


    Function ByteArrayToString(ByVal Bytes() As Byte, Optional ByVal ASCIIEncoding As Boolean = False) As String
      If ASCIIEncoding Then
        Return System.Text.Encoding.ASCII.GetString(Bytes)
      Else
        Return System.Text.Encoding.GetEncoding("utf-16LE").GetString(Bytes) 'Unicode encodin
      End If
    End Function

    Function TextToDate(ByVal Text As String) As Date
      If Not String.IsNullOrEmpty(Text) Then
        Try
          Return Date.Parse(Text, New Globalization.CultureInfo("en-US"))
        Catch ex As Exception
          'Format not valid
        End Try
      End If
      Return Nothing
    End Function

    Function DateToText(ByVal DateValue As Date) As String
      Return DateValue.ToString(New Globalization.CultureInfo("en-US"))
    End Function

    Function RemoveAccent(ByVal Text As String) As String
      Dim stFormD As String = Text.Normalize(NormalizationForm.FormD)
      Dim sb As New StringBuilder
      For Ich As Integer = 0 To stFormD.Length - 1
        Dim Uc As System.Globalization.UnicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD(Ich))
        If Uc <> System.Globalization.UnicodeCategory.NonSpacingMark Then
          sb.Append(stFormD(Ich))
        End If
      Next
      Return (sb.ToString.Normalize(NormalizationForm.FormC))
    End Function

    Function String2Enum(ByVal TypeEnum As System.Type, ByVal Name As String) As [Enum]
      'Verificare se sia equivalente a  Return CType([Enum].Parse(TypeEnum, Name), [Enum])
      Return CType([Enum].GetValues(TypeEnum).GetValue(Array.IndexOf([Enum].GetNames(TypeEnum), Name)), [Enum]) 'http://forums.asp.net/p/1879664/5289345.aspx/1?p=True&t=634953168577361250
    End Function

    Function String2HexHtml(ByVal Text As String, Optional ByVal HexMark As String = "%") As String
      Dim StringBuilder As New StringBuilder(Len(Text) * (2 + Len(HexMark)))
      Dim AscValue As Integer
      For Each Chr As Char In Text.ToCharArray
        AscValue = AscW(Chr)
        If AscValue <= &HFF Then
          StringBuilder.Append(HexMark & AscValue.ToString("X2"))
          '  StringBuilder.Append(HexMark & Extension.Right("00" & Hex(AscValue), 2))
        Else
          StringBuilder.Append(HexMark & AscValue.ToString("X4"))
          ' StringBuilder.Append(HexMark & Extension.Right("0000" & Hex(AscValue), 4))
        End If
      Next
      Return StringBuilder.ToString
    End Function

    Function Bytes2Hex(ByVal Bytes As Byte(), Optional ByVal HexMark As String = Nothing) As String
      Bytes2Hex = Nothing
      For Each B As Byte In Bytes
        Bytes2Hex &= HexMark & B.ToString("X2")
        'Bytes2Hex &= HexMark & Extension.Right("00" & Hex(B), 2)
      Next
    End Function

    Function AbjustForJavascriptString(ByVal Text As String) As String
      'This function abjust the text for JavaScript use

      If Text IsNot Nothing Then
        Return System.Web.HttpUtility.JavaScriptStringEncode(Text)
      End If

      'Dim AscValue As Integer
      'If Text <> "" Then
      '  Dim StringBuilder As New StringBuilder(Len(Text) * 3)
      '  For Each Chr As Char In Text
      '    If Char.IsLetterOrDigit(Chr) Then
      '      StringBuilder.Append(Chr)
      '    ElseIf Chr = "’"c Or Chr = "€"c Then
      '      StringBuilder.Append(Chr)
      '    Else
      '      AscValue = AscW(Chr)
      '      If AscValue <= &HFF Then
      '        StringBuilder.Append("\x" & Extension.Right("00" & Hex(AscValue), 2))
      '      Else
      '        StringBuilder.Append("\u" & Extension.Right("0000" & Hex(AscValue), 4))
      '      End If
      '    End If
      '  Next
      '  Return StringBuilder.ToString
      'End If
      Return Nothing
    End Function

    Function AbjustNameFile(ByVal Text As String) As String
      AbjustNameFile = System.Web.HttpUtility.UrlEncode(Text, System.Text.Encoding.UTF8)
    End Function

    Public Function HtmlToXhtml(ByVal Html As String) As String
      'Convert Html To Text
      Dim Tag As Boolean = False
      If Not Html Is Nothing Then
        Dim StringBuilder As New System.Text.StringBuilder(Html.Length)
        Dim Chr As Char
        For N As Integer = 0 To Html.Length - 1
          Chr = Html.Chars(N)
          Select Case Chr
            Case "<"c
              Tag = True
              StringBuilder.Append(Chr)
            Case ">"c
              Tag = False
              StringBuilder.Append(Chr)
            Case Else
              If Tag Then
                StringBuilder.Append(Char.ToLower(Chr))
              Else
                StringBuilder.Append(Chr)
              End If
          End Select
        Next
        Html = ReplaceText(Html, "<br>", "<br />")
        Html = ReplaceText(Html, "<hr>", "<hr />")
        Return Html
      End If
      Return Nothing
    End Function

    Function HtmlEncode(Text As String) As String
      Return ReplaceBin(HttpUtility.HtmlEncode(Text), vbLf, "<br />")
    End Function
    Function StringToRandom(Text As String) As Random
      Dim Random As New Random(0)
      For Each C As Char In Text
        Random = New Random(AscW(C) Xor Random.Next)
      Next
      Return Random
    End Function

  End Module

End Namespace
