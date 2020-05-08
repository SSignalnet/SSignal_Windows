Imports System.IO
Imports System.Text.Encoding
Imports System.Threading
Imports System.Net
Imports NAudio.Wave
Imports AMRConvertor

Public Class 类_音频_播放

    Private Structure 路径_复合数据
        Dim 下载路径, 保存路径 As String
    End Structure

    Private WithEvents Wave输出 As WaveOutEvent
    Dim 文件路径2 As String
    Friend 原始路径 As String
    Dim AMR转换器 As AMRConverting
    Dim 字节保存器 As BinaryWriter
    Dim Wave流 As WaveStream
    Dim 线程 As Thread
    Dim 正在下载, 不通知2, 播放完关闭 As Boolean

    Public Event 播放完毕(ByVal sender As Object)

    Public ReadOnly Property 正在播放 As Boolean
        Get
            If 线程 IsNot Nothing Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public Function 开始播放AMR(ByVal 路径 As String, Optional ByVal 是原始路径 As Boolean = True) As Boolean
        Dim 文件路径 As String = 路径
        Dim I As Integer = 文件路径.LastIndexOf(".")
        If I < 0 Then Return False
        If String.Compare(文件路径.Substring(I + 1), "amr", True) <> 0 Then Return False
        If 文件路径.StartsWith("https://") OrElse 文件路径.StartsWith("http://") Then
            I = 文件路径.LastIndexOf("/")
            If I < 0 Then Return False
            Dim 段() As String = 文件路径.Substring(I + 1).Split(New String() {"&"}, StringSplitOptions.RemoveEmptyEntries)
            If 段.Length <= 0 Then Return False
            Const 参数名 As String = "FileName="
            For I = 段.Length - 1 To 0 Step -1
                If 段(I).StartsWith(参数名) Then Exit For
            Next
            If I < 0 Then Return False
            Dim 目录路径 As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址
            If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
            Dim 文件路径3 As String = 目录路径 & "\" & 段(I).Substring(参数名.Length)
            If File.Exists(文件路径3) = False Then
                Dim 下载和保存路径 As 路径_复合数据
                下载和保存路径.下载路径 = 文件路径
                下载和保存路径.保存路径 = 文件路径3
                正在下载 = True
                线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
                线程.Start(下载和保存路径)
                Return True
            Else
                文件路径 = 文件路径3
            End If
        End If
        If String.Compare(文件路径, 文件路径2) = 0 Then
            Wave流.Seek(0, SeekOrigin.Begin)
            GoTo 跳转点1
        End If
        Dim 内存流 As MemoryStream
        Try
            Dim 字节数组() As Byte = File.ReadAllBytes(文件路径)
            If 字节数组 Is Nothing Then Return False
            内存流 = 音频文件解码(字节数组)
            If 内存流 Is Nothing Then Return False
        Catch ex As Exception
            Return False
        End Try
        Wave流 = New WaveFileReader(内存流)
        Wave输出 = New WaveOutEvent
        Wave输出.Init(Wave流)
        文件路径2 = 文件路径
        If 是原始路径 = True Then 原始路径 = 路径
跳转点1:
        线程 = New Thread(New ThreadStart(AddressOf 播放))
        线程.Start()
        Return True
    End Function

    Private Sub 下载(ByVal 参数 As Object)
        Dim 路径 As 路径_复合数据 = 参数
        Dim 收到的字节数组() As Byte
        Dim 收到的总字节数 As Integer
        Dim 重试次数 As Integer
