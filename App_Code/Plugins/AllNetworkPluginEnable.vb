'By Andrea Bruno
Namespace WebApplication.Plugin		'Standard namespace obbligatory for all plugins
  Public Class AllNetworkPluginEnabler
    Public Shared WithEvents Plugin As PluginManager.Plugin = Initialize()
    Shared Function Initialize() As PluginManager.Plugin
      If Plugin Is Nothing Then Plugin = New PluginManager.Plugin(AddressOf Description, Authentication.User.RoleType.Supervisor, False, False, PluginManager.Plugin.Characteristics.AlwaysEnabled, Nothing, GetType(SharedConfiguration))
      Return Plugin
    End Function
    Shared Sub New()
      Initialize()
    End Sub

    Private Shared Function Description(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
      Select Case Language
        Case LanguageManager.Language.Italian
          If ShortDescription Then
            Return "Attiva/disattiva plugin"
          Else
            Return "Questo plugin ti permette di attivare o disattivare ogni plugin in tutti i siti web"
          End If
        Case Else
          If ShortDescription Then
            Return "Enable/disable plugin"
          Else
            Return "This plugin allows you to enable or disable each plugin in all websites"
          End If
      End Select
    End Function

    Class SharedConfiguration
      Inherits DynamicObjectSerializabled
      Sub New()
        SetDefaultValues()
      End Sub
      Sub SetDefaultValues()
        ResetMembers()
        Dim SkinSetup As Config.SkinSetup = Config.SkinSetup.Load(CurrentSetting.Skin().Setups, True)
        For Each Plugin As PluginManager.Plugin In AllPlugins()
          If Plugin.Characteristic <> PluginManager.Plugin.Characteristics.AlwaysEnabled AndAlso Plugin.Characteristic <> PluginManager.Plugin.Characteristics.CorePlugin Then
            Me(Plugin.Name) = Action.NoAction
          End If
        Next
      End Sub
    End Class

    Enum Action
      NoAction
      EnableThisPluginInAllWebsites
      DisableThisPluginInAllWebsites
    End Enum

    Private Shared Sub Plugin_BeforeEditPluginSharedConfiguration(Configuration As Object, SetTablePropertyPhraseCorrispondence As StringDictionary) Handles Plugin.BeforeEditPluginSharedConfiguration
      Dim Language As LanguageManager.Language = CurrentSetting.Language
      SetTablePropertyPhraseCorrispondence = New StringDictionary
      For Each Member In CType(Configuration, SharedConfiguration).MembersArray
        Dim Plugin = PluginManager.GetPlugin(Member.Name)
        If Plugin IsNot Nothing Then
          SetTablePropertyPhraseCorrispondence.Add(Plugin.Name, Plugin.Description(Language))
        End If
      Next
    End Sub
 
    Private Shared Sub Plugin_BeforeSavePluginSharedConfiguration(Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean) Handles Plugin.BeforeSavePluginSharedConfiguration
      Dim Settings As Collections.Generic.List(Of SubSite) = AllSubSite()
      For Each Member In CType(Configuration, SharedConfiguration).MembersArray
        Dim Plugin = PluginManager.GetPlugin(Member.Name)
        If Plugin IsNot Nothing AndAlso Plugin.Characteristic <> PluginManager.Plugin.Characteristics.AlwaysEnabled AndAlso Plugin.Characteristic <> PluginManager.Plugin.Characteristics.CorePlugin Then
          Dim ActionToExecuting As Action = CType([Enum].Parse(GetType(Action), Member.Value), Action)
          For Each Setting As Config.SubSite In Settings
            Select Case ActionToExecuting
              Case Action.DisableThisPluginInAllWebsites
                Plugin.IsEnabled(Setting) = False
                Plugin.SaveAttribute(Setting)
              Case Action.EnableThisPluginInAllWebsites
                Plugin.IsEnabled(Setting) = True
                Plugin.SaveAttribute(Setting)
            End Select
            Member.Value = CStr(Action.NoAction)
          Next
        End If
      Next
      Configuration = Nothing
    End Sub
  End Class

End Namespace