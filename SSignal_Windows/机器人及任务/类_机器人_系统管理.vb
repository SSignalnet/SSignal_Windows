Option Strict Off
Imports System.IO
Imports SSignal_Protocols
Imports SSignalDB
Imports SSignal_GlobalCommonCode

Friend Class 类_机器人_系统管理
    Inherits 类_机器人
    Implements IDisposable

    Friend Sub New(ByVal 主窗体2 As 主窗体, ByVal 聊天控件1 As 控件_聊天)
        主窗体1 = 主窗体2
        聊天控件 = 聊天控件1
    End Sub

    Friend Overrides Sub 回答(ByVal 用户输入 As String, ByVal 时间 As Long)
        Select Case 用户输入
            Case 任务名称_报表 : Call 获取报表(用户输入)
            Case 任务名称_备份数据库 : Call 备份数据库()
            Case 任务名称_新传送服务器 : Call 添加新传送服务器(用户输入)
            Case 任务名称_新大聊天群服务器 : Call 添加新大聊天群服务器(用户输入)
            Case 任务名称_小宇宙中心服务器 : Call 添加小宇宙中心服务器(用户输入)
            Case 任务名称_添加可注册者 : Call 添加可注册者(用户输入)
            Case 任务名称_移除可注册者 : Call 移除可注册者(用户输入)
            Case 任务名称_商品编辑者 : Call 设置商品编辑者(用户输入)
            Case 任务名称_取消
                If 任务 IsNot Nothing Then
                    任务.结束()
                    任务 = Nothing
                    说(界面文字.获取(16, "已取消。"))
                Else
                    说(界面文字.获取(93, "需要我做什么？"))
                End If
            Case Else
                任务接收用户输入(用户输入)
        End Select
    End Sub

    Private Sub 获取报表(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        任务 = New 类_任务(用户输入, Me)
        说(界面文字.获取(7, "请稍等。"))
        启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(当前用户.域名_英语) & "C=GetReport&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器)))
    End Sub

    Private Sub 备份数据库()
        If 任务 IsNot Nothing Then 任务.结束()
        If 备份管理器 IsNot Nothing Then
            说(备份管理器.获取当前备份状态)
        Else
            If String.IsNullOrEmpty(备份文件存放路径) = False Then
                If Directory.Exists(备份文件存放路径) Then
                    备份管理器 = New 类_备份管理器(主窗体1)
                    If 备份管理器.开始() = False Then
                        备份管理器 = Nothing
                    End If
                    说(备份管理器.获取当前备份状态)
                    Return
                End If
            End If
            说(界面文字.获取(207, "请选择一个文件夹，用于保存数据库文件。"))
            If 主窗体1.文件夹浏览器.ShowDialog = DialogResult.OK Then
                备份文件存放路径 = 主窗体1.文件夹浏览器.SelectedPath
                Call 数据库_更新备份文件存放路径()
                备份管理器 = New 类_备份管理器(主窗体1)
                If 备份管理器.开始() = False Then
                    备份管理器 = Nothing
                End If
                说(备份管理器.获取当前备份状态)
            End If
        End If
    End Sub

    Private Sub 数据库_更新备份文件存放路径()
        Try
            Dim 列添加器_新数据 As New 类_列添加器
            列添加器_新数据.添加列_用于插入数据("备份文件存放路径", 备份文件存放路径)
            Dim 列添加器 As New 类_列添加器
            列添加器.添加列_用于筛选器("英语讯宝地址", 筛选方式_常量集合.等于, 当前用户.英语讯宝地址)
            Dim 筛选器 As New 类_筛选器
            筛选器.添加一组筛选条件(列添加器)
            Dim 指令2 As New 类_数据库指令_更新数据(副数据库, "用户", 列添加器_新数据, 筛选器)
            指令2.执行()
        Catch ex As Exception
            说(ex.Message)
        End Try
    End Sub

    Private Sub 添加新传送服务器(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.传送服务器主机名, 界面文字.获取(103, "请输入主机名。要以#%开头，其后的2个字符是国家代码，接着是2个字符州省代码，最后是数字代表第几台服务器，如 #%cnbj01（域名：#%cnbj01.#%）。", New Object() {讯宝中心服务器主机名, 讯宝中心服务器主机名, 讯宝中心服务器主机名, 当前用户.域名_英语}))
        任务.添加步骤(任务步骤_常量集合.服务器网络地址, 界面文字.获取(163, "请输入服务器的IP地址。如果是在本机测试，请填 ::1 ，不要填 127.0.0.1"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 添加新大聊天群服务器(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.大聊天群服务器主机名, 界面文字.获取(103, "请输入主机名。要以#%开头，其后的2个字符是国家代码，接着是2个字符州省代码，最后是数字代表第几台服务器，如 #%cnbj01（域名：#%cnbj01.#%）。", New Object() {讯宝大聊天群服务器主机名前缀, 讯宝大聊天群服务器主机名前缀, 讯宝大聊天群服务器主机名前缀, 当前用户.域名_英语}))
        任务.添加步骤(任务步骤_常量集合.服务器网络地址, 界面文字.获取(163, "请输入服务器的IP地址。如果是在本机测试，请填 ::1 ，不要填 127.0.0.1"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 添加小宇宙中心服务器(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.服务器网络地址, 界面文字.获取(163, "请输入服务器的IP地址。如果是在本机测试，请填 ::1 ，不要填 127.0.0.1"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 添加可注册者(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.添加移除可注册者, 界面文字.获取(301, "请输入将要获得注册权的电子邮箱地址。如果允许任何人都可注册，请输入*。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 移除可注册者(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.添加移除可注册者, 界面文字.获取(302, "请输入将要取消注册权的电子邮箱地址。如果不允许任何人都可注册，请输入*。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 设置商品编辑者(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then 任务.结束()
        If String.IsNullOrEmpty(当前用户.凭据_管理员) Then
            说(界面文字.获取(265, "请先登录管理中心。"))
            Return
        End If
        任务 = New 类_任务(用户输入, 聊天控件.输入框, Me)
        任务.添加步骤(任务步骤_常量集合.设置商品编辑者, 界面文字.获取(312, "请输入一个英语讯宝地址，他/她将拥有编辑商品的权限。"))
        说(任务.获取当前步骤提示语)
    End Sub

    Private Sub 任务接收用户输入(ByVal 用户输入 As String)
        If 任务 IsNot Nothing Then
            If 任务.步骤数量 > 0 Then
                Dim 结果 As String = 任务.保存当前步骤输入值(用户输入)
                If String.IsNullOrEmpty(结果) Then
                    结果 = 任务.获取当前步骤提示语
                    If String.IsNullOrEmpty(结果) = False Then
                        说(结果)
                    Else
                        启动HTTPS访问线程(任务.生成访问设置)
                    End If
                Else
                    说(结果)
                End If
                Return
            End If
        End If
    End Sub

    Protected Overrides Sub HTTPS请求成功(ByVal SS包() As Byte)
        If 聊天控件.InvokeRequired Then
            Dim d As New HTTPS请求成功_跨线程(AddressOf HTTPS请求成功)
            聊天控件.Invoke(d, New Object() {SS包})
        Else
            聊天控件.下拉列表_任务.Enabled = True
            聊天控件.按钮_说话.Enabled = True
            If SS包 IsNot Nothing Then
                Dim SS包解读器 As New 类_SS包解读器(SS包)
                Select Case SS包解读器.查询结果
                    Case 查询结果_常量集合.成功
                        Select Case 任务.名称
                            Case 任务名称_添加可注册者, 任务名称_移除可注册者
                                说(界面文字.获取(245, "完成。"))
                            Case 任务名称_商品编辑者
                                说(界面文字.获取(245, "完成。"))
                                说(界面文字.获取(327, "#% 须重新登录。", New Object() {任务.获取某步骤的输入值(任务步骤_常量集合.设置商品编辑者)}))
                            Case 任务名称_报表
                                Dim 报表 As String = ""
                                SS包解读器.读取_有标签("报表", 报表)
                                说(报表)
                            Case 任务名称_新传送服务器
                                If String.IsNullOrEmpty(当前用户.主机名) Then
                                    SS包解读器.读取_有标签("主机名", 当前用户.主机名)
                                    当前用户.位置号 = 0
                                End If
                                说(界面文字.获取(154, "服务器账号创建成功。#%.#% [#%]", New Object() {任务.获取某步骤的输入值(任务步骤_常量集合.传送服务器主机名), 当前用户.域名_英语, 任务.获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)}))
                            Case 任务名称_新大聊天群服务器
                                说(界面文字.获取(154, "服务器账号创建成功。#%.#% [#%]", New Object() {任务.获取某步骤的输入值(任务步骤_常量集合.大聊天群服务器主机名), 当前用户.域名_英语, 任务.获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)}))
                            Case 任务名称_小宇宙中心服务器
                                说(界面文字.获取(154, "服务器账号创建成功。#%.#% [#%]", New Object() {讯宝小宇宙中心服务器主机名, 当前用户.域名_英语, 任务.获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)}))
                        End Select
                    Case 查询结果_常量集合.无权操作 : 说(界面文字.获取(154, "你无权进行此项操作。"))
                    Case 查询结果_常量集合.稍后重试 : 说(界面文字.获取(20, "你的操作过于频繁，请#%分钟后再尝试。", New Object() {最近操作次数统计时间_分钟}))
                    Case 查询结果_常量集合.凭据无效 : 说(界面文字.获取(229, "请注销，然后重新登录。")) : Return   '不可直接调用主窗体的 注销成功 方法
                    Case 查询结果_常量集合.账号停用 : 说(界面文字.获取(15, "账号已停用。"))
                    Case 查询结果_常量集合.系统维护 : 说(界面文字.获取(14, "由于服务器正在维护中，暂停服务。"))
                    Case 查询结果_常量集合.出错 : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.出错提示文本}))
                    Case 查询结果_常量集合.失败 : 说(界面文字.获取(148, "由于未知原因，操作失败。"))
                    Case 查询结果_常量集合.数据库未就绪 : 说(界面文字.获取(141, "数据库未就绪。"))
                    Case Else : 说(界面文字.获取(108, "出错 #%", New Object() {SS包解读器.查询结果}))
                End Select
            End If
            任务.结束()
            任务 = Nothing
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                If 线程_HTTPS访问 IsNot Nothing Then
                    Try
                        线程_HTTPS访问.Abort()
                    Catch ex As Exception
                    End Try
                    线程_HTTPS访问 = Nothing
                End If
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

#End Region

End Class
