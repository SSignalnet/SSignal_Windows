Imports System.Net
Imports System.Threading
Imports System.IO
Imports SSignalDB

Friend Class 类_下载媒体文件
    Implements IDisposable

#Region "定义和声明"

    Friend Enum 常量集合_结果 As Byte
        无 = 0
        失败_未找到文件 = 1
        失败_未成功连接网站 = 2
        失败_下载中断 = 3
        失败_字节数不一致 = 4
        成功 = 5
    End Enum

    Dim 正在下载的项目 As 类_流星语或商品
    Friend 下载地址 As String
    Dim 保存路径 As String
    Dim 总字节数, 已下载字节数 As Integer
    Friend 正在下载 As Boolean
    Shared 线程 As Thread
    Dim HTTP网络请求 As HttpWebRequest
    Dim 内存流 As MemoryStream
    Dim 文件修改时间 As Date

    Friend Event 下载结束(ByVal sender As Object, ByVal 结果 As 常量集合_结果, ByVal 正在下载的项目 As 类_流星语或商品)

#End Region

    Friend Sub 下载(ByVal 正在下载的项目1 As 类_流星语或商品, ByVal 下载地址1 As String, ByVal 保存路径1 As String)
        If 线程 Is Nothing Then
            正在下载的项目 = 正在下载的项目1
            If String.Compare(下载地址, 下载地址1) <> 0 Then
                下载地址 = 下载地址1
                Call 清零()
            End If
            保存路径 = 保存路径1
            正在下载 = True
            If 线程 Is Nothing Then
                线程 = New Thread(New ThreadStart(AddressOf 开始下载))
                线程.Start()
            End If
        End If
    End Sub

    Private Sub 开始下载()
        Try
            Dim 文件未找到, 文件未改变 As Boolean
            Dim HTTP网络回应 As HttpWebResponse = Nothing
            Dim 下载开始时间 As Date = Date.Now
            Try
                If 总字节数 > 0 AndAlso 已下载字节数 > 0 Then
                    If 内存流 IsNot Nothing Then
                        If 内存流.Length >= 总字节数 OrElse 已下载字节数 <> 内存流.Length Then
                            Call 清零()
                        End If
                    Else
                        Call 清零()
                    End If
                Else
                    Call 清零()
                End If
                Dim 文件修改时间_新 As Date
                Dim 流 As Stream
HTTP从头开始:
                HTTP网络请求 = WebRequest.Create(下载地址)
                HTTP网络请求.Timeout = 10000
                If 总字节数 > 0 AndAlso 已下载字节数 > 0 Then
                    HTTP网络请求.AddRange(已下载字节数, 总字节数 - 1)
                    HTTP网络回应 = HTTP网络请求.GetResponse
                    Select Case HTTP网络回应.StatusCode
                        Case HttpStatusCode.NotFound
                            文件未找到 = True
                            Exit Try
                        Case HttpStatusCode.OK, HttpStatusCode.RequestedRangeNotSatisfiable  '服务器不支持断点续传
                            HTTP网络回应.Close()
                            Call 清零()
                            GoTo HTTP从头开始
                        Case HttpStatusCode.PartialContent
                            文件修改时间_新 = HTTP网络回应.LastModified
                            If 已下载字节数 + HTTP网络回应.ContentLength = 总字节数 Then
                                If 内存流 IsNot Nothing Then
                                    If 文件修改时间.Ticks <> 文件修改时间_新.Ticks Then
                                        GoTo 跳转点1
                                    End If
                                Else
                                    GoTo 跳转点1
                                End If
                            Else
