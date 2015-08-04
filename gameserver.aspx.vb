'Note by Andrea Bruno (the King developer)
'Use this blank page to create a new plugin: Modify Blank.aspx & Blank.aspx.vb add extra functions in your web abblication!
Imports WebApplication
Imports System.Xml.Serialization

Partial Class GameServer
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
          Return "Game Server"
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
    Dim Result As HallOfFame
    SyncLock AllHallOfFame
      If Not AllHallOfFame.TryGetValue(AppName, Result) Then
        Result = CType(LoadObject(GetType(HallOfFame), AppName), HallOfFame)
        If Result Is Nothing Then Result = New HallOfFame
        AllHallOfFame.Add(AppName, Result)
      End If
    End SyncLock
    Return Result
  End Function

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Dim AppName As String = Request.QueryString("app")
    If Request.HttpMethod = "POST" Then
      Select Case Request.QueryString("post")
        Case "RecordPlayer"
          Dim HallOfFame As HallOfFame = GetHallOfFame(AppName)
          Dim Record As RecordPlayer
          Dim XML As New System.Xml.Serialization.XmlSerializer(GetType(RecordPlayer))
          Dim postData As String = Request.Headers("object")
          Record = CType(XML.Deserialize(New IO.StringReader(postData)), RecordPlayer)
          HallOfFame.AddRecord(Record)
          SaveObject(HallOfFame, AppName)
          Response.End()
      End Select
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
  End Sub

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
