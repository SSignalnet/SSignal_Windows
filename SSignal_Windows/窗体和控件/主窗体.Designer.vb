<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class 主窗体
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.任务栏小图标 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.文件选取器 = New System.Windows.Forms.OpenFileDialog()
        Me.定时器_心跳 = New System.Windows.Forms.Timer(Me.components)
        Me.文件夹浏览器 = New System.Windows.Forms.FolderBrowserDialog()
        Me.讯友录的容器 = New System.Windows.Forms.Panel()
        Me.聊天控件的容器 = New System.Windows.Forms.Panel()
        Me.文件保存对话框 = New System.Windows.Forms.SaveFileDialog()
        Me.定时器_等待确认 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        '任务栏小图标
        '
        Me.任务栏小图标.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.任务栏小图标.Text = "SSignal"
        '
        '定时器_心跳
        '
        Me.定时器_心跳.Interval = 60000
        '
        '文件夹浏览器
        '
        Me.文件夹浏览器.RootFolder = System.Environment.SpecialFolder.MyComputer
        '
        '讯友录的容器
        '
        Me.讯友录的容器.Dock = System.Windows.Forms.DockStyle.Left
        Me.讯友录的容器.Location = New System.Drawing.Point(0, 0)
        Me.讯友录的容器.Margin = New System.Windows.Forms.Padding(0)
        Me.讯友录的容器.Name = "讯友录的容器"
        Me.讯友录的容器.Size = New System.Drawing.Size(400, 962)
        Me.讯友录的容器.TabIndex = 0
        '
        '聊天控件的容器
        '
        Me.聊天控件的容器.Dock = System.Windows.Forms.DockStyle.Fill
        Me.聊天控件的容器.Location = New System.Drawing.Point(400, 0)
        Me.聊天控件的容器.Margin = New System.Windows.Forms.Padding(0)
        Me.聊天控件的容器.Name = "聊天控件的容器"
        Me.聊天控件的容器.Size = New System.Drawing.Size(1278, 962)
        Me.聊天控件的容器.TabIndex = 1
        '
        '定时器_等待确认
        '
        Me.定时器_等待确认.Interval = 20000
        '
        '主窗体
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(1678, 962)
        Me.Controls.Add(Me.聊天控件的容器)
        Me.Controls.Add(Me.讯友录的容器)
        Me.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.KeyPreview = True
        Me.MinimumSize = New System.Drawing.Size(1700, 1018)
        Me.Name = "主窗体"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "讯宝"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents 任务栏小图标 As System.Windows.Forms.NotifyIcon
    Friend WithEvents 文件选取器 As OpenFileDialog
    Friend WithEvents 定时器_心跳 As Timer
    Friend WithEvents 文件夹浏览器 As FolderBrowserDialog
    Friend WithEvents 讯友录的容器 As Panel
    Friend WithEvents 聊天控件的容器 As Panel
    Friend WithEvents 文件保存对话框 As SaveFileDialog
    Friend WithEvents 定时器_等待确认 As Timer
End Class
