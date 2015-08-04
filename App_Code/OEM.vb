Namespace WebApplication
  Public Class OEM
    'OEM customization: 
    Public Const DefaultSkin As String = "Win8" 'Is case sensitive and the first char must be uppercase! Ex.: "Default", "Best Skyn", "Win8".
    Public Const DefaultSiteConfiguration As String = Nothing 'Low Case
    Public Const VideoAtFirstStartup As String = Nothing 'ID video. Ex.: ""PBvPQ9aukuU""
    Public Const SkinAtFirstStartup As String = "Win8" 'Is case sensitive and the first char must be uppercase! Ex.: "Default", "Best Skyn".
    Public Const SoundAtFirstStartup As String = "sac14.mod" '.mod file run when create a supervisor account
    Public Const AfterCreationOfSupervisorAccountGoToPage As String = Nothing 'Ex.: "default.aspx", "forum.aspx"
    Public Const DefaultTypeOfForum As Forum.TypeOfForum = Forum.TypeOfForum.Forum
    Shared Sub RunAtFirsStartup(Setting As SubSite, MasterPage As Components.MasterPageEnhanced)
      'Add here the code to run at first startup
      If Now.Month >= 11 OrElse Now.Month <= 1 Then
        PluginManager.GetPlugin("Snowfall").IsEnabled(Setting) = True
      End If
      'Setting.Language = Language.Italian
    End Sub
  End Class
End Namespace