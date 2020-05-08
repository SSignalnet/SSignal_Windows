<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class 窗体_播放视频
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(窗体_播放视频))
        Me.媒体播放器 = New AxWMPLib.AxWindowsMediaPlayer()
        Me.文字_关闭 = New System.Windows.Forms.Label()
        CType(Me.媒体播放器, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        '媒体播放器
        '
        Me.媒体播放器.Dock = System.Windows.Forms.DockStyle.Fill
        Me.媒体播放器.Enabled = True
        Me.媒体播放器.Location = New System.Drawing.Point(0, 0)
        Me.媒体播放器.Name = "媒体播放器"
        Me.媒体播放器.OcxState = CType(resources.GetObject("媒体播放器.OcxState"), System.Windows.Forms.AxHost.State)
        Me.媒体播放器.Size = New System.Drawing.Size(560, 450)
        Me.媒体播放器.TabIndex = 0
        '
        '文字_关闭
        '
        Me.文字_关闭.BackColor = System.Drawing.Color.Black
        Me.文字_关闭.Font = New System.Drawing.Font("微软雅黑", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.文字_关闭.ForeColor = System.Drawing.Color.Gray
        Me.文字_关闭.Location = New System.Drawing.Point(12, 9)
        Me.文字_关闭.Name = "文字_关闭"
        Me.文字_关闭.Size = New System.Drawing.Size(154, 47)
        Me.文字_关闭.TabIndex = 1
        Me.文字_关闭.Text = "关闭"
        '
        '窗体_播放视频
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(560, 450)
        Me.Controls.Add(Me.文字_关闭)
        Me.Controls.Add(Me.媒体播放器)
        Me.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "窗体_播放视频"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "播放视频"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.媒体播放器, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents 媒体播放器 As AxWMPLib.AxWindowsMediaPlayer
    Friend WithEvents 文字_关闭 As Label
End Class
