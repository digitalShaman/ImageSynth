Imports System.Drawing.Drawing2D

Module ConfigManager
    Public ConfigInstances As New List(Of ConfigInstance)
    Public Client As Panel

    Private ConfigBitmap As Bitmap


    Public Sub AddInstance(ByVal NewFunction As ConfigInstance)
        If NewFunction.Rect.IsEmpty Then
            NewFunction.Rect.Location = New Point(10, 10)
            NewFunction.UpdateParametersAndRectSize()
            Dim pt As New Point(10, 10)
            Do While ConfigInstances.Any(Function(o) o.Rect.Location = pt)
                pt.Offset(10, 10)
            Loop
            NewFunction.Rect.Location = pt
        End If
        NewFunction.UpdateParametersAndRectSize()

        ConfigInstances.Add(NewFunction)
    End Sub


    Public Sub DrawConfig()
        Static isDragPainted As Boolean = False

        EnsureBitmap()

        If ConfigInstances.Any(Function(o) o.IsDrag) Then
            If isDragPainted Then
                frmMain.picConfig.Invalidate()
                Exit Sub
            Else
                isDragPainted = True
            End If
        Else
            isDragPainted = False
        End If

        Using g As Graphics = Graphics.FromImage(ConfigBitmap)

            g.Clear(Color.White)
            For Each oFunc As ConfigInstance In ConfigInstances.Where(Function(o) Not o.IsDrag)
                oFunc.Draw(g, ConfigInstances)
            Next

            For Each oFunc As ConfigInstance In ConfigInstances.Where(Function(o) Not o.IsDrag)
                oFunc.DrawConnections(g, ConfigInstances)
            Next

        End Using

        Dim picBox As PictureBox = CType(Client.Controls(0), PictureBox)
        picBox.Image = ConfigBitmap
        frmMain.picConfig.Invalidate()
    End Sub

    Public Sub PaintConfig(ByVal g As Graphics, ByVal MouseLocation As Point)
        g.DrawImageUnscaledAndClipped(ConfigBitmap, New Rectangle(0, 0, ConfigBitmap.Width, ConfigBitmap.Height))


        Dim oInstanceUnderMouse As ConfigInstance = ConfigInstances.LastOrDefault(Function(o) o.IsHit(MouseLocation))
        Dim oInstanceOutputAdded As ConfigInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsNewOutputAdd)
        Dim oInstanceDragging As ConfigInstance = ConfigInstances.FirstOrDefault(Function(o) o.IsDrag)

        Static oOldHightlight As ConfigInstance
        If oInstanceUnderMouse IsNot Nothing Then
            oInstanceUnderMouse.DrawHightlight(g, MouseLocation, oInstanceOutputAdded IsNot Nothing)
            oOldHightlight = oInstanceUnderMouse
        ElseIf oOldHightlight IsNot Nothing Then
            'altes highlight wieder wegnehmen
            If ConfigInstances.Contains(oOldHightlight) Then
                'aber nur, wenn das modul nicht gelöscht wurde
                oOldHightlight.DrawHightlight(g, MouseLocation, oInstanceOutputAdded IsNot Nothing)
            End If
            oOldHightlight = Nothing
        End If

        If oInstanceOutputAdded IsNot Nothing Then
            'neuer output connector wird gerade hinzugefügt -> Linie drübermalen
            Dim ptFrom As Point = oInstanceOutputAdded.GetOutputLocation(oInstanceOutputAdded.NewOutputAddPin)
            Using aGradientBrush As Brush = New LinearGradientBrush(ptFrom, MouseLocation, Color.Black, Color.Red)
                Using aGradientPen As Pen = New Pen(aGradientBrush)
                    g.DrawLine(aGradientPen, ptFrom, MouseLocation)
                End Using
            End Using
        End If

        If oInstanceDragging IsNot Nothing Then
            'eine instanz wird gerade verschoben
            oInstanceDragging.Draw(g, ConfigInstances)
            oInstanceDragging.DrawConnections(g, ConfigInstances)

            Dim ltReceivingFrom As List(Of ConfigInstance) = ConfigInstances.Where(Function(o) o.Function.Outputs.Any(
                                                                           Function(o2) o2.ToFunction.Equals(oInstanceDragging.Function))).ToList
            ltReceivingFrom.ForEach(Sub(o) o.DrawConnections(g, ConfigInstances, True))
        End If

    End Sub

    Private Sub EnsureBitmap()
        If ConfigInstances.Any(Function(o) o.IsDrag) Then Exit Sub 'bei einem drag nicht das image vergrößern/verkleinern!
        Dim xMax, yMax As Int32
        xMax = ConfigInstances.Select(Function(o) o.Rect.Right).Max + 20
        yMax = ConfigInstances.Select(Function(o) o.Rect.Bottom).Max + 20

        If Client.Width > xMax Then xMax = Client.Width - 5
        If Client.Height > yMax Then yMax = Client.Height - 5

        If ConfigBitmap Is Nothing OrElse ConfigBitmap.Width <> xMax OrElse ConfigBitmap.Height <> yMax Then
            ConfigBitmap = New Bitmap(xMax, yMax)
        End If
    End Sub
End Module