重试:
        收到的字节数组 = Nothing
        收到的总字节数 = 0
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(路径.下载路径)
            HTTP网络请求.Method = "GET"
            HTTP网络请求.Timeout = 10000
            Dim 字节数组(8191) As Byte
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    ReDim 收到的字节数组(HTTP网络回应.ContentLength - 1)
                    Dim 收到的字节数 As Integer
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
                    Do
                        收到的字节数 = 输入流.Read(字节数组, 0, 字节数组.Length)
                        If 收到的字节数 > 0 Then
                            Array.Copy(字节数组, 0, 收到的字节数组, 收到的总字节数, 收到的字节数)
                            收到的总字节数 += 收到的字节数
                        Else
                            Exit Do
                        End If
                    Loop
                End If
            End Using
        Catch ex As Exception
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 重试
            Else
                Return
            End If
        Finally
            正在下载 = False
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Try
                    File.WriteAllBytes(路径.保存路径, 收到的字节数组)
                Catch ex As Exception
                    Return
                End Try
                开始播放AMR(路径.保存路径, False)
            End If
        End If
    End Sub

    Friend Sub 开始播放本地MP3(ByVal 路径 As String)
        播放完关闭 = True
        Wave流 = New Mp3FileReader(路径)
        Wave输出 = New WaveOutEvent
        Wave输出.Init(Wave流)
        线程 = New Thread(New ThreadStart(AddressOf 播放))
        线程.Start()
    End Sub

    Private Sub 播放()
        Try
            Wave输出.Play()
            While Wave输出.PlaybackState = PlaybackState.Playing
                Thread.Sleep(100)
            End While
        Catch ex As Exception
        End Try
    End Sub

    Private Function 音频文件解码(ByVal 音频数据() As Byte) As MemoryStream
        Dim AMR数据头() As Byte = ASCII.GetBytes("#!AMR" & ChrW(10))
        If 音频数据.Length > AMR数据头.Length Then
            Dim I As Short
            For I = 0 To AMR数据头.Length - 1
                If 音频数据(I) <> AMR数据头(I) Then Return Nothing
            Next
            Dim AMR主数据(音频数据.Length - AMR数据头.Length - 1) As Byte
            Array.Copy(音频数据, AMR数据头.Length, AMR主数据, 0, AMR主数据.Length)
            Dim Wave主数据() As Byte
            Try
                If AMR转换器 Is Nothing Then AMR转换器 = New AMRConverting
                Wave主数据 = AMR转换器.DecodeAMRToWAVE(AMR主数据)
            Catch ex As Exception
                Return Nothing
            End Try
            If Wave主数据 IsNot Nothing Then
                Dim 内存流 As MemoryStream = Nothing
                Call 创建WAVE存储流(内存流, 字节保存器, 1, 8000, 8000 * 2, 16)
                字节保存器.Write(Wave主数据, 0, Wave主数据.Length)
                Call 设置WAVE长度值(字节保存器, Wave主数据.Length)
                内存流.Seek(0, SeekOrigin.Begin)
                Return 内存流
            End If
        End If
        Return Nothing
    End Function

    Private Sub 创建WAVE存储流(ByRef 内存流 As MemoryStream, ByRef 字节保存器 As BinaryWriter, ByVal 声道数 As Short,
                              ByVal 采样率 As Integer, ByVal 每秒字节数 As Integer, ByVal 样本位数 As Short)
        If 字节保存器 IsNot Nothing Then 字节保存器.Close()
        内存流 = New MemoryStream
        字节保存器 = New BinaryWriter(内存流)
        Dim 文件标识 As Char() = {"R", "I", "F", "F"}
        Dim 文件类型 As Char() = {"W", "A", "V", "E"}
        Dim 文件格式 As Char() = {"f", "m", "t", " "}
        Dim 文件数据 As Char() = {"d", "a", "t", "a"}
        Dim shPad As Short = 1
        Dim 格式区长度 As Integer = 16
        Dim 文件长度 As Integer = 0
        Dim 数据长度1 As Integer = 0
        字节保存器.Write(文件标识)
        字节保存器.Write(文件长度)
        字节保存器.Write(文件类型)
        字节保存器.Write(文件格式)
        字节保存器.Write(格式区长度)
        字节保存器.Write(shPad)
        字节保存器.Write(声道数)
        字节保存器.Write(采样率)
        字节保存器.Write(每秒字节数)
        字节保存器.Write(CShort(样本位数 / 8))
        字节保存器.Write(样本位数)
        字节保存器.Write(文件数据)
        字节保存器.Write(数据长度1)
    End Sub

    Private Sub 设置WAVE长度值(ByRef 字节保存器 As BinaryWriter, ByVal 数据长度 As Integer)
        字节保存器.Seek(4, SeekOrigin.Begin)
        字节保存器.Write(CInt(数据长度 + 36))
        字节保存器.Seek(40, SeekOrigin.Begin)
        字节保存器.Write(数据长度)
    End Sub

    Public Sub 停止播放(Optional ByVal 不通知 As Boolean = False)
        If 正在下载 = True Then
            If 线程 IsNot Nothing Then
                Try
                    线程.Abort()
                Catch ex As Exception
                End Try
                线程 = Nothing
            End If
            正在下载 = False
        End If
        If Wave输出 IsNot Nothing Then
            If Wave输出.PlaybackState <> PlaybackState.Stopped Then
                不通知2 = 不通知
                Wave输出.Stop()
            End If
        End If
    End Sub

    Private Sub Wave输出_PlaybackStopped(sender As Object, e As StoppedEventArgs) Handles Wave输出.PlaybackStopped
        If 播放完关闭 = False Then
            线程 = Nothing
            If 不通知2 = False Then
                RaiseEvent 播放完毕(Me)
            Else
                不通知2 = False
            End If
        Else
            关闭()
        End If
    End Sub

    Public Sub 关闭()
        If 正在下载 = True Then
            If 线程 IsNot Nothing Then
                Try
                    线程.Abort()
                Catch ex As Exception
                End Try
                线程 = Nothing
            End If
            正在下载 = False
        End If
        If Wave输出 IsNot Nothing Then
            Wave输出.Dispose()
            Wave输出 = Nothing
        End If
        If Wave流 IsNot Nothing Then
            Wave流.Close()
            Wave流 = Nothing
        End If
        If 字节保存器 IsNot Nothing Then
            字节保存器.Close()
            字节保存器 = Nothing
        End If
        文件路径2 = Nothing
    End Sub

End Class
