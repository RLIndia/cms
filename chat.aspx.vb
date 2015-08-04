Imports WebApplication
Partial Class Chat
  Inherits System.Web.UI.Page

  Private Setting As SubSite
  Private JoinUser As String
  Private ChatRoom As String
  Shared Plugin As PluginManager.Plugin = Initialize()
  Shared Function Initialize() As PluginManager.Plugin
    If Plugin Is Nothing Then Plugin = New PluginManager.Plugin(AddressOf Description, Authentication.User.RoleType.Visitors, True, False, PluginManager.Plugin.Characteristics.EnabledByDefault)
    Return Plugin
  End Function
  Shared Sub New()
    Initialize()
  End Sub

  Shared Function Description(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
    If ShortDescription Then
      Return Phrase(Language, 2001)
    Else
      Return Phrase(Language, 2001)
    End If
  End Function

  Protected Sub Chat_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    If Config.Setup.Security.OnlyLoggedUsersInChat Then
      RegisterAnonimus()
    End If
    TryBlock(Setting)

    Dim MasterPage As Components.MasterPageEnhanced = SetMasterPage(Me, Nothing, False, True)
    MasterPage.TitleDocument = Phrase(Setting.Language, 2001)
    MasterPage.Description = Phrase(Setting.Language, 2001)
    MasterPage.KeyWords = Phrase(Setting.Language, 2001)
    MasterPage.AdSenseDisabled = True 'Note: Google ads, search boxes or search results may not be in chat programs http://support.google.com/adsense/bin/answer.py?hl=en&answer=2661562

  End Sub

  Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
    SendMessage()
  End Sub

  Private Sub SendMessage()
    RedirectBannedUser(Setting)
    Dim Text As String = Extension.Left(TextMessage.Text, TextMessage.MaxLength)
    Text = Text.Trim
    If Not String.IsNullOrEmpty(Text) Then
      Dim CurrentUser As User = Authentication.CurrentUser(Session)
      If CurrentUser.Role(Setting.Name) < Authentication.User.RoleType.AdministratorJunior Then
        If ContainCensoredWord(Text) Then
          BlockUserInChat()
        End If
      End If
      Text = Normalize(Text)
      'Text = HttpUtility.HtmlEncode(Text)

      'No resend identical message!
      If MySession("ChatLastText") <> Text Then
        MySession("ChatLastText") = Text


        Dim Author As String
        Dim Visitors As Boolean = False
        If Not String.IsNullOrEmpty(CurrentUser.Username) Then
          Author = CurrentUser.Username
        Else
          Author = FirstUpper(Request.QueryString(QueryKey.ChatJoinUser))
          If Author Is Nothing Then
            Author = HttpContext.Current.Request.UserHostAddress
          End If
          Visitors = True
        End If
        ChatManager.SendMessage(Text, Author, Visitors, ChatRoom, IsBlockedInChat(Setting))
      End If
    End If
    'Reset text if page
    TextMessage.Text = ""
  End Sub

  Private Sub Page_PreInit(sender As Object, e As EventArgs) Handles Me.PreInit
    'Url request parameters:
    'r = chat room

    Setting = CurrentSetting()
    Dim CheffOfChatRoom As User = Nothing
    If Not Request.QueryString(QueryKey.ChatRoom) Is Nothing Then
      ChatRoom = Request.QueryString(QueryKey.ChatRoom)
      CheffOfChatRoom = Authentication.User.Load(ChatRoom)
    Else
      ChatRoom = DefaultChatRoom
    End If

    Dim CurrentUser As User = Authentication.CurrentUser(Session)
    If Not Request.QueryString(QueryKey.ChatJoinUser) Is Nothing Then
      JoinUser = Request.QueryString(QueryKey.ChatJoinUser)
      'Block not autorized call a join
      Dim UserInvitedToJoin As Authentication.User = Authentication.User.Load(JoinUser)
      If Not UserInvitedToJoin.SocialConfiguration.ContainContact(CurrentUser) Then
        EndResponse()
      ElseIf StrComp(CurrentUser.Username, ChatRoom, CompareMethod.Text) <> 0 Then
        EndResponse()
      End If
      UserInvitedToJoin.SocialConfiguration.ChatAccessibility.IsAbleToAccess(ChatRoom) = True
      AddComunication(CurrentUser.Username, JoinUser, TypeOfComunication.JoinToChat)
      Response.Redirect(HrefPrivateChatRoom(Setting, CurrentUser.Username))
    End If

    'Block not autorized
    If CheffOfChatRoom IsNot Nothing Then
      If Not ChatRoom.First = "_"c AndAlso CurrentUser.Username <> ChatRoom AndAlso Not CurrentUser.SocialConfiguration.ChatAccessibility.IsAbleToAccess(ChatRoom) Then
        EndResponse()
      End If
    End If

    Dim Action As TypeOfRequest = CType(Request(QueryKey.ChatAction), TypeOfRequest)
    Select Case Action
      Case TypeOfRequest.IsUpdated
        Dim LastMessage As Integer = CInt(Session("ChatLastMessage"))
        Dim Refresh As Boolean = False
        If LastMessageID(ChatRoom, IsBlockedInChat(Setting)) > LastMessage Then
          Refresh = True
        End If
        Response.ContentType = "text/html"
        Response.Write(Refresh.ToString)
        Response.End()
      Case TypeOfRequest.SendContent, TypeOfRequest.SendContentPreview
        Dim Preview As Boolean = Action = TypeOfRequest.SendContentPreview
        Dim Messages As Control = ControlMessages(ChatRoom, CurrentUser, Setting, Preview)
        Dim HTML As String = ControlToText(Messages)
        Dim LastMessage As Integer = CInt(Session("ChatLastMessage"))
        If Not String.IsNullOrEmpty(Request.QueryString(QueryKey.ChatForceRefresh)) OrElse LastMessageID(ChatRoom, IsBlockedInChat(Setting)) > LastMessage Then
          Session("ChatLastMessage") = LastMessageID(Nothing, IsBlockedInChat(Setting))
          Response.ContentType = "text/javascript"
          Response.Cache.SetExpires(Now.AddSeconds(10))
          'If the client perform the "unescape", need replace the "%" with "%25"
          Response.Write("document.getElementById(""displaymsg"").innerHTML=unescape(""" & ReplaceBin(AbjustForJavascriptString(HTML), "%", "%25") & """);")
        End If
        Response.End()
    End Select

  End Sub

  Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.PreRender
    Discussion.Controls.Add(Fieldset(Phrase(Setting.Language, 2001), ChatWindow(Setting, ChatRoom)))
    AddEmoticonsTool(Emoticons, TextMessage)
    AddLiteral(CType(Compose, Control), "<script type=""text/javascript"">document.getElementById(""" & TextMessage.ClientID & """).focus();</script>")
    Exit Sub
    'Add online users
  End Sub

  Private Sub TextMessage_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextMessage.TextChanged
    SendMessage()
  End Sub

End Class
