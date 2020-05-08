Imports System.Net
Imports System.IO
Imports System.Threading
Imports System.Text.Encoding

Public Class 类_数据库下载器
    Implements IDisposable

#Region "定义和声明"

    Const 千字节 As Integer = 1024

    Friend Enum 常量集合_结果 As Byte
        无 = 0
        成功 = 1
        失败 = 2
        失败_重试下载 = 3
    End Enum

    Dim 下载地址 As String
    Dim 总字节数, 已下载字节数 As Integer
    Friend 正在下载 As Boolean
    Dim 线程 As Thread
    Dim HTTP网络请求 As HttpWebRequest
    Dim 内存流 As MemoryStream

    Friend Event 下载结束(ByVal sender As Object, ByVal 结果 As 常量集合_结果, ByVal 下载的数据() As Byte)

#End Region

    Friend Sub 下载(ByVal 下载地址1 As String)
        下载地址 = 下载地址1
        Call 清零()
        正在下载 = True
        线程 = New Thread(New ThreadStart(AddressOf 开始下载))
        线程.Start()
    End Sub

    Private Sub 开始下载()
        Try
            Dim HTTP网络回应 As HttpWebResponse = Nothing
            Dim 错误信息 As String = ""
            Try
                Dim 流 As Stream
HTTP从头开始:
                HTTP网络请求 = WebRequest.Create(下载地址)
                HTTP网络请求.Timeout = 10000
                HTTP网络回应 = HTTP网络请求.GetResponse
                Select Case HTTP网络回应.StatusCode
                    Case HttpStatusCode.OK
                        总字节数 = HTTP网络回应.ContentLength
                        If 总字节数 > 0 Then
                            If 正在下载 = False Then Exit Try
                            流 = HTTP网络回应.GetResponseStream
                        Else
                            Exit Try
                        End If
                    Case Else
                        Exit Try
                End Select
                If 内存流 Is Nothing Then 内存流 = New MemoryStream(总字节数)
                Dim 字节数组(千字节 * 4 - 1) As Byte
                Dim 读取的字节数 As Integer
                Do
                    读取的字节数 = 流.Read(字节数组, 0, 字节数组.Length)
                    If 读取的字节数 > 0 AndAlso 正在下载 = True Then
                        内存流.Write(字节数组, 0, 读取的字节数)
                        已下载字节数 += 读取的字节数
                    End If
                Loop Until 读取的字节数 = 0 OrElse 已下载字节数 >= 总字节数 OrElse 正在下载 = False
            Catch ex As Exception
                错误信息 = ex.Message
            End Try
            正在下载 = False
            Try
                If HTTP网络回应 IsNot Nothing Then HTTP网络回应.Close()
            Catch ex As Exception
            End Try
            If 总字节数 > 0 Then
                If 已下载字节数 < 总字节数 Then
                    Call 下载停止(常量集合_结果.失败_重试下载, Unicode.GetBytes(错误信息))
                Else
                    Call 下载停止(常量集合_结果.成功, 内存流.ToArray)
                End If
            ElseIf String.IsNullOrEmpty(错误信息) = False Then
                Call 下载停止(常量集合_结果.失败_重试下载, Unicode.GetBytes(错误信息))
            Else
                Call 下载停止(常量集合_结果.失败, Unicode.GetBytes("数据长度为零"))
            End If
        Catch ex As Exception
            正在下载 = False
        End Try
    End Sub

    Private Sub 清零()
        总字节数 = 0
        已下载字节数 = 0
        Call 关闭内存流()
    End Sub

    Private Sub 关闭内存流()
        If 内存流 IsNot Nothing Then
            内存流.Close()
            内存流.Dispose()
            内存流 = Nothing
        End If
    End Sub

    Friend Sub 暂停()
        正在下载 = False
        Try
            If HTTP网络请求 IsNot Nothing Then
                HTTP网络请求.Abort()
                HTTP网络请求 = Nothing
            End If
        Catch ex As Exception
        End Try
        Call 关闭内存流()
        If 线程 IsNot Nothing Then
            线程.Abort()
            线程 = Nothing
        End If
    End Sub

    Private Sub 下载停止(ByVal 结果 As 常量集合_结果, Optional ByVal 下载的数据() As Byte = Nothing)
        RaiseEvent 下载结束(Me, 结果, 下载的数据)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Call 暂停()
            End If

            ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
            ' TODO:  将大型字段设置为 null。
        End If
        Me.disposedValue = True
    End Sub

    ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
    'Protected Overrides Sub Finalize()
    '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic 添加此代码是为了正确实现可处置模式。
    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
