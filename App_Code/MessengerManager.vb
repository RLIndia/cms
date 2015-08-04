'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Namespace WebApplication
	Public Module MessengerManager

    'Enum Window As Integer
    '	None
    '	Action
    '	Card
    '	Messenger
    'End Enum

		Enum Action As Integer
			None
			AddContact
			RemoveContact
			SendMessengerComunication
			BlockUser
			DeleteTopics
			RequireTopicCensore
			RequireAdmonishUser
      RatingUser
      CloseTopic
      OpenTopic
      MarkTopicAsResolved
    End Enum

    Function HrefPrivateChatRoom(ByVal Setting As SubSite, ByVal UserName As String, Optional ByVal UserNameInvitedToJoin As String = Nothing, Optional ByVal AbsoluteAddress As Boolean = False) As String
      If UserNameInvitedToJoin Is Nothing Then
        Return Href(Setting.Name, AbsoluteAddress, "chat.aspx", QueryKey.ChatRoom, UserName)
      Else
        Return Href(Setting.Name, AbsoluteAddress, "chat.aspx", QueryKey.ChatRoom, UserName, QueryKey.ChatJoinUser, UserNameInvitedToJoin)
      End If
    End Function


    Function QueryStringParameters(Optional ByVal UserName As String = Nothing, Optional ByVal Action As Action = Action.None, Optional ByVal Confirm As Boolean = False, Optional ByVal Value As String = Nothing, Optional ByVal ForumId As Integer = 0, Optional ByVal TopicId As Integer = 0, Optional ByVal ReplyId As Integer = 0) As String()
      Dim Result() As String = Nothing
      If Value IsNot Nothing Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgValue)
        AddElement(Result, Value)
      End If
      If UserName IsNot Nothing Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgUser)
        AddElement(Result, UserName)
      End If
      Dim Culture As Globalization.CultureInfo = New Globalization.CultureInfo("en-US")
      If Action <> MessengerManager.Action.None Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgAction)
        AddElement(Result, CStr(Action))
      End If
      If Confirm Then
        Dim KeyValue(1) As String
        AddElement(Result, "Confirm")
        AddElement(Result, CStr(Confirm))
      End If
      If ForumId <> 0 Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgForumId)
        AddElement(Result, CStr(ForumId))
      End If
      If TopicId <> 0 Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgTopicId)
        AddElement(Result, CStr(TopicId))
      End If
      If ReplyId <> 0 Then
        Dim KeyValue(1) As String
        AddElement(Result, QueryKey.MsgReplyId)
        AddElement(Result, CStr(ReplyId))
      End If
      Return Result
    End Function

		Function ChatTarget(ByVal ChatRoom As String) As String
      Return "_chat_" & ReplaceBin(ChatRoom, " ", "_")
		End Function

		Class SocialConfiguration
			Public EmailAccessibility As Accessibility = Accessibility.UserContacts
			Public PrivateConversation As Accessibility = Accessibility.UserContacts
			Public Popularity As Integer
			Private Sub AddPopularity(ByVal UserName As String, ByVal Value As Integer)
        Dim User As User = Authentication.User.Load(UserName)
				User.SocialConfiguration.Popularity += Value
				User.Save()
			End Sub
			Function ContainContact(ByVal UserName As String) As Boolean
				If UserContacts Is Nothing Then
					Return False
				Else
					For Each Contact As String In UserContacts
						If StrComp(Contact, UserName, CompareMethod.Text) = 0 Then
							Return True
						End If
					Next
					Return False
				End If
			End Function
			Function ContainContact(ByVal User As User) As Boolean
				Return ContainContact(User.Username)
			End Function
			Public UserContacts As String()

			Function AddUserContact(ByVal User As String) As Boolean
				If UserContacts Is Nothing Then
					ReDim UserContacts(0)
					UserContacts(0) = User
					AddPopularity(User, 1)
					Return True
				Else
					For Each Contact As String In UserContacts
						If StrComp(Contact, User, CompareMethod.Text) = 0 Then
							Return False
						End If
					Next
					ReDim Preserve UserContacts(UserContacts.Length)
					UserContacts(UBound(UserContacts)) = User
					AddPopularity(User, 1)
					Return True
				End If
			End Function
      Function RemoveUserContact(ByVal user As String) As Boolean
        If UserContacts Is Nothing Then
          Return False
        Else
          Dim NewContacts(UBound(UserContacts)) As String
          Dim N As Integer = 0
          For Each Contact As String In UserContacts
            If StrComp(Contact, user, CompareMethod.Text) <> 0 Then
              NewContacts(N) = Contact
              N += 1
            End If
          Next
          ReDim Preserve NewContacts(N - 1)
          If UserContacts.Length = NewContacts.Length Then
            Return False
          Else
            UserContacts = NewContacts
            AddPopularity(user, -1)
            Return True
          End If
          Return False
        End If
      End Function

			Public ChatAccessibility As New ChatManager.ChatAccessibility
		End Class

		Enum Accessibility
			None
			AllUser
			UserContacts
		End Enum

		Function PrivateConversation(ByVal User As User, Optional ByVal CurrentUser As User = Nothing) As Accessibility
			Return CheckAccessibility(Media.Chat, User, CurrentUser)
		End Function

		Function EmailAccessibility(ByVal User As User, Optional ByVal CurrentUser As User = Nothing) As Accessibility
			Return CheckAccessibility(Media.Email, User, CurrentUser)
		End Function

		Private Function CheckAccessibility(ByVal Media As Media, ByVal User As User, Optional ByVal CurrentUser As User = Nothing) As Accessibility
			If CurrentUser Is Nothing Then
        CurrentUser = Authentication.CurrentUser(Nothing)
			End If
      If CurrentUser.Username = User.Username OrElse String.IsNullOrEmpty(CurrentUser.Username) Then
        Return Accessibility.None
      Else
        Dim Accessibility As Accessibility = MessengerManager.Accessibility.None
        Select Case Media
          Case MessengerManager.Media.Email
            Accessibility = User.SocialConfiguration.EmailAccessibility
          Case MessengerManager.Media.Chat
            Accessibility = User.SocialConfiguration.PrivateConversation
        End Select
        Select Case Accessibility
          Case Accessibility.None
            Return Accessibility.None
          Case Accessibility.AllUser
            Return Accessibility.AllUser
          Case Accessibility.UserContacts
            If User.SocialConfiguration.ContainContact(CurrentUser.Username) Then
              Return Accessibility.UserContacts
            End If
        End Select
      End If
      Return MessengerManager.Accessibility.None
    End Function

		Private Enum Media
			Email
			Chat
			Audio
			Video
		End Enum

		Function ScriptCheckEvents(ByVal Setting As Config.SubSite) As Control

			'Questo algoritmo va in conflitto con openwysiwyg (l'editor html open source)
			'Questa funzione viene pertanto rimossa
			'Ci sarà da studiare qualche cosa di alternativo per il futuro

			Dim Control As New Control
      If CurrentUser(Nothing).GeneralRole > User.RoleType.Visitors Then
        Dim Src As String = Common.Href(Setting.Name, False, "messenger.aspx", QueryStringParameters(, Action.SendMessengerComunication))

        Dim ScriptCheck As String = _
        "On Error Resume Next" & vbCrLf & _
        "if lcase(document.all.MessengerChecker.recordset(0))<>"""" then" & vbCrLf & _
         "if msgbox(document.all.MessengerChecker.recordset(0),1)=1 then" & vbCrLf & _
         "document.all.MessengerChecker.recordset.moveNext:gotourl=document.all.MessengerChecker.recordset(0)" & vbCrLf & _
         "document.all.MessengerChecker.recordset.moveNext:target=document.all.MessengerChecker.recordset(0)" & vbCrLf & _
         "open gotourl,target" & vbCrLf & _
         "end if" & vbCrLf & _
        "end if" & vbCrLf

        Control.Controls.Add(Components.Script(ScriptCheck, Components.ScriptLanguage.vbscript, "MessengerChecker", "ondatasetcomplete"))

        Dim XmlChecker As New LiteralControl
        XmlChecker.Text = "<xml id=MessengerChecker src=""" & Src & """></xml>"
        Control.Controls.Add(XmlChecker)

        Dim TimerCode As String = "On Error Resume Next" & vbCrLf & _
         "IDTimer = window.setInterval (""MessengerChecker()"",10000)" & vbCrLf & _
        "sub MessengerChecker()" & vbCrLf & _
         "document.all.MessengerChecker.src=""" & Src & """" & vbCrLf & _
        "end sub" & vbCrLf
        Control.Controls.Add(Components.Script(TimerCode, ScriptLanguage.vbscript))
      End If
			Return Control
		End Function

    Private MessengerComunications As New Dictionary(Of String, Collections.Generic.List(Of MessengerComunication))
    Sub AddComunication(ByVal FromUser As String, ByVal ToUser As String, ByVal TypeOfComunication As TypeOfComunication)
      Dim ComunicarionsToUser As Collections.Generic.List(Of MessengerComunication)
      If MessengerComunications.ContainsKey(ToUser) Then
        ComunicarionsToUser = MessengerComunications(ToUser)
      Else
        ComunicarionsToUser = New Collections.Generic.List(Of MessengerComunication)
        MessengerComunications.Add(ToUser, ComunicarionsToUser)
      End If
      ComunicarionsToUser.Add(New MessengerComunication(FromUser, TypeOfComunication))
    End Sub
		Function MessengerComunicarionsToUser(ByVal ToUser As String) As MessengerComunication
      If Not String.IsNullOrEmpty(ToUser) AndAlso MessengerComunications.ContainsKey(ToUser) = True Then
        Dim ComunicarionsToUser As Collections.Generic.List(Of MessengerComunication) = MessengerComunications(ToUser)
        While CBool(ComunicarionsToUser.Count)
          Dim Result As MessengerComunication = ComunicarionsToUser(0)
          ComunicarionsToUser.Remove(Result)
          If ComunicarionsToUser.Count = 0 Then
            MessengerComunications.Remove(ToUser)
          End If
          If Now < Result.Expire Then
            Return Result
          End If
        End While
      End If
      Return Nothing
    End Function
		Class MessengerComunication
			Public FromUser As String
			Public TypeOfComunication As TypeOfComunication
			Public Expire As Date
			Public Sub New(ByVal FromUser As String, ByVal TypeOfComunication As TypeOfComunication, Optional ByVal Expire As Date = Nothing)
				If Me.Expire = Expire Then
          Me.Expire = Now.AddMinutes(Setup.Ambient.SessionTimeOut)
				End If
				Me.FromUser = FromUser
				Me.TypeOfComunication = TypeOfComunication
			End Sub
		End Class

		Enum TypeOfComunication
			JoinToChat
		End Enum

	End Module
End Namespace
