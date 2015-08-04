'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications
Option Explicit On
Option Strict On
Imports System.Xml.Serialization
Namespace WebApplication
  'Forum Default ID assign
  Public Module ReservedForums
    Public Const PrivateMessage As Integer = 100
    Public Const PhotoComment As Integer = 101
    Public Const ArchiveComment As Integer = 102
  End Module

  Public Module ErrorLog
    Public DomainevaluationError As Integer
    Public ErrorDiskSpacePreserved As Integer
    Public ErrorSendEmail As String
    Public ExchangeRateUnavailable As Boolean
  End Module

  Public Module Config
    Public Setup As Config.Configuration

    Public Class Configuration
      Public Performance As New PerformanceConfiguration
      Class PerformanceConfiguration
        Public MultyThread As Boolean = False
        Public InhibitSessionForCrawlers As Boolean
        Public DaysToKeepInCacheTheExternalPagesAcquired As Integer = 30
      End Class

      Public Ambient As New AmbientConfiguration
      Class AmbientConfiguration
        Public DiskSpaceReservedGb As Integer
        Public DiskSpacePreservedGb As Integer
        Public SessionTimeOut As Integer = 10
        Public HtmlEditor As HtmlEditorNames
        Enum HtmlEditorNames
          WysiwygNet 'Free Html Editor .net by Andrea Bruno © http://wysiwygnet.com/
          TinyMCE
        End Enum
      End Class
      Public ReportErrorsByEmail As New ReportErrorsByEmailConfiguration
      Class ReportErrorsByEmailConfiguration
        Public Enabled As Boolean = True
        Public ExcludeErrorHttp404 As Boolean = True
        Public ExcludeError80004005 As Boolean = True
        Public ExcludeError80070057 As Boolean = True
        Public ExcludeError800704CD As Boolean = True
      End Class

      Public RenderingEngine As New RenderingEngineConfiguration
      Class RenderingEngineConfiguration
        Public TheLinkOfThumbnailsContainsAJumpToImagePosition As Boolean = True
        Public OnPageLoad As OnLoadPageAction
        Enum OnLoadPageAction
          JumpToSectionTagsInPage
          NoAction
        End Enum
        Public EnablePdfVersion As New EnablePdfVersionConfiguration
        Class EnablePdfVersionConfiguration
          Public TopicsOfForum As Boolean
          Public Photos As Boolean
          Public Pages As Boolean
        End Class

        Public UploadImageResize As Integer = 600
        Public UploadMaintainOriginalImage As Boolean = True
        Public GeneralShowDefaultImageSize As Integer = 600
        Public GeneralSizeThumbnail As Integer = 110
        Public GeneralSizeImagePreview As Integer = 300
        Public InterlaceJpegImages As Boolean = True
        Public HideStatsCode As Boolean = True
        Public HideModPlayer As Boolean = True
        Public ImageCopyright As Boolean = True
        Public FontCopyright As New FontCopyrightConfiguration
        Class FontCopyrightConfiguration
          Public Name As String = "Arial"
          Public Size As Integer = 20
          Public TransparencyPercentage As Integer = 80
        End Class
        Public AddLinksDomains As Boolean
        Public HideLinksDomains As Boolean
      End Class

      Public Security As New SecurityConfiguration
      Class SecurityConfiguration
        Public DdosPrevention As DdosPreventionConfiguration = New DdosPreventionConfiguration
        Class DdosPreventionConfiguration
          Public AntiAbnormalSimultaneousRequests As Boolean = True
          Public DisableTheSourcesOfAbnormalHighTraffic As Boolean
          Public TemporarilyCrawlersStoppingDuringOverload As Boolean = True
          Public LogEvents As Boolean
        End Class
        Public DoNotOpenIframeOnSitesContainingMalware As Boolean = True
        Public EnableProxyUsersToInteract As Boolean
        Public MaxBannedCodeUser As Integer = 20
        Public HoursEvaluationTimeNewUser As Integer = 12
        Public BlockIPAddresses As BlockIPMode
        'Forum Admonition
        Public QuorumToBlockUser As Integer = 6
        Public QuorumToCensored As Integer = 6
        Public DaysToWaitForTheExpirationOfTheAdmonition As Integer = 6
        Public DaysRequiredForReadmissionBlockedUser As Integer = 3
        Public OnlyLoggedUsersInChat As Boolean
        'Secure autentication by micropayments
        Public AuthenticateNewUsersThroughMicropayment As New AuthenticateThroughMicropayment
        Class AuthenticateThroughMicropayment
          Public Payment As Money = New Money
          Public DoNotAuthenticateUsersSubscribedToTheSitesWithTheForumId(-1) As Integer
        End Class
      End Class

      Enum BlockIPMode
        Disabled
        BlockIp
        BlockIpPort
      End Enum

      Public SEO As New SearchEngineOptimizationConfiguration
      Class SearchEngineOptimizationConfiguration
        Public WWWSubdomine As WWWUsage = WWWUsage.Both
        Enum WWWUsage
          OnlyWWW
          NoWWW
          Both
        End Enum
        Public NoindexForDocumentsInArchive0 As Boolean 'on is well for the multi-site (no contents duplicated)
        Public GoogleOffGoogleOnTagsEnabled As Boolean = True
        Public AddDescriptiveExifInfoToImages As Boolean = True
        Public MaintainsContentsInTheNewsListForDays As Integer = 60
        Public ForContentAcquiredFromExternalSourcesApplyTheTagGooglebotUnavailableAfterSettedToDays As Integer = 60
        Public GenerateSitemapDynamically As Boolean = True
        Public GenerateImagesSitemapDynamically As Boolean = True
        Public AtGooglebotExcludeIndexedPagesInSitemap As Boolean = True
        Public ChangeTheContentOfAggregatorsUsingSynonyms As Boolean = True
        Public PreventCrawlerArchivingOfContent As Boolean = True
        Public ShowContentAggregatorInThisHost As Boolean = True
        Public EntrapUsersFromSearchEngines As Boolean = True
        Public ErrorToBeUsedInCaseOfOverload As HttpError = HttpError.Error503
        Enum HttpError
          'http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html
          Error408
          Error503
        End Enum
        Public TypeOfContextualLink As LinkType
        Enum LinkType
          DoFollow
          NoFollow
          Javascript
        End Enum
      End Class

      Public UserInteract As New UserInteractConfiguration
      Class UserInteractConfiguration
        Public EnabledUsersToMakeSubPersonalPhotoAlbum As Boolean = True
      End Class

      Public Contacts As New ContactsConfiguration
      Class ContactsConfiguration
        'Phone Contacts
        '<XmlIgnore()> Property PhoneContactsElements As Integer
        '	Get
        '		If PhoneContacts Is Nothing Then
        '			Return 0
        '		Else
        '			Return PhoneContacts.Length
        '		End If
        '	End Get
        '	Set(ByVal value As Integer)
        '		Dim OldUblund
        '		If PhoneContacts Is Nothing Then
        '			OldUblund = -1
        '		Else
        '			OldUblund = UBound(PhoneContacts)
        '		End If

        '		ReDim Preserve PhoneContacts(value - 1)
        '		If OldUblund < UBound(PhoneContacts) Then
        '			For n As Integer = OldUblund To UBound(PhoneContacts)
        '				If n >= 0 Then
        '					PhoneContacts(n) = New PhoneContact
        '				End If
        '			Next
        '		End If
        '	End Set
        'End Property
        Public PhoneContacts(-1) As PhoneContact
        Class PhoneContact
          Public PhoneNumber As String
          Public Service As Service
          Public Language As LanguageManager.Language
          Public FreeCall As Boolean
          Public Sub New(ByVal PhoneNumber As String, ByVal Service As Service, Optional ByVal Language As LanguageManager.Language = LanguageManager.Language.NotDefinite, Optional ByVal FreeCall As Boolean = False)
            Me.PhoneNumber = PhoneNumber
            Me.Service = Service
            Me.Language = Language
            Me.FreeCall = FreeCall
          End Sub
          Public Sub New()
            'No remove! This Sub is necessary for XML serialization 
          End Sub

          Function Text(ByVal Language As LanguageManager.Language) As String
            Dim Id As Integer = 0
            Select Case Service
              Case Service.Fax
                Id = 3109
              Case Service.General
                Id = 3184
              Case Service.TechnicalSupport
                Id = 3185
              Case Service.CustomerCare
                Id = 3198
            End Select
            Return Phrase(Language, Id)
          End Function
        End Class
        Enum Service
          General
          CustomerCare
          TechnicalSupport
          Fax
        End Enum
      End Class
      Class ContactsConfigurationOption
        Inherits Config.Configuration.ContactsConfiguration
        Public Enabled As Boolean
      End Class
      Public Email As New EmailConfiguration
      Public Class EmailConfiguration
        Public SmtpServer As String
        Public SmtpPort As Integer
        Public SSL As Boolean
        Public SmtpUserName As String
        Public SmtpPassword As String
        Public Email As New Sender
        Public EmailSupervisor As New Sender
        Public EmailFaxService As New Sender
        Public FaxPassword As String
      End Class

      Class Sender
        Public Email As String
        Public SmtpServer As String
        Public SmtpPort As Integer = 25
        Public SSL As Boolean
        Public SmtpUserName As String
        Public SmtpPassword As String
      End Class

      Class SenderOption
        Inherits Config.Configuration.Sender
        Public Enabled As Boolean
      End Class

      Enum EmailSender As Integer
        Supervisor
        Administrator
        ToFax
      End Enum

      Public Forum As New ForumConfiguration
      Class ForumConfiguration
        Public NPaginating As Integer = 20
        Public MaxLengthReply As Integer = 4000
        Public RepliesForPages As Integer = 20
        Public TopicsForPage As Integer = 30
      End Class

      Public PaymentGateway As New PaymentGatewayConfiguration
      Class PaymentGatewayConfiguration
        'Account for online payment with cards
        Public EnabledOnlinePayment As Boolean
        Public PayPalAccount As String
        Public PayPalAccountMicropayments As String
        Public MoneybookersAccount As String
      End Class

      Public Affiliations As New AffiliationsConfiguration
      Class AffiliationsConfiguration
        'Google AdSense
        Public Google_Client As String
      End Class

    End Class

    Public Sub SaveConfiguration(ByVal FileName As String, Optional ByVal IfNotExists As Boolean = False)
      Dim File As String = MapPath(ReadWriteDirectory & "/" & FileName)
      If IfNotExists = False OrElse Not System.IO.File.Exists(File) Then
        Serialize(Setup, File)
        Pipeline.NotifyChangement(GetType(Configuration), FileName)
      End If
    End Sub

    Public Sub LoadConfiguration(ByVal FileName As String)
      Setup = CType(Deserialize(MapPath(ReadWriteDirectory & "/" & FileName), GetType(Configuration)), Configuration)
      Static AddedActionForUpdate As Boolean
      If AddedActionForUpdate = False Then
        AddedActionForUpdate = True
        Pipeline.AddActionForNotification(GetType(Configuration), New Pipeline.NotifyChangementDelegate(AddressOf LoadConfiguration))
      End If
    End Sub


    'No customizable settings:
    Public Const AutoAdSenseForSearch As Boolean = False
    Public Const Icon_Forum As Components.IconType = Components.IconType.Contacts
    Public Const MetaRevisitAfterDays As Integer = 200

    'Tiscali account webservices
    Public Tiscali_User As String
    Public Tiscali_Password As String

    'External module 
    'Public MethodImportDating As [Delegate]
    'Application Setting 
    Public LocalizationDataBaseConnection As String '= "DSN=localization;UID=;PWD=; DRIVER={Microsoft OdBc for Oracle}; SERVER=;"

    ''Public DataProvider = "SQLEXPRESS;Driver={Microsoft Access Driver (*.mdb)}"
    'Public DataProvider = "MSDASQL;Driver={Microsoft Access Driver (*.mdb)}"
    ''Public KeyDataSource = "Dbq"
    'Public DataProvider = "Microsoft.ACE.OdBc.12.0"
    'Public DataProvider = "Microsoft.Jet.OdBc.4.0"
    Public DataProvider As String = "Microsoft.Jet.OLEDB.4.0"
    Public KeyDataSource As String = "Data Source"

    'Public KeyDataSource = "AttachDBFilename"
    Public NavigateUrlIpLookup As String = "http://whatismyipaddress.com/staticpages/index.php/lookup-results?ip="
    Public SerializationMode As SerializationModeType = SerializationModeType.XML 'ATTENTION: Binary serialization have a security problem in Godaddy host
    Public Enum SerializationModeType
      XML
      Binary 'This mode is not compatible with more hosting service (for exempl "Godaddy")
    End Enum
    'General setting

    Property ShowDefaultImageSize(Optional ByVal HttpContext As HttpContext = Nothing) As Integer
      Get
        Try
          If HttpContext Is Nothing Then
            HttpContext = HttpContext.Current
          End If
          If HttpContext.Request.Browser.IsMobileDevice Then
            Return 220
          End If
        Catch ex As Exception

        End Try
        Return Setup.RenderingEngine.GeneralShowDefaultImageSize
      End Get
      Set(ByVal value As Integer)
        Setup.RenderingEngine.GeneralShowDefaultImageSize = value
      End Set
    End Property

    Property SizeThumbnail() As Integer
      Get
        Return Setup.RenderingEngine.GeneralSizeThumbnail
      End Get
      Set(ByVal value As Integer)
        Setup.RenderingEngine.GeneralSizeThumbnail = value
      End Set
    End Property

    Property SizeImagePreview(Optional ByVal HttpContext As HttpContext = Nothing) As Integer
      Get
        Try
          If HttpContext.Request Is Nothing Then
            HttpContext = HttpContext.Current
          End If
          If HttpContext.Request.Browser.IsMobileDevice Then
            Return 220
          End If
        Catch ex As Exception

        End Try
        Return Setup.RenderingEngine.GeneralSizeImagePreview
      End Get
      Set(ByVal value As Integer)
        Setup.RenderingEngine.GeneralSizeImagePreview = value
      End Set
    End Property

    'directory with read and write permission
    Public ReadWriteDirectory As String = "App_Data"

    'File name
    Public LocalizationDataFile As String = FileManager.MapPath(ReadWriteDirectory & "/localization.mdb")

    'const (sub directory with read and write permission):
    Public PreserveDiskSpaceDirectory As String = "WebApplication"
    Public ValuesSubDirectory As String = ReadWriteDirectory & "/values"
    Public PhotoAlbums_Sub_Directory As String = ReadWriteDirectory & "/PhotoAlbum"
    Public UsersSubDirectory As String = ReadWriteDirectory & "/users"
    Public SubSitesSubDirectory As String = ReadWriteDirectory & "/subsites"
    Public ArchiveSubDirectory As String = ReadWriteDirectory & "/archive"
    Public ForumSubDirectory As String = ReadWriteDirectory & "/forum"
    Public LogSubDirectory As String = ReadWriteDirectory & "/log"
    Public DomainConfigurationsSubDirectory As String = ReadWriteDirectory & "/domains"
    Public ObjectSubDirectory As String = ReadWriteDirectory & "/objects"
    Public CodesSubDirectory As String = ReadWriteDirectory & "/codes"
    Public ChatSubDirectory As String = ReadWriteDirectory & "/chat"
    Public SkinsSubDirectory As String = ReadWriteDirectory & "/skins"
    Public DocumentsSubDirectory As String = ReadWriteDirectory & "/documents"
    Public PipelineDirectory As String = ReadWriteDirectory & "/pipeline"
    Public PluginsSubDirectory As String = ReadWriteDirectory & "/Plugins"

    ' Code Couters for statistic
    Public CodeCounter As String = ReadAll(MapPath(CodesSubDirectory & "/counter.html"))

    'read only directory
    Private _Resources As String
    Function PayPalAccountIsSetting() As Boolean
      If Not String.IsNullOrEmpty(Setup.PaymentGateway.PayPalAccount) OrElse Not String.IsNullOrEmpty(Setup.PaymentGateway.PayPalAccountMicropayments) Then
        Return True
      End If
      Return False
    End Function
    Public Property Resources() As String
      Get
        Return (_Resources)
      End Get
      Set(ByVal value As String)
        _Resources = (value)
        ImgagesResources = _Resources & "/images"
        SoundsResources = _Resources & "/sounds"
        ScriptsResources = _Resources & "/scripts"
        FlashResources = _Resources & "/flash"
      End Set
    End Property
    Public ImgagesResources As String
    Public SoundsResources As String
    Public ScriptsResources As String
    Public FlashResources As String

    Public WebServicesEnabledSendSMS As WebServicesSendSms = WebServicesSendSms.None
    Sub AutoSetWebServices()
      Dim Request As Net.WebRequest = Net.WebRequest.Create("http://www.webservicex.com/sendsmsworld.asmx?WSDL")
      Dim Response As Net.HttpWebResponse = Nothing
      Try
        Response = CType(Request.GetResponse(), Net.HttpWebResponse)
      Catch ex As Exception
      End Try
      If Response Is Nothing OrElse Response.StatusCode <> Net.HttpStatusCode.OK Then
        WebServicesEnabledSendSMS = WebServicesSendSms.WwwWebservicexCom
      End If
    End Sub

    Enum WebServicesSendSms As Integer
      None = 0
      WwwWebservicexCom = 1
    End Enum

    Public Function CheckWritePermission() As Boolean
      Static Checked As Boolean
      Static InError As Boolean
      If Checked = False OrElse InError = True Then
        Checked = True
        Dim RWDir As New System.IO.DirectoryInfo(MapPath(ReadWriteDirectory))
        If CBool(RWDir.Attributes And System.IO.FileAttributes.ReadOnly) Then
          'Remove ReadOnly Attribute
          RWDir.Attributes = CType(RWDir.Attributes - IO.FileAttributes.ReadOnly, IO.FileAttributes)
        End If
        Try
          'Check writable permission
          System.IO.File.WriteAllText(MapPath(ReadWriteDirectory & "\test.tmp"), "test write")
          System.IO.File.Delete(MapPath(ReadWriteDirectory & "\test.tmp"))
          If InError Then
            'Restart the Application, the issue is fixed!
            System.Web.HttpRuntime.UnloadAppDomain()
          End If
          InError = False
        Catch ex As Exception
          InError = True
        End Try
        If InError Then
          Dim ErrorMessage As String = "This Web Application doesn't have permissions enough to read/write contents of """ & ReadWriteDirectory & """ subfolder! Enable Write Permissions from your Web Hosting Control Panel!"
          Try
            Dim Message As String = "<html><body><h1>" & ErrorMessage & "<br><a href='" & CopyrightLink & "forum.aspx?f=202&c=1'>Online Technical Support</a></h1></body></html>"
            HttpContext.Current.Response.Clear()
            HttpContext.Current.Response.ContentType = "text/html"
            HttpContext.Current.Response.Write(Message)
            HttpContext.Current.Response.End()
          Catch ex As Exception
            'Err.Raise(70, , ErrorMessage)
          End Try
        End If
      End If
      Return InError
    End Function

    Public Sub Run()
      'set resources directory location
      Resources = "resources"

      If CheckWritePermission() Then
        'dont inizialize! The app_data directory is inaccessible!
        Exit Sub
      End If
      CreateDirectory(ReadWriteDirectory)
      CreateDirectory(PhotoAlbums_Sub_Directory)
      CreateDirectory(UsersSubDirectory)
      CreateDirectory(UsersSubDirectory & "\reports")
      CreateDirectory(ValuesSubDirectory)
      CreateDirectory(SubSitesSubDirectory)
      CreateDirectory(ArchiveSubDirectory)
      If Not MenuManager.Archive.Exist(0) Then
        MenuManager.Archive.Create(0) ' Add standard shared archive ID=0
      End If
      CreateDirectory(ForumSubDirectory)
      CreateDirectory(LogSubDirectory)
      CreateDirectory(DomainConfigurationsSubDirectory)
      CreateDirectory(ObjectSubDirectory)
      CreateDirectory(CodesSubDirectory)
      CreateDirectory(ChatSubDirectory)
      CreateDirectory(SkinsSubDirectory)
      CreateDirectory(DocumentsSubDirectory)
      CreateDirectory(PipelineDirectory)
      CreateDirectory(PluginsSubDirectory)

      Dim RootPhotoAlbum As New PhotoManager.PhotoAlbum
      RootPhotoAlbum.SubPhotoAlbumsNotCreatable = False
      RootPhotoAlbum.Deletable = PhotoManager.Permission.None
      RootPhotoAlbum.IsRoot = True
      RootPhotoAlbum.Save()

      Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "end CreateDirectory")

      LoadConfiguration("config.xml")
      Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "LoadConfiguration")

      If Setup Is Nothing Then
        Setup = New Configuration
        SaveConfiguration("config.xml", True)
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "SaveConfiguration")
      End If

      'Make default forums:
      'Make forum for personal message
      Dim ForumForPrivateMessage As Forum = CType(ForumManager.Forum.Load.GetItem(ReservedForums.PrivateMessage), Forum)
      If ForumForPrivateMessage Is Nothing Then
        ForumManager.Forum.NewForum(ReservedForums.PrivateMessage, Forum.TypeOfForum.Forum)
      End If

      'Make forum for photos comments
      Dim ForumForPhotoComment As Forum = CType(ForumManager.Forum.Load.GetItem(ReservedForums.PhotoComment), Forum)
      If ForumForPhotoComment Is Nothing Then
        ForumManager.Forum.NewForum(ReservedForums.PhotoComment, Forum.TypeOfForum.Forum)
      End If

      'Make forum for archive comments
      Dim ForumForArchiveComment As Forum = CType(ForumManager.Forum.Load.GetItem(ReservedForums.ArchiveComment), Forum)
      If ForumForArchiveComment Is Nothing Then
        ForumManager.Forum.NewForum(ReservedForums.ArchiveComment, Forum.TypeOfForum.Forum)
      End If

      Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "end Create forum for inside use")
      Startup()

      '==============================REMOVE THIS BLOCK!!! (routine necessary to update old version)
      If False Then 'Set this "True" to execute conversion from 4.xx version 
        Try
          Dim Root As String = FileManager.MapPath(UsersSubDirectory)
          Dim dir As New System.IO.DirectoryInfo(Root)
          Dim Files As System.IO.FileSystemInfo() = dir.GetFileSystemInfos("*.xml")
          Dim N As Integer
          Extension.Log("Conversion", 1000, "Files=" & Files.Count)
          For Each File As System.IO.FileSystemInfo In Files
            'If N > 55000 Then
            Dim x As String = ReadAll(File.FullName)
            x = ReplaceBin(x, "<RoleType>-1</RoleType>", "<RoleType>Excluded</RoleType>")
            x = ReplaceBin(x, "<RoleType>0</RoleType>", "<RoleType>Visitors</RoleType>")
            x = ReplaceBin(x, "<RoleType>1</RoleType>", "<RoleType>User</RoleType>")
            x = ReplaceBin(x, "<RoleType>2</RoleType>", "<RoleType>Senior</RoleType>")
            x = ReplaceBin(x, "<RoleType>3</RoleType>", "<RoleType>AdministratorJunior</RoleType>")
            x = ReplaceBin(x, "<RoleType>4</RoleType>", "<RoleType>Administrator</RoleType>")
            x = ReplaceBin(x, "<RoleType>5</RoleType>", "<RoleType>WebMaster</RoleType>")
            x = ReplaceBin(x, "<RoleType>6</RoleType>", "<RoleType>Supervisor</RoleType>")
            WriteAll(x, File.FullName)
            'End If
            N += 1
            If N Mod 100 = 0 Then
              Extension.Log("Conversion", 1000, N)
            End If
          Next
        Catch ex As Exception
          Extension.Log("Conversion", 1000, ex.Message, ex.StackTrace)
        End Try
      End If
      '++++++++++++++++++++++++++++++++++++++++++++ END REMOVE





      If False Then
        'Initialize application
        Initialize()
      Else
        'Initialize application in separate task
        Dim NewThread As System.Threading.Thread = New System.Threading.Thread(AddressOf Initialize)
        NewThread.Priority = Threading.ThreadPriority.Lowest
        NewThread.Name = "Initialize"
        NewThread.IsBackground = True
        NewThread.Start()
      End If

    End Sub

    Public AcceptHttpRequest As Boolean
    Private Sub Startup()
      Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "Startup")
      Try
        'Initialization operation in Thread
        LoadPhrasesBooks() 'Conversion of Phrases Books from .MDB to XML need the "Full trust level" 
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "LoadPhrasesBooks")

        LoadAllPlugins()
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "LoadAllPlugins")

        DomainConfiguration.UpdateDataCorrelatedToDomainConfiguration()
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "UpdateDataCorrelatedToDomainConfiguration")

        Dim CacheImageSize As CacheWidthHeight = CType(LoadObject(GetType(CacheWidthHeight), "Resources"), CacheWidthHeight)
        If CacheImageSize IsNot Nothing Then
          Components.CacheImageSize = CacheImageSize
        End If
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "CacheImageSize")

        ChatManager.LoadMessages(DefaultChatRoom)
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "DefaultChatRoom")

        If Not IsLocal() Then
          Polling.Timer.Start()
        Else
          'Test the culture routine
          For Each CheckLanguage In LanguageManager.LanguagesUsed
            Dim CultureInfo As System.Globalization.CultureInfo
            CultureInfo = LanguageManager.Culture(CheckLanguage)
          Next
        End If
        AcceptHttpRequest = True
      Catch ex As Exception
        Extension.Log("application_start", 1000, "ERROR Startup", ex.Message, ex.StackTrace)
        ReportErrorByEmail(ex)
      End Try
    End Sub

    Public Sub Initialize()
      Try

        'Initialization operation in NEW Thread
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "Start Initialize")

        PreserveDiskSpace()
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "PreserveDiskSpace")

        ContextualLink.PrepareAndCollectContextualLinks()
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "PrepareAndCollectContextualLinks")

        ForumManager.Topic.LoadCache() 'Load cache forum turbo method
        Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "CachesTopic")

        If Not IsLocal() Then

          AutoSetWebServices()
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "AutoSetWebServices")

          UpdateBlogAggregator()
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "UpdateBlogAggregator")

          UpdateNews()
          NewsManager.Updater.Start()
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "UpdateNews")

          ForumManager.LoadCachesForum() 'Load cache forum normal method
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "LoadCachesForum")

          UpdateWeather()
          WeatherManager.Updater.Start()
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "WeatherManager")

          UpdateIndexedPagesOnGoogle()
          Extension.Log("application_start", 1000, Threading.Thread.CurrentThread.GetHashCode, "UpdateIndexedPagesOnGoogle")
        End If
      Catch ex As Exception
        Extension.Log("application_start", 1000, "Error in ThreadInitialize: " & ex.Message & vbCrLf & ex.StackTrace)
        ReportErrorByEmail(ex)
      End Try

    End Sub

    Public Sub Application_End()
      'Save datas at end of application
      'FileManager.DeleteDirectoryInSpooler()
      Extension.Log("application_end", 1000, "DeleteDirectoryInSpooler")

      EndPipelineComunication()
      Extension.Log("application_end", 1000, "EndPipelineComunication")

      SaveObject(CacheImageSize, "Resources")
      Extension.Log("application_end", 1000, "CacheImageSize")

      CloseAllConnections()
      Extension.Log("application_end", 1000, "CloseAllConnections")

      ChatManager.SaveMessages(DefaultChatRoom)
      Extension.Log("application_end", 1000, "DefaultChatRoom")

      SaveIpIPBlockedCollection()
      Extension.Log("application_end", 1000, "SaveIpIPBlockedCollection")

      LogOutAllUsers()
      Extension.Log("application_end", 1000, "LogOutAllUsers")

      ForumManager.Topic.SaveCache()  'Save cache of forum for Turbo load 
      Extension.Log("application_end", 1000, "SaveForumCache")

    End Sub

    Public GoogleOffTag As String = "<!--googleoff: index-->"
    Public GoogleOnTag As String = "<!--googleon: index-->"

    Public Class SubSite
      Inherits PluginAttributes
      Public Shared Load As New Cache(AddressOf LoadObject, 100, GetType(SubSite))
      Shared Function GetSubSite(SubSiteName As String) As SubSite
        Return CType(Config.SubSite.Load.GetItem(SubSiteName), Config.SubSite)
      End Function

      Shared Function DefaultSiteConfiguration() As String
        If Not String.IsNullOrEmpty(OEM.DefaultSiteConfiguration) Then
          Return OEM.DefaultSiteConfiguration
        Else
          Return "default"
        End If
      End Function
      Public Sub UpdateBlogAggregator()
        If Me.EnableRelatedBlogAggregator Then
          Dim Keyword As String
          Dim Keywords() As String = Split(Me.KeyWords, ",")
          If Keywords IsNot Nothing AndAlso Keywords.Length > 0 Then
            Keyword = Keywords(0)
            If String.IsNullOrEmpty(Keyword) Then
              Keyword = Me.Description
            End If
            If Not String.IsNullOrEmpty(Keyword) Then
              Dim Feeds As New System.Collections.Generic.List(Of NewsManager.Notice)
              Dim Request As New NewsManager.NewsRequire
              Request.AddAllRecords = True
              Dim Acronym As String
              If Me.Language = LanguageManager.Language.Chinese Then
                Acronym = "zh-CN"
              Else
                Acronym = LanguageManager.Acronym(Me.Language)
              End If
              Request.XmlHref = "http://blogsearch.google.com/blogsearch_feeds?hl=" & Acronym & "&lr=lang_" & Acronym & "&q=" & HttpUtility.UrlEncode(Keyword) & "&ie=utf-8&num=10&safe=archive&output=rss"
              ReadFeed(Feeds, Request)
              Me.BlogList = Feeds
            End If
          End If
        End If
      End Sub
      'Public DomainConfiguration As DomainConfiguration
      <XmlIgnore()> Friend NotLoadabled As Boolean
      Public Function NotExist() As Boolean
        Return NotLoadabled
      End Function
      Public Name As String
      Public Language As LanguageManager.Language = LanguageManager.Language.English
      Function Culture() As Globalization.CultureInfo
        Return LanguageManager.Culture(Language)
      End Function

      Private m_DateFormat As String = "dd/MM/yyyy"
      Property DateFormat As String
        Get
          Return m_DateFormat
        End Get
        Set(ByVal value As String)
          Try
            Now.ToString(value)
            m_DateFormat = value
          Catch ex As Exception
            m_DateFormat = "dd/MM/yyyy"
          End Try
        End Set
      End Property
      Private m_TimeFormat As String = "H\:mm\:ss" 'http://msdn.microsoft.com/it-it/library/vstudio/system.globalization.datetimeformatinfo(v=vs.100).aspx
      Property TimeFormat As String
        Get
          Return m_TimeFormat
        End Get
        Set(ByVal value As String)
          Try
            Now.ToString(value)
            m_TimeFormat = value
          Catch ex As Exception
            m_TimeFormat = "H\:mm\:ss" 'http://msdn.microsoft.com/it-it/library/vstudio/system.globalization.datetimeformatinfo(v=vs.100).aspx
          End Try
        End Set
      End Property

      Function DateTimeFormat() As String
        Return DateFormat & " " & TimeFormat
      End Function

      Private SkinName As String
      Public Property IdSkin As String
        Get
          If Not String.IsNullOrEmpty(SkinName) Then
            Return SkinName
          Else
            'NOTE: is case sensitive!
            If Not String.IsNullOrEmpty(OEM.DefaultSkin) Then
              Return OEM.DefaultSkin
            Else
              Return "Default"
            End If
          End If
        End Get
        Set(ByVal value As String)
          If SkinName <> value Then
            SkinName = value
            MySkin = Nothing
          End If
        End Set
      End Property

      Private MySkin As Skin
      Public Function Skin() As Skin
        If MySkin Is Nothing Then
          MySkin = Config.Skin.Load(IdSkin)
        End If
        Return MySkin
      End Function
      Public Archive As Integer()
      Function MainArchive() As Integer
        If Archive IsNot Nothing AndAlso Archive.Length = 1 Then
          Return Archive(0)
        Else
          Return 0
        End If
      End Function
      Public Forums() As Integer
      Public Description As String
      Public Title As String
      'Public AdSense As Boolean
      Public KeyWords As String
      Public CorrelatedWords As New CerrelatedStructure
      Public NotIncludeSitesCensoredIntoFrame As Boolean = True
      Class CerrelatedStructure
        Public Words() As String
        Public Status As CorrelatedStatus
        Enum CorrelatedStatus As Integer
          NotEnabled
          Enabled
          Context
        End Enum
      End Class
      Public News As NewsManager.FilterNews
      Public EnableRelatedBlogAggregator As Boolean
      Private BlogList As System.Collections.Generic.List(Of NewsManager.Notice)
      Function Blog() As System.Collections.Generic.List(Of NewsManager.Notice)
        Return BlogList
      End Function
      Public DisablingCommentsToPages As Boolean
      Public DisablingCommentsToPhotos As Boolean
      Public Weathers() As WeatherLocality
      Public Currency As String = "EUR"
      Public CurrencyExchangeWidget As Boolean
      Public Photoalbums As New StringCollection

      'Modules
      Public AboutUs As Boolean = True
      Public Contacts As Boolean = True
      Public UsersOnline As Boolean = True

      'Preferences
      Public ShowRelatedPagesWithTheSearchCarriedOutByTheVisitor As Boolean = True

      Public TextRendering As New TextRenderingConfiguration
      Class TextRenderingConfiguration
        Public LinkToMapTheNamesOfGeographicalLocations As Boolean = True
      End Class

      Public EnablePdfVersion As New EnablePdfVersionConfiguration
      Class EnablePdfVersionConfiguration
        Public TopicsOfForum As EnabledStatus
        Public Photos As EnabledStatus
        Public Pages As EnabledStatus
      End Class


      Function Email() As Config.Configuration.Sender
        If EmailCustomization.Enabled Then
          Return EmailCustomization
        Else
          Return Config.Setup.Email.Email
        End If
      End Function

      Public EmailCustomization As New Config.Configuration.SenderOption

      Function PhoneContacts() As Config.Configuration.ContactsConfiguration
        If ContactsCustomization.Enabled Then
          Return ContactsCustomization
        Else
          Return Config.Setup.Contacts
        End If
      End Function

      Public ContactsCustomization As New Config.Configuration.ContactsConfigurationOption

      Public PluginInHomePage As String
      Public Aspect As New AspectConfiguration
      Class AspectConfiguration
        Public ShowTheDateInTheArticles As Boolean
        Public ShowTheAuthorInTheArticles As Boolean
        Public FirstDocumentInHomePage As Boolean = True
        Public MenuContractible As Boolean = True
        Public RandomPicturesForPhotoAlbum As Integer = 1
        Public SuggestTheBestChoice As BestChoiceEffect = BestChoiceEffect.Blink
        Enum BestChoiceEffect
          None
          Blink
          RedBlink
          Evidencing
          BackgroundRed
        End Enum
        Public SizeOfVideoEmbedded As VideoSize
        Public AddImageIntoTheBackgroundOfButtonPhotoAlbum As Boolean
        Enum VideoSize
          Fluid
          SameAsImagesPreviewConfiguration
        End Enum
        Public ForumPreviewInMainPage As New ForumPreviewInMainPageConfiguration
        Class ForumPreviewInMainPageConfiguration
          Public TypeOfForumPreview As ForumPreviewType
          Enum ForumPreviewType
            LastTopics
            SummaryByCategories
          End Enum
          Public TopicsOfForumInPreview As Integer = 3
        End Class
        Public OrderOfTheElementsOnTheMainPage As New OrderOfTheElementsOnTheMainPageConfiguration
        Class OrderOfTheElementsOnTheMainPageConfiguration
          Public ForumPreview As PagePosition = PagePosition.Position1
          Public NewsPreview As PagePosition = PagePosition.Position2
          Public ChatPreview As PagePosition = PagePosition.Position3
          Public Content As PagePosition = PagePosition.Position4
          Enum PagePosition
            Position1
            Position2
            Position3
            Position4
          End Enum
        End Class
        Public ShowElementsOnTheMainPage As New ShowElementsOnTheMainPageConfiguration
        Class ShowElementsOnTheMainPageConfiguration
          Public ForumPreview As Boolean = True
          Public NewsPreview As Boolean = True
          Public ChatPreview As Boolean = True
        End Class
        Public ImageRendering As New ImageRenderingConfiguration
        Class ImageRenderingConfiguration
          Public ShowExifInfo As Boolean
          Public ImageTone As ColorTone
          Public DoNotApplyTheImagesToneToUsers As Boolean = True
        End Class
      End Class


      Public SEO As New SeoFunctions
      Class SeoFunctions
        Public ShareCorrelatedWords As Boolean = True
        Public ApplyCorrelatedWordsFromOtherSites As Boolean = True
        Public AutoTagDlForThePages As Boolean = True
        Public AutomaticallyDelimitsBlocksOfCodeInTheText As Boolean = True
        Public Indexing As IndexingDirective
        Enum IndexingDirective
          Index
          Noindex
          GooglebotNoindex
          MsnbotNoindex
          SlurpNoindex
          ToemaNoindex
        End Enum
        Public TransformationOfTheUrlInTheText As UrlTransformation = UrlTransformation.ExternalLinksInFrames
        Enum UrlTransformation
          NoTransformation
          LinksToPage
          RelNofollowLinksToPage
          ExternalLinksInFrames
        End Enum
        Public TextOnHomePageButton As HomeButtonText
        Enum HomeButtonText
          HomePage
          WebsiteTitle
        End Enum
        Public LinkToWebsitesOfUsersInTheForums As TypeLink
        Enum TypeLink
          No
          Yes
          YesWithTheTitle
          YesWithTheIcon
        End Enum
        Public CopyPrevention As New CopyPreventionSetup
        Class CopyPreventionSetup
          Public ForumAndComments As Boolean
          Public Pages As Boolean
          Public FromExternalSources As Boolean = True
        End Class
        Public PhotoObfuscation As New PhotoObfuscationSetup
        Class PhotoObfuscationSetup
          Public PhotoOfUsers As PhotoObfuscationType
          Public PhotoOfWebsite As PhotoObfuscationType
          Enum PhotoObfuscationType
            NoManipulation
            MakeThemOriginal
          End Enum
        End Class
        'Public Add5StarRatingIntoPagesOfContent As Boolean = True
      End Class

      Public ForumOfPresentation As New PresentationForum
      Class PresentationForum
        Public InviteNewUsersToWriteTheirPresentation As Boolean = True
        Public ForumId As Integer
        Public SubcategoryId As Integer
      End Class

      Private Shared Function DirectoryName(ByVal SubSiteName As String) As String
        Return MapPath(Config.SubSitesSubDirectory)
      End Function

      Private Shared Function FileName(ByVal SubSiteName As String) As String
        Return DirectoryName(SubSiteName) & "/" & SubSiteName & ".xml"
      End Function

      Public Shared Sub Delete(Name As String)
        If AllSubSiteNames.Contains(Name) Then
          AllSubSiteNames.Remove(Name)
        End If
        If IO.File.Exists(FileName(Name)) Then
          FileManager.Delete(FileName(Name))
        End If
        'Remove from cache and notify the deletion to others servers
        Config.SubSite.Load.NotifyDeletion(GetType(SubSite), Name)
      End Sub

      Public Sub Save()
        If Not System.IO.Directory.Exists(DirectoryName(Name)) Then
          System.IO.Directory.CreateDirectory(DirectoryName(Name))
        End If
        Serialize(Me, FileName(Name))
        Me.NotLoadabled = False
        Pipeline.NotifyChangement(GetType(SubSite), Name)
      End Sub

      Public Function PhoneContact(ByVal Service As Config.Configuration.ContactsConfiguration.Service, Optional ByVal FreeCallIfExists As Boolean = True) As Config.Configuration.ContactsConfiguration.PhoneContact
        If Me.PhoneContacts IsNot Nothing Then
          Dim Contacts() As Config.Configuration.ContactsConfiguration.PhoneContact = Me.PhoneContacts.PhoneContacts
          Dim SortCriterion As IComparer(Of Configuration.ContactsConfiguration.PhoneContact) = New PhoneContactsComparer(Me.Language, Service)
          System.Array.Sort(Contacts, SortCriterion)
          For Each Contact As Config.Configuration.ContactsConfiguration.PhoneContact In Contacts
            If FreeCallIfExists OrElse Contact.FreeCall = False Then
              If Contact.Language = LanguageManager.Language.NotDefinite OrElse Contact.Language = Me.Language Then
                If Contact.Service = Configuration.ContactsConfiguration.Service.General OrElse Contact.Service = Service Then
                  Dim Result As New Configuration.ContactsConfiguration.PhoneContact(Contact.PhoneNumber, Service, Contact.Language, Contact.FreeCall)
                  Return Result
                End If
              End If
            End If
          Next
        End If
        Return Nothing
      End Function

      Public Class PhoneContactsComparer
        Implements IComparer(Of Configuration.ContactsConfiguration.PhoneContact)
        Private Language As LanguageManager.Language
        Private Service As Config.Configuration.ContactsConfiguration.Service
        Public Function Compare(ByVal T1 As Config.Configuration.ContactsConfiguration.PhoneContact, ByVal T2 As Config.Configuration.ContactsConfiguration.PhoneContact) As Integer Implements IComparer(Of Configuration.ContactsConfiguration.PhoneContact).Compare
          If T1.Service = T2.Service Then
            If T1.Language = T2.Language Then
              Return 0
            ElseIf T1.Language = Me.Language Then
              Return -1
            ElseIf T2.Language = Me.Language Then
              Return 1
            End If
            Return 0
          Else
            If T1.Service = Me.Service Then
              Return -1
            ElseIf T2.Service = Me.Service Then
              Return 1
            ElseIf T1.Service = Configuration.ContactsConfiguration.Service.General Then
              Return -1
            ElseIf T2.Service = Configuration.ContactsConfiguration.Service.General Then
              Return 1
            End If
            Return 0
          End If
        End Function 'Compare

        Public Sub New(ByVal Language As LanguageManager.Language, ByVal Service As Config.Configuration.ContactsConfiguration.Service)
          Me.Language = Language
          Me.Service = Service
        End Sub
      End Class 'SizeComparer
      Private Shared SubSiteLoaded As New Collections.Generic.Dictionary(Of String, SubSite)
      Private Shared Function LoadObject(ByVal SubSiteName As String) As SubSite
        Dim NewSetting As SubSite
        Dim File As String = FileName(SubSiteName)
        If System.IO.File.Exists(File) Then
          NewSetting = CType(Deserialize(File, GetType(SubSite)), SubSite)
          NewSetting.Name = SubSiteName
          Return NewSetting
        End If
        Dim NewSubSite As New SubSite
        NewSubSite.NotLoadabled = True
        NewSubSite.Name = DefaultSiteConfiguration()
        Return NewSubSite
      End Function

    End Class

    Function CurrentMasterPage() As Components.MasterPageEnhanced
      Dim Page As Page = CType(HttpContext.Current.Handler, UI.Page)
      Return CType(Page.Master, Components.MasterPageEnhanced)
    End Function

    Function CurrentSubSiteName() As String
      Dim SubSiteName As String = CStr(HttpContext.Current.Items("ss"))
      If Not String.IsNullOrEmpty(SubSiteName) Then
        Return SubSiteName
      Else
        SubSiteName = HttpContext.Current.Request.QueryString("ss")
        If Not String.IsNullOrEmpty(SubSiteName) Then
          Return SubSiteName
        Else
          Dim Domain As DomainConfiguration = CurrentDomainConfiguration()
          If CBool(Domain.SubSitesAvailableLength()) Then
            SubSiteName = Domain.SubSitesAvailable()(0)
          Else
            SubSiteName = Config.SubSite.DefaultSiteConfiguration()
          End If
          HttpContext.Current.Items("ss") = SubSiteName
          Return SubSiteName
        End If
      End If
    End Function

    Function CurrentSetting() As Config.SubSite
      Dim Setting As Config.SubSite = CType(HttpContext.Current.Items("CurrentSetting"), SubSite)
      If Setting Is Nothing Then
        Dim SubSiteName As String = CurrentSubSiteName()
        Setting = CType(Config.SubSite.Load.GetItem(SubSiteName), SubSite)
        HttpContext.Current.Items("CurrentSetting") = Setting
      End If
      Return Setting
    End Function

    Private Sub UpdateAllSubSiteNames()
      Static Names As New Collections.Specialized.StringCollection
      Dim Path As String = MapPath(Config.SubSitesSubDirectory)
      For Each File As String In IO.Directory.GetFiles(Path, "*.xml")
        Dim Name As String = Mid(ReplaceBin(File, "/", "\"), InStrRev(File, "\") + 1)
        Name = Extension.Left(Name, Len(Name) - 4)
        Names.Add(Name)
      Next
      If Names.Count = 0 Then
        Names.Add(SubSite.DefaultSiteConfiguration())
      End If
      AllSubSiteNames = Names
    End Sub

    Private SubSiteNames As New Collections.Specialized.StringCollection
    Property AllSubSiteNames() As Collections.Specialized.StringCollection
      Get
        If SubSiteNames.Count = 0 Then
          UpdateAllSubSiteNames()
        End If
        Return SubSiteNames
      End Get
      Set(ByVal value As Collections.Specialized.StringCollection)
        SubSiteNames = value
      End Set
    End Property

    Public Function AllSubSite() As Collections.Generic.List(Of SubSite)
      Dim Collection As New Collections.Generic.List(Of SubSite)
      For Each Name As String In AllSubSiteNames
        Dim SubSite As SubSite = CType(Config.SubSite.Load.GetItem(Name), Config.SubSite)
        If SubSite IsNot Nothing Then
          Collection.Add(SubSite)
        End If
      Next
      Return Collection
    End Function

    Function AllSkinID() As Collections.Specialized.StringCollection
      AllSkinID = New Collections.Specialized.StringCollection
      Dim Path As String = MapPath(Config.SkinsSubDirectory)
      Dim Directory As New IO.DirectoryInfo(Path)
      For Each File As IO.FileInfo In Directory.GetFiles("*.xml")
        Dim ID As String = Extension.Left(File.Name, Len(File.Name) - 4)
        AllSkinID.Add(ID)
      Next
    End Function

    Private DomainNames As StringCollection

    Function AllDomainNamesNotConnected() As StringCollection
      Dim Domains As New StringCollection
      Dim MyIp As String = Extension.MyIp
      For Each Domain As String In AllDomainNames()
        Try
          Dim IPs As System.Net.IPAddress() = System.Net.Dns.GetHostAddresses(Domain)
          If IPs(0).ToString <> MyIp Then
            Domains.Add(Domain)
          End If
        Catch ex As Exception
          Domains.Add(Domain)
        End Try
      Next
      Return Domains
    End Function
    Private SyncDomains As New Object
    Function AllDomainNames() As StringCollection
      SyncLock SyncDomains
        If DomainNames Is Nothing Then
          Dim Domains As New StringCollection
          Dim Path As String = MapPath(Config.DomainConfigurationsSubDirectory)
          Dim Directory As New IO.DirectoryInfo(Path)
          For Each Dir As IO.DirectoryInfo In Directory.GetDirectories()
            Dim Domain As String = Dir.Name
            If Not Domain.First = "#"c Then
              Domains.Add(Domain)
            End If
          Next
          DomainNames = Domains
        End If
        Return DomainNames
      End SyncLock
    End Function

    Public Class Skin
      Public Shared CacheSkin As New System.Collections.Specialized.StringDictionary
      Public Shared SkinUseCustomSetup As New System.Collections.Generic.SortedDictionary(Of String, Boolean)

      Shared Sub RemoveSkinFromCache(Id As String)
        SyncLock CacheSkin
          Id = Id & vbTab
          Dim RemoveList As New StringCollection
          For Each Key As String In CacheSkin.Keys
            If Key.StartsWith(Id) Then
              RemoveList.Add(Key)
            End If
          Next
          For Each Key As String In RemoveList
            CacheSkin.Remove(Key)
          Next
        End SyncLock
      End Sub

      Public Id As String = "default"
      Public Shared SkinSetups As New Collections.Generic.Dictionary(Of String, SkinSetup)

      Function SkinSetup() As SkinSetup
        Dim ReturnValue As SkinSetup = Nothing
        Dim Key As String = SkinSetupCacheKey()
        SyncLock SkinSetups
          If SkinSetups.ContainsKey(Key) Then
            Return SkinSetups(Key)
          Else
            ReturnValue = Config.SkinSetup.Load(Setups)
            'Key = SkinSetupCacheKey() 'Refresh key for custom setup
            SkinSetups.Add(Key, ReturnValue)
          End If
        End SyncLock
        Return ReturnValue
      End Function

      Private Function SkinSetupCacheKey() As String
        Dim Setting As SubSite = CurrentSetting()
        If PluginManager.Plugin.UseSkinCustomization(Setting) Then
          Return vbTab & Setting.Name
        Else
          Return Id
        End If

        'Dim SiteName As String = CurrentSubSiteName()
        'If Skin.SkinUseCustomSetup.ContainsKey(SiteName) Then
        '  Return vbTab & SiteName
        'Else
        '  Return Id
        'End If
      End Function

      'Configuration reference
      Public Styles As New System.Collections.Specialized.StringCollection
      Public Setups As New System.Collections.Specialized.StringCollection

      'Private Container
      ReadOnly Property FolderIcons() As String
        Get
          Dim Skin = SkinSetup()
          If Skin IsNot Nothing AndAlso Skin.Variables("foldericons") IsNot Nothing Then
            Dim Folder As String = Skin.Variables("foldericons")
            If Not String.IsNullOrEmpty(Folder) Then
              Return "icons/" & Folder
            Else
              Return ""
            End If
          Else
            Return "icons/" & "48"
          End If
        End Get
      End Property
      Public FolderToolIcons As String = "toolicons"
      Private Shared SyncCollection As New Object
      Shared SkinsCollection As New Collections.Generic.Dictionary(Of String, Skin)
      Shared Function Load(ByVal ID As String) As Skin
        SyncLock SyncCollection
          If SkinsCollection.ContainsKey(ID) Then
            Return SkinsCollection(ID)
          Else
            Dim NewSkin As New Skin
            NewSkin.Id = ID
            Dim myXmlDocument As New System.Xml.XmlDocument
            Try
              Dim NameFile As String = MapPath(SkinsSubDirectory & "\" & ID & ".xml")
              If Not IO.File.Exists(NameFile) Then
                NameFile = MapPath(SkinsSubDirectory & "\default.xml")
              End If
              myXmlDocument.LoadXml(ReadAll(NameFile))
              Dim Node, Element As System.Xml.XmlNode
              For N As Integer = 0 To myXmlDocument.ChildNodes.Count - 1
                Node = myXmlDocument.ChildNodes(N)
                If StrComp(Node.Name, "skin", CompareMethod.Text) = 0 Then
                  For N2 As Integer = 0 To Node.ChildNodes.Count - 1
                    Element = Node.ChildNodes(N2)
                    'Select Case LCase(Element.Name)
                    Select Case Element.Attributes("key").Value.ToLower()
                      Case "style"
                        NewSkin.Styles.Add(Element.Attributes("value").Value)
                      Case "setup"
                        NewSkin.Setups.Add(Element.Attributes("value").Value)
                    End Select
                  Next
                End If
              Next
            Catch ex As Exception

            End Try
            SkinsCollection.Add(ID, NewSkin)
            Load = NewSkin
          End If
        End SyncLock
      End Function

    End Class

    Class SkinSetup
      Public Variables As New Collections.Specialized.NameValueCollection 'Don't change this type of collection
      Shared Function Load(ByVal IDs As System.Collections.Specialized.StringCollection, Optional ExcludePluginSetting As Boolean = False) As SkinSetup
        Dim Setting As SubSite = CurrentSetting()
        Dim SkinSetup As SkinSetup = Nothing

        Dim Thread As System.Threading.Thread = Threading.Thread.CurrentThread
        Dim StackTrace As System.Diagnostics.StackTrace = New System.Diagnostics.StackTrace(Thread, False)
        If StackTrace.GetFrames IsNot Nothing Then
          For Each StackFrame In StackTrace.GetFrames
            If StackFrame.GetMethod.Name = "RaiseGetSkinSetupEvent" OrElse String.IsNullOrEmpty(StackFrame.GetMethod.Name) Then
              ExcludePluginSetting = True
              Exit For
            End If
          Next
        End If

        If Not ExcludePluginSetting Then
          RaiseGetSkinSetupEvent(SkinSetup, Setting)
          Dim SubSiteName As String = CurrentSubSiteName()
          If SkinSetup IsNot Nothing Then
            SyncLock Skin.SkinUseCustomSetup
              If Skin.SkinUseCustomSetup.ContainsKey(SubSiteName) Then
                Skin.SkinUseCustomSetup.Remove(SubSiteName)
              End If
              Skin.SkinUseCustomSetup.Add(SubSiteName, True)
            End SyncLock
            Return SkinSetup
          Else
            SyncLock Skin.SkinUseCustomSetup
              If Skin.SkinUseCustomSetup.ContainsKey(SubSiteName) Then
                Skin.SkinUseCustomSetup.Remove(SubSiteName)
              End If
            End SyncLock
          End If
        End If

        For Each XmlDocument As System.Xml.XmlDocument In LoadXmlDocuments(IDs)
          Try
            Dim Node, Element As System.Xml.XmlNode
            For N As Integer = 0 To XmlDocument.ChildNodes.Count - 1
              Node = XmlDocument.ChildNodes(N)
              If StrComp(Node.Name, "variables", CompareMethod.Text) = 0 Then
                SkinSetup = New SkinSetup
                For N2 As Integer = 0 To Node.ChildNodes.Count - 1
                  Element = Node.ChildNodes(N2)
                  If Element.Attributes IsNot Nothing Then
                    Dim Key As String = Nothing, Value As String = Nothing
                    For Na As Integer = 0 To Element.Attributes.Count - 1
                      Dim Attribute As String = LCase(Element.Attributes.Item(Na).Name)
                      Select Case Attribute
                        Case "key"
                          Key = Element.Attributes.Item(Na).Value
                        Case "value"
                          Value = Element.Attributes.Item(Na).Value
                      End Select
                    Next
                    If Not String.IsNullOrEmpty(Key) Then
                      SkinSetup.Variables.Add(Key, Value)
                    End If
                  End If
                Next
                Exit For
              End If
            Next
          Catch ex As Exception
          End Try
        Next
        Return SkinSetup
      End Function

      Private Shared Function LoadXmlDocuments(ByVal IDs As System.Collections.Specialized.StringCollection) As Collections.Generic.List(Of System.Xml.XmlDocument)
        Dim Xmls As New Collections.Generic.List(Of System.Xml.XmlDocument)
        For Each ID As String In IDs
          Dim XmlDocument As New System.Xml.XmlDocument
          Try
            XmlDocument.Load(MapPath(SkinsSubDirectory & "\setups\" & ID & ".xml"))
            Xmls.Add(XmlDocument)
          Catch ex As Exception
          End Try
        Next
        Return Xmls
      End Function
    End Class

    Function ControlSkin(ByVal Src As String, Optional ByVal Description As String = Nothing, Optional ByVal NavigeteUrl As String = Nothing) As Control
      If Not Src Is Nothing Then
        Dim Parts As String() = Src.Split("?"c)
        Dim Query As String = Nothing
        Dim File As String = Parts(0)
        If Parts.Length = 2 Then
          Query = Parts(1)
        End If
        Select Case GetFileExtension(File)
          Case "gif", "jpg", "jpeg", "png"
            Dim Image As New HtmlControls.HtmlImage
            Dim Width As Integer = 0
            Dim Height As Integer = 0
            CacheImageSize.LoadWidthHeight(File, Width, Height)
            Image.Src = ReplaceBin(File, " ", "%20")
            Image.Width = Width
            Image.Height = Height
            If Not String.IsNullOrEmpty(Description) Then
              Description = EncodingAttribute(Description)
              Image.Attributes("alt") = Description
              Image.Attributes("title") = Description
            Else
              Image.Attributes("alt") = DomainName(HttpContext.Current.Request)
            End If
            If Not NavigeteUrl Is Nothing Then
              Dim HiperLink As New WebControls.HyperLink
              HiperLink.NavigateUrl = NavigeteUrl
              HiperLink.Controls.Add(Image)
              Return HiperLink
            Else
              Return Image
            End If
          Case "swf"
            Dim Width As Integer = 200
            Dim Height As Integer = 200
            If Query IsNot Nothing Then
              For Each Part As String In Query.Split("&"c)
                Dim KeyValue As String() = Part.Split("="c)
                Select Case LCase(KeyValue(0))
                  Case "width"
                    Width = ValInt(KeyValue(1))
                  Case "height"
                    Height = ValInt(KeyValue(1))
                End Select
              Next
            End If
            Return FlashControl(File, Width, Height)
        End Select
      End If
      Return Nothing
    End Function

    Function BackgroundSkin(ByVal Background As String) As String
      If Not Background Is Nothing Then
        If Extension.Left(Background, 1) = "#" Then
          Return " bgcolor=""" & Background & """"
        Else
          Return " background=""" & Background & """"
        End If
      Else
        Return ""
      End If
    End Function

    Class DomainConfiguration
      Private Const ListSubSitesNameFile As String = "SubSites.txt"
      Private Const ListNotSubSitesNameFile As String = "NotSubSites.txt"
      Private Const RedirectNameFile As String = "Redirect.txt"
      'Public Name As String
      Private m_Name As String
      Property Name() As String
        Get
          Return m_Name
        End Get
        Set(ByVal Value As String)
          If Value.StartsWith("www.") Then
            m_Name = Mid(Value, 5)
          Else
            m_Name = Value
          End If
        End Set
      End Property
      Function Href() As String
        Return "http://" & m_Name
      End Function
      Private m_SubSitesAvailable As New Collections.Specialized.StringCollection
      Private m_SubSitesNotAvailable As New Collections.Specialized.StringCollection
      Function SubSitesAvailableLength() As Integer
        Return m_SubSitesAvailable.Count
      End Function
      Function SubSitesAvailable() As String()
        SyncLock m_SubSitesAvailable
          Dim Sities(m_SubSitesAvailable.Count - 1) As String
          If CBool(m_SubSitesAvailable.Count) Then
            m_SubSitesAvailable.CopyTo(Sities, 0)
          End If
          Return Sities
        End SyncLock
      End Function
      Function SubSitesNotAvailable() As String()
        SyncLock m_SubSitesNotAvailable
          Dim Sities(m_SubSitesNotAvailable.Count - 1) As String
          If CBool(m_SubSitesNotAvailable.Count) Then
            m_SubSitesNotAvailable.CopyTo(Sities, 0)
          End If
          Return Sities
        End SyncLock
      End Function
      Public AvailableAllSubSite As Boolean
      Private m_Redirect As String
      Public Property Redirect As String
        Get
          Return m_Redirect
        End Get
        Set(value As String)
          If value <> m_Redirect Then
            m_Redirect = value
            WriteAll(value, Me.Path & DomainConfiguration.RedirectNameFile, False, False)
          End If
        End Set
      End Property
      Public Skin As Skin
      Public Sub New()
        'SubSitesAvailable = AllSubSiteNames()
      End Sub
      Private Shared Function LoadFromFile(ByVal Name As String) As DomainConfiguration
        Dim NewDomain As New DomainConfiguration

        NewDomain.Name = Name
        Dim Records() As String

        Dim File As String
        File = NewDomain.Path & RedirectNameFile
        If System.IO.File.Exists(File) Then
          Records = ReadAllRows(File)
          If Records IsNot Nothing AndAlso CBool(Records.Length) Then
            NewDomain.m_Redirect = Records(0)
          End If
        End If

        If String.IsNullOrEmpty(NewDomain.Redirect) Then

          SyncLock NewDomain.m_SubSitesAvailable
            Records = Nothing
            File = NewDomain.Path & ListSubSitesNameFile
            If System.IO.File.Exists(File) Then
              Records = ReadAllRows(File)
              If Records IsNot Nothing Then
                For Each Record As String In Records
                  If Not String.IsNullOrEmpty(Record) Then
                    NewDomain.m_SubSitesAvailable.Add(Record)
                  End If
                Next
              End If
            End If

            If NewDomain.SubSitesAvailable() Is Nothing Then
              NewDomain.AvailableAllSubSite = True
              NewDomain.m_SubSitesAvailable = AllSubSiteNames()
            End If
            Records = Nothing
            File = NewDomain.Path & ListNotSubSitesNameFile
            If System.IO.File.Exists(File) Then
              'Remove specified subsites
              Records = Nothing
              Records = ReadAllRows(File)
              If Records IsNot Nothing Then
                For Each Record As String In Records
                  If Not String.IsNullOrEmpty(Record) Then
                    NewDomain.m_SubSitesNotAvailable.Add(Record)
                    If NewDomain.m_SubSitesAvailable.Contains(Record) Then
                      NewDomain.m_SubSitesAvailable.Remove(Record)
                    End If
                  End If
                Next
              End If
            End If
          End SyncLock
        End If
        Return NewDomain
      End Function

      Sub Save(Optional ByVal Name As String = Nothing)
        If Not Name Is Nothing Then
          Me.Name = Name
        End If

        'Create a folder for host configuration
        Dim Dir As New System.IO.DirectoryInfo(Path)
        If Not Dir.Exists Then
          SyncLock SyncDomains
            If DomainNames Is Nothing Then
              DomainNames = New StringCollection
            End If
            DomainNames.Add(Me.Name)
          End SyncLock
          Dir.Create()
        End If

        Dim Data As String = Nothing
        Dim AddCrLf As Boolean = False
        For Each SubSiteName In SubSitesAvailable()
          If AddCrLf Then
            Data &= vbCrLf
          End If
          Data &= SubSiteName
          AddCrLf = True
        Next
        WriteAll(Data, Path() & ListSubSitesNameFile)

        Data = Nothing
        AddCrLf = False
        For Each SubSiteName In SubSitesNotAvailable()
          If AddCrLf Then
            Data &= vbCrLf
          End If
          Data &= SubSiteName
          AddCrLf = True
        Next
        WriteAll(Data, Path() & ListNotSubSitesNameFile)
        NotifyChangement(GetType(DomainConfiguration), Name)
        Dim Thread As New System.Threading.Thread(AddressOf UpdateDataCorrelatedToDomainConfiguration)
        Thread.Name = "UpdateDataCorrelatedToDomainConfiguration"
        Thread.Priority = Threading.ThreadPriority.Normal
        Thread.IsBackground = True
        Thread.Start()
      End Sub

      Shared Sub UpdateDataCorrelatedToDomainConfiguration()
        UpdateSubSiteToDomain()
        FindDomainForSearchFunction()
      End Sub

      Private Shared Sub UpdateSubSiteToDomain()
        Dim SubSiteToDomain As New Collections.Specialized.StringDictionary
        Dim ArchiveToDomain As Collections.Generic.Dictionary(Of Integer, String)
        Dim ArchiveToDomainCollection As New Collections.Generic.Dictionary(Of Language, Collections.Generic.Dictionary(Of Integer, String))
        For Each Language In LanguagesUsed
          ArchiveToDomain = New Collections.Generic.Dictionary(Of Integer, String)
          ArchiveToDomainCollection.Add(Language, New Collections.Generic.Dictionary(Of Integer, String))
        Next

        'SyncLock Links
        Dim AllDomain As StringCollection = AllDomainNames()
        If AllDomain IsNot Nothing Then
          For Each DomainName As String In AllDomain
            If Not DomainName.First = "#"c Then
              Dim Domain As Config.DomainConfiguration = Config.DomainConfiguration.Load(DomainName)
              If Not Domain.SubSitesAvailable() Is Nothing Then
                For Each SubSiteName As String In Domain.SubSitesAvailable()
                  Dim Setting As SubSite = CType(Config.SubSite.Load.GetItem(SubSiteName), SubSite)
                  If Domain.SubSitesAvailableLength() = 1 Then
                    If SubSiteToDomain.ContainsKey(SubSiteName) Then
                      SubSiteToDomain.Remove(SubSiteName)
                    End If
                  End If
                  If Setting Is Nothing Then
                    Domain.RemoveSubSite(SubSiteName)
                  Else
                    If SubSiteToDomain(SubSiteName) Is Nothing Then
                      SubSiteToDomain.Add(SubSiteName, DomainName)
                    End If
                    If Setting.Archive IsNot Nothing Then
                      ArchiveToDomain = ArchiveToDomainCollection(Setting.Language)
                      For Each Archive As Integer In Setting.Archive
                        If Setting.Archive.Length = 1 Then
                          If ArchiveToDomain.ContainsKey(Archive) Then
                            ArchiveToDomain.Remove(Archive)
                          End If
                          ArchiveToDomain.Add(Archive, DomainName)
                        Else
                          If Not ArchiveToDomain.ContainsKey(Archive) Then
                            ArchiveToDomain.Add(Archive, DomainName)
                          End If
                        End If
                      Next
                    End If
                  End If
                Next
              End If
            End If
          Next
        End If
        Common.SubSiteToDomain = SubSiteToDomain

        Common.ArchiveToDomain = ArchiveToDomainCollection
        DetectAllMultilingualSubSite()
      End Sub

      Function SubSites() As Collections.Generic.List(Of SubSite)
        Dim Collection As New Collections.Generic.List(Of SubSite)
        Dim Availables As String() = Me.SubSitesAvailable()
        If CBool(Availables.Length) Then
          For Each Name As String In Availables
            Dim Setup As SubSite = CType(Config.SubSite.Load.GetItem(Name), SubSite)
            If Setup Is Nothing Then
              RemoveSubSite(Name)
            Else
              Collection.Add(Setup)
            End If
          Next
        End If
        Return Collection
      End Function

      Private Shared Sub FindDomainForSearchFunction()
        Dim Domains As StringCollection = AllDomainNames()
        If Domains IsNot Nothing Then
          For Each DomainName As String In Domains
            Dim Domain As Config.DomainConfiguration = Config.DomainConfiguration.Load(DomainName)
            If Not Domain.SubSitesAvailable() Is Nothing Then
              If Domain.SubSitesAvailableLength() = 1 Then
                Dim FirstSubSite As SubSite = CType(Config.SubSite.Load.GetItem(Domain.SubSitesAvailable()(0)), Config.SubSite)
                If FirstSubSite.IsTrue("Enabled", "Search") AndAlso FirstSubSite.PluginInHomePage = "search" Then 'c# .Common.->.
                  If Not CollectionDomainForSearchFunction.ContainsKey(FirstSubSite.Language) Then
                    CollectionDomainForSearchFunction.Add(FirstSubSite.Language, Domain.Name)
                  End If
                End If
              End If
            End If
          Next
        End If
      End Sub

      Public Shared Function DomainForSearchFunction(ByVal Language As LanguageManager.Language) As String
        If CollectionDomainForSearchFunction.ContainsKey(Language) Then
          Return CollectionDomainForSearchFunction(Language)
        End If
        Return Nothing
      End Function

      Private Shared CollectionDomainForSearchFunction As New System.Collections.Generic.Dictionary(Of LanguageManager.Language, String)

      Private Function Path() As String
        Return MapPath(DomainConfigurationsSubDirectory & "/" & Me.Name & "/")
      End Function
      Shared Function Load(ByVal Domain As String) As DomainConfiguration
        If Domain IsNot Nothing Then
          SyncLock AllDomainConfiguration
            If AllDomainConfiguration.ContainsKey(Domain) Then
              Return AllDomainConfiguration.Item(Domain)
            Else
              Dim DomainConfiguration As Config.DomainConfiguration = Config.DomainConfiguration.LoadFromFile(Domain)
              AllDomainConfiguration.Add(Domain, DomainConfiguration)
              Return DomainConfiguration
            End If
          End SyncLock
        End If
        Return Nothing
      End Function

      Private Shared AllDomainConfiguration As New Collections.Generic.Dictionary(Of String, DomainConfiguration)

      Public Sub RemoveSubSite(ByVal Setup As String)
        SyncLock m_SubSitesAvailable
          SyncLock m_SubSitesNotAvailable
            Me.m_SubSitesNotAvailable.Clear()
          End SyncLock
          If Me.m_SubSitesAvailable.Contains(Setup) Then
            Me.m_SubSitesAvailable.Remove(Setup)
          End If
        End SyncLock
        AvailableAllSubSite = False
        Save()
      End Sub

      Public Sub AddSubSite(ByVal Setup As String)
        SyncLock m_SubSitesAvailable
          SyncLock m_SubSitesNotAvailable
            Me.m_SubSitesNotAvailable.Clear()
          End SyncLock
          If Not Me.m_SubSitesAvailable.Contains(Setup) Then
            Me.m_SubSitesAvailable.Add(Setup)
          End If
          AvailableAllSubSite = False
          Save()
        End SyncLock
      End Sub

      'This line initialize a pipeline multi-server notification necessary to notify a changement of Domain Configuration to another server
      Private Shared EnableServerNotification As Boolean = StartupServerNotification()
      Private Shared Function StartupServerNotification() As Boolean
        AddActionForNotification(GetType(DomainConfiguration), New Pipeline.NotifyChangementDelegate(AddressOf ChangedByAnotherServer))
        Return True
      End Function

      Public Shared Sub ChangedByAnotherServer(ByVal Domain As String)
        SyncLock AllDomainConfiguration
          If AllDomainConfiguration.ContainsKey(Domain) Then
            AllDomainConfiguration.Remove(Domain)
          End If
        End SyncLock
      End Sub


    End Class

    Public Function CurrentDomainConfiguration() As DomainConfiguration
      If HttpContext.Current IsNot Nothing Then
        Dim DomainConfiguration As DomainConfiguration = Nothing
        If HttpContext.Current.Session IsNot Nothing Then
          'Sesion is Nothing (in context page error 404)
          DomainConfiguration = CType(HttpContext.Current.Session("Config " & HttpContext.Current.Request.ServerVariables("HTTP_HOST")), Config.DomainConfiguration)
          'Note: The adding of this string HttpContext.Current.Request.ServerVariables("HTTP_HOST") prevent a bug if the navigator jump from II level domain to III leve, ex.: mydomain.xx > it.mydomain.xx
        End If
        If DomainConfiguration IsNot Nothing Then
          Return DomainConfiguration
        Else
          DomainConfiguration = Config.DomainConfiguration.Load(CurrentDomain)
          If HttpContext.Current.Session IsNot Nothing Then
            'Sesion is Nothing (in context page error 404)
            HttpContext.Current.Session("Config " & HttpContext.Current.Request.ServerVariables("HTTP_HOST")) = DomainConfiguration
          End If
          Return DomainConfiguration
        End If
      End If
      Return Nothing
    End Function
  End Module

End Namespace
