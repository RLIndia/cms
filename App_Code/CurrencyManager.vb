'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Namespace WebApplication

	Public Module CurrencyManager
		Private ExchangeCurrencyBase As String = "EUR"
		Private ExchangeRates As Collections.Generic.Dictionary(Of String, Single)
		Sub UpdateCurrencyExchange()
      Const ResourceUrl As String = "http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml"
			Try
				Dim Xml As System.Xml.XmlDocument
				Try
					Xml = ReadXmlFromWeb(ResourceUrl)
					WriteAll(Xml.InnerXml, MapPath(ReadWriteDirectory & "\ExchangeRates.xml"))
					ExchangeRateUnavailable = False
				Catch ex As Exception
					ExchangeRateUnavailable = True
					Xml = ReadXml(MapPath(ReadWriteDirectory & "\ExchangeRates.xml"))
				End Try
				Dim Nodes As System.Xml.XmlNodeList = Xml.ChildNodes(1).ChildNodes(2).ChildNodes(0).ChildNodes
				Dim NewEcxhangeRates As New Collections.Generic.Dictionary(Of String, Single)
				For Each Node As System.Xml.XmlNode In Nodes
          NewEcxhangeRates.Add(Node.Attributes("currency").InnerText, Convert.ToSingle(Node.Attributes("rate").InnerText, Globalization.CultureInfo.InvariantCulture))
				Next
				ExchangeRates = NewEcxhangeRates
			Catch ex As Exception
				ExchangeRateUnavailable = True
			End Try
		End Sub

		Function ExchangeRate(ByVal Currency As String, ByVal CurrencyBase As String) As Single
			Static SyncLockExchangeRate As New Object
			SyncLock SyncLockExchangeRate
				If ExchangeRates Is Nothing AndAlso ExchangeRateUnavailable = False Then
					UpdateCurrencyExchange()
				End If
			End SyncLock
			If ExchangeRates IsNot Nothing Then
				Currency = UCase(Currency)
				CurrencyBase = UCase(CurrencyBase)
				If Currency = CurrencyBase Then
					Return 1
				Else
					If Currency = ExchangeCurrencyBase Then
						If ExchangeRates.ContainsKey(CurrencyBase) Then
							Return ExchangeRates(CurrencyBase)
						End If
					ElseIf CurrencyBase = ExchangeCurrencyBase Then
						If ExchangeRates.ContainsKey(Currency) Then
							Return 1 / ExchangeRates(Currency)
						End If
					Else
						Return ExchangeRate(Currency, ExchangeCurrencyBase) / ExchangeRate(CurrencyBase, ExchangeCurrencyBase)
					End If
				End If
			End If
      Return 0.0!
    End Function

    Public Function ExchangeTable(Currency As String, Optional Language As LanguageManager.Language = LanguageManager.Language.English, Optional Skin As Skin = Nothing) As Control
      Currency = UCase(Currency)
      Dim Table As New HtmlControls.HtmlTable
      'Table.CssClass = "Exchange"
      Dim FirstRow As New HtmlControls.HtmlTableRow
      Dim Cell As New HtmlControls.HtmlTableCell
      Dim Setting As SubSite = CurrentSetting()
      AddLabel(Cell, UCase(Phrase(Language, 41)) & " " & Now.ToString(Setting.DateFormat, Setting.Culture()))
      Cell.ColSpan = 3
      FirstRow.Controls.Add(Cell)
      Table.Controls.Add(FirstRow)
      Table.Controls.Add(Row("", "EUR", "USD"))
      Table.Controls.Add(Row(Currency, ExchangeRate("EUR", Currency).ToString("0.###", Setting.Culture()), ExchangeRate("USD", Currency).ToString("0.###", Setting.Culture())))
      Table.Controls.Add(Row("USD", ExchangeRate("EUR", "USD").ToString("0.###", Setting.Culture()), "1"))
      Table.Controls.Add(Row("EUR", "1", ExchangeRate("USD", "EUR").ToString("0.###", Setting.Culture())))
      Table.Controls.Add(Row("CHF", ExchangeRate("EUR", "CHF").ToString("0.###", Setting.Culture()), ExchangeRate("USD", "CHF").ToString("0.###", Setting.Culture())))
      Table.Controls.Add(Row("JPY", ExchangeRate("EUR", "JPY").ToString("0.###", Setting.Culture()), ExchangeRate("USD", "JPY").ToString("0.###", Setting.Culture())))
      Table.Style.Add("width", "100%")
      Dim Div As New WebControl(HtmlTextWriterTag.Div)
      Div.CssClass = "Exchange"
      Div.Controls.Add(Table)
      Return Div
    End Function

		Private Function Row(ByRef c1 As String, ByRef c2 As String, ByRef c3 As String) As HtmlControls.HtmlTableRow
			Dim Cell1 As New HtmlControls.HtmlTableCell
			Cell1.InnerText = c1
			Dim Cell2 As New HtmlControls.HtmlTableCell
			Cell2.InnerText = c2
			Dim Cell3 As New HtmlControls.HtmlTableCell
			Cell3.InnerText = c3
			Dim NewRow As New HtmlControls.HtmlTableRow
			NewRow.Controls.Add(Cell1)
			NewRow.Controls.Add(Cell2)
			NewRow.Controls.Add(Cell3)
			Return NewRow
		End Function

		Public Function Exchange(ByVal Value As Double, ByVal Currency As String, ByVal ReturnCurrency As String, Optional ByRef Round As Integer = 2) As Double
			Return Value * ExchangeRate(Currency, ReturnCurrency)
		End Function

		Class Money
			Public Currency As String = "EUR"
			Public Amount As Double
		End Class
	End Module

End Namespace