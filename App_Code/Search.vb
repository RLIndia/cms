﻿'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Namespace WebApplication
	Public Module Search
		Public Sub FindAllResults(ByRef Pages As StringCollection, Optional ByVal Query As String = Nothing, Optional ByVal Domain As String = Nothing)
      Dim Page As Integer = 0
			Do
				Find(Pages, Query, Domain, Page, 100)
				Page += 1
				If Pages.Count < Page * 100 Then
					Exit Do
				End If
			Loop
		End Sub

    Public Sub Find(Pages As StringCollection, Optional Query As String = Nothing, Optional Domain As String = Nothing, Optional Page As Integer = 1, Optional ResultsForPage As Integer = 10)
      Dim Start As Integer = Page * ResultsForPage + 1
      If Not String.IsNullOrEmpty(Query) Then
        Query = HttpUtility.UrlDecode(Query)
      End If
      If Not String.IsNullOrEmpty(Domain) Then
        If Not String.IsNullOrEmpty(Query) Then
          Query &= " "
        End If
        Query &= "site%3A" & Domain
      End If
      Dim GoogleFind As String = "http://google.com/search?access=a&start=" & Start & "&num=" & ResultsForPage & "&q=" & Query
      Dim Html As String = ReadHtmlFromWeb(GoogleFind, Nothing, 20000)
      Dim Exclude As String = Nothing
      If Domain Is Nothing Then
        Exclude = "google"
      End If
      Pages = ExtrapolateLinks(Html, Domain, Exclude, Pages)
    End Sub

	End Module


End Namespace