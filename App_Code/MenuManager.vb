'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Imports Microsoft.VisualBasic

Namespace WebApplication
	Public Module MenuManager
		Class Menu
      Private Shared Menus As New Extension.Cache(AddressOf Load, 100, GetType(Menu))
			Public Archive As Integer
			Public Language As LanguageManager.Language
      Function Name(Optional ShortFormat As Boolean = False) As String
        Dim Text As String = Archive & ": "
        If CBool(ShortFormat) Then
          If CBool(ItemsMenu.Count) Then
            Dim Title As String = ItemsMenu(0).Description.Title
            Dim Label As String = ItemsMenu(0).Description.Label
            If Not String.IsNullOrEmpty(Label) Then
              Text &= Label
            End If
            If Not String.IsNullOrEmpty(Title) Then
              If Not String.IsNullOrEmpty(Label) Then
                Text &= " (" & Title & ")"
              Else
                Text &= Title
              End If
            End If
            Text = TruncateText(Text, 60)
          End If
        Else
          For Each ItemMenu As ItemMenu In ItemsMenu
            If ItemMenu.Level = LevelMenuItem.Sphera Then
              Text &= "\" & ItemMenu.Description.Label
            End If
          Next
        End If
        Return Text
      End Function
      Function Exists() As Boolean
        Return System.IO.File.Exists(File)
      End Function
			Sub Create()
				WriteAll("", Me.File)
			End Sub
			Shared Function File(ByVal Archive As Integer, ByVal Language As LanguageManager.Language) As String
				Return MapPath(ArchiveSubDirectory & "/" & Archive & "/" & 1 & ".menu." & Acronym(Language) & ".txt")
			End Function
			Function File() As String
				Return File(Me.Archive, Me.Language)
			End Function
			Sub New(ByVal Archive As Integer, ByVal Language As LanguageManager.Language)
				Me.Archive = Archive
				Me.Language = Language
			End Sub
			Public Sub UpdateItemsMenu()
				'Add new page to ItemsMenu and remove not exists pages
				Dim Dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo(DirectoryName(Archive))
				If Dir.Exists Then

					Dim ItemsMenu As New ItemMenuCollection

					Dim Records As Array = Split(ReadAll(File(Archive, Language)), vbCrLf)
					'Add items of menu and removed record of not exists pages
					For Each Record As String In Records
            If Not String.IsNullOrEmpty(Record) Then
              Dim Item As MenuManager.ItemMenu = MenuManager.ItemMenu.Load(Record, Archive, Language)
              If Item IsNot Nothing Then
                'The page of item exists
                ItemsMenu.Add(Item)
              End If
            End If
					Next
					'Find new page added and add new page to menu
					Dim Find As String = "*." & Acronym(Me.Language) & ".htm"
					Dim Files As System.IO.FileInfo() = Dir.GetFiles(Find)
					If Not Files Is Nothing Then
            For Each EachFile As System.IO.FileInfo In Files
              'aggiungi le voci che mancano
              Dim Page As Integer = CInt(Extension.Left(EachFile.Name, InStr(EachFile.Name, ".") - 1))
              Dim Item As ItemMenu
              Item = ItemsMenu.Page(Page)
              If Item Is Nothing Then
                Dim Code As String = FileManager.ReadAll(EachFile.FullName)
                Dim MetaTags As MetaTags = New MetaTags(Code)
                If Item Is Nothing Then
                  Item = New ItemMenu
                  Item.IdPage = Page
                  Item.Off = True
                  Item.Level = CType(2, LevelMenuItem)
                  Item.Language = Language
                End If
                Item.Description = New DescriptionType(MetaTags.Title, MetaTags.Description)
                ItemsMenu.Add(Item)
              End If
            Next
					End If

					Me.ItemsMenu = ItemsMenu
					Pipeline.NotifyChangement(GetType(Menu), Archive & " " & Language)

				End If
			End Sub

      Sub LevelAndCategoryItem(Page As Integer, ByRef Level As Components.LevelMenuItem, ByRef Category As Integer)
        For Each Item As ItemMenu In ItemsMenu
          If CInt(Item.Level) <= 2 Then
            Category += 1
          End If
          If Item.IdPage = Page Then
            If CInt(Item.Level) >= 3 Then
              Level = LevelMenuItem.Theme
            Else
              Level = LevelMenuItem.Sphera
            End If
            Exit For
          End If
        Next
      End Sub

      Public ItemsMenu As ItemMenuCollection = New ItemMenuCollection

			Class ItemMenuCollection
				Inherits Collections.Generic.List(Of ItemMenu)
				Function Page(ByVal IdPage As Integer) As ItemMenu
					For Each Item As ItemMenu In Me
						If Item.IdPage = IdPage Then
							Return Item
						End If
					Next
          Return Nothing
        End Function
			End Class

			Private Shared Function Load(ByVal Key As String) As Menu
				Dim Params As String() = Split(Key)
				Dim Menu As Menu
        Menu = New Menu(CInt(Params(0)), CType(CInt(Params(1)), LanguageManager.Language))
				Menu.UpdateItemsMenu()
				Return Menu
			End Function

      Shared Function Load(Archive As Integer, Language As LanguageManager.Language) As Menu
        Dim Key As String = Archive & " " & Language
        Return CType(Menus.GetItem(Key), Menu)
      End Function

      Function Trace(IdPage As Integer, DomainConfiguration As DomainConfiguration, Setting As SubSite, Optional ReturnMetaTags As MetaTags = Nothing, Optional ByRef ReturnSubject As String = Nothing) As Control()
        'Dim CtrlCollection As New Collections.Generic.List(Of Control)
        Dim Id As Integer = IndexItem(IdPage)
        Dim Level As Components.LevelMenuItem = ItemsMenu(Id).Level
        Dim ObjectTrace As New Trace
        For N As Integer = Id To 0 Step -1
          If N > 0 Then
            If ItemsMenu(N - 1).Level < Level Then
              Level = ItemsMenu(N - 1).Level
              If ItemsMenu(N - 1).Level < ItemsMenu(N).Level Then
                Dim ItemMenu As ItemMenu = ItemsMenu(N - 1)
                ObjectTrace.AddElement(ItemMenu.Description.Label, ItemMenu.Description.Title, ItemMenu.Href(DomainConfiguration, Setting))
                If ReturnMetaTags IsNot Nothing Then
                  Dim Title, Description As String
                  Title = ItemMenu.Description.Label
                  Description = ItemMenu.Description.Title
                  If Not String.IsNullOrEmpty(Title) Then
                    If Not String.IsNullOrEmpty(ReturnSubject) Then
                      ReturnSubject &= ","
                    End If
                    ReturnSubject &= Title
                  End If

                  If Not String.IsNullOrEmpty(Description) Then
                    If Not String.IsNullOrEmpty(ReturnSubject) Then
                      ReturnSubject &= ","
                    End If
                    ReturnSubject &= Description
                  End If

                  If String.IsNullOrEmpty(Description) Then
                    Description = Title
                  End If
                  ReturnMetaTags.Title = Title & IfStr(Not String.IsNullOrEmpty(ReturnMetaTags.Title), ":", "") & ReturnMetaTags.Title
                  ReturnMetaTags.Description = Description & IfStr(Not String.IsNullOrEmpty(ReturnMetaTags.Description), ":", "") & ReturnMetaTags.Description

                  If Not String.IsNullOrEmpty(ReturnMetaTags.KeyWords) Then
                    ReturnMetaTags.KeyWords &= ","
                  End If
                  ReturnMetaTags.KeyWords &= ItemMenu.Description.Label
                  If Not String.IsNullOrEmpty(ItemMenu.Description.Title) Then
                    ReturnMetaTags.KeyWords &= "," & ReturnMetaTags.Title
                  End If
                End If
              End If
            End If
          End If
        Next

        Return ObjectTrace.Controls(True)
      End Function

      Function Control(DomainConfiguration As DomainConfiguration, Setting As Config.SubSite, Optional ByVal RestrictToIdPage As Integer = 0) As Control
        'If Not Menu Is Nothing Then
        Dim CtrlMenu As New Control

        Dim Language As LanguageManager.Language = Setting.Language
        If Not Me.ItemsMenu.Count = 0 Then
          Dim FirstIdItem As Integer = IndexItemParent(RestrictToIdPage)
          Dim FirstLevel As Components.LevelMenuItem = Me.ItemsMenu(FirstIdItem).Level
 
          Dim MainBox As UI.ControlCollection = Nothing
          For Id As Integer = FirstIdItem To Me.ItemsMenu.Count - 1
            'For Each Item As ItemMenu In Me.ItemsMenu
            Dim Item As ItemMenu = Me.ItemsMenu(Id)
            If Item.Off = False AndAlso Item.JoinPrevious = False Then
              If RestrictToIdPage <> 0 AndAlso Id <> FirstIdItem AndAlso Item.Level <= FirstLevel Then
                Exit For
              End If
              If MainBox Is Nothing OrElse Item.Level = LevelMenuItem.Sphera Then
                Dim TopLevel As New HtmlGenericControl("nav") 'html5
                CtrlMenu.Controls.Add(TopLevel)
                TopLevel.Attributes.Add("class", "Menu")
                MainBox = TopLevel.Controls
                MainBox.Add(Item.Control(DomainConfiguration, Setting))
              Else
                MainBox.Add(Item.Control(DomainConfiguration, Setting))
              End If
            End If
          Next
        End If
        'AbjustUlOlLevel(CtrlMenu)
        Return CtrlMenu
        'End If
      End Function

			Private Sub AssignCtrlLevel(ByVal Levels As Collections.Generic.Dictionary(Of Integer, Control), ByVal Level As Control, ByVal Key As Integer)
				If Levels.ContainsKey(Key) Then
					Levels.Remove(Key)
				End If
				Levels.Add(Key, Level)
			End Sub

      Private Sub AbjustUlOlLevel(Control As Control)
        Dim Removed As Boolean
        Do
          Removed = False
          If Control IsNot Nothing Then
            For Each Ctrl As HtmlGenericControl In Control.Controls
              If (StrComp(Ctrl.TagName, "ul") = 0 OrElse StrComp(Ctrl.TagName, "ol") = 0) AndAlso LiFinded(Ctrl) = False Then
                'Remove This Ul-Ol control
                If Ctrl.Parent IsNot Nothing Then
                  For Each InsideControl As Control In Ctrl.Controls
                    Ctrl.Parent.Controls.Add(InsideControl)
                  Next
                  Ctrl.Parent.Controls.Remove(Ctrl)
                  Ctrl.Dispose()
                  Removed = True
                  Exit For
                End If
              Else
                If StrComp(Ctrl.TagName, "li") <> 0 AndAlso StrComp(Ctrl.TagName, "legend") <> 0 Then
                  AbjustUlOlLevel(CType(Ctrl, Control))
                End If
              End If
            Next
          End If
        Loop Until Not Removed
      End Sub

			Private Function LiFinded(ByRef Control As HtmlGenericControl) As Boolean
				For Each Ctrl As HtmlGenericControl In Control.Controls
					If StrComp(Ctrl.TagName, "li") = 0 Then
						Return True
					End If
				Next
				Return False
			End Function

			Private Function IndexItem(ByVal IdPage As Integer) As Integer
        Dim Id As Integer = 0
				For Each Item As ItemMenu In Me.ItemsMenu
					If Item.IdPage = IdPage Then
						Return Id
					End If
					Id += 1
        Next
        Return 0
			End Function

			Private Function IndexItemParent(ByVal IdPage As Integer) As Integer
        If CBool(IdPage) Then
          Dim Level As Components.LevelMenuItem = LevelMenuItem.Sphera
          For Each Item As ItemMenu In Me.ItemsMenu
            If Item.IdPage = IdPage Then
              Level = Item.Level
              Exit For
            End If
          Next
          Dim Id As Integer = 0, ReturnId As Integer = 0
          For Each Item As ItemMenu In Me.ItemsMenu
            If Item.IdPage = IdPage Then
              If Level = LevelMenuItem.Sphera Then
                Return Id
              Else
                Return ReturnId
              End If
            End If
            If Item.Off = False AndAlso Item.Level < Level Then
              ReturnId = Id
            End If
            Id += 1
          Next
        End If
        Return 0
      End Function

		End Class
    Public Function MenuControls(ByVal DomainConfiguration As DomainConfiguration, ByRef Setting As SubSite) As System.Web.UI.Control
      Dim Control As New Control
      If Setting.Archive IsNot Nothing Then
        For Each Archive As Integer In Setting.Archive
          Dim Menu As MenuManager.Menu = MenuManager.Menu.Load(Archive, Setting.Language)
          If Menu IsNot Nothing Then
            Control.Controls.Add(Menu.Control(DomainConfiguration, Setting))
          End If
        Next
      End If
      Return Control
    End Function
		Class DescriptionType
			Public Label As String
			Public Title As String
			Public Link As String
			Public Sub New()
			End Sub
			Public Sub New(ByVal Label As String, ByVal Title As String, Optional ByVal Link As String = Nothing)
				Me.Label = Label
				Me.Title = Title
				Me.Link = Link
			End Sub
		End Class

    Public Class ItemMenu
      Public Archive As Integer
      Public IdPage As Integer
      Public Level As LevelMenuItem
      Public Description As New DescriptionType
      Public Off As Boolean  'Disabled
      Public JoinPrevious As Boolean
      Public Language As LanguageManager.Language
      Shared Function Href(ByVal Setting As SubSite, ByVal Page As Integer, ByVal Archive As Integer, ByVal Title As String, Optional ByVal Absolute As Boolean = False, Optional ByVal DomainConfiguration As DomainConfiguration = Nothing) As String
        Return Common.Href(DomainConfiguration, Setting.Name, Absolute, "default.aspx", QueryKey.ArticleNumber, Page, QueryKey.ArchiveNumber, Archive)
      End Function
      Function Href(ByVal DomainConfiguration As DomainConfiguration, ByVal Setting As SubSite, Optional ByVal Absolute As Boolean = False) As String
        If CBool(IdPage) Then
          Dim Title As String = Nothing
          If Description IsNot Nothing Then
            Title = Description.Label
          End If
          Return Href(Setting, IdPage, Archive, Title, Absolute, DomainConfiguration)
        Else
          Return Description.Link
        End If
      End Function

      Function Control(ByVal DomainConfiguration As DomainConfiguration, ByVal Setting As SubSite) As Control
        If Not Me.Off Then
          Dim ToolTip As String = Nothing, Text As String = Nothing
          If Not Me.Description Is Nothing Then
            ToolTip = Me.Description.Title
            Text = Me.Description.Label
          End If
          Return ControlItem(Me.Href(DomainConfiguration, Setting), Text, ToolTip, Me.Level)
        End If
        Return Nothing
      End Function

      Function Record() As String
        Dim Level As String = Nothing, ID As String = Nothing, Label As String = Nothing, Title As String = Nothing, Link As String = Nothing
        Dim Parameters As String = Nothing
        Level = StrDup(CInt(Me.Level), ".")
        If CBool(Me.IdPage) Then
          ID = "[#" & Me.IdPage & "]"
        Else
          ID = "[]"
          Label = "[" & Me.Description.Label & "]"
          Title = "[" & Me.Description.Title & "]"
          If Not String.IsNullOrEmpty(Me.Description.Link) Then
            Link = "[" & Me.Description.Link & "]"
          End If
        End If
        If Me.Off Then
          Parameters &= "{off}"
        End If
        If Me.JoinPrevious Then
          Parameters &= "{join}"
        End If
        Return Level & ID & Label & Title & Link & Parameters
      End Function
      Public Sub New()
      End Sub
      Shared Function Load(ByVal Record As String, ByVal Archive As Integer, ByVal Language As LanguageManager.Language) As ItemMenu
        Dim MewItem As New ItemMenu
        MewItem.Language = Language
        MewItem.Archive = Archive

        Dim p1 As Integer, p2 As Integer = 0
        If Not String.IsNullOrEmpty(Record) Then
          For n As Integer = 1 To Len(Record)
            If Mid(Record, n, 1) = "." Then
              MewItem.Level = CType(MewItem.Level + 1, LevelMenuItem)
            Else
              Exit For
            End If
          Next
          p1 = InStr(1, Record, "[", vbTextCompare)
          If CBool(p1) Then
            p2 = InStr(p1 + 1, Record, "]", vbTextCompare)
            Dim Field As String = Mid(Record, p1 + 1, p2 - p1 - 1)
            If Field.StartsWith("#") Then
              MewItem.IdPage = CInt(Mid(Field, 2))
            End If
            If CBool(MewItem.IdPage) Then
              MewItem.Description = LoadDescription(Archive, MewItem.IdPage, Language)
              If MewItem.Description Is Nothing Then
                'The corrispondent page is deleted
                Return Nothing
              End If
            Else
              p1 = InStr(p2, Record, "[", vbTextCompare)
              If CBool(p1) Then
                MewItem.Description = New DescriptionType
                p2 = InStr(p1 + 1, Record, "]", vbTextCompare)
                MewItem.Description.Label = Mid(Record, p1 + 1, p2 - p1 - 1)
                p1 = InStr(p2, Record, "[", vbTextCompare)
                If CBool(p1) Then
                  p2 = InStr(p1 + 1, Record, "]", vbTextCompare)
                  MewItem.Description.Title = Mid(Record, p1 + 1, p2 - p1 - 1)
                  p1 = InStr(p2, Record, "[", vbTextCompare)
                  If CBool(p1) Then
                    p2 = InStr(p1 + 1, Record, "]", vbTextCompare)
                    MewItem.Description.Link = Mid(Record, p1 + 1, p2 - p1 - 1)
                  End If
                End If
              End If
            End If
          End If
          MewItem.Off = InStr(p2, Record, "{off}", vbTextCompare) <> 0
          MewItem.JoinPrevious = InStr(p2, Record, "{join}", vbTextCompare) <> 0
        End If
        Return MewItem
      End Function
    End Class
		Private Function LoadDescription(ByVal Archive As Integer, ByVal IDPage As Integer, ByVal Language As LanguageManager.Language) As DescriptionType
			Dim Key As String = Archive & "," & IDPage & "," & Language
			SyncLock PagesDescriptions
				Dim FileInfo As New System.IO.FileInfo(MenuManager.PageNameFile(Archive, IDPage, Language))
        If Recent(FileInfo.LastWriteTimeUtc, Now.ToUniversalTime(), New TimeSpan(0, 0, 30)) OrElse Not PagesDescriptions.ContainsKey(Key) Then
          Return SetDescription(Archive, IDPage, Language)
        Else
          Return PagesDescriptions(Key)
        End If
			End SyncLock
		End Function
		Function SetDescription(ByVal Archive As Integer, ByVal IDPage As Integer, ByVal Language As LanguageManager.Language, Optional ByVal Description As DescriptionType = Nothing) As DescriptionType
			If Description Is Nothing Then
				Dim Html As String = ReadAll(MenuManager.PageNameFile(Archive, IDPage, Language))
				If Html = Nothing Then
					Return Nothing
				End If
        Dim MetaTags As MetaTags = New MetaTags(Html)
				Description = New DescriptionType(MetaTags.Title, MetaTags.Description)
			End If
			Dim Key As String = Archive & "," & IDPage & "," & Language
			SyncLock PagesDescriptions
				If PagesDescriptions.ContainsKey(Key) Then
					PagesDescriptions.Remove(Key)
				End If
				PagesDescriptions.Add(Key, Description)
			End SyncLock
			Return Description
		End Function
		Function UnSetDescription(ByVal Archive As Integer, ByVal IDPage As Integer, ByVal Language As LanguageManager.Language) As Boolean
			Dim Key As String = Archive & "," & IDPage & "," & Language
			SyncLock PagesDescriptions
				If PagesDescriptions.ContainsKey(Key) Then
					PagesDescriptions.Remove(Key)
					Return True
				Else
					Return False
				End If
			End SyncLock
		End Function
		Private PagesDescriptions As New Collections.Generic.Dictionary(Of String, DescriptionType)
		Public Function PageNameFile(ByVal Archive As Integer, ByVal IDPage As Integer, ByVal Language As LanguageManager.Language) As String
			Return MapPath(ArchiveSubDirectory & "/" & Archive & "/" & IDPage & "." & Acronym(Language) & ".htm")
		End Function
		Public Function DirectoryName(ByVal Archive As Integer) As String
			Return MapPath(ArchiveSubDirectory & "/" & Archive)
		End Function
		Public Function AllArchives(Optional ByVal WithoutForSystemUse As Boolean = True) As Archive()
			Dim Root As String = FileManager.MapPath(ArchiveSubDirectory)
			Dim dir As New System.IO.DirectoryInfo(Root)
			Dim Folders As System.IO.FileSystemInfo() = dir.GetFileSystemInfos()
			Dim Archives(UBound(Folders)) As Archive
      Dim N As Integer = 0
			For Each Folder As System.IO.FileSystemInfo In Folders
				Try
					If WithoutForSystemUse = False OrElse Folder.Name <> "0" Then
            Dim Archive As New Archive(CInt(Folder.Name))
						Archives(N) = Archive
						N += 1
					End If
				Catch ex As Exception
					'directoty is not an Archive!
				End Try
			Next
			ReDim Preserve Archives(N - 1)
			Return Archives
		End Function
    Function NewArchive(Optional ByVal ID As Integer = 0) As Archive
      If ID = 0 Then ID = NewSubDirectoryId(FileManager.MapPath(ArchiveSubDirectory))
      Dim Archive As New Archive(ID)
      Return Archive
    End Function
		Class Archive
			Public ID As Integer
      Public Function Name(Language As LanguageManager.Language, Optional ShortFormat As Boolean = False) As String
        Dim Menu As MenuManager.Menu = MenuManager.Menu.Load(ID, Language)
        Return Menu.Name(ShortFormat)
      End Function

			Sub Load(ByVal ID As Integer)
				Me.ID = ID
			End Sub

			Shared Sub Create(ByVal ID As Integer)
				Dim Dir As New System.IO.DirectoryInfo(MenuManager.DirectoryName(ID))
				Try
					Dir.Create()
				Catch ex As Exception
				End Try
			End Sub

			Shared Function Exist(ByVal ID As Integer) As Boolean
				Dim Dir As New System.IO.DirectoryInfo(MenuManager.DirectoryName(ID))
				Return Dir.Exists
			End Function


			Public Sub New(ByVal ID As Integer)
				Load(ID)
			End Sub

			Private Function DirectoryName() As String
				Return MenuManager.DirectoryName(ID)
			End Function
		End Class

		Public Function FirstDocument(ByVal Setting As SubSite, ByRef ArchiveNumber As Integer) As Integer
			If Setting.Archive IsNot Nothing Then
				For Each Archive As Integer In Setting.Archive
          Dim Menu As MenuManager.Menu = MenuManager.Menu.Load(Archive, Setting.Language)
					If Not Menu Is Nothing Then
						ArchiveNumber = Archive
						If Menu.ItemsMenu IsNot Nothing Then
							For Each ItemMenu As MenuManager.ItemMenu In Menu.ItemsMenu
                If CBool(ItemMenu.IdPage) Then
                  Return ItemMenu.IdPage
                End If
							Next
						End If
					End If
				Next
      End If
      Return 0
		End Function

		Public Function FindMenu(ByVal Setting As SubSite, ByVal ArchiveNumber As Integer) As Menu
			If Setting.Archive IsNot Nothing Then
				For Each Archive As Integer In Setting.Archive
					If Archive = ArchiveNumber Then
            Return MenuManager.Menu.Load(Archive, Setting.Language)
					End If
				Next
			End If
      Return Nothing
		End Function

    Public Sub InsertPageContent(Content As Control, Master As Components.MasterPageEnhanced, Setting As SubSite, Menu As Menu, PageNumber As Integer, Optional IsHomePage As Boolean = True)
      Dim ReturnMetaTags As New Collections.Generic.List(Of MetaTags)
      If Not IsHomePage AndAlso Menu.Archive <> 0 Then
        Dim Subject As String = Nothing
        Master.SetTrace(Menu.Trace(PageNumber, CurrentDomainConfiguration, Setting, Nothing, Subject))

        'ReturnMetaTags.Title = "" 'Remove full track in title [REM this line for full title]

        If Not String.IsNullOrEmpty(Subject) Then
          Master.AddMetaTag("Subject", Subject)
        End If
      End If

      Dim MetaTags As Common.MetaTags = New Common.MetaTags
      ReturnMetaTags.Add(MetaTags)

      'Find Level and Category
      Dim Category As Integer = 0
      Dim Level As Components.LevelMenuItem = LevelMenuItem.Sphera
      If Menu.ItemsMenu IsNot Nothing Then
        Menu.LevelAndCategoryItem(PageNumber, Level, Category)
      End If

      Components.AddPageArchived(Content, Master, Menu.Archive, PageNumber, HttpContext.Current, CurrentDomainConfiguration, Setting, MetaTags, IsHomePage, True, True, Level, Category)

      'Add Pages is next pages is join to the previous
      Dim FlgFinded As Boolean = False

      For Each ItemMenu As MenuManager.ItemMenu In Menu.ItemsMenu
        If ItemMenu.IdPage = PageNumber Then
          FlgFinded = True
        ElseIf FlgFinded Then
          If ItemMenu.JoinPrevious Then
            If ItemMenu.Off = False Then
              Dim Hr As New WebControl(HtmlTextWriterTag.Hr)
              Content.Controls.Add(Hr)
              MetaTags = New Common.MetaTags
              ReturnMetaTags.Add(MetaTags)
              Components.AddPageArchived(Content, Master, Menu.Archive, ItemMenu.IdPage, HttpContext.Current, CurrentDomainConfiguration, Setting, MetaTags, IsHomePage, True, True, Level, Category)
            End If
          Else
            FlgFinded = False
            Exit For
          End If
        End If
      Next

      If Not IsHomePage Then
        'Set meta tag for a specific page
        If Menu.Archive > 1 Then
          'Joun all metatags
          Dim MetaTag As MetaTags = Nothing
          Dim Title As String = Nothing
          For Each EahcMetaTag As MetaTags In ReturnMetaTags
            If MetaTag Is Nothing Then
              MetaTag = EahcMetaTag
              Title = MetaTag.Title
            Else
              MetaTag.Join(EahcMetaTag)
            End If
          Next
          Master.TitleDocument = Title ' MetaTag.Title
          If MetaTag IsNot Nothing Then
            Master.Description = MetaTag.Description
            Master.KeyWords = MetaTag.KeyWords
          End If
          'Add menu restrict of the context
          Dim Fieldset As Control = Components.Fieldset(Phrase(Setting.Language, 3240), Menu.Control(CurrentDomainConfiguration, Setting, PageNumber))
          Content.Controls.Add(Fieldset)
        End If
      End If
    End Sub
  End Module
End Namespace