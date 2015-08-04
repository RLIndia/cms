'Plugin: Unplug = Remove this file
'By Andrea Bruno
Namespace WebApplication.Plugin		'Standard namespace obbligatory for all plugins
  Public Class FeedRss2Forum
    Public Shared WithEvents Plugin As PluginManager.Plugin = Initialize()
    Shared Function Initialize() As PluginManager.Plugin
      If Plugin Is Nothing AndAlso Not CurrentPluginRunning() Then
        Plugin = New PluginManager.Plugin(AddressOf Description, , , , PluginManager.Plugin.Characteristics.StandardPlugin, GetType(FeedsConfiguration), , , 60000 * 20)
      End If
      Return Plugin
    End Function
    Shared Sub New()
      Initialize()
    End Sub

    Private Shared Function Description(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
      Select Case Language
        Case LanguageManager.Language.Italian
          If ShortDescription Then
            Return "Feed nel forum"
          Else
            Return "Pubblica e tieni sincronizzati i feed RSS nel forum"
          End If
        Case Else
          If ShortDescription Then
            Return "Feeds on the Forum"
          Else
            Return "Publish RSS feeds and keep synchronized on the forum"
          End If
      End Select
    End Function

    Private Shared Sub Plugin_TimerElapsed(ByRef SetNextElapsedInterval As Integer, ByRef SetTimerEnabled As Boolean) Handles Plugin.TimerElapsed
      For Each SubSite As SubSite In AllSubSite()
        LoadSource(SubSite)
      Next
      SetNextElapsedInterval = 60000 * 20 'Next update after 20 min 
    End Sub

    Class FeedsConfiguration
      Public FeedsSources(-1) As FeedRssConfiguration
    End Class

    Class FeedRssConfiguration
      Public UrlOfSource As String
      Public AddLink As Boolean
    End Class

    Class FeedDataPlugin
      Public Data(-1) As DataFeedRss
    End Class

    Class DataFeedRss
      Public UrlOfSource As String
      Public ForumId As Integer
      Public SubCategory As Integer
      Public Last As Date
    End Class

    Private Shared Sub Plugin_AfterSavePluginConfiguration(Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean) Handles Plugin.AfterSavePluginConfiguration
      RunLoadSource(CurrentSetting)
    End Sub

    Shared Sub RunLoadSource(Setting As SubSite)
      Dim NewThread As System.Threading.Thread = New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf LoadSource))
      NewThread.IsBackground = True
      NewThread.Start(Setting)
    End Sub

    Shared Sub LoadSource(Setting As Object)
      Dim SubSite As Config.SubSite = CType(Setting, Config.SubSite)
      Static Running As Boolean
      If Not Running Then
        Running = True
        Try
          If Plugin.IsEnabled(SubSite) Then
            SyncLock Plugin
              Dim Config As FeedsConfiguration
              Config = CType(Plugin.LoadObject(GetType(FeedsConfiguration), SubSite.Name), FeedsConfiguration)
              If Config.FeedsSources.Length <> 0 Then
                Dim Data As FeedDataPlugin = CType(Plugin.LoadObject(GetType(FeedDataPlugin), SubSite.Name), FeedDataPlugin)
                Dim SaveObject As Boolean = False

                For Each FeedsSource As FeedRssConfiguration In Config.FeedsSources
                  ImportFromFeedRss(FeedsSource, SubSite, SaveObject, Data)
                Next

                If SaveObject Then
                  Plugin.SaveObject(Data, SubSite.Name)
                End If
              End If
            End SyncLock
          End If
        Catch ex As Exception
          Extension.Log("WebApplication", 100, "ERROR!", ex.Message, ex.Source, ex.StackTrace)
        End Try
        Running = False
      End If
    End Sub

    Shared Sub ImportFromFeedRss(FeedRssConfig As FeedRssConfiguration, SubSite As SubSite, ByRef SaveObject As Boolean, Data As FeedDataPlugin)
      Dim UrlOfSource As String = FeedRssConfig.UrlOfSource
      If Not String.IsNullOrEmpty(UrlOfSource) Then
        Dim DataFeedRss As DataFeedRss = Nothing
        Dim ChannelFinded As Boolean = False
        For Each DataFeedRss In Data.Data
          If DataFeedRss.UrlOfSource = UrlOfSource Then
            ChannelFinded = True
            Exit For
          End If
        Next
        If Not ChannelFinded Then
          DataFeedRss = Nothing
        End If

        If DataFeedRss Is Nothing Then
          DataFeedRss = New DataFeedRss
          DataFeedRss.UrlOfSource = UrlOfSource
          ReDim Preserve Data.Data(Data.Data.Length)
          Data.Data(UBound(Data.Data)) = DataFeedRss
          SaveObject = True
        End If

        Dim Feeds As New Collections.Generic.List(Of NewsManager.Notice)
        Dim Request As New NewsRequire
        Request.AddAllRecords = True
        Request.XmlHref = UrlOfSource
        Dim FeedTitle As String = Nothing
        Dim FeedDescription As String = Nothing
        ReadFeed(Feeds, Request, , , FeedTitle, FeedDescription)

        If Feeds.Count <> 0 Then
          If SubSite.Forums IsNot Nothing AndAlso SubSite.Forums.Length <> 0 Then
            If DataFeedRss.ForumId <> SubSite.Forums(0) Then
              SaveObject = True
              DataFeedRss.ForumId = SubSite.Forums(0)
              'Add subcategory to forum
              Dim Forum As ForumManager.Forum = CType(ForumManager.Forum.Load.GetItem(DataFeedRss.ForumId), ForumManager.Forum)
              If Forum.ForumStructure IsNot Nothing AndAlso Forum.ForumStructure.Categories.Count > 0 Then
                Dim TitleCategory As String = Description(SubSite.Language, True)
                Dim Category As ForumStructure.Category = Nothing
                For Each ForumCategory As ForumStructure.Category In Forum.ForumStructure.Categories
                  If ForumCategory.Title = TitleCategory Then
                    Category = ForumCategory
                    Exit For
                  End If
                Next
                If Category Is Nothing Then
                  Category = Forum.ForumStructure.AddCategory(TitleCategory)
                End If
                DataFeedRss.SubCategory = Forum.ForumStructure.LastId() + 1
                Category.AddSubcategory(FeedTitle, FeedDescription, DataFeedRss.SubCategory)
                Forum.ForumStructure.Save()
              End If
            End If

            'Add all feed
            For N = Feeds.Count - 1 To 0 Step -1
              Dim Feed As Notice
              Feed = Feeds.Item(N)
              If Feed.pubDate > DataFeedRss.Last Then
                DataFeedRss.Last = Feed.pubDate
                AddPost(SubSite, Feed, DataFeedRss.ForumId, DataFeedRss.SubCategory, FeedTitle, FeedRssConfig.AddLink)
                SaveObject = True
              End If
            Next
          End If
        End If
      End If
    End Sub

    Shared Function RetrivePlayListId(VideoChannel As String) As String
      Dim Html As String = ReadHtmlFromWeb("http://www.youtube.com/user/" & VideoChannel, Nothing, 30000)
      Return ExtrapolateTextBetween(Html, ";list=", "&")
    End Function

    Shared Sub AddPost(SubSite As SubSite, Feed As Notice, ForumId As Integer, CategoryId As Integer, Author As String, AddLink As Boolean)
      If Feed.Author <> "" Then
        Author = Feed.Author
      End If
      Author = FirstUpper(Author)

      Dim Title As String = Trim(Replace(Inner(RemoveCDATA(Feed.Title)), vbCr, ". "))
      If Title = "" Then
        Title = Author
      End If
      Title = TruncateText(Title, 60)

      Dim Text As String
      Text = Trim(Inner(RemoveCDATA(Feed.Description)))
      If AddLink Then
        Text &= vbCrLf & Feed.Link
      End If
      Text = Normalize(Text)
      Text = HttpUtility.HtmlEncode(Text)
      Text = ReplaceBin(Text, vbCr, "<br />")

      Dim Video As String = Nothing
      If Feed.Video <> "" Then
        Video = Feed.Video
      Else
        Dim VideoId As String = ExtrapolateVideoID(Feed.Link)
        If VideoId <> "" AndAlso Not VideoId.Contains("//") Then
          Video = VideoId
        ElseIf IsMediaSource(Feed.Link) Then
          Video = Feed.Video
        End If
      End If
      If Video = "" Then
        Video = ExtrapolateVideoSource(Feed.Description)
      End If

      Dim PhotoID As String = Nothing
      If Video = "" Then 'Or video or image
        Dim ImageUrl As String
        If Feed.Image <> "" Then
          ImageUrl = Feed.Image
        Else
          ImageUrl = ExtrapolateImg(Feed.Description)
        End If
        If ImageUrl <> "" Then
          If ImageUrl.Contains(".akamaihd.net/") Then
            'Take a big foto from facebook archive
            ImageUrl = ImageUrl.Replace("_s", "_n")
          End If
          Dim Photo As New PhotoManager.Photo
          Dim Forum As Forum = CType(ForumManager.Forum.Load.GetItem(ForumId), ForumManager.Forum)
          Photo.Album = Forum.PhotoAlbum
          If Not String.IsNullOrEmpty(Photo.Album) Then
            Photo.FromUrl(ImageUrl)
            Photo.Title(SubSite.Language) = Author
            Photo.Description(SubSite.Language) = Title
            Photo.Save()
            PhotoID = Photo.NameCode()
            Photo.Dispose()
          End If
        End If
      End If

      Dim Topic As Topic = New ForumManager.Topic(ForumId, CategoryId, Author, Title, Nothing, Text, PhotoID, Video, True, Nothing, Author, 0, False, Feed.pubDate)
    End Sub

    Private Shared Function ExtrapolateImg(Html As String) As String
      If Html IsNot Nothing Then      
        Dim Lhtml = Html.ToLower
        Dim P = Lhtml.IndexOf("<img ")
        If P <> -1 Then
          Dim S = Lhtml.IndexOf(" src=""", P)
          If S <> -1 Then
            Dim E = Lhtml.IndexOf("""", S + 7)
            If E <> -1 Then
              S = S + 6
              Return HttpUtility.HtmlDecode(Html.Substring(S, E - S))
            End If
          End If
        End If
      End If
      Return Nothing
    End Function

    Private Shared Function ExtrapolateVideoSource(Html As String) As String
      If Html <> "" Then
        Dim decHtml = HttpUtility.HtmlDecode(Html)
        decHtml = Replace(decHtml, "\/", "/")
        Dim Pres() As String = {"http://", "https:/", "rtmp://", "mms//", "rtsp://"}
        For Each Pr In Pres
          Pr = """" & Pr
          Dim P As Integer = 0
          Dim E As Integer = 0
          P = decHtml.IndexOf(Pr)
          Do Until P = -1
            E = decHtml.IndexOf("""", P + 1)
            If E <> -1 Then
              Dim Url = decHtml.Substring(P + 1, E - P - 1)
              If IsMediaSource(Url) Then
                Return Url
              Else
                Dim VideoId As String = ExtrapolateVideoID(Url)
                If VideoId <> "" AndAlso Not VideoId.Contains("//") Then
                  Return VideoId
                End If
              End If
            End If
            P = decHtml.IndexOf(Pr, P + 1)
          Loop
        Next
      End If
    End Function

    Private Shared Function RemoveCDATA(Html As String) As String
      If Html.StartsWith("<![CDATA[") Then
        Html = Html.Substring(9)
      End If
      If Html.EndsWith("]]") Then
        Html = Html.Substring(0, Html.Length - 2)
      End If
      Return Trim(Html)
    End Function

    Private Shared Sub Plugin_OnEnabledStatusChanged(SubSite As SubSite, Enabled As Boolean) Handles Plugin.OnEnabledStatusChanged
      If Enabled Then
        RunLoadSource(SubSite)
      End If
    End Sub

  End Class

End Namespace