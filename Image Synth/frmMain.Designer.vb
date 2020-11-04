<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.picOutput = New System.Windows.Forms.PictureBox()
        Me.btnAddFunction = New System.Windows.Forms.Button()
        Me.btnHelp = New System.Windows.Forms.Button()
        Me.btnLoad = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.tmr = New System.Windows.Forms.Timer(Me.components)
        Me.picProgress = New System.Windows.Forms.PictureBox()
        Me.btnSize = New System.Windows.Forms.Button()
        Me.pnlOutput = New System.Windows.Forms.Panel()
        Me.btnCompile = New System.Windows.Forms.Button()
        Me.btnClearAll = New System.Windows.Forms.Button()
        Me.pnlConfig = New ImageSynthesis.NoScrollPanel()
        Me.picConfig = New System.Windows.Forms.PictureBox()
        Me.btnDebug = New System.Windows.Forms.Button()
        CType(Me.picOutput, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picProgress, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlOutput.SuspendLayout()
        Me.pnlConfig.SuspendLayout()
        CType(Me.picConfig, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picOutput
        '
        Me.picOutput.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.picOutput.Location = New System.Drawing.Point(3, 3)
        Me.picOutput.Name = "picOutput"
        Me.picOutput.Size = New System.Drawing.Size(272, 272)
        Me.picOutput.TabIndex = 0
        Me.picOutput.TabStop = False
        '
        'btnAddFunction
        '
        Me.btnAddFunction.Location = New System.Drawing.Point(12, 290)
        Me.btnAddFunction.Name = "btnAddFunction"
        Me.btnAddFunction.Size = New System.Drawing.Size(97, 41)
        Me.btnAddFunction.TabIndex = 2
        Me.btnAddFunction.Text = "Funktion hinzufügen"
        Me.btnAddFunction.UseVisualStyleBackColor = True
        '
        'btnHelp
        '
        Me.btnHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnHelp.Location = New System.Drawing.Point(162, 590)
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.Size = New System.Drawing.Size(69, 26)
        Me.btnHelp.TabIndex = 5
        Me.btnHelp.Text = "Hilfe"
        Me.btnHelp.UseVisualStyleBackColor = True
        '
        'btnLoad
        '
        Me.btnLoad.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnLoad.Location = New System.Drawing.Point(12, 558)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(69, 26)
        Me.btnLoad.TabIndex = 6
        Me.btnLoad.Text = "Laden"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        Me.btnSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnSave.Location = New System.Drawing.Point(12, 590)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(69, 26)
        Me.btnSave.TabIndex = 7
        Me.btnSave.Text = "Speichern"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 345)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(56, 13)
        Me.lblProgress.TabIndex = 9
        Me.lblProgress.Text = "Berechne:"
        '
        'tmr
        '
        Me.tmr.Enabled = True
        '
        'picProgress
        '
        Me.picProgress.Location = New System.Drawing.Point(12, 361)
        Me.picProgress.Name = "picProgress"
        Me.picProgress.Size = New System.Drawing.Size(272, 16)
        Me.picProgress.TabIndex = 10
        Me.picProgress.TabStop = False
        '
        'btnSize
        '
        Me.btnSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSize.Location = New System.Drawing.Point(250, 278)
        Me.btnSize.Name = "btnSize"
        Me.btnSize.Size = New System.Drawing.Size(26, 24)
        Me.btnSize.TabIndex = 13
        Me.btnSize.Text = ">"
        Me.btnSize.UseVisualStyleBackColor = True
        '
        'pnlOutput
        '
        Me.pnlOutput.Controls.Add(Me.picOutput)
        Me.pnlOutput.Controls.Add(Me.btnSize)
        Me.pnlOutput.Location = New System.Drawing.Point(12, 12)
        Me.pnlOutput.Name = "pnlOutput"
        Me.pnlOutput.Size = New System.Drawing.Size(279, 306)
        Me.pnlOutput.TabIndex = 14
        '
        'btnCompile
        '
        Me.btnCompile.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnCompile.Location = New System.Drawing.Point(87, 558)
        Me.btnCompile.Name = "btnCompile"
        Me.btnCompile.Size = New System.Drawing.Size(69, 26)
        Me.btnCompile.TabIndex = 16
        Me.btnCompile.Text = "Run OGL"
        Me.btnCompile.UseVisualStyleBackColor = True
        '
        'btnClearAll
        '
        Me.btnClearAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnClearAll.Location = New System.Drawing.Point(87, 590)
        Me.btnClearAll.Name = "btnClearAll"
        Me.btnClearAll.Size = New System.Drawing.Size(69, 26)
        Me.btnClearAll.TabIndex = 17
        Me.btnClearAll.Text = "Löschen"
        Me.btnClearAll.UseVisualStyleBackColor = True
        '
        'pnlConfig
        '
        Me.pnlConfig.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlConfig.AutoScroll = True
        Me.pnlConfig.Controls.Add(Me.picConfig)
        Me.pnlConfig.Location = New System.Drawing.Point(297, 12)
        Me.pnlConfig.Name = "pnlConfig"
        Me.pnlConfig.Size = New System.Drawing.Size(743, 604)
        Me.pnlConfig.TabIndex = 15
        '
        'picConfig
        '
        Me.picConfig.Location = New System.Drawing.Point(0, 0)
        Me.picConfig.Name = "picConfig"
        Me.picConfig.Size = New System.Drawing.Size(201, 272)
        Me.picConfig.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picConfig.TabIndex = 4
        Me.picConfig.TabStop = False
        '
        'btnDebug
        '
        Me.btnDebug.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnDebug.Location = New System.Drawing.Point(162, 558)
        Me.btnDebug.Name = "btnDebug"
        Me.btnDebug.Size = New System.Drawing.Size(69, 26)
        Me.btnDebug.TabIndex = 18
        Me.btnDebug.Text = "Debug"
        Me.btnDebug.UseVisualStyleBackColor = True
        Me.btnDebug.Visible = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1052, 628)
        Me.Controls.Add(Me.btnDebug)
        Me.Controls.Add(Me.btnClearAll)
        Me.Controls.Add(Me.btnCompile)
        Me.Controls.Add(Me.pnlConfig)
        Me.Controls.Add(Me.btnAddFunction)
        Me.Controls.Add(Me.pnlOutput)
        Me.Controls.Add(Me.picProgress)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.btnHelp)
        Me.KeyPreview = True
        Me.Name = "frmMain"
        Me.Text = "Image Synth"
        CType(Me.picOutput, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picProgress, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlOutput.ResumeLayout(False)
        Me.pnlConfig.ResumeLayout(False)
        Me.pnlConfig.PerformLayout()
        CType(Me.picConfig, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents picOutput As System.Windows.Forms.PictureBox
    Friend WithEvents btnAddFunction As System.Windows.Forms.Button
    Friend WithEvents picConfig As System.Windows.Forms.PictureBox
    Friend WithEvents btnHelp As System.Windows.Forms.Button
    Friend WithEvents btnLoad As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents lblProgress As Label
    Friend WithEvents tmr As Timer
    Friend WithEvents picProgress As PictureBox
    Friend WithEvents btnSize As Button
    Friend WithEvents pnlOutput As Panel
    Friend WithEvents pnlConfig As NoScrollPanel
    Friend WithEvents btnCompile As Button
    Friend WithEvents btnClearAll As Button
    Friend WithEvents btnDebug As Button
End Class
