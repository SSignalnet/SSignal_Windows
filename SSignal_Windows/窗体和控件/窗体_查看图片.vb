Imports System.IO
Imports System.Threading
Imports System.Net

Public Class 窗体_查看图片

    Private Structure 路径_复合数据
        Dim 下载路径, 保存路径 As String
    End Structure

    Dim 图片 As Image
    Dim 绘图区域 As RectangleF
    Dim 画笔 As SolidBrush
    Dim 格式 As StringFormat
    Dim 线程 As Thread
    Dim 下载进度 As Integer = -1
    Dim 是网络文件 As Boolean
    Dim 文件名 As String

    Private Delegate Sub 显示图片_跨线程(ByVal 文件路径 As String)
    Private Delegate Sub 更新下载进度_跨线程(ByVal 进度 As Integer)

    Public Sub New()
        InitializeComponent()
        Me.Opacity = 0
    End Sub

    Private Sub 窗体_查看图片_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Icon = My.Resources.icon
        画笔 = New SolidBrush(Color.White)
        格式 = StringFormat.GenericTypographic
        格式.Alignment = StringAlignment.Center
        格式.LineAlignment = StringAlignment.Center
        定时器_减少透明度.Start()
    End Sub

    Private Sub 定时器_减少透明度_Tick(sender As Object, e As EventArgs) Handles 定时器_减少透明度.Tick
        If Me.Opacity < 1 Then
            Me.Opacity += 0.1
        Else
            定时器_减少透明度.Stop()
        End If
    End Sub

    Friend Sub 显示图片(ByVal 文件路径 As String)
        If InvokeRequired Then
            Dim d As New 显示图片_跨线程(AddressOf 显示图片)
            Invoke(d, New Object() {文件路径})
        Else
            If 文件路径.StartsWith("https://") OrElse 文件路径.StartsWith("http://") Then
                是网络文件 = True
                Dim I As Integer = 文件路径.LastIndexOf("/")
                If I < 0 Then Return
                Dim 段() As String = 文件路径.Substring(I + 1).Split(New String() {"&"}, StringSplitOptions.RemoveEmptyEntries)
                If 段.Length <= 0 Then Return
                Const 参数名 As String = "FileName="
                For I = 段.Length - 1 To 0 Step -1
                    If 段(I).StartsWith(参数名) Then Exit For
                Next
                If I < 0 Then Return
                Dim 目录路径 As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址
                If Directory.Exists(目录路径) = False Then Directory.CreateDirectory(目录路径)
                文件名 = 段(I).Substring(参数名.Length)
                Dim 保存路径 As String = 目录路径 & "\" & 文件名
                If File.Exists(保存路径) = False Then
                    下载进度 = 0
                    Me.Invalidate()
                    Dim 下载和保存路径 As 路径_复合数据
                    下载和保存路径.下载路径 = 文件路径
                    下载和保存路径.保存路径 = 保存路径
                    线程 = New Thread(New ParameterizedThreadStart(AddressOf 下载))
                    线程.Start(下载和保存路径)
                Else
                    图片 = Image.FromFile(保存路径)
                    调整绘图区域()
                    Me.Invalidate()
                End If
            ElseIf File.Exists(文件路径) = True Then
                图片 = Image.FromFile(文件路径)
                调整绘图区域()
                Me.Invalidate()
            End If
        End If
    End Sub

    Private Sub 下载(ByVal 参数 As Object)
        Dim 路径 As 路径_复合数据 = 参数
        Dim 进度 As Integer
        Dim 重试次数 As Integer
        Dim 收到的字节数组() As Byte
        Dim 收到的总字节数, 总字节数 As Long
