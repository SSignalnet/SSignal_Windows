<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class 控件_聊天
    Inherits System.Windows.Forms.UserControl

    'UserControl 重写释放以清理组件列表。
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
        Me.按钮_说话 = New System.Windows.Forms.Button()
        Me.输入框 = New System.Windows.Forms.TextBox()
        Me.自动布局容器 = New System.Windows.Forms.FlowLayoutPanel()
        Me.文字_字数 = New System.Windows.Forms.Label()
        Me.下拉列表_任务 = New System.Windows.Forms.ComboBox()
        Me.定时器_机器人回答 = New System.Windows.Forms.Timer(Me.components)
        Me.定时器_录音 = New System.Windows.Forms.Timer(Me.components)
        Me.聊天内容的容器 = New System.Windows.Forms.Panel()
        Me.小宇宙的容器 = New System.Windows.Forms.Panel()
        Me.说话对象的容器 = New System.Windows.Forms.FlowLayoutPanel()
        Me.自动布局容器.SuspendLayout()
        Me.SuspendLayout()
        '
        '按钮_说话
        '
        Me.按钮_说话.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.按钮_说话.Enabled = False
        Me.按钮_说话.Location = New System.Drawing.Point(500, 571)
        Me.按钮_说话.Margin = New System.Windows.Forms.Padding(0)
        Me.按钮_说话.Name = "按钮_说话"
        Me.按钮_说话.Size = New System.Drawing.Size(100, 105)
        Me.按钮_说话.TabIndex = 1
        Me.按钮_说话.Text = "说话"
        Me.按钮_说话.UseVisualStyleBackColor = True
        '
        '输入框
        '
        Me.输入框.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.输入框.BackColor = System.Drawing.Color.White
        Me.输入框.Font = New System.Drawing.Font("微软雅黑", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.输入框.Location = New System.Drawing.Point(0, 571)
        Me.输入框.Margin = New System.Windows.Forms.Padding(0)
        Me.输入框.Multiline = True
        Me.输入框.Name = "输入框"
        Me.输入框.Size = New System.Drawing.Size(500, 70)
        Me.输入框.TabIndex = 0
        '
        '自动布局容器
        '
        Me.自动布局容器.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.自动布局容器.Controls.Add(Me.文字_字数)
        Me.自动布局容器.Location = New System.Drawing.Point(0, 644)
        Me.自动布局容器.Margin = New System.Windows.Forms.Padding(0)
        Me.自动布局容器.Name = "自动布局容器"
        Me.自动布局容器.Size = New System.Drawing.Size(294, 32)
        Me.自动布局容器.TabIndex = 4
        '
        '文字_字数
        '
        Me.文字_字数.AutoSize = True
        Me.文字_字数.Location = New System.Drawing.Point(3, 3)
        Me.文字_字数.Margin = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Me.文字_字数.Name = "文字_字数"
        Me.文字_字数.Size = New System.Drawing.Size(21, 24)
        Me.文字_字数.TabIndex = 5
        Me.文字_字数.Text = "0"
        '
        '下拉列表_任务
        '
        Me.下拉列表_任务.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.下拉列表_任务.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.下拉列表_任务.FormattingEnabled = True
        Me.下拉列表_任务.Location = New System.Drawing.Point(297, 644)
        Me.下拉列表_任务.Name = "下拉列表_任务"
        Me.下拉列表_任务.Size = New System.Drawing.Size(200, 32)
        Me.下拉列表_任务.TabIndex = 2
        '
        '定时器_机器人回答
        '
        Me.定时器_机器人回答.Interval = 600
        '
        '定时器_录音
        '
        '
        '聊天内容的容器
        '
        Me.聊天内容的容器.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.聊天内容的容器.Location = New System.Drawing.Point(0, 0)
        Me.聊天内容的容器.Margin = New System.Windows.Forms.Padding(0)
        Me.聊天内容的容器.Name = "聊天内容的容器"
        Me.聊天内容的容器.Size = New System.Drawing.Size(600, 571)
        Me.聊天内容的容器.TabIndex = 3
        '
        '小宇宙的容器
        '
        Me.小宇宙的容器.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.小宇宙的容器.Location = New System.Drawing.Point(600, 0)
        Me.小宇宙的容器.Margin = New System.Windows.Forms.Padding(0)
        Me.小宇宙的容器.Name = "小宇宙的容器"
        Me.小宇宙的容器.Size = New System.Drawing.Size(613, 676)
        Me.小宇宙的容器.TabIndex = 7
        '
        '说话对象的容器
        '
        Me.说话对象的容器.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.说话对象的容器.AutoSize = True
        Me.说话对象的容器.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.说话对象的容器.Location = New System.Drawing.Point(0, 571)
        Me.说话对象的容器.Margin = New System.Windows.Forms.Padding(0)
        Me.说话对象的容器.Name = "说话对象的容器"
        Me.说话对象的容器.Size = New System.Drawing.Size(600, 0)
        Me.说话对象的容器.TabIndex = 6
        '
        '控件_聊天
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.Controls.Add(Me.小宇宙的容器)
        Me.Controls.Add(Me.输入框)
        Me.Controls.Add(Me.按钮_说话)
        Me.Controls.Add(Me.自动布局容器)
        Me.Controls.Add(Me.下拉列表_任务)
        Me.Controls.Add(Me.聊天内容的容器)
        Me.Controls.Add(Me.说话对象的容器)
        Me.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Name = "控件_聊天"
        Me.Size = New System.Drawing.Size(1213, 676)
        Me.自动布局容器.ResumeLayout(False)
        Me.自动布局容器.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents 按钮_说话 As Button
    Friend WithEvents 输入框 As TextBox
    Friend WithEvents 自动布局容器 As FlowLayoutPanel
    Friend WithEvents 文字_字数 As Label
    Friend WithEvents 下拉列表_任务 As ComboBox
    Friend WithEvents 定时器_机器人回答 As Timer
    Friend WithEvents 定时器_录音 As Timer
    Friend WithEvents 聊天内容的容器 As Panel
    Friend WithEvents 小宇宙的容器 As Panel
    Friend WithEvents 说话对象的容器 As FlowLayoutPanel
End Class
