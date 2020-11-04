Public Class frmChooseFunction

    Public SelectedFunction As IFunction

    Private Sub frmChooseFunction_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        SelectedFunction = Nothing

        Dim ltAvailable() As IFunction = {
            New Sinus, New Voronoi, New SimplexNoise, New RandomNoise, New Scratches, New Concentric, New Polygon, New Mandelbrot, New Julia,
            New Add, New Multiply, New Filter, New Clamp, New Compressor, New Crossfade,
            New Repeater, New LinearPlayer}

        Dim pt As New Point(5, 15)
        For Each Func As IFunction In ltAvailable
            Select Case Func.Type
                Case FunctionType.Generator
                    AddButton(Func, Me.SplitContainer1.Panel1)
                Case FunctionType.Function
                    AddButton(Func, Me.SplitContainer2.Panel1)
                Case FunctionType.Repeater, FunctionType.Player
                    AddButton(Func, Me.SplitContainer2.Panel2)
            End Select
        Next
    End Sub

    Private Sub AddButton(ByVal Func As IFunction, ByVal Parent As Panel)
        Dim ptLocation As New Point(5, 15)
        If Parent.Tag IsNot Nothing Then
            ptLocation = CType(Parent.Tag, Point)
        End If

        Dim btn As New Button
        btn.Location = ptLocation
        btn.Size = New Size(Parent.Width - 10, 30)
        btn.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btn.Text = Func.Name
        btn.Tag = Func
        Select Case Func.Type
            Case FunctionType.Generator
                btn.BackColor = Color.AliceBlue
            Case FunctionType.Function
                btn.BackColor = Color.Beige
            Case FunctionType.Player
                btn.BackColor = Color.LightPink
            Case FunctionType.Repeater
                btn.BackColor = Color.LightPink
        End Select
        AddHandler btn.Click, AddressOf Button_Click

        Parent.Controls.Add(btn)
        ptLocation.Offset(0, 30)
        Parent.Tag = ptLocation
    End Sub

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim btn As Button = CType(sender, Button)
        SelectedFunction = CType(btn.Tag, IFunction)
        Me.Hide()
    End Sub
End Class