重试:
        收到的字节数组 = Nothing
        收到的总字节数 = 0
        下载进度 = 0
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(路径.下载路径)
            HTTP网络请求.Method = "GET"
            HTTP网络请求.Timeout = 10000
            Dim 字节数组(8191) As Byte
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    总字节数 = HTTP网络回应.ContentLength
                    ReDim 收到的字节数组(总字节数 - 1)
                    Dim 收到的字节数 As Integer
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
                    Do
                        收到的字节数 = 输入流.Read(字节数组, 0, 字节数组.Length)
                        If 收到的字节数 > 0 Then
                            Array.Copy(字节数组, 0, 收到的字节数组, 收到的总字节数, 收到的字节数)
                            收到的总字节数 += 收到的字节数
                            进度 = Int((收到的总字节数 / 总字节数) * 100)
                            If 进度 - 下载进度 >= 5 Then
                                更新下载进度(进度)
                            End If
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
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Try
                    File.WriteAllBytes(路径.保存路径, 收到的字节数组)
                Catch ex As Exception
                    Return
                End Try
                显示图片(路径.保存路径)
            End If
        End If
    End Sub

    Private Sub 更新下载进度(ByVal 进度 As Integer)
        If InvokeRequired Then
            Dim d As New 更新下载进度_跨线程(AddressOf 更新下载进度)
            Invoke(d, New Object() {进度})
        Else
            下载进度 = 进度
            Me.Invalidate()
        End If
    End Sub

    Private Sub 窗体_查看图片_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        If 图片 IsNot Nothing Then
            e.Graphics.DrawImage(图片, 绘图区域)
        ElseIf 下载进度 >= 0 Then
            e.Graphics.DrawString(下载进度 & " %", Me.Font, 画笔, 绘图区域, 格式)
        End If
    End Sub

    Private Sub 窗体_查看图片_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If 图片 IsNot Nothing Then
            调整绘图区域()
        Else
            绘图区域 = New RectangleF(0, 0, Me.Width, Me.Height)
        End If
    End Sub

    Private Sub 调整绘图区域()
        If 图片.Width > Me.Width Then
            If 图片.Height > Me.Height Then
                If 图片.Width / 图片.Height > Me.Width / Me.Height Then
                    GoTo 跳转点1
                Else
                    GoTo 跳转点2
                End If
            Else
跳转点1:
                Dim 缩小后高度 As Integer = 图片.Height * (Me.Width / 图片.Width)
                绘图区域 = New RectangleF(0, (Me.Height - 缩小后高度) / 2, Me.Width, 缩小后高度)
            End If
        ElseIf 图片.Height > Me.Height Then
跳转点2:
            Dim 缩小后宽度 As Integer = 图片.Width * (Me.Height / 图片.Height)
            绘图区域 = New RectangleF((Me.Width - 缩小后宽度) / 2, 0, 缩小后宽度, Me.Height)
        Else
            绘图区域 = New RectangleF((Me.Width - 图片.Width) / 2, (Me.Height - 图片.Height) / 2, 图片.Width, 图片.Height)
        End If
        Me.Invalidate()
    End Sub

    Private Sub 窗体_查看图片_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Escape : Me.Close()
        End Select
    End Sub

    Private Sub 窗体_查看图片_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If 线程 IsNot Nothing Then
            Try
                线程.Abort()
            Catch ex As Exception
            End Try
            线程 = Nothing
        End If
        If 图片 IsNot Nothing Then
            图片.Dispose()
            图片 = Nothing
        End If
    End Sub

    Private Sub 窗体_查看图片_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If e.Button = MouseButtons.Right Then
            If 是网络文件 = False Then Return
            Dim 路径 As String = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址 & "\" & 文件名
            If File.Exists(路径) Then
                主窗体.文件保存对话框.FileName = 文件名
                If 主窗体.文件保存对话框.ShowDialog(Me) = DialogResult.OK Then
                    Try
                        File.Copy(路径, 主窗体.文件保存对话框.FileName)
                    Catch ex As Exception
                        MsgBox(ex.Message)
                    End Try
                End If
            End If
        End If
    End Sub

    Private Sub 窗体_查看图片_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        If e.Button = MouseButtons.Left Then
            If 图片 IsNot Nothing Then Me.Close()
        End If
    End Sub

    Private Sub 窗体_查看图片_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Me.MouseDoubleClick
        If e.Button = MouseButtons.Left Then
            Me.Close()
        End If
    End Sub

End Class
