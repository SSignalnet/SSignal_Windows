Imports System.Text.Encoding
Imports System.IO
Imports System.Threading
Imports System.Net
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Friend Class 类_备份器

    Const 分钟 As Integer = 5

    Dim 备份管理器 As 类_备份管理器
    Friend 服务器子域名, 状态信息 As String
    Dim 连接凭据, 备份凭据 As String
    Dim 验证数字, 备份数据文件长度 As Long
    Dim 更新的页数 As Integer
    WithEvents 数据库下载器 As 类_数据库下载器
    WithEvents 定时器 As System.Timers.Timer
    Dim 下载失败次数 As Short
    Dim 文件流_备份数据库 As FileStream
    Dim 备份数据库 As 类_数据库
    Dim 备份中心服务器, 有新凭据 As Boolean
    Friend 停止备份 As Boolean
    Dim 线程_HTTPS访问 As Thread
    Dim 未下载的流星语(), 正在下载的流星语() As 类_流星语或商品
    Dim 未下载的商品(), 正在下载的商品() As 类_流星语或商品
    Dim 未下载流星语数, 未下载商品数, 索引_正在下载的流星语, 索引_正在下载的商品 As Integer

    WithEvents 媒体下载器 As 类_下载媒体文件

    Friend Event 显示异常信息(ByVal 来自 As Object, ByVal 文本 As String)

    Friend Sub New(ByVal 备份管理器1 As 类_备份管理器, ByVal 服务器子域名1 As String, Optional ByVal 备份中心服务器1 As Boolean = False)
        备份管理器 = 备份管理器1
        服务器子域名 = 服务器子域名1
        备份中心服务器 = 备份中心服务器1
        定时器 = New System.Timers.Timer()
    End Sub

    Friend Function 开始() As Boolean
        If 数据库_获取备份凭据() = False Then Return False
        If String.IsNullOrEmpty(备份凭据) Then
            If 数据库_添加备份凭据() = False Then Return False
        Else
            Dim 文件信息 As New FileInfo(获取文件路径)
            If 文件信息.Exists = False Then
                If 验证数字 > 0 Then
                    记录异常信息(界面文字.获取(211, "备份的数据库已被移走（#%） 。", New Object() {文件信息.FullName}))
                    Return False
                End If
            Else
                备份数据文件长度 = 文件信息.Length
            End If
        End If
        下载失败次数 = 0
        有新凭据 = False
        停止备份 = False
        记录异常信息(界面文字.获取(212, "正在下载数据库文件。"), False)
        Call 下载数据库文件()
        Return True
    End Function

    Private Sub 下载数据库文件()
        If 备份中心服务器 Then
            If 数据库下载器 Is Nothing Then 数据库下载器 = New 类_数据库下载器
            数据库下载器.下载("https://" & 获取服务器域名(服务器子域名) & "/backup/?UserID=" & 当前用户.编号 & "&Credential=" & 当前用户.凭据_中心服务器 & "&BackupCredential=" & 备份凭据 & "&VerificationNumber=" & 验证数字)
        Else
            If String.IsNullOrEmpty(连接凭据) Then
                线程_HTTPS访问 = New Thread(New ThreadStart(AddressOf HTTPS访问))
                线程_HTTPS访问.Start()
            Else
                If 数据库下载器 Is Nothing Then 数据库下载器 = New 类_数据库下载器
                数据库下载器.下载("https://" & 获取服务器域名(服务器子域名) & "/backup/?Credential=" & 连接凭据 & "&BackupCredential=" & 备份凭据 & "&VerificationNumber=" & 验证数字)
            End If
        End If
    End Sub

    Private Sub HTTPS访问()
        Dim 收到的字节数组() As Byte
        Dim 重试次数 As Integer
        Dim 收到的字节数, 收到的总字节数 As Integer
