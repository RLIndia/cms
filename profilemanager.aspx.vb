Imports WebApplication

Partial Class ProfileManager
  Inherits System.Web.UI.Page
  Private Setting As subsite
  Private UserProfile As User

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Setting = CurrentSetting()
    If Not Request("u") Is Nothing Then
			UserProfile = Authentication.User.Load(Request("u"))
    End If
  End Sub

  Protected Sub Blank_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
    Dim MasterPage As Components.MasterPageEnhanced = SetMasterPage(Me, Nothing, False, True)
    'MasterPage.TitleDocument = Phrase(Setting.Language, 3288)
    'MasterPage.Description = Phrase(Setting.Language, 3287)
    'MasterPage.KeyWords = Phrase(Setting.Language, 3287)

    If UserProfile IsNot Nothing Then
      If CurrentUser(Session).Role(Setting.Name) >= Authentication.User.RoleType.WebMaster Then
        Panel1.Visible = True
        PlaceHolder1.Controls.Add(IconUnicode(IconName.MalteseCross))
        PlaceHolder2.Controls.Add(IconUnicode(IconName.Biohazard))
        PlaceHolder3.Controls.Add(IconUnicode(IconName.YinYang))
        PlaceHolder4.Controls.Add(IconUnicode(IconName.Peace))
        Medal1.Text = CStr(UserProfile.Honors.Medal1)
        Medal2.Text = CStr(UserProfile.Honors.Medal2)
        Medal3.Text = CStr(UserProfile.Honors.Medal3)
        Medal4.Text = CStr(UserProfile.Honors.Medal4)
      End If
    End If

  End Sub

  Protected Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
    UserProfile.Honors.Medal1 = IfInt(Val(Medal1.Text) <= 5, ValInt(Medal1.Text), 5)
    UserProfile.Honors.Medal2 = IfInt(Val(Medal2.Text) <= 5, ValInt(Medal2.Text), 5)
    UserProfile.Honors.Medal3 = IfInt(Val(Medal3.Text) <= 5, ValInt(Medal3.Text), 5)
    UserProfile.Honors.Medal4 = IfInt(Val(Medal4.Text) <= 5, ValInt(Medal4.Text), 5)
    UserProfile.Save()
    Dim MasterPage As Components.MasterPageEnhanced = CType(Page.Master, Components.MasterPageEnhanced) : MasterPage.AddMessage(Phrase(Setting.Language, 1332), Setting)
  End Sub
End Class
