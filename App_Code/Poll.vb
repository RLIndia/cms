'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Imports Microsoft.VisualBasic
Namespace WebApplication
	Public Module PollManager
    Function AddPoll(ByVal DomainConfiguration As DomainConfiguration, ByVal Setting As SubSite, ByRef Html As String, ByVal Context As PollContext, ByVal Votes() As Integer, ByVal ParamArray Params() As Integer) As Boolean
      Dim Result As Boolean = False
      Dim Html2 As String = ReplaceText(Html, "<p>", "")
      Html2 = ReplaceText(Html, "</p>", "<br />")
      Html2 = ReplaceBin(Html2, vbCr, "")
      Html2 = ReplaceBin(Html2, vbLf, "")
      Html2 = ReplaceBin(Html2, "<br>", "<br />")
      Html2 = ReplaceBin(Html2, "</blockquote>", "</blockquote><br />")
      Dim Paragraphs As String() = Split(Html2, "<br />")
      'Dim IsPoll As Boolean
      Dim Poll As StringCollection = Nothing
      Dim Flag As Boolean = False
      Dim Blockquote As Boolean = False
      For Each P As String In Paragraphs
        If Not Blockquote AndAlso P.Contains("<blockquote>") Then
          Blockquote = True
        End If
        If Blockquote Then
          If P.Contains("</blockquote>") Then
            Blockquote = False
          End If
        Else
          If Len(P) >= 10 AndAlso (P.Last = "?"c OrElse P.First = "¿"c OrElse P.Last = "？"c) Then '¿=191 ？=65311 http://rishida.net/tools/conversion/
            Flag = True
            Poll = New StringCollection
            Poll.Add(P)
            'IsPoll = True
          Else
            If Flag = True Then
              Dim LastChar As Char
              Dim LastOk As Boolean = False
              If Len(Trim(P)) > 0 Then
                LastChar = P(P.Length - 1)
                Select Case LastChar
                  Case """"c, "!"c, ")"c, "]"c, "£"c, "€"c, "°"c, "$"c, "%"c '£=163 €=8364 °=176 http://rishida.net/tools/conversion/
                    LastOk = True
                End Select
              End If
              Dim PharagrapDecoded As String = HttpUtility.HtmlDecode(P)
              If Len(Trim(P)) > 0 AndAlso (Char.IsLetterOrDigit(PharagrapDecoded(PharagrapDecoded.Length - 1)) OrElse LastOk) Then
                Poll.Add(P)
              Else
                Flag = False
                If Poll.Count <= 2 Then
                  Poll = Nothing
                Else
                  Exit For
                End If
              End If
            Else
            End If
          End If
        End If
      Next
      If Poll IsNot Nothing AndAlso Poll.Count > 2 Then
        Result = True
        If Context <> PollContext.IsPoll Then
          Dim Position As Integer = 0
          Dim NLine As Integer = 0
          Dim TotalVotes As Integer = 0
          If Votes IsNot Nothing Then
            For Each Vote As Integer In Votes
              TotalVotes += Vote
            Next
          End If
          For Each Line As String In Poll
            Position = InStr(Position + 1, Html, Line)
            If CBool(Position) Then
              Dim ReplaceString As String
              Select Case NLine
                Case 0
                  If Line.StartsWith("<p>") Then
                    ReplaceString = "<p><strong class=""poll"">" & Line.Substring(3) & "</strong>"
                  Else
                    ReplaceString = "<strong class=""poll"">" & Line & "</strong>"
                  End If
                Case Else
                  Dim Vote As Integer
                  If Votes IsNot Nothing AndAlso Votes.Length >= NLine Then
                    Vote = Votes(NLine - 1)
                  Else
                    Vote = 0
                  End If
                  Dim Link As New HyperLink
                  Link.CssClass = "pollanswer"
                  If Line.StartsWith("<p>") Then
                    Link.Text = "<p>" & NLine & ") " & Line.Substring(3)
                  Else
                    Link.Text = NLine & ") " & Line
                  End If
                  Dim TextParams As String = "c" & vbTab & Context & vbTab & "v" & vbTab & (NLine - 1)
                  Dim N As Integer = 0
                  For Each Param As Integer In Params
                    N += 1
                    TextParams &= vbTab & N & vbTab & Param
                  Next
                  Link.NavigateUrl = Href(DomainConfiguration, Setting.Name, False, "poll.aspx", Split(TextParams, vbTab))
                  Link.ToolTip = EncodingAttribute(Phrase(Setting.Language, 77))

                  Link.Attributes.Add("rel", "nofollow")

                  Dim Label As New Label
                  Label.CssClass = "pollresult"
                  Dim Percentage As Double
                  If CBool(Vote) Then
                    Percentage = Vote / TotalVotes
                  Else
                    Percentage = 0
                  End If
                  Label.Text = " " & Percentage.ToString("#0.0%", Setting.Culture)
                  Label.ToolTip = Vote & "/" & TotalVotes
                  Label.Style.Add("font-Size", "80%")
                  ReplaceString = ControlToText(Link) & ControlToText(Label)
              End Select
              Html = Html.Substring(0, Position - 1) & Replace(Html, Line, ReplaceString, Position, 1)
              Position = Position + ReplaceString.Length
            End If
            NLine += 1
          Next
        End If
      End If
      Return Result
    End Function
		Enum PollContext
			IsPoll 'This value test only is the html is a poll 
			Forum
			Archine
		End Enum
		Private PollUsers As ArrayList = LoadPollUsers
		Private Function LoadPollUsers() As ArrayList
      Dim List As ArrayList = CType(LoadObject(GetType(ArrayList), "PollUsers"), ArrayList)
      If List Is Nothing Then
        List = New ArrayList
      End If
			Return List
		End Function

    Sub Vote(ByVal Context As PollContext, ByVal VoteN As Integer, ByVal ParamArray Params() As String)
      If Not IsCrawler() Then
        Dim CokieKey As String = Context & "_" & Join(Params, "_")
        Dim Cokie As String = Cookie(CokieKey)
        If String.IsNullOrEmpty(Cokie) Then
          Cookie(CokieKey) = "1"
          Dim Key As String = HttpContext.Current.Request.ServerVariables("REMOTE_ADDR") & " " & CokieKey
          If Not PollUsers.Contains(Key) Then
            If Not IsProxy(HttpContext.Current.Request.UserHostAddress) Then
              PollUsers.Add(Key)
              SaveObject(PollUsers, "PollUsers")
              Dim Votes As Integer()
              Select Case Context
                Case PollContext.Forum
                  Dim ThisReply As ForumManager.Reply = ForumManager.Reply.Load(ValInt(Params(0)), ValInt(Params(1)), ValInt(Params(2)))
                  Votes = ThisReply.Poll
                  ActionVote(Votes, VoteN)
                  ThisReply.Poll = Votes
                  ThisReply.Save()
                Case PollContext.Archine
                  Dim Document As Common.HtmlDocument = New Common.HtmlDocument(PageNameFile(ValInt(Params(0)), ValInt(Params(1)), CType(ValInt(Params(2)), LanguageManager.Language)))
                  Dim Poll As String = Nothing
                  If Document.MetaTags.Collection.ContainsKey("poll") Then
                    Poll = Document.MetaTags.Collection("poll")
                  End If
                  Votes = PollStringToVotes(Poll)
                  ActionVote(Votes, VoteN)
                  Document.MetaTags.AddMetaTag("poll", VotesToPollString(Votes))
                  WriteAll(Document.Html, PageNameFile(ValInt(Params(0)), ValInt(Params(1)), CType(ValInt(Params(2)), LanguageManager.Language)))
              End Select
            End If
          End If
        End If
      End If
    End Sub
		Function PollStringToVotes(ByVal Poll As String) As Integer()
			Dim Votes() As Integer
      If Not String.IsNullOrEmpty(Poll) Then
        Dim Polls() As String = Split(Poll, ",")
        ReDim Votes(UBound(Polls))
        Dim N As Integer = 0
        For Each Value As String In Split(Poll, ",")
          Votes(N) = CInt(Value)
          N += 1
        Next
        Return Votes
      End If
      Return Nothing
    End Function
		Function VotesToPollString(ByVal Votes() As Integer) As String
			If Votes IsNot Nothing Then
        Dim Poll As String = Nothing
				For Each Vote As Integer In Votes
					Poll &= Vote & ","
				Next
				Poll = Poll.Substring(0, Poll.Length - 1)
				Return Poll
			End If
      Return Nothing
    End Function

		Private Sub ActionVote(ByRef Votes() As Integer, ByRef VoteN As Integer)
			If Votes Is Nothing OrElse UBound(Votes) < VoteN Then
				ReDim Preserve Votes(VoteN)
			End If
			Votes(VoteN) += 1
		End Sub
	End Module

End Namespace
