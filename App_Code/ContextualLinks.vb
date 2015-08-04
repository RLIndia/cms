Option Explicit On
Option Strict On
Namespace WebApplication
  Public Class ContextualLink

    Const Algorithm As Integer = 1 'Switch 0=Classic, 1=Recursive (0 is fast for short vocabolary, 1 is fast for big vocabolary but slow for very short)

    Private Shared Links As Collections.Generic.Dictionary(Of LanguageManager.Language, Link())
    Private Shared LinkFinderStructures As Collections.Generic.Dictionary(Of LanguageManager.Language, LinkFinderStructure)

    Class Link
      Public Text As String
      Public NavigateUrl As String
      Public Archive As Integer
      Public Level As Components.LevelMenuItem
      Public Category As Integer
      Public ToolTip As String
      Public Language As LanguageManager.Language
      Public Type As Config.SubSite.CerrelatedStructure.CorrelatedStatus
      Public Sub New(Text As String, NavigateUrl As String, Archive As Integer, Optional ToolTip As String = Nothing, Optional Language As LanguageManager.Language = LanguageManager.Language.NotDefinite, Optional Type As Config.SubSite.CerrelatedStructure.CorrelatedStatus = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context, Optional Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional Category As Integer = 0)
        Me.Text = Text
        Me.NavigateUrl = NavigateUrl
        Me.Archive = Archive
        Me.Level = Level
        Me.Category = Category
        Me.ToolTip = ToolTip
        Me.Language = Language
        Me.Type = Type
      End Sub
    End Class

    Public Shared Function AddLinks(ByRef Html As String, ByVal Setting As SubSite, ByVal DomainConfiguration As DomainConfiguration, ByVal Archive As Integer, Optional AddWWW As AddWWW = AddWWW.Auto) As String
      Html = UrlToLink(Html, Setting, DomainConfiguration)
      AddContextualLinks(Html, Setting, Archive, LevelMenuItem.Sphera, 0, False, AddWWW)
      Return Html
    End Function

    Shared Sub AddContextualLinks(ByRef Html As String, ByVal Setting As SubSite, ByVal Archive As Integer, Optional ByVal Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional ByVal Category As Integer = 0, Optional AbsoluteUrl As Boolean = False, Optional AddWWW As AddWWW = AddWWW.Auto)
      If Links IsNot Nothing AndAlso Links.ContainsKey(Setting.Language) AndAlso Setting.SEO.ApplyCorrelatedWordsFromOtherSites Then
        AddLinksToText(Html, Links(Setting.Language), TextType.Html, False, Archive, Setting.Language, Level, Category, AbsoluteUrl, AddWWW)
      ElseIf LinkFinderStructures IsNot Nothing AndAlso Setting.SEO.ApplyCorrelatedWordsFromOtherSites Then
        Dim FinderStructure As LinkFinderStructure = Nothing
        If ContextualLink.LinkFinderStructures.TryGetValue(Setting.Language, FinderStructure) Then
          AddLinksToText(Html, FinderStructure, TextType.Html, False, Archive, Setting.Language, Level, Category, AbsoluteUrl, AddWWW)
        End If
      End If
    End Sub

    'Add a single link to text
    Shared Sub AddLinkToText(ByRef Text As String, ByVal Link As Link, Optional ByVal SourceType As TextType = TextType.Text, Optional ByVal Sort As Boolean = False, Optional ByVal Archive As Integer = 0, Optional Language As Language = Language.NotDefinite)
      Dim SingleLink(0) As Link
      SingleLink(0) = Link
      AddLinksToText(Text, SingleLink, SourceType, Sort, Archive, Language)
    End Sub

    Shared Sub AddLinksToText(ByRef Text As String, ByRef Links() As Link, Optional SourceType As TextType = TextType.Text, Optional Sort As Boolean = False, Optional Archive As Integer = 0, Optional Language As Language = Language.NotDefinite, Optional Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional Category As Integer = 0, Optional AbsoluteUrl As Boolean = False, Optional AddWWW As AddWWW = AddWWW.Auto)
      ExecuteAddLinksToText(Text, Nothing, Links, SourceType, Sort, Archive, Language, Level, Category, AbsoluteUrl, AddWWW)
    End Sub

    Shared Sub AddLinksToText(ByRef Text As String, LinkFinderStructure As LinkFinderStructure, Optional SourceType As TextType = TextType.Text, Optional Sort As Boolean = False, Optional Archive As Integer = 0, Optional Language As Language = Language.NotDefinite, Optional Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional Category As Integer = 0, Optional AbsoluteUrl As Boolean = False, Optional AddWWW As AddWWW = AddWWW.Auto)
      ExecuteAddLinksToText(Text, LinkFinderStructure, Nothing, SourceType, Sort, Archive, Language, Level, Category, AbsoluteUrl, AddWWW)
    End Sub

    Private Shared Sub ExecuteAddLinksToText(ByRef Text As String, LinkFinderStructure As LinkFinderStructure, Links() As Link, Optional SourceType As TextType = TextType.Text, Optional Sort As Boolean = False, Optional Archive As Integer = 0, Optional Language As Language = Language.NotDefinite, Optional Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional Category As Integer = 0, Optional AbsoluteUrl As Boolean = False, Optional AddWWW As AddWWW = AddWWW.Auto)
      If Not String.IsNullOrEmpty(Text) Then
        If Links IsNot Nothing OrElse LinkFinderStructure IsNot Nothing Then
          Dim www As String = Nothing
          If AddWWW = ContextualLink.AddWWW.Auto Then
            If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Request IsNot Nothing Then
              If DomainName(HttpContext.Current.Request).StartsWith("www.") Then
                www = "www."
              End If
            End If
          ElseIf AddWWW = ContextualLink.AddWWW.Yes Then
            www = "www."
          End If
          Dim MapHtml() As Boolean = Nothing
          If SourceType = TextType.Html Then
            MapHtml = CheckHtml(Text)
          End If
          If Sort Then
            Dim SortCriterion As IComparer(Of Link) = New SortLinkByTextLength(CurrentSetting().Language)
            System.Array.Sort(Links, SortCriterion)
          End If
          Dim Replaces As New Collections.Generic.List(Of ReplaceLink)

          Dim TextLCase As String = LCase(Text) 'To speedUp this function Instr instruction is in Bynary mode (all text parameter is in lcase)
          If Links IsNot Nothing Then
            'perform Algorithm 0
            Dim S As Integer 'S is a base 1 word position inside the text
            For Each Link As Link In Links
              '======= block #1: If you change this block, then modify the block #1 ====================
              If Link.Type = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Enabled OrElse (Link.Type = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context AndAlso Link.Archive = Archive) Then
                If Link.Level = LevelMenuItem.Sphera OrElse Link.Category = Category Then
                  '======= end block #1 ================================================================
                  'Dim NavigateUrl As String = Nothing
                  S = 1 'S is base 1
                  Do
                    S = FindWord(S, TextLCase, Link.Text, Common.TextType.Text, MapHtml, Microsoft.VisualBasic.CompareMethod.Binary) 'To speedUp this function Instr instruction is in Bynary mode (all text parameter is in lcase)
                    If CBool(S) Then
                      PrepareToReplace(Replaces, Link, Archive, Language, AbsoluteUrl, www, Text, TextLCase, S)
                    Else
                      Exit Do
                    End If
                  Loop
                End If
              End If
            Next
          Else
            'perform Algorithm 1
            Dim PrevChr As Char
            Dim ThisChar As Char = " "c
            For Idx As Integer = 0 To TextLCase.Length - 1
              PrevChr = ThisChar
              ThisChar = TextLCase(Idx)
              If MapHtml Is Nothing OrElse Not MapHtml(Idx) Then
                If Char.IsLetterOrDigit(PrevChr) = False Then
                  Dim LinksAtIdx As Collections.Generic.List(Of Link)
                  LinksAtIdx = LinkFinderStructure.ContainWord(TextLCase, Idx)
                  If LinksAtIdx IsNot Nothing Then
                    Dim HtmlCollision As Boolean
                    HtmlCollision = False
                    If MapHtml IsNot Nothing Then
                      For N = Idx To Idx + LinksAtIdx(0).Text.Length - 1
                        If MapHtml(Idx) = True Then
                          HtmlCollision = True
                          Exit For
                        End If
                      Next
                    End If
                    If HtmlCollision = False Then
                      For Each Link As Link In LinksAtIdx
                        '======= block #1: If you change this block, then modify the block #1 ====================
                        If Link.Type = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Enabled OrElse (Link.Type = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context AndAlso Link.Archive = Archive) Then
                          If Link.Level = LevelMenuItem.Sphera OrElse Link.Category = Category Then
                            '======= end block #1 ================================================================
                            PrepareToReplace(Replaces, Link, Archive, Language, AbsoluteUrl, www, Text, TextLCase, Idx + 1)
                            Idx += Link.Text.Length - 1
                            Exit For
                          End If
                        End If
                      Next
                    End If
                  End If
                End If
              End If
            Next
          End If
          If CBool(Replaces.Count) Then
            Text = ApplyLinksToText(Text, Replaces)
          End If
        End If
      End If
    End Sub

    Enum AddWWW
      Auto
      Yes
      No
    End Enum

    Shared Sub PrepareAndCollectContextualLinks()
      Dim Links() As Link = Nothing
      Dim Languages As New Collections.Generic.Dictionary(Of LanguageManager.Language, Collections.Generic.List(Of Integer))
      For Each Language As LanguageManager.Language In LanguagesUsed
        Languages.Add(Language, New Collections.Generic.List(Of Integer))
      Next
      For Each Setup As SubSite In AllSubSite()
        If Not Setup.NotExist Then
          Dim Domain As DomainConfiguration
          If SubSiteToDomain IsNot Nothing Then
            Domain = Config.DomainConfiguration.Load(SubSiteToDomain(Setup.Name))
          Else
            Domain = Nothing
          End If
          If Domain IsNot Nothing Then
            'AddLink at Domain
            If Setup.CorrelatedWords.Status <> Config.SubSite.CerrelatedStructure.CorrelatedStatus.NotEnabled Then
              If Not Setup.CorrelatedWords.Words Is Nothing Then
                Dim Status As Config.SubSite.CerrelatedStructure.CorrelatedStatus
                If Setup.SEO.ShareCorrelatedWords Then
                  Status = Setup.CorrelatedWords.Status
                Else
                  Status = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context
                End If
                Dim NavigateUrl As String = Mid(Href(Domain, Setup.Name, True, "default.aspx"), 8)
                For Each Word As String In Setup.CorrelatedWords.Words
                  AddLinkToArray(Links, Word, NavigateUrl, 0, Setup.Description, Setup.Language, Status)
                Next
              End If
            End If
            'AddLink at all pages
            If Not Setup.Archive Is Nothing Then
              For Each Archive As Integer In Setup.Archive
                If Not Languages(Setup.Language).Contains(Archive) Then
                  Languages(Setup.Language).Add(Archive)
                  Dim ThisMenu As MenuManager.Menu = MenuManager.Menu.Load(Archive, Setup.Language)
                  If ThisMenu IsNot Nothing Then
                    Dim ItemsMenu As Collections.Generic.List(Of ItemMenu) = ThisMenu.ItemsMenu
                    If Not ItemsMenu Is Nothing Then
                      Dim Category As Integer = 0
                      For Each Item As ItemMenu In ItemsMenu
                        If CInt(Item.Level) <= 2 Then
                          Category += 1
                        End If
                        Dim Href As String = Item.Href(Domain, Setup)
                        If Not String.IsNullOrEmpty(Href) Then
                          If Not Item.Off Then
                            Dim FileName As String = MenuManager.PageNameFile(Item.Archive, Item.IdPage, Item.Language)
                            If IO.File.Exists(FileName) Then
                              Dim Document As HtmlDocument
                              Document = New HtmlDocument(FileName)
                              Dim Words() As String = SplitCorrelateWords(Document.MetaTags.Collection("CorrelatedKeyWords"))
                              If Not Words Is Nothing Then
                                Dim Status As Config.SubSite.CerrelatedStructure.CorrelatedStatus
                                Status = CType(Convert.ToInt32(Document.MetaTags.Collection("CorrelatedStatus")), Config.SubSite.CerrelatedStructure.CorrelatedStatus)
                                If Status <> Config.SubSite.CerrelatedStructure.CorrelatedStatus.NotEnabled Then
                                  If Not Setup.SEO.ShareCorrelatedWords Then
                                    Status = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context
                                  End If
                                  Dim Level As Components.LevelMenuItem
                                  If Item.Level >= LevelMenuItem.SubTopic Then
                                    Level = LevelMenuItem.Theme
                                  Else
                                    Level = LevelMenuItem.Sphera
                                  End If
                                  For Each Word As String In Words
                                    Dim Description As MenuManager.DescriptionType = Item.Description 'resolve c# issue
                                    AddLinkToArray(Links, Word, Href, Archive, Description.Title, Setup.Language, Status, Level, Category)
                                  Next
                                End If
                              End If
                            End If
                          End If
                        End If
                      Next
                    End If
                  End If
                End If
              Next
            End If
          End If
        End If
      Next

      Dim LinksLanguage As New Collections.Generic.Dictionary(Of LanguageManager.Language, Link())
      Dim LinkFinderStructuresByLanguage As New Collections.Generic.Dictionary(Of LanguageManager.Language, LinkFinderStructure)

      If Not Links Is Nothing Then
        For Each Language As LanguageManager.Language In LanguageManager.LanguagesUsed
          Dim SortCriterion As IComparer(Of Link) = New SortLinkByTextLength(Language)
          Dim LinksSorted() As Link = CType(Links.Clone(), Link())
          System.Array.Sort(LinksSorted, SortCriterion)
          Select Case Algorithm
            Case 0
              LinksLanguage.Add(Language, LinksSorted)
            Case Else
              LinkFinderStructuresByLanguage.Add(Language, New LinkFinderStructure(LinksSorted))
          End Select
        Next
        Select Case Algorithm
          Case 0
            ContextualLink.Links = LinksLanguage
            LinkFinderStructures = Nothing
          Case Else
            ContextualLink.Links = Nothing
            LinkFinderStructures = LinkFinderStructuresByLanguage
        End Select
      End If
    End Sub

    Private Shared Sub AddLinkToArray(ByRef Links() As Link, Text As String, NavigateUrl As String, Archive As Integer, Optional ToolTip As String = Nothing, Optional Language As LanguageManager.Language = LanguageManager.Language.NotDefinite, Optional Type As Config.SubSite.CerrelatedStructure.CorrelatedStatus = Config.SubSite.CerrelatedStructure.CorrelatedStatus.Context, Optional Level As Components.LevelMenuItem = LevelMenuItem.Sphera, Optional Category As Integer = 0)
      If Text IsNot Nothing Then
        Text = Text.Trim
        If Not String.IsNullOrEmpty(Text) Then
          Text = LCase(Text) 'To speedUp AddLinksToText function, all Instr instruction is in Binary mode (all text parameter must be in lcase)
          If Links Is Nothing Then
            ReDim Links(0)
          Else
            ReDim Preserve Links(Links.Length)
          End If
          Dim Link As New Link(Text, NavigateUrl, Archive, ToolTip, Language, Type, Level, Category)
          Links(UBound(Links)) = Link
        End If
      End If
    End Sub

    Private Shared Function ApplyLinksToText(ByRef Text As String, Replaces As Collections.Generic.List(Of ReplaceLink)) As String
      Dim SortCriterion As IComparer(Of ReplaceLink) = New SortReplaces
      Replaces.Sort(SortCriterion)
      'System.Array.Sort(Replaces, SortCriterion)
      Dim Result As New StringBuilder(Len(Text) * 2 + 512)
      Dim P As Integer
      Dim S As Integer = 0
      Dim IsFirst As Boolean = True
      For Each Replace As ReplaceLink In Replaces
        P = Replace.StartPosition
        If IsFirst Then
          Result.Append(Left(Text, P - 1))
          IsFirst = False
        Else
          Result.Append(Mid(Text, S, P - S))
        End If
        S = Replace.NextPosition

        Dim Link As New HyperLink
        Link.ToolTip = EncodingAttribute(Replace.Link.ToolTip)
        Link.NavigateUrl = Replace.Link.NavigateUrl
        Link.Text = Replace.Link.Text
        If Setup.SEO.TypeOfContextualLink = Configuration.SearchEngineOptimizationConfiguration.LinkType.NoFollow Then
          Link.Attributes.Add("rel", "nofollow")
        ElseIf Setup.SEO.TypeOfContextualLink = Configuration.SearchEngineOptimizationConfiguration.LinkType.Javascript Then
          Link.Attributes.Add("rel", "nofollow")
          Link.NavigateUrl = "javascript:window.location.assign('" & Link.NavigateUrl & "')"
        End If
        Result.Append(ControlToText(Link))
      Next
      Result.Append(Mid(Text, S))
      Return Result.ToString
    End Function

    Class SortReplaces
      Implements IComparer(Of ReplaceLink)
      Public Function Compare(ByVal ReplaceLink1 As ReplaceLink, ByVal ReplaceLink2 As ReplaceLink) As Integer Implements IComparer(Of ReplaceLink).Compare
        Select Case ReplaceLink1.StartPosition
          Case Is > ReplaceLink2.StartPosition
            Return 1
          Case Is < ReplaceLink2.StartPosition
            Return -1
          Case Else
            Return 0
        End Select
      End Function
    End Class

    Public Class ReplaceLink
      Public Link As Link
      Public StartPosition As Integer
      Public NextPosition As Integer
      Public Sub New(ByRef Text As String, ByRef NavigateUrl As String, Optional ByRef ToolTip As String = Nothing)
        Dim Link As Link = New Link(Text, NavigateUrl, 0, ToolTip)
        Me.Link = Link
      End Sub
    End Class

    Class SortLinkByTextLength
      Implements IComparer(Of Link)
      Public Function Compare(ByVal Link1 As Link, ByVal Link2 As Link) As Integer Implements IComparer(Of Link).Compare
        Dim X1 As Integer = Len(Link1.Text)
        Dim X2 As Integer = Len(Link2.Text)
        If X1 < X2 Then
          Return 1
        ElseIf X1 > X2 Then
          Return -1
        ElseIf Link1.Language = Link2.Language Then
          Return 0
        ElseIf Link1.Language = Language Then
          Return -1
        ElseIf Link2.Language = Language Then
          Return 1
        Else
          Return 0
        End If
      End Function
      Sub New(ByVal Language As LanguageManager.Language)
        Me.Language = Language
      End Sub
      Private Language As LanguageManager.Language
    End Class

    Private Shared Sub PrepareToReplace(Replaces As Collections.Generic.List(Of ReplaceLink), Link As Link, Archive As Integer, Language As Language, AbsoluteUrl As Boolean, www As String, Text As String, TextLcase As String, ByRef S As Integer)
      Const Cancel As Char = Chr(24)
      Dim NavigateUrl As String = Nothing

      'Finded word
      If Archive <> 0 AndAlso Link.Archive = Archive AndAlso Link.Language = Language Then
        'Is a inside link
        If AbsoluteUrl Then
          Try
            Dim Domain As String = DomaniForThisArchive(Link.Archive, Language)
            NavigateUrl = "http://" & www & Domain & Link.NavigateUrl.Substring(1)
          Catch ex As Exception
            'SubSiteToDomain collection is Nothing (Use sub UpdateSubSiteToDomain to load this collection)
          End Try
        Else
          NavigateUrl = Link.NavigateUrl
        End If
      Else
        'Is a external link
        If Not Link.NavigateUrl.First = "."c Then
          NavigateUrl = "http://" & www & Link.NavigateUrl
        Else
          Dim Domain As String = Nothing
          Try
            Domain = DomaniForThisArchive(Link.Archive, Link.Language)
          Catch ex As Exception
            'SubSiteToDomain collection is Nothing (Use sub UpdateSubSiteToDomain to load this collection)
          End Try
          If Not String.IsNullOrEmpty(Domain) Then
            If CountChar(Domain, "."c) > 1 Then
              NavigateUrl = "http://" & Domain & Link.NavigateUrl.Substring(1)
            Else
              NavigateUrl = "http://" & www & Domain & Link.NavigateUrl.Substring(1)
            End If
          Else
            NavigateUrl = Link.NavigateUrl
          End If
        End If
      End If

      'Save copy of text
      Dim TextReplaced As String = Mid(Text, S, Link.Text.Length)
      'Set "cancel" caracters
      Mid(TextLcase, S) = StrDup(Link.Text.Length, Cancel)

      'Add a replace object
      Dim Replace As New ReplaceLink(TextReplaced, NavigateUrl, Link.ToolTip)
      Replace.StartPosition = S
      S = S + Link.Text.Length
      Replace.NextPosition = S
      Replaces.Add(Replace)

    End Sub

    Class LinkFinderStructure
      Public Letters As New Collections.Generic.Dictionary(Of Char, CharStructure)
      Class CharStructure
        'Public Letter As Char
        Public Find As New LinkFinderStructure
        Public Links As Collections.Generic.List(Of Link)
      End Class
      Function ContainWord(Text As String, Position As Integer) As Collections.Generic.List(Of Link)
        Dim C As Char
        If Position < Text.Length Then
          C = Text(Position)
        Else
          Return Nothing
        End If
        Dim L As CharStructure
        If Letters.ContainsKey(C) Then
          L = Letters(C)
          If L.Links IsNot Nothing Then
            Dim HexFirstChar As Integer = AscW(L.Links(0).Text)
            'True is the first char is in ideogram Alphabet
            Dim Chinese As Boolean = HexFirstChar >= &H2E80 AndAlso HexFirstChar <= &HFA6A
            If Chinese OrElse Position = Text.Length - 1 OrElse Not Char.IsLetterOrDigit(Text(Position + 1)) Then
              'Verify if exists e word/phrase more long
              Dim Result As Collections.Generic.List(Of Link)
              Result = L.Find.ContainWord(Text, Position + 1)
              If Result IsNot Nothing Then
                Return Result 'Exists a word/phrase more long 
              Else
                Return L.Links 'Is the longer word/phrase
              End If
            End If
          Else
            Return L.Find.ContainWord(Text, Position + 1)
          End If
        End If
        Return Nothing
      End Function
      Sub PopulateLink(Link As Link, Level As Integer)
        If Level < Link.Text.Length Then
          Dim C As Char = Link.Text(Level)
          'NewList.Add(Link)
          Dim L As CharStructure
          If Not Me.Letters.ContainsKey(C) Then
            L = New CharStructure
            Me.Letters.Add(C, L)
          Else
            L = Me.Letters(C)
          End If

          If Level = Link.Text.Length - 1 Then
            If L.Links Is Nothing Then
              L.Links = New Collections.Generic.List(Of Link)
            End If
            L.Links.Add(Link)
          Else
            L.Find.PopulateLink(Link, Level + 1)
          End If
        End If
      End Sub

      Sub New(Links() As Link)
        For Each Link As Link In Links
          PopulateLink(Link, 0)
        Next
      End Sub
      Sub New()
      End Sub
    End Class

  End Class

End Namespace
