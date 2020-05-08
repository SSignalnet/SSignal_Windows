Imports System.IO
Imports System.Text
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Module 模块_静态方法

    Public Function 替换HTML和JS敏感字符(ByVal 文本 As String) As String
        If String.IsNullOrEmpty(文本) = False Then
            Dim 字符数组() As Char = 文本.ToCharArray
            Dim 变长文本 As New StringBuilder(文本.Length * 2)
            Dim 文本写入器 As New StringWriter(变长文本)
            Dim I As Integer
            For I = 0 To 字符数组.Length - 1
                Select Case 字符数组(I)
                    Case "<"c : 文本写入器.Write("&lt;")
                    Case ">"c : 文本写入器.Write("&gt;")
                    Case "&"c : 文本写入器.Write("&amp;")
                    Case "'"c : 文本写入器.Write("&apos;")
                    Case """"c : 文本写入器.Write("&quot;")
                    Case ChrW(10) : 文本写入器.Write("<br>")
                    Case ChrW(13)
                    Case "\"c : 文本写入器.Write("\\")
                    Case Else : 文本写入器.Write(字符数组(I))
                End Select
            Next
            文本写入器.Close()
            Return 文本写入器.ToString
        Else
            Return ""
        End If
    End Function

    Friend Function 处理文件路径以用作JS函数参数(ByVal 文件路径 As String) As String
        If String.IsNullOrEmpty(文件路径) = False Then
            Dim 字符数组() As Char = 文件路径.ToCharArray
            Dim 变长文本 As New StringBuilder(字符数组.Length * 2)
            Dim 文本写入器 As New StringWriter(变长文本)
            Dim I As Integer
            For I = 0 To 字符数组.Length - 1
                Select Case 字符数组(I)
                    Case "\" : 文本写入器.Write("/")
                    Case ChrW(39) : 文本写入器.Write("\'")
                    Case ChrW(34) : 文本写入器.Write("\""")
                    Case Else : 文本写入器.Write(字符数组(I))
                End Select
            Next
            文本写入器.Close()
            Return 文本写入器.ToString
        Else
            Return ""
        End If
    End Function

    Friend Sub 收集标签(ByVal 某一标签 As String, ByRef 讯友标签() As String, ByRef 讯友标签数 As Integer)
        If String.IsNullOrEmpty(某一标签) Then Return
        Dim J As Integer
        If 讯友标签数 > 0 Then
            For J = 0 To 讯友标签数 - 1
                If String.Compare(讯友标签(J), 某一标签, True) = 0 Then
                    Return
                End If
            Next
        End If
        讯友标签(J) = 某一标签
        讯友标签数 += 1
    End Sub

    Friend Function 数据库_保存要发送的一对一讯宝(ByVal 机器人 As 类_机器人, ByVal 讯宝地址 As String, ByVal 存储时间 As Long,
                                       ByVal 讯宝指令 As 讯宝指令_常量集合, Optional ByVal 文本 As String = Nothing,
                                       Optional ByVal 宽度 As Short = 0, Optional ByVal 高度 As Short = 0,
                                       Optional ByVal 秒数 As Byte = 0) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 讯宝地址)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(讯宝地址, 地址或域编号)
                    If 地址或域编号 = 0 Then Return False
                End If
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("地址或域编号", 地址或域编号)
                列添加器.添加列_用于插入数据("群编号", 0)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("讯宝地址", 讯宝地址)
            列添加器.添加列_用于插入数据("是接收者", True)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("已收听", True)
            列添加器.添加列_用于插入数据("发送序号", 0)
            列添加器.添加列_用于插入数据("发送时间", 存储时间)
            列添加器.添加列_用于插入数据("存储时间", 存储时间)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "一对一讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            机器人.说(ex.Message)
            Return False
        End Try
    End Function

    Friend Function 数据库_保存要发送的小聊天群讯宝(ByVal 机器人 As 类_机器人, ByVal 群主英语讯宝地址 As String,
                                    ByVal 群编号 As Byte, ByVal 存储时间 As Long, ByVal 讯宝指令 As 讯宝指令_常量集合,
                                    Optional ByVal 文本 As String = Nothing, Optional ByVal 宽度 As Short = 0,
                                    Optional ByVal 高度 As Short = 0, Optional ByVal 秒数 As Byte = 0) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 文本库号 As Short
            Dim 文本编号 As Long
            Dim 列添加器 As 类_列添加器
            If String.IsNullOrEmpty(文本) = False Then
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 群主英语讯宝地址)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
                Dim 地址或域编号 As Long
                读取器 = 指令2.执行()
                While 读取器.读取
                    地址或域编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                If 地址或域编号 = 0 Then
                    Call 数据库_分配地址或域编号(群主英语讯宝地址, 地址或域编号)
                    If 地址或域编号 = 0 Then Return False
                End If
                文本库号 = 获取文本库号(文本.Length)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于获取数据("编号")
                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", Nothing, 1, 列添加器, , 主键索引名, True)
                读取器 = 指令2.执行()
                While 读取器.读取
                    文本编号 = 读取器(0)
                    Exit While
                End While
                读取器.关闭()
                文本编号 += 1
                列添加器 = New 类_列添加器
                列添加器.添加列_用于插入数据("编号", 文本编号)
                列添加器.添加列_用于插入数据("文本", 文本)
                列添加器.添加列_用于插入数据("地址或域编号", 地址或域编号)
                列添加器.添加列_用于插入数据("群编号", 群编号)
                Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, 文本库号 & "库", 列添加器)
                指令3.执行()
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("群主讯宝地址", 群主英语讯宝地址)
            列添加器.添加列_用于插入数据("群编号", 群编号)
            列添加器.添加列_用于插入数据("发送者讯宝地址", 当前用户.英语讯宝地址)
            列添加器.添加列_用于插入数据("指令", 讯宝指令)
            列添加器.添加列_用于插入数据("文本库号", 文本库号)
            列添加器.添加列_用于插入数据("文本编号", 文本编号)
            列添加器.添加列_用于插入数据("宽度", 宽度)
            列添加器.添加列_用于插入数据("高度", 高度)
            列添加器.添加列_用于插入数据("秒数", 秒数)
            列添加器.添加列_用于插入数据("已收听", True)
            列添加器.添加列_用于插入数据("发送序号", 0)
            列添加器.添加列_用于插入数据("发送时间", 存储时间)
            列添加器.添加列_用于插入数据("存储时间", 存储时间)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "小聊天群讯宝", 列添加器)
            指令.执行()
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            机器人.说(ex.Message)
            Return False
        End Try
    End Function

    Friend Function 数据库_撤回讯宝(ByVal 发送者英语讯宝地址 As String, ByVal 群编号 As Byte,
                             ByVal 群主英语讯宝地址 As String, ByVal 发送序号 As Long, ByVal 发送时间 As Long) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 表名称, 索引名称 As String
            Dim 列添加器 As New 类_列添加器
            If 群编号 = 0 Then
                表名称 = "一对一讯宝"
                列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, False)
                列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                索引名称 = "#地址是接收者发送序号"
            Else
                表名称 = "小聊天群讯宝"
                列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                索引名称 = "#群主编号发送者发送序号"
            End If
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"指令", "文本库号", "文本编号", "发送时间"})
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, 表名称, 筛选器, 1, 列添加器, , 索引名称)
            Dim 讯宝指令 As 讯宝指令_常量集合
            Dim 文本库号 As Short
            Dim 文本编号, 发送时间2 As Long
            读取器 = 指令2.执行()
            While 读取器.读取
                讯宝指令 = 读取器(0)
                文本库号 = 读取器(1)
                文本编号 = 读取器(2)
                发送时间2 = 读取器(3)
                Exit While
            End While
            读取器.关闭()
            If 发送时间2 > 0 AndAlso 发送时间2 > Date.FromBinary(发送时间).AddSeconds(-(最大值_常量集合.讯宝可撤回的时限_秒 + 30)).Ticks Then
                列添加器 = New 类_列添加器
                If 群编号 = 0 Then
                    列添加器.添加列_用于筛选器("讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("是接收者", 筛选方式_常量集合.等于, False)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                Else
                    列添加器.添加列_用于筛选器("群主讯宝地址", 筛选方式_常量集合.等于, 群主英语讯宝地址)
                    列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                    列添加器.添加列_用于筛选器("发送者讯宝地址", 筛选方式_常量集合.等于, 发送者英语讯宝地址)
                    列添加器.添加列_用于筛选器("发送序号", 筛选方式_常量集合.等于, 发送序号)
                End If
                筛选器 = New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令 As New 类_数据库指令_删除数据(副数据库, 表名称, 筛选器, 索引名称)
                If 指令.执行() > 0 Then
                    If 文本库号 > 0 Then
                        Select Case 讯宝指令
                            Case 讯宝指令_常量集合.发送语音, 讯宝指令_常量集合.发送图片
                                列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                                筛选器 = New 类_筛选器
                                筛选器.添加一组筛选条件(列添加器)
                                列添加器 = New 类_列添加器
                                列添加器.添加列_用于获取数据("文本")
                                指令2 = New 类_数据库指令_请求获取数据(副数据库, 文本库号 & "库", 筛选器, 1, 列添加器, , 主键索引名)
                                Dim 文本 As String = Nothing
                                读取器 = 指令2.执行()
                                While 读取器.读取
                                    文本 = 读取器(0)
                                    Exit While
                                End While
                                读取器.关闭()
                                If String.IsNullOrEmpty(文本) = False Then
                                    文本 = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData & "\" & 当前用户.英语讯宝地址 & "\" & 文本
                                    If File.Exists(文本) Then
                                        Try
                                            File.Delete(文本)
                                        Catch ex As Exception
                                        End Try
                                    End If
                                End If
                        End Select
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 文本编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令 = New 类_数据库指令_删除数据(副数据库, 文本库号 & "库", 筛选器, 主键索引名)
                        指令.执行()
                    End If
                    Return True
                End If
            End If
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
        Return False
    End Function

    Friend Function 数据库_更新最近互动讯友排名(ByVal 地址或域名 As String, ByVal 群编号 As Long, Optional ByRef 刷新 As Boolean = False) As Boolean
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于获取数据(New String() {"地址或域名", "群编号"})
            Dim 指令 As New 类_数据库指令_请求获取数据(副数据库, "最近", Nothing, 1, , , "#时间")
            Dim 英语讯宝地址2 As String = Nothing
            Dim 群编号2 As Long
            读取器 = 指令.执行()
            While 读取器.读取
                英语讯宝地址2 = 读取器(0)
                群编号2 = 读取器(1)
                Exit While
            End While
            读取器.关闭()
            If String.IsNullOrEmpty(英语讯宝地址2) OrElse String.Compare(英语讯宝地址2, 地址或域名) <> 0 OrElse 群编号2 <> 群编号 Then
                Dim 列添加器_新数据 As New 类_列添加器
                列添加器_新数据.添加列_用于插入数据("时间", Date.Now.Ticks)
                列添加器 = New 类_列添加器
                列添加器.添加列_用于筛选器("地址或域名", 筛选方式_常量集合.等于, 地址或域名)
                列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                Dim 筛选器 As New 类_筛选器
                筛选器.添加一组筛选条件(列添加器)
                Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "最近", 列添加器_新数据, 筛选器, "#地址群编号")
                If 指令2.执行() = 0 Then
                    列添加器 = New 类_列添加器
                    列添加器.添加列_用于插入数据("地址或域名", 地址或域名)
                    列添加器.添加列_用于插入数据("群编号", 群编号)
                    列添加器.添加列_用于插入数据("时间", Date.Now.Ticks)
                    列添加器.添加列_用于插入数据("新讯宝数量", 0)
                    Dim 指令3 As New 类_数据库指令_插入新数据(副数据库, "最近", 列添加器)
                    指令3.执行()
                End If
                刷新 = True
            End If
            Return True
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
            Return False
        End Try
    End Function

    Friend Sub 数据库_清除大聊天群数据(ByVal 子域名 As String, ByVal 群编号 As Long)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            Dim 地址或域编号 As Long
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域", 筛选方式_常量集合.等于, 子域名)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            列添加器 = New 类_列添加器
            列添加器.添加列_用于获取数据("编号")
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, 列添加器, , 主键索引名)
            读取器 = 指令2.执行()
            While 读取器.读取
                地址或域编号 = 读取器(0)
                Exit While
            End While
            读取器.关闭()
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("子域名", 筛选方式_常量集合.等于, 子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令 As New 类_数据库指令_删除数据(副数据库, "大聊天群讯宝", 筛选器, "#子域名群编号发送时间")
            指令.执行()
            If 地址或域编号 > 0 Then
                Dim I, 最大字数, 最大字数2 As Integer
                For I = 1 To 最大值_常量集合.讯宝文本长度
                    最大字数 = 获取文本库号(I)
                    If 最大字数 <> 最大字数2 Then
                        最大字数2 = 最大字数
                        列添加器 = New 类_列添加器
                        列添加器.添加列_用于筛选器("地址或域编号", 筛选方式_常量集合.等于, 地址或域编号)
                        列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
                        筛选器 = New 类_筛选器
                        筛选器.添加一组筛选条件(列添加器)
                        指令 = New 类_数据库指令_删除数据(副数据库, 最大字数 & "库", 筛选器, "#地址域群编号")
                        指令.执行()
                    End If
                Next
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于筛选器("地址或域名", 筛选方式_常量集合.等于, 子域名)
            列添加器.添加列_用于筛选器("群编号", 筛选方式_常量集合.等于, 群编号)
            筛选器 = New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            指令 = New 类_数据库指令_删除数据(副数据库, "最近", 筛选器, "#地址群编号")
            指令.执行()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
    End Sub

    Friend Sub 数据库_分配地址或域编号(ByVal 英语讯宝地址或域名 As String, Optional ByRef 地址或域编号 As Long = 0)
        Dim 读取器 As 类_读取器_外部 = Nothing
        Try
            地址或域编号 = Date.UtcNow.Ticks
            Dim 找到了 As Boolean
跳转点1:
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("编号", 筛选方式_常量集合.等于, 地址或域编号)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_请求获取数据(副数据库, "地址或域编号", 筛选器, 1, , , "#编号")
            找到了 = False
            读取器 = 指令2.执行()
            While 读取器.读取
                找到了 = True
                Exit While
            End While
            读取器.关闭()
            If 找到了 Then
                地址或域编号 += 1
                GoTo 跳转点1
            End If
            列添加器 = New 类_列添加器
            列添加器.添加列_用于插入数据("地址或域", 英语讯宝地址或域名)
            列添加器.添加列_用于插入数据("编号", 地址或域编号)
            Dim 指令 As New 类_数据库指令_插入新数据(副数据库, "地址或域编号", 列添加器)
            指令.执行()
        Catch ex As Exception
            If 读取器 IsNot Nothing Then 读取器.关闭()
        End Try
    End Sub

    Friend Sub 打开资源管理器并选中文件(ByVal 文件路径 As String)
        Dim 进程 As New Process
        进程.StartInfo.FileName = "explorer"
        进程.StartInfo.Arguments = "/select," + 文件路径
        进程.Start()
    End Sub

End Module
