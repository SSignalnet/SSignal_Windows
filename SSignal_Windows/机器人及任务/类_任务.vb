Imports System.Net
Imports SSignal_Protocols
Imports SSignal_GlobalCommonCode

Friend Class 类_任务

    Private Structure 步骤_复合数据
        Dim 步骤代码 As 任务步骤_常量集合
        Dim 提示语, 输入值 As String
        Dim 是空字符串 As Boolean
    End Structure

    Friend 名称 As String
    Dim 步骤() As 步骤_复合数据
    Friend 步骤数量 As Integer
    Dim 输入框 As TextBox
    Dim 机器人 As 类_机器人

    Friend 身份码类型 As String
    Friend 需要获取验证码图片 As Boolean
    Friend 验证码添加时间 As Long

    Friend Sub New(ByVal 名称1 As String, ByVal 机器人1 As 类_机器人)
        名称 = 名称1
        机器人 = 机器人1
        机器人.提示新任务()
    End Sub

    Friend Sub New(ByVal 名称1 As String, ByVal 输入框1 As TextBox, ByVal 机器人1 As 类_机器人, Optional ByVal 步骤数量1 As Integer = 10)
        名称 = 名称1
        ReDim 步骤(步骤数量1 - 1)
        输入框 = 输入框1
        机器人 = 机器人1
        机器人.提示新任务()
    End Sub

    Friend Sub 添加步骤(ByVal 步骤代码 As 任务步骤_常量集合, ByVal 提示语 As String, Optional ByVal 输入值 As String = Nothing)
        If 步骤数量 > 0 AndAlso String.IsNullOrEmpty(输入值) Then
            Dim I As Integer
            For I = 0 To 步骤数量 - 1
                If 步骤(I).步骤代码 = 步骤代码 Then
                    步骤(I).输入值 = Nothing
                    Return
                End If
            Next
        End If
        With 步骤(步骤数量)
            .步骤代码 = 步骤代码
            .提示语 = 提示语
            .输入值 = 输入值
        End With
        步骤数量 += 1
    End Sub

    Friend Function 获取当前步骤提示语() As String
        Dim I As Integer
        For I = 0 To 步骤数量 - 1
            With 步骤(I)
                If String.IsNullOrEmpty(.输入值) AndAlso .是空字符串 = False Then Exit For
            End With
        Next
        If I < 步骤数量 Then
            Select Case 步骤(I).步骤代码
                Case 任务步骤_常量集合.密码, 任务步骤_常量集合.重复密码, 任务步骤_常量集合.当前密码
                    输入框.PasswordChar = "●"c
                    输入框.BackColor = Color.Pink
                Case Else
                    输入框.PasswordChar = vbNullChar
                    输入框.BackColor = Color.White
            End Select
            If 步骤(I).步骤代码 = 任务步骤_常量集合.验证码 Then
                If 需要获取验证码图片 = True Then
                    Dim 域名 As String
                    Select Case 名称
                        Case 任务名称_注册
                            域名 = 获取某步骤的输入值(任务步骤_常量集合.域名)
                        Case Else
                            Dim 讯宝地址 As String = 获取某步骤的输入值(任务步骤_常量集合.讯宝地址)
                            Dim 段() As String = 讯宝地址.Split(讯宝地址标识)
                            域名 = 段(1)
                    End Select
                    机器人.启动HTTPS访问线程(New 类_访问设置(获取中心服务器访问路径开头(域名) & "C=GetVCodePicture"))
                    Return 界面文字.获取(36, "正在获取验证码图片。")
                End If
            End If
            Return 步骤(I).提示语
        Else
            Return Nothing
        End If
    End Function

    Friend Function 保存当前步骤输入值(ByVal 文本 As String) As String
        Dim I As Integer
        For I = 0 To 步骤数量 - 1
            If String.IsNullOrEmpty(步骤(I).输入值) Then Exit For
        Next
        If I < 步骤数量 Then
            With 步骤(I)
                Select Case .步骤代码
                    Case 任务步骤_常量集合.添加讯友
                        If 文本.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯宝和电子邮箱地址长度})
                        End If
                        If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        End If
                        If String.Compare(文本, 当前用户.英语讯宝地址, True) = 0 OrElse String.Compare(文本, 当前用户.本国语讯宝地址) = 0 Then
                            Return 界面文字.获取(106, "这是你自己的讯宝地址。")
                        End If
                        If 当前用户.查找讯友(文本) IsNot Nothing Then
                            Return 界面文字.获取(105, "此讯友已添加。请输入一个未添加的讯宝地址。")
                        End If
                    Case 任务步骤_常量集合.删除讯友
                        Dim 某一讯友 As 类_讯友
                        If IsNumeric(文本) Then
                            Dim 编号 As Short
                            If Short.TryParse(文本, 编号) = False Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 当前用户.讯友目录 Is Nothing Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 编号 < 1 OrElse 编号 > 当前用户.讯友目录.Length Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            某一讯友 = 当前用户.讯友目录(编号 - 1)
                        Else
                            某一讯友 = 当前用户.查找讯友(文本)
                            If 某一讯友 Is Nothing Then
                                Return 界面文字.获取(120, "在你的讯友录中不存在。")
                            End If
                        End If
                        If 当前用户.加入的小聊天群 IsNot Nothing Then
                            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                            For I = 0 To 加入的小聊天群.Length - 1
                                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 某一讯友.英语讯宝地址) = 0 Then
                                    Return 界面文字.获取(177, "你加入了此讯友创建的聊天群。")
                                End If
                            Next
                        End If
                        文本 = 某一讯友.英语讯宝地址
                    Case 任务步骤_常量集合.添加讯友备注
                        If 文本.Length > 最大值_常量集合.讯友备注字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯友备注字符数})
                        End If
                    Case 任务步骤_常量集合.添加现有标签, 任务步骤_常量集合.添加新标签
                        If 文本.Length > 最大值_常量集合.讯友标签字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯友标签字符数})
                        End If
                        With 机器人.聊天控件.聊天对象.讯友或群主
                            If String.Compare(.标签一, 文本, True) = 0 OrElse String.Compare(.标签二, 文本, True) = 0 Then
                                Return 界面文字.获取(132, "此标签已添加过了。请输入一个新的标签名称。")
                            End If
                        End With
                        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                        For I = 0 To 讯友目录.Length - 1
                            With 讯友目录(I)
                                If String.Compare(.标签一, 文本, True) = 0 OrElse String.Compare(.标签二, 文本, True) = 0 Then
                                    Exit For
                                End If
                            End With
                        Next
                        If .步骤代码 = 任务步骤_常量集合.添加现有标签 Then
                            If I = 讯友目录.Length Then
                                Return 界面文字.获取(139, "这不是现有的标签名称。")
                            End If
                        Else
                            If I < 讯友目录.Length Then
                                Return 界面文字.获取(136, "这不是新标签名称。")
                            End If
                        End If
                    Case 任务步骤_常量集合.移除标签
                        With 机器人.聊天控件.聊天对象.讯友或群主
                            If String.Compare(.标签一, 文本, True) <> 0 AndAlso String.Compare(.标签二, 文本, True) <> 0 Then
                                Return 界面文字.获取(147, "请输入目前的标签名称。")
                            End If
                        End With
                    Case 任务步骤_常量集合.原标签名称
                        Dim 讯友目录() As 类_讯友 = 当前用户.讯友目录
                        For I = 0 To 讯友目录.Length - 1
                            With 讯友目录(I)
                                If String.Compare(.标签一, 文本, True) = 0 Then
                                    Exit For
                                ElseIf String.Compare(.标签二, 文本, True) = 0 Then
                                    Exit For
                                End If
                            End With
                        Next
                        If I = 讯友目录.Length Then
                            Return 界面文字.获取(146, "请选择已有的标签名称。")
                        End If
                    Case 任务步骤_常量集合.新标签名称
                        If 文本.Length > 最大值_常量集合.讯友标签字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯友标签字符数})
                        End If
                    Case 任务步骤_常量集合.修改讯友备注
                        If 文本.Length > 最大值_常量集合.讯友备注字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯友备注字符数})
                        End If
                        If String.Compare(机器人.聊天控件.聊天对象.讯友或群主.备注, 文本) = 0 Then
                            Return 界面文字.获取(155, "新备注与当前的备注没有任何差异。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.取消拉黑讯友
                        Dim 某一讯友 As 类_讯友
                        If IsNumeric(文本) Then
                            Dim 编号 As Short
                            If Short.TryParse(文本, 编号) = False Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 当前用户.讯友目录 Is Nothing Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 编号 < 1 OrElse 编号 > 当前用户.讯友目录.Length Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            某一讯友 = 当前用户.讯友目录(编号 - 1)
                        Else
                            某一讯友 = 当前用户.查找讯友(文本)
                            If 某一讯友 Is Nothing Then
                                Return 界面文字.获取(120, "在你的讯友录中不存在。")
                            End If
                        End If
                        If 某一讯友.拉黑 = False Then
                            Return 界面文字.获取(121, "你未将此讯友拉黑。")
                        Else
                            文本 = 某一讯友.英语讯宝地址
                        End If
                    Case 任务步骤_常量集合.添加黑域
                        Dim 可选域名() As 域名_复合数据 = Nothing
                        Dim 可选域名数 As Integer
                        CType(机器人, 类_机器人_主控).添加黑域时统计可选域名(可选域名, 可选域名数)
                        For I = 0 To 可选域名数 - 1
                            If String.Compare(文本, 可选域名(I).英语) = 0 Then
                                Exit For
                            End If
                        Next
                        If I = 可选域名数 Then
                            Return 界面文字.获取(241, "请从上面列出的域名中选择。")
                        End If
                    Case 任务步骤_常量集合.添加白域
                        Dim 可选域名() As 域名_复合数据 = Nothing
                        Dim 可选域名数 As Integer
                        CType(机器人, 类_机器人_主控).添加白域时统计可选域名(可选域名, 可选域名数)
                        For I = 0 To 可选域名数 - 1
                            If String.Compare(文本, 可选域名(I).英语) = 0 Then
                                Exit For
                            End If
                        Next
                        If I = 可选域名数 Then
                            Return 界面文字.获取(241, "请从上面列出的域名中选择。")
                        End If
                    Case 任务步骤_常量集合.小聊天群名称
                        If 文本.Length > 最大值_常量集合.群名称字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.群名称字符数})
                        End If
                        If 当前用户.加入的小聊天群 IsNot Nothing Then
                            Dim 加入的小聊天群() As 类_聊天群_小 = 当前用户.加入的小聊天群
                            For I = 0 To 加入的小聊天群.Length - 1
                                If String.Compare(加入的小聊天群(I).群主.英语讯宝地址, 当前用户.英语讯宝地址) = 0 Then
                                    If String.Compare(加入的小聊天群(I).备注, 文本) = 0 Then
                                        Return 界面文字.获取(184, "此名称已用于其它聊天群。")
                                    End If
                                End If
                            Next
                        End If
                    Case 任务步骤_常量集合.大聊天群名称
                        If 文本.Length > 最大值_常量集合.群名称字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.群名称字符数})
                        End If
                    Case 任务步骤_常量集合.大聊天群估计成员数
                        Dim 成员数 As Integer
                        If Integer.TryParse(文本, 成员数) = False Then
