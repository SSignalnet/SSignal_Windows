<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class 窗体_浏览器
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.浏览器的容器 = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        '浏览器的容器
        '
        Me.浏览器的容器.Dock = System.Windows.Forms.DockStyle.Fill
        Me.浏览器的容器.Location = New System.Drawing.Point(0, 0)
        Me.浏览器的容器.Name = "浏览器的容器"
        Me.浏览器的容器.Size = New System.Drawing.Size(1678, 962)
        Me.浏览器的容器.TabIndex = 0
        '
        '窗体_浏览器
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(1678, 962)
        Me.Controls.Add(Me.浏览器的容器)
        Me.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Name = "窗体_浏览器"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents 浏览器的容器 As Panel
End Class
