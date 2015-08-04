'© By Andrea Bruno
'Open source, but: This source code (or part of this code) is not usable in other applications

Imports Microsoft.VisualBasic
Imports System.Xml.Serialization

Namespace WebApplication

	Public Module FileManager
		Private Const Timeout As Integer = 30000 'Milliseconds
		Private Const TryEvery As Integer = 250	 'Milliseconds
		Private Locker As New LockByKeyString

    Function MapPath(Optional ByVal File As String = "") As String
      File = ReplaceBin(File, "/", "\")
      If Extension.Left(File, 1) = "\" Then
        File = Mid(File, 2)
      End If
      Return AppDomain.CurrentDomain.BaseDirectory & File
    End Function

		Public Sub Serialize(ByVal Obj As Object, ByVal NameFile As String)
			Dim KeyLock As String = LCase(NameFile)
      Dim Er As Exception = Nothing
			Try
				SyncLock Locker.LockKey(KeyLock)
          Dim NTry As Integer
          Dim NTryError As Integer = 0
          Do
            NTry = NTryError
            Dim Stream As New System.IO.FileStream(NameFile, System.IO.FileMode.Create)
            Try
              Dim xml As New XmlSerializer(Obj.GetType)
              Dim xmlns As New XmlSerializerNamespaces
              xmlns.Add(String.Empty, String.Empty)
              xml.Serialize(Stream, Obj, xmlns)
            Catch ex As Exception
              NTryError += 1
              System.Threading.Thread.Sleep(TryEvery)
            Finally
              If Stream IsNot Nothing Then
                Stream.Close()
                Stream.Dispose()
              End If
            End Try
          Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try

			If Er IsNot Nothing Then
				Log("SpecificError", 1000, "Serialize", NameFile, Er.Message)
			End If

		End Sub

		Public Function Deserialize(ByVal NameFile As String, Optional ByVal Type As Type = Nothing) As Object
			'Parameter "Type" is need for XML deserialization
      Dim Obj As Object = Nothing
      If System.IO.File.Exists(NameFile) AndAlso New IO.FileInfo(NameFile).Length <> 0 Then
        Dim KeyLock As String = LCase(NameFile)
        Dim Er As Exception
        Try
          SyncLock Locker.LockKey(KeyLock)
            Dim NTry As Integer
            Dim NTryError As Integer = 0
            Do
              NTry = NTryError
              Dim Stream As New System.IO.FileStream(NameFile, System.IO.FileMode.Open, System.IO.FileAccess.Read)
              Try
                Dim XML As New XmlSerializer(Type)
                Obj = XML.Deserialize(Stream)
                Er = Nothing
              Catch ex As Exception
                Er = ex
                If Microsoft.VisualBasic.Err.Number = 5 Then
                  Exit Do
                End If
                NTryError += 1
                System.Threading.Thread.Sleep(TryEvery)
              Finally
                If Stream IsNot Nothing Then
                  Stream.Close()
                  Stream.Dispose()
                End If
              End Try
            Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
          End SyncLock
          Locker.UnlockKey(KeyLock)
        Catch ex As Exception
          Locker.UnlockKey(KeyLock)
          Throw ex
        End Try

        If Er IsNot Nothing Then
          Extension.Log("SpecificError", 1000, "Deserialize", NameFile, Er.Message)
        End If
      End If
      Return Obj
    End Function

		Function ReadXml(ByVal File As String) As System.Xml.XmlDocument
			'Read XML data source from file
			Dim Xml As New System.Xml.XmlDocument
			Xml.Load(File)
			Return Xml
		End Function

		Function ReadAll(ByVal File As String, Optional ByVal GenerateErrorIfNotExists As Boolean = False) As String
			'!!! If need read record from file, see function "RecordsFromTextFile"
      ReadAll = Nothing
      Dim KeyLock As String = LCase(File)
			Try
				SyncLock Locker.LockKey(KeyLock)

					If System.IO.File.Exists(File) Then
            Dim NTry As Integer
            Dim NTryError As Integer = 0
            Do
              NTry = NTryError
              Dim Stream As System.IO.StreamReader = Nothing
              Try
                Stream = New System.IO.StreamReader(File)
                ReadAll = Stream.ReadToEnd()
              Catch ex As Exception
                NTryError += 1
                System.Threading.Thread.Sleep(TryEvery)
              Finally
                If Stream IsNot Nothing Then
                  Stream.Close()
                  Stream.Dispose()
                End If
              End Try
            Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
					Else
						If GenerateErrorIfNotExists Then
              Microsoft.VisualBasic.Err.Raise(70, Nothing, "File not found: " & File)
						End If
					End If
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try
      Return ReadAll
		End Function

		Function ReadAllRows(ByVal File As String, Optional ByVal RemoveEmpty As Boolean = True) As String()
      ReadAllRows = Nothing
      If RemoveEmpty Then
        Dim Data As String = ReadAll(File)
        If Data IsNot Nothing Then
          Return Data.Split(vbCrLf.ToCharArray, StringSplitOptions.RemoveEmptyEntries)
        End If
      Else
        Dim KeyLock As String = LCase(File)
        Try
          SyncLock Locker.LockKey(KeyLock)
            If System.IO.File.Exists(File) Then
              Dim NTry As Integer
              Dim NTryError As Integer = 0
              Do
                NTry = NTryError
                Dim Stream As System.IO.StreamReader = Nothing
                Try
                  ReadAllRows = System.IO.File.ReadAllLines(File)
                Catch ex As Exception
                  NTryError += 1
                  System.Threading.Thread.Sleep(TryEvery)
                Finally
                  If Stream IsNot Nothing Then
                    Stream.Close()
                    Stream.Dispose()
                  End If
                End Try
              Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
            Else
              Microsoft.VisualBasic.Err.Raise(70, Nothing, "File not found: " & File)
            End If
          End SyncLock
          Locker.UnlockKey(KeyLock)
        Catch ex As Exception
          Locker.UnlockKey(KeyLock)
          Throw ex
        End Try
      End If
      Return ReadAllRows
    End Function

		Function ReadAllBinary(ByVal File As String, Optional ByVal GenerateErrorIfNotExists As Boolean = False) As Byte()
      Dim Bytes As Byte() = Nothing
      Dim KeyLock As String = LCase(File)
			Try
				SyncLock Locker.LockKey(KeyLock)
					Dim FileInfo As New System.IO.FileInfo(File)
					If FileInfo.Exists Then
            Dim NTry As Integer
            Dim NTryError As Integer = 0
            Do
              NTry = NTryError
              Dim Stream As System.IO.FileStream = Nothing
              Try
                Stream = New System.IO.FileStream(File, IO.FileMode.Open, IO.FileAccess.Read)
                ReDim Bytes(CInt(Stream.Length) - 1)
                Stream.Read(Bytes, 0, CInt(Stream.Length))
              Catch ex As Exception
                NTryError += 1
                System.Threading.Thread.Sleep(TryEvery)
              Finally
                If Stream IsNot Nothing Then
                  Stream.Close()
                  Stream.Dispose()
                End If
              End Try
            Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
					Else
						If GenerateErrorIfNotExists Then
              Microsoft.VisualBasic.Err.Raise(1)
						End If
					End If
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try
      Return Bytes
    End Function

		Sub WriteAllBinary(ByVal Data As Byte(), ByVal File As String, Optional ByVal Append As Boolean = False)
			Dim KeyLock As String = LCase(File)
			Try
				SyncLock Locker.LockKey(KeyLock)
          Dim NTry As Integer
          Dim NTryError As Integer = 0
          Do
            NTry = NTryError
            Dim Stream As System.IO.StreamWriter = Nothing
            Try
              Stream = New System.IO.StreamWriter(File, Append, System.Text.Encoding.ASCII)
              Dim BinaryWriter As New System.IO.BinaryWriter(Stream.BaseStream)
              BinaryWriter.Write(Data)
              'Stream.Write(Data)
              BinaryWriter.Close()
              BinaryWriter = Nothing
            Catch ex As Exception
              NTryError += 1
              System.Threading.Thread.Sleep(TryEvery)
            Finally
              If Stream IsNot Nothing Then
                Stream.Close()
                Stream.Dispose()
              End If
            End Try
          Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try

		End Sub

		Sub WriteAll(ByVal Data As String, ByVal File As String, Optional ByVal Append As Boolean = False, Optional ByVal BackUp As Boolean = True)
      Dim TmpFile As String = Nothing
      Dim Delete As Boolean = False
			Dim KeyLock As String = LCase(File)
			Try
				SyncLock Locker.LockKey(KeyLock)

					If BackUp AndAlso Append = False AndAlso System.IO.File.Exists(File) Then
						TmpFile = File & ".tmp"
					End If

          Dim NTry As Integer
          Dim NTryError As Integer = 0
					Do
						NTry = NTryError
            Dim Stream As System.IO.StreamWriter = Nothing
						Try
              Dim NotSet As Boolean
              NotSet = TmpFile Is Nothing
              Stream = New System.IO.StreamWriter(IfStr(NotSet, File, TmpFile), Append)
							Stream.Write(Data)
						Catch ex As Exception
							NTryError += 1
							System.Threading.Thread.Sleep(TryEvery)
						Finally
							If Stream IsNot Nothing Then
								Stream.Close()
								Stream.Dispose()
							End If
						End Try
          Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)

					If TmpFile IsNot Nothing Then
						If NTry = NTryError Then
							NTry = 0 : NTryError = 0
							Do
								Try
									System.IO.File.Replace(TmpFile, File, File & ".old")
								Catch ex As Exception
									NTryError += 1
									System.Threading.Thread.Sleep(TryEvery)
								End Try
              Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
						End If
						'Delete outside of SinkLock block
						Delete = True
					End If
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try

			If Delete Then
				FileManager.Delete(TmpFile)
			End If
		End Sub

		Sub WriteToAppend(ByVal Data As String, ByVal File As String)
			WriteAll(Data, File, True)
		End Sub

		Function Delete(ByVal File As String) As Boolean
      Dim Successfull As Boolean = False
			Dim KeyLock As String = LCase(File)
			Try
				SyncLock Locker.LockKey(KeyLock)
					If System.IO.File.Exists(File) Then
            Dim NTry As Integer
            Dim NTryError As Integer = 0
            Do
              NTry = NTryError
              Try
                System.IO.File.Delete(File)
                Successfull = True
              Catch ex As Exception
                NTryError += 1
                System.Threading.Thread.Sleep(TryEvery)
              End Try
            Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
					End If
				End SyncLock
				Locker.UnlockKey(KeyLock)
			Catch ex As Exception
				Locker.UnlockKey(KeyLock)
				Throw ex
			End Try
			Return Successfull
		End Function

    Public Sub DeleteDirectory(ByVal Path As String, Optional ByVal Recursive As Boolean = True)
      'In .NET 2.0 delete directory cause a restart of web application! Please use only spooler to delete directories!

      'Strong control to verify accident delete of important directory!
      If Path.StartsWith(MapPath(ReadWriteDirectory)) Then
        If Len(Path) > Len(ReadWriteDirectory) Then

          Dim KeyLock As String = LCase(Path)
          Try
            SyncLock Locker.LockKey(KeyLock)
              If System.IO.Directory.Exists(Path) Then
                Dim NTry As Integer
                Dim NTryError As Integer = 0
                Do
                  NTry = NTryError
                  Try
                    System.IO.Directory.Delete(Path, Recursive)
                  Catch ex As Exception
                    NTryError += 1
                    System.Threading.Thread.Sleep(TryEvery)
                  End Try
                Loop Until NTry = NTryError OrElse NTryError > (Timeout / TryEvery)
              End If
            End SyncLock
            Locker.UnlockKey(KeyLock)
          Catch ex As Exception
            Locker.UnlockKey(KeyLock)
            Throw ex
          End Try
        End If
      End If
    End Sub

		Public Sub CreateDirectory(ByVal Name As String)
			Name = MapPath(Name)
			If System.IO.Directory.Exists(Name) = False Then
				System.IO.Directory.CreateDirectory(Name)
			End If
		End Sub

    Function GetFileExtension(ByVal File As String) As String
      Dim Point As Integer = InStrRev(File, ".")
      If Point <> 0 Then
        Return LCase(Mid(File, Point + 1))
      End If
      Return Nothing
    End Function

    Function GetFileName(ByVal PathNameFile As String) As String
      Dim Separator As Integer = InStrRev(PathNameFile, "\")
      If Separator <> 0 Then
        Return LCase(Mid(PathNameFile, Separator + 1))
      End If
      Return Nothing
    End Function

	End Module

End Namespace
