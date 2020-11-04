Imports System.Drawing.Drawing2D

Public Class ConfigInstance

    Public Enum HittestResult As Int32
        None = 0
        Empty
        Name
        Delete
        OutputConnector
        Info
        Solo
        Mute
        EditRepeaterValues
        ParameterName = &H100
        ParameterValue = &H200
        ParameterInc = &H300
        ParameterDec = &H400
        InputConnector = &H500
    End Enum

    Public [Function] As IFunction
    Public Rect As Rectangle

    Private tblParameters As DataTable

    Private ltHitboxes As New List(Of Tuple(Of Rectangle, HittestResult))

    'dragdrop der instanz
    Public IsDrag As Boolean
    Public MouseDown As Point

    'Drag einer outputlinie
    Public IsNewOutputAdd As Boolean
    Public NewOutputAddPin As Int32    'welcher output? nur für repeater relevant

    Public IsParameterDrag As Boolean
    Public ParameterIndexAtMousedown As Int32
    Public PrevParameterStep As Int32

    Private Const RECT_WIDTH As Int32 = 170
    Private Const NAME_HEIGHT As Int32 = 16
    Private Const PARAM_HEIGHT As Int32 = 13
    Private Const PARAM_WIDTH As Int32 = 90
    Private Const VALUE_WIDTH As Int32 = 50
    Private Const TEXT_MARGIN As Int32 = 2
    Private Const CONNECTOR_SIZE As Int32 = 8

    Public Function GetOutputLocation(ByVal Index As Int32) As Point
        If Index = -1 Then
            'normaler output
            Return New Point(Rect.Right + CONNECTOR_SIZE \ 2, (Rect.Top + Rect.Bottom) \ 2)
        ElseIf Me.Function.Type = FunctionType.Repeater Then
            'einer der vielen Outputs des repeaters
            Return New Point(Rect.Right + CONNECTOR_SIZE \ 2, Rect.Top + TEXT_MARGIN + NAME_HEIGHT + PARAM_HEIGHT * Index + PARAM_HEIGHT \ 2)
        Else
            Throw New NotImplementedException
        End If
    End Function

    Public Sub UpdateParametersAndRectSize()
        tblParameters = Me.[Function].ParameterInfos
        If Me.Function.Name = "Add" Then
            'bei add 2. parameter verstecken
            Me.Rect.Size = New Size(RECT_WIDTH, TEXT_MARGIN * 2 + NAME_HEIGHT + (tblParameters.Rows.Count - 1) * PARAM_HEIGHT)
        Else
            Me.Rect.Size = New Size(RECT_WIDTH, TEXT_MARGIN * 2 + NAME_HEIGHT + tblParameters.Rows.Count * PARAM_HEIGHT)
        End If
    End Sub

    Private oFontSmall As New Font("Microsoft Sans Serif", 9, GraphicsUnit.Pixel)
    Public Sub Draw(ByVal g As Graphics, ByVal Instances As List(Of ConfigInstance))
        Dim oFont As New Font("Microsoft Sans Serif", 11, GraphicsUnit.Pixel)
        Dim oFontBold As New Font("Microsoft Sans Serif", 11, FontStyle.Bold, GraphicsUnit.Pixel)

        Dim isImageOutput As Boolean = Me.[Function].Type = FunctionType.ImageOutput
        Dim isRepeater As Boolean = Me.[Function].Type = FunctionType.Repeater
        Dim isPlayer As Boolean = Me.[Function].Type = FunctionType.Player
        Dim isNormalFunction As Boolean = Not (isImageOutput OrElse isRepeater OrElse isPlayer)
        Dim isSolo As Boolean = Me.[Function].IsSolo
        Dim isMute As Boolean = Me.[Function].IsMute

        ltHitboxes.Clear()

        'Kastl
        Select Case Me.[Function].Type
            Case FunctionType.Generator
                g.FillRectangle(Brushes.AliceBlue, Me.Rect)
            Case FunctionType.Function
                g.FillRectangle(Brushes.Beige, Me.Rect)
            Case FunctionType.ImageOutput
                g.FillRectangle(Brushes.Plum, Me.Rect)
            Case FunctionType.Repeater
                g.FillRectangle(Brushes.LightPink, Me.Rect)
            Case FunctionType.Player
                g.FillRectangle(Brushes.LightPink, Me.Rect)
        End Select

        g.DrawRectangle(Pens.Black, Me.Rect)
        ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(Me.Rect, HittestResult.Empty))

        Dim rect As Rectangle
        Dim format As StringFormat

        If isNormalFunction Or isPlayer Then
            'ein Output Connector kugerl rechts
            rect = New Rectangle(Me.Rect.Right, CInt((Me.Rect.Top + Me.Rect.Bottom - CONNECTOR_SIZE) / 2), CONNECTOR_SIZE, CONNECTOR_SIZE)
            g.FillEllipse(Brushes.Black, rect)
            ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, HittestResult.OutputConnector))
        End If

        'Name und Schließsymbol
        Dim ptText As New Point(Me.Rect.X + TEXT_MARGIN, Me.Rect.Y + TEXT_MARGIN)
        g.DrawString(Me.[Function].Name, oFontBold, Brushes.Black, ptText)
        If Not isImageOutput Then
            'Schließsymbol
            rect = New Rectangle(New Point(Me.Rect.Right - PARAM_HEIGHT - TEXT_MARGIN, ptText.Y + 1),
                                        New Size(PARAM_HEIGHT - 2, PARAM_HEIGHT - 2))
            ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, HittestResult.Delete))
            DrawCloseSymbol(g)

            'info box
            rect.Offset(-PARAM_HEIGHT + 2, 0)
            ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.Info, HittestResult)))
            DrawInfoSymbol(g)

            If isNormalFunction Then
                'Mute box
                rect.Offset(-PARAM_HEIGHT + 2, 0)
                ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.Mute, HittestResult)))
                DrawMuteSymbol(g)

                'solo box
                rect.Offset(-PARAM_HEIGHT + 2, 0)
                ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.Solo, HittestResult)))
                DrawSoloSymbol(g)
            End If
        End If
        ptText.Offset(0, NAME_HEIGHT)

        'Parameter
        UpdateParametersAndRectSize()
        For iParameter As Int32 = 0 To tblParameters.Rows.Count - 1
            If Me.Function.Name = "Add" AndAlso iParameter = tblParameters.Rows.Count - 1 Then
                Exit For 'wenn add, dann zweiten parameter nicht anführen
            End If

            Dim row As DataRow = tblParameters.Rows(iParameter)

            If isNormalFunction OrElse isImageOutput Then
                'Kugerl links
                rect = New Rectangle(ptText.X - TEXT_MARGIN - CONNECTOR_SIZE, CInt(ptText.Y + PARAM_HEIGHT / 2 - CONNECTOR_SIZE / 2), CONNECTOR_SIZE, CONNECTOR_SIZE)
                g.FillEllipse(Brushes.Black, rect)
                ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.InputConnector + iParameter, HittestResult)))
            ElseIf isRepeater Then
                'Kugerl rechts
                rect = New Rectangle(Me.Rect.Right, CInt(ptText.Y + PARAM_HEIGHT / 2 - CONNECTOR_SIZE / 2), CONNECTOR_SIZE, CONNECTOR_SIZE)
                g.FillEllipse(Brushes.Black, rect)
                ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.InputConnector + iParameter, HittestResult)))
            End If

            'name
            If isNormalFunction OrElse isImageOutput OrElse isPlayer Then
                rect = New Rectangle(ptText, New Size(PARAM_WIDTH, PARAM_HEIGHT))
                format = New StringFormat With {.Alignment = StringAlignment.Near}
            Else
                rect = New Rectangle(ptText, New Size(Me.Rect.Width - TEXT_MARGIN, PARAM_HEIGHT))
                format = New StringFormat With {.Alignment = StringAlignment.Far}
            End If
            g.DrawString(row("Name").ToString, oFont, Brushes.Black, rect, format)
            ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.ParameterName + iParameter, HittestResult)))

            If isNormalFunction Or isPlayer Then
                Dim iTemp As Int32 = iParameter
                Dim isConnected As Boolean =
                    Instances.Any(Function(o) Not o.Function.IsMute AndAlso
                    o.Function.Outputs.Any(Function(o2)
                                               Return o2.ToFunction.Equals(Me.[Function]) AndAlso
                                                                            o2.ToPin = iTemp
                                           End Function))

                'wert
                If Not isConnected OrElse Me.Function.Name = "Add" Then
                    rect = New Rectangle(New Point(ptText.X + PARAM_WIDTH, ptText.Y), New Size(VALUE_WIDTH, PARAM_HEIGHT))
                    format = New StringFormat With {.Alignment = StringAlignment.Far}
                    g.DrawString(CDbl(row("Current")).ToString("N3"), oFont, Brushes.Black, rect, format)
                    ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.ParameterValue + iParameter, HittestResult)))
                End If

                '+/-
                Dim pen As Pen = If(isConnected, Pens.LightGray, Pens.Black)
                rect = New Rectangle(New Point(ptText.X + PARAM_WIDTH + VALUE_WIDTH, ptText.Y + 1), New Size(PARAM_HEIGHT - 2, PARAM_HEIGHT - 2))
                g.DrawRectangle(pen, rect)
                g.DrawLine(pen, rect.X + 2, CInt(rect.Y + rect.Height / 2), rect.X + rect.Width - 2, CInt(rect.Y + rect.Height / 2))
                If Not isConnected Then
                    ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.ParameterDec + iParameter, HittestResult)))
                End If
                rect.Offset(rect.Width, 0)
                g.DrawRectangle(pen, rect)
                g.DrawLine(pen, rect.X + 2, CInt(rect.Y + rect.Height / 2), rect.X + rect.Width - 2, CInt(rect.Y + rect.Height / 2))
                g.DrawLine(pen, CInt(rect.X + rect.Width / 2), rect.Y + 2, CInt(rect.X + rect.Width / 2), rect.Y + rect.Height - 2)
                If Not isConnected Then
                    ltHitboxes.Add(New Tuple(Of Rectangle, HittestResult)(rect, CType(HittestResult.ParameterInc + iParameter, HittestResult)))
                End If
            End If

            ptText.Offset(0, PARAM_HEIGHT)
        Next
    End Sub

    Private Sub DrawCloseSymbol(g As Graphics, Optional Highlight As Boolean = False)
        Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = HittestResult.Delete).Item1
        Dim pen As New Pen(If(Highlight, Color.Red, Color.Black))
        g.DrawRectangle(pen, rect)
        g.DrawLine(pen, rect.Left + 2, rect.Top + 2, rect.Right - 2, rect.Bottom - 2)
        g.DrawLine(pen, rect.Left + 2, rect.Bottom - 2, rect.Right - 2, rect.Top + 2)
    End Sub
    Private Sub DrawInfoSymbol(g As Graphics, Optional Highlight As Boolean = False)
        Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = HittestResult.Info).Item1
        DrawBox(g, rect, oFontSmall, "i", If(Highlight, Color.Red, Color.Black))
    End Sub
    Private Sub DrawMuteSymbol(g As Graphics, Optional Highlight As Boolean = False)
        Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = HittestResult.Mute).Item1
        DrawBox(g, rect, oFontSmall, "M", If(Highlight, Color.Red, Color.Black), If(Me.Function.IsMute, Color.HotPink, Nothing))
    End Sub
    Private Sub DrawSoloSymbol(g As Graphics, Optional Highlight As Boolean = False)
        Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = HittestResult.Solo).Item1
        DrawBox(g, rect, oFontSmall, "S", If(Highlight, Color.Red, Color.Black), If(Me.Function.IsSolo, Color.HotPink, Nothing))
    End Sub


    Public Sub DrawHightlight(g As Graphics, MouseLocation As Point, Optional IsDragTo As Boolean = False)
        'wenn über einem input connector, dann diesen rot malen
        Dim iHittestResult As ConfigInstance.HittestResult = Me.Hittest(MouseLocation)
        If (iHittestResult And &HFF00) = ConfigInstance.HittestResult.InputConnector Then
            'über einem Input Connector
            Dim iParameter As Int32 = iHittestResult And &HFF
        End If

        Select Case iHittestResult
            Case ConfigInstance.HittestResult.Delete
                DrawCloseSymbol(g, True)

            Case ConfigInstance.HittestResult.Info
                DrawInfoSymbol(g, True)

            Case ConfigInstance.HittestResult.Solo
                DrawSoloSymbol(g, True)

            Case ConfigInstance.HittestResult.Mute
                DrawMuteSymbol(g, True)

            Case ConfigInstance.HittestResult.OutputConnector
                If Not IsDragTo Then
                    'hoover: neue verbindung? -> connector rot zeichnen
                    Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = HittestResult.OutputConnector).Item1
                    g.FillEllipse(Brushes.Red, rect)
                End If

            Case Is > ConfigInstance.HittestResult.OutputConnector
                Dim iParameter As Int32 = iHittestResult And &HFF
                Select Case iHittestResult And &HFF00
                        'Parameter zeugs
                    Case ConfigInstance.HittestResult.ParameterInc, ConfigInstance.HittestResult.ParameterDec

                    Case ConfigInstance.HittestResult.InputConnector
                        If IsDragTo Then
                            'hoover: verbindung hier hin? -> connector rot zeichnen
                            Dim rect As Rectangle = ltHitboxes.First(Function(o) o.Item2 = iHittestResult).Item1
                            g.FillEllipse(Brushes.Red, rect)
                        Else
                            'hoover über input connector -> ist da was verbunden?
                            Dim oInstanceConnectedFrom As ConfigInstance
                            oInstanceConnectedFrom = ConfigInstances.Where(
                                    Function(o) o.[Function].Outputs.Any(
                                        Function(o2) o2.ToFunction.Equals(Me.[Function]) _
                                                     AndAlso o2.ToPin = iParameter
                                    )).OrderBy(Function(o) o.ConnectorHitscore(MouseLocation, Me)).FirstOrDefault
                            If oInstanceConnectedFrom IsNot Nothing Then
                                'connector linie highlighten
                                Dim oConnector As OutputTo = oInstanceConnectedFrom.Function.Outputs.First(Function(o) o.ToFunction.Equals(Me.Function) AndAlso o.ToPin = iParameter)
                                Dim ptFrom As Point = oInstanceConnectedFrom.GetOutputLocation(oConnector.FromPin)
                                Dim iToPin As Int32 = oConnector.ToPin
                                If Me.Function.Name = "Add" Then
                                    iToPin = 0  'add immer zum ersten pin zeichnen
                                End If
                                Dim ptTo As New Point(CInt(Me.Rect.Left - CONNECTOR_SIZE / 2),
                                  CInt(Me.Rect.Top + TEXT_MARGIN + NAME_HEIGHT + iToPin * PARAM_HEIGHT + PARAM_HEIGHT / 2))

                                Using aGradientBrush As Brush = New LinearGradientBrush(ptFrom,ptTo, Color.Black, Color.Red)
                                    Using aGradientPen As Pen = New Pen(aGradientBrush)
                                        g.DrawLine(aGradientPen, ptFrom, ptTo)
                                    End Using
                                End Using

                            End If
                        End If
                End Select
            Case Else
        End Select

    End Sub


    Private Sub DrawBox(ByVal g As Graphics, ByVal rect As Rectangle, ByVal Font As Font, ByVal Letter As String,
                        Optional ForeColor As Color = Nothing, Optional ByVal FillColor As Color = Nothing)
        If FillColor <> Color.Empty Then
            g.FillRectangle(New SolidBrush(FillColor), rect)
        End If
        If ForeColor = Color.Empty Then
            ForeColor = Color.Black
        End If
        Dim format As New StringFormat With {.Alignment = StringAlignment.Center}
        g.DrawRectangle(New Pen(ForeColor), rect)
        g.DrawString(Letter, Font, New SolidBrush(ForeColor), rect, format)
    End Sub

    Public Sub DrawConnections(ByVal g As Graphics, ByVal Instances As List(Of ConfigInstance), Optional DrawDrag As Boolean = False)
        Dim ptFromDefault As Point = Me.GetOutputLocation(-1)
        Dim ptFrom As Point

        For Each oOut As OutputTo In Me.[Function].Outputs
            If oOut.FromPin = -1 Then
                ptFrom = ptFromDefault
            Else
                ptFrom = Me.GetOutputLocation(oOut.FromPin)
            End If
            Dim oTemp As IFunction = oOut.ToFunction
            Dim oFuncTo As ConfigInstance = Instances.FirstOrDefault(Function(o)
                                                                         Return o.[Function].Equals(oTemp)
                                                                     End Function)
            If DrawDrag OrElse Not oFuncTo.IsDrag Then
                Dim iToPin As Int32 = oOut.ToPin
                If oFuncTo.Function.Name = "Add" Then
                    iToPin = 0  'add immer zum ersten pin zeichnen
                End If
                Dim ptTo As New Point(CInt(oFuncTo.Rect.Left - CONNECTOR_SIZE / 2),
                                  CInt(oFuncTo.Rect.Top + TEXT_MARGIN + NAME_HEIGHT + iToPin * PARAM_HEIGHT + PARAM_HEIGHT / 2))

                g.DrawLine(If(Me.[Function].IsMute, Pens.LightGray, Pens.Black), ptFrom, ptTo)
            End If
        Next
    End Sub


    Public Function IsHit(ByVal pt As Point) As Boolean
        Return New Rectangle(Me.Rect.Left - CONNECTOR_SIZE, Me.Rect.Top, Me.Rect.Width + CONNECTOR_SIZE * 2, Me.Rect.Height).Contains(pt)
    End Function

    Public Function Hittest(ByVal pt As Point) As HittestResult
        Dim oResult As Tuple(Of Rectangle, HittestResult) = ltHitboxes.LastOrDefault(Function(o) o.Item1.Contains(pt))
        If oResult IsNot Nothing Then
            Return oResult.Item2
        Else
            Return HittestResult.None
        End If
    End Function

    Public Function ConnectorHitscore(ByVal pt As Point, ByVal FuncConnectedTo As ConfigInstance) As Double
        Dim ptFrom As New Point(CInt(Me.Rect.Right + CONNECTOR_SIZE / 2), CInt((Me.Rect.Top + Me.Rect.Bottom) / 2))
        Dim fMin As Double = Double.MaxValue
        For Each oOut As OutputTo In Me.[Function].Outputs

            Dim ptTo As New Point(CInt(FuncConnectedTo.Rect.Left - CONNECTOR_SIZE / 2),
                                  CInt(FuncConnectedTo.Rect.Top + TEXT_MARGIN + NAME_HEIGHT + oOut.ToPin * PARAM_HEIGHT + PARAM_HEIGHT / 2))

            Dim yDeltaCurrent As Double = pt.Y - ptFrom.Y
            Dim xDeltaCurrent As Double = pt.X - ptFrom.X

            Dim fAlphaConnector As Double = Math.Atan2(ptTo.Y - ptFrom.Y, ptTo.X - ptFrom.X)
            Dim fDistConnector As Double = Math.Sqrt((ptTo.X - ptFrom.X) * (ptTo.X - ptFrom.X) + (ptTo.Y - ptFrom.Y) * (ptTo.Y - ptFrom.Y))

            Dim fAlpha As Double = fAlphaConnector - Math.Atan2(yDeltaCurrent, xDeltaCurrent)
            Dim fDistAdjacent As Double = Math.Sqrt(xDeltaCurrent * xDeltaCurrent + yDeltaCurrent * yDeltaCurrent)
            Dim fAbscissa As Double = Math.Sin(fAlpha) * fDistAdjacent
            Dim fOrdinate As Double = Math.Cos(fAlpha) * fDistAdjacent

            Dim isOnLine As Boolean = Math.Abs(fAlpha) < Math.PI / 2 AndAlso fOrdinate < fDistConnector

            If Math.Abs(fAbscissa) < fMin Then
                fMin = Math.Abs(fAbscissa)
            End If
        Next
        Return fMin
    End Function



    Public Sub Serialize(ByVal Parent As Xml.XmlNode)
        Dim nd As Xml.XmlElement = Parent.OwnerDocument.CreateElement("ConfigInstance")
        Parent.AppendChild(nd)
        nd.SetAttribute("LocationX", Rect.X.ToString)
        nd.SetAttribute("LocationY", Rect.Y.ToString)

        [Function].Serialize(nd)
    End Sub

    Public Sub New()

    End Sub
    Public Sub New(ByVal Node As Xml.XmlElement)
        Rect.X = CInt(Node.Attributes("LocationX").Value)
        Rect.Y = CInt(Node.Attributes("LocationY").Value)
        Dim oTemp As New Sinus
        [Function] = oTemp.Deserialize(Node.SelectSingleNode("FunctionInstance"))
    End Sub


    Public Function Clone() As ConfigInstance
        Dim xml As New Xml.XmlDocument
        Dim nd As Xml.XmlElement = xml.CreateElement("dummy")
        Me.Serialize(nd)

        Dim oResult As New ConfigInstance(CType(nd.SelectSingleNode("ConfigInstance"), Xml.XmlElement))
        oResult.Function.Outputs.Clear()
        oResult.Function.Guid = Guid.NewGuid
        Return oResult
    End Function
End Class