重试:
        收到的总字节数 = 0
        收到的字节数组 = Nothing
        Try
            Dim HTTP网络请求 As HttpWebRequest = WebRequest.Create(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AdminCredential&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&Domain=" & 获取服务器域名(服务器子域名))
            HTTP网络请求.Method = "POST"
            HTTP网络请求.Timeout = 20000
            HTTP网络请求.ContentType = "text/xml"
            HTTP网络请求.ContentLength = 0
            Using HTTP网络回应 As HttpWebResponse = HTTP网络请求.GetResponse
                If HTTP网络回应.ContentLength > 0 Then
                    ReDim 收到的字节数组(HTTP网络回应.ContentLength - 1)
                    Dim 输入流 As Stream = HTTP网络回应.GetResponseStream
继续:
                    收到的字节数 = 输入流.Read(收到的字节数组, 收到的总字节数, 收到的字节数组.Length - 收到的总字节数)
                    If 收到的字节数 > 0 Then
                        收到的总字节数 += 收到的字节数
                        If 收到的总字节数 < 收到的字节数组.Length Then
                            GoTo 继续
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            If 重试次数 < 2 Then
                重试次数 += 1
                GoTo 重试
            Else
                Call HTTPS请求失败(ex.Message)
                Return
            End If
        End Try
        If 收到的字节数组 IsNot Nothing Then
            If 收到的总字节数 = 收到的字节数组.Length Then
                Call HTTPS请求成功(收到的字节数组)
                Return
            End If
        End If
        Call HTTPS请求成功(Nothing)
    End Sub

    Private Sub HTTPS请求成功(ByVal SS包() As Byte)
        线程_HTTPS访问 = Nothing
        If SS包 IsNot Nothing Then
            Try
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                Select Case SS包解读器.查询结果
                    Case 查询结果_常量集合.成功
                        状态信息 = 服务器子域名
                        SS包解读器.读取_有标签("凭据", 连接凭据)
                        If String.IsNullOrEmpty(连接凭据) = False Then
                            下载数据库文件()
                        Else
                            状态信息 = 服务器子域名 & "<br>" & 界面文字.获取(148, "由于未知原因，操作失败。")
                            RaiseEvent 显示异常信息(Me, 状态信息)
                            定时器.Interval = 分钟 * 60000
                            定时器.Start()
                        End If
                    Case 查询结果_常量集合.失败
                        状态信息 = 服务器子域名 & "<br>" & 界面文字.获取(148, "由于未知原因，操作失败。")
                        RaiseEvent 显示异常信息(Me, 状态信息)
                        记录异常信息(界面文字.获取(213, "#% 分钟后重试", New Object() {分钟}), False)
                        定时器.Interval = 分钟 * 60000
                        定时器.Start()
                    Case Else
                        状态信息 = 服务器子域名 & "<br>" & 界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果})
                        RaiseEvent 显示异常信息(Me, 状态信息)
                        记录异常信息(界面文字.获取(213, "#% 分钟后重试", New Object() {分钟}), False)
                        定时器.Interval = 分钟 * 60000
                        定时器.Start()
                End Select
            Catch ex As Exception
                记录异常信息(ex.Message)
            End Try
        End If
    End Sub

    Private Sub HTTPS请求失败(ByVal 原因 As String)
        线程_HTTPS访问 = Nothing
        记录异常信息(原因)
        定时器.Interval = 分钟 * 60000
        定时器.Start()
    End Sub

    Private Sub 下载器_下载结束(sender As Object, 结果 As 类_数据库下载器.常量集合_结果, 下载的数据() As Byte) Handles 数据库下载器.下载结束
        Select Case 结果
            Case 类_数据库下载器.常量集合_结果.成功
                下载失败次数 = 0
                Call 处理数据(下载的数据)
            Case 类_数据库下载器.常量集合_结果.失败_重试下载
                If 停止备份 = False Then
                    下载失败次数 += 1
                    If 下载失败次数 < 10 Then
                        Call 下载数据库文件()
                    Else
                        下载失败次数 = 0
                        记录异常信息(界面文字.获取(213, "#% 分钟后重试", New Object() {分钟}), False)
                        定时器.Interval = 分钟 * 60000
                        定时器.Start()
                    End If
                End If
            Case 类_数据库下载器.常量集合_结果.失败
                记录异常信息(Unicode.GetString(下载的数据))
                定时器.Interval = 分钟 * 60000
                定时器.Start()
        End Select
    End Sub

    Private Sub 处理数据(ByVal 字节数组() As Byte)
        Dim 继续下载 As Boolean
        Try
            Dim SS包解读器 As New 类_SS包解读器(字节数组)
            If 有新凭据 Then
                Select Case SS包解读器.查询结果
                    Case 查询结果_常量集合.新凭据添加成功
                        有新凭据 = False
                    Case 查询结果_常量集合.失败, 查询结果_常量集合.出错
                    Case Else
                        记录异常信息(界面文字.获取(218, "未收到[新凭据添加成功]回答。"))
                        RaiseEvent 显示异常信息(Me, 状态信息)
                        Return
                End Select
            End If
            Select Case SS包解读器.查询结果
                Case 查询结果_常量集合.新页缓存文件数据
                    Dim 新验证数字 As Long
                    SS包解读器.读取_有标签("NewVerificationNumber", 新验证数字)
                    Dim 新页缓存文件名 As String = Nothing
                    SS包解读器.读取_有标签("FileName", 新页缓存文件名)
                    Dim 页数 As Integer
                    SS包解读器.读取_有标签("PageNumber", 页数)
                    Dim 头数据() As Byte = Nothing
                    SS包解读器.读取_有标签("HeadData", 头数据)
                    Dim 字节数组2() As Byte = Nothing
                    SS包解读器.读取_有标签("Data", 字节数组2)
                    If 字节数组2 IsNot Nothing Then
                        If 文件流_备份数据库 IsNot Nothing Then 文件流_备份数据库.Close()
                        Call 打开文件流()
                        Call 将新页缓存文件的备份数据写入文件流(文件流_备份数据库, 备份文件存放路径, 新页缓存文件名, 头数据, 字节数组2, 页数)
                        备份数据文件长度 = 文件流_备份数据库.Length
                        更新的页数 += 页数
                        If 更新的页数 >= 2000 Then
                            更新的页数 = 0
                            Call 下载媒体文件()
                        End If
                    End If
                    继续下载 = 数据库_更新验证数字(新验证数字)
                Case 查询结果_常量集合.强制替换旧页
                    Dim 秒数 As Integer = 5
                    记录异常信息(界面文字.获取(217, "服务器数据库正在强制替换旧页，#% 秒后重试。", New Object() {秒数}), False)
                    定时器.Interval = 秒数 * 60000
                    定时器.Start()
                Case 查询结果_常量集合.稍后重试
                    Dim 秒数 As Integer = 5
                    记录异常信息(界面文字.获取(216, "服务器数据库正在替换旧页。#% 秒后重试。", New Object() {秒数}), False)
                    定时器.Interval = 秒数 * 60000
                    定时器.Start()
                Case 查询结果_常量集合.无新页, 查询结果_常量集合.放弃强制替换旧页
                    Dim 文件长度 As Long
                    SS包解读器.读取_有标签("FileLength", 文件长度)
                    If 文件长度 <> 备份数据文件长度 Then
                        If SS包解读器.查询结果 = 查询结果_常量集合.无新页 Then
                            记录异常信息(界面文字.获取(215, "由于文件长度不一致，暂停备份！服务器数据库文件长度为 #% 字节，备份数据库文件长度为 #% 字节。", New Object() {FormatNumber(文件长度, 0, , , TriState.True), FormatNumber(备份数据文件长度, 0, , , TriState.True)}))
                            Return
                        End If
                    End If
                    Call 下载媒体文件()
                    记录异常信息(界面文字.获取(214, "#% 无新页，#% 分钟后再次检查。", New Object() {Date.Now.ToLongTimeString, 分钟}), False)
                    定时器.Interval = 分钟 * 60000
                    定时器.Start()
                Case 查询结果_常量集合.旧页数据
                    Dim 新验证数字 As Long
                    SS包解读器.读取_有标签("NewVerificationNumber", 新验证数字)
                    Dim 字节数组2() As Byte = Nothing
                    SS包解读器.读取_有标签("Data", 字节数组2)
                    If 文件流_备份数据库 IsNot Nothing Then 文件流_备份数据库.Close()
                    Call 打开文件流()
                    Call 将旧页数据写入文件流(文件流_备份数据库, 字节数组2)
                    备份数据文件长度 = 文件流_备份数据库.Length
                    继续下载 = 数据库_更新验证数字(新验证数字)
                Case 查询结果_常量集合.无旧页
                    Call 旧页数据已全部写入(文件流_备份数据库)
                    继续下载 = True
                Case 查询结果_常量集合.新凭据添加成功
                    Call 打开文件流()
                    继续下载 = True
                Case 查询结果_常量集合.验证数字不正确
                    记录异常信息(界面文字.获取(219, "验证数字不正确。"))
                Case 查询结果_常量集合.备份凭据不正确
                    记录异常信息(界面文字.获取(220, "备份凭据不正确。"))
                Case 查询结果_常量集合.数据库备份凭据已被锁定
                    记录异常信息(界面文字.获取(221, "数据库备份凭据已被锁定。"))
                Case 查询结果_常量集合.无法更新主数据库的复制记录
                    记录异常信息(界面文字.获取(222, "无法更新主数据库的复制记录。"))
                Case 查询结果_常量集合.用哈希验证新页缓存文件时出现异常
                    记录异常信息(界面文字.获取(223, "用哈希验证新页缓存文件时出现异常。"))
                Case 查询结果_常量集合.复制旧页数据时超时
                    记录异常信息(界面文字.获取(224, "复制旧页数据时超时。请用新凭据备份。"))
                Case 查询结果_常量集合.失败
                    Dim 原因 As String = Nothing
                    SS包解读器.读取_有标签("Reason", 原因)
                    记录异常信息(原因)
                Case 查询结果_常量集合.出错
                    记录异常信息(SS包解读器.出错提示文本)
            End Select
        Catch ex As Exception
            记录异常信息(ex.Message)
        End Try
        If 继续下载 AndAlso 停止备份 = False Then Call 下载数据库文件()
    End Sub

    Private Function 获取文件路径() As String
        Dim 路径 As String = 备份文件存放路径 & "\" & 服务器子域名 & "\"
        If 服务器子域名.StartsWith(讯宝中心服务器主机名 & ".") Then
            Return 路径 & "CenterData_RequireBackup" & 数据库文件扩展名
        ElseIf 服务器子域名.StartsWith(讯宝中心服务器主机名) Then
            Return 路径 & "TransportData_RequireBackup" & 数据库文件扩展名
        ElseIf 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
            Return 路径 & "TinyUniverseData_RequireBackup" & 数据库文件扩展名
        ElseIf 服务器子域名.StartsWith(讯宝大聊天群服务器主机名前缀) Then
            Return 路径 & "ChatGroupData_RequireBackup" & 数据库文件扩展名
        Else
            Return ""
        End If
    End Function

    Private Sub 记录异常信息(ByVal 文本 As String, Optional ByVal 显示到窗体 As Boolean = True)
        状态信息 = 服务器子域名 & "<br>" & 文本
        If 显示到窗体 = True Then RaiseEvent 显示异常信息(Me, 状态信息)
    End Sub

    Private Sub 打开文件流()
        Dim 路径 As String = 获取文件路径()
        Dim 目录 As String = Path.GetDirectoryName(路径)
        If Directory.Exists(目录) = False Then Directory.CreateDirectory(目录)
        文件流_备份数据库 = New FileStream(路径, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
    End Sub

    Private Sub 定时器_Elapsed(sender As Object, e As System.Timers.ElapsedEventArgs) Handles 定时器.Elapsed
        定时器.Stop()
        Call 下载数据库文件()
    End Sub

    Private Function 数据库_获取备份凭据() As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"备份凭据", "验证数字"})
            Dim 指令 As New 类_数据库指令_请求获取数据(备份管理器.数据库_备份凭据, "数据库备份凭据", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                备份凭据 = 读取器(0)
                验证数字 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_添加备份凭据() As Boolean
        备份凭据 = 生成大小写英文字母与数字的随机字符串(20)
        有新凭据 = True
        验证数字 = 0
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于插入数据("子域名", 服务器子域名)
            列添加器.添加列_用于插入数据("备份凭据", 备份凭据)
            列添加器.添加列_用于插入数据("验证数字", 验证数字)
            Dim 指令3 As New 类_数据库指令_插入新数据(备份管理器.数据库_备份凭据, "数据库备份凭据", 列添加器)
            指令3.执行()
            Return True
        Catch ex As Exception
            备份凭据 = Nothing
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_更新验证数字(ByVal 新验证数字 As Long) As Boolean
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("验证数字", 新验证数字)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(备份管理器.数据库_备份凭据, "数据库备份凭据", 列添加器_新数据, 筛选器, 主键索引名)
            If 指令.执行 > 0 Then
                验证数字 = 新验证数字
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Sub 下载媒体文件()
        If 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") = False AndAlso 服务器子域名.StartsWith(讯宝大聊天群服务器主机名前缀) = False Then Return
        If 文件流_备份数据库 IsNot Nothing Then
            文件流_备份数据库.Close()
            文件流_备份数据库 = Nothing
        End If
        If 备份数据库 Is Nothing Then
            Dim 类 As New 类_打开或创建数据库
            备份数据库 = 类.打开备份数据库(获取文件路径)
            If 备份数据库 Is Nothing Then
                记录异常信息("无法打开备份数据库")
                Return
            End If
        End If
        Dim 最大编号 As Long
        If 副数据库_获取流星语最大编号(最大编号) = True Then
            Dim 编号(99) As Long
            Dim 编号数量 As Integer
            If 备份数据库_获取新流星语(最大编号, 编号, 编号数量) = True Then
                If 编号数量 > 0 Then
                    If 副数据库_添加新流星语(编号, 编号数量) = False Then
                        备份数据库.关闭()
                        备份数据库 = Nothing
                        Return
                    End If
                End If
            Else
                备份数据库.关闭()
                备份数据库 = Nothing
                Return
            End If
        Else
            备份数据库.关闭()
            备份数据库 = Nothing
            Return
        End If
        If 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
            最大编号 = 0
            If 副数据库_获取商品最大编号(最大编号) = True Then
                Dim 编号(99) As Long
                Dim 编号数量 As Integer
                If 备份数据库_获取新商品(最大编号, 编号, 编号数量) = True Then
                    If 编号数量 > 0 Then
                        If 副数据库_添加新商品(编号, 编号数量) = False Then
                            备份数据库.关闭()
                            备份数据库 = Nothing
                            Return
                        End If
                    End If
                Else
                    备份数据库.关闭()
                    备份数据库 = Nothing
                    Return
                End If
            Else
                备份数据库.关闭()
                备份数据库 = Nothing
                Return
            End If
        End If
        备份数据库.关闭()
        备份数据库 = Nothing
        If 媒体下载器 Is Nothing Then 下载媒体文件1()
    End Sub

    Private Function 下载媒体文件1() As Short
        If 备份数据库 Is Nothing Then
            Dim 类 As New 类_打开或创建数据库
            备份数据库 = 类.打开备份数据库(获取文件路径)
            If 备份数据库 Is Nothing Then
                Return 0
            End If
        End If
        If 数据库_获取未下载的流星语() = False Then
            备份数据库.关闭()
            备份数据库 = Nothing
            Return 0
        End If
        If 未下载流星语数 > 0 Then
            ReDim 正在下载的流星语(未下载流星语数 - 1)
            Array.Copy(未下载的流星语, 0, 正在下载的流星语, 0, 未下载流星语数)
            索引_正在下载的流星语 = 0
        Else
            正在下载的流星语 = Nothing
        End If
        If 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
            If 数据库_获取未下载的商品() = False Then
                备份数据库.关闭()
                备份数据库 = Nothing
                Return 0
            End If
            If 未下载商品数 > 0 Then
                If 未下载商品数 < 未下载的商品.Length Then ReDim Preserve 未下载的商品(未下载商品数 - 1)
                ReDim 正在下载的商品(未下载商品数 - 1)
                Array.Copy(未下载的商品, 0, 正在下载的商品, 0, 未下载商品数)
                索引_正在下载的商品 = 0
            Else
                正在下载的商品 = Nothing
            End If
        End If
        备份数据库.关闭()
        备份数据库 = Nothing
        If 未下载流星语数 > 0 OrElse 未下载商品数 > 0 Then
            Return 下载媒体文件2()
        End If
        Return -1
    End Function

    Private Function 下载媒体文件2() As Short
        If 正在下载的流星语 IsNot Nothing Then
跳转点1:
            If 索引_正在下载的流星语 < 正在下载的流星语.Length Then
                With 正在下载的流星语(索引_正在下载的流星语)
                    If .文件名 IsNot Nothing AndAlso .索引_当前下载的文件 < .文件名.Length Then
                        Dim 保存路径 As String = 备份文件存放路径 & "\" & 服务器子域名 & "\MR\" & .英语用户名 & "\" & .文件名(.索引_当前下载的文件)
                        Dim 下载地址 As String
                        If 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
                            下载地址 = "https://" & 获取服务器域名(服务器子域名) & "/backup/media/?Credential=" & 连接凭据 & "&EnglishUsername=" & 替换URI敏感字符(.英语用户名) & "&FileName=" & 替换URI敏感字符(.文件名(.索引_当前下载的文件))
                        Else
                            下载地址 = "https://" & 获取服务器域名(服务器子域名) & "/backup/media/?Credential=" & 连接凭据 & "&GroupID=" & .群编号 & "&FileName=" & 替换URI敏感字符(.文件名(.索引_当前下载的文件))
                        End If
                        If 媒体下载器 Is Nothing Then 媒体下载器 = New 类_下载媒体文件()
                        媒体下载器.下载(正在下载的流星语(索引_正在下载的流星语), 下载地址 & "&v=" & Date.Now.Ticks, 保存路径)
                        Return 1
                    End If
                End With
                索引_正在下载的流星语 += 1
                GoTo 跳转点1
            ElseIf 索引_正在下载的流星语 = 正在下载的流星语.Length Then
                Dim I As Integer
                For I = 0 To 索引_正在下载的流星语 - 1
                    If 副数据库_流星语已下载(正在下载的流星语(I).编号) = False Then Return 0
                Next
            End If
        End If
        If 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".") Then
            If 正在下载的商品 IsNot Nothing Then
