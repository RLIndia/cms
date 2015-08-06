Imports WebApplication
Partial Class log
	Inherits System.Web.UI.Page
	Private Action As LogActionType = LogActionType.LogOn
	Private Setting As Config.SubSite
  Private ReturnUrl As String
  Private CurrentUser As Authentication.User

	Protected Sub log_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
    Action = CType(ValInt(Request.QueryString(QueryKey.ActionLog)), LogActionType)
		Setting = CurrentSetting()
    CurrentUser = Authentication.CurrentUser(Session)
    If Action <> LogActionType.LogOff Then
      ReturnUrl = CStr(Session("ReturnUrl"))
      If String.IsNullOrEmpty(ReturnUrl) Then
        If Request IsNot Nothing AndAlso Request.UrlReferrer IsNot Nothing AndAlso DomainName(Request) = Request.UrlReferrer.Host Then
          If Not Request.UrlReferrer.AbsolutePath.EndsWith("/log.aspx", StringComparison.InvariantCultureIgnoreCase) Then
            ReturnUrl = Request.UrlReferrer.AbsoluteUri
            Session("ReturnUrl") = ReturnUrl
          End If
        End If
      End If
    End If

		If Request.Browser.Cookies Then
			If Page.IsPostBack = False Then
				Select Case Action
					Case LogActionType.NewUser
						LayoutNew()
					Case LogActionType.EditUser
						LayoutEdit()
					Case LogActionType.LogOff
						CookieUserName = Nothing
						Session.Abandon()
            'Authentication.RemoveOnLineUser(Session)
            'CurrentUser(Session).LogOut()
						'Session.Clear()
            'Authentication.AddOnLineUser(Me.Session)
            If Not String.IsNullOrEmpty(ReturnUrl) Then
              Response.Redirect(ReturnUrl, True)
            Else
              Layout_sconnected()
            End If
					Case LogActionType.LostPassword
						LayoutLostPassword()
					Case Else
						If CurrentUser.Role(Setting.Name) = Authentication.User.RoleType.Visitors Then
							LayoutLogin()
						Else
							LayoutLoged()
						End If
				End Select
			End If

		End If

	End Sub

	Protected Sub log_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Dim MasterPage As Components.MasterPageEnhanced = SetMasterPage(Me, Nothing, False, False)
    MasterPage.Suggest(Submit1)
    MasterPage.AddMetaTag("robots", "noindex")
    Label3.Text = "* = " & Phrase(Setting.Language, 30)
    HyperLink1.NavigateUrl = HrefLog(LogActionType.LogOff)
    HyperLink2.NavigateUrl = HrefLog(LogActionType.NewUser)
    HyperLink3.NavigateUrl = HrefLog(LogActionType.LogOn)
    HyperLink4.NavigateUrl = HrefLog(LogActionType.EditUser)
    If Not Request.Browser.Cookies Then
      'If Cookies is not enabled, write allert!
      Label1.Text = Phrase(Setting.Language, 422)
      table2.Visible = False
      Buttons.Visible = False
      MasterPage = CType(Page.Master, Components.MasterPageEnhanced) : MasterPage.AddMessage(Phrase(Setting.Language, 3070), Setting)
    End If
  End Sub

  Protected Sub Submit1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Submit1.Click
    Submit()
  End Sub

  Function HrefLog(ByVal Action As LogActionType) As String
    If Action <> LogActionType.LogOn Then
      Return Href(Setting.Name, False, "log.aspx", QueryKey.ActionLog, Action)
    Else
      Return Href(Setting.Name, False, "log.aspx")
    End If
  End Function

  Sub Layout_sconnected()
    table2.Visible = False
    Buttons.Visible = False
    Label1.Text = Phrase(Setting.Language, 33)
    HyperLink3.Visible = True
  End Sub

  Sub LayoutNew()
    'No robots!
    If Extension.IsCrawler() Then
      Submit1.Visible = False
    End If

    'First startup
    If IsFirstRunning(Setting) = TypeOfFirstRunning.Application Then
      If Not String.IsNullOrEmpty(OEM.VideoAtFirstStartup) Then
        Dim MasterPage As Components.MasterPageEnhanced = CType(Page.Master, Components.MasterPageEnhanced)
        MasterPage.ContentPlaceHolder.Parent.Controls.AddAt(0, VideoObject(OEM.VideoAtFirstStartup, Setting, HttpContext.Current))
      End If
      Setting.IdSkin = OEM.SkinAtFirstStartup
      If Not String.IsNullOrEmpty(OEM.SoundAtFirstStartup) Then
        Dim SkinSetup = Setting.Skin().SkinSetup()
        SkinSetup.Variables.Remove("bgsound")
        SkinSetup.Variables.Remove("soundonlyatstartsession")
        SkinSetup.Variables.Add("bgsound", OEM.SoundAtFirstStartup)
        Config.Skin.CacheSkin.Clear()
      End If
      OEM.RunAtFirsStartup(Setting, CType(Page.Master, MasterPageEnhanced))
      'Auto setting user datas
      username.Value = "Super"
      Try : FirstName.Value = System.Environment.UserName : Catch ex As Exception : End Try
      If String.IsNullOrEmpty(FirstName.Value) Then
        Try : FirstName.Value = My.User.Name : Catch ex As Exception : End Try
      End If
      If String.IsNullOrEmpty(FirstName.Value) Then
        Try : FirstName.Value = Request.ServerVariables("LOGON_USER") : Catch ex As Exception : End Try
      End If
      'remove \
      If Not String.IsNullOrEmpty(FirstName.Value) Then
        FirstName.Value = FirstName.Value.Substring(FirstName.Value.IndexOf("\") + 1)
      End If
    End If

    table2.Visible = True
    Label1.Text = Phrase(Setting.Language, 20)
    Label3.Visible = True
    HyperLink3.Visible = True
    SkypeInfo.NavigateUrl = "http://www.skype.com/intl/" & LCase(Acronym(Setting.Language)) & "/share/buttons/status.html"
    AddExtraUserAttribute()
  End Sub

  Sub LayoutEdit()
    LayoutNew()
    HyperLink3.Visible = False
    HyperLink1.Visible = True
    table2.Rows(0).Visible = False
    Label1.Text = Phrase(Setting.Language, 320)
    Dim User As User = CurrentUser
    username.Value = User.Username
    FirstName.Value = User.FirstName
    LastName.Value = User.LastName
    City.Value = User.City
    country.Value = User.Country
    Email.Value = User.Email
    Skype.Value = User.Skype
    Select Case User.Gender
      Case Authentication.User.GenderType.Female
        F.Checked = True
      Case Authentication.User.GenderType.Male
        M.Checked = True
    End Select

    Dim UserPhotoAlbum As PhotoManager.PhotoAlbum = User.PhotoAlbum
    PersonalPhotoAlbum.Checked = Not UserPhotoAlbum Is Nothing
    If Not UserPhotoAlbum Is Nothing Then
      PhotoAlbum.Controls.Add(UserPhotoAlbum.Control(Setting, 0))
    End If

    'language.value=User.Language
    phone.Value = User.Phone
    Url.Value = User.URL
  End Sub

  
  
 
 Sub LayoutLogin()
  If Not Page.IsPostBack Then
    username.Value = CookieUserName
  End If
  HyperLink2.Visible = True
  Label1.Text = Phrase(Setting.Language, 21)
  table2.Visible = True
  For N As Integer = 2 To table2.Rows.Count - 1
    table2.Rows(N).Visible = False
  Next
  Label3.Visible = True
   End Sub

  Sub LayoutLostPassword()
    LayoutLogin()
    Label1.Text = Phrase(Setting.Language, 321)
    table2.Rows(1).Visible = False 
  End Sub

  Sub LayoutLoged()
    table2.Visible = False
    HyperLink1.Visible = True
    HyperLink4.Visible = True
    Label1.Text = Phrase(Setting.Language, 34) & " " & CurrentUser.Username
    Buttons.Visible = False
    Table2.Visible = False
  End Sub

  Sub AddExtraUserAttribute()
    Dim User As User = CurrentUser
    For Each Plugin As PluginManager.Plugin In AllPlugins()
      If Plugin.IsEnabled(Setting) Then
        If Plugin.AddExtraUserAttributes IsNot Nothing Then
          Dim Attributes As Collections.Generic.List(Of PluginManager.Plugin.ExtraUserAttribute) = Plugin.AddExtraUserAttributes.Invoke()
          If Attributes IsNot Nothing Then
            For Each Attribute As PluginManager.Plugin.ExtraUserAttribute In Attributes
              Dim Row As New HtmlControls.HtmlTableRow
              Row.Cells.Add(New HtmlControls.HtmlTableCell)
              Row.Cells.Add(New HtmlControls.HtmlTableCell)
              Row.Cells(0).Attributes.Add("class", "leftcoll")
              Row.Cells(1).Attributes.Add("class", "rightcoll")
              Dim Label As New WebControls.Label
              Label.Text = Attribute.Description(Setting.Language)
              Row.Cells(0).Controls.Add(Label)
              Dim TextBox As New WebControls.TextBox
              If CBool(Attribute.MaxLengt) Then
                TextBox.MaxLength = Attribute.MaxLengt
              End If
              If Attribute.TypeAttribute = PluginManager.Plugin.ExtraUserAttribute.TypeOfAttribute.TextMultiLine Then
                TextBox.Rows = 5
                TextBox.TextMode = TextBoxMode.MultiLine
              Else
                TextBox.TextMode = TextBoxMode.SingleLine
              End If
              TextBox.Columns = 30
              TextBox.Text = Attribute.DefaultValue
              TextBox.ID = Attribute.Name
              If IsPostBack Then
                TextBox.Text = Request.Form.Get(Attribute.Name)
                If PageIsValid Then
                  User.Attribute(Attribute.Name) = Request.Form.Get(Attribute.Name)
                End If
              Else
                TextBox.Text = CStr(User.Attribute(Attribute.Name))
              End If
              Row.Cells(1).Controls.Add(New LiteralControl(ControlToText(TextBox)))
              table2.Rows.Add(Row)
            Next
          End If
        End If
      End If
    Next
  End Sub

  Public Function LogIn(ByVal UserName As String, ByVal Password As String) As Boolean
    If ValidateUser(UserName, Password) Then
      Authentication.LogIn(UserName, Password)
      If String.IsNullOrEmpty(ReturnUrl) Then
        ReturnUrl = Href(Setting.Name, False, "default.aspx")
      End If

      If Action = LogActionType.NewUser AndAlso AuthenticationIsRequired() Then
        'New user pay for iscription & autentication
        Response.Redirect(PayLinkForAuthentication(ReturnUrl), True)
      End If

      SetNextMessage(Phrase(Setting.Language, 34) & " " & ChrW(9787)) 'Welcome + smile

      'Note: The redirect 207 resolve the issue with a Chrome browser cache after redirect: http://cmsaspnet.com/forum.aspx?f=202&t=59&p=1
      Response.Status = "307 Temporary Redirect"
      Response.AddHeader("Location", ReturnUrl)
      Response.End()
      'Response.Redirect(ReturnUrl, True)
      Return True
    End If
    Return False
  End Function

  Public Function ExistUser(ByVal UserName As String) As Boolean
    Dim File As String = Server.MapPath(Config.UsersSubDirectory) & "\" & UserName & ".xml"
    Return System.IO.File.Exists(File)
  End Function

  Private Sub CustomValidator1_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator1.ServerValidate
    args.IsValid = IsValidUsername(args.Value)
  End Sub
  Public PageIsValid As Boolean
  Private Sub Submit()
    Dim MasterPage As Components.MasterPageEnhanced = CType(Page.Master, Components.MasterPageEnhanced)
    If PageValidation() Then
      PageIsValid = True
      Dim User As Authentication.User
      Select Case Action
        Case LogActionType.NewUser, LogActionType.EditUser
          If Action = LogActionType.NewUser Then
            'Anti robot
            If DateDiff(DateInterval.Second, CDate(Session("TimePageLoaded")), Now) < 20 Then
              MasterPage.AddMessage(Phrase(Setting.Language, 422, 3245), Setting)
              LayoutNew()
              Exit Sub
            End If

            User = New Authentication.User
            'Is a New User
            User.Username = username.Value
            ContainCensoredWord(User.Username) 'Block te user if Usarname contain a spam message
            User.GeneralRole = Authentication.User.RoleType.User
            User.Role(Setting.Name) = Authentication.User.RoleType.User
            User.SubSite = CurrentSubSiteName()
            CurrentUser = User
          Else
            User = CurrentUser
          End If
          User.FirstName = FirstName.Value
          User.LastName = LastName.Value
          User.Password = Password.Value
          User.Phone = phone.Value
          User.Country = country.Value
          User.City = city.Value
          User.URL = Url.Value
          If F.Checked Then
            User.Gender = Authentication.User.GenderType.Female
          Else
            If M.Checked Then
              User.Gender = Authentication.User.GenderType.Male
            Else
              User.Gender = Authentication.User.GenderType.NotDefinite
            End If
          End If
          User.Language = Setting.Language
          User.Email = Email.Value
          User.Skype = Trim(Skype.Value)
          If Not String.IsNullOrEmpty(Request("Unique_ID")) AndAlso Not Request("Unique_ID").EndsWith("-000000000000}") Then
            User.CodeUser = Request("Unique_ID")
          Else
            User.CodeUser = CodeUser(Request("Unique_ID") & Request("Browser_Datas"))
          End If
          User.SetUser()
          If PersonalPhotoAlbum.Checked Then
            If User.PhotoAlbum Is Nothing Then
              'Make a PhotoAlbum
              Dim RootPhotoAlbum As PhotoAlbum = CType(PhotoManager.PhotoAlbum.Load.GetItem("users"), PhotoManager.PhotoAlbum)
              If RootPhotoAlbum Is Nothing Then
                RootPhotoAlbum = New PhotoAlbum
                RootPhotoAlbum.Name = "users"
                RootPhotoAlbum.AddPermitted = PhotoManager.Permission.Permitted
                RootPhotoAlbum.Deletable = Permission.Permitted
                RootPhotoAlbum.Save()
              End If
              Dim PhotoAlbum As PhotoAlbum = RootPhotoAlbum.CreateSubFotoAlbum(CurrentUser, Setting)
              PhotoAlbum.IsRoot = True
              PhotoAlbum.Editable = PhotoManager.EditablePermission.Author
              PhotoAlbum.AddPermitted = PhotoManager.Permission.Author
              PhotoAlbum.Deletable = PhotoManager.Permission.None
              PhotoAlbum.SubPhotoAlbumsNotCreatable = Not Config.Setup.UserInteract.EnabledUsersToMakeSubPersonalPhotoAlbum
              PhotoAlbum.Title(Setting.Language) = Phrase(Setting.Language, 3224)
              PhotoAlbum.Description(Setting.Language) = User.Username
              PhotoAlbum.Save()
              User.NamePhotoAlbum = PhotoAlbum.Name
            End If
          Else
            'Destroy PhotoAlbum
            Dim UserPhotoAlbum As PhotoAlbum = User.PhotoAlbum
            If Not UserPhotoAlbum Is Nothing Then
              UserPhotoAlbum.Delete(CurrentUser, Setting)
              User.NamePhotoAlbum = Nothing
            End If
          End If

          If Action = LogActionType.NewUser Then
            SendEmailToUser(User, Setting)
            Extension.Log("new_users", 1000, Setting.Name, User.Username)

            'The first user registered is a Supervisor
            If IsFirstRunning(Setting) = TypeOfFirstRunning.Application Then
              User.GeneralRole = Authentication.User.RoleType.Supervisor
              If Not String.IsNullOrEmpty(OEM.SoundAtFirstStartup) Then
                Dim Skin As Config.Skin = Setting.Skin()
                Skin.SkinSetup().Variables.Remove("bgsound")
                Config.Skin.CacheSkin.Clear()
              End If
              If Not String.IsNullOrEmpty(OEM.DefaultSiteConfiguration) Then
                CurrentDomainConfiguration.AddSubSite(OEM.DefaultSiteConfiguration)
              End If
              If Not String.IsNullOrEmpty(OEM.AfterCreationOfSupervisorAccountGoToPage) Then
                ReturnUrl = OEM.AfterCreationOfSupervisorAccountGoToPage
              Else
                If Not String.IsNullOrEmpty(OEM.DefaultSiteConfiguration) Then
                  ReturnUrl = "default.aspx"
                Else
                  ReturnUrl = "setup.aspx"
                End If
              End If
            End If
          End If

          User.Save()
          AddExtraUserAttribute() 'Save the extra data configured with plugin
          LayoutLoged()
          LogIn(username.Value, Password.Value)
        Case LogActionType.LogOn
          LogIn(username.Value, Password.Value)
          LayoutLoged()
        Case LogActionType.LostPassword
          If Not String.IsNullOrEmpty(username.Value) Then
            User = Authentication.User.Load(username.Value)
            If User IsNot Nothing Then
              If Session("LostPasswordEmailDelivered") Is Nothing Then
                Session("LostPasswordEmailDelivered") = True
                SendEmailToUser(User, Setting, True)
              End If
              MasterPage.AddMessage(Phrase(Setting.Language, 323), Setting)
            Else
              MasterPage.AddMessage(Phrase(Setting.Language, 322), Setting)
            End If
          Else
            MasterPage.AddMessage(Phrase(Setting.Language, 9, 26), Setting)
          End If
          LayoutLostPassword()
      End Select
    Else
      MasterPage.AddMessage(Phrase(Setting.Language, 27), Setting)
      Select Case Action
        Case LogActionType.LogOn
          Dim ButtonLostPassword As Control = Components.Button(Setting, Phrase(Setting.Language, 321), HrefLog(LogActionType.LostPassword), Nothing, IconType.Help, Nothing, True)
          'MessagePlaceHolder.Controls.Add(Components.ButtonLostPassword)
          Dim Ctrl As Control = table2.Rows(1).Cells(1)
          Ctrl.Controls.Add(BR)
          Ctrl.Controls.Add(ButtonLostPassword)
          MasterPage.Suggest(ButtonLostPassword)
          LayoutLogin()
        Case LogActionType.NewUser
          LayoutNew()
        Case LogActionType.EditUser
          LayoutEdit()
        Case LogActionType.LostPassword
          LayoutLostPassword()
      End Select
    End If
  End Sub
	'    Enum ActionType
	'        LogOn
	'        LogOff
	'        NewUser
	'        EditUser
	'    End Enum

	Private Sub CustomValidator2_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator2.ServerValidate
		If Action = LogActionType.LogOn Then
			args.IsValid = ValidateUser(username.Value, Password.Value)
		Else
			args.IsValid = True
		End If
	End Sub

	Private Sub CustomValidator3_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator3.ServerValidate
    If Action = LogActionType.NewUser AndAlso Not String.IsNullOrEmpty(username.Value) Then
      args.IsValid = Not ExistUser(username.Value)
    Else
      args.IsValid = True
    End If
	End Sub

	Private Sub Customvalidator4_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles Customvalidator4.ServerValidate
		args.IsValid = args.Value.Length <= username.MaxLength
	End Sub

	Protected Sub CustomValidator5_ServerValidate(ByVal source As Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator5.ServerValidate
    args.IsValid = WebApplication.Skype.IsValidSkypeUserName(args.Value)
	End Sub

	Function PageValidation() As Boolean
		If Action <> LogActionType.EditUser Then
			If Not CustomValidator1.IsValid OrElse Not Customvalidator4.IsValid OrElse Not RequiredFieldValidator2.IsValid OrElse Not CustomValidator2.IsValid OrElse Not CustomValidator3.IsValid Then
				Return False
			End If
		End If
		If Action <> LogActionType.LostPassword AndAlso Action <> LogActionType.EditUser Then
			If Not RequiredFieldValidator1.IsValid Then
				Return False
			End If
		End If
		Select Case Action
			Case LogActionType.EditUser, LogActionType.NewUser
				If Not CompareValidator1.IsValid OrElse Not RequiredFieldValidator4.IsValid Then
					Return False
				End If
				If Not RequiredFieldValidator5.IsValid OrElse Not RequiredFieldValidator6.IsValid OrElse Not RegularExpressionValidator2.IsValid OrElse Not RegularExpressionValidator3.IsValid OrElse Not RequiredFieldValidator8.IsValid OrElse Not CustomValidator5.IsValid Then
					Return False
				End If
		End Select
		Return True
	End Function

	Protected Sub Page_PreRender(sender As Object, e As System.EventArgs) Handles Me.PreRender
    Session("TimePageLoaded") = Now
    If table2.Visible Then
      Page.Header.Controls.Add(New LiteralControl("<style type=""text/css"">.leftcoll{text-align:right;white-space:nowrap;}.rightcoll{text-align:left;white-space:nowrap;}</style>"))
    End If
	End Sub
End Class
