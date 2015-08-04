'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Imports Microsoft.VisualBasic

Namespace WebApplication
	Public Module Pipeline
		Private Directory As String = FileManager.MapPath(PipelineDirectory)
		Private PipelineFile As String = FileManager.MapPath(PipelineDirectory & "\" & My.Computer.Name)
    Private TimeOfLastNotify As Date
    Private Progressive As UInteger
    Private NRecord As Integer
    Public Sub NotifyChangement(ByVal Element As System.Type, ByVal Id As String)
      If RunInHostingMultiServer OrElse Element = GetType(HttpApplication) Then
        'Dim Execute As Threading.ThreadStart = Sub()
        '                                         Const SecTimeNotify As Integer = 5
        '                                         SyncLock Directory
        '                                           Dim Record As String = Pipeline.Progressive & vbTab & Now.ToUniversalTime().Ticks & vbTab & Element.FullName & vbTab & Id & vbCrLf
        '                                           Dim Append As Boolean
        '                                           If Microsoft.VisualBasic.DateAndTime.DateDiff(DateInterval.Second, Pipeline.TimeOfLastNotify, Now.ToUniversalTime()) < SecTimeNotify Then
        '                                             Append = True
        '                                             If Pipeline.NRecord = 50 Then
        '                                               System.Threading.Thread.Sleep(SecTimeNotify)
        '                                             End If
        '                                             Pipeline.NRecord += 1
        '                                           Else
        '                                             Pipeline.NRecord = 1
        '                                           End If
        '                                           FileManager.WriteAll(Record, PipelineFile, Append, False)
        '                                           Pipeline.TimeOfLastNotify = Now.ToUniversalTime()
        '                                           Pipeline.Progressive = CUInt(Pipeline.Progressive + 1)
        '                                         End SyncLock
        '                                       End Sub
        Dim Execute As Threading.ParameterizedThreadStart = AddressOf ExecuteNotifyChangement
        Dim Thread As System.Threading.Thread = New System.Threading.Thread(Execute)
        Thread.Priority = Threading.ThreadPriority.Normal
        Thread.IsBackground = True
        Thread.Start(New ExecuteNotifyChangementParameters(Element, Id))
      End If
    End Sub

    Private Class ExecuteNotifyChangementParameters
      Public Element As System.Type
      Public Id As String
      Public Sub New(Element As System.Type, Id As String)
        Me.Element = Element
        Me.Id = Id
      End Sub
    End Class

    Private Sub ExecuteNotifyChangement(parameter As Object)
      Dim Unboxing As ExecuteNotifyChangementParameters = CType(parameter, WebApplication.Pipeline.ExecuteNotifyChangementParameters)
      Dim Element As System.Type = Unboxing.Element
      Dim Id As String = Unboxing.Id

      Const SecTimeNotify As Integer = 5
      SyncLock Directory
        Dim Record As String = Pipeline.Progressive & vbTab & Now.ToUniversalTime().Ticks & vbTab & Element.FullName & vbTab & ID & vbCrLf
        Dim Append As Boolean = False
        If Microsoft.VisualBasic.DateAndTime.DateDiff(DateInterval.Second, Pipeline.TimeOfLastNotify, Now.ToUniversalTime()) < SecTimeNotify Then
          Append = True
          If Pipeline.NRecord = 50 Then
            System.Threading.Thread.Sleep(SecTimeNotify)
          End If
          Pipeline.NRecord += 1
        Else
          Pipeline.NRecord = 1
        End If
        FileManager.WriteAll(Record, PipelineFile, Append, False)
        Pipeline.TimeOfLastNotify = Now.ToUniversalTime()
        Pipeline.Progressive = CUInt(Pipeline.Progressive + 1)
      End SyncLock
    End Sub

    Public Sub StartPipelineComunication()
      PipelineDirectoryEvent(Nothing, Nothing, CacheItemRemovedReason.DependencyChanged)
      NotifyChangement(GetType(HttpApplication), "start")
    End Sub

    Public Sub EndPipelineComunication()
      Delete(PipelineFile)
      NotifyChangement(GetType(HttpApplication), "end")
    End Sub

    Private Sub PipelineDirectoryEvent(ByVal k As String, ByVal v As Object, ByVal r As CacheItemRemovedReason)
      If r <> CacheItemRemovedReason.Underused Then
        For Each File As String In System.IO.Directory.GetFiles(Directory)
          If File <> PipelineFile Then
            If WebCache(File) Is Nothing Then
              Dim Info As New IO.FileInfo(File)
              If DateDiff(DateInterval.Hour, Info.LastWriteTime.ToUniversalTime(), Now.ToUniversalTime()) < 2 Then
                WatchPipelineFile(File, Nothing, CacheItemRemovedReason.DependencyChanged)
              End If
            End If
          End If
        Next
      End If

      'Add a watcher change directory
      Dim onRemoveCallback = New CacheItemRemovedCallback(AddressOf PipelineDirectoryEvent)
      WebCache.Add("PipelineDirectoryEvent", True, New Web.Caching.CacheDependency(Directory), Web.Caching.Cache.NoAbsoluteExpiration, Web.Caching.Cache.NoSlidingExpiration, Web.Caching.CacheItemPriority.Default, onRemoveCallback)
    End Sub
    Private RunInHostingMultiServer As Boolean
    Private Sub WatchPipelineFile(ByVal k As String, ByVal v As Object, ByVal r As CacheItemRemovedReason)
      Static LastActionTime As Date
      Static LastProgressive As UInteger
      If r <> CacheItemRemovedReason.Underused Then
        Dim Rows As String() = ReadAllRows(k)
        If Rows IsNot Nothing Then
          RunInHostingMultiServer = True
          For Each Row As String In Rows
            Dim Values As String() = Split(Row, vbTab)
            Dim Progressive As UInteger = CUInt(Values(0))
            Dim ActionTime As Date = New Date(CLng(Values(1)))
            If Progressive > LastProgressive OrElse ActionTime > LastActionTime Then
              LastActionTime = ActionTime
              LastProgressive = Progressive
              Dim Element As String = Values(2)
              Dim Id As String = Values(3)
              Dim Action As NotifyChangementDelegate
              If ActionCollection.ContainsKey(Element) Then
                Action = ActionCollection(Element)
                Action(Id)
              ElseIf GetType(HttpApplication).FullName = Element Then
                'A new thread of web application is started in another server
                NotifyChangement(GetType(HttpApplication), "ok")
              End If
            End If
          Next
        End If
      End If

      'Add a watcher change directory
      Dim onRemoveCallback = New CacheItemRemovedCallback(AddressOf WatchPipelineFile)
      WebCache.Add(k, True, New Web.Caching.CacheDependency(k), Web.Caching.Cache.NoAbsoluteExpiration, Web.Caching.Cache.NoSlidingExpiration, Web.Caching.CacheItemPriority.Default, onRemoveCallback)
    End Sub

    'Enum Element
    Private ActionCollection As New Collections.Generic.Dictionary(Of String, NotifyChangementDelegate)

    Sub AddActionForNotification(ByVal Element As Type, ByVal Action As NotifyChangementDelegate)
      If Not ActionCollection.ContainsKey(Element.FullName) Then
        ActionCollection.Add(Element.FullName, Action)
      End If
    End Sub

    Public Delegate Sub NotifyChangementDelegate(ByVal Key As String)

    Function Servers() As Collections.Specialized.StringCollection
      Servers = New Collections.Specialized.StringCollection
      If RunInHostingMultiServer Then
        Dim DirectoryInfo As New IO.DirectoryInfo(Directory)
        For Each File As System.IO.FileSystemInfo In DirectoryInfo.GetFileSystemInfos
          Servers.Add(File.Name)
        Next
      End If
    End Function
  End Module
End Namespace
