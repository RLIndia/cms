'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Namespace WebApplication
	Public Module PluginManager
		Private Sub LoadPlugins(ByVal Assembly As System.Reflection.[Assembly])
			Static AssemblyCheckedForPlugin As New Collections.Generic.List(Of Integer)
			If Not AssemblyCheckedForPlugin.Contains(Assembly.GetHashCode) Then
				AssemblyCheckedForPlugin.Add(Assembly.GetHashCode)
				'Load plugin in the current assenply
				For Each objType As Type In Assembly.GetTypes
					'Only look at public types
					'Ignore abstract classes
          If Not ((objType.Attributes And System.Reflection.TypeAttributes.Abstract) = System.Reflection.TypeAttributes.Abstract) Then
            If objType.Namespace IsNot Nothing AndAlso objType.Namespace.EndsWith("WebApplication.Plugin") AndAlso Not objType.FullName.Contains("+") Then
              Try
                Dim [Module] As Object = Assembly.CreateInstance(objType.FullName)
              Catch ex As Exception
              End Try
            End If
          End If
				Next
			End If
		End Sub

		Public Sub LoadPlugunsAspx(ByVal Assembly As System.Reflection.[Assembly])
			Static AssemblyCheckedForPlugin As New Collections.Generic.List(Of Integer)
			If Not AssemblyCheckedForPlugin.Contains(Assembly.GetHashCode) Then
				AssemblyCheckedForPlugin.Add(Assembly.GetHashCode)
				'Load plugin in the current assenply
				For Each objType As Type In Assembly.GetTypes
					'Only look at public types
					If objType.IsPublic = True Then
						'Ignore abstract classes
						If Not ((objType.Attributes And System.Reflection.TypeAttributes.Abstract) = System.Reflection.TypeAttributes.Abstract) Then
							If objType.BaseType.FullName = "System.Web.UI.Page" Then
								Try
									Dim Page As Object = Assembly.CreateInstance(objType.FullName)
								Catch ex As Exception

								End Try
							End If
						End If
					End If
				Next
			End If
		End Sub

		Enum NeedAlternativeMethodLoad
			No
			Need
			Run
			Completed
			Failed
		End Enum

		Public UseAlternativeLoadPluginMethod As NeedAlternativeMethodLoad = NeedAlternativeMethodLoad.No

		Sub LoadAllPlugins()
			LoadPlugunsAspx(System.Reflection.Assembly.GetExecutingAssembly)
			LoadPlugins(System.Reflection.Assembly.GetExecutingAssembly)
			LoadPluginsFromDll()
		End Sub

		Private Sub LoadPluginsFromDll()
			Dim FileAssembly As System.IO.FileInfo
			Try
				FileAssembly = New System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly.Location)
			Catch ex As Exception
				'Trust policy don't give access to assembly directoty, then load plgin by a second algoritm!
				UseAlternativeLoadPluginMethod = NeedAlternativeMethodLoad.Need
				Exit Sub
			End Try
			Dim Dir As String = FileAssembly.DirectoryName
			Dim Assembly As System.Reflection.[Assembly]
			For Each File As String In System.IO.Directory.GetFiles(Dir, "*.dll")
				Try
					Assembly = System.Reflection.[Assembly].LoadFrom(File)
				Catch ex As Exception
					Assembly = Nothing
				End Try
				If Assembly IsNot Nothing Then
					'Loop through each type in the DLL
					LoadPlugunsAspx(Assembly)
					LoadPlugins(Assembly)
				End If
			Next
		End Sub

    Sub LoadPluginsByAspxFileWebRequest(Base As Object)
      Try
        Dim Aspxs As String() = IO.Directory.GetFiles(MapPath(""), "*.aspx")
        For Each FullFileName As String In Aspxs
          Dim File As String = LCase(GetFileName(FullFileName))
          For Each Plugin As Plugin In AllPlugins()
            If LCase(Plugin.AspxFileName) = File Then
              File = Nothing
              Exit For
            End If
          Next
          If File IsNot Nothing Then
            If IsAspxPlugin(FullFileName) Then
              Dim WebRequest As System.Net.HttpWebRequest
              WebRequest = CType(System.Net.HttpWebRequest.Create(CStr(Base) & File), Net.HttpWebRequest)
              WebRequest.Timeout = 60000
              Try
                WebRequest.GetResponse()
              Catch ex As Exception
                'Time Out error
              End Try
            End If
          End If
        Next
        UseAlternativeLoadPluginMethod = NeedAlternativeMethodLoad.Completed
      Catch ex As Exception
        UseAlternativeLoadPluginMethod = NeedAlternativeMethodLoad.Failed
      End Try
    End Sub

		Function IsAspxPlugin(ByVal AspxFullFileName As String) As Boolean
			Try
				Dim File As String = AspxFullFileName & ".vb"
        If Not IO.File.Exists(File) Then
          File = AspxFullFileName & ".cs"
        End If
        If Not IO.File.Exists(File) Then
          File = AspxFullFileName
        End If
				Dim Code As String = ReadAll(File)
        If CBool(InStr(Code, "PluginManager.Plugin", CompareMethod.Text)) Then
          Return True
        End If
			Catch ex As Exception
				Return True
			End Try
      Return False
    End Function

		Private Plugins As New Collections.Generic.Dictionary(Of String, Plugin)

		Function Exists(ByVal PluginName As String) As Boolean
			Return Plugins.ContainsKey(PluginName)
		End Function

    Function CurrentPluginRunning() As Boolean
      Dim Thread As System.Threading.Thread = Threading.Thread.CurrentThread
      Dim StackTrace As System.Diagnostics.StackTrace = New System.Diagnostics.StackTrace(Thread, False)
      Dim PluginName As String = StackTrace.GetFrame(1).GetMethod.DeclaringType.Name
      Return Plugins.ContainsKey(PluginName)
    End Function

		Function AllPlugins() As Collections.Generic.List(Of Plugin)
			SyncLock Plugins
				Dim Collection As New Collections.Generic.List(Of Plugin)
				For Each Plugin As Plugin In Plugins.Values
					Collection.Add(Plugin)
				Next
				Return Collection
			End SyncLock
		End Function

		Function AddPlugin(ByVal Plugin As Plugin) As Boolean
			SyncLock Plugins
				If Not Plugins.ContainsKey(Plugin.Name) Then
					Plugins.Add(Plugin.Name, Plugin)
					Return True
				End If
			End SyncLock
      Return False
    End Function

		Function GetPlugin(ByVal Name As String) As Plugin
			SyncLock Plugins
				If Plugins.ContainsKey(Name) Then
					Return Plugins(Name)
				End If
			End SyncLock
      Return Nothing
    End Function

		Property IsEnabled(ByVal Setting As SubSite, ByVal Plugin As String) As Boolean
			Get
				If Plugins.ContainsKey(Plugin) Then
					Return Plugins(Plugin).IsEnabled(Setting)
				End If
        Return False
      End Get
			Set(ByVal value As Boolean)
				If Plugins.ContainsKey(Plugin) Then
          Plugins(Plugin).IsEnabled(Setting) = value
				End If
			End Set
		End Property

		Property Attribute(ByVal Setting As SubSite, ByVal Plugin As String, ByVal AttributeName As String) As Object
			Get
				If Plugins.ContainsKey(Plugin) Then
					Return Plugins(Plugin).Attribute(AttributeName, Setting)
				End If
        Return Nothing
      End Get
			Set(ByVal value As Object)
				If Plugins.ContainsKey(Plugin) Then
					Plugins(Plugin).Attribute(AttributeName, Setting) = value
				End If
			End Set
		End Property

		Class Plugin
			Inherits PluginEditor
			Private PluginName As String
      'Is the name of plugin
      Overrides ReadOnly Property Name As String
        Get
          Return PluginName
        End Get
      End Property


      'Overrides Function Name() As String
      '  Return PluginName
      'End Function

      'Function Name(Optional ByVal Language As LanguageManager.Language = LanguageManager.Language.NotDefinite) As String
      'Is the name show to the users by language
      'End Function																							 

      Function IsAccessible(ByVal User As User, ByVal Setting As SubSite) As Boolean
        'Return if the user have access to this plugin
        Return IsAccessible(User.Role(Setting.Name))
      End Function

      Function IsAccessible(Role As User.RoleType) As Boolean
        'Return if the user have access to this plugin
        Return Role >= MinimumRoleRequired
      End Function

      Function IsEnabledAndAccessible(Role As User.RoleType, ByVal Setting As SubSite) As Boolean
        Return IsAccessible(Role) AndAlso IsEnabled(Setting) 
      End Function

      Function IsEnabledAndAccessible(ByVal User As User, ByVal Setting As SubSite) As Boolean
        Return IsAccessible(User, Setting) AndAlso IsEnabled(Setting)
      End Function

      Private CacheObject As New Collection
      Private CacheSharedObject As Object


      Overrides Function LoadObject(ByVal Type As System.Type, Optional ByVal SubSiteName As String = Nothing) As Object
        Dim Key As String
        Dim Obj As Object = Nothing
        If SubSiteName Is Nothing AndAlso Type = Me.TypeObject(CInt(ConfigurationType.SharedConfiguration)) Then
          'Is the shared object
          If CacheSharedObject IsNot Nothing Then
            Return CacheSharedObject
          End If
          Key = "Shared Configuration"
          Obj = Persistent.LoadObject(Type, Key)
          If Obj Is Nothing Then Obj = Activator.CreateInstance(Type, True)
          CacheSharedObject = Obj
        Else
          SyncLock CacheObject
            If SubSiteName IsNot Nothing Then
              Key = SubSiteName
            Else
              Key = CurrentSubSiteName()
            End If

            Dim CacheKey As String = Type.FullName & " " & Key
            If CacheObject.Contains(CacheKey) Then
              Return CacheObject.Item(CacheKey)
            End If
            Obj = Persistent.LoadObject(Type, Key)
            If Obj Is Nothing Then Obj = Activator.CreateInstance(Type)
            CacheObject.Add(Obj, CacheKey)
          End SyncLock
        End If
        Return Obj
      End Function

      Overrides Sub SaveObject(ByVal Obj As Object, Optional ByVal SubSiteName As String = Nothing)
        Dim Key As String
        If SubSiteName Is Nothing AndAlso Obj.GetType = Me.TypeObject(CInt(ConfigurationType.SharedConfiguration)) Then
          'Is the shared object
          Key = "Shared Configuration"
          Persistent.SaveObject(Obj, Key)
          CacheSharedObject = Obj
          Pipeline.NotifyChangement(Obj.GetType, "")
        Else
          SyncLock CacheObject
            If SubSiteName IsNot Nothing Then
              Key = SubSiteName
            Else
              Key = CurrentSubSiteName()
            End If
            Persistent.SaveObject(Obj, Key)
            Dim CacheKey As String = Obj.GetType.FullName & " " & Key
            If CacheObject.Contains(CacheKey) Then
              CacheObject.Remove(CacheKey)
            End If
            CacheObject.Add(Obj, CacheKey)
          End SyncLock
          Pipeline.NotifyChangement(Obj.GetType, Key)
        End If
      End Sub

      Property Attribute(ByVal Name As String, ByVal Setting As SubSite) As Object
        Get
          'Return if this plugin is enabled for the specify setting 
          Return Setting.Attribute(Name, PluginName)
        End Get
        Set(ByVal value As Object)
          'On/Off this plugin for the specify setting
          Setting.Attribute(Name, PluginName) = value
        End Set
      End Property

      Sub SaveAttribute(ByVal Setting As SubSite)
        Setting.Save()
      End Sub

      Public Characteristic As Characteristics

      Enum Characteristics
        StandardPlugin
        CorePlugin          'Core Plugin are not listed in configuration page setup.aspx
        EnabledByDefault
        AlwaysEnabled
      End Enum

      Property IsEnabled(ByVal Setting As SubSite) As Boolean
        Get
          'Return if this plugin is enabled for the specify setting 
          If Characteristic = Characteristics.AlwaysEnabled Then
            Return True
          End If
          Dim Result As Object = Setting.Attribute("Enabled", PluginName)
          If Result IsNot Nothing Then
            Return CBool(Result)
          Else
            If Characteristic <> Characteristics.StandardPlugin Then
              Return True
            End If
          End If
          Return False
        End Get
        Set(ByVal value As Boolean)
          'On/Off this plugin for the specify setting
          If Characteristic <> Characteristics.AlwaysEnabled Then
            Dim CurrentValue As Object = Setting.Attribute("Enabled", PluginName)
            If CurrentValue Is Nothing AndAlso Characteristic <> Characteristics.StandardPlugin Then
              CurrentValue = True
            End If
            If CBool(CurrentValue) <> value Then
              Setting.Attribute("Enabled", PluginName) = value
              If OnEnabledStatusChangedEvent IsNot Nothing Then
                Me.OnEnabledStatusChangedEvent(Setting, value)
              End If
              Refresh(True)
            End If
          End If
        End Set
      End Property
      Private MinimumRoleRequired As User.RoleType
      Public AspxFileName As String

      Public Delegate Function RetriveDescription(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
      Private m_Description As RetriveDescription
      ReadOnly Property Description(ByVal Language As LanguageManager.Language, Optional ByVal ShortDescription As Boolean = True) As String
        Get
          If m_Description IsNot Nothing Then
            Return m_Description(Language, ShortDescription)
          Else
            Return JoinedTextToSpecedText(PluginName)
          End If
        End Get
      End Property
      Public AddItemToMenu As Boolean
      Public SelectableAsHomePage As Boolean

      Public Event Delivery(ByVal IdOrder As String, ByVal EmailToVerify As String, ByRef ReturnSuccessful As Boolean)
      Friend Sub RaiseDelivery(ByVal IdOrder As String, ByVal EmailToVerify As String, ByRef ReturnSuccessful As Boolean)
        RaiseEvent Delivery(IdOrder, EmailToVerify, ReturnSuccessful)
      End Sub

      Event RobotsTxtPreRender(ByRef Text As String, DomainConfiguration As Config.DomainConfiguration)
      Friend Shared Sub RaiseRobotsTxtPreRenderEvent(ByRef Text As String, DomainConfiguration As DomainConfiguration)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.RobotsTxtPreRenderEvent IsNot Nothing Then
            For Each Setting As SubSite In DomainConfiguration.SubSites()
              If Plugin.IsEnabledAndAccessible(Authentication.User.RoleType.Supervisor, Setting) Then
                Plugin.RobotsTxtPreRenderEvent(Text, DomainConfiguration)
                Exit For
              End If
            Next
          End If
        Next
      End Sub

      Event TimerElapsed(ByRef SetNextElapsedInterval As Integer, ByRef SetTimerEnabled As Boolean)
      Private WithEvents Timer As Timers.Timer
      Private Sub Timer_Elapsed(sender As Object, e As Timers.ElapsedEventArgs) Handles Timer.Elapsed
        If TimerElapsedEvent IsNot Nothing Then
          Dim Interval As Integer = CInt(Timer.Interval)
          Dim Enabled As Boolean = Timer.Enabled
          TimerElapsedEvent(Interval, Enabled)
          If CInt(Timer.Interval) <> Interval Then Timer.Interval = CDbl(Interval)
          If Timer.Enabled <> Enabled Then Timer.Enabled = Enabled
        End If
      End Sub

      Event MasterPagePreRender(ByVal MasterPage As Components.MasterPageEnhanced)
      Friend Shared Sub RaiseMasterPagePreRenderEvent(ByVal MasterPage As Components.MasterPageEnhanced, ByVal User As User, ByVal Setting As SubSite)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.MasterPagePreRenderEvent IsNot Nothing Then
            If Plugin.IsEnabledAndAccessible(User, Setting) Then
              Plugin.MasterPagePreRenderEvent(MasterPage)
            End If
          End If
        Next
      End Sub

      Public Event MasterPageInit(ByVal MasterPage As Components.MasterPageEnhanced)
      Friend Shared Sub RaiseMasterPageInitEvent(ByVal MasterPage As Components.MasterPageEnhanced, ByVal User As User, ByVal Setting As SubSite)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.MasterPageInitEvent IsNot Nothing Then
            If Plugin.IsEnabledAndAccessible(User, Setting) Then
              Plugin.MasterPageInitEvent(MasterPage)
            End If
          End If
        Next
      End Sub

      Public Event OnEnabledStatusChanged(SubSite As SubSite, ByVal Enabled As Boolean)
      Friend Shared Sub RaiseOnEnabledStatusChanged(Plugin As Plugin)
        For Each Setting As SubSite In AllSubSite()
          If Plugin.OnEnabledStatusChangedEvent IsNot Nothing Then
            Plugin.OnEnabledStatusChangedEvent(Setting, Plugin.IsEnabled(Setting))
          End If
        Next
      End Sub

      'Event BeforeSaveObject(ByVal Obj As Object, ByVal Key As String)

      Event Ipn(ByVal IdOrder As String, ByVal Request As HttpRequest)
      Friend Shared Sub RaiseIpnEvent(ByVal PluginName As String, ByVal IdOrder As String, ByVal Request As HttpRequest)
        Dim RequiredPlugin As Plugin = GetPlugin(PluginName)
        RequiredPlugin.IpnEvent(IdOrder, Request)
      End Sub

      Event OnSitemapGeneration(ByVal Type As TypeOfSitemap, ByVal Setting As SubSite, ByVal SiteMap As System.Xml.XmlTextWriter, ByVal Domain As String, ByVal Exclude As StringCollection, ByVal www As Boolean)
      Friend Shared Sub RaiseOnSitemapGenerationEvent(ByVal Type As TypeOfSitemap, ByVal Setting As SubSite, ByVal SiteMap As System.Xml.XmlTextWriter, ByVal Domain As String, ByVal Exclude As StringCollection, ByVal www As Boolean)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.OnSitemapGenerationEvent IsNot Nothing Then
            If Plugin.IsEnabled(Setting) Then
              Plugin.OnSitemapGenerationEvent(Type, Setting, SiteMap, Domain, Exclude, www)
            End If
          End If
        Next
      End Sub

      Event DailyOperation()
      Friend Shared Sub RaiseDailyOperationEvent()
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.DailyOperationEvent IsNot Nothing Then
            'Fire all Daily Operation for plugins
            For Each SubSite As SubSite In AllSubSite()
              If Plugin.IsEnabled(SubSite) Then
                Plugin.DailyOperationEvent()
                Exit For
              End If
            Next
          End If
        Next
      End Sub

      Event ApplicationEnd()
      Friend Shared Sub RaiseApplicationEndEvent()
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.ApplicationEndEvent IsNot Nothing Then
            Plugin.ApplicationEndEvent()
          End If
        Next
      End Sub

      Event ChatMessageWrited(ByVal ChatRoom As String, ByVal Text As String, ByVal Author As String, ByVal Setting As SubSite)
      Friend Shared Sub RaiseChatMessageWritedEvent(ByVal ChatRoom As String, ByVal Text As String, ByVal Author As String, ByVal Setting As SubSite)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.ChatMessageWritedEvent IsNot Nothing Then
            'Fire events for enabled plugins
            If Plugin.IsEnabled(Setting) Then
              Plugin.ChatMessageWritedEvent(ChatRoom, Text, Author, Setting)
            End If
          End If
        Next
      End Sub

      Event ForumReplyPosted(Forum As Forum, ByVal Reply As Reply, ForumPostType As ForumPostType, ByVal Setting As SubSite)
      Friend Shared Sub RaiseForumReplyPostedEvent(Forum As Forum, ByVal Reply As Reply, ForumPostType As ForumPostType, ByVal Setting As SubSite)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.ForumReplyPostedEvent IsNot Nothing Then
            'Fire events for enabled plugins
            If Plugin.IsEnabled(Setting) Then
              Plugin.ForumReplyPostedEvent(Forum, Reply, ForumPostType, Setting)
            End If
          End If
        Next
      End Sub

      Event RenderingPage(ByVal Archive As Integer, ByVal Page As Integer, ByVal Setting As SubSite, ByVal HttpContext As HttpContext, ByVal Where As Control, ByVal MetaTags As MetaTags, ByVal Body As String, ByVal BottomContent As HtmlControl)
      Friend Shared Sub RaiseRenderingPageEvent(ByVal Archive As Integer, ByVal Page As Integer, ByVal Setting As SubSite, ByVal HttpContext As HttpContext, ByVal Where As Control, ByVal MetaTags As MetaTags, ByVal Body As String, ByVal BottomContent As HtmlControl)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.RenderingPageEvent IsNot Nothing Then
            'Fire events for enabled plugins
            If Plugin.IsEnabled(Setting) Then
              Plugin.RenderingPageEvent(Archive, Page, Setting, HttpContext, Where, MetaTags, Body, BottomContent)
            End If
          End If
        Next
      End Sub

      Event GetSkinSetup(ByRef SetSkinSetup As SkinSetup, Setting As SubSite)
      Friend Shared Sub RaiseGetSkinSetupEvent(ByRef SetSkinSetup As SkinSetup, ByVal Setting As SubSite)
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.GetSkinSetupEvent IsNot Nothing Then
            'Fire events for enabled plugins
            If Plugin.IsEnabled(Setting) Then
              Plugin.GetSkinSetupEvent(SetSkinSetup, Setting)
            End If
          End If
        Next
      End Sub

      Public Shared Function UseSkinCustomization(Setting As SubSite) As Boolean
        For Each Plugin As Plugin In AllPlugins()
          If Plugin.GetSkinSetupEvent IsNot Nothing Then
            'Fire events for enabled plugins
            If Plugin.IsEnabled(Setting) Then
              Return True
            End If
          End If
        Next
        Return False
      End Function

      Friend Overrides Sub Refresh(Optional AnyCase As Boolean = False)
        Dim Setting As SubSite = CurrentSetting()
        If Me.GetSkinSetupEvent IsNot Nothing Then
          'Fire events for enabled plugins
          If AnyCase OrElse Me.IsEnabled(Setting) Then
            Skin.RemoveSkinFromCache("!" & Setting.Name)
            If Skin.SkinSetups.ContainsKey(Setting.IdSkin) Then
              Skin.SkinSetups.Remove(Setting.IdSkin)
            End If
          End If
        End If
      End Sub

      Function Directory() As String
        Static Result As String
        If Result Is Nothing Then
          Result = MapPath(PluginsSubDirectory & "\" & Me.Name & "\")
          If Not IO.Directory.Exists(Result) Then
            System.IO.Directory.CreateDirectory(Result)
          End If
        End If
        Return Result
      End Function

      Event Load()

      Delegate Function ExtraUserAttributes() As Collections.Generic.List(Of ExtraUserAttribute)
      Public AddExtraUserAttributes As ExtraUserAttributes

      Public Class ExtraUserAttribute
        Public Name As String
        Public TypeAttribute As TypeOfAttribute
        Public Delegate Function RetriveDescription(ByVal Language As LanguageManager.Language) As String
        Public Description As RetriveDescription
        Public DefaultValue As String
        Public MaxLengt As Integer
        Public Enum TypeOfAttribute
          Text
          TextMultiLine
        End Enum
        Sub New(TypeAttribute As TypeOfAttribute, Name As String, FunctionDescription As RetriveDescription, Optional MaxLengt As Integer = 0, Optional DefaultValue As String = Nothing)
          Me.Name = Name
          Me.TypeAttribute = TypeAttribute
          Me.Description = FunctionDescription
          Me.MaxLengt = MaxLengt
          Me.DefaultValue = DefaultValue
        End Sub
      End Class
      'Function SetExtraUserAttributes(ParamArray ExtraUserAttribute() As ExtraUserAttribute)
      '	ExtraUserAttributes = New Collections.Generic.List(Of ExtraUserAttribute)
      '	For Each Attribute As ExtraUserAttribute In ExtraUserAttribute
      '		ExtraUserAttributes.Add(Attribute)
      '	Next
      'End Function


      Sub New(Optional ByVal Description As RetriveDescription = Nothing, Optional ByVal Accessibility As User.RoleType = User.RoleType.Visitors, Optional ByVal AddItemToMenu As Boolean = False, Optional ByVal SelectableAsHomePage As Boolean = False, Optional ByVal Characteristic As Characteristics = Characteristics.StandardPlugin, Optional ByVal ConfigurationType As Type = Nothing, Optional ByVal SharedConfigurationType As Type = Nothing, Optional AddExtraUserAttributes As ExtraUserAttributes = Nothing, Optional IntervalTimer As Integer = 0)
        Dim Thread As System.Threading.Thread = Threading.Thread.CurrentThread
        Dim StackTrace As System.Diagnostics.StackTrace = New System.Diagnostics.StackTrace(Thread, False)
        PluginName = StackTrace.GetFrame(1).GetMethod.DeclaringType.Name
        If AddPlugin(Me) Then
          If StackTrace.GetFrame(1).GetMethod.DeclaringType.GetInterface("IHttpHandler") IsNot Nothing Then
            'If PluginName.ToLower.EndsWith(".aspx") Then
            'Me.AspxFileName = PluginName.ToLower
            AspxFileName = PluginName.ToLower() & ".aspx"
          End If

          MinimumRoleRequired = Accessibility
          If Description IsNot Nothing Then
            Me.m_Description = Description
            Me.AddItemToMenu = AddItemToMenu
          Else
            Me.AddItemToMenu = False
          End If
          Me.SelectableAsHomePage = SelectableAsHomePage
          Me.Characteristic = Characteristic
          If ConfigurationType IsNot Nothing Then
            AcquiringEditObjectParameter(ConfigurationType, PluginEditor.ConfigurationType.InheritSubSite)
            Pipeline.AddActionForNotification(ConfigurationType, New WebApplication.Pipeline.NotifyChangementDelegate(AddressOf NotifyConfigurationHasChangedFromAnotherServer))
          End If
          If SharedConfigurationType IsNot Nothing Then
            AcquiringEditObjectParameter(SharedConfigurationType, PluginEditor.ConfigurationType.SharedConfiguration)
            Pipeline.AddActionForNotification(SharedConfigurationType, New WebApplication.Pipeline.NotifyChangementDelegate(AddressOf NotifySharedConfigurationHasChangedFromAnotherServer))
          End If

          If CBool(IntervalTimer) Then
            If Not IsLocal() Then
              If Servers.Count = 0 OrElse My.Computer.Name = Servers()(0) Then 'Run only one istance in the main server
                Timer = New Timers.Timer(IntervalTimer)
                Timer.Enabled = True
                Timer.Start()
              End If
            End If
          End If

          Me.AddExtraUserAttributes = AddExtraUserAttributes
          Plugin.RaiseOnEnabledStatusChanged(Me)
          RaiseEvent Load()

        End If
      End Sub

      Private Sub NotifyConfigurationHasChangedFromAnotherServer(ByVal Key As String)
        If String.IsNullOrEmpty(Key) Then CacheSharedObject = Nothing : Exit Sub 'Is changea the shared configuration with a same Type
        SyncLock CacheObject
          If CacheObject.Contains(Key) Then
            CacheObject.Remove(Key)
          End If
        End SyncLock
      End Sub

      Private Sub NotifySharedConfigurationHasChangedFromAnotherServer(ByVal Key As String)
        CacheSharedObject = Nothing
      End Sub
    End Class

    MustInherit Class PluginEditor
      MustOverride ReadOnly Property Name As String
      'MustOverride Function Name() As String
      MustOverride Sub SaveObject(ByVal Obj As Object, Optional ByVal SubSiteName As String = Nothing)
      MustOverride Function LoadObject(ByVal Type As System.Type, Optional ByVal SubSiteName As String = Nothing) As Object
      Friend MustOverride Sub Refresh(Optional AnyCase As Boolean = False)

      Public Function LoadConfiguration() As Object
        Return LoadConfiguration(ConfigurationType.InheritSubSite, Config.CurrentSubSiteName())
      End Function

      Public Function LoadSharedConfiguration() As Object
        Return LoadConfiguration(ConfigurationType.SharedConfiguration)
      End Function

      Private Function LoadConfiguration(ByVal ConfigurationType As ConfigurationType, Optional SubSiteName As String = Nothing) As Object
        Return Me.LoadObject(TypeObject(CInt(ConfigurationType)), SubSiteName)
      End Function

      Delegate Function EditPlugin() As EditObjectParameters
      Public EditObjectParametersForInheritSubSite As EditPlugin
      Public EditObjectParametersForSharedConfiguration As EditPlugin

      Private Function GenerateEditObjectParametersForInheritSubSite() As EditObjectParameters
        Return GenerateEditObjectParameters(ConfigurationType.InheritSubSite)
      End Function

      Private Function GenerateEditObjectParametersForSharedConfiguration() As EditObjectParameters
        Return GenerateEditObjectParameters(ConfigurationType.SharedConfiguration)
      End Function

      Event BeforeSavePluginConfiguration(Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean)
      Event BeforeSavePluginSharedConfiguration(Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean)
      Event BeforeEditPluginConfiguration(Configuration As Object, SetTablePropertyPhraseCorrispondence As StringDictionary)
      Event BeforeEditPluginSharedConfiguration(Configuration As Object, SetTablePropertyPhraseCorrispondence As StringDictionary)

      Private Sub RaiseEventBeforeSavePluginConfiguration(ByVal Obj As Object, ByVal ConfigurationType As ConfigurationType)
        Dim InvokeConfigurationPageRefresh As Boolean = False
        Select Case ConfigurationType
          Case ConfigurationType.InheritSubSite
            RaiseEvent BeforeSavePluginConfiguration(Obj, InvokeConfigurationPageRefresh)
          Case ConfigurationType.SharedConfiguration
            RaiseEvent BeforeSavePluginSharedConfiguration(Obj, InvokeConfigurationPageRefresh)
        End Select
        If InvokeConfigurationPageRefresh Then HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl) 'Refresh configuration pahe after reset to default values
      End Sub

      Event AfterSavePluginConfiguration(ByVal Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean)
      Event AfterSavePluginSharedConfiguration(ByVal Configuration As Object, ByRef InvokeConfigurationPageRefresh As Boolean)

      Private Sub RaiseEventAfterSavePluginConfiguration(ByVal Obj As Object, ByVal ConfigurationType As ConfigurationType)
        Refresh()
        Dim InvokeConfigurationPageRefresh As Boolean = False
        Select Case ConfigurationType
          Case ConfigurationType.InheritSubSite
            RaiseEvent AfterSavePluginConfiguration(Obj, InvokeConfigurationPageRefresh)
          Case ConfigurationType.SharedConfiguration
            RaiseEvent AfterSavePluginSharedConfiguration(Obj, InvokeConfigurationPageRefresh)
        End Select
        If InvokeConfigurationPageRefresh Then HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl) 'Refresh configuration pahe after reset to default values
      End Sub


      Private Function GenerateEditObjectParameters(ByVal ConfigurationType As ConfigurationType) As EditObjectParameters
        Dim Language As Language = CurrentSetting().Language
        Dim EditObjectParameters As EditObjectParameters = New EditObjectParameters
        Dim TablePropertyPhraseCorrispondence As StringDictionary = Nothing
        If ConfigurationType = PluginEditor.ConfigurationType.InheritSubSite Then
          EditObjectParameters.ObjectToEdit = LoadConfiguration(ConfigurationType, Config.CurrentSubSiteName())
          RaiseEvent BeforeEditPluginConfiguration(EditObjectParameters.ObjectToEdit, TablePropertyPhraseCorrispondence)
          EditObjectParameters.SaveObjectMethod = Sub()
                                                    RaiseEventBeforeSavePluginConfiguration(EditObjectParameters.ObjectToEdit, ConfigurationType)
                                                    Me.SaveObject(EditObjectParameters.ObjectToEdit, Config.CurrentSubSiteName())
                                                    RaiseEventAfterSavePluginConfiguration(EditObjectParameters.ObjectToEdit, ConfigurationType)
                                                  End Sub
        Else
          EditObjectParameters.ObjectToEdit = LoadConfiguration(ConfigurationType)
          RaiseEvent BeforeEditPluginSharedConfiguration(EditObjectParameters.ObjectToEdit, TablePropertyPhraseCorrispondence)
          EditObjectParameters.SaveObjectMethod = Sub()
                                                    RaiseEventBeforeSavePluginConfiguration(EditObjectParameters.ObjectToEdit, ConfigurationType)
                                                    Me.SaveObject(EditObjectParameters.ObjectToEdit)
                                                    RaiseEventAfterSavePluginConfiguration(EditObjectParameters.ObjectToEdit, ConfigurationType)
                                                  End Sub
        End If
        EditObjectParameters.TablePropertyPhraseCorrispondence = TablePropertyPhraseCorrispondence
        EditObjectParameters.TitleInPageEditor = Phrase(Language, 3025, 149) & " «" & UCase(Name) & "»" 'Configuration Plugin «NAMEPLUGIN»
        Return EditObjectParameters
      End Function

      Friend TypeObject(1) As Type

      Friend Sub AcquiringEditObjectParameter(ByVal TypeObject As Type, ByVal ConfigurationType As ConfigurationType)
        If TypeObject IsNot Nothing Then
          Me.TypeObject(CInt(ConfigurationType)) = TypeObject
          Select Case ConfigurationType
            Case ConfigurationType.InheritSubSite
              EditObjectParametersForInheritSubSite = AddressOf GenerateEditObjectParametersForInheritSubSite
            Case ConfigurationType.SharedConfiguration
              EditObjectParametersForSharedConfiguration = AddressOf GenerateEditObjectParametersForSharedConfiguration
          End Select
        End If
      End Sub

      Friend Enum ConfigurationType
        InheritSubSite
        SharedConfiguration
      End Enum

    End Class

    Sub RaiseDailyOperationEvent()
      Plugin.RaiseDailyOperationEvent()
    End Sub

    Sub RaiseApplicationEndEvent()
      Plugin.RaiseApplicationEndEvent()
    End Sub

    Sub RaiseIpnEvent(ByVal PluginName As String, ByVal IdOrder As String, ByVal Request As HttpRequest)
      Plugin.RaiseIpnEvent(PluginName, IdOrder, Request)
    End Sub

    Sub RaiseMasterPageInitEvent(ByVal MasterPage As Components.MasterPageEnhanced, ByVal User As User, ByVal Setting As SubSite)
      Plugin.RaiseMasterPageInitEvent(MasterPage, User, Setting)
    End Sub

    Sub RaiseMasterPagePreRenderEvent(ByVal MasterPage As Components.MasterPageEnhanced, ByVal User As User, ByVal Setting As SubSite)
      Plugin.RaiseMasterPagePreRenderEvent(MasterPage, User, Setting)
    End Sub

		Sub RaiseOnSitemapGenerationEvent(ByVal Type As TypeOfSitemap, ByVal Setting As SubSite, ByVal SiteMap As System.Xml.XmlTextWriter, ByVal Domain As String, ByVal Exclude As StringCollection, ByVal www As Boolean)
			Plugin.RaiseOnSitemapGenerationEvent(Type, Setting, SiteMap, Domain, Exclude, www)
		End Sub

		Sub RaiseRenderingPageEvent(ByVal Archive As Integer, ByVal Page As Integer, ByVal Setting As SubSite, ByVal HttpContext As HttpContext, ByVal Where As Control, ByVal MetaTags As MetaTags, ByVal Body As String, ByVal BottomContent As HtmlControl)
			Plugin.RaiseRenderingPageEvent(Archive, Page, Setting, HttpContext, Where, MetaTags, Body, BottomContent)
		End Sub

		Sub RaiseChatMessageWritedEvent(ByVal ChatRoom As String, ByVal Text As String, ByVal Author As String, ByVal Setting As SubSite)
			Plugin.RaiseChatMessageWritedEvent(ChatRoom, Text, Author, Setting)
		End Sub

		Sub RaiseForumReplyPostedEvent(Forum As Forum, ByVal Reply As Reply, ForumPostType As ForumPostType, ByVal Setting As SubSite)
			Plugin.RaiseForumReplyPostedEvent(Forum, Reply, ForumPostType, Setting)
		End Sub
		Enum ForumPostType
			NewTopic
			NewReply
			Modify
		End Enum

    Sub RaiseGetSkinSetupEvent(ByRef SetSkinSetup As SkinSetup, Setting As SubSite)
      Plugin.RaiseGetSkinSetupEvent(SetSkinSetup, Setting)
    End Sub

    Sub RaiseRobotsTxtPreRenderEvent(ByRef Text As String, DomainConfiguration As Config.DomainConfiguration)
      Plugin.RaiseRobotsTxtPreRenderEvent(Text, DomainConfiguration)
    End Sub


	End Module
End Namespace