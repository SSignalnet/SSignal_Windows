Imports System.Net.Sockets
Imports SSignalDB

Module 模块_定义和声明

    Friend Const 机器人id_系统管理 As String = "r0"
    Friend Const 机器人id_主控 As String = "r1"

    Friend 界面文字 As 类_界面文字

    Friend 组名_一般 As String = "一般"
    Friend 组名_任务 As String = "任务"

    Friend 当前用户 As 类_用户
    Friend 网络连接器 As Socket
    Friend 显示讯友临时编号 As Boolean
    Friend 副数据库 As 类_数据库
    Friend 备份管理器 As 类_备份管理器
    Friend 备份文件存放路径, 备份异常信息 As String

    Friend 开启了媒体管理器 As Boolean

    Friend Enum 任务步骤_常量集合 As Byte
        讯宝地址 = 1
        密码 = 2
        验证码 = 3
        手机号或电子邮箱地址 = 4
        重复密码 = 5
        英语用户名 = 6
        重复英语用户名 = 7
        本国语用户名 = 8
        重复本国语用户名 = 9
        添加讯友 = 10
        添加讯友备注 = 11
        删除讯友 = 12
        取消拉黑讯友 = 13
        添加新标签 = 14
        添加现有标签 = 15
        移除标签 = 16
        原标签名称 = 17
        新标签名称 = 18
        修改讯友备注 = 19
        传送服务器主机名 = 20
        服务器网络地址 = 21
        小聊天群名称 = 22
        小聊天群邀请 = 23
        小聊天群删减成员 = 24
        当前密码 = 25
        手机号 = 26
        电子邮箱地址 = 27
        添加黑域 = 28
        添加白域 = 29
        移除黑白域 = 30
        大聊天群名称 = 31
        大聊天群估计成员数 = 32
        大聊天群邀请 = 33
        大聊天群删减成员 = 34
        大聊天群服务器主机名 = 35
        大聊天群修改角色 = 36
        大聊天群某成员的新角色 = 37
        大聊天群昵称 = 38
        添加移除可注册者 = 39
        域名 = 40
        设置商品编辑者 = 41
    End Enum

    Friend Enum 讯友录显示范围_常量集合 As Byte
        未指定 = 0
        讯友 = 1
        最近 = 2
        聊天群 = 3
        某标签 = 4
        黑名单 = 5
        黑域 = 6
        白域 = 7
    End Enum

    Friend Structure 有新讯宝的群_复合数据
        Dim 编号, 时间, 撤回的讯宝() As Long
        Dim 新讯宝数量, 撤回的讯宝数量 As Integer
    End Structure

End Module
