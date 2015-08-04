Imports WebApplication
Partial Class sms
  Inherits System.Web.UI.Page
	Shared WithEvents Plugin As PluginManager.Plugin = Initialize()
	Shared Function Initialize() As PluginManager.Plugin
    If Plugin Is Nothing Then Plugin = New PluginManager.Plugin(AddressOf Description, Authentication.User.RoleType.Visitors, True)
		Return Plugin
	End Function
	Shared Sub New()
		Initialize()
	End Sub


  Protected Shared Sub Plugin_MasterPagePreRender(ByVal MasterPage As Components.MasterPageEnhanced) Handles Plugin.MasterPagePreRender
    If MasterPage.ShowTopBar Then
      Dim Setting As SubSite = MasterPage.Setting
      'Add link accommodation
      If Plugin.IsEnabledAndAccessible(MasterPage.User, Setting) Then

        If CBool(SmsAvailable()) Then
          'Button Send SMS
          MasterPage.AddButton(Phrase(Setting.Language, 3063), Common.Href(Setting.Name, False, "sms.aspx"), Nothing, Components.IconType.Globe)
        End If

      End If
    End If
  End Sub

  Shared Function Description(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
    If ShortDescription Then
      Return Phrase(Language, 3063)
    Else
      Return Phrase(Language, 3064)
    End If
  End Function

  Private Setting As SubSite
  Protected Sub sms_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Setting = CurrentSetting()
    Dim MasterPage As Components.MasterPageEnhanced = SetMasterPage(Me)
    MasterPage.TitleDocument = Phrase(Setting.Language, 3063)
    MasterPage.Description = Phrase(Setting.Language, 3064)
    MasterPage.KeyWords = Phrase(Setting.Language, 3064)
  End Sub

  Protected Sub CustomValidator1_ServerValidate(ByVal source As Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidator1.ServerValidate
    args.IsValid = IsValidSmsPhoneNumber(args.Value)
  End Sub

  Function InfoSms(ByVal PhoneNumber As String, ByVal Text As String) As Control
    Dim Table As HtmlTable = Components.Table(2, 2, HorizontalAlign.Left)
    Table.Rows(0).Cells(0).InnerText = "<b>" & Phrase(Setting.Language, 15) & ":</b>" 'UserName
    Table.Rows(0).Cells(1).InnerText = PhoneNumber
    Table.Rows(1).Cells(0).InnerText = "<b>" & Phrase(Setting.Language, 60) & ":</b>" 'Password
    Table.Rows(1).Cells(1).InnerText = HttpUtility.HtmlEncode(Text)
    Return Table
  End Function

  Protected Sub sms_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
    Dim CurrentUser As User = Authentication.CurrentUser(Session)
    If CurrentUser.Role(Setting.Name) < Authentication.User.RoleType.User Then
      Button1.Enabled = False
    End If
    Dim MasterPage As Components.MasterPageEnhanced = CType(Page.Master, Components.MasterPageEnhanced)
    If SmsAvailable() > 0 Then
      If Page.IsPostBack Then
        If Page.IsValid Then
          Dim PageLoaded As Date = CDate(Session("TimePageLoaded"))
          If DateDiff(DateInterval.Second, PageLoaded, Now) < 10 Then
            MasterPage.AddMessage(Phrase(Setting.Language, 422, 3245), Setting)
          Else
            If SmsManager.Send(CurrentUser, PhoneNumber.Text, SM.Value) = SmsManager.SmsError.None Then
              Info.Visible = False
              Table1.Visible = False
              MasterPage.AddMessage(Phrase(Setting.Language, 418), Setting)
              MasterPage.AddMessage(InfoSms(PhoneNumber.Text, SM.Value))
            Else
              MasterPage.AddMessage(Phrase(Setting.Language, 422), Setting)
            End If
          End If
        Else
          MasterPage.AddMessage(Phrase(Setting.Language, 404), Setting)
        End If
      Else
        Session("TimePageLoaded") = Now
      End If
    Else
      Button1.Enabled = False
      MasterPage.AddMessage(Phrase(Setting.Language, 3062), Setting)
    End If
  End Sub

End Class
