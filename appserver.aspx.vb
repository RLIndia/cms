'Note by Andrea Bruno (the King developer)
'Use this blank page to create a new plugin: Modify Blank.aspx & Blank.aspx.vb add extra functions in your web abblication!
Imports WebApplication
Imports System.Xml.Serialization

Partial Class AppServer
  'REPLACE THE NAME OF THIS CLASS AND Inherits ATTRIBUTE IN <%@ Page...%> WITH A PLUGIN NAME
  'RENAME A PAGE Blank.aspx
  'EXEMP.: IF THE PAGE NAME IS PluginOne.aspx, THE CLASS NAME MUST BE pluginOne AND SET ATTRIBUTE IN  <%@ Page Inherits="PluginOne" %>  

  Inherits System.Web.UI.Page
  Private Setting As SubSite
  Private CurrentUser As User

  'REMOVE REM OF NEXT LINES:
  Shared WithEvents Plugin As PluginManager.Plugin = Initialize()
  Shared Function Initialize() As PluginManager.Plugin
    If Plugin Is Nothing Then Plugin = New PluginManager.Plugin(AddressOf Description, , , , , )
    Return Plugin
  End Function
  Shared Sub New()
    Initialize()
  End Sub

  Shared Function Description(ByVal Language As LanguageManager.Language, ByVal ShortDescription As Boolean) As String
    If ShortDescription Then
      'Return the short description of plugin
      Select Case Language
        Case Else
          Return "App Server"
          'Return Phrase(Language, 0)	'Replace 0 with appropriate phrase ID
      End Select
    Else
      'Return the long description of plugin
      Select Case Language
        Case Else
          Return "This plugin is a server for external applications"
          'Return Phrase(Language, 0)	'Replace 0 with appropriate phrase ID
      End Select
    End If
  End Function

  Shared Function GetHallOfFame(AppName As String) As HallOfFame
    Static AllHallOfFame As New Dictionary(Of String, HallOfFame)
    Dim Result As HallOfFame = Nothing
    SyncLock AllHallOfFame
      If Not AllHallOfFame.TryGetValue(AppName, Result) Then
        Result = CType(LoadObject(GetType(HallOfFame), AppName), HallOfFame)
        If Result Is Nothing Then Result = New HallOfFame
        AllHallOfFame.Add(AppName, Result)
      End If
    End SyncLock
    Return Result
  End Function

  Class SpolerElement
    Sub New(AppName As String, FromUser As String, ToUser As String, ObjectName As String, XmlObject As String, SecTimeout As Integer)
      Me.AppName = AppName : Me.FromUser = FromUser : Me.ToUser = ToUser : Me.ObjectName = ObjectName : Me.XmlObject = XmlObject
      SyncLock Collection
        Collection.Add(Me)
      End SyncLock
      Timeout.Interval = SecTimeout * 1000
      Timeout.Start()
    End Sub
    Public Shared Collection As New List(Of SpolerElement)
    Public WithEvents Timeout As New Timers.Timer
    Public AppName As String
    Public FromUser As String
    Public ToUser As String
    Public ObjectName As String
    Public XmlObject As String

    Private Sub Timeout_Elapsed(sender As Object, e As Timers.ElapsedEventArgs) Handles Timeout.Elapsed
      Timeout.Stop()
      SyncLock Collection
        If Collection.Contains(Me) Then
          Collection.Remove(Me)
        End If
      End SyncLock
    End Sub
  End Class


  Class CancellRequest
    Public Shared Collection As New List(Of CancellRequest)
    Sub New(FromUser As String, ToUser As String)
      Me.FromUser = FromUser
      Me.ToUser = ToUser
      Me.Expired = Now.AddSeconds(2)
      Collection.Add(Me)
    End Sub
    Public FromUser As String
    Public ToUser As String
    Public Expired As Date
  End Class

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache)
    'Response.Cache.SetExpires(Now.AddDays(-1))


    Dim AppName As String = Request.QueryString("app")
    Dim ToUser As String = Request.QueryString("touser")
    Dim FromUser As String = Request.QueryString("fromuser")
    Dim SecTimeout As Integer
    If Request.QueryString.AllKeys.Contains("sectimeout") Then
      SecTimeout = Integer.Parse(Request.QueryString("sectimeout"))
    End If
    Dim SecWaitAnswer As Integer
    If Request.QueryString.AllKeys.Contains("secwaitanswer") Then
      SecWaitAnswer = Integer.Parse(Request.QueryString("secwaitanswer"))
    End If

    SyncLock CancellRequest.Collection
      For N = CancellRequest.Collection.Count - 1 To 0 Step -1
        Dim CancellRequest As CancellRequest = CancellRequest.Collection(N)
        If CancellRequest.FromUser = FromUser Then
          CancellRequest.Collection.Remove(CancellRequest)
        End If
      Next
      If Request.QueryString("cancellrequest") = "true" Then
        Dim CancellRequest = New CancellRequest(FromUser, ToUser)
      End If
    End SyncLock

    SyncLock SpolerElement.Collection
      If Request.QueryString("removeobjects") = "true" Then
        For N = SpolerElement.Collection.Count - 1 To 0 Step -1
          Dim Element = SpolerElement.Collection(N)
          If Element.ToUser = FromUser Then
            SpolerElement.Collection.Remove(Element)
          End If
        Next
      End If
    End SyncLock

    SyncLock SpolerElement.Collection
      If Request.QueryString("removemyobjects") = "true" Then
        For N = SpolerElement.Collection.Count - 1 To 0 Step -1
          Dim Element = SpolerElement.Collection(N)
          If Element.FromUser = FromUser Then
            SpolerElement.Collection.Remove(Element)
          End If
        Next
      End If
    End SyncLock


    'If Request.HttpMethod = "POST" Then
    Select Case Request.QueryString("post")
      Case "RecordPlayer"
        Dim HallOfFame As HallOfFame = GetHallOfFame(AppName)
        Dim Record As RecordPlayer
        Dim XML As New System.Xml.Serialization.XmlSerializer(GetType(RecordPlayer))
        Dim XmlObject As String = Request.Headers("object")
        Record = CType(XML.Deserialize(New IO.StringReader(XmlObject)), RecordPlayer)
        HallOfFame.AddRecord(Record)
        SaveObject(HallOfFame, AppName)
        Response.End()
      Case "Log"
        Dim XML As New System.Xml.Serialization.XmlSerializer(GetType(Log))
        Dim XmlObject As String = Request.Headers("object")
        Dim Log As Log = CType(XML.Deserialize(New IO.StringReader(XmlObject)), Log)
        Extension.Log(AppName & " " & Log.Name, 100, Log.Text)
        Response.End()
    End Select
    'Else
    If Request.QueryString("post") <> "" Then
      Dim XmlObject As String = Request.Headers("object")
      Dim se = New SpolerElement(AppName, FromUser, ToUser, Request.QueryString("post"), XmlObject, SecTimeout)
    End If

    If Request.QueryString("request") = "" Then
      SendObject(AppName, FromUser, ToUser, SecWaitAnswer)
    Else
      Select Case CType(Request.QueryString("request"), RequestScope)
        Case RequestScope.HallOfFame
          Dim HallOfFame As HallOfFame = GetHallOfFame(AppName)
          Response.ContentType = "text/xml;charset=utf-8"
          Dim xml As New XmlSerializer(GetType(HallOfFame))
          Dim xmlns As New XmlSerializerNamespaces
          xmlns.Add(String.Empty, String.Empty)
          xml.Serialize(Response.OutputStream, HallOfFame, xmlns)
          Response.End()
      End Select
    End If
    'End If
  End Sub

  Sub SendObject(AppName As String, User As String, ToUser As String, SecWaitObject As Integer)
    Dim Sended As Boolean
    If Request.QueryString("nogetobject") <> "true" Then
      Dim Start = Now
      Do
        SyncLock SpolerElement.Collection
          'Execute eventually request of cancellation
          SyncLock CancellRequest.Collection
            Dim Flag As Boolean = False
            For N = CancellRequest.Collection.Count - 1 To 0 Step -1
              Dim CancellRequest As CancellRequest = CancellRequest.Collection(N)
              If Now > CancellRequest.Expired Then
                CancellRequest.Collection.Remove(CancellRequest)
              End If
              If CancellRequest.FromUser = User AndAlso CancellRequest.ToUser = ToUser Then
                CancellRequest.Collection.Remove(CancellRequest)
                Flag = True
              End If
            Next
            If Flag Then
              Response.ContentType = "text/xml;charset=utf-8"
              Response.End()
              Exit Do
            End If
          End SyncLock

          'Send object if is in the spooler
          For Each Element In SpolerElement.Collection
            If AppName = Element.AppName AndAlso Element.FromUser <> User Then
              If User = Element.ToUser OrElse Element.ToUser = "" Then
                If ToUser = Element.FromUser OrElse (ToUser = "" AndAlso Element.ToUser = "") Then
                  SpolerElement.Collection.Remove(Element)
                  Dim ObjectVector As New ObjectVector(Element.FromUser, Element.ObjectName, Element.XmlObject)
                  Response.ContentType = "text/xml;charset=utf-8"
                  Dim xml As New XmlSerializer(GetType(ObjectVector))
                  Dim xmlns As New XmlSerializerNamespaces
                  xmlns.Add(String.Empty, String.Empty)
                  xml.Serialize(Response.OutputStream, ObjectVector, xmlns)
                  Sended = True
                  Response.End()
                  Exit Do
                End If
              End If
            End If
          Next
        End SyncLock
        If SecWaitObject <> 0 Then
          Threading.Thread.Sleep(250)
        End If
      Loop Until (Now - Start).TotalSeconds >= SecWaitObject
    End If
    If Not Sended Then
      Response.Clear()
      Response.End()
    End If
  End Sub

  Class ObjectVector
    Sub New()
    End Sub
    Sub New(FromUser As String, ObjectName As String, XmlObject As String)
      Me.FromUser = FromUser : Me.ObjectName = ObjectName : Me.XmlObject = XmlObject
    End Sub
    Public FromUser As String
    Public ObjectName As String
    Public XmlObject As String
  End Class

  Class Log
    Public Name As String
    Public Text As String
  End Class

  Enum RequestScope
    HallOfFame
  End Enum

  Class RecordPlayer
    Public Property Id As String
    Public Property Name As String
    Public Property Position As Integer
    Public Property Record As Integer
    Public Property LastAccess As Date
  End Class

  Class HallOfFame
    Public Records As New List(Of RecordPlayer)
    Private Sub Sort()
      Records.Sort(Comparer)
      Dim N As Integer = 0
      For Each Record In Records
        N += 1
        Record.Position = N
      Next
    End Sub
    Private Sub RemoveOld()
      For N = Records.Count - 1 To 0 Step -1
        Dim Record = Records(N)
        Dim DateNow = Date.Now.ToUniversalTime
        If (DateNow - Record.LastAccess).Days > 30 Then
          Records.Remove(Record)
        End If
      Next
    End Sub
    Private Sub LimitMaxRecords()
      Do While Records.Count > 100
        Records.Remove(Records(Records.Count - 1))
      Loop
    End Sub
    Public Sub AddRecord(RecordPlayer As RecordPlayer)
      SyncLock Records
        For Each Record In Records
          If Record.Id = RecordPlayer.Id Then
            Records.Remove(Record)
            Exit For
          End If
        Next
        Records.Add(RecordPlayer)
        RemoveOld()
        Sort()
        LimitMaxRecords()
      End SyncLock
    End Sub
    Private Comparer As New RecordsComparer
    Public Class RecordsComparer
      Implements Collections.Generic.IComparer(Of RecordPlayer)
      Public Function Compare(ByVal RecordPlayer1 As RecordPlayer, ByVal RecordPlayer2 As RecordPlayer) As Integer Implements Collections.Generic.IComparer(Of RecordPlayer).Compare
        Return RecordPlayer2.Record - RecordPlayer1.Record
      End Function
    End Class
  End Class

End Class