跳转点2:
                If 索引_正在下载的商品 < 正在下载的商品.Length Then
                    With 正在下载的商品(索引_正在下载的商品)
                        If .文件名 IsNot Nothing AndAlso .索引_当前下载的文件 < .文件名.Length Then
                            Dim 保存路径 As String = 备份文件存放路径 & "\" & 服务器子域名 & "\GD\" & .文件名(.索引_当前下载的文件)
                            Dim 下载地址 As String = "https://" & 获取服务器域名(服务器子域名) & "/backup/media/?Credential=" & 连接凭据 & "&FileName=" & 替换URI敏感字符(.文件名(.索引_当前下载的文件))
                            If 媒体下载器 Is Nothing Then 媒体下载器 = New 类_下载媒体文件()
                            媒体下载器.下载(正在下载的商品(索引_正在下载的商品), 下载地址 & "&v=" & Date.Now.Ticks, 保存路径)
                            Return 1
                        End If
                    End With
                    索引_正在下载的商品 += 1
                    GoTo 跳转点2
                ElseIf 索引_正在下载的商品 = 正在下载的商品.Length Then
                    Dim I As Integer
                    For I = 0 To 索引_正在下载的商品 - 1
                        If 副数据库_商品已下载(正在下载的商品(I).编号) = False Then Return 0
                    Next
                End If
            End If
        End If
        Return -1
    End Function

    Private Function 副数据库_获取流星语最大编号(ByRef 最大编号 As Long) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份管理器.数据库_备份凭据, "流星语", 筛选器, 1, 列添加器, , "#已发布")
            读取器 = 指令.执行()
            While 读取器.读取
                最大编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 副数据库_获取商品最大编号(ByRef 最大编号 As Long) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份管理器.数据库_备份凭据, "商品", Nothing, 1, 列添加器, , 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                最大编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 备份数据库_获取新流星语(ByVal 最大编号 As Long, ByRef 编号() As Long, ByRef 编号数量 As Integer) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.大于, 最大编号)
            列添加器.添加列_用于筛选器("类型", 筛选方式_常量集合.小于等于, 流星语类型_常量集合.视频)
            列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份数据库, "流星语", 筛选器, , 列添加器, 100, 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                If 编号数量 = 编号.Length Then ReDim Preserve 编号(编号数量 * 2 - 1)
                编号(编号数量) = 读取器(0)
                编号数量 += 1
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 备份数据库_获取新商品(ByVal 最大编号 As Long, ByRef 编号() As Long, ByRef 编号数量 As Integer) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.大于, 最大编号)
            列添加器.添加列_用于筛选器("已删除", 筛选方式_常量集合.等于, False)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份数据库, "商品", 筛选器, , 列添加器, 100, 主键索引名)
            读取器 = 指令.执行()
            While 读取器.读取
                If 编号数量 = 编号.Length Then ReDim Preserve 编号(编号数量 * 2 - 1)
                编号(编号数量) = 读取器(0)
                编号数量 += 1
            End While
            读取器.关闭()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 副数据库_添加新流星语(ByVal 编号() As Long, ByVal 编号数量 As Integer) As Boolean
        Try
            Dim I As Integer
            For I = 0 To 编号数量 - 1
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于插入数据("子域名", 服务器子域名)
                列添加器.添加列_用于插入数据("编号", 编号(I))
                列添加器.添加列_用于插入数据("已下载", False)
                Dim 指令3 As New 类_数据库指令_插入新数据(备份管理器.数据库_备份凭据, "流星语", 列添加器)
                指令3.执行()
            Next
            Return True
        Catch ex As Exception
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 副数据库_添加新商品(ByVal 编号() As Long, ByVal 编号数量 As Integer) As Boolean
        Try
            Dim I As Integer
            For I = 0 To 编号数量 - 1
                Dim 列添加器 As New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 编号(I))
                列添加器.添加列_用于插入数据("已下载", False)
                Dim 指令3 As New 类_数据库指令_插入新数据(备份管理器.数据库_备份凭据, "商品", 列添加器)
                指令3.执行()
            Next
            Return True
        Catch ex As Exception
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_获取未下载的流星语() As Boolean
        ReDim 未下载的流星语(99)
        未下载流星语数 = 0
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份管理器.数据库_备份凭据, "流星语", 筛选器, , 列添加器, 100, "#未下载")
            读取器 = 指令.执行()
            While 读取器.读取
                If 未下载流星语数 = 未下载的流星语.Length Then ReDim Preserve 未下载的流星语(未下载流星语数 * 2 - 1)
                未下载的流星语(未下载流星语数) = New 类_流星语或商品
                未下载的流星语(未下载流星语数).编号 = 读取器(0)
                未下载流星语数 += 1
            End While
            读取器.关闭()
            If 未下载流星语数 > 0 Then
                Dim 类型 As 流星语类型_常量集合
                Dim 样式 As 流星语列表项样式_常量集合
                Dim 文本库号 As Short
                Dim 文本编号 As Long
                Dim 图片数量 As Integer
                Dim 正文 As String = Nothing
                Const 图片开始 As String = "<IMG>"
                Const 图片结束 As String = "</IMG>"
                Dim 是群 As Boolean = Not 服务器子域名.StartsWith(讯宝小宇宙中心服务器主机名 & ".")
                Dim I, J, K As Integer
                For I = 0 To 未下载流星语数 - 1
                    With 未下载的流星语(I)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        If 是群 = False Then
                            列添加器.添加列_用于获取数据(New String() {"英语用户名", "类型", "样式", "文本库号", "文本编号"})
                        Else
                            列添加器.添加列_用于获取数据(New String() {"群编号", "类型", "样式", "文本库号", "文本编号"})
                        End If
                        指令 = New 类_数据库指令_请求获取数据(备份数据库, "流星语", 筛选器, 1, 列添加器, , 主键索引名)
                        读取器 = 指令.执行()
                        While 读取器.读取
                            If 是群 = False Then
                                .英语用户名 = 读取器(0)
                            Else
                                .群编号 = 读取器(0)
                            End If
                            类型 = 读取器(1)
                            样式 = 读取器(2)
                            文本库号 = 读取器(3)
                            文本编号 = 读取器(4)
                            Exit While
                        End While
                        读取器.关闭()
                        Select Case 类型
                            Case 流星语类型_常量集合.图文
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据("SS包")
                                指令 = New 类_数据库指令_请求获取数据(备份数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                                Dim SS包() As Byte = Nothing
                                读取器 = 指令.执行()
                                While 读取器.读取
                                    SS包 = 读取器(0)
                                    Exit While
                                End While
                                读取器.关闭()
                                正文 = Nothing
                                图片数量 = 0
                                If SS包 IsNot Nothing Then
                                    Dim SS包解读器 As New 类_SS包解读器(SS包)
                                    SS包解读器.读取_有标签("T", 正文)
                                    If String.IsNullOrEmpty(正文) = False Then
                                        图片数量 = 统计图片数量(正文, 图片开始)
                                    End If
                                End If
                                Select Case 样式
                                    Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.一幅大图片
                                        图片数量 += 1
                                    Case 流星语列表项样式_常量集合.三幅小图片
                                        图片数量 += 3
                                End Select
                                If 图片数量 > 0 Then
                                    ReDim .文件名(图片数量 - 1)
                                    图片数量 = 0
                                    If String.IsNullOrEmpty(正文) = False Then
                                        J = 0
跳转点1:
                                        J = 正文.IndexOf(图片开始, J)
                                        If J >= 0 Then
                                            J += 图片开始.Length
                                            K = 正文.IndexOf(图片结束, J)
                                            If K >= 0 Then
                                                .文件名(图片数量) = .编号 & "_" & (图片数量 + 1) & "." & 正文.Substring(J, K - J)
                                                图片数量 += 1
                                                J = K + 图片结束.Length
                                                GoTo 跳转点1
                                            End If
                                        End If
                                    End If
                                    Select Case 样式
                                        Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.一幅大图片
                                            .文件名(图片数量) = .编号 & "_" & 1 & "_pre.jpg"
                                            图片数量 += 1
                                        Case 流星语列表项样式_常量集合.三幅小图片
                                            For J = 1 To 3
                                                .文件名(图片数量) = .编号 & "_" & J & "_pre.jpg"
                                                图片数量 += 1
                                            Next
                                    End Select
                                End If
                            Case 流星语类型_常量集合.视频
                                ReDim .文件名(1)
                                .文件名(0) = .编号 & ".mp4"
                                .文件名(1) = .编号 & ".jpg"
                        End Select
                    End With
                Next
            End If
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 数据库_获取未下载的商品() As Boolean
        ReDim 未下载的商品(99)
        未下载商品数 = 0
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令 As New 类_数据库指令_请求获取数据(备份管理器.数据库_备份凭据, "商品", Nothing, , 列添加器, 100, "#未下载")
            读取器 = 指令.执行()
            While 读取器.读取
                If 未下载商品数 = 未下载的商品.Length Then ReDim Preserve 未下载的商品(未下载商品数 * 2 - 1)
                未下载的商品(未下载商品数) = New 类_流星语或商品
                未下载的商品(未下载商品数).编号 = 读取器(0)
                未下载商品数 += 1
            End While
            读取器.关闭()
            If 未下载商品数 > 0 Then
                Dim 样式 As 流星语列表项样式_常量集合
                Dim 图片数量 As Integer
                Dim 详情 As String = Nothing
                Const 图片开始 As String = "<IMG>"
                Const 图片结束 As String = "</IMG>"
                Dim I, J, K As Integer
                For I = 0 To 未下载商品数 - 1
                    With 未下载的商品(I)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, .编号)
                        Dim 筛选器 As New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于获取数据(New String() {"样式", "详情"})
                        指令 = New 类_数据库指令_请求获取数据(备份数据库, "商品", 筛选器, 1, 列添加器, , 主键索引名)
                        读取器 = 指令.执行()
                        While 读取器.读取
                            样式 = 读取器(0)
                            详情 = 读取器(1)
                            Exit While
                        End While
                        读取器.关闭()
                        图片数量 = 0
                        图片数量 = 统计图片数量(详情, 图片开始)
                        Select Case 样式
                            Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.一幅大图片
                                图片数量 += 1
                            Case 流星语列表项样式_常量集合.三幅小图片
                                图片数量 += 3
                        End Select
                        ReDim .文件名(图片数量 - 1)
                        图片数量 = 0
                        J = 0
