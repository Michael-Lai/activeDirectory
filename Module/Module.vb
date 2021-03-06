﻿Imports System.Data.Odbc
Imports System.Data
Imports System.Net
Imports System.IO
Imports System.Convert
Imports System.Runtime.InteropServices
Imports System.Text

Module Module1
    Public LogPath As String

    <System.Runtime.InteropServices.DllImportAttribute("Kernel32.dll", EntryPoint:="GetPrivateProfileStringA")> _
  Public Function GetPrivateProfileString(ByVal lpApplicationName As String, _
   ByVal lpKeyName As String, ByVal IpDefault As String, ByVal IpReturnedString As System.Text.StringBuilder, _
   ByVal nSize As Integer, ByVal IpFileName As String) As Integer
    End Function

    <DllImport("Kernel32.dll")> _
  Private Function SetLocalTime(ByRef time As SYSTEMTIME) As Boolean
    End Function

    Public g_Conn As String = "DATABASE"
    Public Outlet_Conn As String = "OUTLETDB"
    Public Connlocal As OdbcConnection
    Public strOutletDB As String
    Public axCZKEM1 As New zkemkeeper.CZKEM
    Public bIsConnected As Boolean
    Public g_strDbConn As String
    Public g_strOutletConn As String
    Public g_msgString As String
    Public LoginSuccess As Boolean
    Public UserName As String
    Public Conn As New OdbcConnection
    Public clsConn As New clsConnection

    Public g_SVR As String
    Public g_DB As String
    Public g_PORT As String
    Public g_User As String
    Public g_PWD As String
    Public g_Com As String
    Public g_OutletCode As String
    Public g_DBTYPE As String = "MySQL"
    Public OutletSVR As String
    Public OutletDB As String
    Public OutletPORT As String
    Public OutletUSER As String
    Public OutletPWD As String
    Public OutletLOC As String

    Public Outlet_SVR As String
    Public Outlet_DB As String
    Public Outlet_PORT As String
    Public Outlet_USER As String
    Public Outlet_PWD As String

    Private g_strINIFile As String = "system.dll"
    Public g_strPassword As String
    Public g_strUserName As String
    Public g_strEnrollID As String
    Public g_strPrivilege As String

    Public FullAccess As String = "3"
    Public Administrator As String = "2"
    Public Enroller As String = "1"
    Public NormalUser As String = "0"
    Public isUpdate As Boolean

    Private Structure SYSTEMTIME
        Public year As Short
        Public month As Short
        Public dayOfWeek As Short
        Public day As Short
        Public hour As Short
        Public minute As Short
        Public second As Short
        Public milliseconds As Short
    End Structure

    Public Enum RetState
        NoNeedUpdate
        NeedUpdate
        SkipUpdate
        ExitError
    End Enum

    Public Sub HQinit()
        Outlet_SVR = GetINI(g_Conn, "SERVER")
        Outlet_DB = GetINI(g_Conn, "DATABASE")
        Outlet_PORT = CHEXStr(GetINI(g_Conn, "PORT"))
        Outlet_User = CHEXStr(GetINI(g_Conn, "UID"))
        Outlet_PWD = CHEXStr(GetINI(g_Conn, "PWD"))

        'Connection_outlet()
    End Sub

    Public Sub init()
        Try
            'AutoUpdate initialization
            isUpdate = False
            Dim strArgs() As String = Split(Microsoft.VisualBasic.Command(), " ")
            If strArgs.Count > 0 And strArgs(0) <> "" Then
                isUpdate = True
            Else
                isUpdate = CBool(Val(GetINI(g_Conn, "ISUPDATE")))
            End If
            LogPath = Application.StartupPath

            g_SVR = GetINI(g_Conn, "SERVER")
            g_DB = GetINI(g_Conn, "DATABASE")
            g_PORT = CHEXStr(GetINI(g_Conn, "PORT"))
            g_User = CHEXStr(GetINI(g_Conn, "UID"))
            g_PWD = CHEXStr(GetINI(g_Conn, "PWD"))
            g_Com = GetINI(g_Conn, "LOCATION")
            g_OutletCode = GetINI(g_Conn, "OutletCode")


            Connection()
        Catch ex As Exception
            MsgBox("Error while trying to the database server", MsgBoxStyle.Critical, "Offline")
        End Try
    End Sub

    Public Sub Connection()

        g_strDbConn = "Driver={MySQL ODBC 3.51 Driver}" & _
                  ";SERVER=" & g_SVR & _
                  ";DATABASE=" & g_DB & _
                  ";PORT=" & g_PORT & _
                  ";UID=" & g_User & _
                  ";PWD=" & g_PWD & _
                  ";OPTION=" & 1 + 2 + 8 + 32 + 2048 + 16384
    End Sub

    Private Sub CheckSystemSetting()
        If Not File.Exists(Path.Combine(Application.StartupPath, "System.dll")) Then
            MessageBox.Show("Setting file, system.dll not found.", _
                            Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Error)
            Application.Exit()
        End If
    End Sub

     Public Function ConnectDB(ByRef Conn As OdbcConnection, ByVal code As String) As Boolean
        Try
            Select Case code
                Case g_Conn
                    If Connlocal Is Nothing Then Connlocal = New OdbcConnection
                    If Connlocal.State <> ConnectionState.Open Then
                        Connlocal.ConnectionString = g_strDbConn
                        Connlocal.Open()
                    End If
                    Conn = Connlocal
                    ConnectDB = True
            End Select
            Exit Function

        Catch ex As Exception
            g_msgString = "Error while trying to connect to " & code & " " & vbNewLine & _
            "Server, Database Server Offline"
            MsgBox(g_msgString, MsgBoxStyle.Exclamation, "Warning!")
        End Try
    End Function

    Public Sub EncryptPassword(ByVal DGgrid As DataGridView, ByVal lcol As Integer)
        With DGgrid
            For i As Integer = 0 To .RowCount - 1
                Dim p As String = IIf(IsDBNull(.Rows(i).Cells(lcol).Value), "", .Rows(i).Cells(lcol).Value)
                If p <> "" Then
                    Dim q As String = ""
                    For s As Integer = 0 To p.Length - 1
                        q = q & "*"
                    Next
                    .Rows(i).Cells(lcol).Value = q
                    .Rows(i).Cells(lcol).Tag = p
                End If
            Next
        End With
    End Sub

    Public Function CNullStr(ByVal Value As Object) As String
        'Convert NULL to EMPTY STRING
        Return IIf(IsEmptyString(Value), "", Value)
    End Function

    Public Function IsEmptyString(ByVal Value) As Boolean
        'NULL OR EMPTY are considered EmptyString
        If IsDBNull(Value) Then
            Return True
        ElseIf Value Is Nothing Then
            Return True
        ElseIf Value.ToString = "" Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function CHEXNUMBER(ByVal Value As String) As Long
        On Error Resume Next
        CHEXNUMBER = CLng("&H" & Value)
    End Function

    Public Function CHEXStr(ByVal Value As String) As String
        Dim pos As Long
        Dim temp As String
        Dim result As String

        result = ""
        For pos = 1 To Len(Value) \ 2
            temp = Mid(Value, (pos * 2) - 1, 2)
            result = result & Chr(CHEXNUMBER(temp) - pos)
        Next
        CHEXStr = result
    End Function

    Public Function Byte2Str(ByVal gByte() As Byte) As String
        Dim X As Integer
        Dim gTmp As String = ""
        For X = 0 To gByte.Length - 1
            gTmp = gTmp & Chr(gByte(X))
        Next
        Return gTmp
    End Function

    Public Function GetINI(ByVal strSection As String, ByVal strKey As String) As String
        Dim lngRet As Long
        Dim strValue As New System.Text.StringBuilder(256)
        lngRet = GetPrivateProfileString(strSection, strKey, "", strValue, 255, AppDomain.CurrentDomain.BaseDirectory() & g_strINIFile)
        If lngRet <> 0 Then
            GetINI = Left(strValue.ToString, lngRet)
        Else
            GetINI = ""
        End If
    End Function

    Public Function CSQLQuote(ByVal value) As String
        Dim strTemp As String
        strTemp = value
        strTemp = Replace(strTemp, "'", "\'")
        strTemp = Replace(strTemp, """", "\""")
        Return strTemp
    End Function

    Public Function CSQLDate(ByVal dtDate As Date) As String
        Return Format(dtDate, "yyyy-MM-dd")
    End Function

    Public Function CSQLDateTime(ByVal Value) As String
        Return Format(Value, "yyyy-MM-dd HH:mm:ss")
    End Function

    Public Function CSQLTime(ByVal Value) As String
        Return Format(Value, "HH:mm:ss")
    End Function

    Public Function CSQLDateToTimeSpan(ByVal dt As Date) As TimeSpan
        Dim TS As TimeSpan
        TS = New TimeSpan(dt.Hour, dt.Minute, dt.Second)
        Return TS
    End Function

    Public Function CSQLBoolean(ByVal Value As Boolean) As String
        CSQLBoolean = IIf(Value, "1", "0")
    End Function

    Public Function UpdateFiles(ByVal RemoteUri As String) As RetState
        Dim ret As RetState = RetState.NoNeedUpdate
        Dim MyWebClient As New WebClient
        Dim IsFldUpdateEnabled As Boolean = False
        MyWebClient.Credentials = New NetworkCredential("update", "autoupdate")
        Dim RemoteUriTmp As String = ""
        Dim tmp As Long

        Try
            Dim Contents As String = MyWebClient.DownloadString(RemoteUri & "/update.txt")
            ' Process the autoupdate
            ' get rid of the line feeds if exists
            If Contents <> String.Empty Then
                Contents = Replace(Contents, Chr(Keys.LineFeed), "")
                Dim FileList() As String = Split(Contents, Chr(Keys.Return))
                Contents = ""
                ' Remove all comments and blank lines
                For Each F As String In FileList
                    If InStr(F, "'") <> 0 Then F = Strings.Left(F, InStr(F, "'") - 1)
                    If F.Trim <> "" Then
                        If Contents <> "" Then Contents += Chr(Keys.Return)
                        Contents += F.Trim
                    End If
                Next

                ' rebuild the file list
                FileList = Split(Contents, Chr(Keys.Return))
                Dim Info() As String = Split(FileList(0), ";")

                If Split(FileList(0), ";").Length > 2 Then
                    IsFldUpdateEnabled = True
                End If

                Dim isToDelete As Boolean = False
                Dim isToUpgrade As Boolean = False
                Dim isINIUpdate As Boolean = False
                Dim TempFileName As String
                Dim FileName As String


                For Each F As String In FileList
                    RemoteUriTmp = RemoteUri
                    Info = Split(F, ";")

                    isToDelete = False
                    isToUpgrade = False
                    isINIUpdate = False

                    If IsFldUpdateEnabled Then
                        FileName = Application.StartupPath & Info(2).Trim & Info(0).Trim
                        TempFileName = Application.StartupPath & Info(2).Trim & Now.TimeOfDay.TotalMilliseconds
                        RemoteUriTmp &= Replace(Info(2).Trim, "\", "/")
                    Else
                        FileName = String.Format("{0}\{1}", Application.StartupPath, Info(0).Trim)
                        TempFileName = String.Format("{0}\{1}", Application.StartupPath, Now.TimeOfDay.TotalMilliseconds)
                    End If

                    Dim FileExists As Boolean = File.Exists(FileName)
                    If Info.Length = 1 Or Info(1) = "" Then
                        ' Just the file as parameter always upgrade
                        isToUpgrade = True
                        isToDelete = FileExists
                    ElseIf Info(1).Trim = "delete" Then
                        ' second parameter is "delete"
                        isToDelete = FileExists
                    ElseIf Info(1).Trim = "?" Then
                        ' second parameter is "?" (dont upgrade if file exists)
                        isToUpgrade = Not FileExists
                    ElseIf Info(1).Trim = "ini" Then
                        ' second parameter is "ini"
                        isToUpgrade = False
                        isToDelete = False
                        isINIUpdate = True
                    ElseIf FileExists Then
                        ' verify the file version
                        Dim fv As FileVersionInfo = FileVersionInfo.GetVersionInfo(FileName)
                        isToUpgrade = (GetVersion(Info(1).Trim) <> GetVersion(String.Format("{0}.{1}.{2}.{3}", fv.FileMajorPart, fv.FileMinorPart, fv.FileBuildPart, fv.FilePrivatePart)))
                        isToDelete = isToUpgrade
                    Else
                        ' the second parameter exists as version number and the file doesn't exists
                        isToUpgrade = True
                    End If
                    If isToDelete OrElse isToUpgrade Then
                        ret = RetState.NeedUpdate
                    Else
                        If Not isINIUpdate Then
                            tmp = GetRemoteFileSize(RemoteUriTmp & Info(0))
                            If tmp > 0 AndAlso GetFileSize(FileName) <> tmp Then
                                ret = RetState.NeedUpdate
                            ElseIf Date.Compare(GetLastModified(RemoteUriTmp & Info(0)), File.GetLastWriteTime(FileName)) > 0 Then
                                ret = RetState.NeedUpdate
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            MessageBox.Show(String.Format("Error while checking for update.{0}{1}", vbCrLf, ex.Message), Application.ProductName, _
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            ret = RetState.ExitError
        End Try
        Try
            If ret = RetState.NeedUpdate Then
                MyWebClient.DownloadFile(Path.Combine(RemoteUri, "AutoUpdate.exe"), Path.Combine(Application.StartupPath, "AutoUpdate.exe"))
            End If
        Catch ex As Exception
            Application.DoEvents()
            MessageBox.Show(String.Format("Error while retrieving AutoUpdate from update server.{0}{1}", vbCrLf, ex), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
        MyWebClient.Dispose()
        Return ret
    End Function

    Public Function GetVersion(ByVal Version As String) As String
        Dim x() As String = Split(Version, ".")
        Return String.Format("{0:00000}{1:00000}{2:00000}{3:00000}", Int(x(0)), Int(x(1)), Int(x(2)), Int(x(3)))
    End Function

    Public Function GetRemoteFileSize(ByVal RFPath As String) As Long
        Dim reqSize As FtpWebRequest = DirectCast(FtpWebRequest.Create(New Uri(RFPath)), FtpWebRequest)
        reqSize.Credentials = New NetworkCredential("update", "autoupdate")
        reqSize.Method = WebRequestMethods.Ftp.GetFileSize
        reqSize.UseBinary = True
        Dim loginresponse As FtpWebResponse = DirectCast(reqSize.GetResponse(), FtpWebResponse)
        Dim respSize As FtpWebResponse = DirectCast(reqSize.GetResponse(), FtpWebResponse)
        respSize = DirectCast(reqSize.GetResponse(), FtpWebResponse)
        Dim size As Long = respSize.ContentLength
        respSize.Close()
        Return size
    End Function

    Public Function GetLastModified(ByVal RFPath As String) As Date
        Dim reqDate As FtpWebRequest = DirectCast(FtpWebRequest.Create(New Uri(RFPath)), FtpWebRequest)
        reqDate.Credentials = New NetworkCredential("update", "autoupdate")
        reqDate.Method = WebRequestMethods.Ftp.GetDateTimestamp
        reqDate.UseBinary = True
        Dim loginresponse As FtpWebResponse = DirectCast(reqDate.GetResponse(), FtpWebResponse)
        Dim respDate As FtpWebResponse = DirectCast(reqDate.GetResponse(), FtpWebResponse)
        respDate = DirectCast(reqDate.GetResponse(), FtpWebResponse)
        Dim lm As Date = respDate.LastModified
        respDate.Close()
        Return lm
    End Function

    Public Function GetFileSize(ByVal MyFilePath As String) As Long
        Dim MyFile As New FileInfo(MyFilePath)
        Return MyFile.Length
    End Function

    Public Sub SyncTime()
        Dim strSQL As String
        Dim drdate As OdbcDataReader
        Dim conn As New OdbcConnection
        Dim curdate As Date

        Try
            strSQL = "SELECT NOW()"
            If Not ConnectDB(Connlocal, g_Conn) Then Exit Sub

            Dim rCmd As New OdbcCommand(strSQL, Connlocal)
            drdate = rCmd.ExecuteReader()

            If drdate.Read Then
                curdate = CType(drdate.Item("now()"), Date)
            End If

            Dim st As SYSTEMTIME
            st.year = curdate.Year
            st.month = curdate.Month
            st.dayOfWeek = curdate.DayOfWeek
            st.day = curdate.Day
            st.hour = curdate.Hour
            st.minute = curdate.Minute
            st.second = curdate.Second
            st.milliseconds = curdate.Millisecond

            'Set the new time...
            SetLocalTime(st)

            ' conn.Close()
        Catch ex As Exception
            MsgBox(ex.ToString)
            'conn.Close()
        End Try
    End Sub
    Public Function GetServerDateTime(ByRef Conn As OdbcConnection, Optional ByRef Trans As OdbcTransaction = Nothing) As Date
        Dim cmd As OdbcCommand
        Dim dr As OdbcDataReader
        Dim strSQL As String = ""
        Dim svrDate As DateTime = Now

        Try
            Select Case g_DBTYPE
                Case "MYSQL"
                    strSQL = "SELECT NOW()"
                Case "MSSQL"
                    strSQL = "SELECT GETDATE()"
            End Select
            If Trans Is Nothing Then
                cmd = New OdbcCommand(strSQL, Conn)
            Else
                cmd = New OdbcCommand(strSQL, Conn, Trans)
            End If
            dr = cmd.ExecuteReader()
            If dr.HasRows Then
                svrDate = dr(0)
            End If
            dr.Close()
            dr = Nothing
        Catch ex As Exception

        End Try
        Return svrDate
    End Function
    Public Sub ShowError(ByVal strModule As String, ByRef strErrMsg As String)

        WriteLog(strModule, strErrMsg)

        MsgBox("Module : " & strModule & vbCrLf & _
                "Error : " & strErrMsg, , "Error")

    End Sub
    Public Sub ConnectionOutlet()

        strOutletDB = "Driver={MySQL ODBC 3.51 Driver}" & _
                  ";SERVER=" & outletSVR & _
                  ";DATABASE=" & outletDB & _
                  ";PORT=" & outletPORT & _
                  ";UID=" & outletUSER & _
                  ";PWD=" & outletPWD & _
                  ";OPTION=" & 1 + 2 + 8 + 32 + 2048 + 16384
    End Sub


    Public Sub WriteLog(ByVal strModule As String, ByVal strErrMsg As String)
        Try
            Dim sw As New StreamWriter(Path.Combine(Application.StartupPath, "Error.log"), True)
            With sw
                .Write(vbCrLf & "*** " & Now() & " ***" & vbCrLf)
                .Write(vbCrLf & "Error :" & strErrMsg & vbCrLf)
                .Write(vbCrLf & "Module :" & strModule & vbCrLf)
                .Close()
            End With
            sw.Dispose()
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

    End Sub

    'Public Function EncryptPwd()
    '    With dgvAllStaff
    '        For i As Integer = 0 To .RowCount - 1
    '            Dim p As String = .Rows(i).Cells("Pwd").Value
    '            Dim q As String = ""
    '            For s As Integer = 0 To p.Length - 1
    '                q = q & "*"
    '            Next
    '            .Rows(i).Cells("Pwd").Value = q
    '            .Rows(i).Cells("Pwd").Tag = p
    '        Next
    '    End With
    'End Function
End Module
