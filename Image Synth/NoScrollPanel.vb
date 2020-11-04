
Public Class NoScrollPanel
    Inherits Panel

    Protected Overrides Sub OnMouseWheel(ByVal e As MouseEventArgs)
        Dim mouseEvent As HandledMouseEventArgs = DirectCast(e, HandledMouseEventArgs)
        mouseEvent.Handled = True
    End Sub
End Class
