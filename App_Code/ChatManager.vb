'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications
Option Explicit On
Option Strict On
Namespace WebApplication
	Public Module ChatManager
		Public Const DefaultChatRoom As String = "_"
		Public Const MaxMessage As Integer = 100
		'Public Messages(MaxMessage) As Message

		Function QueryStringParameters(Optional ByVal Room As String = Nothing, Optional ByVal TypeOfRequest As TypeOfRequest = TypeOfRequest.Show, Optional ByVal JoinToUser As String = Nothing, Optional ByVal ForceRefresh As Boolean = False) As String()
      Dim Result() As String = Nothing
			If Room IsNot Nothing Then
        AddElement(Result, QueryKey.ChatRoom)
				AddElement(Result, Room)
			End If
			If JoinToUser IsNot Nothing Then
        AddElement(Result, QueryKey.ChatJoinUser)
				AddElement(Result, JoinToUser)
			End If
			If TypeOfRequest <> ChatManager.TypeOfRequest.Show Then
        AddElement(Result, QueryKey.ChatAction)
        AddElement(Result, CStr(TypeOfRequest))
			End If
			If ForceRefresh Then
        AddElement(Result, QueryKey.ChatForceRefresh)
        AddElement(Result, "1")
			End If
			Return Result
		End Function

		Private MessagesCollection As New Collections.Specialized.HybridDictionary
    Function Messages(ByVal ChatRoom As String) As Message()
      If MessagesCollection.Contains(ChatRoom) Then
        Return CType(MessagesCollection(ChatRoom), Message())
      Else
        Dim EmptyMessages(MaxMessage - 1) As Message
        MessagesCollection.Add(ChatRoom, EmptyMessages)
        Return EmptyMessages
      End If
    End Function


    'Sub AddMessage(ChatRoom As String, Value As Message())
    '  If MessagesCollection.Contains(ChatRoom) Then
    '    MessagesCollection.Remove(ChatRoom)
    '  End If
    '  MessagesCollection.Add(ChatRoom, Value)
    'End Sub

    Private LastMessageIDNoCensored As Integer
    Private LastMessageIDCensored As Integer
    Property LastMessageID(ByVal KeyChat As String, ByVal Censored As Boolean) As Integer
      Get
        If Censored Then
          Return LastMessageIDCensored
        Else
          Return LastMessageIDNoCensored
        End If
      End Get
      Set(ByVal value As Integer)
        If Censored Then
          LastMessageIDCensored = value
        Else
          LastMessageIDNoCensored = value
        End If
      End Set
    End Property


    Enum TypeOfRequest
      Show
      IsUpdated
      SendContent
      SendContentPreview
    End Enum
    Class Message
      Public Time As Date
      Public Text As String
      Public Author As String
      Public AuthorIsVisitors As Boolean
      Public Censored As Boolean
    End Class
    Function ControlMessages(ByVal ChatRoom As String, ByVal User As User, ByVal Setting As SubSite, Optional ByVal Preview As Boolean = False) As Control
      Dim Controls As New Control
      Dim Counter As Integer = 0
      Dim ShowCensored As Boolean = User.Role(Setting.Name) > Authentication.User.RoleType.AdministratorJunior
      Dim AllMessages As Message() = Messages(ChatRoom)
      Dim BackGroundIsDark As Boolean
      Dim SkinSetup As Config.SkinSetup = Setting.Skin().SkinSetup()
      Dim BackgroundColorPage As String = SkinSetup.Variables("BackgroundColorPage")
      If BackgroundColorPage IsNot Nothing Then
        Dim BgColor As Drawing.Color
        Try
          BgColor = Drawing.ColorTranslator.FromHtml(BackgroundColorPage)
          If BgColor.GetBrightness < 0.5! Then
            BackGroundIsDark = True
          End If
        Catch ex As Exception
        End Try
      End If

      SyncLock AllMessages
        For Each Message As Message In AllMessages
          Counter += 1
          If Not Message Is Nothing Then
            If Message.Censored = False OrElse IsBlockedInChat(Setting) OrElse ShowCensored Then
              If Preview Then
                Dim Author As New Control
                Author = InfoUser(Message, Setting, Preview)
                Dim Text As New Label
                Text.Style.Add("font-size", "smaller")
                Text.Text = ">" & AddEmoticons(Message.Text) & " "
                Controls.Controls.Add(Author)
                Controls.Controls.Add(Text)
                If Counter = 15 Then
                  Exit For
                End If
              Else
                Dim Time As New HtmlGenericControl("time")
                Time.Attributes.Add("datetime", Message.Time.ToString("s"))
                Time.InnerText = Message.Time.ToLocalTime().AddSeconds(User.TimeOffsetSeconds).ToString(Setting.TimeFormat, Setting.Culture()) & " "

                Dim Text As New Label
                Dim Phrase As String = UrlToLink(Message.Text, Setting, CurrentDomainConfiguration, True)


                Text.Text = " " & AddEmoticons(Phrase)
                'Text.ForeColor = ColorMessage(Message.Author)
                If ShowCensored AndAlso Message.Censored Then
                  'Evidence Censored messages for administrator
                  Text.BorderStyle = BorderStyle.Ridge
                End If
                Dim Span As New WebControl(HtmlTextWriterTag.Span)

                Span.Style.Add("color", ColorMessageString(Message.Author, BackGroundIsDark))
                Span.Controls.Add(Time)
                Span.Controls.Add(InfoUser(Message, Setting, Preview))
                Span.Controls.Add(Text)
                Span.Controls.Add(BR)
                Controls.Controls.Add(Span)
              End If
            End If
          End If
        Next
      End SyncLock
      Return Controls
    End Function
    Function ColorMessage(ByVal Author As String) As System.Drawing.Color
      Dim Seme As Double = 0.0#
      For N As Integer = 0 To Len(Author) - 1
        Seme += Asc(Author.Chars(N))
      Next
      Rnd(-1)
      Randomize(Seme)
      Dim Red, Green, Blue As Integer
      Do
        Red = CInt(Rnd() * 255)
        Green = CInt(Rnd() * 255)
        Blue = CInt(Rnd() * 255)
      Loop Until (Red + Green + Blue) < 260
      Return Drawing.Color.FromArgb(Red, Green, Blue)
    End Function
    Function ColorMessageString(Author As String, BackGroundIsDark As Boolean) As String
      Dim Seme As Double = 0.0#
      For N As Integer = 0 To Len(Author) - 1
        Seme += Asc(Author.Chars(N))
      Next
      Rnd(-1)
      Randomize(Seme)
      Dim Red, Green, Blue As Integer
      If BackGroundIsDark Then
        Do
          Red = CInt(Rnd() * 255)
          Green = CInt(Rnd() * 255)
          Blue = CInt(Rnd() * 255)
        Loop Until (Red + Green + Blue) > 500
      Else
        Do
          Red = CInt(Rnd() * 255)
          Green = CInt(Rnd() * 255)
          Blue = CInt(Rnd() * 255)
        Loop Until (Red + Green + Blue) < 260
      End If
      Return "#" & Extension.Right("0" & Hex(Red), 2) & Extension.Right("0" & Hex(Green), 2) & Extension.Right("0" & Hex(Blue), 2)
    End Function

    Function InfoUser(ByVal Message As Message, ByVal Setting As SubSite, ByVal Preview As Boolean) As Control
      Dim Author As User
      Dim ForceOnline As Boolean
      If Message.AuthorIsVisitors Then
        Author = New User
        Author.Username = Message.Author
        ForceOnline = True
        Author.GeneralRole = User.RoleType.Visitors
        If Message.Author.Contains(".") Then
          Author.IP = Message.Author
        End If
      Else
        Author = User.Load(Message.Author)
      End If
      Return QuickInfoUser(Setting, Author, Nothing, Preview)
    End Function

    Public Class ListMessage
      <System.Xml.Serialization.XmlElementAttribute("item")> Public Item As New Items
    End Class
    Public Class Items
      Inherits CollectionBase
      Public Sub Add(ByVal Item As String)
        Dim I As Integer = List.Add(Item)
      End Sub
      Default Public ReadOnly Property Item(ByVal Index As Integer) As String
        Get
          Return CType(List.Item(Index), String)
        End Get
      End Property
    End Class
    Public Function ChatPreview(ByVal ChatRoom As String, ByVal Setting As SubSite) As Control
      If HttpContext.Current.Request.Browser.EcmaScriptVersion.Major >= 1 Then
        Dim Link As New HyperLink
        Link.NavigateUrl = Common.Href(Setting.Name, False, "chat.aspx")
        Dim Window As Control = ChatWindow(Setting, ChatRoom, 60, 100, True)
        Link.Controls.Add(Window)
        'ChatManager.ControlMessages(ChatRoom, CurrentUser, Setting, True)
        Link.Style.Add("display", "block")
        Dim Field As Control = Fieldset(Phrase(Setting.Language, 2002), Link)
        Field.Controls.Add(Link)
        Dim Controls As New Control
        Controls.Controls.Add(New LiteralControl("<!--googleoff: index-->"))
        Controls.Controls.Add(Field)
        Controls.Controls.Add(New LiteralControl("<!--googleon: index-->"))
        Return Controls
      Else
        Return New Control
      End If
    End Function
    Public Sub SaveMessages(ByVal ChatRoom As String)
      Dim Data As String = Nothing
      Dim AllMessages As Message() = Messages(ChatRoom)
      If Not AllMessages Is Nothing Then
        SyncLock AllMessages
          For Each Message As Message In AllMessages
            If Message Is Nothing Then
              Exit For
            Else
              Data &= DateToText(Message.Time) & vbTab & Message.Author & vbTab & Message.AuthorIsVisitors & vbTab & Message.Censored & vbTab & Message.Text & vbCr
            End If
          Next
        End SyncLock
      End If
      WriteAll(Data, FileManager.MapPath(ChatSubDirectory & "\" & ChatRoom & ".txt"))
    End Sub
    Public Sub LoadMessages(ByVal ChatRoom As String)
      Dim Data As String
      Try
        Data = ReadAll(FileManager.MapPath(ChatSubDirectory & "\" & ChatRoom & ".txt"), True)
      Catch ex As Exception
        'file not exists
        Exit Sub
      End Try
      Dim Records() As String = Split(Data, vbCr)
      Dim AllMessages As Message() = Messages(ChatRoom)
      SyncLock AllMessages
        Dim N As Integer = 0
        For Each Record As String In Records
          If Not String.IsNullOrEmpty(Record) Then
            If N < MaxMessage Then
              Dim Fields() As String = Split(Record, vbTab)
              Dim Message As New Message
              Message.Time = TextToDate(Fields(0))
              Message.Author = Fields(1)
              Message.AuthorIsVisitors = CBool(Fields(2))
              Message.Censored = CBool(Fields(3))
              Message.Text = Fields(4)
              AllMessages(N) = Message
              N += 1
            Else
              Exit For
            End If
          End If
        Next
      End SyncLock
    End Sub

    Function ChatWindow(ByVal Setting As Config.SubSite, ByVal ChatRoom As String, Optional ByVal EverySeconds As Integer = 10, Optional ByVal Height As Integer = 300, Optional ByVal Preview As Boolean = False) As Control
      Dim Place As New Control

      Dim Window As New LiteralControl
      If Preview Then
        Dim Marquee As String = "<marquee id=""displaymsg"" scrolldelay=""1"" scrollamount=""1"" direction=""down""></marquee>"
        Marquee = "<script type=""text/javascript"">document.write('" & AbjustForJavascriptString(Marquee) & "');</script>"
        Window.Text = "<div style=""height:" & Height & "px; overflow:auto"">" & Marquee & "</div>"
      Else
        Window.Text = "<div id=""displaymsg"" style=""height:" & Height & "px; overflow:auto""></div>"
      End If
      Place.Controls.Add(Window)

      Dim Request As TypeOfRequest
      If Preview Then
        Request = TypeOfRequest.SendContentPreview
      Else
        Request = TypeOfRequest.SendContent
      End If
      '==============================================
      'New algoritm using polling javascript
      '==============================================

      Dim ScriptSourceMessages As New WebControl(HtmlTextWriterTag.Script)
      ScriptSourceMessages.ID = "sourcemessages"
      'ScriptSourceMessages.Attributes("language") = ScriptLanguage.javascript.ToString().ToLower()
      ScriptSourceMessages.Attributes("type") = ScriptType(ScriptLanguage.javascript)
      ScriptSourceMessages.Attributes.Add("src", Common.Href(Setting.Name, True, "chat.aspx", QueryStringParameters(ChatRoom, Request, Nothing, True)))
      Place.Controls.Add(Components.Literal(ControlToText(ScriptSourceMessages)))
      Dim Script As String

      Script = _
      "SrcMsg=""" & Common.Href(Setting.Name, True, "chat.aspx", QueryStringParameters(ChatRoom, Request)) & """;" & vbCrLf & _
      "function UPDCHAT(){" & vbCrLf & _
       "xhead=document.getElementsByTagName('head')[0];" & vbCrLf & _
       "xscript=document.createElement('script');" & vbCrLf & _
       "xscript.type='text/javascript';" & vbCrLf & _
       "xscript.src=SrcMsg+""&now="" + new Date();" & vbCrLf & _
       "xhead.appendChild(xscript);" & vbCrLf & _
      "}" & vbCrLf & _
      "xupd=setInterval(""UPDCHAT()""," & (EverySeconds * 1000).ToString & ")" & vbCrLf

      Place.Controls.Add(Components.Script(Script, Components.ScriptLanguage.javascript))
      Return Place
    End Function

    Sub BlockUserInChat()
      'block the user
      Const HoursToBlock As Integer = 6
      MySession("UserTempBlockInChat") = "True"
      Cookie("UserTempBlockInChat", Now.AddHours(HoursToBlock)) = "True"
      BlockIPInChat(CurrentUser(Nothing).IP, New TimeSpan(HoursToBlock, 0, 0))
    End Sub

    Function IsBlockedInChat(ByVal Setting As SubSite) As Boolean
      Return IsBlocked(Setting, "UserTempBlockInChat", IPBlockedsInChat)
    End Function

    Public IPBlockedsInChat As Collections.Specialized.StringDictionary = LoadIPBlockedCollection("IPBlockedsInChat")

    Sub BlockIPInChat(ByVal IP As String, ByVal Time As TimeSpan)
      BlockIP(IP, Time, IPBlockedsInChat)
    End Sub

    Function IPIsBlockedInChat(ByVal IP As String) As Boolean
      Return IPIsBlocked(IP, IPBlockedsInChat)
    End Function

    Class ChatAccessibility
      Private Collection As New Collections.Specialized.StringCollection
      Property IsAbleToAccess(ByVal ChatRoom As String) As Boolean
        Get
          If ChatRoom.StartsWith(DefaultChatRoom) Then
            Return True
          Else
            Return Collection.Contains(ChatRoom)
          End If
        End Get
        Set(ByVal value As Boolean)
          If Not ChatRoom.StartsWith(DefaultChatRoom) Then
            Select Case value
              Case True
                If Not Collection.Contains(ChatRoom) Then
                  Collection.Add(ChatRoom)
                End If
              Case False
                If Collection.Contains(ChatRoom) Then
                  Collection.Add(ChatRoom)
                End If
            End Select
          End If
        End Set
      End Property
      Sub Claer()
        Collection.Clear()
      End Sub
    End Class

    Sub SendMessage(ByVal Message As Message)
      SendMessage(Message.Text, Message.Author, Message.AuthorIsVisitors)
    End Sub
    Sub SendMessage(ByVal Text As String, ByVal Author As String, Optional ByVal AuthorIsVisitors As Boolean = False, Optional ByVal ChatRoom As String = DefaultChatRoom, Optional ByVal Censored As Boolean = False)
      Dim Message As New Message
      Message.Time = Now.ToUniversalTime()
      Message.Author = Author
      Message.Text = HttpUtility.HtmlEncode(Text)
      Message.AuthorIsVisitors = AuthorIsVisitors

      Dim AllMessages As Message() = Messages(ChatRoom)
      SyncLock AllMessages
        Array.Copy(AllMessages, 0, AllMessages, 1, MaxMessage - 1)
        AllMessages(0) = Message
      End SyncLock

      Select Case Message.Censored
        Case True
          LastMessageID(ChatRoom, True) = LastMessageID(ChatRoom, True) + 1
        Case False
          LastMessageID(ChatRoom, False) = LastMessageID(ChatRoom, False) + 1
          LastMessageID(ChatRoom, True) = LastMessageID(ChatRoom, True) + 1
      End Select
      SaveMessages(ChatRoom)
      Dim Setting As SubSite
      If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Response IsNot Nothing Then
        Setting = CurrentSetting()
        RaiseChatMessageWritedEvent(ChatRoom, Text, Author, Setting)
      End If
    End Sub


    Private MessageSoppoler As New System.Collections.Generic.List(Of Message)

    Private WithEvents TimerSendMessageGermanyRu As New Timers.Timer

    Private Sub TimerSendMessageGermanyRu_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TimerSendMessageGermanyRu.Elapsed
      Dim Message As Message
      If MessageSoppoler.Count <> 0 Then
        Message = MessageSoppoler(0)
        MessageSoppoler.RemoveAt(0)
        SendMessage(Message)
      End If
      TimerSendMessageGermanyRu.Interval = 4000 + Rnd() * 8000
    End Sub

    Public Const StyleCode As String = "position:fixed;left:40px;width:300px;bottom:10px;z-index: 100;text-align:center;padding:5px;border:5px solid black;color:black;background:white;"

    Public Function ChatPopUpStyle() As WebControl
      Dim Style As New WebControl(HtmlTextWriterTag.Style)
      Style.Controls.Add(Components.Literal(".popUp{" & StyleCode & "}"))
      Return Style
    End Function

    Public Function ChatInvite(ByVal FromUserName As String, ByVal Setting As SubSite, Optional ByVal Title As String = Nothing, Optional ByVal ForceNameInput As Boolean = True) As WebControl
      Dim PopUpBox As New WebControl(HtmlTextWriterTag.Div)
      PopUpBox.CssClass = "popUp"
      PopUpBox.ID = "popUpChat"


      Dim Top As New WebControl(HtmlTextWriterTag.Div)
      Dim Content As New WebControl(HtmlTextWriterTag.Div)
      Dim Bottom As New WebControl(HtmlTextWriterTag.Div)

      PopUpBox.Controls.Add(Top)
      PopUpBox.Controls.Add(Content)
      PopUpBox.Controls.Add(Bottom)

      If Not String.IsNullOrEmpty(Title) Then
        Dim TitleLabel As New Label
        TitleLabel.Text = Title
        TitleLabel.Style.Add("font-size", "xx-larger")
        TitleLabel.Style.Add("font-weight", "bold")
        Dim P As New WebControl(HtmlTextWriterTag.P)
        P.Controls.Add(TitleLabel)
        Top.Controls.Add(P)
      End If

      Dim AcceptMessage As New Label
      Dim User As User = Authentication.User.Load(FromUserName)
      Dim Invitant As String
      If User IsNot Nothing Then
        Invitant = User.FirstName
      Else
        Invitant = FromUserName
      End If
      AcceptMessage.Text = FirstUpper(Invitant) & " " & Phrase(Setting.Language, 3232)  'FirstName invite you to join in private chat. Accept?
      Top.Controls.Add(AcceptMessage)

      Dim PName As New WebControl(HtmlTextWriterTag.P)
      Bottom.Controls.Add(PName)

      Dim NameLabel As New Label
      NameLabel.Text = Phrase(Setting.Language, 24) & ":"
      PName.Controls.Add(NameLabel)

      Dim NameUserChat As New WebControls.TextBox
      NameUserChat.ID = "NameUserChat"
      'NameUserChat.Attributes.Add("name", "NameUserChat")
      NameUserChat.MaxLength = 20
      NameUserChat.Text = CurrentUser(Nothing).Username
      PName.Controls.Add(NameUserChat)

      Dim Enjoy As New WebControl(HtmlTextWriterTag.Button)
      Dim Script As String
      Script = "window.location.assign('" & Href(Setting.Name, True, "chat.aspx", QueryKey.ChatRoom, "_" & HttpContext.Current.Session.SessionID, QueryKey.ChatUser, "") & "'+document.getElementById('NameUserChat').value);"

      If ForceNameInput Then
        Script = "while (!document.getElementById('NameUserChat').value){document.getElementById('NameUserChat').value=prompt(""" & Ask(Phrase(Setting.Language, 24), Setting.Language) & """)};" & Script
      End If
      Enjoy.Attributes.Add("onclick", Script)
      Enjoy.Controls.Add(Components.Literal(Phrase(Setting.Language, 1309)))
      Bottom.Controls.Add(Enjoy)

      Dim CloseButton As New WebControl(HtmlTextWriterTag.Button)
      CloseButton.Attributes.Add("onclick", "document.getElementById('popUpChat').parentNode.removeChild(document.getElementById('popUpChat'));")
      CloseButton.Controls.Add(Components.Literal(Phrase(Setting.Language, 111)))
      Bottom.Controls.Add(CloseButton)

      'Add content into pop ub box

      If User IsNot Nothing AndAlso Not String.IsNullOrEmpty(User.NamePhotoAlbum) Then


        Dim PhotoAlbum As PhotoAlbum = User.PhotoAlbum

        If PhotoAlbum IsNot Nothing Then
          Dim Photos() As Integer = AllPhotosName(PhotoAlbum.Name)
          If Photos IsNot Nothing AndAlso Photos.Length > 0 Then
            Dim Photo As Photo = PhotoManager.Photo.Load(Photos(0), PhotoAlbum.Name)
            Dim Thumbnail As HtmlControl = CType(Photo.ControlThumbnail(Setting, False, 200), HtmlControl)  ' New WebControl(HtmlTextWriterTag.Span)
            Thumbnail.Attributes.Add("class", "BoxElement")
            Content.Controls.Add(Thumbnail)
          End If
        End If
      End If

      Return PopUpBox
    End Function

    Function JavaScriptPopUp(ByVal Html As String, Optional ByVal AfterSeconds As Integer = 0) As String
      Dim JFunction As String = "function createPopUp(popUpCode){var div = document.createElement('div');div.innerHTML=popUpCode;document.body.appendChild(div.firstChild);}" & vbCrLf
      Dim CallFunction As String
      CallFunction = "function ChatWindowPopUp(){createPopUp('" & AbjustForJavascriptString(Html) & "');}" & vbCrLf
      If AfterSeconds <> 0 Then
        'Note: The code Abjustment is necessary 2 times (AbjustForJavascriptString)
        CallFunction &= "setTimeout('" & AbjustForJavascriptString("if (!document.getElementById('NameUserChat')){ChatWindowPopUp()}") & "', " & (AfterSeconds * 1000).ToString & ")" & vbCrLf
      End If
      Return JFunction + CallFunction
    End Function

  End Module
End Namespace