跳转点1:
                        J = 详情.IndexOf(图片开始, J)
                        If J >= 0 Then
                            J += 图片开始.Length
                            K = 详情.IndexOf(图片结束, J)
                            If K >= 0 Then
                                .文件名(图片数量) = .编号 & "_" & (图片数量 + 1) & "." & 详情.Substring(J, K - J)
                                图片数量 += 1
                                J = K + 图片结束.Length
                                GoTo 跳转点1
                            End If
                        End If
                        Select Case 样式
                            Case 流星语列表项样式_常量集合.一幅小图片, 流星语列表项样式_常量集合.一幅大图片
                                .文件名(图片数量) = .编号 & "_" & 1 & "_pre.jpg"
                                图片数量 += 1
                            Case 流星语列表项样式_常量集合.三幅小图片
                                For J = 1 To 3
                                    .文件名(图片数量) = .编号 & "_" & J & "_pre.jpg"
                                    图片数量 += 1
                                Next
                        End Select
                    End With
                Next
            End If
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 统计图片数量(ByVal 正文 As String, ByVal 图片开始 As String) As Integer
        Dim I, 图片数量 As Integer
跳转点1:
        I = 正文.IndexOf(图片开始, I)
        If I >= 0 Then
            图片数量 += 1
            I += 图片开始.Length
            GoTo 跳转点1
        End If
        Return 图片数量
    End Function

    Private Function 副数据库_流星语已下载(ByVal 编号 As Long) As Boolean
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("已下载", True)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 服务器子域名)
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(备份管理器.数据库_备份凭据, "流星语", 列添加器_新数据, 筛选器, "#已发布")
            指令.执行()
            Return True
        Catch ex As Exception
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Private Function 副数据库_商品已下载(ByVal 编号 As Long) As Boolean
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("已下载", True)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_更新数据(备份管理器.数据库_备份凭据, "商品", 列添加器_新数据, 筛选器, 主键索引名)
            指令.执行()
            Return True
        Catch ex As Exception
            记录异常信息(ex.Message)
            Return False
        End Try
    End Function

    Friend Sub 停止()
        停止备份 = True
        If 线程_HTTPS访问 IsNot Nothing Then
            Try
                线程_HTTPS访问.Abort()
                线程_HTTPS访问 = Nothing
            Catch ex As Exception
            End Try
        End If
        If 定时器 IsNot Nothing Then 定时器.Stop()
        If 数据库下载器 IsNot Nothing Then
            数据库下载器.暂停()
            数据库下载器 = Nothing
        End If
    End Sub

    Private Sub 媒体下载器_下载结束(sender As Object, 结果 As 类_下载媒体文件.常量集合_结果, 正在下载的项目 As 类_流星语或商品) Handles 媒体下载器.下载结束
        Select Case 结果
            Case 类_下载媒体文件.常量集合_结果.成功
                正在下载的项目.索引_当前下载的文件 += 1
跳转点1:
                If 停止备份 = False Then
                    If 下载媒体文件2() < 0 Then
                        If 下载媒体文件1() < 0 Then
                            媒体下载器.Dispose()
                            媒体下载器 = Nothing
                        End If
                    End If
                End If
            Case 类_下载媒体文件.常量集合_结果.失败_下载中断, 类_下载媒体文件.常量集合_结果.失败_未成功连接网站, 类_下载媒体文件.常量集合_结果.失败_字节数不一致
                GoTo 跳转点1
            Case 类_下载媒体文件.常量集合_结果.失败_未找到文件
                正在下载的项目.索引_当前下载的文件 += 1
                GoTo 跳转点1
        End Select
    End Sub

End Class