跳转点1:
                            Return 界面文字.获取(298, "请输入一个正整数。")
                        ElseIf 成员数 <= 最大值_常量集合.小聊天群成员数量 Then
                            GoTo 跳转点1
                        End If
                        文本 = 成员数
                    Case 任务步骤_常量集合.小聊天群邀请, 任务步骤_常量集合.大聊天群邀请
                        Dim 某一讯友 As 类_讯友
                        If IsNumeric(文本) Then
                            Dim 编号 As Short
                            If Short.TryParse(文本, 编号) = False Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 当前用户.讯友目录 Is Nothing Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            If 编号 < 1 OrElse 编号 > 当前用户.讯友目录.Length Then
                                Return 界面文字.获取(84, "请输入正确的编号。")
                            End If
                            某一讯友 = 当前用户.讯友目录(编号 - 1)
                        Else
                            某一讯友 = 当前用户.查找讯友(文本)
                            If 某一讯友 Is Nothing Then
                                Return 界面文字.获取(120, "在你的讯友录中不存在。")
                            End If
                        End If
                        If 某一讯友.拉黑 Then
                            Return 界面文字.获取(160, "你已将此讯友拉黑。")
                        End If
                        文本 = 某一讯友.英语讯宝地址
                        If .步骤代码 = 任务步骤_常量集合.小聊天群邀请 Then
                            Dim 群成员() As 类_群成员 = 机器人.聊天控件.聊天对象.小聊天群.群成员
                            For I = 0 To 群成员.Length - 1
                                If String.Compare(群成员(I).英语讯宝地址, 文本) = 0 Then
                                    Select Case 群成员(I).角色
                                        Case 群角色_常量集合.邀请加入_可以发言, 群角色_常量集合.邀请加入_不可发言
                                            Return 界面文字.获取(191, "已给此讯友发送了邀请。要想再次发送，请先[#%]。", New Object() {界面文字.获取(组名_任务, 12, "删减成员")})
                                        Case Else
                                            Return 界面文字.获取(192, "此讯友已加入本群。")
                                    End Select
                                End If
                            Next
                        End If
                    Case 任务步骤_常量集合.小聊天群删减成员
                        Dim 群成员() As 类_群成员 = 机器人.聊天控件.聊天对象.小聊天群.群成员
                        For I = 0 To 群成员.Length - 1
                            If String.Compare(群成员(I).英语讯宝地址, 文本) = 0 OrElse String.Compare(群成员(I).本国语讯宝地址, 文本) = 0 Then
                                Exit For
                            End If
                        Next
                        If I < 群成员.Length Then
                            文本 = 群成员(I).英语讯宝地址
                        Else
                            Return 界面文字.获取(290, "在群成员列表中找不到此讯宝地址。")
                        End If
                    Case 任务步骤_常量集合.大聊天群昵称
                        If 文本.Length > 最大值_常量集合.讯友备注字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯友备注字符数})
                        End If
                    Case 任务步骤_常量集合.大聊天群某成员的新角色
                        Dim 角色 As 群角色_常量集合
                        If Byte.TryParse(文本, 角色) = False Then
                            Return 界面文字.获取(292, "请输入括号内的数字。")
                        End If
                        Select Case 角色
                            Case 群角色_常量集合.成员_不可发言, 群角色_常量集合.成员_可以发言, 群角色_常量集合.管理员
                            Case Else
                                Return 界面文字.获取(292, "请输入括号内的数字。")
                        End Select
                    Case 任务步骤_常量集合.讯宝地址
                        If 文本.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯宝和电子邮箱地址长度})
                        End If
                        If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.密码, 任务步骤_常量集合.当前密码
                        If 文本.Length < 最小值_常量集合.密码长度 Then
                            Return 界面文字.获取(10, "密码最少要有#%个字符。请重新输入。", New Object() {最小值_常量集合.密码长度})
                        ElseIf 文本.Length > 最大值_常量集合.密码长度 Then
                            Return 界面文字.获取(32, "密码最多只能有#%个字符。请重新输入。", New Object() {最大值_常量集合.密码长度})
                        Else
                            输入框.PasswordChar = vbNullChar
                            输入框.BackColor = Color.White
                        End If
                    Case 任务步骤_常量集合.验证码
                        If 文本.Length <> 长度_常量集合.验证码 Then
                            Return 界面文字.获取(11, "验证码有#%个字符。请重新输入。", New Object() {长度_常量集合.验证码})
                        End If
                    Case 任务步骤_常量集合.域名
                        If 文本.Contains(" ") Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        End If
                        Dim 段() As String = 文本.Split(".")
                        If 段.Length < 2 OrElse 段.Length > 3 Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        End If
                        For I = 0 To 段.Length - 1
                            If String.IsNullOrEmpty(段(I)) Then
                                Return 界面文字.获取(8, "格式不正确。请重新输入。")
                            End If
                        Next
                    Case 任务步骤_常量集合.手机号或电子邮箱地址
                        If 文本.Contains("@") Then
                            If 文本.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then
                                Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯宝和电子邮箱地址长度})
                            End If
                            If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                                Return 界面文字.获取(8, "格式不正确。请重新输入。")
                            End If
                            身份码类型 = 身份码类型_电子邮箱地址
                        Else
                            If 文本.Length > 最大值_常量集合.手机号字符数 Then
                                Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.手机号字符数})
                            End If
                            If IsNumeric(文本) = False Then
                                Return 界面文字.获取(33, "这不是手机号码。请重新输入。")
                            End If
                            身份码类型 = 身份码类型_手机号
                        End If
                    Case 任务步骤_常量集合.重复密码
                        If String.Compare(获取某步骤的输入值(任务步骤_常量集合.密码), 文本) <> 0 Then
                            Return 界面文字.获取(34, "与刚才输入的密码不一致。请重新输入。")
                        Else
                            输入框.PasswordChar = vbNullChar
                            输入框.BackColor = Color.White
                        End If
                    Case 任务步骤_常量集合.手机号
                        If 文本.Length > 最大值_常量集合.手机号字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.手机号字符数})
                        End If
                        If IsNumeric(文本) = False Then
                            Return 界面文字.获取(33, "这不是手机号码。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.电子邮箱地址
                        If 文本.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯宝和电子邮箱地址长度})
                        End If
                        If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.英语用户名
                        If 文本.Length < 最小值_常量集合.英语用户名长度 Then
                            Return 界面文字.获取(161, "英语用户名的长度不能少于#%个字符。请重新输入。", New Object() {最小值_常量集合.英语用户名长度})
                        End If
                        If 文本.Length > 最大值_常量集合.英语用户名长度 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.英语用户名长度})
                        End If
                        If 是否是英文用户名(文本) = False Then
                            Return 界面文字.获取(9, "此用户名格式不正确。请重新输入。")
                        End If
                        If String.IsNullOrEmpty(当前用户.电子邮箱地址) = False Then
                            If 当前用户.电子邮箱地址.EndsWith(讯宝地址标识 & 当前用户.域名_英语) Then
                                If 当前用户.电子邮箱地址.StartsWith(文本 & 讯宝地址标识) = False Then
                                    Return 界面文字.获取(303, "请让你的英语讯宝地址与你的电子邮箱地址一致。")
                                End If
                            End If
                        End If
                        机器人.说(界面文字.获取(65, "你的英语讯宝地址将为 #% 。", New Object() {文本 & 讯宝地址标识 & 当前用户.域名_英语}))
                    Case 任务步骤_常量集合.重复英语用户名
                        If String.Compare(获取某步骤的输入值(任务步骤_常量集合.英语用户名), 文本) <> 0 Then
                            Return 界面文字.获取(64, "与刚才输入的用户名不一致。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.本国语用户名
                        If 文本.Length < 最小值_常量集合.本国语用户名长度 Then
                            Return 界面文字.获取(162, "中文用户名的长度不能少于#%个字符。请重新输入。", New Object() {最小值_常量集合.本国语用户名长度})
                        End If
                        If 文本.Length > 最大值_常量集合.本国语用户名长度 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.本国语用户名长度})
                        End If
                        If 是否是中文用户名(文本) = False Then
                            Return 界面文字.获取(9, "此用户名格式不正确。请重新输入。")
                        End If
                        If String.IsNullOrEmpty(当前用户.电子邮箱地址) = False Then
                            If 当前用户.电子邮箱地址.EndsWith(讯宝地址标识 & 当前用户.域名_本国语) Then
                                If 当前用户.电子邮箱地址.StartsWith(文本 & 讯宝地址标识) = False Then
                                    Return 界面文字.获取(304, "请让你的本国语讯宝地址与你的电子邮箱地址一致。")
                                End If
                            End If
                        End If
                        机器人.说(界面文字.获取(66, "你的汉语讯宝地址将为 #% 。", New Object() {文本 & 讯宝地址标识 & 当前用户.域名_本国语}))
                    Case 任务步骤_常量集合.重复本国语用户名
                        If String.Compare(获取某步骤的输入值(任务步骤_常量集合.本国语用户名), 文本) <> 0 Then
                            Return 界面文字.获取(64, "与刚才输入的用户名不一致。请重新输入。")
                        End If
                    Case 任务步骤_常量集合.添加移除可注册者
                        If String.Compare(文本, "*") <> 0 Then
                            If 文本.Contains("@") Then
                                If 文本.Length > 最大值_常量集合.讯宝和电子邮箱地址长度 Then
                                    Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.讯宝和电子邮箱地址长度})
                                End If
                                If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                                    Return 界面文字.获取(8, "格式不正确。请重新输入。")
                                End If
                            Else
                                If 文本.Length > 最大值_常量集合.手机号字符数 Then
                                    Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.手机号字符数})
                                End If
                                If IsNumeric(文本) = False Then
                                    Return 界面文字.获取(33, "这不是手机号码。请重新输入。")
                                End If
                            End If
                        End If
                    Case 任务步骤_常量集合.设置商品编辑者
                        If 是否是有效的讯宝或电子邮箱地址(文本) = False Then
                            Return 界面文字.获取(8, "格式不正确。请重新输入。")
                        Else
                            Dim 段() As String = 文本.Split(New Char() {"@"c, "."})
                            For I = 0 To 段.Length - 1
                                If 是否是英文用户名(段(I)) = False Then Return 界面文字.获取(8, "格式不正确。请重新输入。")
                            Next
                        End If
                    Case 任务步骤_常量集合.传送服务器主机名, 任务步骤_常量集合.大聊天群服务器主机名
                        If 文本.Length > 最大值_常量集合.主机名字符数 Then
                            Return 界面文字.获取(3, "长度不能超过#%个字符。请重新输入。", New Object() {最大值_常量集合.主机名字符数})
                        End If
                        Select Case .步骤代码
                            Case 任务步骤_常量集合.传送服务器主机名
                                If 文本.StartsWith(讯宝中心服务器主机名) = False Then
                                    Return 界面文字.获取(165, "主机名要以#%开头。请重新输入。", New Object() {讯宝中心服务器主机名})
                                End If
                            Case 任务步骤_常量集合.大聊天群服务器主机名
                                If 文本.StartsWith(讯宝大聊天群服务器主机名前缀) = False Then
                                    Return 界面文字.获取(165, "主机名要以#%开头。请重新输入。", New Object() {讯宝大聊天群服务器主机名前缀})
                                End If
                        End Select
                    Case 任务步骤_常量集合.服务器网络地址
                        If String.Compare(文本, "0.0.0.0") = 0 Then
                            Return 界面文字.获取(166, "这不是符合规则的IP地址。请重新输入。")
                        End If
                        Dim 网络地址 As New IPAddress(0)
                        If IPAddress.TryParse(文本, 网络地址) = False Then
                            Return 界面文字.获取(166, "这不是符合规则的IP地址。请重新输入。")
                        End If
                End Select
                .输入值 = 文本
            End With
        End If
        Return Nothing
    End Function

    Friend Function 获取某步骤的输入值(ByVal 步骤代码 As 任务步骤_常量集合) As String
        Dim I As Integer
        For I = 0 To 步骤数量 - 1
            If 步骤(I).步骤代码 = 步骤代码 Then
                Return 步骤(I).输入值
            End If
        Next
        Return Nothing
    End Function

    Friend Sub 清除某步骤的输入值(ByVal 步骤代码 As 任务步骤_常量集合)
        If 步骤数量 > 0 Then
            Dim I As Integer
            For I = 0 To 步骤数量 - 1
                If 步骤(I).步骤代码 = 步骤代码 Then
                    With 步骤(I)
                        .输入值 = Nothing
                        .是空字符串 = False
                    End With
                    Return
                End If
            Next
        End If
    End Sub

    Friend Sub 清除所有步骤的输入值()
        If 步骤数量 > 0 Then
            Dim I As Integer
            For I = 0 To 步骤数量 - 1
                With 步骤(I)
                    .输入值 = Nothing
                    .是空字符串 = False
                End With
            Next
        End If
    End Sub

    Friend Function 生成访问设置() As 类_访问设置
        Dim 访问路径 As String = ""
        Dim 收发时限2 As Integer = 收发时限
        机器人.说(界面文字.获取(7, "请稍等。"))
        Select Case 名称
            Case 任务名称_创建大聊天群
                收发时限2 = 20000
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=CreateGroup&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&Name=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.大聊天群名称)) & "&Number=" & 获取某步骤的输入值(任务步骤_常量集合.大聊天群估计成员数)
            Case 任务名称_密码
                收发时限2 = 20000
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=ChangePassword&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&NewPassword=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.密码)) & "&CurrentPassword=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.当前密码))
            Case 任务名称_手机号
                Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=NewPhoneNumber&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&PhoneNumber=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.手机号)) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.当前密码)) & "&TimezoneOffset=" & 时区偏移量 & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_邮箱地址
                Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=NewEmailAddress&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&EmailAddress=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.电子邮箱地址)) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.当前密码)) & "&TimezoneOffset=" & 时区偏移量 & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_验证手机号
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=VerifyNewPhoneNumber&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&PhoneNumber=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.手机号)) & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_验证邮箱地址
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=VerifyNewEmailAddress&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&EmailAddress=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.电子邮箱地址)) & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_登录
                Dim 讯宝地址 As String = 获取某步骤的输入值(任务步骤_常量集合.讯宝地址)
                Dim 段() As String = 讯宝地址.Split(讯宝地址标识)
                Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
                访问路径 = 获取中心服务器访问路径开头(段(1)) & "C=Login&SSAddress=" & 替换URI敏感字符(讯宝地址) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.密码)) & "&DeviceType=" & 设备类型_电脑 & "&TimezoneOffset=" & 时区偏移量
                Dim 验证码 As String = 获取某步骤的输入值(任务步骤_常量集合.验证码)
                If String.IsNullOrEmpty(验证码) = False Then
                    访问路径 &= "&VerificationCode=" & 替换URI敏感字符(验证码) & "&VCodeCreatedOn=" & 验证码添加时间
                End If
            Case 任务名称_注册
                Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
                访问路径 = 获取中心服务器访问路径开头(获取某步骤的输入值(任务步骤_常量集合.域名)) & "C=Register&IDtype=" & 身份码类型 & "&ID=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.手机号或电子邮箱地址)) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.密码)) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&TimezoneOffset=" & 时区偏移量 & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_验证
                访问路径 = 获取中心服务器访问路径开头(获取某步骤的输入值(任务步骤_常量集合.域名)) & "C=VerifyPhoneOrEmail&IDtype=" & 身份码类型 & "&ID=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.手机号或电子邮箱地址)) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码))
            Case 任务名称_用户名
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=SetAccountName&IDtype=" & 身份码类型 & "&ID=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.手机号或电子邮箱地址)) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.密码)) & "&English=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.英语用户名)) & "&Native=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.本国语用户名))
            Case 任务名称_忘记
                Dim 讯宝地址 As String = 获取某步骤的输入值(任务步骤_常量集合.讯宝地址)
                Dim 段() As String = 讯宝地址.Split(讯宝地址标识)
                Dim 时区偏移量 As Integer = DateDiff(DateInterval.Minute, Date.UtcNow, Date.Now)
                访问路径 = 获取中心服务器访问路径开头(段(1)) & "C=ForgotPassword&SSAddress=" & 替换URI敏感字符(讯宝地址) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&TimezoneOffset=" & 时区偏移量 & "&LanguageCode=" & My.Application.Culture.ThreeLetterISOLanguageName
            Case 任务名称_重设
                Dim 讯宝地址 As String = 获取某步骤的输入值(任务步骤_常量集合.讯宝地址)
                Dim 段() As String = 讯宝地址.Split(讯宝地址标识)
                收发时限2 = 20000
                访问路径 = 获取中心服务器访问路径开头(段(1)) & "C=ResetPassword&SSAddress=" & 替换URI敏感字符(讯宝地址) & "&VCodeCreatedOn=" & 验证码添加时间 & "&VerificationCode=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.验证码)) & "&Password=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.密码))
            Case 任务名称_添加可注册者
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AuthorizeRegister&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&ID=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.添加移除可注册者)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
            Case 任务名称_移除可注册者
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=UnauthorizeRegister&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&ID=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.添加移除可注册者)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
            Case 任务名称_商品编辑者
                收发时限2 = 20000
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=SetGoodsEditor&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&EnglishSSAddress=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.设置商品编辑者)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
            Case 任务名称_新传送服务器
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AddServer&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&Name=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.传送服务器主机名)) & "&IP=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
            Case 任务名称_新大聊天群服务器
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AddServer&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&Name=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.大聊天群服务器主机名)) & "&IP=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
            Case 任务名称_小宇宙中心服务器
                访问路径 = 获取中心服务器访问路径开头(当前用户.域名_英语) & "C=AddServer&UserID=" & 当前用户.编号 & "&Credential=" & 替换URI敏感字符(当前用户.凭据_中心服务器) & "&Name=" & 替换URI敏感字符(讯宝小宇宙中心服务器主机名) & "&IP=" & 替换URI敏感字符(获取某步骤的输入值(任务步骤_常量集合.服务器网络地址)) & "&Passcode=" & 替换URI敏感字符(当前用户.凭据_管理员)
        End Select
        Return New 类_访问设置(访问路径, 收发时限2)
    End Function

    Friend Sub 结束()
        步骤数量 = 0
        步骤 = Nothing
        机器人.提示新任务()
        If 显示讯友临时编号 = True Then
            显示讯友临时编号 = False
            机器人.主窗体1.刷新讯友录()
        End If
    End Sub

End Class