跳转点1:
                                HTTP网络回应.Close()
                                Call 清零()
                                GoTo HTTP从头开始
                            End If
                            文件修改时间 = 文件修改时间_新
                            If 正在下载 = False Then Exit Try
                            流 = HTTP网络回应.GetResponseStream
                        Case Else
                            Exit Try
                    End Select
                Else
                    HTTP网络回应 = HTTP网络请求.GetResponse
                    Select Case HTTP网络回应.StatusCode
                        Case HttpStatusCode.NotFound
                            文件未找到 = True
                            Exit Try
                        Case HttpStatusCode.OK
                            总字节数 = HTTP网络回应.ContentLength
                            文件修改时间_新 = HTTP网络回应.LastModified
                            文件修改时间 = 文件修改时间_新
                            If 正在下载 = False Then Exit Try
                            流 = HTTP网络回应.GetResponseStream
                        Case Else
                            Exit Try
                    End Select
                End If
                If 内存流 Is Nothing Then 内存流 = New MemoryStream
                Dim 字节数组(数据库千字节 * 4 - 1) As Byte
                Dim 读取的字节数 As Integer
                Do
                    读取的字节数 = 流.Read(字节数组, 0, 字节数组.Length)
                    If 读取的字节数 > 0 AndAlso 正在下载 = True Then
                        内存流.Write(字节数组, 0, 读取的字节数)
                        已下载字节数 += 读取的字节数
                    End If
                Loop Until 读取的字节数 = 0 OrElse 已下载字节数 >= 总字节数 OrElse 正在下载 = False
            Catch ex As WebException
                If ex.Message.IndexOf("(404)") > 0 Then 文件未找到 = True
            Catch ex As Exception
            End Try
            正在下载 = False
            Try
                If HTTP网络回应 IsNot Nothing Then HTTP网络回应.Close()
            Catch ex As Exception
            End Try
            线程 = Nothing
            If 文件未找到 = False Then
                If 文件未改变 = False Then
                    If 已下载字节数 < 总字节数 Then
                        Call 下载停止(常量集合_结果.失败_下载中断)
                    ElseIf 总字节数 > 0 Then
                        If 已下载字节数 = 总字节数 Then
                            Try
                                Dim 文件信息 As New FileInfo(保存路径)
                                If 文件信息.Exists = True Then
                                    文件信息.Delete()
                                Else
                                    Dim 目录 As String = Path.GetDirectoryName(保存路径)
                                    If Directory.Exists(目录) = False Then Directory.CreateDirectory(目录)
                                End If
                                File.WriteAllBytes(保存路径, 内存流.ToArray)
                                文件信息 = New FileInfo(保存路径)
                                If 文件信息.Exists Then 文件信息.LastWriteTime = 文件修改时间
                            Catch ex As Exception
                                Call 下载停止(常量集合_结果.失败_下载中断)
                                Return
                            End Try
                            Dim 秒 As Long = DateDiff(DateInterval.Second, 下载开始时间, Date.Now)
                            If 秒 < 60 Then
                                Call 下载停止(常量集合_结果.成功)
                            Else
                                Dim 分钟 As Integer = Int(秒 / 60)
                                Call 下载停止(常量集合_结果.成功)
                            End If
                        Else
                            Call 下载停止(常量集合_结果.失败_字节数不一致)
                        End If
                    Else
                        Call 下载停止(常量集合_结果.失败_未成功连接网站)
                    End If
                Else
                    Call 下载停止(常量集合_结果.成功)
                End If
            Else
                Call 下载停止(常量集合_结果.失败_未找到文件)
            End If
        Catch ex As Exception
            正在下载 = False
            线程 = Nothing
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

    Friend Sub 暂停(Optional ByVal 关闭内存流1 As Boolean = False)
        正在下载 = False
        Try
            If HTTP网络请求 IsNot Nothing Then
                HTTP网络请求.Abort()
                HTTP网络请求 = Nothing
            End If
        Catch ex As Exception
        End Try
        If 关闭内存流1 = True Then Call 关闭内存流()
        If 线程 IsNot Nothing Then
            线程.Abort()
            线程 = Nothing
        End If
    End Sub

    Private Sub 下载停止(ByVal 结果 As 常量集合_结果)
        RaiseEvent 下载结束(Me, 结果, 正在下载的项目)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Call 暂停(True)
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
